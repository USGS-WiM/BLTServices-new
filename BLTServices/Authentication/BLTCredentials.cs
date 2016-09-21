using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenRasta.Security;

namespace BLTServices.Authentication
{
    public class BLTCredentials: Credentials
    {
        public BLTCredentials() { }

        public string salt { get; set; }
    }
}