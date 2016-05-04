//------------------------------------------------------------------------------
// <copyright file="FormConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web.UI;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Can filter and retrieve several types of values from Style controls.
    ///    </para>
    /// </summary>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class FormConverter : StringConverter
    {
        private Form GetContainingForm(MobileControl mc)
        {
            return FindContainer(mc, typeof(Form)) as Form;
        }

        private StyleSheet GetContainingStyleSheet(MobileControl mc)
        {
            return FindContainer(mc, typeof(StyleSheet)) as StyleSheet;
        }

        private Control FindContainer(MobileControl mc, Type containerType)
        {
            for (Control control = mc; control != null; control = control.Parent)
            {
                if (containerType.IsAssignableFrom(control.GetType()))
                {
                    return control;
                }
            }
            return null;
        }

        protected virtual ArrayList GetControls(ITypeDescriptorContext context)
        {
            ArrayList       controlList = new ArrayList();
            MobileControl   control = null;
            IContainer      container = context.Container;

            if (context.Instance is Array)
            {
                Array list = (Array)context.Instance;
                Debug.Assert(list.Length > 0);

                foreach(Object obj in list)
                {
                    Debug.Assert(obj is MobileControl);
                    Form form = GetContainingForm((MobileControl)obj);

                    // If the control is not within a Form control or a StyleSheet control,
                    // simply return the empty combobox.
                    // If the control is placed at UserControl top level, the ID of the 
                    // containing Form is null.
                    if ((form == null || form.ID == null) && 
                        GetContainingStyleSheet((MobileControl)obj) == null)
                    {
                        return null;
                    }
                }
                control = list.GetValue(0) as MobileControl;
            }
            else
            {
                if (context.Instance is MobileControl)
                {
                    control = (MobileControl) context.Instance;
                }
                else if (context.Instance is ChoicePropertyFilter)
                {
                    ChoicePropertyFilter filter = (ChoicePropertyFilter)context.Instance;
                    IDeviceSpecificDesigner designer = filter.Designer;
                    control = designer.UnderlyingObject as MobileControl;
                    Debug.Assert(control != null, "Not a control");
                }
                else
                {
                    Debug.Fail("Unrecognized object passed in");
                    return null;
                }

                Form form = GetContainingForm(control);

                // All controls must be contained within Forms or StyleSheets
                // Show empty combobox for the invalid control.
                if (form == null)
                {
                    if (GetContainingStyleSheet(control) == null)
                    {
                        return null;
                    }
                }
                // MobileUserControl has a default Form with null ID
                else if (form.ID == null && (GetContainingStyleSheet(control) == null))
                {
                    Debug.Assert(container is IDesignerHost &&
                        ((IDesignerHost)container).RootComponent is MobileUserControl);

                    // Just return an empty array list, so that url picker still works.
                    return controlList;
                }
            }

            // If container is null, try to get one from control's IContainer
            if (container == null)
            {
                ISite site = control.Site;
                Debug.Assert(site != null);

                container = site.Container;
            }

            // Is this possible?
            if (container == null)
            {
                Debug.Fail("container is null");
                return null;
            }

            foreach(IComponent component in container.Components)
            {
                Form candidate = component as Form;
                if (candidate != null &&
                    candidate.ID != null &&
                    candidate.ID.Length != 0)
                {
                    controlList.Add(ProcessControlId(candidate.ID));
                }
            }

            controlList.Sort();
            return controlList;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null)
            {
                return null;
            }

            ArrayList objValues = GetControls(context);
            return (objValues != null? new StandardValuesCollection(objValues) : null);
        }

        /// <summary>
        ///    <para>
        ///       Gets whether
        ///       or not the context specified contains exclusive standard values.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    A type descriptor that indicates the context to convert from.
        /// </param>
        /// <returns>
        ///    <para>
        ///    <see langword='true'/> if the specified context contains exclusive standard 
        ///       values, otherwise <see langword='false'/>.
        ///    </para>
        /// </returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        /// <summary>
        ///    <para>
        ///       Gets whether or not the specified context contains supported standard
        ///       values.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    A type descriptor that indicates the context to convert from.
        /// </param>
        /// <returns>
        ///    <para>
        ///    <see langword='true'/> if the specified context conatins supported standard 
        ///       values, otherwise <see langword='false'/>.
        ///    </para>
        /// </returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        ///     Override to manipulate the control id as it is added to the list.
        ///     Do not return the original string, make sure a copy is made.
        ///     See NavigateUrlConverter.cs for an example.
        /// </summary>
        protected virtual String ProcessControlId(String id)
        {
            return id;
        }
    }
}
