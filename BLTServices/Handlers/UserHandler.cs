//------------------------------------------------------------------------------
//----- UserHandler ---------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Site resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 05.15.13 - JKN - Created
#endregion

using BLTServices.Authentication;
using BLTServices.Security;
using OpenRasta.Web;
using OpenRasta.Security;
using BLTDB;
using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;

namespace BLTServices.Handlers
{
    public class UserHandler:HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "user_"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        // returns all USERS
        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<user_> userList;
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                       userList  = GetEntities<user_>(aBLTE).ToList();
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = userList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole, Public })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 userID)
        {
            List<user_> userList;
            try
            {
                if (userID <= 0)
                { return new OperationResult.BadRequest(); }
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        userList = GetEntities<user_>(aBLTE).Where(u => u.user_id== userID).ToList();
                    }//end using
                }//end using
                
                return new OperationResult.OK { ResponseResource = userList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.GET, ForUriName="GetVersionUsers")]
        public OperationResult GetVersionUsers(Int32 versionID)
        {
            List<user_> userList;
            try
            {
                if (versionID <= 0)
                { return new OperationResult.BadRequest(); }
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        userList = GetEntities<user_>(aBLTE).Where(u => u.versions.Any(v => v.version_id == versionID) ||
                                                                                    u.versions1.Any(v => v.version_id == versionID) ||
                                                                                    u.versions2.Any(v => v.version_id == versionID)).ToList();
                    }//end using
                }//end using
                
                return new OperationResult.OK { ResponseResource = userList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole, Public })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetUserByUserName")]
        public OperationResult GetUserByUserName(string userName)
        {
            user_ aUser; 
            try
            {
                if(string.IsNullOrEmpty(username))
                { return new OperationResult.BadRequest(); }
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                       aUser = GetEntities<user_>(aBLTE).FirstOrDefault(u => string.Equals(u.username.ToUpper(), username.ToUpper()));
                    }//end using
                }//end using
                return new OperationResult.OK { ResponseResource = aUser };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

       
        #endregion
        
        #region POST Methods
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(user_ anEntity)
        {
            try
            {
                //Return BadRequest if missing required fields
                if ((string.IsNullOrEmpty(anEntity.username) || anEntity.role_id <= 0))
                { return new OperationResult.BadRequest(); }

                //Get basic authenication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //Prior to running stored procedure, check if username exists
                        if (!Exists(aBLTE, ref anEntity))
                        {
                            if (string.IsNullOrEmpty(anEntity.password))
                                anEntity.password = buildDefaultPassword(anEntity);
                            else
                                anEntity.password = Encoding.UTF8.GetString(Convert.FromBase64String(anEntity.password));
                            
                            //Check if USERNAME exists
                            if (aBLTE.user_.FirstOrDefault(m => String.Equals(m.username.ToUpper().Trim(), anEntity.username.ToUpper().Trim())) != null)
                                return new OperationResult.BadRequest { Description = "Username exists" };

                            // Create user profile using db stored procedure
                            // stored db throws errors internally but does not pass pass error
                            anEntity.salt = Cryptography.CreateSalt();
                            anEntity.password = Cryptography.GenerateSHA256Hash(anEntity.password, anEntity.salt);

                            //aBLTE.USER_PROFILE_ADD(anEntity.USERNAME, buildDefaultPassword(anEntity));
                            //aBLTE.USER_PROFILE_ADDROLE(anEntity.USERNAME, anEntity.ROLE_ID);
                            aBLTE.user_.Add(anEntity);
                            aBLTE.SaveChanges();
                            //remove info not relevant
                            anEntity.password = string.Empty;
                            anEntity.salt = string.Empty;
                        }

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion
        #region PUT Methods
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT, ForUriName = "Put")]
        public OperationResult Put(Int32 userID, user_ aUser)
        {
             //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(aUser.username) || aUser.role_id <= 0))
            { return new OperationResult.BadRequest();}
            
            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (bltEntities aBLTE = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    user_ ObjectToBeUpdated = aBLTE.user_.Single(m => m.user_id == userID);
                    
                    //Username
                    ObjectToBeUpdated.username = aUser.username;
                    //FirstName
                    ObjectToBeUpdated.fname = aUser.fname;
                    //Last Name
                    ObjectToBeUpdated.lname = aUser.lname;
                    //OrganizationID
                    ObjectToBeUpdated.organization_id = aUser.organization_id;
                    //DivisionID
                    ObjectToBeUpdated.division_id = aUser.division_id;
                    //Phone
                    ObjectToBeUpdated.phone = aUser.phone;
                    //Email
                    ObjectToBeUpdated.email = aUser.email;

                   if (!string.IsNullOrEmpty(aUser.password) && !Cryptography
                            .VerifyPassword(Encoding.UTF8.GetString(Convert.FromBase64String(aUser.password)), ObjectToBeUpdated.salt, ObjectToBeUpdated.password))
                    {
                        ObjectToBeUpdated.salt = Cryptography.CreateSalt();
                        ObjectToBeUpdated.password = Cryptography.GenerateSHA256Hash(Encoding.UTF8.GetString(Convert.FromBase64String(aUser.password)), ObjectToBeUpdated.salt);
                        ObjectToBeUpdated.resetFlag = null;
                    }
                    
                    aBLTE.SaveChanges();

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aUser };

        }//end HttpMethod.PUT
        #endregion

        #region DELETE Methods
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 userID)
        {
            user_ ObjectToBeDeleted = null;

            //Return BadRequest if missing required fields
            if (userID <= 0)
            {
                return new OperationResult.BadRequest();
            }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (bltEntities aBLTRDS = GetRDS(securedPassword))
                {

                    // create user profile using db stored proceedure
                    try
                    {
                        //fetch the object to be updated (assuming that it exists)
                        ObjectToBeDeleted = aBLTRDS.user_.SingleOrDefault(m => m.user_id == userID);

                        //Try to remove user profile first
                        //aBLTRDS.USER_PROFILE_REMOVE(ObjectToBeDeleted.username);

                        //delete it
                        aBLTRDS.user_.Remove(ObjectToBeDeleted);
                        aBLTRDS.SaveChanges();
                        //Return object to verify persisitance

                        return new OperationResult.OK { };

                    }
                    catch (Exception)
                    {
                        //TODO: relay failure type message 
                        // EX. if profile failed to be removed
                        return new OperationResult.BadRequest();
                    }

                }// end using
            } //end using
        }//end HTTP.DELETE
        #endregion
        #endregion

        #region Helper Methods
        private Boolean Exists(bltEntities aBLTE, ref user_ aUser)
        {
            user_ existingDataManager;
            user_ thisDataManager = aUser;
            //check if it exists
            try
            {
                existingDataManager = aBLTE.user_.FirstOrDefault(m => m.username == thisDataManager.username);

                if (existingDataManager == null)
                    return false;

                //if exists then update ref contact
                aUser = existingDataManager;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private string buildDefaultPassword(user_ dm)
        {
            //BLTDefau1t+numbercharInlastname+first2letterFirstName
            if (dm.username == "guest")
            {
                return "BLTDefau1t";
            }
            else
            {
                return "BLTDefau1t" + dm.lname.Count() + dm.fname.Substring(0, 2);
            }
        }//end buildDefaultPassword
        #endregion

        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            throw new NotImplementedException();
        }
    }//end class UserHandler
}//end namespace