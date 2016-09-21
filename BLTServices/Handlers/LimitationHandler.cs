//------------------------------------------------------------------------------
//----- LimitationHandler ------------------------------------------------------
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
// 05.14.13 - JKN - Created
#endregion

using BLTServices.Authentication;
using BLTDB;
using OpenRasta.Web;
using OpenRasta.Security;

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
    public class LimitationHandler:HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "limitations"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all CROP_USE
    //    [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<limitation> limitationList;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    limitationList = GetEntities<limitation>(aBLTE).ToList();
                }//end using

                //activateLinks<limitation>(limitationList);

                return new OperationResult.OK { ResponseResource = limitationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all PULAs
        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedLimitations")]
        public OperationResult GetVersionedLimitations(string status, string date)
        {
            IQueryable<limitation> limitQuery;
            List<limitation> limitation;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    limitQuery = GetEntities<limitation>(aBLTE);
                    switch (statustype)
                    {
                        case (StatusType.Published):
                            limitQuery.Where(ai => ai.version.published_time_stamp != null);
                            break;
                        case (StatusType.Reviewed):
                            limitQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                            ai.version.published_time_stamp == null);
                            break;
                        //created
                        default:
                            limitQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                            ai.version.published_time_stamp == null);
                            break;
                    }//end switch

                    limitQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                        ai.version.expired_time_stamp < thisDate.Value.Date);

                    limitation = limitQuery.ToList();

                }//end using

                //activateLinks<limitation>(limitation);

                return new OperationResult.OK { ResponseResource = limitation };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all active CROP_USE
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<limitation> limitationList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    limitationList = GetActive(GetEntities<limitation>(aBLTE), thisDate.Value.Date).OrderBy(a => a.code).ToList();
                }//end using
                

                //activateLinks<limitation>(limitationList);

                return new OperationResult.OK { ResponseResource = limitationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

    //    [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName="GetLimitations")]
        public OperationResult Get(Int32 limitationID, [Optional] string date)
        {
            try
            {
                List<limitation> limitationList;
                DateTime? thisDate = ValidDate(date);

                if (limitationID < 0)
                { return new OperationResult.BadRequest(); }

                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<limitation> query;
                    query = GetEntities<limitation>(aBLTE).Where(l => l.limitation_id == limitationID);

                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    limitationList = query.ToList();
                }//end using

                //activateLinks<limitation>(limitationList);

                return new OperationResult.OK { ResponseResource = limitationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULALimitationsLimitation")]
        public OperationResult GetPULALimitationsLimitation(Int32 pulaLimitationID, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<limitation> limitationsList;
                using (bltEntities aBLTE = GetRDS())
                {

                    IQueryable<limitation> query1 =
                            (from PULALimit in GetActive(aBLTE.pula_limitations.Where(p => p.pula_limitation_id == pulaLimitationID), thisDate.Value.Date)
                             join l in aBLTE.limitations
                             on PULALimit.limitation_id equals l.limitation_id
                             select l).Distinct();

                    limitationsList = GetActive(query1, thisDate.Value).ToList();

                    //activateLinks<limitation>(limitationsList);

                }//end using

                return new OperationResult.OK { ResponseResource = limitationsList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //---------------------Returns individual objects---------------------
       // [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            limitation aLimitation;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    aLimitation = GetEntities<limitation>(aBLTE).SingleOrDefault(l => l.id == entityID);

                }//end using
                
                //activateLinks<limitation>(aLimitation);

                return new OperationResult.OK { ResponseResource = aLimitation };
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
        public OperationResult POST(limitation anEntity)
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

                            anEntity.limitation_id = GetNextID(aBLTE);

                            aBLTE.limitations.Add(anEntity);
                            aBLTE.SaveChanges();
                                                        
                        }//end if

                        //activateLinks<limitation>(anEntity);
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
        public OperationResult Put(Int32 entityID, limitation anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            limitation aLimitation;
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

                        aLimitation = aBLTE.limitations.FirstOrDefault(c => c.id == entityID);
                        if (aLimitation == null)
                        { return new OperationResult.BadRequest(); }

                        if (aLimitation.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.limitation_id = aLimitation.limitation_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.limitations.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            aLimitation = anEntity;
                        }
                        else
                        {
                            aLimitation.limitation1 = anEntity.limitation1;
                        }//end if

                        aBLTE.SaveChanges();

                        //activateLinks<limitation>(aLimitation);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aLimitation };
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

                    limitation ObjectToBeDelete = aBLTE.limitations.FirstOrDefault(l => l.id == entityID);

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
                        aBLTE.limitations.Remove(ObjectToBeDelete);
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
        private bool Exists(bltEntities aBLTE, ref limitation anEntity)
        {
            limitation existingEntity;
            limitation thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.limitations.FirstOrDefault(mt => string.Equals(mt.code.ToUpper(), thisEntity.code.ToUpper()) &&
                                                                         string.Equals(mt.limitation1.ToUpper(), thisEntity.limitation1.ToUpper()));

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
            if (aBLTE.limitations.Count() > 0)
                nextID = aBLTE.limitations.OrderByDescending(p => p.limitation_id).First().limitation_id + 1;

            return nextID;
        }
        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, limitation limitation, DateTime dt)
        {
            //get all published, should only be 1
            List<limitation> limitList = aBLTE.limitations.Where(p => p.limitation_id == limitation.limitation_id &&
                                                                                p.version.published_time_stamp <= dt.Date).ToList();
            if (limitList == null) return;

            foreach (var p in limitList)
            {
                if (!p.Equals(limitation))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities
        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {           
            //get all published, should only be 1
            List<limitation> aiList = aBLTE.limitations.Where(p => p.limitation_id == Id &&
                                                              p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }
        #endregion



    }//end LimitationHandler
}//end namespace