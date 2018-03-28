//------------------------------------------------------------------------------
// <copyright file="DataBoundControlHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.Util;

    /// <devdoc>
    /// Helper class for DataBoundControls and v1 data controls.
    /// This is also used by ControlParameter to find its associated
    /// control.
    /// </devdoc>
    internal static class DataBoundControlHelper {

        private static MethodInfo s_enableDynamicDataMethod;

        /// <devdoc>
        /// Walks up the stack of NamingContainers starting at 'control' to find a control with the ID 'controlID'.
        /// Important : Note that the search is never done on the 'control' itself by this method.
        /// </devdoc>
        public static Control FindControl(Control control, string controlID) {
            Debug.Assert(control != null, "control should not be null");
            Debug.Assert(!String.IsNullOrEmpty(controlID), "controlID should not be empty");
            Control currentContainer = control;
            Control foundControl = null;

            if (control == control.Page) {
                // If we get to the Page itself while we're walking up the
                // hierarchy, just return whatever item we find (if anything)
                // since we can't walk any higher.
                return control.FindControl(controlID);
            }

            while (foundControl == null && currentContainer != control.Page) {
                currentContainer = currentContainer.NamingContainer;
                if (currentContainer == null) {
                    throw new HttpException(SR.GetString(SR.DataBoundControlHelper_NoNamingContainer, control.GetType().Name, control.ID));
                }
                foundControl = currentContainer.FindControl(controlID);
            }

            return foundControl;
        }

        /// <devdoc>
        // return true if the two string arrays have the same members
        /// </devdoc>
        public static bool CompareStringArrays(string[] stringA, string[] stringB) {
            if (stringA == null && stringB == null) {
                return true;
            }

            if (stringA == null || stringB == null) {
                return false;
            }
            
            if (stringA.Length != stringB.Length) {
                return false;
            }

            for (int i = 0; i < stringA.Length; i++) {
                if (!String.Equals(stringA[i], stringB[i], StringComparison.Ordinal)) {
                    return false;
                }
            }
            return true;
        }

        //The enableEnums parameter is introduced for backward comapatibility (false for compatible with older versions).
        internal static bool IsBindableType(Type type, bool enableEnums) {
            if (type == null) {
                return false;
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null) {
                // If the type is Nullable then it has an underlying type, in which case
                // we want to check the underlying type for bindability.
                type = underlyingType;
            }

            if (type.IsPrimitive ||
                   (type == typeof(string)) ||
                   (type == typeof(DateTime)) ||
                   (type == typeof(Decimal)) ||
                   (type == typeof(Guid)) ||
                // support for new SqlServer 2008 types:
                   (type == typeof(DateTimeOffset)) ||
                   (type == typeof(TimeSpan))) {
                return true;
            }
            else {
                BindableTypeAttribute bindableTypeAttribute = (BindableTypeAttribute)TypeDescriptor.GetAttributes(type)[typeof(BindableTypeAttribute)];
                if (bindableTypeAttribute != null) {
                    return bindableTypeAttribute.IsBindable;
                }
                else {
                    //We consider enums as Bindable types by default but provide an opt-out mechanism using BindableTypeAttribute. (Ex : EntityState)
                    //So the order of above if-else block is important.
                    return (enableEnums && type.IsEnum);
                }
            }
        }

        internal static void ExtractValuesFromBindableControls(IOrderedDictionary dictionary, Control container) {
            IBindableControl bindableControl = container as IBindableControl;
            if (bindableControl != null) {
                bindableControl.ExtractValues(dictionary);
            }
            foreach (Control childControl in container.Controls) {
                ExtractValuesFromBindableControls(dictionary, childControl);
            }
        }

        internal static void EnableDynamicData(INamingContainer control, string entityTypeName) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            if (s_enableDynamicDataMethod == null) {
                Type dataControlExtensionsType = Assembly.Load(AssemblyRef.SystemWebDynamicData).GetType("System.Web.UI.DataControlExtensions");
                s_enableDynamicDataMethod = dataControlExtensionsType.GetMethod("EnableDynamicData",
                                                                              BindingFlags.Public | BindingFlags.Static,
                                                                              binder: null,
                                                                              types: new Type[] { typeof(INamingContainer), typeof(Type) },
                                                                              modifiers: null);
            }
            Debug.Assert(s_enableDynamicDataMethod != null);

            Type entityType = BuildManager.GetType(entityTypeName, throwOnError: false);
            if (entityType != null) {
                s_enableDynamicDataMethod.Invoke(obj: null,
                                               parameters: new object[] { control, entityType });
            }
        }
    }
}

