//------------------------------------------------------------------------------
//----- ActiveIngredientHander -------------------------------------------------
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
// 05.14.13 - JKN- Redesigned handler and updated methods
// 11.09.12 - TR - Changed class name from SiteHandler to PULALimitationsHandler and removed ForUriName "Sites" references
// 10.19.12 - JB - Created from STN
#endregion

using BLTServices.Authentication;

using OpenRasta.Web;
using OpenRasta.Security;
using OpenRasta.Diagnostics;
using BLTDB;
using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Reflection;
using System.Web;
using System.Runtime.InteropServices;


namespace BLTServices.Handlers
{
    public class ActiveIngredientHandler : HandlerBase
    {

        #region Properties
        public override string entityName
        {
            get { return "active_ingredient"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all PULAs
        //[WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<active_ingredient> aiList;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        aiList = GetEntities<active_ingredient>(aBLTE).ToList();
                        aiList = aiList.OrderBy(a => a.ingredient_name).ToList();
                    }//end using
                //}//end using

                //activateLinks<active_ingredient>(aiList);

                return new OperationResult.OK { ResponseResource = aiList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all PULAs
        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedActiveIngredients")]
        public OperationResult GetVersionedActiveIngredients(string status, string date)
        {
            IQueryable<active_ingredient> aiQuery;
            List<active_ingredient> activeIngredient;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now;

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        aiQuery = GetEntities<active_ingredient>(aBLTE);
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
                                            ai.version.expired_time_stamp < thisDate.Value);

                        activeIngredient = aiQuery.ToList();
                        activeIngredient = activeIngredient.OrderBy(a => a.ingredient_name).ToList();
                    }//end using
                }//end using

                //activateLinks<active_ingredient>(activeIngredient);

                return new OperationResult.OK { ResponseResource = activeIngredient };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET


