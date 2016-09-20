//------------------------------------------------------------------------------
//----- CropUseHandler ---------------------------------------------------------
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
    public class CropUseHandler:HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "crop_use"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods

        //---------------------Returns List of objects---------------------
        // returns all CROP_USE
       // [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<crop_use> cropUseList;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    cropUseList = GetEntities<crop_use>(aBLTE).ToList();
                }//end using
                
                //activateLinks<crop_use>(cropUseList);

                return new OperationResult.OK { ResponseResource = cropUseList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns versioned CropUses
       // [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedCropUses")]
        public OperationResult GetVersionedCropUses(string status, string date)
        {
            IQueryable<crop_use> cropUseQuery;
            List<crop_use> cropUses;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    cropUseQuery = GetEntities<crop_use>(aBLTE);
                    switch (statustype)
                    {
                        case (StatusType.Published):
                            cropUseQuery.Where(ai => ai.version.published_time_stamp != null);
                            break;
                        case (StatusType.Reviewed):
                            cropUseQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                            ai.version.published_time_stamp == null);
                            break;
                        //created
                        default:
                            cropUseQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                            ai.version.published_time_stamp == null);
                            break;
                    }//end switch

                    cropUseQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                        ai.version.expired_time_stamp < thisDate.Value.Date);

                    cropUses = cropUseQuery.ToList();

                }//end using
               

                //activateLinks<crop_use>(cropUses);

                return new OperationResult.OK { ResponseResource = cropUses };
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
            List<crop_use> aCropUseList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date; 

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    aCropUseList = GetActive(GetEntities<crop_use>(aBLTE), thisDate.Value.Date).OrderBy(a=>a.use).ToList();
                   
                }//end using

                //activateLinks<crop_use>(aCropUseList);

                return new OperationResult.OK { ResponseResource = aCropUseList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

    //    [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName="GetCropUses")]
        public OperationResult Get(Int32 cropUseID, [Optional] string date)
        {
            try
            {
                List<crop_use> cropUseList;
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                if (cropUseID < 0)
                { return new OperationResult.BadRequest(); }

                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<crop_use> query;
                    query = GetEntities<crop_use>(aBLTE).Where(ai => ai.crop_use_id == cropUseID);

                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    cropUseList = query.ToList();
                }//end using               

                //activateLinks<crop_use>(cropUseList);

                return new OperationResult.OK { ResponseResource = cropUseList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULALimitationsCropUse")]
        public OperationResult GetPULALimitationsCropUse(Int32 pulaLimitationID, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<crop_use> cropUseList;
                using (bltEntities aBLTE = GetRDS())
                {

                    IQueryable<crop_use> query1 =
                            (from PULALimit in GetActive(aBLTE.pula_limitations.Where(p => p.pula_limitation_id == pulaLimitationID), thisDate.Value.Date)
                             join cu in aBLTE.crop_use
                             on PULALimit.crop_use_id equals cu.crop_use_id
                             select cu).Distinct();

                    cropUseList = GetActive(query1, thisDate.Value.Date).ToList();

                    //activateLinks<crop_use>(cropUseList);

                }//end using

                return new OperationResult.OK { ResponseResource = cropUseList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //---------------------Returns individual objects---------------------
      //  [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET, ForUriName="GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            crop_use anCropUse;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    anCropUse = GetEntities<crop_use>(aBLTE).SingleOrDefault(c => c.id == entityID);

                }//end using
                

                //activateLinks<crop_use>(anCropUse);

                return new OperationResult.OK { ResponseResource = anCropUse };
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
        public OperationResult POST(crop_use anEntity)
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
                           
                            anEntity.crop_use_id = GetNextID(aBLTE);

                            aBLTE.crop_use.Add(anEntity);

                            aBLTE.SaveChanges();
                        }
                        else
                        {//it exists, check if expired
                            if (anEntity.version.expired_time_stamp.HasValue)
                            {
                                crop_use newCU = new crop_use();
                                newCU.use = anEntity.use;
                                newCU.version_id = SetVersion(aBLTE, newCU.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                                newCU.crop_use_id = anEntity.crop_use_id;
                                //anEntity.ID = 0;
                                aBLTE.crop_use.Add(newCU);
                                aBLTE.SaveChanges();
                            }//end if
                        }//end if

                        //activateLinks<crop_use>(anEntity); 
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
        public OperationResult Put(Int32 entityID, crop_use anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            crop_use aCropUse;
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

                        aCropUse = aBLTE.crop_use.FirstOrDefault(c => c.id == entityID);

                        if (aCropUse == null)
                        { return new OperationResult.BadRequest(); }

                        if (aCropUse.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.crop_use_id = aCropUse.crop_use_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.crop_use.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            aCropUse = anEntity;
                        }
                        else
                        {
                            aCropUse.use = anEntity.use;
                        }//end if

                        aBLTE.SaveChanges();

                        //activateLinks<crop_use>(anEntity);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
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
                    crop_use ObjectToBeDelete = aBLTE.crop_use.FirstOrDefault(am => am.id == entityID);

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
                        aBLTE.crop_use.Remove(ObjectToBeDelete);
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

        private bool Exists(bltEntities aBLTE, ref crop_use anEntity)
        {
            crop_use existingEntity;
            crop_use thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.crop_use.FirstOrDefault(mt => string.Equals(mt.use.ToUpper(), thisEntity.use.ToUpper()));
        

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
            if (aBLTE.crop_use.Count() > 0)
                nextID = aBLTE.crop_use.OrderByDescending(p => p.crop_use_id).First().crop_use_id + 1;

            return nextID;
        }

        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, crop_use cropUse, DateTime dt)
        {
            //get all published, should only be 1
            List<crop_use> aiList = aBLTE.crop_use.Where(p => p.crop_use_id == cropUse.crop_use_id &&
                                                        p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if (!p.Equals(cropUse))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities

        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<crop_use> aiList = aBLTE.crop_use.Where(p => p.crop_use_id == Id &&
                                                        p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }
        #endregion


    }//end class CropUseHandler
}// end namespace