#region Comments
// 10.10.12 - JB - Created from STN
#endregion
#region Copywright
/* Authors:
 *      Jonathan Baier (jbaier@usgs.gov)
 * Copyright:
 *      2012 USGS - WiM
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using BLTDB;
using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;

namespace BLTServices.Authentication
{
    public class BLTBasicAuthentication : IBasicAuthenticator
    {
        public string Realm
        {
            get { return "BLT-Secured-Domain-Realm"; }
        }


        public AuthenticationResult Authenticate(BasicAuthRequestHeader header)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BLTRDSEntities1"].ConnectionString;
            try
            {
                using (bltEntities aLDSE = new bltEntities(string.Format(connectionString, header.Username, header.Password)))           
                {
                    List<String> managerRoles = new List<string>();
                    List<user_> ManagerList = aLDSE.user_.AsEnumerable()
                                    .Where(u => u.username.ToUpper() == header.Username.ToUpper()).ToList();

                    ManagerList.ForEach(m => managerRoles.Add(m.role.role_name));

                    if (ManagerList.Any())
                    {
                        return new AuthenticationResult.Success(header.Username, managerRoles.ToArray());
                    }
                    else
                    {
                        return new AuthenticationResult.Failed();
                    }

                }//end using
            } catch (EntityException)
            {
                return new AuthenticationResult.Failed();
            }
        }

        //Add public method for decoding base 64 later
        public static BasicAuthRequestHeader ExtractBasicHeader(string value)
        {
            try
            {
                var basicBase64Credentials = value.Split(' ')[1];

                var basicCredentials = Encoding.UTF8.GetString( Convert.FromBase64String( basicBase64Credentials ) ).Split(':');

                if (basicCredentials.Length != 2)
                    return null;

                return new BasicAuthRequestHeader(basicCredentials[0], basicCredentials[1]);
            }
            catch
            {
                return null;
            }

        }//end ExtractBasicHeader

    }//end Class STNBAsicAuthentication

    public enum RoleType
    {
        e_Public = 1,
        e_Enforcer = 2,
        e_Create = 3,
        e_Publish = 4,
        e_Administrator = 5
    }

}