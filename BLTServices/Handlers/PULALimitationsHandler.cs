//------------------------------------------------------------------------------
//----- PULALimitationsHandler ---------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

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
// 11.09.12 - TR - Changed class name from SiteHandler to PULALimitationsHandler and removed ForUriName "Sites" references
// 10.19.12 - JB - Created from STN
#endregion


using BLTServices.Authentication;
using BLTServices.Resources;
using BLTDB;
using OpenRasta.Web;
using OpenRasta.Security;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;


namespace BLTServices.Handlers
{
    public class PULALimitationsHandler: HandlerBase
    {

        #region Properties
        public override string entityName
        {
            get { return "pula_limitations"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all PULA_LIMITATIONS
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<pula_limitations> pulaLimit;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    pulaLimit = GetEntities<pula_limitations>(aBLTE).ToList();
                }//end using

                activateLinks<pula_limitations>(pulaLimit);

                return new OperationResult.OK { ResponseResource = pulaLimit };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all PULAs
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedPULALimitations")]
        public OperationResult GetVersionedPULALimitations(string status, string date)
        {
            ObjectQuery<pula_limitations> pulaLimitQuery;
            List<pula_limitations> pulaLimitList;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    pulaLimitQuery = GetEntities<pula_limitations>(aBLTE);
                    switch (statustype)
                    {
                        case (StatusType.Published):
                            pulaLimitQuery.Where(ai => ai.version.published_time_stamp != null);
                            break;
                        case (StatusType.Reviewed):
                            pulaLimitQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                            ai.version.published_time_stamp == null);
                            break;
                        //created
                        default:
                            pulaLimitQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                            ai.version.published_time_stamp == null);
                            break;
                    }//end switch

                    pulaLimitQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                        ai.version.expired_time_stamp < thisDate.Value.Date);

