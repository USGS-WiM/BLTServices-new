//------------------------------------------------------------------------------
//----- PartsController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Parts page and link to individual parts pages 
//
//discussion:   
//
//     

#region Comments
// 07.22.13 - TR - Modifier has become ApplicationMethod and Formulation
// 05.30.13 - TR - Added Species
// 05.29.13 - TR - Added Product
// 05.28.13 - TR - Hooked into services
// 04.23.13 - TR - Created

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Text.RegularExpressions;

using RestSharp;
using BLTServices;
using BLTServices.Authentication;
using BLTServices.Resources;
using BLTWeb.Utilities;
using BLTWeb.Models;
using BLTWeb.Helpers;

namespace BLTWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class PartsController : Controller
    {
        //
        // GET: /Parts/
        public ActionResult Index()
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();

            //get the role of user logged in
            ViewData["Role"] = GetLoggedInMember();
            
            #region AI
            request = new RestRequest();
            request.Resource = "/ActiveIngredients?publishedDate={date}";
            request.RootElement = "ArrayOfACTIVE_INGREDIENT";
            List<ACTIVE_INGREDIENT> aiList = serviceCaller.Execute<List<ACTIVE_INGREDIENT>>(request);
            ViewData["AIList"] = aiList;
            #endregion AI

            #region AI Class
            request = new RestRequest();
            request.Resource = "/AIClasses?publishedDate={date}";
            request.RootElement = "ArrayOfAI_CLASS";
            List<AI_CLASS> AIClassList = serviceCaller.Execute<List<AI_CLASS>>(request);
            ViewData["AIClassList"] = AIClassList;
            #endregion  AI Class

            #region CropUse
            request = new RestRequest();
            request.Resource = "/CropUses?publishedDate={date}";
            request.RootElement = "ArrayOfCROP_USE";
            List<CROP_USE> CropUseList = serviceCaller.Execute<List<CROP_USE>>(request);
            ViewData["CropUseList"] = CropUseList;
            #endregion CropUse

            #region ApplicationMethod
            request = new RestRequest();
            request.Resource = "/ApplicationMethods?publishedDate={date}";
            request.RootElement = "ArrayOfAPPLICATION_METHOD";
            List<APPLICATION_METHOD> AppMethodsList = serviceCaller.Execute<List<APPLICATION_METHOD>>(request);
            ViewData["AppMethodsList"] = AppMethodsList;
            #endregion Modifier

            #region Organization
            request = new RestRequest();
            request.Resource = "/Organizations/";
            request.RootElement = "ArrayOfORGANIZATION";
            List<ORGANIZATION> OrganizationList = serviceCaller.Execute<List<ORGANIZATION>>(request);
            ViewData["OrganizationList"] = OrganizationList;
            #endregion  Organization

            #region Product
            //uses autocomplete
            #endregion Product

            #region Event
            request = new RestRequest();
            request.Resource = "/Events";
            request.RootElement = "ArrayOfEVENT";
            List<EVENT> eventList = serviceCaller.Execute<List<EVENT>>(request);
            ViewData["EventList"] = eventList;
            #endregion Event

            #region LimitationCodes
            request.Resource = "/Limitations?publishedDate={date}";
            request.RootElement = "ArrayOfLIMITATIONS";
            List<LIMITATION> LimitationList = serviceCaller.Execute<List<LIMITATION>>(request);
            ViewData["LimitationList"] = LimitationList;
            #endregion LimitationCodes

            #region Formulation
            request = new RestRequest();
            request.Resource = "/Formulations?publishedDate={date}";
            request.RootElement = "ArrayOfFORMULATION";
            List<FORMULATION> formulationList = serviceCaller.Execute<List<FORMULATION>>(request);
            ViewData["FormulationsList"] = formulationList;
            #endregion Formulation           
          
            #region Division
            request = new RestRequest();
            request.Resource = "/Divisions/";
            request.RootElement = "ArrayOfDIVISION";
            List<DIVISION> DivisionList = serviceCaller.Execute<List<DIVISION>>(request);
            ViewData["DivisionList"] = DivisionList;
            #endregion Division

            return View();
        }

        #region AI

        //want to do something, depends on Create for AI
        [HttpPost]
        public ActionResult AI(FormCollection fc, string Create)
        {
            try
            {
                int AI_Id = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("AINew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    AI_Id = Convert.ToInt32(fc["ID"]);
                    if (AI_Id == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("AIEdit", new { id = AI_Id });
                    }
                }
                else
                {
                    //Copy to New
                    AI_Id = Convert.ToInt32(fc["ID"]);
                    if (AI_Id == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("AICopy", new { id = AI_Id });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //edit page
        public ActionResult AIEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            
            //get this AI based on it's ID

            ACTIVE_INGREDIENT thisAI = GetAnAI(id);

            //get all the AI classes for dropdown
            var request = new RestRequest();
            request.Resource = "/AIClasses?publishedDate={date}";
            request.RootElement = "ArrayOfAI_CLASS";
            List<AI_CLASS> AIClassList = serviceCaller.Execute<List<AI_CLASS>>(request);
            ViewData["AIClassList"] = AIClassList;

            //get the AI's AI_Classes
            request = new RestRequest();
            request.Resource = "/ActiveIngredients/{activeIngredientID}/AIClass?publishedDate={date}";
            request.RootElement = "ArrayOfAI_CLASS";
            request.AddParameter("activeIngredientID", thisAI.ACTIVE_INGREDIENT_ID, ParameterType.UrlSegment);
            List<AI_CLASS> AIClasses = serviceCaller.Execute<List<AI_CLASS>>(request);
            
            //Populate model for view
            AIModel thisAIModel = new AIModel();

            //the AI
            thisAIModel.AI = thisAI;

            if (AIClasses != null)
            {
                ViewData["AIClasses"] = AIClasses.OrderBy(a => a.AI_CLASS_NAME).ToList();
                //the AIClass IDs for hidden input to maintain
                string aicID = string.Empty;
                string trimmedAicID = string.Empty;
                if (AIClasses.Count >= 1)
                {//store each entityID (so the correct version can be gotten)
                    foreach (AI_CLASS aic in AIClasses)
                    {
                        aicID += aic.ID + ",";
                    }
                    trimmedAicID = aicID.TrimEnd(',', ' ');
                }
                thisAIModel.AIClassesToAdd = trimmedAicID;
            }

            //get the Products linked to this AI by ID
            serviceCaller = BLTServiceCaller.Instance;
            request = new RestRequest();
            request.Resource = "/ActiveIngredients/{activeIngredientID}/Product?publishedDate={date}";
            request.RootElement = "ArrayOfPRODUCT";
            request.AddParameter("activeIngredientID", thisAI.ACTIVE_INGREDIENT_ID, ParameterType.UrlSegment);
            List<PRODUCT> AIProducts = serviceCaller.Execute<List<PRODUCT>>(request);
            ViewData["AIProducts"] = AIProducts;

            return View("AI/AIEdit", thisAIModel);
        }
        
        //Post the edit to update the AI
        [HttpPost]
        public ActionResult AI_Edit(int id, AIModel updatedAIModel)
        {
            ACTIVE_INGREDIENT thisAI = updatedAIModel.AI;
            string AIClasses2Add = updatedAIModel.AIClassesToAdd;
            string AIClasses2Remove = updatedAIModel.AIClassesIDsToRemove;            

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            //see if any products have been related or any removed from relationship
            //if prodsToRemove != null -> expire those
            if (!string.IsNullOrWhiteSpace(AIClasses2Remove))
            {
                //parse entityIDs
                string[] RemoveAIC = Regex.Split(AIClasses2Remove, ",");
                foreach (string p in RemoveAIC)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        if (thisAI != null)
                        {
                            //send request to expire it
                            request = new RestRequest(Method.POST);
                            request.Resource = "/AIClasses/{entityID}/RemoveAIClassFromAI?activeIngredientID={aiEntityID}";
                            request.AddParameter("entityID", Convert.ToDecimal(p), ParameterType.UrlSegment);
                            request.AddParameter("aiEntityID", Convert.ToDecimal(thisAI.ACTIVE_INGREDIENT_ID), ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            serviceCaller.Execute<AI_CLASS>(request);
                        }
                    }
                }
            }
            // if prodsToAdd != null -> post those (there's a check to see if they already exist)
            if (!string.IsNullOrWhiteSpace(AIClasses2Add))
            {
                //parse
                string[] AddAIC = Regex.Split(AIClasses2Add, ",").ToArray();
                foreach (string a in AddAIC)
                {
                    if (!string.IsNullOrWhiteSpace(a))
                    {
                        if (thisAI != null)
                        {
                            request = new RestRequest(Method.POST);
                            request.Resource = "/AIClasses/{entityID}/AddAIClass";
                            request.AddParameter("entityID", Convert.ToDecimal(a), ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(thisAI);
                            ACTIVE_INGREDIENT_AI_CLASS newAIC = serviceCaller.Execute<ACTIVE_INGREDIENT_AI_CLASS>(request);
                        }
                    }
                }
            }

            request = new RestRequest(Method.POST);
            request.Resource = "/ActiveIngredients/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<ACTIVE_INGREDIENT>(thisAI), ParameterType.RequestBody);
            ACTIVE_INGREDIENT updatedAI = serviceCaller.Execute<ACTIVE_INGREDIENT>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a AI
        public ActionResult ExpireAI(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/ActiveIngredients/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            ACTIVE_INGREDIENT expiredAI = serviceCaller.Execute<ACTIVE_INGREDIENT>(request);

            return RedirectToAction("Index");
        }
        
        //copy to new page
        public ActionResult AICopy(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            
            ACTIVE_INGREDIENT thisAI = GetAnAI(id);

            var request = new RestRequest();
            request.Resource = "/AIClasses?ActiveDate={date}";
            request.RootElement = "ArrayOfAI_CLASS";
            List<AI_CLASS> AIClassList = serviceCaller.Execute<List<AI_CLASS>>(request);
            ViewData["AIClassList"] = AIClassList;

            return View("AI/AICopy", thisAI);
        }
        
        //post new ai
        [HttpPost]
        public ActionResult AI_Copy(ACTIVE_INGREDIENT newAI)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ActiveIngredients";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<ACTIVE_INGREDIENT>(newAI), ParameterType.RequestBody);

                ACTIVE_INGREDIENT createdAI = serviceCaller.Execute<ACTIVE_INGREDIENT>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }
 
        //new page
        public ActionResult AINew()
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/AIClasses?ActiveDate={date}";
            request.RootElement = "ArrayOfAI_CLASS";
            ViewData["AIClassList"] = serviceCaller.Execute<List<AI_CLASS>>(request);
            
            return View("AI/AINew");
        }

        //post the new AI
        [HttpPost]
        public ActionResult AI_New(AIModel newAI)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ActiveIngredients";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<ACTIVE_INGREDIENT>(newAI.AI), ParameterType.RequestBody);
                ACTIVE_INGREDIENT createdAI = serviceCaller.Execute<ACTIVE_INGREDIENT>(request);


                //if Products were chosen, add to PRODUCT_ACTIVE_INGREDIENT
                if (newAI.AIProdsToAdd != null)
                {
                    if (newAI.AIProdsToAdd.Length >= 1)
                    {
                        string[] prods = Regex.Split(newAI.AIProdsToAdd, ",");
                        List<PRODUCT> productList = new List<PRODUCT>();

                        foreach (string pr in prods)
                        {
                            if (!string.IsNullOrWhiteSpace(pr))
                            {
                                request = new RestRequest(Method.POST);
                                request.Resource = "/Products/{entityID}/AddProductToAI";
                                request.AddParameter("entityID", Convert.ToDecimal(pr), ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(createdAI);
                                serviceCaller.Execute<PRODUCT_ACTIVE_INGREDIENT>(request);
                            }
                        }
                    }
                }
                //if AIClasses were chosen, add to ACTIVE_INGREDIENT_AI_CLASS
                if (newAI.AIClassesToAdd != null)
                {
                    if (newAI.AIClassesToAdd.Length >= 1)
                    {
                        string[] AIclasses = Regex.Split(newAI.AIClassesToAdd, ",");
                        List<AI_CLASS> aiClList = new List<AI_CLASS>();

                        foreach (string aic in AIclasses)
                        {
                            if (!string.IsNullOrWhiteSpace(aic))
                            {
                                //now post it
                                request = new RestRequest(Method.POST);
                                request.Resource = "/AIClasses/{entityID}/AddAIClass";
                                request.AddParameter("entityID", Convert.ToDecimal(aic), ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(createdAI);
                                ACTIVE_INGREDIENT_AI_CLASS newAIcAI = serviceCaller.Execute<ACTIVE_INGREDIENT_AI_CLASS>(request);
                            }
                        }
                    }
                }
                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //want to edit the Product - AI relationship
        public ActionResult PAIEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            ACTIVE_INGREDIENT thisAI = GetAnAI(id);

            //get the AI Class
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/ActiveIngredients/{activeIngredientID}/AIClass?publishedDate={date}";
            request.RootElement = "ArrayOfAI_CLASS";
            request.AddParameter("activeIngredientID", thisAI.ACTIVE_INGREDIENT_ID, ParameterType.UrlSegment);
            List<AI_CLASS> AIClasses = serviceCaller.Execute<List<AI_CLASS>>(request);
            
            if (AIClasses != null)
                ViewData["AIClasses"] = AIClasses.OrderBy(a => a.AI_CLASS_NAME).ToList();

            //get the Products linked to this AI by ID
            serviceCaller = BLTServiceCaller.Instance;
            request = new RestRequest();
            request.Resource = "/ActiveIngredients/{activeIngredientID}/Product?publishedDate={date}";
            request.RootElement = "ArrayOfPRODUCT";
            request.AddParameter("activeIngredientID", thisAI.ACTIVE_INGREDIENT_ID, ParameterType.UrlSegment);
            List<PRODUCT> AIProducts = serviceCaller.Execute<List<PRODUCT>>(request);
            ViewData["AIProducts"] = AIProducts;

            //Populate model for view
            AIModel thisAIModel = new AIModel();

            //the AI
            thisAIModel.AI = thisAI;

            //the product IDs for hidden input to maintain
            string prID = string.Empty;
            string trimmedprID = string.Empty;
            if (AIProducts.Count >= 1)
            {
                foreach (PRODUCT pr in AIProducts)
                {
                    prID += pr.PRODUCT_ID + ",";
                }
                trimmedprID = prID.TrimEnd(',', ' ');
            }
            thisAIModel.AIProdsToAdd = trimmedprID;

            return View("ProductAI/PAIEdit", thisAIModel);
        }

        [HttpPost]  
        public ActionResult PAI_Edit(AIModel updatedAIModel)
        {
            ACTIVE_INGREDIENT thisAI = updatedAIModel.AI;

            string Products2Add = updatedAIModel.AIProdsToAdd;
            string Products2Remove = updatedAIModel.ProductIDsToRemove;
            
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();

            //see if any products have been related or any removed from relationship
            //if prodsToRemove != null -> expire those
            if (!string.IsNullOrWhiteSpace(Products2Remove))
            {
                //parse
                string[] RemovePAI = Regex.Split(Products2Remove, ",");
                foreach (string p in RemovePAI)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        if (thisAI != null)
                        {
                            //send request to expire it
                            request = new RestRequest(Method.POST);
                            request.Resource = "/Products/{entityID}/RemoveProductFromAI?activeIngredientID={aiEntityID}";
                            request.AddParameter("entityID", Convert.ToDecimal(p), ParameterType.UrlSegment);
                            request.AddParameter("aiEntityID", Convert.ToDecimal(thisAI.ACTIVE_INGREDIENT_ID), ParameterType.UrlSegment);
                            request.AddHeader("X-HTTP-Method-Override", "DELETE");
                            request.AddHeader("Content-Type", "application/xml");
                            serviceCaller.Execute<PRODUCT>(request);
                        }
                    }
                }
            }
            // if prodsToAdd != null -> post those (there's a check to see if they already exist)
            if (!string.IsNullOrWhiteSpace(Products2Add))
            {
                //parse
                string[] AddPAI = Regex.Split(Products2Add, ",").ToArray();
                foreach (string a in AddPAI)
                {
                    if (!string.IsNullOrWhiteSpace(a))
                    {
                        if (thisAI != null)
                        {
                            request = new RestRequest(Method.POST);
                            request.Resource = "/Products/{entityID}/AddProductToAI";
                            request.AddParameter("entityID", Convert.ToDecimal(a), ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(thisAI);
                            serviceCaller.Execute<PRODUCT_ACTIVE_INGREDIENT>(request);
                        }
                    }
                }
            }

            return RedirectToAction("../Parts/Index");
        }

        #endregion AI
                
        #region AI_Class

        //want to do something, depends on Create for AI
        [HttpPost]
        public ActionResult AI_Class(FormCollection fc, string Create)
        {
            try
            {
                int AIClass_Id = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("AI_ClassNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    AIClass_Id = Convert.ToInt32(fc["ID"]);
                    if (AIClass_Id == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("AI_ClassEdit", new { id = AIClass_Id });
                    }
                }
                else
                {
                    //Copy to New
                    AIClass_Id = Convert.ToInt32(fc["ID"]);
                    if (AIClass_Id == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("AI_ClassCopy", new { id = AIClass_Id });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //edit page
        public ActionResult AI_ClassEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/AIClasses/{entityID}";
            request.RootElement = "AI_CLASS";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            AI_CLASS thisAIClass = serviceCaller.Execute<AI_CLASS>(request);

            return View("AI_Class/AI_ClassEdit", thisAIClass);
        }

        //Post the edit to update the AI
        [HttpPost]
        public ActionResult AI_Class_Edit(int id, AI_CLASS editedAIClass)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/AIClasses/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<AI_CLASS>(editedAIClass), ParameterType.RequestBody);
            AI_CLASS updatedAIClass = serviceCaller.Execute<AI_CLASS>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a AI_class
        public ActionResult ExpireAI_Class(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/AIClasses/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            AI_CLASS expiredAIc = serviceCaller.Execute<AI_CLASS>(request);

            return RedirectToAction("Index");
        }
        
        //new page
        public ActionResult AI_ClassNew()
        {
            return View("AI_Class/AI_ClassNew");
        }

        //post the new AI
        [HttpPost]
        public ActionResult AI_Class_New(AI_CLASS newAIClass)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/AIClasses";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<AI_CLASS>(newAIClass), ParameterType.RequestBody);

                AI_CLASS createdAIclass = serviceCaller.Execute<AI_CLASS>(request);

                ////now activate it for use
                //request = new RestRequest();
                //request.Resource = "/AIClasses/{entityID}/Activate?ActiveDate={date}";
                //request.RootElement = "AI_CLASS";
                //request.AddParameter("entityID", createdAIclass.ID, ParameterType.UrlSegment);
                //AI_CLASS activatedaiClass = serviceCaller.Execute<AI_CLASS>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        #endregion AI_Class       

        #region CropUse
               
        //want to do something, depends on Create for CropUse
        [HttpPost]
        public ActionResult CropUse(FormCollection fc, string Create)
        {
            try
            {
                int CropUseId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("CropUseNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    CropUseId = Convert.ToInt32(fc["ID"]);
                    if (CropUseId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("CropUseEdit", new { id = CropUseId });
                    }
                }
                else
                {
                    //Copy to New
                    CropUseId = Convert.ToInt32(fc["ID"]);
                    if (CropUseId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("CropUseCopy", new { id = CropUseId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //crop use edit page
        public ActionResult CropUseEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/CropUses/{entityID}";
            request.RootElement = "CROP_USE";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            CROP_USE thisCropUse = serviceCaller.Execute<CROP_USE>(request);
            
            return View("CropUse/CropUseEdit", thisCropUse);        
        }

        //Post the edit to update the CropUse (actually a GET .. no edits allowed
        [HttpPost]
        public ActionResult CropUse_Edit(int id, CROP_USE editedCropUse)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/CropUses/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<CROP_USE>(editedCropUse), ParameterType.RequestBody);
            CROP_USE updatedCropUse = serviceCaller.Execute<CROP_USE>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a Crop Use
        public ActionResult ExpireCropUse(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "CropUses/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            CROP_USE expiredCU = serviceCaller.Execute<CROP_USE>(request);

            return RedirectToAction("Index");
        }

        //crop use copy to new page
        public ActionResult CropUseCopy(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/CropUses/{entityID}";
            request.RootElement = "CROP_USE";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            CROP_USE thisCropUse = serviceCaller.Execute<CROP_USE>(request);

            return View("CropUse/CropUseCopy", thisCropUse);
        }

        //crop use new page
        public ActionResult CropUseNew()
        {
            return View("CropUse/CropUseNew");
        }

        //post the new CropUse
        [HttpPost]
        public ActionResult CropUse_New(CROP_USE newCropUse)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/CropUses";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<CROP_USE>(newCropUse), ParameterType.RequestBody);
                CROP_USE createdCU = serviceCaller.Execute<CROP_USE>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }
               
        #endregion CropUse
      
        #region Application Method

        //want to do something, depends on Create for Application Method
        [HttpPost]
        public ActionResult ApplicationMethods(FormCollection fc, string Create)
        {
            try
            {
                int AppMethodId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("ApplicationMethodsNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    AppMethodId = Convert.ToInt32(fc["ID"]);
                    if (AppMethodId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("ApplicationMethodsEdit", new { id = AppMethodId });
                    }
                }
                else
                {
                    //Copy to New
                    AppMethodId = Convert.ToInt32(fc["ID"]);
                    if (AppMethodId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("ApplicationMethodsCopy", new { id = AppMethodId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //edit page
        public ActionResult ApplicationMethodsEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/ApplicationMethods/{entityID}";
            request.RootElement = "APPLICATION_METHOD";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            APPLICATION_METHOD thisAppMethod = serviceCaller.Execute<APPLICATION_METHOD>(request);

            return View("ApplicationMethods/ApplicationMethodsEdit", thisAppMethod);
        }

        //Post the edit to update the ApplicationMethods
        [HttpPost]
        public ActionResult ApplicationMethods_Edit(int id, APPLICATION_METHOD editedApplicationMethod)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/ApplicationMethods/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<APPLICATION_METHOD>(editedApplicationMethod), ParameterType.RequestBody);
            APPLICATION_METHOD updatedApplicationMethod = serviceCaller.Execute<APPLICATION_METHOD>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a ApplicationMethod
        public ActionResult ExpireApplicationMethod(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/ApplicationMethods/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            APPLICATION_METHOD expiredAppMethod = serviceCaller.Execute<APPLICATION_METHOD>(request);

            return RedirectToAction("Index");
        }
        
        //create new page
        public ActionResult ApplicationMethodsNew()
        {
            return View("ApplicationMethods/ApplicationMethodsNew");
        }

        //post the new ApplicationMethod
        [HttpPost]
        public ActionResult ApplicationMethods_New(APPLICATION_METHOD newApplicationMethod)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ApplicationMethods";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<APPLICATION_METHOD>(newApplicationMethod), ParameterType.RequestBody);

                APPLICATION_METHOD createdApplicationMethod = serviceCaller.Execute<APPLICATION_METHOD>(request);

                //now activate it for use
                //request = new RestRequest();
                //request.Resource = "/ApplicationMethods/{entityID}/Activate?ActiveDate={date}";
                //request.RootElement = "APPLICATION_METHOD";
                //request.AddParameter("entityID", createdApplicationMethod.ID, ParameterType.UrlSegment);
                //APPLICATION_METHOD activatedAppMeth = serviceCaller.Execute<APPLICATION_METHOD>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        #endregion ApplicationMethods

        #region Organization

        //want to do something, depends on Create for organization
        [HttpPost]
        public ActionResult Organizations(FormCollection fc, string Create)
        {
            try
            {
                int OrgId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("OrganizationNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    OrgId = Convert.ToInt32(fc["ORGANIZATION_ID"]);
                    if (OrgId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("OrganizationEdit", new { id = OrgId });
                    }
                }
                else
                {
                    //Copy to New
                    OrgId = Convert.ToInt32(fc["ORGANIZATION_ID"]);
                    if (OrgId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("OrganizationCopy", new { id = OrgId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //organization edit page
        public ActionResult OrganizationEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Organizations/{entityID}";
            request.RootElement = "ORGANIZATION";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            ORGANIZATION thisOrg = serviceCaller.Execute<ORGANIZATION>(request);

            return View("Organizations/OrganizationEdit", thisOrg);
        }

        //Post the edit to update the Organization (actually a GET .. no edits allowed
        [HttpPost]
        public ActionResult Organization_Edit(int id, ORGANIZATION editedOrg)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Organizations/{organizationID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("organizationID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<ORGANIZATION>(editedOrg), ParameterType.RequestBody);
            ORGANIZATION updatedOrg = serviceCaller.Execute<ORGANIZATION>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Organization new page
        public ActionResult OrganizationNew()
        {
            return View("Organizations/OrganizationNew");
        }

        //post the new Organization
        [HttpPost]
        public ActionResult Organization_New(ORGANIZATION newOrg)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Organizations";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<ORGANIZATION>(newOrg), ParameterType.RequestBody);

                ORGANIZATION createdOrg = serviceCaller.Execute<ORGANIZATION>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        #endregion Organization

        #region Product

        //want to do something, depends on Create for Product
        [HttpPost]
        public ActionResult Product(FormCollection fc, string Create)
        {
            try
            {
                int ProductId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("ProductNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    ProductId = Convert.ToInt32(fc["ID"]);
                    if (ProductId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("ProductEdit", new { id = ProductId });
                    }
                }
                else
                {
                    //Copy to New
                    ProductId = Convert.ToInt32(fc["ID"]);
                    if (ProductId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("ProductCopy", new { id = ProductId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //product edit page
        public ActionResult ProductEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Products/{entityID}";
            request.RootElement = "PRODUCT";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            PRODUCT thisProduct = serviceCaller.Execute<PRODUCT>(request);

            return View("Product/ProductEdit", thisProduct);
        }

        //Post the edit to update the Product
        [HttpPost]
        public ActionResult Product_Edit(int id, PRODUCT editedProduct)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Products/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<PRODUCT>(editedProduct), ParameterType.RequestBody);
            PRODUCT updatedProduct = serviceCaller.Execute<PRODUCT>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a Product
        public ActionResult ExpireProduct(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Products/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            PRODUCT expiredProd = serviceCaller.Execute<PRODUCT>(request);

            return RedirectToAction("Index");
        }

        //product new page
        public ActionResult ProductNew()
        {
            return View("Product/ProductNew");
        }

        //post the new Product
        [HttpPost]
        public ActionResult Product_New(PRODUCT newProduct)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Products";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<PRODUCT>(newProduct), ParameterType.RequestBody);

                PRODUCT createdProd = serviceCaller.Execute<PRODUCT>(request);

                //now activate it for use
                //request = new RestRequest();
                //request.Resource = "/Products/{entityID}/Activate?ActiveDate={date}";
                //request.RootElement = "PRODUCT";
                //request.AddParameter("entityID", createdProd.ID, ParameterType.UrlSegment);
                //PRODUCT activatedProd = serviceCaller.Execute<PRODUCT>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //get the active Products
        public JsonResult GetProducts()
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Products?publishedDate={date}";
            request.RootElement = "ArrayOfProduct";
            List<PRODUCT> ProdList = serviceCaller.Execute<List<PRODUCT>>(request);
            List<PRODUCT> NamedProducts = ProdList.Where(p => p.PRODUCT_NAME != "" && p.PRODUCT_NAME != null).OrderBy(p => p.PRODUCT_NAME).ToList();
            return Json(NamedProducts, JsonRequestBehavior.AllowGet);
        }

        #endregion Product
 
        #region Event

        //want to do something, depends on Create for Event
        [HttpPost]
        public ActionResult Events(FormCollection fc, string Create)
        {
            try
            {
                int EventId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("EventNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    EventId = Convert.ToInt32(fc["EVENT_ID"]);
                    if (EventId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("EventEdit", new { id = EventId });
                    }
                }
                else
                {
                    //Copy to New
                    EventId = Convert.ToInt32(fc["EVENT_ID"]);
                    if (EventId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("EventCopy", new { id = EventId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //Event edit page
        public ActionResult EventEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Events/{entityID}";
            request.RootElement = "EVENT";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            EVENT thisEv = serviceCaller.Execute<EVENT>(request);

            return View("Events/EventEdit", thisEv);
        }

        //Post the edit to update the Event (actually a GET .. no edits allowed
        [HttpPost]
        public ActionResult Event_Edit(int id, EVENT editedEv)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Events/{eventID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("eventID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<EVENT>(editedEv), ParameterType.RequestBody);
            EVENT updatedEv = serviceCaller.Execute<EVENT>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Organization new page
        public ActionResult EventNew()
        {
            return View("Events/EventNew");
        }

        //post the new Event
        [HttpPost]
        public ActionResult Event_New(EVENT newEvent)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Events";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<EVENT>(newEvent), ParameterType.RequestBody);

                EVENT createdEvent = serviceCaller.Execute<EVENT>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }
        
        public ActionResult Event_Delete(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Events/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            serviceCaller.Execute<EVENT>(request);

            return RedirectToAction("Index");
        }
        #endregion Event
        
        #region Limitations Codes

        //want to do something, depends on which button they clicked (Create) for Code
        [HttpPost]
        public ActionResult Codes(FormCollection fc, string Create)
        {
            try
            {
                int LimiationId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("CodesNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    LimiationId = Convert.ToInt32(fc["ID"]);
                    if (LimiationId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("CodesEdit", new { id = LimiationId });
                    }
                }
                else
                {
                    //Copy to New
                    LimiationId = Convert.ToInt32(fc["ID"]);
                    if (LimiationId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("CodesCopy", new { id = LimiationId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //get the edit page for codes
        public ActionResult CodesEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Limitations/{entityID}";
            request.RootElement = "LIMITATIONS";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            LIMITATION thisLimitation = serviceCaller.Execute<LIMITATION>(request);
            
            return View("Codes/CodesEdit",thisLimitation);
        }

        //Post the edit to update the Code
        [HttpPost]
        public ActionResult Codes_Edit(int id, LIMITATION editedCode)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Limitations/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<LIMITATION>(editedCode), ParameterType.RequestBody);
            LIMITATION updatedCode = serviceCaller.Execute<LIMITATION>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a Limitation Code
        public ActionResult ExpireCode(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Limitations/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            LIMITATION expiredCode = serviceCaller.Execute<LIMITATION>(request);

            return RedirectToAction("Index");
        }
        
        //copy to new page
        public ActionResult CodesCopy(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Limitations/{entityID}";
            request.RootElement = "LIMITATIONS";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            LIMITATION thisLimitation = serviceCaller.Execute<LIMITATION>(request);

            return View("Codes/CodesCopy", thisLimitation);
        }

        //post the new code
        [HttpPost]
        public ActionResult Codes_Copy(LIMITATION newLimitation)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Limitations";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<LIMITATION>(newLimitation), ParameterType.RequestBody);

                LIMITATION createdLimitation = serviceCaller.Execute<LIMITATION>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //get the codes new page
        public ActionResult CodesNew()
        {
            return View("Codes/CodesNew");
        }

        //post the new Code
        [HttpPost]
        public ActionResult Codes_New(LIMITATION newLimitation)
        {
           try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Limitations";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<LIMITATION>(newLimitation), ParameterType.RequestBody);

                LIMITATION createdLimitation = serviceCaller.Execute<LIMITATION>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        #endregion Limitations Codes
        
        #region Formulations

        //want to do something, depends on Create for Formulations
        [HttpPost]
        public ActionResult Formulations(FormCollection fc, string Create)
        {
            try
            {
                int FormulationId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("FormulationsNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    FormulationId = Convert.ToInt32(fc["ID"]);
                    if (FormulationId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("FormulationsEdit", new { id = FormulationId });
                    }
                }
                else
                {
                    //Copy to New
                    FormulationId = Convert.ToInt32(fc["ID"]);
                    if (FormulationId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("FormulationsCopy", new { id = FormulationId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //edit page
        public ActionResult FormulationsEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Formulations/{entityID}";
            request.RootElement = "FORMULATION";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            FORMULATION thisFormulation = serviceCaller.Execute<FORMULATION>(request);

            return View("Formulations/FormulationsEdit", thisFormulation);
        }

        //Post the edit to update the Formulations
        [HttpPost]
        public ActionResult Formulations_Edit(int id, FORMULATION editedFormulation)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Formulations/{entityID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");
            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<FORMULATION>(editedFormulation), ParameterType.RequestBody);
            FORMULATION updatedFormulation = serviceCaller.Execute<FORMULATION>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Expire a Formulation
        public ActionResult ExpireFormulation(int id)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Formulations/{entityID}";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "DELETE");
            request.AddHeader("Content-Type", "application/xml");
            FORMULATION expiredFormulation = serviceCaller.Execute<FORMULATION>(request);

            return RedirectToAction("Index");
        }

        //create new page
        public ActionResult FormulationsNew()
        {
            return View("Formulations/FormulationsNew");
        }

        //post the new AI
        [HttpPost]
        public ActionResult Formulations_New(FORMULATION newFormulation)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Formulations";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<FORMULATION>(newFormulation), ParameterType.RequestBody);

                FORMULATION createdFormulation = serviceCaller.Execute<FORMULATION>(request);

                //now activate it for use
                //request = new RestRequest();
                //request.Resource = "/Formulations/{entityID}/Activate?ActiveDate={date}";
                //request.RootElement = "FORMULATION";
                //request.AddParameter("entityID", createdFormulation.ID, ParameterType.UrlSegment);
                //FORMULATION activatedMod = serviceCaller.Execute<FORMULATION>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        #endregion Formulations
        
        #region Division

        //want to do something, depends on Create for division
        [HttpPost]
        public ActionResult Divisions(FormCollection fc, string Create)
        {
            try
            {
                int DivId = 0;
                if (Create == "Add New")
                {
                    return RedirectToAction("DivisionNew");
                }
                else if (Create == "Edit")
                {
                    //edit
                    DivId = Convert.ToInt32(fc["DIVISION_ID"]);
                    if (DivId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to edit page                        
                        return RedirectToAction("DivisionEdit", new { id = DivId });
                    }
                }
                else
                {
                    //Copy to New
                    DivId = Convert.ToInt32(fc["DIVISION_ID"]);
                    if (DivId == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //send them to copy to new page
                        return RedirectToAction("DivisionCopy", new { id = DivId });
                    }
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //Division edit page
        public ActionResult DivisionEdit(int id)
        {
            //get the logged in user's role
            ViewData["Role"] = GetLoggedInMember();

            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/Divisions/{entityID}";
            request.RootElement = "DIVISION";
            request.AddParameter("entityID", id, ParameterType.UrlSegment);
            DIVISION thisDiv = serviceCaller.Execute<DIVISION>(request);

            return View("Divisions/DivisionEdit", thisDiv);
        }

        //Post the edit to update the Division (actually a GET .. no edits allowed
        [HttpPost]
        public ActionResult Division_Edit(int id, DIVISION editedDiv)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest(Method.POST);
            request.Resource = "/Divisions/{divisionID}";
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("divisionID", id, ParameterType.UrlSegment);
            request.AddHeader("X-HTTP-Method-Override", "PUT");

            //Use extended serializer
            BLTWebSerializer serializer = new BLTWebSerializer();
            request.AddParameter("application/xml", serializer.Serialize<DIVISION>(editedDiv), ParameterType.RequestBody);
            DIVISION updatedDiv = serviceCaller.Execute<DIVISION>(request);

            //update the AI and go back to the AI index
            return RedirectToAction("Index");
        }

        //Division new page
        public ActionResult DivisionNew()
        {
            return View("Divisions/DivisionNew");
        }

        //post the new Division
        [HttpPost]
        public ActionResult Division_New(DIVISION newDiv)
        {
            try
            {
                BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/Divisions";
                request.RequestFormat = DataFormat.Xml;
                request.AddHeader("Content-Type", "application/xml");
                //Use extended serializer
                BLTWebSerializer serializer = new BLTWebSerializer();
                request.AddParameter("application/xml", serializer.Serialize<DIVISION>(newDiv), ParameterType.RequestBody);

                DIVISION createdDiv = serviceCaller.Execute<DIVISION>(request);

                return RedirectToAction("../Parts/Index");
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        #endregion Division

        //call for who the member logged in is 
        public string GetLoggedInMember()
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "Users?username={userName}";
            request.RootElement = "USER_";
            request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
            USER_ loggedInUser = serviceCaller.Execute<USER_>(request);
            int loggedInMember = Convert.ToInt32(loggedInUser.ROLE_ID);
            string Role = string.Empty;
            switch (loggedInMember)
            {
                case 1: Role = "Admin"; break;
                case 2: Role = "Publish"; break;
                case 3: Role = "Create"; break;
                case 4: Role = "Enforce"; break;
                case 5: Role = "Public"; break;
                case 6: Role = "Review"; break;
                default: Role = "error"; break;
            }

            return Role;
        }
    
        //call to get a Product based on ID
        public ACTIVE_INGREDIENT GetAnAI(int ID)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/ActiveIngredients/{entityID}";
            request.RootElement = "ACTIVE_INGREDIENT";
            request.AddParameter("entityID", ID, ParameterType.UrlSegment);
            ACTIVE_INGREDIENT anAI = serviceCaller.Execute<ACTIVE_INGREDIENT>(request);
            return anAI;
        }

    }
}