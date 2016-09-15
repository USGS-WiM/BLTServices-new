//------------------------------------------------------------------------------
//----- MembersController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Members page and link to individual members pages 
//
//discussion:   
//
//     

#region Comments 
//06.12.12 - TR - Created

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

using RestSharp;
using BLTServices;
using BLTServices.Authentication;
using BLTServices.Resources;
using BLTWeb.Utilities;
using BLTWeb.Models;
using BLTWeb.Helpers;

namespace BLTWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class UsersController : Controller
    {
        //
        // GET: /Settings/

        public ActionResult Index()
        {
            //get logged in user's role
            ViewData["Role"] = GetUserRole();

            //get all the users
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Users";
            request.RootElement = "ArrayOfUSERS_";
            List<USER_> UserList = serviceCaller.Execute<List<USER_>>(request);
            List<USER_> SortedUsers = UserList.OrderBy(x => x.LNAME).ToList();

            //request organizations
            request = new RestRequest();
            request.Resource = "/Organizations";
            request.RootElement = "ArrayOfORGANIZATION";
            List<ORGANIZATION> orgList = serviceCaller.Execute<List<ORGANIZATION>>(request);

            //request roles
            request = new RestRequest();
            request.Resource = "/Roles";
            request.RootElement = "ArrayOfRole";
            List<ROLE> roleList = serviceCaller.Execute<List<ROLE>>(request);

            //loop through to populate model
            List<UsersModel> allUsersModel = new List<UsersModel>();
            foreach (USER_ user in SortedUsers)
            {
                UsersModel thisUser = new UsersModel();
                thisUser.UserID = user.USER_ID;
                thisUser.UserName = user.FNAME + " " + user.LNAME;

                //loop thru organziations to get matching
                if (user.ORGANIZATION_ID != null && user.ORGANIZATION_ID > 0)
                { thisUser.Organization = orgList.SingleOrDefault(o => o.ORGANIZATION_ID == user.ORGANIZATION_ID).NAME; }

                //loop thru roles to get matching
                if (user.ROLE_ID > 0)
                { thisUser.Role = roleList.SingleOrDefault(r => r.ROLE_ID == user.ROLE_ID).ROLE_NAME; }

                allUsersModel.Add(thisUser);
            }

            return View(allUsersModel);
        }

        //user details page
        public ActionResult UserDetails(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Users/{userID}";
            request.RootElement = "USER_";
            request.AddParameter("userID", id, ParameterType.UrlSegment);
            List<USER_> thisUser = serviceCaller.Execute<List<USER_>>(request);
            USER_ aUser = thisUser.FirstOrDefault();

            //get logged in role
            ViewData["Role"] = GetUserRole();

            return View(aUser);
        }

        //partial details view for details page
        public PartialViewResult UserDetailsPV(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Users/{userID}";
            request.RootElement = "ArrayOfUSER_";
            request.AddParameter("userID", id, ParameterType.UrlSegment);
            List<USER_> thisUser = serviceCaller.Execute<List<USER_>>(request);
            USER_ aUser = thisUser.FirstOrDefault();

            //get organization, division and role 
            if (aUser.ORGANIZATION_ID != null && aUser.ORGANIZATION_ID != 0)
            {
                request = new RestRequest();
                request.Resource = "/Organizations/{organizationID}";
                request.RootElement = "ORGANIZATION";
                request.AddParameter("organizationID", aUser.ORGANIZATION_ID, ParameterType.UrlSegment);
                ORGANIZATION anOrg = serviceCaller.Execute<ORGANIZATION>(request);            
                ViewData["Organization"] = anOrg.NAME;
            }
            if (aUser.DIVISION_ID != null && aUser.DIVISION_ID != 0)
            {
                request = new RestRequest();
                request.Resource = "/Divisions/{divisionID}";
                request.RootElement = "DIVISION";
                request.AddParameter("divisionID", aUser.DIVISION_ID, ParameterType.UrlSegment);
                ViewData["Division"] = serviceCaller.Execute<DIVISION>(request).DIVISION_NAME;
            }
            if (aUser != null && aUser.ROLE_ID != 0)
            {
                request = new RestRequest();
                request.Resource = "/Roles/{roleID}";
                request.RootElement = "ROLE";
                request.AddParameter("roleID", aUser.ROLE_ID, ParameterType.UrlSegment);
                ViewData["Role"] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
            }
            return PartialView(aUser);
        }

        //edit partial view
        public PartialViewResult UserEditPV(int id)
        {
            //get the logged in member for authorization
            ViewData["Role"] = GetUserRole();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();

            request.Resource = "Users/{userID}";
            request.RootElement = "ArrayOfUSER_";
            request.AddParameter("userID", id, ParameterType.UrlSegment);
            List<USER_> thisUser = serviceCaller.Execute<List<USER_>>(request);
            USER_ aUser = thisUser.FirstOrDefault();

            //get lists of organizations, divisions and roles
            request = new RestRequest();
            request.Resource = "/Organizations";
            request.RootElement = "ArrayOfORGANIZATION";
            ViewData["AllOrgs"] = serviceCaller.Execute<List<ORGANIZATION>>(request);

            request = new RestRequest();
            request.Resource = "/Divisions";
            request.RootElement = "ArrayOfDIVISION";
            ViewData["AllDivs"] = serviceCaller.Execute<List<DIVISION>>(request);

            request = new RestRequest();
            request.Resource = "/Roles";
            request.RootElement = "ArrayOfROLE";
            ViewData["AllRoles"] = serviceCaller.Execute<List<ROLE>>(request);

            return PartialView(aUser);
        }

        //
        //POST the edit
        [HttpPost]
        public PartialViewResult UserEdit(int id, USER_ aUser)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Users/{userID}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("userID", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");

                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aUser);
                //request.AddHeader("Content-Type", "application/xml");
                //request.AddParameter("application/xml", request.XmlSerializer.Serialize(aMember), ParameterType.RequestBody);

                USER_ updatedUser = serviceCaller.Execute<USER_>(request);

                //populate viewdata info for organization, division and role              
                if (updatedUser.ORGANIZATION_ID != null && updatedUser.ORGANIZATION_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Organizations/{organizationID}";
                    request.RootElement = "ORGANIZATION";
                    request.AddParameter("organizationID", updatedUser.ORGANIZATION_ID, ParameterType.UrlSegment);
                    ViewData["Organization"] = serviceCaller.Execute<ORGANIZATION>(request).NAME;
                }
                if (updatedUser.DIVISION_ID != null && updatedUser.DIVISION_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/divisions/{divisionID}";
                    request.RootElement = "DIVISION";
                    request.AddParameter("divisionID", updatedUser.DIVISION_ID, ParameterType.UrlSegment);
                    ViewData["Division"] = serviceCaller.Execute<DIVISION>(request).DIVISION_NAME;
                }
                if (updatedUser != null && updatedUser.ROLE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/Roles/{roleID}";
                    request.RootElement = "ROLE";
                    request.AddParameter("roleID", updatedUser.ROLE_ID, ParameterType.UrlSegment);
                    ViewData["Role"] = serviceCaller.Execute<ROLE>(request).ROLE_NAME;
                }

                return PartialView("UserDetailsPV", updatedUser);
            }
            catch (Exception e)
            {
                return PartialView(e.ToString());
            }
        }


        //create page for User
        public ActionResult UserCreate()
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();

            //request organizations
            request.Resource = "/Organizations";
            request.RootElement = "ArrayOfORGANIZATION";
            ViewData["OrgList"] = serviceCaller.Execute<List<ORGANIZATION>>(request);

            //get divisions
            request = new RestRequest();
            request.Resource = "/Divisions";
            request.RootElement = "ArrayOfDIVISION";
            ViewData["DivList"] = serviceCaller.Execute<List<DIVISION>>(request);

            //request roles
            request = new RestRequest();
            request.Resource = "/Roles";
            request.RootElement = "ArrayOfRole";
            ViewData["roleList"] = serviceCaller.Execute<List<ROLE>>(request);

            return View();
        }

        //post new user
        [HttpPost]
        public ActionResult UserCreate(USER_ newUser)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/Users";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<USER_>(newUser), ParameterType.RequestBody);
                USER_ createdUser = serviceCaller.Execute<USER_>(request);

                return RedirectToAction("UserDetails", new { id = createdUser.USER_ID });
            }
            catch
            {
                return View();
            }
        }

        //call for who the member logged in is
        public string GetUserRole()
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "Users?username={userName}";
            request.RootElement = "USER_";
            request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
            USER_ thisUser = serviceCaller.Execute<USER_>(request);
            int roleId = Convert.ToInt32(thisUser.ROLE_ID);
            string Role = string.Empty;
            switch (roleId)
            {
                case 1: Role = "Admin"; break;
                case 2: Role = "Publisher"; break;
                case 3: Role = "Creator"; break;
                case 4: Role = "Enforcer"; break;
                case 5: Role = "Public"; break;
                case 6: Role = "Reviewer"; break;
                default: Role = "error"; break;
            }

            return Role;
        }
    }
}
