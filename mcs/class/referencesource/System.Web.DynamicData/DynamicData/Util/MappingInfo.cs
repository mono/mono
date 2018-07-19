namespace System.Web.DynamicData.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    // Stores per request mapping data
    internal class MappingInfo {
        public MetaTable Table { get; set; }
        public DefaultValueMapping DefaultValueMapping { get; set; }
    }  
}
