//------------------------------------------------------------------------------
//----- Application Method Handler -----------------------------------------------------------
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
// 07.22.13 - TR - Renamed from ModifiersHandler to ApplicationMethodHandler
// 05.14.13 - JKN - Created
#endregion

using BLTServices.Authentication;

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
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;


namespace BLTServices.Handlers
{
    public class ApplicationMethodHandler: HandlerBase
    {

        #region Properties
        public override string entityName
        {
            get { return "application_method"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all APPLICATION_METHOD
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<application_method> anApplicationMethod;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        anApplicationMethod = GetEntities<application_method>(aBLTE).ToList();
                    }//end using
              //  }//end using
                activateLinks<application_method>(anApplicationMethod);

                return new OperationResult.OK { ResponseResource = anApplicationMethod };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all PULAs
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedApplicationMethods")]
        public OperationResult GetVersionedApplicationMethods(string status, string date)
        {
            ObjectQuery<application_method> appMethQuery;
            List<application_method> appMethods;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    appMethQuery = GetEntities<application_method>(aBLTE);
                    switch (statustype)
                    {
                        case (StatusType.Published):
                            appMethQuery.Where(ai => ai.version.published_time_stamp != null);
                            break;
                        case (StatusType.Reviewed):
                            appMethQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                            ai.version.published_time_stamp == null);
                            break;
                        //created
                        default:
                            appMethQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                            ai.version.published_time_stamp == null);
                            break;
                    }//end switch

                    appMethQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                        ai.version.expired_time_stamp < thisDate.Value.Date);

