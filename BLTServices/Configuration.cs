//------------------------------------------------------------------------------
//----- Configuration -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jonathan Baier
//              Jeremy Newson          
//  
//   purpose:   Configuration implements the IConfiurationSource interface. OpenRasta
//              will call the Configure method and use it to configure the application 
//              through a fluent interface using the Resource space as root objects. 
//
//discussion:   The ResourceSpace is where you can define the resources in the application and what
//              handles them and how thy are represented. 
//              https://github.com/openrasta/openrasta/wiki/Configuration
//
//     
#region Comments
// 05.15.13 - JKN- Implement remaining resources
// 11.09.12 - TR - Added PULA and PULALimitations Resources
// 10.10.12 - JB - Created
#endregion                          
using System.Collections.Generic;
using BLTDB;
using OpenRasta.Authentication;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Web.UriDecorators;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Pipeline;
using OpenRasta.Security;
using BLTServices.Authentication;
using BLTServices.Resources;
using BLTServices.Handlers;
using BLTServices.Codecs;
using BLTServices.PipelineContributors;

namespace BLTServices
{
    public class Configuration:IConfigurationSource
    {
        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {
                // specify the authentication scheme you want to use, you can register multiple ones
                ResourceSpace.Uses.CustomDependency<IAuthenticationProvider, BLTAuthenticationProvider>(DependencyLifetime.Singleton);
                ResourceSpace.Uses.PipelineContributor<BasicAuthorizerContributor>();

                // register your basic authenticator in the DI resolver
                // Allow codec choice by extension 
                ResourceSpace.Uses.UriDecorator<ContentTypeExtensionUriDecorator>();
                ResourceSpace.Uses.PipelineContributor<CrossDomainPipelineContributor>();

                AddAUTHENTICATION_Resources();
                AddACTIVE_INGREDIENT_Resources();
                AddACTIVE_INGREDIENT_PULA_Resources();
                AddAI_CLASS_Resources();
                AddAPPLICATION_METHOD_Resources();
                AddCROP_USE_Resources();
                AddDIVISION_Resources();
                AddEVENT_Resources();
                AddFORMULATION_Resources();
                AddLIMITATION_Resources();
                AddORGANIZATION_Resources();
                AddPRODUCT_Resources();
                AddPULA_LIMITATIONS_Resources();
                AddROLE_Resources();                
                AddSPECIES_Resources();
                AddUSER_Resources();
                AddVERSION_Resources();

            }//End using OpenRastaConfiguration.Manual

        }//End Configure()

        #region Helper methods

