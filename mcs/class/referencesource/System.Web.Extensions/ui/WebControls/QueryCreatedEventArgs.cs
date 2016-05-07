namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Permissions;

    public class QueryCreatedEventArgs : EventArgs {
        public IQueryable Query { get; set; }

        public QueryCreatedEventArgs(IQueryable query) {
            Query = query;
        }
    }
}
