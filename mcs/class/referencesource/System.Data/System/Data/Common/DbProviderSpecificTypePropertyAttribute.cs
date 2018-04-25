//------------------------------------------------------------------------------
// <copyright file="DBProviderSupportedClasses.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {
    using System;

    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DbProviderSpecificTypePropertyAttribute : System.Attribute {

        private bool _isProviderSpecificTypeProperty;

        public DbProviderSpecificTypePropertyAttribute(bool isProviderSpecificTypeProperty) {
            _isProviderSpecificTypeProperty = isProviderSpecificTypeProperty;
        }

        public bool IsProviderSpecificTypeProperty {
            get {
                return _isProviderSpecificTypeProperty;
            }
        }
    }
}

