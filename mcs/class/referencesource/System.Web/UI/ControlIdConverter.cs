//------------------------------------------------------------------------------
// <copyright file="ControlIDConverter.cs" company="Microsoft">
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
    /// TypeConverter for ControlParameter's ControlID property.
    /// </devdoc>
    public class ControlIDConverter : StringConverter {


        /// <devdoc>
        ///    <para>Determines whether a given control should have its id added to the StandardValuesCollection.</para>
        /// </devdoc>
        protected virtual bool FilterControl(Control control) {
            return true;
        }

        /// <devdoc>
        /// Returns a list of all control IDs in the container.
        /// </devdoc>
        private string[] GetControls(IDesignerHost host, object instance) {
            IContainer container = host.Container;

            // Locate nearest container
            IComponent component = instance as IComponent;
            if (component != null && component.Site != null) {
                container = component.Site.Container;
            }

            if (container == null) {
                return null;
            }
            ComponentCollection allComponents = container.Components;
            ArrayList array = new ArrayList();

            // For each control in the container
            foreach (IComponent comp in (IEnumerable)allComponents) {
                Control control = comp as Control;
                // Ignore DesignerHost.RootComponent (Page or UserControl), controls that don't have ID's, 
                // and the Control itself
                if (control != null &&
                    control != instance && 
                    control != host.RootComponent &&
                    control.ID != null && 
                    control.ID.Length > 0 &&
                    FilterControl(control)) {
                    array.Add(control.ID);
                }
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

            IDesignerHost host = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            Debug.Assert(host != null, "Unable to get IDesignerHost in ControlIDConverter");

            if (host != null) {
                string[] controlIDs = GetControls(host, context.Instance);
                if (controlIDs == null) {
                    return null;
                }
                return new StandardValuesCollection(controlIDs);
            }

            return null;
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

