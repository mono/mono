//------------------------------------------------------------------------------
// <copyright file="PersonalizableTypeEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;

    /// <devdoc>
    /// Used to represent a type that has personalizable properties
    /// and cache information about it.
    /// </devdoc>
    internal sealed class PersonalizableTypeEntry {

        private Type _type;
        private IDictionary _propertyEntries;
        private PropertyInfo[] _propertyInfos;

        public PersonalizableTypeEntry(Type type) {
            _type = type;
            InitializePersonalizableProperties();
        }

        public IDictionary PropertyEntries {
            get {
                return _propertyEntries;
            }
        }

        public ICollection PropertyInfos {
            get {
                if (_propertyInfos == null) {
                    PropertyInfo[] propertyInfos = new PropertyInfo[_propertyEntries.Count];

                    int i = 0;
                    foreach (PersonalizablePropertyEntry entry in _propertyEntries.Values) {
                        propertyInfos[i] = entry.PropertyInfo;
                        i++;
                    }

                    // Set field after the values have been computed, so field will not be cached
                    // if an exception is thrown.
                    _propertyInfos = propertyInfos;
                }

                return _propertyInfos;
            }
        }

        private void InitializePersonalizableProperties() {
            _propertyEntries = new HybridDictionary(/* caseInsensitive */ false);

            // Get all public and non-public instance properties, including those declared on
            // base types.
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo[] props = _type.GetProperties(flags);

            // Sorts PropertyInfos according to their DeclaringType.  Base types appear before derived types.
            Array.Sort(props, new DeclaringTypeComparer());

            // For each PropertyInfo, add it to the dictionary if it is personalizable, else remove
            // it from the dictionary.  We need to remove it from the dictionary, in case the base
            // type declared a valid personalizable property of the same name (VSWhidbey 237437).
            if ((props != null) && (props.Length != 0)) {
                for (int i = 0; i < props.Length; i++) {
                    PropertyInfo pi = props[i];
                    string name = pi.Name;

                    // Get the PersonalizableAttribute (and include any inherited metadata)
                    PersonalizableAttribute pa = Attribute.GetCustomAttribute(pi,
                      PersonalizableAttribute.PersonalizableAttributeType, true) as PersonalizableAttribute;

                    // If the property is not personalizable, remove it from the dictionary
                    if (pa == null || !pa.IsPersonalizable) {
                        _propertyEntries.Remove(name);
                        continue;
                    }

                    // If the property has parameters, or does not have a public get or set
                    // accessor, throw an exception.
                    ParameterInfo[] paramList = pi.GetIndexParameters();
                    if ((paramList != null && paramList.Length > 0) || pi.GetGetMethod() == null || pi.GetSetMethod() == null) {
                        throw new HttpException(SR.GetString(SR.PersonalizableTypeEntry_InvalidProperty, name, _type.FullName));
                    }

                    // Add the property to the dictionary
                    _propertyEntries[name] = new PersonalizablePropertyEntry(pi, pa);
                }
            }
        }

        // Sorts PropertyInfos according to their DeclaringType.  Base types appear before derived types.
        private sealed class DeclaringTypeComparer : IComparer {
            public int Compare(Object x, Object y) {
                Type declaringTypeX = ((PropertyInfo)x).DeclaringType;
                Type declaringTypeY = ((PropertyInfo)y).DeclaringType;

                if (declaringTypeX == declaringTypeY) {
                    return 0;
                }
                else if (declaringTypeX.IsSubclassOf(declaringTypeY)) {
                    return 1;
                }
                else {
                    return -1;
                }
            }
        }
    }
}