        private void AddACTIVE_INGREDIENT_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<active_ingredient>>()
                .AtUri("/ActiveIngredients")
                .And.AtUri("/ActiveIngredients?status={status}&date={date}").Named("GetVersionedActiveIngredients")
                .And.AtUri("/ActiveIngredients?publishedDate={date}")
                .And.AtUri("/ActiveIngredients?aiID={activeIngredientID}&publishedDate={date}").Named("GetAI")
                .And.AtUri("/PULAS/{pulaID}/ActiveIngredients?publishedDate={date}").Named("GetPULAactiveIngredients") //not being used by internal app
                .HandledBy<ActiveIngredientHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<active_ingredient>()
                .AtUri("/ActiveIngredients/{entityID}").Named("GetEntity")
                .HandledBy<ActiveIngredientHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");


        }//end AddACTIVE_INGREDIENT_Resources

        private void AddACTIVE_INGREDIENT_PULA_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<active_ingredient_pula>>()
                .AtUri("/PULAs").Named("GetAll")
                .And.AtUri("/PULAs?status={status}&date={date}").Named("GetVersionedPulas")
                .And.AtUri("/PULAs?publishedDate={date}").Named("GetPublishedPulas")
                .And.AtUri("/PULAs/AIPulas?PulaShapeId={pulaShapeId}&PublishedDate={date}").Named("GetAIPULAS")
                .HandledBy<ActiveIngredientPULAHander>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<PULAList>()
               .AtUri("/PULAs/simplePULAs?publishedDate={date}").Named("GetSimplePULAS")
               .And.AtUri("/PULAs/FilteredSimplePULAs?date={date}&aiID={activeIngredientID}&productID={productID}&eventID={eventID}").Named("GetFilteredSimplePULAs")
               .And.AtUri("/PULAs/EffectiveSimplePULAs?publishedDate={date}&aiID={activeIngredientID}&productID={productID}").Named("GetEffectiveSimplePULAs") //not sure if this is getting used
               .And.AtUri("/Events/{eventId}/PULAs").Named("GetEventPULAs")
               .HandledBy<ActiveIngredientPULAHander>()
               .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");


            ResourceSpace.Has.ResourcesOfType<active_ingredient_pula>()
                .AtUri("/PULAs/{entityID}").Named("GetEntity")
                .And.AtUri("/PULAs/POI/{shapeId}?publishedDate={date}").Named("GetShapePULAS")
                .And.AtUri("/PULAs/{entityID}/updateStatus?status={status}&statusDate={date}").Named("UpdatePulaStatus")
                .And.AtUri("/PULAs/{pulaID}/Expire&date={date}").Named("ExpirePULA") //don't think this is used since updateStatus can do this
                .And.AtUri("/PULAs/{pulaID}/AddComments").Named("AddComments")
                .HandledBy<ActiveIngredientPULAHander>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        } //end AddACTIVE_INGREDIENT_PULA_Resources
       
        private void AddAI_CLASS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<ai_class>>()
                .AtUri("/AIClasses")
                .And.AtUri("/AIClasses?status={status}&date={date}").Named("GetVersionedAIClasss")
                .And.AtUri("/AIClasses?publishedDate={date}")
                .And.AtUri("/AIClasses?aiClassID={aiClassID}&publishedDate={date}") //not being used by internal app
                .And.AtUri("/ActiveIngredients/{activeIngredientID}/AIClass?publisedDate={date}").Named("GetActiveIngredientClasses")
                .HandledBy<AIClassHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<ai_class>()
                .AtUri("/AIClasses/{entityID}").Named("GetEntity")
                .And.AtUri("/AIClasses/{entityID}/RemoveAIClassFromAI?activeIngredientID={aiEntityID}").Named("RemoveAIClassFromAI")
                .And.AtUri("/AIClasses/{entityID}/AddAIClass").Named("AddAIClassToAI")
                .HandledBy<AIClassHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddAI_CLASS_Resources

        private void AddAPPLICATION_METHOD_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<application_method>>()
                .AtUri("/ApplicationMethods")
                .And.AtUri("/ApplicationMethods?status={status}&date={date}").Named("GetVersionedApplicationMethods")
                .And.AtUri("/ApplicationMethods?publishedDate={date}")
                .And.AtUri("/ApplicationMethods?applicationMethodID={applicationMethodID}&publishedDate={date}").Named("GetApplicationMethods")
                .And.AtUri("/PULALimitations/ApplicationMethod?PulaLimitationId={pulaLimitationID}&PublishedDate={date}").Named("GetPULALimitationsApplicationMethod")
                .HandledBy<ApplicationMethodHandler>().TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<application_method>()
                .AtUri("/ApplicationMethods/{entityID}").Named("GetEntity")
                .HandledBy<ApplicationMethodHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
                        
        }//end AddAPPLICATION_METHOD_Resources

        private void AddAUTHENTICATION_Resources()
        {
            //Authentication
            ResourceSpace.Has.ResourcesOfType<user_>()
            .AtUri("/login")
            .HandledBy<LoginHandler>()
            .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddAUTHENTICATION_Resources

        private void AddCROP_USE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<crop_use>>()
                .AtUri("/CropUses")
                .And.AtUri("/CropUses?status={status}&date={date}").Named("GetVersionedCropUses")
                .And.AtUri("/CropUses?publishedDate={date}")
                .And.AtUri("/CropUses?CropUseID={cropUseID}&publishedDate={date}")
                .And.AtUri("/PULALimitations/CropUse?PulaLimitationId={pulaLimitationID}&PublishedDate={date}").Named("GetPULALimitationsCropUse")
                .HandledBy<CropUseHandler>()
                
            .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<crop_use>()
                .AtUri("/CropUses/{entityID}").Named("GetEntity")
                .HandledBy<CropUseHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
                        
        }//end AddCROP_USE_Resources

        private void AddDIVISION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<division>>()
                .AtUri("/Divisions")
                .HandledBy<DivisionHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<division>()
                .AtUri("/Divisions/{divisionID}")
                .HandledBy<DivisionHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
                        
        }//end AddDIVISION_Resources

        private void AddEVENT_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<@event>>()
                .AtUri("/Events")
                .HandledBy<EventsHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<@event>()
                .AtUri("/Events/{eventID}")
                .HandledBy<EventsHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddEVENT_Resources
       
        private void AddFORMULATION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<formulation>>()
                .AtUri("/Formulations")
                .And.AtUri("/Formulations?status={status}&date={date}").Named("GetVersionedFormulations")
                .And.AtUri("/Formulations?publishedDate={date}")
                .And.AtUri("/Formulations?FormulationID={formulationID}&publishedDate={date}")
                .And.AtUri("/PULALimitations/Formulation?PulaLimitationId={pulaLimitationID}&PublishedDate={date}").Named("GetPULALimitationsFormulation")
                .HandledBy<FormulationHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<formulation>()
                .AtUri("/Formulations/{entityID}").Named("GetEntity")
                .HandledBy<FormulationHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddFORMULATION_Resources

        private void AddLIMITATION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<limitation>>()
                .AtUri("/Limitations")
                .And.AtUri("/Limitations?status={status}&date={date}").Named("GetVersionedLimitations")
                .And.AtUri("/Limitations?publishedDate={date}")
                .And.AtUri("/Limitations/{limitationID}?publishedDate={date}").Named("GetLimitations")
                .And.AtUri("/PULALimitations/Limitation?PulaLimitationId={pulaLimitationID}&PublishedDate={date}").Named("GetPULALimitationsLimitation")
                .HandledBy<LimitationHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<limitation>()
                .AtUri("/Limitations/{entityID}").Named("GetEntity")
                .HandledBy<LimitationHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddLIMITATION_Resources

        private void AddORGANIZATION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<organization>>()
                .AtUri("/Organizations")
                .HandledBy<OrganizationHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<organization>()
                .AtUri("/Organizations/{organizationID}")
                .HandledBy<OrganizationHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddORGANIZATION_Resources

        private void AddPRODUCT_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<product>>()
                .AtUri("/Products")
                .And.AtUri("/Products?publishedDate={date}")
                .And.AtUri("/Products?ProductID={productID}&publishedDate={date}")
                .And.AtUri("/Products?publishedDate={date}&term={product}").Named("GetJqueryProductRequest")
                .And.AtUri("/ActiveIngredients/{activeIngredientID}/Product?publishedDate={date}").Named("GetActiveIngredientProduct")
                .HandledBy<ProductHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");


            ResourceSpace.Has.ResourcesOfType<product>()
                .AtUri("/Products/{entityID}").Named("GetEntity")
                .And.AtUri("/Products/{entityID}/RemoveProductFromAI?activeIngredientID={aiEntityID}").Named("RemoveProductFromAI")
                .And.AtUri("/Products/{entityID}/AddProductToAI").Named("addProductToAI")
                .HandledBy<ProductHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddPRODUCT_Resources

        private void AddPULA_LIMITATIONS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<pula_limitations>>()
                .AtUri("/PULALimitations")
                .And.AtUri("/PULALimitations?publishedDate={date}")
                .And.AtUri("/PULALimitations?pulaLimitationsID={pulaLimitationsID}&publishedDate={date}")
                .And.AtUri("/PULAs/{pulaID}/PULALimitations?publishedDate={date}").Named("GetPULALimitations")                
                .HandledBy<PULALimitationsHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<pula_limitations>()
                .AtUri("/PULALimitations/{entityID}").Named("GetEntity")
                .And.AtUri("/PULALimitations/{entityID}/updateStatus?status={status}&statusDate={date}").Named("UpdatePulaLimitationsStatus")
                .HandledBy<PULALimitationsHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<MapperLimitations>()
                .AtUri("/PULAs/{pulaID}/LimitationsForMapper?ShapeID={pulaSHPID}&EffectDate={date}").Named("GetMapperLimitations")
                .HandledBy<PULALimitationsHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddPULA_LIMITATIONS_Resources

        private void AddROLE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<role>>()
                .AtUri("/Roles")
                .HandledBy<RoleHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<role>()
                .AtUri("/Roles/{roleID}")
                .HandledBy<RoleHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddROLE_Resources

        private void AddSPECIES_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<SpeciesList>()
                .AtUri("/ActiveIngredientPULA/{activeIngredientPULAID}/Species").Named("GetPULASpecies")
                .And.AtUri("/PULAs/{entityID}/AddSpeciesToPULA?publishedDate={date}").Named("AddSpeciesToPULA")
                .And.AtUri("/SimpleSpecies").Named("GetSpeciesList")
                .And.AtUri("/PULAs/{entityID}/RemoveSpeciesFromPULA?publishedDate={date}").Named("RemoveSpeciesFromPULA")                
                .HandledBy<SpeciesHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
            
            ResourceSpace.Has.ResourcesOfType<SimpleSpecies>()
                .AtUri("/Species/{speciesID}")
                .HandledBy<SpeciesHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddSPECIES_Resources
        
        private void AddUSER_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<user_>>()
                .AtUri("/Users")
                .And.AtUri("Versions/{versionID}/Users").Named("GetVersionUsers")
                .HandledBy<UserHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<user_>()
                .AtUri("/Users/{userID}")
                .And.AtUri("Users?username={userName}").Named("GetUserByUserName")
                //.And.AtUri("/Users?username={userName}newPass={newPassword}").Named("ChangeUserPassword")
                .HandledBy<UserHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddUSER_Resources

        private void AddVERSION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<version>>()
               .AtUri("/Versions")
               .And.AtUri("/Versions?publishedDate={date}")
               .HandledBy<VersionHandler>()
               .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9"); 
            
            ResourceSpace.Has.ResourcesOfType<version>()
                .AtUri("/Versions/{PULALimitID}") //don't see this endpoint in the handler
                .And.AtUri("/Version/{entityID}").Named("GetVersion")
                .And.AtUri("/ActiveIngredients/{aiEntityID}/Version").Named("GetActiveIngredientVersion")
                .HandledBy<VersionHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

        }
        #endregion

    }//End class Configuration
}//End namespace