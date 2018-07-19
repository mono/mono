//------------------------------------------------------------------------------
// <copyright file="ControlPropertyNameConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Util;
    using System.Security.Permissions;


    /// <devdoc>
    /// TypeConverter for ControlParameter's PropertyName property.
    /// </devdoc>
    public class ControlPropertyNameConverter : StringConverter {

        /// <devdoc>
        /// Returns a list of all the propery names for a given control.
        /// </devdoc>
        private string[] GetPropertyNames(Control control) {

            ArrayList array = new ArrayList();

            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(control.GetType());

            foreach (PropertyDescriptor desc in pdc) {
                array.Add(desc.Name);
            }

            array.Sort(Comparer.Default);

            return (string[])array.ToArray(typeof(string));
        }


        /// <devdoc>
        /// Returns a collection of standard values retrieved from the context specified
        /// by the specified type descriptor.
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (context == null) {
                return null;
            }

            // Get ControlID
            ControlParameter param = (ControlParameter)context.Instance;
            string controlID = param.ControlID;

            // Check that we actually have a control ID
            if (String.IsNullOrEmpty(controlID))
                return null;

            // Get designer host
            IDesignerHost host = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            Debug.Assert(host != null, "Unable to get IDesignerHost in ControlPropertyNameConverter");

            if (host == null)
                return null;

            // Get control
            ComponentCollection allComponents = host.Container.Components;

            Control control = allComponents[controlID] as Control;

            if (control == null)
                return null;

            string[] propertyNames = GetPropertyNames(control);

            return new StandardValuesCollection(propertyNames);
        }


        /// <devdoc>
        /// Gets whether or not the context specified contains exclusive standard values.
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return false;
        }


        /// <devdoc>
        /// Gets whether or not the specified context contains supported standard values.
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return (context != null);
        }
    }
}

