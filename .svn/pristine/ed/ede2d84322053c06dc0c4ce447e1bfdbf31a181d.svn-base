using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

using BLTServices;

namespace BLTWeb.Models
{
    public class PULALimitation
    {

        [DataType(DataType.Text)]
        [Display(Name = "PulaLimitID")]
        public string PulaLimitID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "AI")]
        public string AI { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "AI_ID")]
        public string AI_ID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Product")]
        public string Product { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Prod_ID")]
        public string Prod_ID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Prod_RegNum")]
        public string Prod_RegNum { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Application Method")]
        public string AppMethod { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "AppMeth_ID")]
        public string AppMeth_ID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Formulation")]
        public string Formulation { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Form_ID")]
        public string Form_ID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Crop Use")]
        public string CropUse { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "CropUse_ID")]
        public string CropUse_ID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Code_ID")]
        public string Code_ID { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Limitation")]
        public string Limitation { get; set; }

        //[DataType(DataType.Text)]
        //[Display(Name = "Species")]
        //public string Species { get; set; }

    }

    public class PULA_Model
    {
        public ACTIVE_INGREDIENT_PULA anAIPULA { get; set; }
        public string CommentName { get; set; }
        public string CommentOrg { get; set; }
        public string Comment { get; set; }
        public string EffMonths { get; set; }
        public string EffYears { get; set; }
        public string ExMonths { get; set; }
        public string ExYears { get; set; }
        public string ExpirationChanged { get; set; }
        public string SpeciesToAdd { get; set; }
        public string SpeciesToRemove { get; set; }
        public string LimitationsToAdd { get; set; }
        public string ExistingLimitToRemove { get; set; }
    }

    public class AIModel
    {
        public ACTIVE_INGREDIENT AI { get; set; }
        public string AIProdsToAdd { get; set; }
        public string AIClassesToAdd { get; set; }
        public string ProductIDsToRemove { get; set; }
        public string AIClassesIDsToRemove { get; set; }
    }

    public class CommentsModel
    {
        public string Name { get; set; }
        public string Org { get; set; }
        public string Comment { get; set; }
    }
}
