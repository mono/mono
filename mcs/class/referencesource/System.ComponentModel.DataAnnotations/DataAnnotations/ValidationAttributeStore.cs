using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Cache of <see cref="ValidationAttribute"/>s
    /// </summary>
    /// <remarks>
    /// This internal class serves as a cache of validation attributes and [Display] attributes.
    /// It exists both to help performance as well as to abstract away the differences between
    /// Reflection and TypeDescriptor.
    /// </remarks>
    internal class ValidationAttributeStore {
        private static ValidationAttributeStore _singleton = new ValidationAttributeStore();
        private Dictionary<Type, TypeStoreItem> _typeStoreItems = new Dictionary<Type, TypeStoreItem>();

        /// <summary>
        /// Gets the singleton <see cref="ValidationAttributeStore"/>
        /// </summary>
        internal static ValidationAttributeStore Instance {
            get {
                return _singleton;
            }
        }

        /// <summary>
        /// Retrieves the type level validation attributes for the given type.
        /// </summary>
        /// <param name="validationContext">The context that describes the type.  It cannot be null.</param>
        /// <returns>The collection of validation attributes.  It could be empty.</returns>
        internal IEnumerable<ValidationAttribute> GetTypeValidationAttributes(ValidationContext validationContext) {
            EnsureValidationContext(validationContext);
            TypeStoreItem item = this.GetTypeStoreItem(validationContext.ObjectType);
            return item.ValidationAttributes;
        }

        /// <summary>
        /// Retrieves the <see cref="DisplayAttribute"/> associated with the given type.  It may be null.
        /// </summary>
        /// <param name="validationContext">The context that describes the type.  It cannot be null.</param>
        /// <returns>The display attribute instance, if present.</returns>
        internal DisplayAttribute GetTypeDisplayAttribute(ValidationContext validationContext) {
            EnsureValidationContext(validationContext);
            TypeStoreItem item = this.GetTypeStoreItem(validationContext.ObjectType);
            return item.DisplayAttribute;
        }

        /// <summary>
        /// Retrieves the set of validation attributes for the property
        /// </summary>
        /// <param name="validationContext">The context that describes the property.  It cannot be null.</param>
        /// <returns>The collection of validation attributes.  It could be empty.</returns>
        internal IEnumerable<ValidationAttribute> GetPropertyValidationAttributes(ValidationContext validationContext) {
            EnsureValidationContext(validationContext);
            TypeStoreItem typeItem = this.GetTypeStoreItem(validationContext.ObjectType);
            PropertyStoreItem item = typeItem.GetPropertyStoreItem(validationContext.MemberName);
            return item.ValidationAttributes;
        }

        /// <summary>
        /// Retrieves the <see cref="DisplayAttribute"/> associated with the given property
        /// </summary>
        /// <param name="validationContext">The context that describes the property.  It cannot be null.</param>
        /// <returns>The display attribute instance, if present.</returns>
        internal DisplayAttribute GetPropertyDisplayAttribute(ValidationContext validationContext) {
            EnsureValidationContext(validationContext);
            TypeStoreItem typeItem = this.GetTypeStoreItem(validationContext.ObjectType);
            PropertyStoreItem item = typeItem.GetPropertyStoreItem(validationContext.MemberName);
            return item.DisplayAttribute;
        }

        /// <summary>
        /// Retrieves the Type of the given property.
        /// </summary>
        /// <param name="validationContext">The context that describes the property.  It cannot be null.</param>
        /// <returns>The type of the specified property</returns>
        internal Type GetPropertyType(ValidationContext validationContext) {
            EnsureValidationContext(validationContext);
            TypeStoreItem typeItem = this.GetTypeStoreItem(validationContext.ObjectType);
            PropertyStoreItem item = typeItem.GetPropertyStoreItem(validationContext.MemberName);
            return item.PropertyType;
        }

        /// <summary>
        /// Determines whether or not a given <see cref="ValidationContext"/>'s
        /// <see cref="ValidationContext.MemberName"/> references a property on
        /// the <see cref="ValidationContext.ObjectType"/>.
        /// </summary>
        /// <param name="validationContext">The <see cref="ValidationContext"/> to check.</param>
        /// <returns><c>true</c> when the <paramref name="validationContext"/> represents a property, <c>false</c> otherwise.</returns>
        internal bool IsPropertyContext(ValidationContext validationContext) {
            EnsureValidationContext(validationContext);
            TypeStoreItem typeItem = this.GetTypeStoreItem(validationContext.ObjectType);
            PropertyStoreItem item = null;
            return typeItem.TryGetPropertyStoreItem(validationContext.MemberName, out item);
        }

        /// <summary>
        /// Retrieves or creates the store item for the given type
        /// </summary>
        /// <param name="type">The type whose store item is needed.  It cannot be null</param>
        /// <returns>The type store item.  It will not be null.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "_typeStoreItems", Justification = "This is used for caching the attributes for a type which is fine.")]
        private TypeStoreItem GetTypeStoreItem(Type type)
        {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            lock (this._typeStoreItems) {
                TypeStoreItem item = null;
                if (!this._typeStoreItems.TryGetValue(type, out item)) {
                    IEnumerable<Attribute> attributes =
#if SILVERLIGHT
 type.GetCustomAttributes(true).Cast<Attribute>();
#else
 TypeDescriptor.GetAttributes(type).Cast<Attribute>();
#endif
                    item = new TypeStoreItem(type, attributes);
                    this._typeStoreItems[type] = item;
                }
                return item;
            }
        }

        /// <summary>
        /// Throws an ArgumentException of the validation context is null
        /// </summary>
        /// <param name="validationContext">The context to check</param>
        private static void EnsureValidationContext(ValidationContext validationContext) {
            if (validationContext == null) {
                throw new ArgumentNullException("validationContext");
            }
        }

        /// <summary>
        /// Private abstract class for all store items
        /// </summary>
        private abstract class StoreItem {
            private static IEnumerable<ValidationAttribute> _emptyValidationAttributeEnumerable = new ValidationAttribute[0];

            private IEnumerable<ValidationAttribute> _validationAttributes;

            internal StoreItem(IEnumerable<Attribute> attributes) {
                this._validationAttributes = attributes.OfType<ValidationAttribute>();
                this.DisplayAttribute = attributes.OfType<DisplayAttribute>().SingleOrDefault();
            }

            internal IEnumerable<ValidationAttribute> ValidationAttributes {
                get {
                    return this._validationAttributes;
                }
            }

            internal DisplayAttribute DisplayAttribute { get; set; }
        }

        /// <summary>
        /// Private class to store data associated with a type
        /// </summary>
        private class TypeStoreItem : StoreItem {
            private object _syncRoot = new object();
            private Type _type;
            private Dictionary<string, PropertyStoreItem> _propertyStoreItems;

            internal TypeStoreItem(Type type, IEnumerable<Attribute> attributes)
                : base(attributes) {
                this._type = type;
            }

            internal PropertyStoreItem GetPropertyStoreItem(string propertyName) {
                PropertyStoreItem item = null;
                if (!this.TryGetPropertyStoreItem(propertyName, out item)) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.AttributeStore_Unknown_Property, this._type.Name, propertyName), "propertyName");
                }
                return item;
            }

            internal bool TryGetPropertyStoreItem(string propertyName, out PropertyStoreItem item) {
                if (string.IsNullOrEmpty(propertyName)) {
                    throw new ArgumentNullException("propertyName");
                }

                if (this._propertyStoreItems == null) {
                    lock (this._syncRoot) {
                        if (this._propertyStoreItems == null) {
                            this._propertyStoreItems = this.CreatePropertyStoreItems();
                        }
                    }
                }
                if (!this._propertyStoreItems.TryGetValue(propertyName, out item)) {
                    return false;
                }
                return true;
            }

            private Dictionary<string, PropertyStoreItem> CreatePropertyStoreItems() {
                Dictionary<string, PropertyStoreItem> propertyStoreItems = new Dictionary<string, PropertyStoreItem>();

#if SILVERLIGHT
                PropertyInfo[] properties = this._type.GetProperties();
                foreach (PropertyInfo property in properties) {
                    PropertyStoreItem item = new PropertyStoreItem(property.PropertyType, property.GetCustomAttributes(true).Cast<Attribute>());
                    propertyStoreItems[property.Name] = item;
                }
#else
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this._type);
                foreach (PropertyDescriptor property in properties) {
                    PropertyStoreItem item = new PropertyStoreItem(property.PropertyType, GetExplicitAttributes(property).Cast<Attribute>());
                    propertyStoreItems[property.Name] = item;
                }
