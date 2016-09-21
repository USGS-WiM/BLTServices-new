//------------------------------------------------------------------------------
//----- ActiveIngredientPULAHander ---------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Peak summary resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     

#region Comments
// 05.07.13 - JKN  -Created

#endregion
using BLTServices.Authentication;
using BLTDB;
using OpenRasta.Web;
using OpenRasta.Security;
using OpenRasta.Diagnostics;

using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using BLTServices.Resources;

namespace BLTServices.Handlers   
{
    public class ActiveIngredientPULAHander : HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "active_ingredient_pula"; }
        }
        #endregion
        #region Routed Methods    
        #region Get Methods
        //---------------------Returns List of objects---------------------
        // returns all PULAs
        [HttpOperation(HttpMethod.GET, ForUriName="GetAll")]
        public OperationResult Get()
        {
            List<active_ingredient_pula> aiPula;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        aiPula = GetEntities<active_ingredient_pula>(aBLTE).ToList();
                    }//end using
               // }//end using

                    //activateLinks<active_ingredient_pula>(aiPula);

                return new OperationResult.OK { ResponseResource = aiPula };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all PULAs
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedPulas")]
        public OperationResult GetVersionedPulas(string status, string date)
        {
            IQueryable<active_ingredient_pula> aiQuery;
            List<active_ingredient_pula> aiPula;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        aiQuery = GetEntities<active_ingredient_pula>(aBLTE);
                        switch (statustype)
                        {
                            case (StatusType.Published):
                                 aiQuery.Where(ai => ai.version.published_time_stamp != null);
                                 break;
                            case (StatusType.Reviewed):
                                 aiQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                                 ai.version.published_time_stamp == null);
                                 break;
                                //created
                            default:
                                 aiQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                                 ai.version.published_time_stamp == null);
                                 break;
                        }//end switch

                        aiQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                            ai.version.expired_time_stamp < thisDate.Value.Date);

                        aiPula=aiQuery.ToList();

                    }//end using
               // }//end using

                    //activateLinks<active_ingredient_pula>(aiPula);

                return new OperationResult.OK { ResponseResource = aiPula };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
        
        // returns all active PULAs
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPublishedPulas")]
        public OperationResult Get(string date)
        {
            List<active_ingredient_pula> aiPula = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;
                
                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    aiPula = GetActive(GetEntities<active_ingredient_pula>(aBLTE), thisDate.Value.Date).ToList();
                }//end using
                //activateLinks<active_ingredient_pula>(aiPula);

                return new OperationResult.OK { ResponseResource = aiPula };
            }
            catch (Exception ex)
            {
               return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

        // returns the active PULAS for the given PULAID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAIPULA")]
        public OperationResult GetAIPULA(Int32 pulaShapeId, [Optional] string date)
        {
            List<active_ingredient_pula> aiPulas;
            try
            {
                if (pulaShapeId <= 0)
                { return new OperationResult.BadRequest { }; }

                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    aiPulas = GetActive(GetEntities<active_ingredient_pula>(aBLTE), thisDate.Value.Date).Where(p => p.pula_shape_id == pulaShapeId).ToList();
                }//end using

                //activateLinks<active_ingredient_pula>(aiPulas);

                return new OperationResult.OK { ResponseResource = aiPulas };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active PULAS for the given shapeID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetSimplePULAS")]
        public OperationResult GetSimplePULAS([Optional] string date)
        {
            PULAList aiPulas = new PULAList();
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;


                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    //IQueryable<active_ingredient_pula> query = GetEntities<active_ingredient_pula>(aBLTE);
                    
                    aiPulas.PULA = GetActive(GetEntities<active_ingredient_pula>(aBLTE), thisDate.Value.Date).ToList().Select(p => new SimplePULA()
                    {
                        entityID = p.id,
                        ShapeID = p.pula_shape_id,
                        isPublished = p.is_published,
                        Created = p.version.created_time_stamp,
                        Published = p.version.published_time_stamp,
                        Expired = p.version.expired_time_stamp
                    }).ToList();
                }//end using   

                ////activateLinks<SimplePULA>(aiPulas.PULA);

                return new OperationResult.OK { ResponseResource = aiPulas };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active PULAS for these filter choices for internal app
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFilteredSimplePULAs")]
        public OperationResult GetFilteredSimplePULAs([Optional] string date, [Optional] string activeIngredientID, [Optional] string productID, [Optional] string eventID)
        {
             PULAList aiList = new PULAList();
             IQueryable<active_ingredient_pula> query = null;
            try
            {
                DateTime? thisDate = null;
                if (date != null)
                {
                    thisDate = ValidDate(date); 
                }
                
                Int32 FilteredActiveIngredient = !string.IsNullOrEmpty(activeIngredientID) ? Convert.ToInt32(activeIngredientID) : -1;
                Int32 FilteredProduct = !string.IsNullOrEmpty(productID) ? Convert.ToInt32(productID) : -1;
                Int32 FilteredEvent = !string.IsNullOrEmpty(eventID) ? Convert.ToInt32(eventID) : -1;
                
                //changed validateDate to return today if null.. since need to return effective pulas
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    query = GetEntities<active_ingredient_pula>(aBLTE).Where(p => p.effective_date.Value <= thisDate.Value);

                    if (FilteredEvent > 0)
                    {
                        query = (from PULA in query
                                 where PULA.event_id == FilteredEvent
                                 select PULA);
                    }//end if

                    if (FilteredActiveIngredient > 0)
                    {
                       query = (from PULA in query
                            join pLimit in aBLTE.pula_limitations
                            on PULA.pula_id equals pLimit.pula_id
                            where pLimit.active_ingredient_id == FilteredActiveIngredient
                            select PULA);
                        
                    }//end if

                    if (FilteredProduct > 0)
                    {
                        query = (from PULA in query
                                 join pLimit in aBLTE.pula_limitations
                                 on PULA.pula_id equals pLimit.pula_id
                                 where pLimit.product_id == FilteredProduct
                                 select PULA);
                    }//end if


                    aiList.PULA = query.ToList().Select(p => new SimplePULA()
                        {
                            entityID = p.id,
                            ShapeID = p.pula_shape_id,
                            isPublished = p.is_published,
                            Created = p.version.created_time_stamp,
                            Published = p.version.published_time_stamp,
                            Effective = p.effective_date,
                            Expired = p.version.expired_time_stamp
                        }).ToList();

                }//end using   

                ////activateLinks<SimplePULA>(aiList.PULA);

                return new OperationResult.OK { ResponseResource = aiList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active PULAS for these filter choices for internal app
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEffectiveSimplePULAs")]
        public OperationResult GetEffectiveSimplePULAs([Optional] string date, [Optional] Int32 activeIngredientID, [Optional] Int32 productID)
        {
            PULAList aiList = new PULAList();
            IQueryable<active_ingredient_pula> query = null;
            try
            {
                DateTime? thisDate = null;
                if (date != "0")
                {
                    thisDate = ValidDate(date);
                }

                Int32 FilteredActiveIngredient = (activeIngredientID > 0) ? activeIngredientID : -1;
                Int32 FilteredProduct = (productID > 0) ? productID : -1;
                Boolean isManager = CanManage();
                //changed validateDate to return today if null.. since need to return effective pulas
                if (!thisDate.HasValue && !isManager)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    query = GetActive(GetEntities<active_ingredient_pula>(aBLTE), thisDate.Value.Date);

                    if (FilteredActiveIngredient > 0)
                    {
                        query = (from PULA in query
                                 join pLimit in aBLTE.pula_limitations
                                 on PULA.pula_id equals pLimit.pula_id
                                 where pLimit.active_ingredient_id == FilteredActiveIngredient
                                 select PULA);

                    }//end if

                    if (FilteredProduct > 0)
                    {
                        query = (from PULA in query
                                 join pLimit in aBLTE.pula_limitations
                                 on PULA.pula_id equals pLimit.pula_id
                                 where pLimit.product_id == FilteredProduct
                                 select PULA);
                    }//end if


                    aiList.PULA = query.AsEnumerable().Select(p => new SimplePULA()
                    {
                        entityID = p.id,
                        ShapeID = p.pula_shape_id,
                        isPublished = p.is_published,
                        Created = p.version.created_time_stamp,
                        Published = p.version.published_time_stamp,
                        Expired = p.version.expired_time_stamp,
                        Effective = p.effective_date
                    }).ToList<SimplePULA>();

                }//end using   

                ////activateLinks<SimplePULA>(aiList.PULA);

                return new OperationResult.OK { ResponseResource = aiList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active PULAS for the given shapeID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetShapePULAS")]
        public OperationResult GetShapePULAS(Int32 shapeId, [Optional]string date)
        {
            active_ingredient_pula aiPula;
            List<active_ingredient_pula> aiPulaList;
            IQueryable<active_ingredient_pula> query = null;
            try
            {
                Boolean mana = CanManage();
                //string role = Context.User.IsInRole();
                Boolean auth = IsAuthorized("REVIEW");
                Boolean isManager = (CanManage() || IsAuthorized("REVIEW"));
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !isManager) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    query = GetEntities<active_ingredient_pula>(aBLTE).Where(p => p.pula_shape_id == shapeId);

                    if (!isManager)
                        aiPula = GetActive(query, thisDate.Value.Date).FirstOrDefault();
                    else
                    { 
                        //remove expired
                        aiPulaList = query.Where(p =>
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate.Value < p.version.expired_time_stamp)).ToList();

                        if (aiPulaList.Where(p => p.version.published_time_stamp.HasValue)
                                .ToList().Count > 0) 
                            //published
                            aiPulaList = aiPulaList.Where(p => p.version.published_time_stamp.HasValue)
                                .ToList();

                        else if (aiPulaList.Where(p => p.version.reviewed_time_stamp.HasValue)
                                .ToList().Count > 0)
                            //Reviewed
                            aiPulaList = aiPulaList.Where(p => p.version.reviewed_time_stamp.HasValue)
                                .ToList();

                        //take the first
                        aiPula = aiPulaList.FirstOrDefault();

                    }//end if
                
                }//end using   

                //activateLinks<active_ingredient_pula>(aiPula);

                return new OperationResult.OK { ResponseResource = aiPula };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //returns PULAs for a given eventID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEventPULAs")]
        public OperationResult GetEventSimplePULAs(Int32 eventId)
        {
            PULAList aiPulas = new PULAList();
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<active_ingredient_pula> query = GetEntities<active_ingredient_pula>(aBLTE);
                    
                    aiPulas.PULA = aBLTE.active_ingredient_pula.AsEnumerable().Where(aip => aip.event_id == eventId)
                        .Select(aip => new SimplePULA
                        {
                            entityID = aip.id,
                            ShapeID = aip.pula_shape_id,
                            isPublished = aip.is_published,
                            Created = aip.version.created_time_stamp,
                            Published = aip.version.published_time_stamp,
                            Effective = aip.effective_date,
                            Expired = aip.version.expired_time_stamp
                    }).ToList<SimplePULA>();
                }//end using   

                ////activateLinks<SimplePULA>(aiPulas.PULA);

                return new OperationResult.OK { ResponseResource = aiPulas };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
   
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            active_ingredient_pula anAIPULA;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        //assign pulaID
                        anAIPULA = GetEntities<active_ingredient_pula>(aBLTE).SingleOrDefault(p => p.id == entityID);
                        
                    }//end using
               // }//end using

                    //activateLinks<active_ingredient_pula>(anAIPULA);

                return new OperationResult.OK { ResponseResource = anAIPULA };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion

        #region Status Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "UpdatePulaStatus")]
        public OperationResult UpdatePulaStatus(Int32 entityID, string status, [Optional] string date)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            active_ingredient_pula aPULA;
            try
            {
                StatusType statusType = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue || thisDate.Value.Month < DateTime.Now.Month)
                {
                    thisDate = DateTime.Now.Date;
                }
                if (entityID <= 0 || string.IsNullOrEmpty(status) || statusType == StatusType.Created)
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //version user
                        loggedInUser = LoggedInUser(aBLTE);

                        aPULA = aBLTE.active_ingredient_pula.FirstOrDefault(ai => ai.id == entityID);
                        
                        if (aPULA == null)
                        { return new OperationResult.BadRequest(); }

                        //assign version
                        aPULA.version_id = SetVersion(aBLTE, aPULA.version_id, loggedInUser.user_id, statusType, thisDate.Value.Date).version_id;
                        
                        if (statusType == StatusType.Published)
                        {
                            aPULA.is_published = 1;
                            ExpireOtherEntities(aBLTE, loggedInUser.user_id, aPULA, thisDate.Value.Date);
                            aBLTE.SaveChanges();
                           
                        }//end if

                        if (statusType == StatusType.Expired)
                        {
                            //set date to be 1st of current month for future query-ing
                            int nextMo = DateTime.Now.Month + 1;
                            DateTime nextMonth = Convert.ToDateTime(nextMo + "/01/" + DateTime.Now.Year);
                            ExpireEntities(aBLTE, LoggedInUser(aBLTE).user_id, aPULA.pula_id, nextMonth.Date);

                            aBLTE.SaveChanges();

                        }//end if

                        aBLTE.SaveChanges();
                        //activateLinks<active_ingredient_pula>(aPULA);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aPULA };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion

        #region POST Methods

         [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult POST(active_ingredient_pula anEntity)
        {            
            try
            {
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        if (!Exists(aBLTE, ref anEntity))
                        {
                            //assign pulaID
                            anEntity.pula_id = GetNextID(aBLTE);

                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, LoggedInUser(aBLTE).user_id, StatusType.Created, DateTime.Today).version_id;

                            aBLTE.active_ingredient_pula.Add(anEntity);
                            aBLTE.SaveChanges();
                        }//end if
                                            
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion

        #region PUT Methods
        /// 
        /// Force the user to provide authentication and authorization 
        /// 
        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.PUT)]
         public OperationResult Put(Int32 entityID, active_ingredient_pula anEntity)
        {
            active_ingredient_pula aPULA;

            //Return BadRequest if missing required fields
            if ((entityID <= 0||anEntity.pula_shape_id <= 0))
                 return new OperationResult.BadRequest();
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        aPULA = aBLTE.active_ingredient_pula.FirstOrDefault(aip => aip.id == entityID);
                        if (aPULA == null)
                        { return new OperationResult.BadRequest(); }

                        //NOTE: ShapeID can not be changed
                        if (aPULA.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.pula_id = GetNextID(aBLTE);

                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;

                            anEntity.is_published = 1;
                            aBLTE.active_ingredient_pula.Add(anEntity);

                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);

                            aPULA = anEntity;
                        }
                        else
                        {
                            //Update fields
                            aPULA.pula_id = anEntity.pula_id;
                            aPULA.pula_shape_id = anEntity.pula_shape_id;
                            aPULA.other_justification = anEntity.other_justification;
                            aPULA.base_data = anEntity.base_data;
                            aPULA.base_data_modifiers = anEntity.base_data_modifiers;
                            aPULA.additional_information = anEntity.additional_information;
                            aPULA.interim_proposed_decision = anEntity.interim_proposed_decision;
                            aPULA.focus_meeting = anEntity.focus_meeting;
                            aPULA.biological_opinion_regreview = anEntity.biological_opinion_regreview;
                            aPULA.biological_opinion_lit = anEntity.biological_opinion_lit;
                            aPULA.proposed_decision = anEntity.proposed_decision;
                            aPULA.version_id = anEntity.version_id;
                            aPULA.id = anEntity.id;
                            aPULA.is_published = anEntity.is_published;
                            aPULA.effective_date = anEntity.effective_date;
                            aPULA.event_id = anEntity.event_id;
                            aPULA.comments = anEntity.comments;
                            aPULA.sec3_newchem = anEntity.sec3_newchem;
                            aPULA.sec3_newuse = anEntity.sec3_newuse;
                            aPULA.sec24 = anEntity.sec24;
                            aPULA.final_decision = anEntity.final_decision;
                            aPULA.sec18 = anEntity.sec18;
                            aPULA.interim_decision = anEntity.interim_decision;

                        }// end if

                        aBLTE.SaveChanges();

                        //activateLinks<active_ingredient_pula>(aPULA);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aPULA };

            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.PUT

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.PUT, ForUriName = "AddComments")]
        public OperationResult PUT(Int32 pulaID, active_ingredient_pula anAIPULA)
        {
            contributorpulaview aContributorPULA;
            //Return BadRequest if missing required fields
            if (pulaID <= 0)
                return new OperationResult.BadRequest();
            try
            {
                //Get basic authentication password

                using (bltEntities aBLTE = GetRDS())
                {
                    aContributorPULA = getTable<contributorpulaview>(new Object[1] { null }).FirstOrDefault(aip => aip.id == pulaID);
                    //aContributorPULA = aBLTE.CONTRIBUTORPULAVIEWs.FirstOrDefault(aip => aip.id == pulaID);

                    if (aContributorPULA == null)
                    { return new OperationResult.BadRequest(); }

                    if (aContributorPULA.comments != null)
                    { aContributorPULA.comments += anAIPULA.comments; }
                    else
                    { aContributorPULA.comments = anAIPULA.comments; }

                    active_ingredient_pula aPULA = aBLTE.active_ingredient_pula.FirstOrDefault(aip => aip.id == aContributorPULA.id);
                    aPULA.comments = aContributorPULA.comments;
                    aBLTE.SaveChanges();                    

                }//end using

                return new OperationResult.OK { };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.PUT
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

                    active_ingredient_pula ObjectToBeDelete = aBLTE.active_ingredient_pula.FirstOrDefault(aip => aip.id == entityID);
                    
                    if (ObjectToBeDelete == null)
                    { return new OperationResult.BadRequest(); }

                    //NOTE: ShapeID can not be changed
                    if (ObjectToBeDelete.version.published_time_stamp.HasValue)
                    {
                        //set the date to be first of following month 
                        int nextMo = DateTime.Now.Month + 1;
                        DateTime nextMonth = Convert.ToDateTime(nextMo + "/01/" + DateTime.Now.Year);
                        ObjectToBeDelete.version_id = SetVersion(ObjectToBeDelete.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, nextMonth.Date).version_id;
                    }
                    else
                    {
                        aBLTE.active_ingredient_pula.Remove(ObjectToBeDelete);
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
        private bool Exists(bltEntities aBLTE, ref active_ingredient_pula anEntity)
        {
            active_ingredient_pula existingEntity;
            active_ingredient_pula thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.active_ingredient_pula.FirstOrDefault(mt => mt.pula_shape_id == thisEntity.pula_shape_id &&
                                                                             mt.version.created_time_stamp.HasValue & mt.version.published_time_stamp.HasValue &
                                                                             !mt.version.expired_time_stamp.HasValue);

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
            if (aBLTE.active_ingredient_pula.Count() > 0)
            {
                nextID = aBLTE.active_ingredient_pula.OrderByDescending(p => p.pula_id).First().pula_id + 1;
            }
            return nextID;            
        }

        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, active_ingredient_pula pula, DateTime dt)
        {
            //get all published AIPULAS, should only be 1
            List<active_ingredient_pula> aiPulaList = aBLTE.active_ingredient_pula.Where(p => p.pula_id == pula.pula_id).ToList();
           
            if (aiPulaList == null) return;

            foreach (var p in aiPulaList)
            {
                if (!p.Equals(pula))
                {
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt);
                }//end if
            }//next
        }//end ExpireOtherEntities

        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published AIPULAS, should only be 1
            List<active_ingredient_pula> aiPulaList = aBLTE.active_ingredient_pula.Where(p => p.pula_id == Id).ToList();

            if (aiPulaList == null) return;

            foreach (var p in aiPulaList)
            {
                if (!p.version.expired_time_stamp.HasValue)
                {
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt);
                }//end if
            }//next
        }

        public IQueryable<T> getTable<T>(object[] args) where T : class,new()
        {
            try
            {
                string sql = string.Empty;
                //if (args[0] != null)
                //{
                //    if (args[0].ToString() == "baro_view" || args[0].ToString() == "met_view" || args[0].ToString() == "rdg_view" || args[0].ToString() == "stormtide_view" || args[0].ToString() == "waveheight_view")
                //        sql = String.Format(getSQLStatement(args[0].ToString()));
                //}
                //else
                sql = String.Format(getSQLStatement(typeof(T).Name), args);
                var context = new bltEntities(string.Format(connectionString, "bltadmin", new EasySecureString("1MhTGVxs").decryptString()));
                context.Configuration.ProxyCreationEnabled = false;
                return context.Database.SqlQuery<T>(sql).AsQueryable();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string getSQLStatement(string type) 
        {
            string sql = string.Empty;
            switch (type)
            {
                case "contributorpulaview":                    
                    return @"SELECT * FROM contributorpulaview;";
                case "met_view":
                    return @"SELECT * FROM meteorological_view;";
                case "rdg_view":
                    return @"SELECT * FROM rapid_deployment_view;";
                case "stormtide_view":
                    return @"SELECT * FROM storm_tide_view;";
                case "waveheight_view":
                    return @"SELECT * FROM wave_height_view;";  
                default:
                    throw new Exception("No sql for table " + type);
            }//end switch;
        
        }
        #endregion
        
    }//end CollectionMethodsHandler

}//end namespace