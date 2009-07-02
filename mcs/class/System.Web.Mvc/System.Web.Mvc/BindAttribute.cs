/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class BindAttribute : Attribute {

        private string _exclude;
        private string[] _excludeSplit = new string[0];
        private string _include;
        private string[] _includeSplit = new string[0];

        public string Exclude {
            get {
                return _exclude ?? String.Empty;
            }
            set {
                _exclude = value;
                _excludeSplit = AuthorizeAttribute.SplitString(value);
            }
        }

        public string Include {
            get {
                return _include ?? String.Empty;
            }
            set {
                _include = value;
                _includeSplit = AuthorizeAttribute.SplitString(value);
            }
        }

        public string Prefix {
            get;
            set;
        }

        internal static bool IsPropertyAllowed(string propertyName, string[] includeProperties, string[] excludeProperties) {
            // We allow a property to be bound if its both in the include list AND not in the exclude list.
            // An empty include list implies all properties are allowed.
            // An empty exclude list implies no properties are disallowed.
            bool includeProperty = (includeProperties == null) || (includeProperties.Length == 0) || includeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            bool excludeProperty = (excludeProperties != null) && excludeProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
            return includeProperty && !excludeProperty;
        }

        public bool IsPropertyAllowed(string propertyName) {
            return IsPropertyAllowed(propertyName, _includeSplit, _excludeSplit);
        }
    }
}
