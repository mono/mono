namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Globalization;

    internal class DataBoundControlParameterTarget : IControlParameterTarget {
        private Control _control;
        public DataBoundControlParameterTarget(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            _control = control;
        }

        public MetaTable Table {
            get {
                return _control.FindMetaTable();
            }
        }

        public MetaColumn FilteredColumn {
            get {
                return null;
            }
        }

        public string GetPropertyNameExpression(string columnName) {
            // Get the DataKeyPropertyAttribute and use that as the to get the correct property name expression
            DataKeyPropertyAttribute attribute = _control.GetType().GetCustomAttributes(true).OfType<DataKeyPropertyAttribute>().FirstOrDefault();
            if ((attribute != null) && !String.IsNullOrEmpty(attribute.Name)) {
                return attribute.Name + String.Format(CultureInfo.InvariantCulture, "['{0}']",  columnName);
            }
            // 
            return String.Empty;
        }
    }
}
