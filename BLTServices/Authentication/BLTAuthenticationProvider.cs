using System;
using System.Linq;
using System.Data.Entity;

using System.Configuration;
using OpenRasta.Security;
using BLTServices.Security;
using BLTDB;
namespace BLTServices.Authentication
{
    public class BLTAuthenticationProvider:IAuthenticationProvider
    {

        public Credentials GetByUsername(string username)
        {
            string connectionString = "metadata=res://*/BLTEntities.csdl|res://*/BLTEntities.ssdl|res://*/BLTEntities.msl;provider=Npgsql;provider connection string=';Database=blt;Host=bltnew.ck2zppz9pgsw.us-east-1.rds.amazonaws.com;Username={0};PASSWORD={1};ApplicationName=blt';";

            using (bltEntities sa = new bltEntities(string.Format(connectionString, "bltadmin", "1MhTGVxs")))
            {
                user_ user = sa.user_.Include(r => r.role).AsEnumerable().FirstOrDefault(u => string.Equals(u.username, username, StringComparison.OrdinalIgnoreCase));
                if (user == null) return (null);
                return (new BLTCredentials()
                {
                    Username = user.username,
                    salt = user.salt,
                    Password = user.password,
                    Roles = new string[] { user.role.role_name }
                });
            }//end using            
        }

        public bool ValidatePassword(Credentials credentials, string suppliedPassword)
        {
            if (credentials == null) return (false);
            BLTCredentials creds = (BLTCredentials)credentials;
            bool authenticated = Cryptography.VerifyPassword(suppliedPassword, creds.salt, creds.Password);
            return authenticated;
        }       
    }
    public enum RoleType
        {
            e_Public = 1,
            e_Enforcer = 2,
            e_Create = 3,
            e_Publish = 4,
            e_Administrator = 5
        }
}