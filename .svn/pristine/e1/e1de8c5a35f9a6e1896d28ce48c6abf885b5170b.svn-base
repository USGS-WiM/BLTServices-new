//------------------------------------------------------------------------------
//----- AuthorizeRoleAttribute.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Attribute to allow authorization and role requirement. (i.e. [Authorize("Admin")]
//
//discussion:   
//
//     

#region Comments
// 10.11.12 - JB - Created 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using BLTServices.Handlers;
using BLTWeb.Utilities;

namespace BLTWeb.Providers
{
    public sealed class RoleAuthorize : AuthorizeAttribute
    {
        readonly List<string> _roleNames = new List<string>();

        public RoleAuthorize()
        {
                _roleNames.Add("Public");
        }

        public RoleAuthorize(string aRole)
        {
            if (aRole == null) throw new ArgumentNullException("roleName");
            
            _roleNames.Add(aRole);
        }

        public RoleAuthorize(string[] roles)
        {
            foreach (string role in roles)
            {
                _roleNames.Add(role);

            }//next

            if (_roleNames.Count < 0) throw new ArgumentNullException("roleNames");
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            BLTServiceCaller serviceCaller = BLTServiceCaller.Instance;
            if ((serviceCaller.CurrentRole != null) && _roleNames.Contains(serviceCaller.CurrentRole.ROLE_NAME))
            {
                base.OnAuthorization(filterContext);
            }
            else {
                base.HandleUnauthorizedRequest(filterContext);
            }

        }



    }
}