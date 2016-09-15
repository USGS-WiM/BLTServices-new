//------------------------------------------------------------------------------
//----- BLTServiceCaller.cs ----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Helper to call services with REST sharp
//
//   discussion:   
//
//     

#region Comments
// 04.22.13 - TR - Created
#endregion

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using BLTServices;
using BLTServices.Authentication;

namespace BLTWeb.Utilities
{
    public class BLTServiceCaller
    {
        private static readonly BLTServiceCaller _instance = new BLTServiceCaller();
        public static BLTServiceCaller Instance {
            get {
                return _instance;
            }
        }

        private RestClient client = new RestClient();
        public USER_ CurrentUser;
        public ROLE CurrentRole;

        //Singleton
        private BLTServiceCaller()
        {                         
            client.BaseUrl = ConfigurationManager.AppSettings["BLTServicesBase"];
        }



        //Sets the base url for the service
        public void setBaseUrl(string baseUrl)
        {
            client.BaseUrl = baseUrl;
        }


        //Sets Username and Password to send with request
        public bool setAuthentication(String username, EasySecureString password)
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password.decryptString());

            //Check login
            RestRequest request = new RestRequest();
            request.Resource = ConfigurationManager.AppSettings["BLTServicesLoginEndpoint"];
            request.RootElement = "boolean";
            
            CurrentUser = _instance.Execute<USER_>(request);

            if (CurrentUser != null)
            {

                //get the role
                if (CurrentUser.ROLE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "entities/ROLES/{roleId}";
                    request.RootElement = "ROLE";
                    request.AddParameter("roleId", CurrentUser.ROLE_ID, ParameterType.UrlSegment);
                    CurrentRole = _instance.Execute<ROLE>(request);
                }

            }
            else
            {
                clearAuthentication();
            }

            return (CurrentUser != null);

        }


        public void clearAuthentication() {
            client.Authenticator = new HttpBasicAuthenticator("", "");
            FormsAuthentication.SignOut();
        }



        //Generic Execute, allows request to be deserialized in any type
        public T Execute<T>(RestRequest request) where T : new()
        {
            RestResponse response = client.Execute(request) as RestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (response.ContentLength > 0)
                {
                    if (response.ContentType == "application/xml")
                    {

                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        using (TextReader reader = new StringReader(response.Content))
                        {
                            return (T)serializer.Deserialize(reader);
                        }
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<T>(response.Content);
                    }
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                clearAuthentication();
            }
            return default(T);
        }

       

        //Generic Execute, allows request to be deserialized in any type 
        public T ExecuteExtraTypes<T>(RestRequest request, Type[] extraTypes) where T : new()
        {
            RestResponse response = client.Execute(request) as RestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T), extraTypes);
                using (TextReader reader = new StringReader(response.Content))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                clearAuthentication();
            }
            return default(T);
        }



    }
}