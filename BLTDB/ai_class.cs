//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BLTDB
{
    using System;
    using System.Collections.Generic;
    
    public partial class ai_class
    {
        public int ai_class_id { get; set; }
        public string ai_class_name { get; set; }
        public int version_id { get; set; }
        public int id { get; set; }
    
        public virtual version version { get; set; }
    }
}
