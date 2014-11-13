//------------------------------------------------------------------------------
// <copyright file="PersonalizablePropertyEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Reflection;

    /// <devdoc>
    /// Represents a property that has been marked as personalizable
    /// </devdoc>
    internal sealed class PersonalizablePropertyEntry {

        private PropertyInfo _propertyInfo;
        private PersonalizationScope _scope;
        private bool _isSensitive;

        public PersonalizablePropertyEntry(PropertyInfo pi, PersonalizableAttribute attr) {
            _propertyInfo = pi;
            _scope = attr.Scope;
            _isSensitive = attr.IsSensitive;
        }

        public bool IsSensitive {
            get {
                return _isSensitive;
            }
        }

        public PersonalizationScope Scope {
            get {
                return _scope;
            }
        }

        public PropertyInfo PropertyInfo {
            get {
                return _propertyInfo;
            }
        }
    }
}
