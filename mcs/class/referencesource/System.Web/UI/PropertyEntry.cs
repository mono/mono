//------------------------------------------------------------------------------
// <copyright file="PropertyEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Reflection;

    /// <devdoc>
    /// Base class for all PropertyEntries.
    /// 
    /// PropertyEntry
    ///     BoundPropertyEntry
    ///     BuilderPropertyEntry
    ///         ComplexPropertyEntry
    ///         TemplatePropertyEntry
    ///     SimplePropertyEntry
    /// </devdoc>
    public abstract class PropertyEntry {
        private string _filter;
        private PropertyInfo _propertyInfo;
        private string _name;
        private Type _type;
        private int _index;
        private int _order;

        internal PropertyEntry() {
        }


        /// <devdoc>
        /// </devdoc>
        public string Filter {
            get {
                return _filter;
            }
            set {
                _filter = value;
            }
        }

        // The order of the entry that needs to be sorted.
        internal int Order {
            get {
                return _order;
            }
            set {
                _order = value;
            }
        }

        // The index of the entry declared in persisted format.
        internal int Index {
            get {
                return _index;
            }
            set {
                _index = value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public PropertyInfo PropertyInfo {
            get {
                return _propertyInfo;
            }
            set {
                _propertyInfo = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public Type Type {
            get {
                return _type;
            }
            set {
                _type = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public Type DeclaringType {
            get {
                if (_propertyInfo == null)
                    return null;

                return _propertyInfo.DeclaringType;
            }
        }
    }
}


