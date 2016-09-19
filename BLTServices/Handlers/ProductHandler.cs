//------------------------------------------------------------------------------
//----- ProductHandler ---------------------------------------------------------
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
// 05.29.13 - TR - Created
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Runtime.InteropServices;

namespace BLTServices.Handlers
{
    public class ProductHandler:HandlerBase
    {
        #region Properties
        public override string entityName
        {
            get { return "product"; }
        }
        #endregion
        #region Routed Methods
        #region GetMethods
        //---------------------Returns List of objects---------------------
        // returns all PRODUCT
        //[WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<product> productList;
            try
            {
                //using (EasySecureString securedPassword = GetSecuredPassword())
                //{
                using (bltEntities aBLTE = GetRDS())
                    {
                        productList = GetEntities<product>(aBLTE).ToList();
                    }//end using
                //}//end using
                return new OperationResult.OK { ResponseResource = productList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //[RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetVersionedProducts")]
        public OperationResult GetVersionedProducts(string status, string date)
        {
            IQueryable<product> productQuery;
            List<product> products;
            try
            {
                StatusType statustype = getStatusType(status);
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    productQuery = GetEntities<product>(aBLTE);
                    switch (statustype)
                    {
                        case (StatusType.Published):
                            productQuery.Where(ai => ai.versions.published_time_stamp != null);
                            break;
                        case (StatusType.Reviewed):
                            productQuery.Where(ai => ai.versions.reviewed_time_stamp != null &&
                                            ai.versions.published_time_stamp == null);
                            break;
                        //created
                        default:
                            productQuery.Where(ai => ai.versions.reviewed_time_stamp == null &&
                                            ai.versions.published_time_stamp == null);
                            break;
                    }//end switch

                    productQuery.Where(ai => !ai.versions.expired_time_stamp.HasValue ||
                                        ai.versions.expired_time_stamp < thisDate.Value.Date);

                    products = productQuery.ToList();

                }//end using

                return new OperationResult.OK { ResponseResource = products };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET
        
        // returns all active PRODUCT
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(string date)
        {
            List<product> aProductList = null;
            //ProductList aProductList = new ProductList();
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date; 

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    //just return the first 50 for now..takes too long
                    //aProductList = aBLTE.PRODUCT.Where(a => a.PRODUCT_NAME.StartsWith("ZOECON")).ToList();
                    aProductList = GetActive(GetEntities<product>(aBLTE), thisDate.Value.Date).OrderBy(a => a.product_name).ToList();
                }//end using                

                ////activateLinks<product>(aProductList);

                return new OperationResult.OK { ResponseResource = aProductList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

        // returns all active PRODUCT
        [HttpOperation(HttpMethod.GET, ForUriName="GetJqueryProductRequest")]
        public OperationResult Get(string date, string product)
        {
            //product = first 3 letters of the Product name or it could be the first 3 numbers of a Reg Number
            List<product> aProductList = null;
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                using (bltEntities aBLTE = GetRDS())
                {
                    //see if spaces were passed in (comes through as "+")
                    if (product.Contains("+"))
                    {
                        product = product.Replace("+", " ");
                    }

                    IQueryable<product> query;

                    query = GetEntities<product>(aBLTE).Where(p => p.product_name.StartsWith(product.ToUpper()));
                    if (query.Count() <= 0)
                    {
                        query = GetEntities<product>(aBLTE).Where(p => p.product_registration_number.StartsWith(product.ToUpper()));
                    }
                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    query = query.OrderBy(a => a.product_name);
                    aProductList = query.ToList();

                }//end using                
                return new OperationResult.OK { ResponseResource = aProductList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }

        }//end HttpMethod.GET

       // [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName="GetProducts")]
        public OperationResult Get(Int32 productID, [Optional] string date)
        {
            try
            {
                List<product> productList;
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                if (productID < 0)
                { return new OperationResult.BadRequest(); }

                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<product> query;
                    query = GetEntities<product>(aBLTE).Where(ai => ai.product_id == productID);

                    if (thisDate.HasValue)
                        query = GetActive(query, thisDate.Value.Date);

                    //process
                    productList = query.ToList();
                }//end using

                //activateLinks<product>(productList);

                return new OperationResult.OK { ResponseResource = productList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetActiveIngredientProduct")]
        public OperationResult GetActiveIngredientProduct(Int32 activeIngredientID, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<product> productList;
                using (bltEntities aBLTE = GetRDS())
                {
                    IQueryable<product> query1 =
                            (from ProductAI in GetActive(aBLTE.product_active_ingredient.Where(p => p.active_ingredient_id == activeIngredientID), thisDate.Value.Date)
                             join p in aBLTE.products
                             on ProductAI.product_id equals p.product_id 
                             select p).Distinct();

                    //only return published
                    productList = GetActive(query1, thisDate.Value.Date).ToList();
                    
                    ////activateLinks<product>(productList);

                }//end using

                return new OperationResult.OK { ResponseResource = productList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET


        // returns active Active Ingredients for the given pulaID
        [HttpOperation(HttpMethod.GET, ForUriName = "GetPULALimitationsProduct")]
        public OperationResult GetPULALimitationsCropUse(Int32 pulaLimitatationsID, [Optional]string date)
        {
            try
            {
                DateTime? thisDate = ValidDate(date);
                if (!thisDate.HasValue)
                    thisDate = DateTime.Now.Date;

                //set the date to current date if request is not authorized
                if (DateTime.Compare(thisDate.Value.Date, DateTime.Now.Date) < 0 && !CanManage()) thisDate = DateTime.Now.Date;

                List<product> productList;
                using (bltEntities aBLTE = GetRDS())
                {

                    IQueryable<product> query1 =
                            (from PULALimit in GetActive(aBLTE.pula_limitations.Where(p => p.pula_limitation_id == pulaLimitatationsID), thisDate.Value.Date)
                             join cu in aBLTE.products
                             on PULALimit.product_id equals cu.product_id
                             select cu).Distinct();

                    productList = GetActive(query1, thisDate.Value.Date).ToList();

                    ////activateLinks<product>(productList);

                }//end using

                return new OperationResult.OK { ResponseResource = productList };
            }
            catch (Exception ex)
            {
                return new OperationResult.InternalServerError { ResponseResource = ex.InnerException.ToString() };
            }
        }//end HttpMethod.GET

        //---------------------Returns individual objects---------------------
 //       [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.GET, ForUriName="GetEntity")]
        public OperationResult GetEntity(Int32 entityID)
        {
            product aProduct;
            try
            {
                using (bltEntities aBLTE = GetRDS())
                {
                    aProduct = GetEntities<product>(aBLTE).SingleOrDefault(c => c.id == entityID);

                }//end using
                return new OperationResult.OK { ResponseResource = aProduct };
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
        public OperationResult POST(product anEntity)
        {
            //Return BadRequest if missing required fields
            if (string.IsNullOrEmpty(anEntity.product_registration_number) || string.IsNullOrEmpty(anEntity.product_name))
                return new OperationResult.BadRequest();

            anEntity.product_name = anEntity.product_name.ToUpper();

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
                           
                            anEntity.product_id = GetNextID(aBLTE);

                            aBLTE.products.Add(anEntity);

                            aBLTE.SaveChanges();
                        }
                        else
                        {//it exists, check if expired
                            if (anEntity.versions.expired_time_stamp.HasValue)
                            {
                                product newP = new product();
                                newP.product_name = anEntity.product_name;
                                newP.product_registration_number = anEntity.product_registration_number;
                                newP.version_id = SetVersion(aBLTE, newP.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                                newP.product_id = anEntity.product_id;
                                //anEntity.ID = 0;
                                aBLTE.products.Add(newP);
                                aBLTE.SaveChanges();
                            }//end if
                        }//end if//end if//end if
                    }//end using
                }//end using

                return new OperationResult.OK { ResponseResource = anEntity };
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }

        }//end HttpMethod.GET

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole, CreatorRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "addProductToAI")]
        public OperationResult AddProductToAI(Int32 entityID, active_ingredient AI)
        {
            product aProduct = null;
            //ACTIVE_INGREDIENT aAIngr = null;
            product_active_ingredient newPAI = null;
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
                        aProduct = GetEntities<product>(aBLTE).FirstOrDefault(c => c.product_id == entityID);
                       
                        if (aProduct == null || AI == null)
                        { return new OperationResult.BadRequest(); }

                        newPAI = new product_active_ingredient();
                        newPAI.product_id = aProduct.product_id;
                        newPAI.active_ingredient_id = AI.active_ingredient_id;

                        //check if this relationship already exists
                        if (!Exists1(aBLTE, ref newPAI))
                        {
                            newPAI.product_active_ingredient_id = GetNextPAI_ID(aBLTE);

                            //create version
                            newPAI.version_id = SetVersion(aBLTE, newPAI.version_id, LoggedInUser(aBLTE).user_id, StatusType.Published, DateTime.Now.Date).version_id;
                           
                            aBLTE.product_active_ingredient.Add(newPAI);
                            aBLTE.SaveChanges();                        
                  
                        }//end if

                    }//end using
                }//end using

                return new OperationResult.OK ();
            }
            catch (Exception)
            { return new OperationResult.BadRequest(); }


        }//end HttpMethod.GET

        #endregion
        
        #region PUT/EDIT Methods

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole})]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityID, product anEntity)
        {
            //No editing of tables are allowed. An edit call will create and activate a new entity, and expire the old one
            user_ loggedInUser;
            product aProduct;
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

                        aProduct = aBLTE.products.FirstOrDefault(c => c.id == entityID);

                        if (aProduct == null)
                        { return new OperationResult.BadRequest(); }

                        if (aProduct.versions.published_time_stamp.HasValue)
                        {
                            //can not edit a published entity. Create new
                            //assign next pulaID
                            anEntity.product_id = aProduct.product_id;
                            //assign version
                            anEntity.version_id = SetVersion(aBLTE, anEntity.version_id, loggedInUser.user_id, StatusType.Published, DateTime.Now.Date).version_id;
                            aBLTE.products.Add(anEntity);
                            //expire originals
                            ExpireOtherEntities(aBLTE, LoggedInUser(aBLTE).user_id, anEntity, DateTime.Now.Date);
                            aProduct = anEntity;
                        }
                        else
                        {
                            aProduct.product_name = anEntity.product_name;
                        }//end if

                        aBLTE.SaveChanges();
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

                    product ObjectToBeDelete = aBLTE.products.FirstOrDefault(am => am.id == entityID);

                    if (ObjectToBeDelete == null)
                    { return new OperationResult.BadRequest(); }

                    //NOTE: ShapeID can not be changed
                    if (ObjectToBeDelete.versions.published_time_stamp.HasValue)
                    {
                        //set the date to be first of following month 
                        //int nextMo = DateTime.Now.Month + 1;
                        //DateTime nextMonth = Convert.ToDateTime(nextMo + "/01/" + DateTime.Now.Year);
                        ObjectToBeDelete.version_id = SetVersion(ObjectToBeDelete.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, DateTime.Now.Date).version_id;
                    }
                    else
                    {
                        aBLTE.products.Remove(ObjectToBeDelete);
                    }//end if

                    aBLTE.SaveChanges();
                }//end using
            }//end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }//end HttpMethod.DELETE

        [WiMRequiresRole(new string[] { AdminRole, PublisherRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveProductFromAI")]
        public OperationResult RemoveProductFromAI(Int32 entityID, Int32 aiEntityID)
        {
            //No deleting of tables are allowed. An delete call will expire the entity
            user_ loggedInUser;
            product aProd;
            product_active_ingredient aPAI;

            try
            {
                DateTime? thisDate = DateTime.Now;

                if (entityID <= 0 || aiEntityID <= 0)
                { return new OperationResult.BadRequest { }; }

                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (bltEntities aBLTE = GetRDS(securedPassword))
                    {
                        loggedInUser = LoggedInUser(aBLTE);
                        aProd = aBLTE.products.FirstOrDefault(c => c.product_id == entityID);
                        
                        aPAI = aBLTE.product_active_ingredient.FirstOrDefault(c => c.product_id == aProd.product_id && c.active_ingredient_id == aiEntityID);

                        if (aPAI == null)
                        { return new OperationResult.BadRequest(); }
                         //NOTE: ShapeID can not be changed
                        if (aPAI.version.published_time_stamp.HasValue)
                        {
                            //expire the Product-AI relationship
                            aPAI.version = SetVersion(aBLTE, aPAI.version_id, LoggedInUser(aBLTE).user_id, StatusType.Expired, thisDate.Value.Date);
                        }
                        else
                        {
                            aBLTE.product_active_ingredient.Remove(aPAI);
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
        private bool Exists(bltEntities aBLTE, ref product anEntity)
        {
            product existingEntity;
            product thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.products.FirstOrDefault(mt => string.Equals(mt.product_name.ToUpper(), thisEntity.product_name.ToUpper()) &&
                                                                         string.Equals(mt.product_registration_number,thisEntity.product_registration_number));
        

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

        private bool Exists1(bltEntities aBLTE, ref product_active_ingredient anEntity)
        {
            product_active_ingredient existingEntity;
            product_active_ingredient thisEntity = anEntity;
            //check if it exists
            try
            {
                existingEntity = aBLTE.product_active_ingredient.FirstOrDefault(pai => pai.active_ingredient_id == thisEntity.active_ingredient_id && pai.product_id == thisEntity.product_id);


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
            if (aBLTE.products.Count() > 0)
                nextID = aBLTE.products.OrderByDescending(p => p.product_id).First().product_id + 1;

            return nextID;
        }

        private Int32 GetNextPAI_ID(bltEntities aBLTE)
        {
            //create pulaID
            Int32 nextID = 1;
            if (aBLTE.product_active_ingredient.Count() > 0)
                nextID = aBLTE.product_active_ingredient.OrderByDescending(p => p.product_active_ingredient_id).First().product_active_ingredient_id + 1;

            return nextID;
        }

        private void ExpireOtherEntities(bltEntities aBLTE, Int32 userId, product product, DateTime dt)
        {
            //get all published, should only be 1
            List<product> aiList = aBLTE.products.Where(p => p.product_id == product.product_id &&
                                                        p.versions.published_time_stamp <= dt.Date).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                if (!p.Equals(product))
                    p.versions = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }//end ExpireOtherEntities
        protected override void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt)
        {
            //get all published, should only be 1
            List<product> aiList = aBLTE.products.Where(p => p.product_id == Id &&
                                                        p.versions.published_time_stamp <= dt).ToList();
            if (aiList == null) return;

            foreach (var p in aiList)
            {
                p.versions = SetVersion(aBLTE, p.version_id, userId, StatusType.Expired, dt.Date);
            }//next
        }
        #endregion


    }//end class CropUseHandler
}// end namespace