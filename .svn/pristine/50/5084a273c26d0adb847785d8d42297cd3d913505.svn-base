using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.ComponentModel.DataAnnotations;

namespace BLTWeb.Models
{
    public class UsersModel
    {
        [Required]
        public decimal UserID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Organization")]
        public string Organization { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Role")]
        public string Role { get; set; }
    }
}