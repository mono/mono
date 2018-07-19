namespace System.Web.UI.WebControls.Expressions {    
    using System;
    using System.Linq;
    using System.Security.Permissions;
    using System.Collections.Generic;

    public class CustomExpressionEventArgs : EventArgs {
        public IQueryable Query { get; set; }
        public IDictionary<string, object> Values { get; private set; }

        public CustomExpressionEventArgs(IQueryable source, IDictionary<string, object> values) {
            Query = source;
            Values = values;
        }
    }
}