#endif // SILVERLIGHT

                return propertyStoreItems;
            }

#if !SILVERLIGHT
            /// <summary>
            /// Method to extract only the explicitly specified attributes from a <see cref="PropertyDescriptor"/>
            /// </summary>
            /// <remarks>
            /// Normal TypeDescriptor semantics are to inherit the attributes of a property's type.  This method
            /// exists to suppress those inherited attributes.
            /// </remarks>
            /// <param name="propertyDescriptor">The property descriptor whose attributes are needed.</param>
            /// <returns>A new <see cref="AttributeCollection"/> stripped of any attributes from the property's type.</returns>
            public static AttributeCollection GetExplicitAttributes(PropertyDescriptor propertyDescriptor) {
                List<Attribute> attributes = new List<Attribute>(propertyDescriptor.Attributes.Cast<Attribute>());
                IEnumerable<Attribute> typeAttributes = TypeDescriptor.GetAttributes(propertyDescriptor.PropertyType).Cast<Attribute>();
                bool removedAttribute = false;
                foreach (Attribute attr in typeAttributes) {
                    for (int i = attributes.Count - 1; i >= 0; --i) {
                        // We must use ReferenceEquals since attributes could Match if they are the same.
                        // Only ReferenceEquals will catch actual duplications.
                        if (object.ReferenceEquals(attr, attributes[i])) {
                            attributes.RemoveAt(i);
                            removedAttribute = true;
                        }
                    }
                }
                return removedAttribute ? new AttributeCollection(attributes.ToArray()) : propertyDescriptor.Attributes;
            }
#endif // !SILVERLIGHT
        }

        /// <summary>
        /// Private class to store data associated with a property
        /// </summary>
        private class PropertyStoreItem : StoreItem {
            private Type _propertyType;

            internal PropertyStoreItem(Type propertyType, IEnumerable<Attribute> attributes)
                : base(attributes) {
                this._propertyType = propertyType;
            }

            internal Type PropertyType {
                get {
                    return this._propertyType;
                }
            }
        }
    }
}