                    appMethods = appMethQuery.ToList();

                }//end using
                

                activateLinks<application_method>(appMethods);

                return new OperationResult.OK { ResponseResource = appMethods };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all active APPLICATION_METHOD
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<application_method> anApplicationMethodList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date; 

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    anApplicationMethodList = GetActive(GetEntities<application_method>(aBLTE), thisDate.Value.Date).OrderBy(a=>a.method).ToList();
                }//end using

                activateLinks<application_method>(anApplicationMethodList);

                return new OperationResult.OK { ResponseResource = anApplicationMethodList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetApplicationMethods")]
        public OperationResult GetApplicationMethods(Int32 applicationMethodID, [Optional] string date)
        {
            try
            {
                List<application_method> applicationMethodList;
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                if (applicationMethodID < 0)
                { return new OperationResult.BadRequest(); }


                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<application_method> query;
                    query = GetEntities<application_method>(aBLTE).Where(am => am.application_method_id == applicationMethodID);

                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    applicationMethodList = query.ToList();
                }//end using
                

                activateLinks<application_method>(applicationMethodList);

                return new OperationResult.OK { ResponseResource = applicationMethodList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULALimitationsApplicationMethod")]
        public OperationResult GetPULALimitationsApplicationMethod(Int32 pulaLimitatationsID, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<application_method> applicationMethodList;
                using (bltEntities aBLTE = GetRDS())
                {

                    IQueryable<application_method> query1 =
                            (from PULALimit in GetActive(aBLTE.pula_limitations.Where(p => p.pula_limitation_id == pulaLimitatationsID), thisDate.Value.Date)
                             join am in aBLTE.application_method
                             on PULALimit.application_method_id equals am.application_method_id
                             select am).Distinct();

                    applicationMethodList = GetActive(query1, thisDate.Value.Date).ToList();

                    activateLinks<application_method>(applicationMethodList);

                }//end using

                return new OperationResult.OK { ResponseResource = applicationMethodList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
       
        //---------------------Returns individual objects---------------------
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            application_method anApplicationMethod;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    anApplicationMethod = GetEntities<application_method>(aBLTE).SingleOrDefault(c => c.id == entityID);

                }//end using                

                activateLinks<application_method>(anApplicationMethod);

                return new OperationResult.OK { ResponseResource = anApplicationMethod };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        #endregion

        #region POST Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(application_method anEntity)
        {
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        if (!Exists(aBLTE, ref anEntity))
                        {
                            //create version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;

                            anEntity.application_method_id = GetNextID(aBLTE);

                            aBLTE.application_method.Add(anEntity);

                            aBLTE.SaveChanges();
                        }
                        else
                        {//it exists, check if expired
                            if (anEntity.version.expired_time_stamp.HasValue)
                            {
                                application_method newAM = new application_method();
                                newAM.method = anEntity.method;
                                newAM.version_id = SetVersion(aBLTE, newAM.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                                newAM.application_method_id = anEntity.application_method_id;
                                //anEntity.ID = 0;
                                aBLTE.application_method.Add(newAM);
                                aBLTE.SaveChanges();
                            }//end if
                        }
                        activateLinks<application_method>(anEntity);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion

        #region PUT/EDIT Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityID, application_method anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            application_method anApplicationMethod;
            try
            {
                if (entityID <= 0)
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //version user
                        loggedInUser = LoggedInUser(aBLTE);

                        anApplicationMethod = aBLTE.application_method.FirstOrDefault(c => c.id == entityID);
                        if (anApplicationMethod == null)
                        { return new OperationResult.BadRequest(); }

                        if (anApplicationMethod.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.application_method_id = anApplicationMethod.application_method_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.application_method.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            anApplicationMethod = anEntity;
                        }
                        else
                        {
                            anApplicationMethod.method = anEntity.method;
                        }//end if

                        aBLTE.SaveChanges();

                        activateLinks<application_method>(anApplicationMethod);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anApplicationMethod };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion

        #region DELETE Methods
        [WiMRequiresRole(new string[] { AdminRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityID)
        {
            //Return BadRequest if missing required fields
            if (entityID <= 0)
                return new OperationResult.BadRequest();
            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (bltEntities aBLTE = GetRDS(securedPassword))
                {
                    application_method ObjectToBeDelete = aBLTE.application_method.FirstOrDefault(am => am.id == entityID);

                    if (ObjectToBeDelete == null)
                    { return new OperationResult.BadRequest(); }

                    //NOTE: ShapeID can not be changed
                    if (ObjectToBeDelete.version.published_time_stamp.HasValue)
                    {
                        //set the date to be first of following month 
                        //int nextMo = DateTime.Now.Month + 1;
                        //DateTime nextMonth = Convert.ToDateTime(nextMo + "/01/" + DateTime.Now.Year);
                        ObjectToBeDelete.version_id = SetVersion(ObjectToBeDelete.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, DateTime.Now.Date).version_id;
                    }
                    else
                    {
                        aBLTE.application_method.Remove(ObjectToBeDelete);
                    }//end if

                    aBLTE.SaveChanges();
                }//end using
            }//end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }//end HttpMethod.DELETE
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(bltEntities aBLTE, ref application_method anEntity)
        {
            application_method existingEntity;
            application_method thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.application_method.FirstOrDefault(mt => string.Equals(mt.method.ToUpper(), thisEntity.method.ToUpper()));
           

                if (existingEntity == null)
                    return false;

                //if exists then update ref contact
                anEntity = existingEntity;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists
        private Int32 GetNextID(bltEntities aBLTE)
        {
            //create pulaID
            Int32 nextID = 1;
            if (aBLTE.application_method.Count() > 0)
                nextID = aBLTE.application_method.OrderByDescending(p => p.application_method_id).First().application_method_id + 1;

            return nextID;
        }
        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, application_method applicationMethod, DateTime dt)
        {
            //get all published, should only be 1
            List<application_method> aiList = aBLTE.application_method.Where(p => p.application_method_id == applicationMethod.application_method_id &&
                                                                                p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if (!p.Equals(applicationMethod))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities
        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<application_method> aiList = aBLTE.application_method.Where(p => p.application_method_id == Id &&
                                                               p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }
        #endregion
    }//end class ApplicationMethodHandler

}//end namespace