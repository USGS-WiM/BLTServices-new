using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace BLTDB
{
    public partial class bltEntities : DbContext
    {
        public bltEntities(string connectionstring)
            : base(connectionstring)
        {
        }
    }
}
