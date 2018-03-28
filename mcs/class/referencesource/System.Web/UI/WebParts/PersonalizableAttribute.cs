//------------------------------------------------------------------------------
// <copyright file="PersonalizableAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    /// <devdoc>
    /// Used to mark a property as personalizable.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PersonalizableAttribute : Attribute {

        internal static readonly Type PersonalizableAttributeType = typeof(PersonalizableAttribute);

        private static readonly IDictionary PersonalizableTypeTable = Hashtable.Synchronized(new Hashtable());

        /// <devdoc>
        /// Indicates that the property is not personalizable.
        /// </devdoc>
        public static readonly PersonalizableAttribute NotPersonalizable = new PersonalizableAttribute(false);

        /// <devdoc>
        /// Indicates that the property is personalizable.
        /// </devdoc>
        public static readonly PersonalizableAttribute Personalizable = new PersonalizableAttribute(true);

        /// <devdoc>
        /// Indicates that the property is personalizable and can be changed per user.
        /// </devdoc>
        public static readonly PersonalizableAttribute UserPersonalizable = new PersonalizableAttribute(PersonalizationScope.User);

        /// <devdoc>
        /// Indicates that the property is personalizable that can only be changed for all users.
        /// </devdoc>
        public static readonly PersonalizableAttribute SharedPersonalizable = new PersonalizableAttribute(PersonalizationScope.Shared);

        /// <devdoc>
        /// The default personalizable attribute for a property or type. The default
        /// is to indicate that a property or type is not personalizable.
        /// </devdoc>
        public static readonly PersonalizableAttribute Default = NotPersonalizable;

        private bool _isPersonalizable;
        private bool _isSensitive;
        private PersonalizationScope _scope;

        /// <devdoc>
        /// Initializes an instance of PersonalizableAttribute to indicate
        /// a personalizable property.
        /// By default personalized properties can be personalized per user.
        /// </devdoc>
        public PersonalizableAttribute() : this(true, PersonalizationScope.User, false) {
        }

        /// <devdoc>
        /// Initializes an instance of PersonalizableAttribute with the
        /// specified value.
        /// </devdoc>
        public PersonalizableAttribute(bool isPersonalizable) : this(isPersonalizable, PersonalizationScope.User, false) {
        }

        /// <devdoc>
        /// Initializes an instance of PersonalizableAttribute to indicate
        /// a personalizable property along with the specified personalization scope.
        /// </devdoc>
        public PersonalizableAttribute(PersonalizationScope scope) : this(true, scope, false) {
        }

        /// <devdoc>
        /// Initializes an instance of PersonalizableAttribute to indicate
        /// a personalizable property along with the specified personalization scope and sensitivity.
        /// </devdoc>
        public PersonalizableAttribute(PersonalizationScope scope, bool isSensitive) : this(true, scope, isSensitive) {
        }

        /// <internalonly/>
        /// <devdoc>
        /// Initializes an instance of PersonalizableAttribute with the specified values.
        /// </devdoc>
        private PersonalizableAttribute(bool isPersonalizable, PersonalizationScope scope, bool isSensitive) {
            Debug.Assert((isPersonalizable == true || isSensitive == false), "Only Personalizable properties can be sensitive");
            _isPersonalizable = isPersonalizable;
            _isSensitive = isSensitive;
            if (_isPersonalizable) {
                _scope = scope;
            }
        }

        /// <devdoc>
        /// Whether the property or the type has been marked as personalizable.
        /// </devdoc>
        public bool IsPersonalizable {
            get {
                return _isPersonalizable;
            }
        }

        /// <devdoc>
        /// Whether the property or the type has been marked as sensitive.
        /// </devdoc>
        public bool IsSensitive {
            get {
                return _isSensitive;
            }
        }

        /// <devdoc>
        /// The personalization scope associated with the personalizable property.
        /// This property only has meaning when IsPersonalizable is true.
        /// </devdoc>
        public PersonalizationScope Scope {
            get {
                return _scope;
            }
        }

        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            PersonalizableAttribute other = obj as PersonalizableAttribute;
            if (other != null) {
                return (other.IsPersonalizable == IsPersonalizable) &&
                       (other.Scope == Scope) &&
                       (other.IsSensitive == IsSensitive);
            }

            return false;
        }

        /// <internalonly/>
        public override int GetHashCode() {
            return HashCodeCombiner.CombineHashCodes(_isPersonalizable.GetHashCode(), _scope.GetHashCode(),
                                                     _isSensitive.GetHashCode());
        }

        /// <devdoc>
        /// Returns the list of personalizable properties as a collection of
        /// PropertyInfos for the specified type.
        /// </devdoc>
        public static ICollection GetPersonalizableProperties(Type type) {
            Debug.Assert(type != null);

            PersonalizableTypeEntry typeEntry = (PersonalizableTypeEntry)PersonalizableTypeTable[type];
            if (typeEntry == null) {
                typeEntry = new PersonalizableTypeEntry(type);
                PersonalizableTypeTable[type] = typeEntry;
            }

            return typeEntry.PropertyInfos;
        }

        /// <devdoc>
        /// Returns the list of personalizable properties as a collection of
        /// PropertyInfos for the specified type.
        /// </devdoc>
        internal static IDictionary GetPersonalizablePropertyEntries(Type type) {
            Debug.Assert(type != null);

            PersonalizableTypeEntry typeEntry = (PersonalizableTypeEntry)PersonalizableTypeTable[type];

            if (typeEntry == null) {
                typeEntry = new PersonalizableTypeEntry(type);
                PersonalizableTypeTable[type] = typeEntry;
            }

            return typeEntry.PropertyEntries;
        }

        /// <devdoc>
        /// </devdoc>
        internal static IDictionary GetPersonalizablePropertyValues(Control control, PersonalizationScope scope, bool excludeSensitive) {
            IDictionary propertyBag = null;

            IDictionary propertyEntries = GetPersonalizablePropertyEntries(control.GetType());
            if (propertyEntries.Count != 0) {
                foreach (DictionaryEntry entry in propertyEntries) {
                    string name = (string)entry.Key;
                    PersonalizablePropertyEntry propEntry = (PersonalizablePropertyEntry)entry.Value;

                    if (excludeSensitive && propEntry.IsSensitive) {
                        continue;
                    }
                    if ((scope == PersonalizationScope.User) &&
                        (propEntry.Scope == PersonalizationScope.Shared)) {
                        continue;
                    }

                    if (propertyBag == null) {
                        propertyBag = new HybridDictionary(propertyEntries.Count, /* caseInsensitive */ false);
                    }

                    object value = FastPropertyAccessor.GetProperty(control, name, control.DesignMode);

                    propertyBag[name] = new Pair(propEntry.PropertyInfo, value);
                }
            }

            if (propertyBag == null) {
                propertyBag = new HybridDictionary(/* caseInsensitive */ false);
            }
            return propertyBag;
        }

        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }

        /// <internalonly/>
        public override bool Match(object obj) {
            if (obj == this) {
                return true;
            }

            PersonalizableAttribute other = obj as PersonalizableAttribute;
            if (other != null) {
                return (other.IsPersonalizable == IsPersonalizable);
            }

            return false;
        }
    }
}
