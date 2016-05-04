//------------------------------------------------------------------------------
// <copyright file="SupportsEventValidation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class SupportsEventValidationAttribute : Attribute {

        private static Hashtable _typesSupportsEventValidation;        

        static SupportsEventValidationAttribute() {
            // Create a synchronized wrapper
            _typesSupportsEventValidation = Hashtable.Synchronized(new Hashtable());
        }

        public SupportsEventValidationAttribute() {
        }

        internal static bool SupportsEventValidation(Type type) {
            object result = _typesSupportsEventValidation[type];
            if (result != null) {
                return (bool)result;
            }

            // Check the attributes on the type to see if it supports SupportsEventValidationAttribute
            // Note that this attribute does not inherit from the base class, since derived classes may 
            // not be able to validate properly.
            object[] attribs = type.GetCustomAttributes(typeof(SupportsEventValidationAttribute), false /* inherits */);
            bool supportsEventValidation = ((attribs != null) && (attribs.Length > 0));
            _typesSupportsEventValidation[type] = supportsEventValidation;

            return supportsEventValidation;
        }
    }
}
 
