using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BLTDB;
using BLTServices;
using BLTServices.Resources;
using WiM.Test;

namespace BLTServices.Test
{
    [TestClass]
    public class EndpointTests : EndpointTestBase
    {
        #region Private Fields
        private string host = "http://localhost/";
        private string basicAuth = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                .GetBytes("bltadmin:1MhTGVxs"));


        #endregion
        #region Constructor
        public EndpointTests() : base(new Configuration()) { }
        #endregion
        #region Test Methods
        [TestMethod]
        public void TEST()
        {
            Assert.IsTrue(true);
        }//end method
        [TestMethod]
        public void AIRequest()
        {
            //GET LIST
            List<active_ingredient> RequestList = this.GETRequest<List<active_ingredient>>(host + "/ActiveIngredients");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedActiveIngredients'
            List<active_ingredient> versAIRequest = this.GETRequest<List<active_ingredient>>(host + "/ActiveIngredients?status=PUBLISHED&date=09/14/2016");
            Assert.IsNotNull(versAIRequest, RequestList.Count.ToString());

            //'Get'
            List<active_ingredient> getAIRequest = this.GETRequest<List<active_ingredient>>(host + "/ActiveIngredients?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetAI'
            List<active_ingredient> getAIR = this.GETRequest<List<active_ingredient>>(host + "/ActiveIngredients?aiID=3151&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetPULAactiveIngredients'
            List<active_ingredient> getPULAAIR = this.GETRequest<List<active_ingredient>>(host + "/PULAS/15/ActiveIngredients?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //POST
            active_ingredient postObj;
            postObj = this.POSTRequest<active_ingredient>(host + "/ActiveIngredients", new active_ingredient() { ingredient_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.active_ingredient_id.ToString());

            //GET POSTed item
            active_ingredient RequestObj = this.GETRequest<active_ingredient>(host + "/ActiveIngredients/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.ingredient_name = "put-test";
            active_ingredient putObj = this.PUTRequest<active_ingredient>(host + "/ActiveIngredients/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<active_ingredient>(host + "/ActiveIngredients/" + postObj.id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void aiPULARequest()
        {
            //GET LIST
            List<active_ingredient_pula> RequestList = this.GETRequest<List<active_ingredient_pula>>(host + "/PULAs");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedPulas'
            List<active_ingredient_pula> versPUlaRequest = this.GETRequest<List<active_ingredient_pula>>(host + "/PULAs?status=CREATED&date=");
            Assert.IsNotNull(versPUlaRequest, RequestList.Count.ToString());

            //'Get'
            List<active_ingredient_pula> getAIRequest = this.GETRequest<List<active_ingredient_pula>>(host + "/PULAs?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetAIPULAS'
            List<active_ingredient_pula> getAIR = this.GETRequest<List<active_ingredient_pula>>(host + "/PULAs?pulaId={{    }}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetSimplePULAS'
            List<active_ingredient_pula> getSimpPula = this.GETRequest<List<active_ingredient_pula>>(host + "/PULAs/simplePULAs?publishedDate=09/14/2016");
            Assert.IsNotNull(getSimpPula, RequestList.Count.ToString());

            //'GetFilteredSimplePULAs'
            List<active_ingredient_pula> getFiltSimpPula = this.GETRequest<List<active_ingredient_pula>>(host + "/PULAs/FilteredSimplePULAs?date=09/14/2016&aiID={activeIngredientID}&productID={productID}&eventID={eventID}");
            Assert.IsNotNull(getFiltSimpPula, RequestList.Count.ToString());

            //'GetEventPULAs'
            List<active_ingredient_pula> getEvSimpPula = this.GETRequest<List<active_ingredient_pula>>(host + "/Events/{eventId}/PULAs");
            Assert.IsNotNull(getEvSimpPula, RequestList.Count.ToString());

            //POST
            active_ingredient_pula postObj;
            postObj = this.POSTRequest<active_ingredient_pula>(host + "/PULAs", new active_ingredient_pula() { pula_shape_id = 1, base_data = "post-test", event_id = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.pula_id.ToString());

            //GET POSTed item
            active_ingredient_pula RequestObj = this.GETRequest<active_ingredient_pula>(host + "/PULAs/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //GetShapePULAS
            active_ingredient_pula shapePULA = this.GETRequest<active_ingredient_pula>(host + "/PULAs/POI/{shapeId}?publishedDate={date}");
            Assert.IsNotNull(shapePULA);

            //UpdatePulaStatus
            active_ingredient_pula statPULA = this.GETRequest<active_ingredient_pula>(host + "/PULAs/{entityID}/updateStatus?status={status}&statusDate={date}");
            Assert.IsNotNull(statPULA);

            //PUT POSTed item
            postObj.base_data = "put-test";
            active_ingredient_pula putObj = this.PUTRequest<active_ingredient_pula>(host + "/PULAs/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(putObj);

            //AddComments
            postObj.base_data = "put-test";
            active_ingredient_pula putCommentObj = this.PUTRequest<active_ingredient_pula>(host + "/PULAs/" + postObj.id + "/AddComments", new active_ingredient_pula() { comments = "AddComment-test" }, basicAuth);
            Assert.IsNotNull(putCommentObj);

            //Delete POSTed item
            bool success = this.DELETERequest<active_ingredient_pula>(host + "/PULAs/" + postObj.id, basicAuth);
            Assert.IsTrue(success);

        }//end method
        [TestMethod]
        public void aiClassRequest()
        {
            //GET LIST
            List<ai_class> RequestList = this.GETRequest<List<ai_class>>(host + "/AIClasses");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedAIClasss'
            List<ai_class> versAICRequest = this.GETRequest<List<ai_class>>(host + "/AIClasses?status=PUBLISHED&date=");
            Assert.IsNotNull(versAICRequest, RequestList.Count.ToString());

            //'Get'
            List<ai_class> getAICRequest = this.GETRequest<List<ai_class>>(host + "/AIClasses?publishedDate=09/14/2016");
            Assert.IsNotNull(getAICRequest, RequestList.Count.ToString());

            //'GetAIC'
            List<ai_class> getAICR = this.GETRequest<List<ai_class>>(host + "/AIClasses?aiClassID=3151&publishedDate=09/14/2016");
            Assert.IsNotNull(getAICR, RequestList.Count.ToString());

            //'GetActiveIngredientClasses'
            List<ai_class> getAIaiClassR = this.GETRequest<List<ai_class>>(host + "/ActiveIngredients/{activeIngredientID}/AIClass?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIaiClassR, RequestList.Count.ToString());

            //POST
            ai_class postObj;
            postObj = this.POSTRequest<ai_class>(host + "/AIClasses", new ai_class() { ai_class_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.ai_class_id.ToString());

            //AddAIClassToAI
            ai_class postAIObj;
            postAIObj = this.POSTRequest<ai_class>(host + "/AIClasses/{id}/AddAIClass", postObj, basicAuth);
            Assert.IsNotNull(postAIObj, "ID: " + postObj.ai_class_id.ToString());

            //GET POSTed item
            ai_class RequestObj = this.GETRequest<ai_class>(host + "/AIClasses/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.ai_class_name = "put-test";
            ai_class putObj = this.PUTRequest<ai_class>(host + "/AIClasses/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<active_ingredient>(host + "/AIClasses/" + postObj.id, basicAuth);
            Assert.IsTrue(success);

            //RemoveAIClassFromAI
            bool successRemove = this.DELETERequest<ai_class>(host + "/AIClasses/{id}/RemoveAIClassFromAI?activeIngredientID={ai id}", basicAuth);
            Assert.IsTrue(successRemove);


        }//end method
        [TestMethod]
        public void appMethodRequest()
        {
            //GET LIST
            List<application_method> RequestList = this.GETRequest<List<application_method>>(host + "/ApplicationMethods");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedApplicationMethods'
            List<application_method> versAIRequest = this.GETRequest<List<application_method>>(host + "/ApplicationMethods?status=PUBLISHED&date=");
            Assert.IsNotNull(versAIRequest, RequestList.Count.ToString());

            //'Get'
            List<application_method> getAIRequest = this.GETRequest<List<application_method>>(host + "/ApplicationMethods?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetApplicationMethods'
            List<application_method> getAIR = this.GETRequest<List<application_method>>(host + "/ApplicationMethods?applicationMethodID={applicationMethodID}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetPULALimitationsApplicationMethod'
            List<application_method> getPULAAIR = this.GETRequest<List<application_method>>(host + "/PULALimitations/{pulaLimitationsID}/ApplicationMethod?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //POST
            application_method postObj;
            postObj = this.POSTRequest<application_method>(host + "/ApplicationMethods", new application_method() { method = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.application_method_id.ToString());

            //GET POSTed item
            application_method RequestObj = this.GETRequest<application_method>(host + "/ApplicationMethods/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.method = "put-test";
            application_method putObj = this.PUTRequest<application_method>(host + "/ApplicationMethods/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<application_method>(host + "/ApplicationMethods/" + postObj.id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void authRequest()
        {
            //login
            user_ RequestObj = this.GETRequest<user_>(host + "/login", basicAuth);
            Assert.IsNotNull(RequestObj);

        }//end method
        [TestMethod]
        public void cropUseRequest()
        {
            //GET LIST
            List<crop_use> RequestList = this.GETRequest<List<crop_use>>(host + "/CropUses");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedCropUses'
            List<crop_use> versAIRequest = this.GETRequest<List<crop_use>>(host + "/CropUses?status=PUBLISHED&date=");
            Assert.IsNotNull(versAIRequest, RequestList.Count.ToString());

            //'Get'
            List<crop_use> getAIRequest = this.GETRequest<List<crop_use>>(host + "/CropUses?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetApplicationMethods'
            List<crop_use> getAIR = this.GETRequest<List<crop_use>>(host + "/CropUses?CropUseID={cropuseID}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetPULALimitationsCropUse'
            List<crop_use> getPULAAIR = this.GETRequest<List<crop_use>>(host + "/PULALimitations/{pulaLimitationsID}/CropUse?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //POST
            crop_use postObj;
            postObj = this.POSTRequest<crop_use>(host + "/CropUses", new crop_use() { use = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.crop_use_id.ToString());

            //GET POSTed item
            crop_use RequestObj = this.GETRequest<crop_use>(host + "/CropUses/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.use = "put-test";
            crop_use putObj = this.PUTRequest<crop_use>(host + "/CropUses/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<crop_use>(host + "/CropUses/" + postObj.id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void divisionRequest()
        {
            //GET LIST
            List<division> RequestList = this.GETRequest<List<division>>(host + "/Divisions");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            division postObj;
            postObj = this.POSTRequest<division>(host + "/Divisions", new division() { division_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.division_id.ToString());

            //GET POSTed item
            division RequestObj = this.GETRequest<division>(host + "/Divisions/" + postObj.division_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.division_name = "put-test";
            division putObj = this.PUTRequest<division>(host + "/Divisions/" + postObj.division_id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

        }//end method
        [TestMethod]
        public void eventRequest()
        {
            //GET LIST
            List<@event> RequestList = this.GETRequest<List<@event>>(host + "/Events");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            @event postObj;
            postObj = this.POSTRequest<@event>(host + "/Events", new @event() { name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.event_id.ToString());

            //GET POSTed item
            @event RequestObj = this.GETRequest<@event>(host + "/Events/" + postObj.event_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.name = "put-test";
            @event putObj = this.PUTRequest<@event>(host + "/Events/" + postObj.event_id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<@event>(host + "/Events/" + postObj.event_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void formulationRequest()
        {
            //GET LIST
            List<formulation> RequestList = this.GETRequest<List<formulation>>(host + "/Formulations");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedFormulations'
            List<formulation> versAIRequest = this.GETRequest<List<formulation>>(host + "/Formulations?status=PUBLISHED&date=");
            Assert.IsNotNull(versAIRequest, RequestList.Count.ToString());

            //'Get'
            List<formulation> getAIRequest = this.GETRequest<List<formulation>>(host + "/Formulations?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetApplicationMethods'
            List<formulation> getAIR = this.GETRequest<List<formulation>>(host + "/Formulations?FormulationID={formulationID}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetPULALimitationsFormulation'
            List<formulation> getPULAAIR = this.GETRequest<List<formulation>>(host + "/PULALimitations/{pulaLimitationsID}/Formulation?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //POST
            formulation postObj;
            postObj = this.POSTRequest<formulation>(host + "/Formulations", new formulation() { form = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.formulation_id.ToString());

            //GET POSTed item
            formulation RequestObj = this.GETRequest<formulation>(host + "/Formulations/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.form = "put-test";
            formulation putObj = this.PUTRequest<formulation>(host + "/Formulations/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<formulation>(host + "/Formulations/" + postObj.id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void limitationRequest()
        {
            //GET LIST
            List<limitation> RequestList = this.GETRequest<List<limitation>>(host + "/Limitations");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'GetVersionedLimitations'
            List<limitation> versAIRequest = this.GETRequest<List<limitation>>(host + "/Limitations?status=PUBLISHED&date=");
            Assert.IsNotNull(versAIRequest, RequestList.Count.ToString());

            //'Get'
            List<limitation> getAIRequest = this.GETRequest<List<limitation>>(host + "/Limitations?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetLimitations'
            List<limitation> getAIR = this.GETRequest<List<limitation>>(host + "/Limitations/{limitationID}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetPULALimitationsLimitation'
            List<limitation> getPULAAIR = this.GETRequest<List<limitation>>(host + "/PULALimitations/{pulaLimitationsID}/Limitation?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //POST
            limitation postObj;
            postObj = this.POSTRequest<limitation>(host + "/Limitations", new limitation() { code = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.limitation_id.ToString());

            //GET POSTed item
            limitation RequestObj = this.GETRequest<limitation>(host + "/Limitations/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.code = "put-test";
            limitation putObj = this.PUTRequest<limitation>(host + "/Limitations/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<limitation>(host + "/Limitations/" + postObj.id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void organizationRequest()
        {
            //GET LIST
            List<organization> RequestList = this.GETRequest<List<organization>>(host + "/Organizations");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            organization postObj;
            postObj = this.POSTRequest<organization>(host + "/Organizations", new organization() { name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.organization_id.ToString());

            //GET POSTed item
            organization RequestObj = this.GETRequest<organization>(host + "/Organizations/" + postObj.organization_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.name = "put-test";
            organization putObj = this.PUTRequest<organization>(host + "/Organizations/" + postObj.organization_id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);
        }//end method
        [TestMethod]
        public void productRequest()
        {
            //GET LIST
            List<product> RequestList = this.GETRequest<List<product>>(host + "/Products");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'Get'
            List<product> getAIRequest = this.GETRequest<List<product>>(host + "/Products?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //'GetProduct'
            List<product> getAIR = this.GETRequest<List<product>>(host + "/Products?ProductID={productID}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetJqueryProductRequest'
            List<product> getATermIR = this.GETRequest<List<product>>(host + "/Products?publishedDate={date}&term={product}");
            Assert.IsNotNull(getATermIR, RequestList.Count.ToString());

            //'GetActiveIngredientProduct'
            List<product> getPULAAIR = this.GETRequest<List<product>>(host + "/ActiveIngredients/{activeIngredientID}/Product?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //POST
            product postObj;
            postObj = this.POSTRequest<product>(host + "/Products", new product() { product_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.product_id.ToString());

            //POSTproductToAI
            //product postP2AIObj;
            //active_ingredient useThisOne = this.GETRequest<active_ingredient>(host + "/ActiveIngredients/{entityId}");
            //postP2AIObj = this.POSTRequest<active_ingredient>(host + "/Products/{product_id}/AddProductToAI", useThisOne, basicAuth);
            //Assert.IsNotNull(postP2AIObj, "ID: " + postObj.product_id.ToString());

            //GET POSTed item
            product RequestObj = this.GETRequest<product>(host + "/Products/" + postObj.id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.product_name = "put-test";
            product putObj = this.PUTRequest<product>(host + "/Products/" + postObj.id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<product>(host + "/Products/" + postObj.id, basicAuth);
            Assert.IsTrue(success);

            //Delete POSTed ProductAIRel
            bool successprodAI = this.DELETERequest<product>(host + "Products/" + postObj.product_id + "/RemoveProductFromAI?activeIngredientID={active_ingredient_id}", basicAuth);
            Assert.IsTrue(successprodAI);
        }//end method
        [TestMethod]
        public void pulaLimitsRequest()
        {
            //GET LIST
            List<pula_limitations> RequestList = this.GETRequest<List<pula_limitations>>(host + "/PULALimitations");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //'Get'
            List<pula_limitations> getAIRequest = this.GETRequest<List<pula_limitations>>(host + "/PULALimitations?publishedDate=09/14/2016");
            Assert.IsNotNull(getAIRequest, RequestList.Count.ToString());

            //''
            List<pula_limitations> getAIR = this.GETRequest<List<pula_limitations>>(host + "/PULALimitations?pulaLimitationsID={pulaLimitationsID}&publishedDate=09/14/2016");
            Assert.IsNotNull(getAIR, RequestList.Count.ToString());

            //'GetPULALimitations'
            List<pula_limitations> getPULAAIR = this.GETRequest<List<pula_limitations>>(host + "/PULAs/{pulaID}/PULALimitations?publishedDate=09/14/2016");
            Assert.IsNotNull(getPULAAIR, RequestList.Count.ToString());

            //'GetMapperLimitations'
            List<pula_limitations> getmapperLim = this.GETRequest<List<pula_limitations>>(host + "/PULAs/{pulaID}/LimitationsForMapper?ShapeID={pulaSHPID}&EffectDate=09/14/2016");
            Assert.IsNotNull(getmapperLim, RequestList.Count.ToString());

            //POST
            pula_limitations postObj;
            postObj = this.POSTRequest<pula_limitations>(host + "/PULALimitation", new pula_limitations() { pula_id = 1, active_ingredient_id = 1, formulation_id = 1, crop_use_id = 1, limitation_id = 1 }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.formulation_id.ToString());

            //GET POSTed item
            pula_limitations RequestObj = this.GETRequest<pula_limitations>(host + "/PULALimitation/" + postObj.pula_limitation_id);
            Assert.IsNotNull(RequestObj);

            //GET POSTed item
            pula_limitations RequestUpdateObj = this.GETRequest<pula_limitations>(host + "/PULALimitation/" + postObj.pula_limitation_id + "/updateStatus/PUBLISHED&statusDate=09/14/2016", basicAuth);
            Assert.IsNotNull(RequestUpdateObj);

            //PUT POSTed item
            postObj.active_ingredient_id = 2;
            pula_limitations putObj = this.PUTRequest<pula_limitations>(host + "/PULALimitation/" + postObj.pula_limitation_id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<pula_limitations>(host + "/PULALimitation/" + postObj.pula_limitation_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void roleRequest()
        {
            //GET LIST
            List<role> RequestList = this.GETRequest<List<role>>(host + "/Roles");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //POST
            role postObj;
            postObj = this.POSTRequest<role>(host + "/Roles", new role() { role_name = "post-test" }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.role_id.ToString());

            //GET POSTed item
            role RequestObj = this.GETRequest<role>(host + "/Roles/" + postObj.role_id);
            Assert.IsNotNull(RequestObj);

            //PUT POSTed item
            postObj.role_name = "put-test";
            role putObj = this.PUTRequest<role>(host + "/Roles/" + postObj.role_id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);
        }//end method
        [TestMethod]
        public void speciesMethodRequest()
        {
            //GET GetPULASpecies
            List<SpeciesList> RequestList = this.GETRequest<List<SpeciesList>>(host + "/ActiveIngredientPULA/{activeIngredientPULAID}/Species");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetSpeciesList
            List<SpeciesList> RequestSppList = this.GETRequest<List<SpeciesList>>(host + "/SimpleSpecies");
            Assert.IsNotNull(RequestSppList, RequestList.Count.ToString());

            //AddSpeciesToPULA
            //SpeciesList postObj;
            //postObj = this.POSTRequest<SpeciesList>(host + "//PULAs/{pula_id}/AddSpeciesToPULA?publishedDate={date}", new SpeciesList() { SPECIES = { "ENTITY_ID"="1" } }, basicAuth);
            //Assert.IsNotNull(postObj, "ID: " + postObj.role_id.ToString());

            //GET item
            SimpleSpecies RequestObj = this.GETRequest<SimpleSpecies>(host + "/Species/{speciesID}");
            Assert.IsNotNull(RequestObj);

            //RemoveSpeciesFromPULA
            List<SpeciesList> RequestRemSppList = this.GETRequest<List<SpeciesList>>(host + "/PULAs/{entityID}/RemoveSpeciesFromPULA?publishedDate={date}");
            Assert.IsNotNull(RequestRemSppList, RequestList.Count.ToString());
        }//end method
        [TestMethod]
        public void userRequest()
        {
            //GET LIST
            List<user_> RequestList = this.GETRequest<List<user_>>(host + "/Users", basicAuth);
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET GetVersionUsers
            List<user_> versUsers = this.GETRequest<List<user_>>(host + "Versions/{versionID}/Users", basicAuth);
            Assert.IsNotNull(versUsers, versUsers.Count.ToString());

            //POST
            user_ postObj;
            postObj = this.POSTRequest<user_>(host + "/Users", new user_()
            {
                username = "newUser",
                fname = "post-test",
                lname = "user",
                phone = "123-456-7890",
                email = "test@usgs.gov",
                organization_id = 1,
                //password = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("BLTDef@u1t"))
            }, basicAuth);
            Assert.IsNotNull(postObj, "ID: " + postObj.user_id.ToString());

            //GET POSTed item
            user_ RequestObj = this.GETRequest<user_>(host + "/Users/" + postObj.user_id);
            Assert.IsNotNull(RequestObj);

            //GET POSTed item by name
            user_ RequestByNameObj = this.GETRequest<user_>(host + "/Users?username=" + postObj.username);
            Assert.IsNotNull(RequestByNameObj);

            //PUT POSTed item
            postObj.fname = "put-test";
            user_ putObj = this.PUTRequest<user_>(host + "/Users/" + postObj.user_id, postObj, basicAuth);
            Assert.IsNotNull(RequestObj);

            //Delete POSTed item
            bool success = this.DELETERequest<user_>(host + "Users/" + postObj.user_id, basicAuth);
            Assert.IsTrue(success);
        }//end method
        [TestMethod]
        public void versionRequest()
        {
            //GET LIST
            List<version> RequestList = this.GETRequest<List<version>>(host + "/Versions");
            Assert.IsNotNull(RequestList, RequestList.Count.ToString());

            //GET LIST
            List<version> RequestpubVList = this.GETRequest<List<version>>(host + "/Versions?publishedDate={date}");
            Assert.IsNotNull(RequestpubVList, RequestList.Count.ToString());

            //GET item
            version RequestObj1 = this.GETRequest<version>(host + "/ActiveIngredients/{aiEntityID}/Version");
            Assert.IsNotNull(RequestObj1);

            //GET item
            version RequestObj2 = this.GETRequest<version>(host + "/Version/{entityID}");
            Assert.IsNotNull(RequestObj2);
        }//end method
        
        #endregion
    }
}
