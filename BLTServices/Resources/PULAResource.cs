//------------------------------------------------------------------------------
//-----  PULAResource ----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   PULA resources.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              PULAResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 05.20.13 - jkn - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BLTDB;
using System.Xml.Serialization;

namespace BLTServices.Resources
{    
    public class PULAList
    {        
        public List<SimplePULA> PULA { get; set; }
    }//end PULAList

    public class SimplePULA:PULABase
    {        
        public DateTime? Created { get; set; }
        public bool ShouldSerializeCreated()
        { return Created.HasValue; }

        public DateTime? Published { get; set; }
        public bool ShouldSerializePublished()
        { return Published.HasValue; }

        public DateTime? Effective { get; set; }
        public bool ShouldSerializeEffective()
        { return Effective.HasValue; }

        public DateTime? Expired { get; set; }
        public bool ShouldSerializeExpired()
        { return Expired.HasValue; }

    }//end class simplePULA

    public class PULABase
    {        
        public decimal entityID { get; set; }        
        public decimal ShapeID { get; set; }
        public decimal isPublished { get; set; }

    }//end class simplePULA

    public class MapperLimitations
    {      
        public List<MapperLimit> MapperLimits { get; set; }
    }//end MapperLimitations

    public class MapperLimit
    {        
        public Int32 PULAID { get; set; }
        public Int32 PULASHPID { get; set; }
        public string NAME { get; set; }
        public string USE { get; set; }
        public string APPMETHOD { get; set; }
        public string FORM { get; set; }
        public limitation LIMIT { get; set; }
    }//end class simplePULA

    public class contributorpulaview
    {
        public Int32 id { get; set; }
        public string comments { get; set; }
        
    }//end class contributorpulaview
}//end namespace