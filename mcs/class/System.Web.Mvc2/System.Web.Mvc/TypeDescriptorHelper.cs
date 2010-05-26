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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    // TODO: Remove this class in MVC 3
    //
    // We brought in a private copy of the AssociatedMetadataTypeTypeDescriptionProvider
    // from .NET 4, because it provides several bug fixes and perf improvements. If we're
    // running on .NET < 4, we'll use our private copy.

    internal static class TypeDescriptorHelper {

        private static Func<Type, ICustomTypeDescriptor> _typeDescriptorFactory = GetTypeDescriptorFactory();

        private static Func<Type, ICustomTypeDescriptor> GetTypeDescriptorFactory() {
            if (Environment.Version.Major < 4) {
                return type => new _AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
            }

            return type => new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }

        public static ICustomTypeDescriptor Get(Type type) {
            return _typeDescriptorFactory(type);
        }

        // Private copies of the .NET 4 AssociatedMetadataType classes

        private class _AssociatedMetadataTypeTypeDescriptionProvider : TypeDescriptionProvider {
            public _AssociatedMetadataTypeTypeDescriptionProvider(Type type)
                : base(TypeDescriptor.GetProvider(type)) {
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
                ICustomTypeDescriptor baseDescriptor = base.GetTypeDescriptor(objectType, instance);
                return new _AssociatedMetadataTypeTypeDescriptor(baseDescriptor, objectType);
            }
        }

        private class _AssociatedMetadataTypeTypeDescriptor : CustomTypeDescriptor {
            private Type AssociatedMetadataType {
                get;
                set;
            }

            public _AssociatedMetadataTypeTypeDescriptor(ICustomTypeDescriptor parent, Type type)
                : base(parent) {
                AssociatedMetadataType = TypeDescriptorCache.GetAssociatedMetadataType(type);
                if (AssociatedMetadataType != null) {
                    TypeDescriptorCache.ValidateMetadataType(type, AssociatedMetadataType);
                }
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
                return GetPropertiesWithMetadata(base.GetProperties(attributes));
            }

            public override PropertyDescriptorCollection GetProperties() {
                return GetPropertiesWithMetadata(base.GetProperties());
            }

            private PropertyDescriptorCollection GetPropertiesWithMetadata(PropertyDescriptorCollection originalCollection) {
                if (AssociatedMetadataType == null) {
                    return originalCollection;
                }

                bool customDescriptorsCreated = false;
                List<PropertyDescriptor> tempPropertyDescriptors = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor propDescriptor in originalCollection) {
                    Attribute[] newMetadata = TypeDescriptorCache.GetAssociatedMetadata(AssociatedMetadataType, propDescriptor.Name);
                    PropertyDescriptor descriptor = propDescriptor;
                    if (newMetadata.Length > 0) {
                        // Create a metadata descriptor that wraps the property descriptor
                        descriptor = new _MetadataPropertyDescriptorWrapper(propDescriptor, newMetadata);
                        customDescriptorsCreated = true;
                    }

                    tempPropertyDescriptors.Add(descriptor);
                }

                if (customDescriptorsCreated) {
                    return new PropertyDescriptorCollection(tempPropertyDescriptors.ToArray(), true);
                }
                return originalCollection;
            }

            public override AttributeCollection GetAttributes() {
                // Since normal TD behavior is to return cached attribute instances on subsequent
                // calls to GetAttributes, we must be sure below to use the TD APIs to get both
                // the base and associated attributes
                AttributeCollection attributes = base.GetAttributes();
                if (AssociatedMetadataType != null) {
                    // Note that the use of TypeDescriptor.GetAttributes here opens up the possibility of
                    // infinite recursion, in the corner case of two Types referencing each other as
                    // metadata types (or a longer cycle)
                    Attribute[] newAttributes = TypeDescriptor.GetAttributes(AssociatedMetadataType).OfType<Attribute>().ToArray();
                    attributes = AttributeCollection.FromExisting(attributes, newAttributes);
                }
                return attributes;
            }

            private static class TypeDescriptorCache {
                private static readonly Attribute[] emptyAttributes = new Attribute[0];

                // Stores the associated metadata type for a type
                private static readonly Dictionary<Type, Type> _metadataTypeCache = new Dictionary<Type, Type>();

                // For a type and a property name stores the attributes for that property name.
                private static readonly Dictionary<Tuple<Type, string>, Attribute[]> _typeMemberCache = new Dictionary<Tuple<Type, string>, Attribute[]>();

                // Stores whether or not a type and associated metadata type has been checked for validity
                private static readonly Dictionary<Tuple<Type, Type>, bool> _validatedMetadataTypeCache = new Dictionary<Tuple<Type, Type>, bool>();

                public static void ValidateMetadataType(Type type, Type associatedType) {
                    Tuple<Type, Type> typeTuple = new Tuple<Type, Type>(type, associatedType);

                    lock (_validatedMetadataTypeCache) {
                        if (!_validatedMetadataTypeCache.ContainsKey(typeTuple)) {
                            CheckAssociatedMetadataType(type, associatedType);
                            _validatedMetadataTypeCache.Add(typeTuple, true);
                        }
                    }
                }

                public static Type GetAssociatedMetadataType(Type type) {
                    Type associatedMetadataType = null;
                    lock (_metadataTypeCache) {
                        if (_metadataTypeCache.TryGetValue(type, out associatedMetadataType)) {
                            return associatedMetadataType;
                        }
                    }

                    // Try association attribute
                    MetadataTypeAttribute attribute = (MetadataTypeAttribute)Attribute.GetCustomAttribute(type, typeof(MetadataTypeAttribute));
                    if (attribute != null) {
                        associatedMetadataType = attribute.MetadataClassType;
                    }

                    lock (_metadataTypeCache) {
                        _metadataTypeCache[type] = associatedMetadataType; 
                    }

                    return associatedMetadataType;
                }

                private static void CheckAssociatedMetadataType(Type mainType, Type associatedMetadataType) {
                    // Only properties from main type
                    HashSet<string> mainTypeMemberNames = new HashSet<string>(mainType.GetProperties().Select(p => p.Name));

                    // Properties and fields from buddy type
                    var buddyFields = associatedMetadataType.GetFields().Select(f => f.Name);
                    var buddyProperties = associatedMetadataType.GetProperties().Select(p => p.Name);
                    HashSet<string> buddyTypeMembers = new HashSet<string>(buddyFields.Concat(buddyProperties), StringComparer.Ordinal);

                    // Buddy members should be a subset of the main type's members
                    if (!buddyTypeMembers.IsSubsetOf(mainTypeMemberNames)) {
                        // Reduce the buddy members to the set not contained in the main members
                        buddyTypeMembers.ExceptWith(mainTypeMemberNames);

                        throw new InvalidOperationException(String.Format(
                            CultureInfo.CurrentCulture,
                            MvcResources.PrivateAssociatedMetadataTypeTypeDescriptor_MetadataTypeContainsUnknownProperties,
                            mainType.FullName,
                            String.Join(", ", buddyTypeMembers.ToArray())));
                    }
                }

                public static Attribute[] GetAssociatedMetadata(Type type, string memberName) {
                    var memberTuple = new Tuple<Type, string>(type, memberName);
                    Attribute[] attributes;
                    lock (_typeMemberCache) {
                        if (_typeMemberCache.TryGetValue(memberTuple, out attributes)) {
                            return attributes;
                        }
                    }

                    // Allow fields and properties
                    MemberTypes allowedMemberTypes = MemberTypes.Property | MemberTypes.Field;
                    // Only public static/instance members
                    BindingFlags searchFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                    // Try to find a matching member on type
                    MemberInfo matchingMember = type.GetMember(memberName, allowedMemberTypes, searchFlags).FirstOrDefault();
                    if (matchingMember != null) {
                        attributes = Attribute.GetCustomAttributes(matchingMember, true /* inherit */);
                    }
                    else {
                        attributes = emptyAttributes;
                    }

                    lock (_typeMemberCache) {
                        _typeMemberCache[memberTuple] = attributes;
                    }
                    return attributes;
                }

                private class Tuple<T1, T2> {
                    public T1 Item1 { get; set; }
                    public T2 Item2 { get; set; }

                    public Tuple(T1 item1, T2 item2) {
                        Item1 = item1;
                        Item2 = item2;
                    }

                    public override int GetHashCode() {
                        int h1 = Item1.GetHashCode();
                        int h2 = Item2.GetHashCode();
                        return ((h1 << 5) + h1) ^ h2;
                    }

                    public override bool Equals(object obj) {
                        var other = obj as Tuple<T1, T2>;
                        if (other != null) {
                            return other.Item1.Equals(Item1) && other.Item2.Equals(Item2);
                        }
                        return false;
                    }
                }
            }
        }

        private class _MetadataPropertyDescriptorWrapper : PropertyDescriptor {
            private PropertyDescriptor _descriptor;
            private bool _isReadOnly;

            public _MetadataPropertyDescriptorWrapper(PropertyDescriptor descriptor, Attribute[] newAttributes)
                : base(descriptor, newAttributes) {
                _descriptor = descriptor;
                var readOnlyAttribute = newAttributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
                _isReadOnly = (readOnlyAttribute != null ? readOnlyAttribute.IsReadOnly : false);
            }

            public override void AddValueChanged(object component, EventHandler handler) { _descriptor.AddValueChanged(component, handler); }

            public override bool CanResetValue(object component) { return _descriptor.CanResetValue(component); }

            public override Type ComponentType { get { return _descriptor.ComponentType; } }

            public override object GetValue(object component) { return _descriptor.GetValue(component); }

            public override bool IsReadOnly {
                get {
                    // Dev10 Bug 594083
                    // It's not enough to call the wrapped _descriptor because it does not know anything about
                    // new attributes passed into the constructor of this class.
                    return _isReadOnly || _descriptor.IsReadOnly;
                }
            }

            public override Type PropertyType { get { return _descriptor.PropertyType; } }

            public override void RemoveValueChanged(object component, EventHandler handler) { _descriptor.RemoveValueChanged(component, handler); }

            public override void ResetValue(object component) { _descriptor.ResetValue(component); }

            public override void SetValue(object component, object value) { _descriptor.SetValue(component, value); }

            public override bool ShouldSerializeValue(object component) { return _descriptor.ShouldSerializeValue(component); }

            public override bool SupportsChangeEvents { get { return _descriptor.SupportsChangeEvents; } }
        }
    
    }
}
