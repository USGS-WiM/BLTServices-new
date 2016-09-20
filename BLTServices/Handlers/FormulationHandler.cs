//------------------------------------------------------------------------------
//----- Formulation Handler -----------------------------------------------------------
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
// 07.22.13 - TR - created from ModifiersHandler

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
    public class FormulationHandler : HandlerBase
    {

        #region Properties
        public override string entityName
        {
            get { return "formulation"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all FORMULATION
      //  [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<formulation> aFormulation;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    aFormulation = GetEntities<formulation>(aBLTE).ToList();
                }//end using

                //activateLinks<formulation>(aFormulation);

                return new OperationResult.OK { ResponseResource = aFormulation };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all PULAs
        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedFormulations")]
        public OperationResult GetVersionedFormulations(string status, string date)
        {
            IQueryable<formulation> formulaQuery;
            List<formulation> formulaList;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    formulaQuery = GetEntities<formulation>(aBLTE);
                    switch (statustype)
                    {
                        case (StatusType.Published):
                            formulaQuery.Where(ai => ai.version.published_time_stamp != null);
                            break;
                        case (StatusType.Reviewed):
                            formulaQuery.Where(ai => ai.version.reviewed_time_stamp != null &&
                                            ai.version.published_time_stamp == null);
                            break;
                        //created
                        default:
                            formulaQuery.Where(ai => ai.version.reviewed_time_stamp == null &&
                                            ai.version.published_time_stamp == null);
                            break;
                    }//end switch

                    formulaQuery.Where(ai => !ai.version.expired_time_stamp.HasValue ||
                                        ai.version.expired_time_stamp < thisDate.Value.Date);

                    formulaList = formulaQuery.ToList();

                }//end using
                
                //activateLinks<formulation>(formulaList);

                return new OperationResult.OK { ResponseResource = formulaList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns all active FORMULATION
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<formulation> aFormulationList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    aFormulationList = GetActive(GetEntities<formulation>(aBLTE), thisDate.Value.Date).OrderBy(a=>a.form).ToList();
                }//end using

                //activateLinks<formulation>(aFormulationList);

                return new OperationResult.OK { ResponseResource = aFormulationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

   //     [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName= "GetFormulations")]
        public OperationResult Get(Int32 formulationID, [Optional] string date)
        {
            try
            {
                List<formulation> formulationList;
                DateTime? thisDate = ValidDate(date);

                if (formulationID < 0)
                { return new OperationResult.BadRequest(); }

                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<formulation> query;
                    query = GetEntities<formulation>(aBLTE).Where(f => f.formulation_id == formulationID);

                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    formulationList = query.ToList();
                }//end using
                
                //activateLinks<formulation>(formulationList);

                return new OperationResult.OK { ResponseResource = formulationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULALimitationsFormulation")]
        public OperationResult GetPULALimitationsFormulation(Int32 pulaLimitationID, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<formulation> formulationList;
                using (bltEntities aBLTE = GetRDS())
                {

                    IQueryable<formulation> query1 =
                            (from PULALimit in GetActive(aBLTE.pula_limitations.Where(p => p.pula_limitation_id == pulaLimitationID), thisDate.Value.Date)
                             join f in aBLTE.formulations
                             on PULALimit.formulation_id equals f.formulation_id
                             select f).Distinct();

                    formulationList = GetActive(query1, thisDate.Value.Date).ToList();

                    //activateLinks<formulation>(formulationList);

                }//end using

                return new OperationResult.OK { ResponseResource = formulationList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //---------------------Returns individual objects---------------------
   //     [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            formulation aFormulation;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    aFormulation = GetEntities<formulation>(aBLTE).SingleOrDefault(c => c.id == entityID);

                }//end using

                //activateLinks<formulation>(aFormulation);

                return new OperationResult.OK { ResponseResource = aFormulation };
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
        public OperationResult POST(formulation anEntity)
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

                            anEntity.formulation_id = GetNextID(aBLTE);

                            aBLTE.formulations.Add(anEntity);

                            aBLTE.SaveChanges();
                        }
                        else
                        {//it exists, check if expired
                            if (anEntity.version.expired_time_stamp.HasValue)
                            {
                                formulation newF = new formulation();
                                newF.form = anEntity.form;
                                newF.version_id = SetVersion(aBLTE, newF.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                                newF.formulation_id = anEntity.formulation_id;
                                //anEntity.ID = 0;
                                aBLTE.formulations.Add(newF);
                                aBLTE.SaveChanges();
                            }//end if
                        }//end if//end if

                        //activateLinks<formulation>(anEntity);
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
        public OperationResult Put(Int32 entityID, formulation anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            formulation aFormulation;
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

                        aFormulation = aBLTE.formulations.FirstOrDefault(c => c.id == entityID);
                        if (aFormulation == null)
                        { return new OperationResult.BadRequest(); }

                        if (aFormulation.version.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.formulation_id = aFormulation.formulation_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.formulations.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            aFormulation = anEntity;
                        }
                        else
                        {
                            aFormulation.form = anEntity.form;
                        }//end if

                        aBLTE.SaveChanges();

                        //activateLinks<formulation>(aFormulation);
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = aFormulation };
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
                    formulation ObjectToBeDelete = aBLTE.formulations.FirstOrDefault(f => f.id == entityID);

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
                        aBLTE.formulations.Remove(ObjectToBeDelete);
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
        private bool Exists(bltEntities aBLTE, ref formulation anEntity)
        {
            formulation existingEntity;
            formulation thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.formulations.FirstOrDefault(f => string.Equals(f.form.ToUpper(), thisEntity.form.ToUpper()));


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
            if (aBLTE.formulations.Count() > 0)
                nextID = aBLTE.formulations.OrderByDescending(p => p.formulation_id).First().formulation_id + 1;

            return nextID;
        }
        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, formulation formulation, DateTime dt)
        {
            //get all published, should only be 1
            List<formulation> aiList = aBLTE.formulations.Where(p => p.formulation_id == formulation.formulation_id &&
                                                                                p.version.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if (!p.Equals(formulation))
                    p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities
        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<formulation> formulationList = aBLTE.formulations.Where(p => p.formulation_id == Id &&
                                                               p.version.published_time_stamp <= dt.Date).ToList();
            if (formulationList == null) return;

            foreach (var p in formulationList)
            {
                p.version = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }
        #endregion


    }//end class FormulationHandler

}//end namespace