                    pulaLimitList = pulaLimitQuery.ToList();

                }//end using

                activateLinks<pula_limitations>(pulaLimitList);

                return new OperationResult.OK { ResponseResource = pulaLimitList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all active PULA_LIMITATIONS
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<pula_limitations> pulaLimit = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date; 

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    pulaLimit = GetActive(GetEntities<pula_limitations>(aBLTE), thisDate.Value.Date).ToList();
                }//end using

                activateLinks<pula_limitations>(pulaLimit);

                return new OperationResult.OK { ResponseResource = pulaLimit };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

        // returns all PULALimitations (active if date is supplied)
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 pulaLimitationsID, [Optional] string date)
        {
            try
            {
                List<pula_limitations> pulaLimitationList;
                DateTime? thisDate = ValidDate(date);

                if (pulaLimitationsID < 0)
                { return new OperationResult.BadRequest(); }

                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<pula_limitations> query;
                    query = GetEntities<pula_limitations>(aBLTE).Where(pl => pl.pula_limitation_id == pulaLimitationsID);

                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    pulaLimitationList = query.ToList();
                }//end using

                activateLinks<pula_limitations>(pulaLimitationList);

                return new OperationResult.OK { ResponseResource = pulaLimitationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns Active PULA_LIMITATIONS for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULALimitations")]
        public OperationResult GetPULALimitations(Int32 pulaId, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<pula_limitations> pulaLimitationList;
                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<pula_limitations> query1 = GetEntities<pula_limitations>(aBLTE).Where(a => a.pula_id == pulaId);

                    pulaLimitationList = GetActive(query1, thisDate.Value.Date).ToList();

                    activateLinks<pula_limitations>(pulaLimitationList);

                }//end using

                return new OperationResult.OK { ResponseResource = pulaLimitationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns Active PULA_LIMITATIONS for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetMapperLimitations")]
        public OperationResult GetMapperLimitations(Int32 pulaId, Int32 pulaSHPID, string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                MapperLimitations MapLimitList = new MapperLimitations();

                using (bltEntities aBLTE = GetRDS())
                {
                    active_ingredient_pula thisPULA = aBLTE.active_ingredient_pula.Where(x=>x.pula_id == pulaId && x.pula_shape_id == pulaSHPID).FirstOrDefault();

                    //go get the limitations, then when populating each MapperLimit - go get each piece using the id of each
                    IQueryable<pula_limitations> query1 = GetEntities<pula_limitations>(aBLTE).Where(a => a.pula_id == pulaId);

                    List<pula_limitations> pulaLimitationList = GetActive(query1, thisDate.Value.Date).ToList();
                    MapLimitList.MapperLimits = pulaLimitationList.Select(p => new MapperLimit()
                    {
                        PULAID = pulaId,
                        PULASHPID = pulaSHPID,
                        NAME = GetAIorProdName(p.active_ingredient_id, p.product_id, thisDate.Value.Date, aBLTE),
                        USE = GetActive(aBLTE.crop_use.Where(u => u.crop_use_id == p.crop_use_id), thisDate.Value.Date).FirstOrDefault().use,
                        APPMETHOD = aBLTE.application_method.Where(u => u.application_method_id == p.application_method_id).FirstOrDefault().method,
                        FORM = aBLTE.formulations.Where(u => u.formulation_id == p.formulation_id).FirstOrDefault().form,
                        LIMIT = aBLTE.limitations.Where(u => u.limitation_id == p.limitation_id).FirstOrDefault()
                    }).ToList();
                    
                }//end using

                return new OperationResult.OK { ResponseResource = MapLimitList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }

        //---------------------Returns individual objects---------------------
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            pula_limitations aPULALimit;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    aPULALimit = GetEntities<pula_limitations>(aBLTE).SingleOrDefault(ai => ai.pula_limitation_id == entityID);

                }//end using

                activateLinks<pula_limitations>(aPULALimit);

                return new OperationResult.OK { ResponseResource = aPULALimit };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        #endregion

        #region Status Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "UpdatePulaLimitationsStatus")]
        public OperationResult UpdatePulaLimitationsStatus(Int32 entityID, string status, [Optional] string date)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            pula_limitations aPULALimitation;
            try
            {
                StatusType statusType = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue || thisDate.Value.Date < DateTime.Now.Date)
                    thisDate = DateTime.Now.Date;

                if (entityID <= 0 || string.IsNullOrEmpty(status) || statusType == StatusType.Created)
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //version user
                        loggedInUser = LoggedInUser(aBLTE);

                        aPULALimitation = aBLTE.pula_limitations.FirstOrDefault(ai => ai.pula_limitation_id == entityID);

                        if (aPULALimitation == null)
                        { return new OperationResult.BadRequest(); }

                        //assign version
                        aPULALimitation.version_id = SetVersion(aBLTE, aPULALimitation.version_id, loggedInUser.user_id, statusType, thisDate.Value.Date).version_id;

                        aBLTE.SaveChanges();
                        activateLinks<pula_limitations>(aPULALimitation);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aPULALimitation };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion

        #region POST Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(pula_limitations anEntity)
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
                            
                            aBLTE.pula_limitations.Add(anEntity);

                            aBLTE.SaveChanges();
                        }//end if
                        activateLinks<pula_limitations>(anEntity);
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
        public OperationResult Put(Int32 entityID, pula_limitations anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            pula_limitations aPULALimit;
            try
            {
                if (entityID >= 0)
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //version user
                        loggedInUser = LoggedInUser(aBLTE);

                        aPULALimit = aBLTE.pula_limitations.FirstOrDefault(ai => ai.pula_limitation_id == entityID);
                        if (aPULALimit == null)
                        { return new OperationResult.BadRequest(); }

                        if (aPULALimit.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. create new
                            // assign Version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.pula_limitations.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            aPULALimit = anEntity;
                        }
                        else
                        {
                            aPULALimit.active_ingredient_id = anEntity.active_ingredient_id;
                            aPULALimit.application_method_id = anEntity.application_method_id;
                            aPULALimit.crop_use_id = anEntity.crop_use_id;
                            aPULALimit.formulation_id = anEntity.formulation_id;
                            aPULALimit.limitation_id = anEntity.limitation_id;
                            aPULALimit.product_id = anEntity.product_id;
                            aPULALimit.pula_id = anEntity.pula_id;
                        }//end if
                            
                        aBLTE.SaveChanges();

                        activateLinks<pula_limitations>(anEntity);
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
                    pula_limitations ObjectToBeDelete = aBLTE.pula_limitations.FirstOrDefault(f => f.pula_limitation_id == entityID);

                    if (ObjectToBeDelete == null)
                    { return new OperationResult.BadRequest(); }

                    //NOTE: ShapeID can not be changed
                    //need to see if the PULA this PULA_LIMITATION is a part of is published (PULA_LIMITATION is published when created)
                    active_ingredient_pula thisPULA = aBLTE.active_ingredient_pula.FirstOrDefault(a => a.pula_id == ObjectToBeDelete.pula_id);
                    if (thisPULA.is_published == 1)
                    {
                        ObjectToBeDelete.version_id = SetVersion(ObjectToBeDelete.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, DateTime.Now.Date).version_id;
                    }
                    else
                    {
                        aBLTE.pula_limitations.Remove(ObjectToBeDelete);
                    }//end if

                    aBLTE.SaveChanges();
                }//end using
            }//end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }//end HttpMethod.GET

        #endregion
        
        #endregion
        #region Helper Methods
        private bool Exists(bltEntities aBLTE, ref pula_limitations anEntity)
        {
            pula_limitations existingEntity;
            pula_limitations thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.pula_limitations.FirstOrDefault(mt => mt.pula_id == thisEntity.pula_id &&
                                                                            (mt.active_ingredient_id == thisEntity.active_ingredient_id || thisEntity.active_ingredient_id <= 0 || thisEntity.active_ingredient_id == null) &&
                                                                            (mt.product_id == thisEntity.product_id || thisEntity.product_id <= 0 || thisEntity.product_id == null) &&
                                                                            (mt.limitation_id == thisEntity.limitation_id || thisEntity.limitation_id <= 0 || thisEntity.limitation_id == null) &&
                                                                            (mt.crop_use_id == thisEntity.crop_use_id || thisEntity.crop_use_id <= 0 || thisEntity.crop_use_id == null) &&
                                                                            (mt.application_method_id == thisEntity.application_method_id || thisEntity.application_method_id <= 0 || thisEntity.application_method_id == null) &&
                                                                            (mt.formulation_id == thisEntity.formulation_id || thisEntity.formulation_id <= 0 || thisEntity.formulation_id == null)
                                                                            );
            
                if (existingEntity == null)
                    return false;

                //if exists then update ref
                anEntity = existingEntity;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists
        protected void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, pula_limitations pulaLimitation, DateTime dt)
        {
            //get all published, should only be 1
            List<pula_limitations> aiList = aBLTE.pula_limitations.Where(p => p.pula_limitation_id == pulaLimitation.pula_limitation_id &&
                                                                                p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if (!p.Equals(pulaLimitation))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities
        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<pula_limitations> pulaLimitList = aBLTE.pula_limitations.Where(p => p.pula_limitation_id == Id &&
                                                               p.version.published_time_stamp <= dt.Date).ToList();
            if (pulaLimitList == null) return;

            foreach (var p in pulaLimitList)
            {
                p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }

        private string GetAIorProdName(decimal? AI_ID, decimal? PROD_ID, DateTime d, bltEntities aBLTE)
        {
            string name = string.Empty;
            //IQueryable<PULA_LIMITATIONS> query1 = GetEntities<PULA_LIMITATIONS>(aBLTE).Where(a => a.PULA_ID == pulaId);
            //List<PULA_LIMITATIONS> pulaLimitationList = GetActive(query1, thisDate.Value.Date).ToList();

            if (AI_ID > 0)
            {
                IQueryable<active_ingredient> qA = aBLTE.active_ingredient.Where(x => x.active_ingredient_id == AI_ID);
                
                name = GetActive(qA, d).FirstOrDefault().ingredient_name;
                    
            }
            else
            {

                IQueryable<product> qP = aBLTE.products.Where(x => x.product_id == PROD_ID);

                product qprod = GetActive(qP, d).FirstOrDefault();
                name = qprod.product_name + " [" + qprod.product_registration_number + "]";

                //PRODUCT aProd = GetActive(aBLTE.PRODUCT.Where(x => x.PRODUCT_ID == PROD_ID).FirstOrDefault, d);
                //name = aProd.PRODUCT_NAME 
            }
            return name;
        }//end HttpMethod.GET
        
        #endregion
        
    }//end class PULALimitationsHandler

}//end namespace