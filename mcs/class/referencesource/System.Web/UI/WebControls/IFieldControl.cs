namespace System.Web.UI.WebControls {
    using System;
    using System.Security.Permissions;

    public interface IFieldControl {
        IAutoFieldGenerator FieldsGenerator {
            get;
            set;
        }
    }
}