        // returns all active Ingredients
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<active_ingredient> aiList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now; 
                
                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value, DateTime.Now) < 0 && !CanManage()) thisDate = DateTime.Now;

                using (bltEntities aBLTE = GetRDS())
                {
                    aiList = GetActive(GetEntities<active_ingredient>(aBLTE), thisDate.Value).OrderBy(a => a.ingredient_name).ToList();
                }//end using
                //activateLinks<active_ingredient>(aiList);

                return new OperationResult.OK { ResponseResource = aiList };
            }
            catch (Exception ex)
            {
               return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

       // [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAI")]
        public OperationResult Get(Int32 activeIngredientID, [Optional] string date)
        {
            try
            {
                List<active_ingredient> aiList;
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value, DateTime.Now) < 0 && !CanManage()) thisDate = DateTime.Now;

                if (activeIngredientID < 0)
                { return new OperationResult.BadRequest(); }

                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        IQueryable<active_ingredient> query;
                        query = GetEntities<active_ingredient>(aBLTE).Where(ai => ai.active_ingredient_id == activeIngredientID);
        
                        if (thisDate.HasValue)
                            query = GetActive(query, thisDate.Value);

                        //process
                        aiList = query.ToList();
                    }//end using
                //}//end using

                    //activateLinks<active_ingredient>(aiList);

                return new OperationResult.OK { ResponseResource = aiList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULAactiveIngredients")]
        public OperationResult GetPULAactiveIngredients(Int32 pulaId, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now;
                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value, DateTime.Now) < 0 && !CanManage()) thisDate = DateTime.Now;

                List<active_ingredient> aiList;
                using (bltEntities aBLTE = GetRDS())
                {
                    //active PULA_LIMITATION.. get all PULALimitation with pula id = and activedate = .. 
                    //then join ai (get all of them activeingredient table)..on matching ids, select distinct
                    IQueryable<active_ingredient> query1 =
                            (from PULALimit in GetActive(aBLTE.pula_limitations.Where(p => p.pula_id == pulaId), thisDate.Value)
                             join AI in aBLTE.active_ingredient
                            on PULALimit.active_ingredient_id equals AI.active_ingredient_id
                            select AI).Distinct();

                    aiList = GetActive(query1,thisDate.Value).ToList();

                    //activateLinks<active_ingredient>(aiList);

                }//end using

                return new OperationResult.OK { ResponseResource = aiList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
    

        //---------------------Returns individual objects---------------------
    //    [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET, ForUriName="GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            active_ingredient anAI;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        anAI = GetEntities<active_ingredient>(aBLTE).SingleOrDefault(ai => ai.id == entityID);

                    }//end using
                //}//end using

                    //activateLinks<active_ingredient>(anAI);

                return new OperationResult.OK { ResponseResource = anAI };
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
        public OperationResult POST(active_ingredient anEntity)
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

                            anEntity.active_ingredient_id = GetNextID(aBLTE);

                            aBLTE.active_ingredient.Add(anEntity);

                            aBLTE.SaveChanges();
                        }
                        else
                        {
                            //it exists, check if expired
                            if (anEntity.version.expired_time_stamp.HasValue)
                            {
                                active_ingredient newAI = new active_ingredient();
                                newAI.ingredient_name = anEntity.ingredient_name;
                                newAI.pc_code = anEntity.pc_code;
                                newAI.cas_number = anEntity.cas_number;
                                newAI.version_id = SetVersion(aBLTE, newAI.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                                newAI.active_ingredient_id = anEntity.active_ingredient_id;
                                //anEntity.ID = 0;
                                aBLTE.active_ingredient.Add(newAI);
                                aBLTE.SaveChanges();
                            }
                        }//end if

                        //activateLinks<active_ingredient>(anEntity);                       
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
        public OperationResult Put(Int32 entityID, active_ingredient anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            active_ingredient aActiveIngredient;
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

                        aActiveIngredient = aBLTE.active_ingredient.FirstOrDefault(ai => ai.id == entityID);

                        if (aActiveIngredient == null)
                        { return new OperationResult.BadRequest(); }

                        //all active ingredients are published when created... doesn't mean the PULA is published

                        if (aActiveIngredient.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //use same active_ingredient_id
                            anEntity.active_ingredient_id = aActiveIngredient.active_ingredient_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.active_ingredient.Add(anEntity);//.AddObject(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            aActiveIngredient = anEntity;
                        }
                        else
                        {
                            aActiveIngredient.ingredient_name = anEntity.ingredient_name;
                        }//end if

                        aBLTE.SaveChanges();

                        //activateLinks<active_ingredient>(anEntity);
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

                    active_ingredient ObjectToBeDelete = aBLTE.active_ingredient.FirstOrDefault(am => am.id == entityID);

                    if (ObjectToBeDelete == null)
                    { return new OperationResult.BadRequest(); }

                    //NOTE: ShapeID can not be changed
                    if (ObjectToBeDelete.version.published_time_stamp.HasValue)
                    {
                        //set the date to be first of following month 
                        int nextMo = DateTime.Now.Month + 1;
                        DateTime nextMonth = Convert.ToDateTime(nextMo + "/01/" + DateTime.Now.Year);
                        ObjectToBeDelete.version_id = SetVersion(ObjectToBeDelete.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, nextMonth).version_id;
                    }
                    else
                    {
                        aBLTE.active_ingredient.Remove(ObjectToBeDelete);
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
        private bool Exists(bltEntities aBLTE, ref active_ingredient anEntity)
        {
            active_ingredient existingEntity;
            active_ingredient thisEntity = anEntity;
            //check if it exists
            try
            {

                existingEntity = aBLTE.active_ingredient.FirstOrDefault(mt => string.Equals(mt.ingredient_name.ToUpper(), thisEntity.ingredient_name.ToUpper()) &&
                                                                            (string.Equals(mt.pc_code.ToUpper(),thisEntity.pc_code.ToUpper())) &&
                                                                            (string.Equals(mt.cas_number.ToUpper(), thisEntity.cas_number.ToUpper()) || string.IsNullOrEmpty(thisEntity.cas_number)));
               

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
                if (aBLTE.active_ingredient.Count() > 0)
                    nextID = aBLTE.active_ingredient.OrderByDescending(p => p.active_ingredient_id).First().active_ingredient_id + 1;
                
                return nextID;
        }

        private decimal GetNextprID(bltEntities aBLTE)
        {
            //create pulaID
            Decimal nextID = 1;
            if (aBLTE.product_active_ingredient.Count() > 0)
                nextID = aBLTE.product_active_ingredient.OrderByDescending(p => p.product_active_ingredient_id).First().product_active_ingredient_id + 1;

            return nextID;
        }

        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, active_ingredient ai, DateTime dt)
        {
            //get all published, should only be 1
            List<active_ingredient> aiList = aBLTE.active_ingredient.Where(p => p.active_ingredient_id == ai.active_ingredient_id && 
                                                                                p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if(!p.Equals(ai))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt);
            }//next
        }//end ExpireOtherEntities
        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<active_ingredient> aiList = aBLTE.active_ingredient.Where(p => p.active_ingredient_id == Id &&
                                                                                p.version.published_time_stamp <= dt).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt);
            }//next
        }//end ExpireEntities
        
       #endregion

    }//end class ActiveIngredientHandler

}//end namespace