using System;
using System.Data;
using System.Data.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using System.Linq;
using System.Web;
using System.Text;
using System.Configuration;
using BLTDB;
using BLTServices.Authentication;
using OpenRasta.Web;
using OpenRasta.Security;

namespace BLTServices.Handlers
{
   public abstract class HandlerBase
   {
       #region Constants
       //role constants must match db table
       protected const string AdminRole = "ADMIN";
       protected const string PublisherRole = "PUBLISH";
       protected const string CreatorRole = "CREATE";
       protected const string EnforcerRole = "ENFORCE";
       protected const string Public = "PUBLIC";
       protected const string ReviewRole = "REVIEW";

       #endregion

       #region "Base Properties"
       protected String connectionString = ConfigurationManager.ConnectionStrings["BLTRDSEntities1"].ConnectionString;

       // will be automatically injected by DI in OpenRasta
       public ICommunicationContext Context { get; set; }
    
       public string username
       {
           get { return Context.User.Identity.Name; }
       }

       public abstract string entityName { get; }
        #endregion
       #region Base Queries

       protected IQueryable<T> GetEntities<T>(bltEntities aBLTE) where T : class,new()
       {
           return aBLTE.Set<T>().AsQueryable();
       }//end GetSecuredEntities

       protected IQueryable<active_ingredient_pula> GetActive(IQueryable<active_ingredient_pula> Obj, DateTime thisDate)
       {

           return Obj.Where(p => p.is_published >= 1 &&
                                 (p.effective_date.HasValue && thisDate >= p.effective_date) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<active_ingredient> GetActive(IQueryable<active_ingredient> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<active_ingredient_ai_class> GetActive(IQueryable<active_ingredient_ai_class> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<limitation> GetActive(IQueryable<limitation> Obj, DateTime thisDate)
       {
           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<pula_limitations> GetActive(IQueryable<pula_limitations> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<ai_class> GetActive(IQueryable<ai_class> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<crop_use> GetActive(IQueryable<crop_use> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<product> GetActive(IQueryable<product> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.versions.published_time_stamp.HasValue && thisDate >= p.versions.published_time_stamp) &&
                                 ((!p.versions.expired_time_stamp.HasValue) || thisDate < p.versions.expired_time_stamp));
           
       }//end GetActive
       protected IQueryable<product_active_ingredient> GetActive(IQueryable<product_active_ingredient> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<application_method> GetActive(IQueryable<application_method> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<formulation> GetActive(IQueryable<formulation> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.version.published_time_stamp.HasValue && thisDate >= p.version.published_time_stamp) &&
                                 ((!p.version.expired_time_stamp.HasValue) || thisDate < p.version.expired_time_stamp));

       }//end GetActive
       protected IQueryable<version> GetActive(IQueryable<version> Obj, DateTime thisDate)
       {

           return Obj.Where(p => (p.published_time_stamp.HasValue && thisDate.Date >= p.published_time_stamp) &&
                                 ((!p.expired_time_stamp.HasValue) || thisDate.Date < p.expired_time_stamp));

       }//end GetActive

       #endregion
       #region "Base Methods"
       protected abstract void ExpireEntities(bltEntities aBLTE, Int32 userId, Int32 Id, DateTime dt);
       public bool CanManage()
       {
           try
           {
               if(IsAuthorized(EnforcerRole))
                   return true;

               if (IsAuthorized(PublisherRole))
                   return true;

               if (IsAuthorized(AdminRole))
                   return true;

               if (IsAuthorized(CreatorRole))
                   return true;

               //else
               return false;
           }
           catch (Exception)
           {
               return false;
           }//end try

       }//end CanManager
       public bool IsAuthorized(string role)
       {
           try
           { 
               return Context.User.IsInRole(role);
           }
           catch (Exception)
           {

               return false;
           }//end try
          
       }//end IsAuthorized

       protected version SetVersion(Int32 versionId, Int32 userId, StatusType stype, DateTime timeStamp)
       {
           version newVersion;
           using (EasySecureString securedPassword = GetSecuredPassword())
           {
               using (bltEntities aBLTE = GetRDS(securedPassword))
               {

                  newVersion= SetVersion(aBLTE,versionId, userId, stype, timeStamp);

               }//end using
           }//end using

           return newVersion;
       
       }//end SetVersion
       protected version SetVersion(bltEntities aBLTE, Int32 versionId, Int32 userId, StatusType stype, DateTime timeStamp)
       {           
               if (versionId == null) versionId = -1;

               version newVersion = aBLTE.versions.FirstOrDefault(v => v.version_id == versionId);
            
               switch (stype)
               {
                   case StatusType.Created:

                       newVersion = new version();
                       newVersion.creator_id = userId;
                       newVersion.created_time_stamp = timeStamp.Date;

                       aBLTE.versions.Add(newVersion);
                       break;
                   case StatusType.Reviewed:
                       if (newVersion == null)
                           newVersion = SetVersion(aBLTE, -1, userId, StatusType.Created, DateTime.Today);

                       newVersion.reviewer_id = userId;
                       newVersion.reviewed_time_stamp = timeStamp.Date;
                       break;
                   case StatusType.Published:

                       if (newVersion == null)
                           newVersion = SetVersion(aBLTE, -1, userId, StatusType.Reviewed, DateTime.Today);
                       //do not overwrite
                       if (newVersion.published_time_stamp.HasValue) break;
                       //else
                       newVersion.publisher_id = userId;
                       newVersion.published_time_stamp = timeStamp.Date;
                       break;
                   case StatusType.Expired:
                       if (newVersion.expired_time_stamp.HasValue && 
                           newVersion.published_time_stamp.HasValue ) break;

                       if (newVersion.expired_time_stamp.HasValue && 
                           DateTime.Compare(newVersion.expired_time_stamp.Value, timeStamp) == 0) break;

                       //else
                       newVersion.expirer_id = userId;
                       newVersion.expired_time_stamp = timeStamp.Date;
                       break;
                   default:
                       break;
               }//end switch
                //save
                aBLTE.SaveChanges();
           

           return newVersion;

       }//end SetVersion

       protected user_ LoggedInUser(bltEntities rds)
       {
           return rds.user_.FirstOrDefault(dm => dm.username.ToUpper().Equals(username.ToUpper()));
       }//loggedInUser

       protected bltEntities GetRDS(EasySecureString password)
       {
           return new bltEntities(string.Format(connectionString, Context.User.Identity.Name, password.decryptString()));
       }

       protected bltEntities GetRDS()
       {
           string connectionString = "metadata=res://*/BLTEntities.csdl|res://*/BLTEntities.ssdl|res://*/BLTEntities.msl;provider=Npgsql;provider connection string=';Database=blt;Host=bltnewtest.ck2zppz9pgsw.us-east-1.rds.amazonaws.com;Username={0};PASSWORD={1};ApplicationName=blt';";
        
           //return new bltEntities(string.Format(connectionString, "BLTPUBLIC", "B1TPu673sS"));
           return new bltEntities(string.Format(connectionString, "bltadmin", "1MhTGVxs"));
       }

       protected EasySecureString GetSecuredPassword()
       {
           //return new EasySecureString("B1TMan673sS");
           return new EasySecureString(BLTBasicAuthentication.ExtractBasicHeader(Context.Request.Headers["Authorization"]).Password);
       }

       protected DateTime? ValidDate(string date)
       {
           DateTime tempDate;
           try
           {
               if (date == null) return null;
               if (!DateTime.TryParse(date, out tempDate))
               {
                   //try oadate
                   tempDate = DateTime.FromOADate(Convert.ToDouble(date));
               }


               return tempDate;
               // 
           }
           catch (Exception)
           {

               return null;
           }

       }//end ValidDate

       protected void activateLinks<T>(T anEntity)where T:HypermediaEntity
       {
           if (anEntity != null)
               anEntity.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
       }//end activateLinks
       protected void activateLinks<T>(List<T> EntityList) where T: HypermediaEntity
       {
           if (EntityList != null)
               EntityList.ForEach(x=>x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
       }//end activateLinks

       #endregion
       protected StatusType getStatusType(string statusString)
       {
           try
           {
               return (StatusType)Enum.Parse(typeof(StatusType), statusString);
           }
           catch (Exception)
           {
               switch (statusString.ToUpper().Trim())
               {
                   case "1":
                   case "CREATED":
                       return StatusType.Created;
                   case "2":
                   case "REVIEWED":
                       return StatusType.Reviewed;
                   case "3":
                   case "PUBLISHED":
                       return StatusType.Published;
                   case "4":
                   case "EFFECTIVE":
                       return StatusType.Effective;
                   case "5":
                   case "EXPIRED":
                       return StatusType.Expired;
                   default:
                       return StatusType.Created;
               }//end switch
           }//end try
       
       }//end getStatusType
       protected enum StatusType
       {
           Created = 1,
           Reviewed = 2,
           Published = 3,
           Effective = 4,
           Expired = 5
       }//end enum
    }//end class HandlerBase

}//end namespace