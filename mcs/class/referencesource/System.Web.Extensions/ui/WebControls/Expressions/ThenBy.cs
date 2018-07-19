namespace System.Web.UI.WebControls.Expressions {
    using System.Web.UI.WebControls;
    using System.Security.Permissions;    

    public class ThenBy {
        public string DataField {
            get;
            set; 
        }

        public SortDirection Direction {
            get;
            set;
        }
    }
}
