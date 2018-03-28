//------------------------------------------------------------------------------
// <copyright file="ViewStateModeByIdAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ViewStateModeByIdAttribute : Attribute {
        static Hashtable _viewStateIdTypes = Hashtable.Synchronized(new Hashtable());

        public ViewStateModeByIdAttribute() {
        }

        internal static bool IsEnabled(Type type) {
            if (!_viewStateIdTypes.ContainsKey(type)) {
                System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes(type);
                ViewStateModeByIdAttribute attr = (ViewStateModeByIdAttribute)attrs[typeof(ViewStateModeByIdAttribute)];
                _viewStateIdTypes[type] = (attr != null);
            }
            return (bool)_viewStateIdTypes[type];
        }
    }
}

