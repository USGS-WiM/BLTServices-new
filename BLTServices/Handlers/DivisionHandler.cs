﻿//------------------------------------------------------------------------------
//----- DivisionHandler --------------------------------------------------------
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
    public class DivisionHandler: HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "Division"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        // returns all Divisions
     //   [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<division> divList;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    divList = GetEntities<division>(aBLTE).OrderBy(a=>a.division_name).ToList();
                }//end using                

                //activateLinks<division>(divList);

                return new OperationResult.OK { ResponseResource = divList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

      //  [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 divisionID)
        {
            division anDiv;
            try
            {
                if (divisionID <= 0)
                { return new OperationResult.BadRequest(); }
                using (bltEntities aBLTE = GetRDS())
                {
                    anDiv = GetEntities<division>(aBLTE).SingleOrDefault(d=>d.division_id==divisionID);

                }//end using
                //activateLinks<division>(anDiv);

                return new OperationResult.OK { ResponseResource = anDiv };
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
        public OperationResult POST(division anEntity)
        {
            try
            {
                //Return BadRequest if missing required fields
                if (string.IsNullOrEmpty(anEntity.division_name))
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //Prior to running stored procedure, check if username exists
                        if (!Exists(aBLTE, ref anEntity))
                        {
                            aBLTE.divisions.Add(anEntity);
                            aBLTE.SaveChanges();
                        }

                        //activateLinks<division>(anEntity);

                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        #endregion
        #region PUT Methods
        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult PUT(Int32 divisionID, division instance)
        {
            try
            {
                if (divisionID <= 0)
                { return new OperationResult.BadRequest(); }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        division ObjectToBeUpdated = aBLTE.divisions.SingleOrDefault(o => o.division_id == divisionID);

                        if (ObjectToBeUpdated == null) { return new OperationResult.BadRequest(); }

                        //Name
                        ObjectToBeUpdated.division_name = (string.IsNullOrEmpty(instance.division_name) ?
                            ObjectToBeUpdated.division_name : instance.division_name);

                        aBLTE.SaveChanges();
                    }// end using
                }// end using

                //activateLinks<division>(instance);

                return new OperationResult.OK { ResponseResource = instance };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
        #endregion
        //#region DELETE Methods
        //[RequiresRole(AdminRole)]
        //[HttpOperation(HttpMethod.DELETE)]
        //public OperationResult Delete(Int32 divisionID)
        //{
        //    DIVISION ObjectToBeDeleted = null;

        //    //Return BadRequest if missing required fields
        //    if (divisionID <= 0)
        //    { return new OperationResult.BadRequest(); }


        //    //Get basic authentication password
        //    using (EasySecureString securedPassword = GetSecuredPassword())
        //    {
        //        using (BLTRDSEntities aBLTRDS = GetRDS(securedPassword))
        //        {

        //            // create user profile using db stored proceedure
        //            try
        //            {
        //                //fetch the object to be updated (assuming that it exists)
        //                ObjectToBeDeleted = aBLTRDS.DIVISIONS.SingleOrDefault(
        //                                        m => m.DIVISION_ID == divisionID);

        //                //delete it
        //                aBLTRDS.DIVISIONS.DeleteObject(ObjectToBeDeleted);
        //                aBLTRDS.SaveChanges();
        //                //Return object to verify persisitance

        //                return new OperationResult.OK { };

        //            }
        //            catch (Exception)
        //            {
        //                //TODO: relay failure type message 
        //                // EX. if profile failed to be removed
        //                return new OperationResult.BadRequest();
        //            }

        //        }// end using
        //    } //end using
        //}//end HTTP.DELETE
        //#endregion
        #endregion

        #region Helper Methods
        private Boolean Exists(bltEntities aBLTE, ref division aDivision)
        {
            division existingOrg;
            division thisOrg = aDivision;
            //check if it exists
            try
            {
                existingOrg = aBLTE.divisions.FirstOrDefault(o => o.division_name == thisOrg.division_name);

                if (existingOrg == null)
                    return false;

                //if exists then update ref contact
                aDivision = existingOrg;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            throw new NotImplementedException();
        }
    }// end class divisionHandler
}//end namespace