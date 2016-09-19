//------------------------------------------------------------------------------
//----- EventHandler ---------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
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
// 09.18.13 - TR - Created
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
    public class EventsHandler: HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "event"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        // returns all EVENTS
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<@event> eventList;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    eventList  = GetEntities<@event>(aBLTE).OrderBy(a=>a.name).ToList();
                }//end using
                
                //activateLinks<@event>(eventList);

                return new OperationResult.OK { ResponseResource = eventList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //[WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreaterRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 eventID)
        {
            @event anEvent;
            try
            {
                if (eventID <= 0)
                { return new OperationResult.BadRequest(); }

                using (bltEntities aBLTE = GetRDS())
                {
                    anEvent = GetEntities<@event>(aBLTE).SingleOrDefault(o=>o.event_id == eventID);
                }//end using
                
                //activateLinks<@event>(anEvent);

                return new OperationResult.OK { ResponseResource = anEvent };
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
        public OperationResult POST(@event anEntity)
        {
            try
            {
                //Return BadRequest if missing required fields
                if (string.IsNullOrEmpty(anEntity.name) )
                {return new OperationResult.BadRequest();}

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        //Prior to running stored procedure, check if username exists
                        if (!Exists(aBLTE, ref anEntity))
                        {
                           aBLTE.events.Add(anEntity);
                            aBLTE.SaveChanges();
                        }

                        //activateLinks<@event>(anEntity);

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
        public OperationResult PUT(Int32 eventID, @event instance)
        {
            try
            {
                if (eventID <= 0)
                { return new OperationResult.BadRequest(); }

                 using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        @event ObjectToBeUpdated = aBLTE.events.SingleOrDefault(o => o.event_id == eventID);

                        if (ObjectToBeUpdated == null) { return new OperationResult.BadRequest(); }

                        //Name
                        ObjectToBeUpdated.name = (string.IsNullOrEmpty(instance.name) ?
                            ObjectToBeUpdated.name : instance.name);

                        aBLTE.SaveChanges();
                    }// end using
                }// end using

                //activateLinks<@event>(instance);

                return new OperationResult.OK { ResponseResource = instance };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
        #endregion

        #region DELETE Methods
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 eventID)
        {
            @event ObjectToBeDeleted = null;

            //Return BadRequest if missing required fields
            if (eventID <= 0)
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (bltEntities aBLTRDS = GetRDS(securedPassword))
                {
                    try
                    {
                        //fetch the object to be updated (assuming that it exists)
                        ObjectToBeDeleted = aBLTRDS.events.SingleOrDefault(
                                                m => m.event_id == eventID);

                        //delete it
                        aBLTRDS.events.Remove(ObjectToBeDeleted);
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
        private Boolean Exists(bltEntities aBLTE, ref @event anEvent)
        {
            @event existingEvent;
            @event thisEvent = anEvent;
            //check if it exists
            try
            {
                existingEvent = aBLTE.events.FirstOrDefault(o => o.name == thisEvent.name);

                if (existingEvent == null)
                    return false;

                //if exists then update ref contact
                anEvent = existingEvent;
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
    }//end class OrganizationHandler
}//end namespace