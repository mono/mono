//------------------------------------------------------------------------------
// <copyright file="ValidatedMobileControlConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Converters
{
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.MobileControls;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ValidatedMobileControlConverter: StringConverter
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

        protected virtual Object [] GetValidatableControls(Object instance)
        {
            System.Web.UI.MobileControls.BaseValidator thisValidator = null;

            if (instance is Array)
            {
                Array controlList = (Array)instance;
                Debug.Assert(controlList.Length > 0);

                thisValidator = (BaseValidator) controlList.GetValue(0);
                Form firstForm = GetContainingForm(thisValidator);

                for (int i = 1; i < controlList.Length; i++)
                {
                    BaseValidator validator = (BaseValidator)controlList.GetValue(i);
                    if (GetContainingForm(validator) != firstForm)
                    {
                        return null;
                    }
                }
            }

            if (instance is System.Web.UI.MobileControls.BaseValidator)
            {
                thisValidator = (System.Web.UI.MobileControls.BaseValidator) instance;
            }
            else if (instance is ChoicePropertyFilter)
            {
                IDeviceSpecificDesigner designer = 
                    ((ChoicePropertyFilter)instance).Designer;

                thisValidator = designer.UnderlyingObject 
                    as System.Web.UI.MobileControls.BaseValidator;
            }

            if (thisValidator == null)
            {
                Debug.Fail("Unsupported object passed in");
                return null;
            }

            ArrayList controlArray = new ArrayList();

            if (GetContainingStyleSheet(thisValidator) != null)
            {
                ISite site = thisValidator.Site;
                IContainer container = null;

                if (site != null)
                {
                    container = site.Container;
                    Debug.Assert(container != null);

                    foreach(IComponent component in container.Components)
                    {
                        Control control = component as Control;
                        if (control != null && CanBeValidated(control))
                        {
                            controlArray.Add(control.ID);
                        }
                    }
                }
            }
            else
            {
                Form parentForm = GetContainingForm(thisValidator);

                if (parentForm != null)
                {
                    ExtractValidatableControls(parentForm, controlArray);
                }
                else
                {
                    return null;
                }
            }

            controlArray.Sort();
            return controlArray.ToArray();
        }

        private void ExtractValidatableControls(Control parent, ArrayList controlArray)
        {
            foreach (Control control in parent.Controls)
            {
                if (CanBeValidated(control))
                {
                    controlArray.Add(control.ID);
                }
                if (!(control is Form))
                {
                    ExtractValidatableControls(control, controlArray);
                }
            }
        }

        private bool CanBeValidated(Control control)
        {
            // Control must have an ID
            if (control.ID == null || control.ID.Length == 0)
            {
                return false;
            }

            // Control must have a ValidationProperty attribute
            ValidationPropertyAttribute valProp = 
                (ValidationPropertyAttribute) 
                TypeDescriptor.GetAttributes(control)[typeof(ValidationPropertyAttribute)];

            if (null != valProp && null != valProp.Name)
            {
                return true;    
            }

            return false;
        }

        /// <summary>
        ///    <para>
        ///       Returns a collection of standard values retrieved from the context specified
        ///       by the specified type descriptor.
        ///    </para>
        /// </summary>
        /// <param name='context'>
        ///    A type descriptor that specifies the location of the context to convert from.
        /// </param>
        /// <returns>
        ///    <para>
        ///       A StandardValuesCollection that represents the standard values collected from
        ///       the specified context.
        ///    </para>
        /// </returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null || context.Instance == null)
            {
                return null;
            }

            Object [] objValues = GetValidatableControls(context.Instance);
            if (objValues != null)
            {
                return new StandardValuesCollection(objValues);
            }
            else
            {
                return null;
            }            
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
    }    
}
