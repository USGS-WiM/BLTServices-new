//------------------------------------------------------------------------------
//----- AIClassHandler ---------------------------------------------------------
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
    public class AIClassHandler: HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "ai_class"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all AIClass
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<ai_class> aiClassList;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        aiClassList = GetEntities<ai_class>(aBLTE).ToList();
                    }//end using
               // }//end using
                    activateLinks<ai_class>(aiClassList);

                return new OperationResult.OK { ResponseResource = aiClassList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns versioned AI Classes
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedAIClasss")]
        public OperationResult GetVersionedAIClasses(string status, string date)
        {
            ObjectQuery<ai_class> aiClassQuery;
            List<ai_class> aiClasses;
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
                        aiClassQuery = GetEntities<ai_class>(aBLTE);
                        switch (statustype)
                        {
                            case (StatusType.Published):
                                aiClassQuery.Where(ai => ai.version.published_time_stamp != null);
                                break;
                            case (StatusType.Reviewed):
                                aiClassQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                                ai.version.published_time_stamp == null);
                                break;
                            //created
                            default:
                                aiClassQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                                ai.version.published_time_stamp == null);
                                break;
                        }//end switch

                        aiClassQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                            ai.version.expired_time_stamp < thisDate.Value.Date);

                        aiClasses = aiClassQuery.ToList();

                    }//end using
                //}//end using

                activateLinks<ai_class>(aiClasses);

                return new OperationResult.OK { ResponseResource = aiClasses };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
        
        // returns all active AIClass
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<ai_class> aiClassList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now; 

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    aiClassList = GetActive(GetEntities<ai_class>(aBLTE), thisDate.Value.Date).OrderBy(a=>a.ai_class_name).ToList();
                    
                }//end using

                activateLinks<ai_class>(aiClassList);

                return new OperationResult.OK { ResponseResource = aiClassList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

        // returns all AIClass (active if date is supplied)
        [HttpOperation(HttpMethod.GET, ForUriName="GetAIClasses")]
        public OperationResult GetAIClasses(Int32 aiClassID, [Optional] string date)
        {
            try
            {
                List<ai_class> aiClassList;
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;
                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;
 
                if (aiClassID < 0)
                { return new OperationResult.BadRequest(); }

                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        IQueryable<ai_class> query;
                        query = GetEntities<ai_class>(aBLTE).Where(aic => aic.ai_class_id == aiClassID);

                        if (thisDate.HasValue)
                            query = GetActive(query, thisDate.Value.Date);

                        //process
                        aiClassList = query.ToList();
                    }//end using
              //  }//end using

                activateLinks<ai_class>(aiClassList);

                return new OperationResult.OK { ResponseResource = aiClassList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
        
        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetActiveIngredientClasses")]
        public OperationResult GetActiveIngredientClasses(Int32 activeIngredientId, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<ai_class> aiClassList;
                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<ai_class> query1 =
                        (from AI_AIClass in GetActive(aBLTE.active_ingredient_ai_class.Where(ai => ai.active_ingredient_id == activeIngredientId), thisDate.Value.Date)
                         join AIclass in aBLTE.ai_class
                         on AI_AIClass.ai_class_id equals AIclass.ai_class_id
                         select AIclass).Distinct();

                    aiClassList = GetActive(query1, thisDate.Value.Date).ToList();

                    activateLinks<ai_class>(aiClassList);

                }//end using

                return new OperationResult.OK { ResponseResource = aiClassList };
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
            ai_class anAIClass;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        anAIClass = GetEntities<ai_class>(aBLTE).SingleOrDefault(ai => ai.id == entityID);

                    }//end using
             //   }//end using
            
                activateLinks<ai_class>(anAIClass);

                return new OperationResult.OK { ResponseResource = anAIClass };
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
        public OperationResult POST(ai_class anEntity)
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

                            anEntity.ai_class_id = GetNextID(aBLTE);

                            aBLTE.ai_class.Add(anEntity);

                            aBLTE.SaveChanges();
                        }
                        else
                        {
                            //it exists, check if expired
                            if (anEntity.version.expired_time_stamp.HasValue)
                            {
                                ai_class newAIClass = new ai_class();
                                newAIClass.ai_class_name = anEntity.ai_class_name;
                                newAIClass.version_id =  SetVersion(aBLTE, newAIClass.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                                newAIClass.ai_class_id = anEntity.ai_class_id;
                                //anEntity.ID = 0;
                                aBLTE.ai_class.Add(newAIClass);
                                aBLTE.SaveChanges();
                            }
                        }
                        activateLinks<ai_class>(anEntity);
                    }//end using
                }//end using

                
                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddAIClassToAI")]
        public OperationResult AddAIClass(Int32 entityID, active_ingredient AI)
        {
            ai_class anAIClass = null;
            //ACTIVE_INGREDIENT aAIngr = null;
            active_ingredient_ai_class newAIC_AI = null;
            try
            {
                //Return BadRequest if missing required fields
                if (entityID <= 0 || AI.active_ingredient_id <= 0)
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        decimal ai_ID = AI.active_ingredient_id;
                        anAIClass = GetEntities<ai_class>(aBLTE).SingleOrDefault(c => c.id == entityID);
                        if (anAIClass == null || AI == null)
                        { return new OperationResult.BadRequest(); }

                        newAIC_AI = new active_ingredient_ai_class();
                        newAIC_AI.active_ingredient_id = AI.active_ingredient_id;
                        newAIC_AI.ai_class_id = anAIClass.ai_class_id;

                        //check if this relationship already exists
                        if (!Exists1(aBLTE, ref newAIC_AI))
                        {
                            newAIC_AI.active_ingredient_ai_class_id = GetNextAIcAI_ID(aBLTE);

                            //create version
                            newAIC_AI.version_id = SetVersion(aBLTE, newAIC_AI.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;

                            aBLTE.active_ingredient_ai_class.Add(newAIC_AI);
                            aBLTE.SaveChanges();

                        }//end if

                    }//end using
                }//end using

                return new OperationResult.OK();
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        #endregion
        
        #region PUT/EDIT Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityID, ai_class anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            ai_class anAIClass;
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

                        anAIClass = aBLTE.ai_class.FirstOrDefault(c => c.id == entityID);
                        if (anAIClass == null)
                        { return new OperationResult.BadRequest(); }

                        if (anAIClass.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.ai_class_id = anAIClass.ai_class_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.ai_class.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            anAIClass = anEntity;
                        }
                        else
                        {
                            anAIClass.ai_class_name = anEntity.ai_class_name;
                        }//end if

                        aBLTE.SaveChanges();

                        activateLinks<ai_class>(anAIClass);
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
                    ai_class ObjectToBeDelete = aBLTE.ai_class.FirstOrDefault(am => am.id == entityID);

                    if (ObjectToBeDelete == null)
                    { return new OperationResult.BadRequest(); }

                    //NOTE: ShapeID can not be changed
                    if (ObjectToBeDelete.version.published_time_stamp.HasValue)
                    {
                        ObjectToBeDelete.version_id = SetVersion(ObjectToBeDelete.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, DateTime.Now.Date).version_id;
                    }
                    else
                    {
                        aBLTE.ai_class.Remove(ObjectToBeDelete);
                    }//end if

                    aBLTE.SaveChanges();
                }//end using
            }//end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }//end HttpMethod.DELETE

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveAIClassFromAI")]
        public OperationResult RemoveAIClassFromAI(Int32 entityID, Int32 aiEntityID)
        {
            //No deleting of tables are allowed. An delete call will expire the entity
            user_ loggedInUser;
            ai_class anAIClass;
            active_ingredient anAI;
            active_ingredient_ai_class anAIC;

            try
            {
                DateTime? thisDate = DateTime.Now.Date;

                if (entityID <= 0 || aiEntityID <= 0)
                { return new OperationResult.BadRequest { }; }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        loggedInUser = LoggedInUser(aBLTE);
                        anAIClass = aBLTE.ai_class.FirstOrDefault(c => c.id == entityID);
                        anAI = aBLTE.active_ingredient.FirstOrDefault(c => c.id == aiEntityID);

                        anAIC = aBLTE.active_ingredient_ai_class.FirstOrDefault(c => c.ai_class_id == anAIClass.ai_class_id && c.active_ingredient_id == anAI.active_ingredient_id);

                        if (anAIC == null)
                        { return new OperationResult.BadRequest(); }
                        //NOTE: ShapeID can not be changed
                        if (anAIC.version.published_time_stamp.HasValue)
                        {
                            //expire the Product-AI relationship
                            anAIC.version = SetVersion(aBLTE, anAIC.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, thisDate.Value.Date);
                        }
                        else
                        {
                            aBLTE.active_ingredient_ai_class.Remove(anAIC);
                        }//end if

                        aBLTE.SaveChanges();

                    }//end using
                }//end using

                return new OperationResult.OK();
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET
        #endregion

        #endregion
        #region Helper Methods
        private bool Exists(bltEntities aBLTE, ref ai_class anEntity)
        {
            ai_class existingEntity;
            ai_class thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.ai_class.FirstOrDefault(mt => string.Equals(mt.ai_class_name.ToUpper(), thisEntity.ai_class_name.ToUpper()));
              

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

        private bool Exists1(bltEntities aBLTE, ref active_ingredient_ai_class anEntity)
        {
            active_ingredient_ai_class existingEntity;
            active_ingredient_ai_class thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.active_ingredient_ai_class.FirstOrDefault(pai => pai.active_ingredient_id == thisEntity.active_ingredient_id && pai.ai_class_id == thisEntity.ai_class_id);


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

        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, ai_class aiClass, DateTime dt)
        {
            //get all published, should only be 1
            List<ai_class> aiList = aBLTE.ai_class.Where(p => p.ai_class_id == aiClass.ai_class_id &&
                                                              p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if (!p.Equals(aiClass))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities
        private Int32 GetNextID(bltEntities aBLTE)
        {
            //create pulaID
            Int32 nextID = 1;
            if (aBLTE.ai_class.Count() > 0)
                nextID = aBLTE.ai_class.OrderByDescending(p => p.ai_class_id).First().ai_class_id + 1;

            return nextID;
        }
        private Int32 GetNextAIcAI_ID(bltEntities aBLTE)
        {
            //create pulaID
            Int32 nextID = 1;
            if (aBLTE.active_ingredient_ai_class.Count() > 0)
                nextID = aBLTE.active_ingredient_ai_class.OrderByDescending(p => p.active_ingredient_ai_class_id).First().active_ingredient_ai_class_id + 1;

            return nextID;
        }


        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<ai_class> aiList = aBLTE.ai_class.Where(p => p.ai_class_id == Id &&
                                                         p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }
        #endregion

    }//end class AIClassHandler
}//end namespace