using System.Diagnostics.CodeAnalysis;

namespace System.Activities.Presentation.Metadata
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Activities.Presentation.Internal.Metadata;
    using System.Runtime;
    using System.Activities.Presentation;

    /// <summary>
    /// The MetadataStore is a container of custom attribute metadata.
    /// Custom attributes may be defined in an attribute table and added
    /// to the metadata store.  Once added, these attributes will appear
    /// in calls made to TypeDescriptor.
    /// </summary>
    public static class MetadataStore
    {

        private static object _syncLock = new object();
        private static MetadataStoreProvider _objectProvider;
        private static Dictionary<Type, Type> _interfaces;
        private static Hashtable _typeAttributeCache;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static MetadataStore()
        {
            TypeDescriptor.Refreshed += TypeDescriptor_Refreshed;
        }

        static void TypeDescriptor_Refreshed(RefreshEventArgs e)
        {
            _typeAttributeCache = null;
        }

        /// <summary>
        /// Adds the given table to the current AppDomain’s attribute store.  
        /// Once added, calls to TypeDescriptor will use attributes defined 
        /// in the newly added table.  Multiple tables may be added to the 
        /// attribute store.  In the case of conflicts, attributes in the 
        /// most recently added table win.
        /// </summary>
        /// <param name="table">The table to add</param>
        /// <exception cref="ArgumentNullException">if table is null</exception>
        public static void AddAttributeTable(AttributeTable table)
        {
            AddAttributeTableCore(table, false);
        }

        internal static void AddSystemAttributeTable(AttributeTable table)
        {
            AddAttributeTableCore(table, true);
        }

        private static void AddAttributeTableCore(AttributeTable table, bool isSystemAttributeTable = false)
        {
            if (table == null) throw FxTrace.Exception.ArgumentNull("table");

            lock (_syncLock)
            {
                if (_objectProvider == null)
                {
                    _objectProvider = new MetadataStoreProvider(table);
                    TypeDescriptor.AddProvider(_objectProvider, typeof(object));
                }
                else
                {
                    _objectProvider.AddTable(table, isSystemAttributeTable);
                }

                foreach (Type t in table.AttributedTypes)
                {

                    // If there are any interface types in the given
                    // table, we must add providers specifically for those
                    // types because interfaces do not derive from object and
                    // are therefore missed by our global hook.

                    if (t.IsInterface)
                    {
                        if (_interfaces == null)
                        {
                            _interfaces = new Dictionary<Type, Type>();
                        }
                        if (!_interfaces.ContainsKey(t))
                        {
                            TypeDescriptor.AddProvider(_objectProvider, t);
                            _interfaces.Add(t, t);
                        }
                    }
                }
            }

            // Now invalidate the types.
            foreach (Type t in table.AttributedTypes)
            {
                TypeDescriptor.Refresh(t);
            }

#if DEBUG
            table.DebugValidateProvider();
#endif
        }

        //
        // This type description provider is used for all objects and
        // interfaces.
        //
        private class MetadataStoreProvider : TypeDescriptionProvider
        {

            private Dictionary<Type, Type> _metadataTypeCache;
            private Dictionary<Type, AttributeCollection> _attributeCache;
            private Dictionary<DescriptorKey, MemberDescriptor> _descriptorCache;
            private AttributeTable[] _tables;
            private object _syncLock = new object();

            internal MetadataStoreProvider(AttributeTable table)
                : base(TypeDescriptor.GetProvider(typeof(object)))
            {
                _tables = new AttributeTable[] { table };
            }

            //
            // Called by the metadata store to add a new table to our
            // provider.  Guarded by a lock on the metadata store.
            //
            internal void AddTable(AttributeTable table, bool isSystemAttributeTable = false)
            {

                // We don't expect a huge number of tables,
                // so creating a new array here is fine.  Also,
                // this has the added benefit of allowing code
                // that eumerates tables to be thread-safe 
                // without taking any locks.  Caller is responsible
                // for ensuring AddTable is synchronized.

                AttributeTable[] newTables;
                newTables = new AttributeTable[_tables.Length + 1];

                if (isSystemAttributeTable)
                {
                    _tables.CopyTo(newTables, 0);
                    newTables[newTables.Length - 1] = table;
                }
                else
                {
                    _tables.CopyTo(newTables, 1);
                    newTables[0] = table;
                }

                _tables = newTables;

                if (_attributeCache != null) _attributeCache.Clear();

                // Clear all metadata types that will change as
                // a result of adding this new table

                if (_metadataTypeCache != null)
                {
                    List<Type> toRemove = null;
                    foreach (Type t in table.AttributedTypes)
                    {
                        foreach (Type cached in _metadataTypeCache.Keys)
                        {
                            Type realCachedType = cached.UnderlyingSystemType;
                            if (t.IsAssignableFrom(realCachedType))
                            {
                                if (toRemove == null) toRemove = new List<Type>();
                                toRemove.Add(realCachedType);
                            }

                            // if a type definition is changed, clear all types defined by this type definition.
                            // eg. if Cat<> is changed, clear Cat<int>, Cat<object> etc.
                            if (t.IsGenericTypeDefinition
                                && realCachedType.IsGenericType
                                && t == realCachedType.GetGenericTypeDefinition())
                            {
                                if (toRemove == null) toRemove = new List<Type>();
                                toRemove.Add(realCachedType);
                            }
                        }
                    }

                    if (toRemove != null)
                    {
                        foreach (Type t in toRemove)
                        {
                            _metadataTypeCache.Remove(t);
                        }
                    }
                }
            }

            //
            // Private helper that creates an attribute collection given an array of attributes.
            // This does the appropriate pairing down of attributes with matching TypeIds so
            // the collection contains no redundant attributes.  Attributes should be ordered
            // from most derived to least derived, so the first attribute reported wins.
            //
            private static AttributeCollection CreateAttributeCollection(Attribute[] attrArray)
            {
                Dictionary<object, Attribute> dict = new Dictionary<object, Attribute>(attrArray.Length);

                // Attrs array is ordered by priority, first one in 
                // wins.

                int finalCount = attrArray.Length;

                for (int idx = 0; idx < attrArray.Length; idx++)
                {
                    Attribute a = attrArray[idx];
                    if (dict.ContainsKey(a.TypeId))
                    {
                        attrArray[idx] = null;
                        finalCount--;
                    }
                    else
                    {
                        dict.Add(a.TypeId, a);
                    }
                }

                Attribute[] finalArray;

                if (finalCount != attrArray.Length)
                {
                    finalArray = new Attribute[finalCount];
                    int finalIdx = 0;
                    for (int idx = 0; idx < attrArray.Length; idx++)
                    {
                        if (attrArray[idx] != null)
                        {
                            finalArray[finalIdx++] = attrArray[idx];
                        }
                    }
                }
                else
                {
                    finalArray = attrArray;
                }

                return new AttributeCollection(finalArray);
            }

            //
            // Returns a cached attribute collection for the given
            // object type and member.  Member can be null to get
            // attributes for the class.
            //
            private AttributeCollection GetAttributes(Type objectType)
            {
                Fx.Assert(objectType != null, "objectType parameter should not be null");
                AttributeCollection attributes;

                // Dictionary does not support thread-safe reads.  We need to lock on 
                // all access.

                lock (_syncLock)
                {
                    if (_attributeCache == null) _attributeCache = new Dictionary<Type, AttributeCollection>();
                    if (!_attributeCache.TryGetValue(objectType, out attributes))
                    {
                        attributes = CreateAttributeCollection(GetRawAttributes(objectType, null, null, false));
                        _attributeCache[objectType] = attributes;
                    }
                }

                return attributes;
            }

            //
            // Raw API to return attributes.  This is used by GetAttributes to 
            // populate the AttributeCollection (which is cached), and by
            // the Attributes property on property and event descriptors (where
            // caching is handled by the Member Descriptor). Attributes
            // are returned from this ordered so the highest priority attributes
            // are first (first in wins).
            //

            private static Attribute[] GetRawAttributes(Type objectType, string member, MemberDescriptor parentDescriptor, bool isEvent)
            {
                Fx.Assert(objectType != null, "objectType parameter should not be null");
                Type reflectType = TypeDescriptor.GetReflectionType(objectType);

                // There is a bug in CLR reflection that does not respect the "inherit"
                // flag for event or property infos.  Our custom metadata type does respect
                // this flag and correctly does the right thing.  If the object type we
                // are passed is not a metadata type, just use the default behavior of the
                // parent member descriptor.  It will be right, and since we're not a metadata
                // type that means we have no overrides anyway.
                //
                // The reason we have to call our type with inherit, instead of just using
                // one code path is we need to support the interleaving of CLR and 
                // metadata table attributes up the inheritance hierarchy.  MetadataType 
                // does that for us.

                if (parentDescriptor != null && !(reflectType is MetadataType))
                {
                    AttributeCollection attrs = parentDescriptor.Attributes;
                    Attribute[] attrArray = new Attribute[attrs.Count];

                    // CLR property descriptor reverses attribute order.  Fix it.
                    for (int idx = 0; idx < attrArray.Length; idx++)
                    {
                        attrArray[idx] = attrs[attrArray.Length - idx - 1];
                    }

                    return attrArray;
                }

                MemberInfo reflectMember;
                Type reflectMemberType = null;

                Attribute[] attributes;

                if (member == null)
                {
                    reflectMember = reflectType;
                }
                else if (isEvent)
                {
                    EventInfo info = null;

                    MemberInfo[] infos = reflectType.GetMember(member, MemberTypes.Event, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                    if (infos.Length > 0)
                    {
                        info = infos[0] as EventInfo;
                    }

                    reflectMember = info;
                    if (info != null) reflectMemberType = TypeDescriptor.GetReflectionType(info.EventHandlerType);
                }
                else
                {
                    PropertyInfo info = null;

                    MemberInfo[] infos = reflectType.GetMember(member, MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                    if (infos.Length > 0)
                    {
                        info = infos[0] as PropertyInfo;
                    }

                    reflectMember = info;
                    if (info != null) reflectMemberType = TypeDescriptor.GetReflectionType(info.PropertyType);
                }

                if (reflectMember == null)
                {
                    Debug.Fail("Member " + member + " is not a member of type " + objectType.Name);
                    attributes = new Attribute[0];
                }
                else
                {
                    // Cannot simply cast to Attribute[]
                    Object[] attrs = reflectMember.GetCustomAttributes(typeof(Attribute), true);
                    List<Object> attrList = new List<Object>(attrs);

                    Hashtable cache = _typeAttributeCache;
                    if (cache == null)
                    {
                        cache = new Hashtable();
                        _typeAttributeCache = cache;
                    }

                    // Get the base type attributes if we have them
                    if (reflectMemberType != null)
                    {
                        reflectType = reflectMemberType;

                        attrs = (object[])cache[reflectType];
                        if (attrs == null)
                        {
                            attrs = reflectType.GetCustomAttributes(typeof(Attribute), true);
                            lock (cache.SyncRoot)
                            {
                                cache[reflectType] = attrs;
                            }
                        }

                        attrList.AddRange(attrs);
                    }

                    // Get interface attributes too.

                    foreach (Type iface in reflectType.GetInterfaces())
                    {
                        attrs = (object[])cache[iface];
                        if (attrs == null)
                        {
                            attrs = iface.GetCustomAttributes(typeof(Attribute), false);
                            lock (cache.SyncRoot)
                            {
                                cache[iface] = attrs;
                            }
                        }

                        attrList.AddRange(attrs);
                    }

                    // Now go through the attributes and expand those that are 
                    // AttributeProviderAttributes
                    for (int idx = 0; idx < attrList.Count; idx++)
                    {
                        AttributeProviderAttribute a = attrList[idx] as AttributeProviderAttribute;
                        if (a != null)
                        {
                            reflectType = Type.GetType(a.TypeName);
                            if (reflectType != null)
                            {
                                reflectType = TypeDescriptor.GetReflectionType(reflectType);
                                reflectMember = reflectType;

                                if (a.PropertyName != null && a.PropertyName.Length > 0)
                                {
                                    MemberInfo[] infos = reflectType.GetMember(a.PropertyName);
                                    if (infos != null && infos.Length > 0)
                                    {
                                        reflectMember = infos[0];
                                    }
                                }

                                attrList.AddRange(reflectMember.GetCustomAttributes(typeof(Attribute), true));
                            }
                        }
                    }

                    attributes = new Attribute[attrList.Count];

                    for (int idx = 0; idx < attrList.Count; idx++)
                    {
                        attributes[idx] = (Attribute)attrList[idx];
                    }
                }

                return attributes;
            }

            //
            // Access to our descriptor cache
            //
            private MemberDescriptor GetCachedDescriptor(Type objectType, MemberDescriptor descriptor)
            {
                MemberDescriptor cached;
                DescriptorKey key = new DescriptorKey(objectType, descriptor);

                lock (_syncLock)
                {
                    if (_descriptorCache == null || !_descriptorCache.TryGetValue(key, out cached))
                    {
                        cached = null;
                    }
                }

                return cached;
            }

            //
            // Access to our descriptor cache
            //
            private void CacheDescriptor(Type objectType, MemberDescriptor descriptor, MemberDescriptor cache)
            {

                lock (_syncLock)
                {
                    if (_descriptorCache == null)
                    {
                        _descriptorCache = new Dictionary<DescriptorKey, MemberDescriptor>();
                    }
                    DescriptorKey key = new DescriptorKey(objectType, descriptor);

                    // Caller may ---- and this cache slot may already be allocated.
                    // That's OK; we'll just replace it.
                    _descriptorCache[key] = cache;
                }
            }

            //
            // Takes a collection of events and merges them with our own descriptors.
            //
            private EventDescriptorCollection MergeEvents(Type objectType, EventDescriptorCollection incoming)
            {
                EventDescriptor[] array = new EventDescriptor[incoming.Count];
                for (int idx = 0; idx < array.Length; idx++)
                {
                    EventDescriptor theirs = incoming[idx];
                    EventDescriptor ours = (EventDescriptor)GetCachedDescriptor(objectType, theirs);
                    if (ours == null)
                    {
                        ours = new MetadataStoreEventDescriptor(objectType, theirs);
                        CacheDescriptor(objectType, theirs, ours);
                    }
                    array[idx] = ours;
                }
                return new EventDescriptorCollection(array, true);
            }

            //
            // Takes a collection of properties and merges them with our own descriptors.
            //
            private PropertyDescriptorCollection MergeProperties(Type objectType, PropertyDescriptorCollection incoming)
            {
                PropertyDescriptor[] array = new PropertyDescriptor[incoming.Count];
                for (int idx = 0; idx < array.Length; idx++)
                {
                    PropertyDescriptor theirs = incoming[idx];
                    PropertyDescriptor ours = (PropertyDescriptor)GetCachedDescriptor(objectType, theirs);
                    if (ours == null)
                    {
                        ours = new MetadataStorePropertyDescriptor(objectType, theirs);
                        CacheDescriptor(objectType, theirs, ours);
                    }
                    array[idx] = ours;
                }
                return new PropertyDescriptorCollection(array, true);
            }

            //
            // Looks at objectType and returns a wrapped type if needed.  The
            // parameter may be null, in which case this API returns null.
            //
            internal Type MergeType(Type objectType)
            {
                if (objectType == null) return null;
                return MergeTypeInternal(objectType, null);
            }

            //
            // Helper method for both MergeType and GetReflectionType
            //
            private Type MergeTypeInternal(Type objectType, object instance)
            {

                // If the incoming object type is already one of our wrapped types,
                // there is nothing more for us to do
                if (objectType is MetadataType) return objectType;

                Type baseReflectionType = base.GetReflectionType(objectType, instance);
                Type reflectionType;

                lock (_syncLock)
                {
                    if (_metadataTypeCache == null || !_metadataTypeCache.TryGetValue(baseReflectionType, out reflectionType))
                    {
                        Type objectTypeDefinition = null;
                        if (objectType.IsGenericType && !objectType.IsGenericTypeDefinition)
                        {
                            objectTypeDefinition = objectType.GetGenericTypeDefinition();
                        }

                        // See if we need to build a custom type for this objectType

                        bool containsAttributes = false;
                        foreach (AttributeTable table in _tables)
                        {
                            if (table.ContainsAttributes(objectType))
                            {
                                containsAttributes = true;
                                break;
                            }

                            if (objectTypeDefinition != null && table.ContainsAttributes(objectTypeDefinition))
                            {
                                containsAttributes = true;
                                break;
                            }
                        }

                        // If we failed to find attributes quickly, we need
                        // to check base classes and interfaces.

                        if (!containsAttributes)
                        {
                            foreach (AttributeTable table in _tables)
                            {
                                foreach (Type t in table.AttributedTypes)
                                {
                                    if (t.IsAssignableFrom(objectType))
                                    {
                                        containsAttributes = true;
                                        break;
                                    }
                                }
                            }
                        }

                        // If we have a table that contains attributes for this type, we need
                        // to wrap the type in our own reflection type.  If not, we will
                        // store baseReflectionType in the cache slot.

                        if (containsAttributes)
                        {
                            reflectionType = new MetadataType(baseReflectionType, _tables, this);
                        }
                        else
                        {
                            reflectionType = baseReflectionType;
                        }

                        if (_metadataTypeCache == null)
                        {
                            _metadataTypeCache = new Dictionary<Type, Type>();
                        }
                        _metadataTypeCache[baseReflectionType] = reflectionType;
                    }
                }

                return reflectionType;
            }

            //
            // This method is the metadata store's "master hook".  It will return a custom
            // "MetadataType" for all types that have custom metadata declared in the
            // attribute table.  By infusing our custom metadata at this low level,
            // everything that builds on top of reflection can be accomodated.
            //
            public override Type GetReflectionType(Type objectType, object instance)
            {
                if (objectType == null) throw FxTrace.Exception.ArgumentNull("objectType");
                return MergeTypeInternal(objectType, instance);
            }

            //
            // Returns a custom type descriptor for the given object
            //
            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                ICustomTypeDescriptor descriptor = base.GetTypeDescriptor(objectType, instance);
                descriptor = new MetadataStoreTypeDescriptor(this, objectType, descriptor);
                return descriptor;
            }

            //
            // A descriptor key is a dictionary key used by
            // our member descriptor cache.  The key consists of a type
            // and a member descriptor.
            // 
            private struct DescriptorKey : IEquatable<DescriptorKey>
            {
                internal readonly Type Type;
                internal readonly MemberDescriptor Member;

                internal DescriptorKey(Type type, MemberDescriptor member)
                {
                    Type = type;
                    Member = member;
                }

                public override int GetHashCode()
                {
                    int hash = Type.GetHashCode();
                    if (Member != null)
                    {
                        hash ^= Member.GetHashCode();
                    }
                    return hash;
                }

                public override bool Equals(object obj)
                {
                    return Equals((DescriptorKey)obj);
                }

                public static bool operator ==(DescriptorKey a, DescriptorKey b)
                {
                    return a.Equals(b);
                }

                public static bool operator !=(DescriptorKey a, DescriptorKey b)
                {
                    return !a.Equals(b);
                }

#if DEBUG
                public override string ToString() {
                    string v = Type.FullName;
                    if (Member != null) v = string.Concat(v, ".", Member);
                    return v;
                }
#endif

                // IEquatable<DescriptorKey> Members

                public bool Equals(DescriptorKey other)
                {
                    if (Type != other.Type) return false;
                    return object.ReferenceEquals(Member, other.Member);
                }

            }

            //
            // This type descriptor is what provides additional metadata.
            // We implement ICustomTypeDescriptor ourselves, rather than
            // derive from the helper CustomTypeDescriptor class because
            // we want this implementation to be built on a struct for
            // performance reasons.
            //
            private struct MetadataStoreTypeDescriptor : ICustomTypeDescriptor
            {

                private MetadataStoreProvider _provider;
                private Type _objectType;
                private ICustomTypeDescriptor _parent;

                internal MetadataStoreTypeDescriptor(MetadataStoreProvider provider, Type objectType, ICustomTypeDescriptor parent)
                {
                    _provider = provider;
                    _objectType = objectType;
                    _parent = parent;
                }

                // ICustomTypeDescriptor Members

                // Calls that just forward the the parent API
                string ICustomTypeDescriptor.GetClassName() { return _parent.GetClassName(); }
                string ICustomTypeDescriptor.GetComponentName() { return _parent.GetComponentName(); }
                TypeConverter ICustomTypeDescriptor.GetConverter() { return _parent.GetConverter(); }
                EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() { return _parent.GetDefaultEvent(); }
                PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() { return _parent.GetDefaultProperty(); }
                object ICustomTypeDescriptor.GetEditor(Type editorBaseType) { return _parent.GetEditor(editorBaseType); }
                object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) { return _parent.GetPropertyOwner(pd); }

                //
                // Override to provide merged metadata
                //
                AttributeCollection ICustomTypeDescriptor.GetAttributes()
                {
                    return _provider.GetAttributes(_objectType);
                }

                //
                // Override to provide merged metadata
                //
                EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
                {
                    return _provider.MergeEvents(_objectType, _parent.GetEvents(attributes));
                }

                //
                // Override to provide merged metadata
                //
                EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
                {
                    return _provider.MergeEvents(_objectType, _parent.GetEvents());
                }

                //
                // Override to provide merged metadata
                //
                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
                {
                    return _provider.MergeProperties(_objectType, _parent.GetProperties(attributes));
                }

                //
                // Override to provide merged metadata
                //
                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
                {
                    return _provider.MergeProperties(_objectType, _parent.GetProperties());
                }

            }

            //
            // A property descriptor that adds additional metadata to an existing
            // property descriptor.
            //
            private class MetadataStorePropertyDescriptor : PropertyDescriptor
            {

                private static readonly object _noValue = new object();
                private static readonly object _invalidValue = new object();

                private Type _objectType;
                private PropertyDescriptor _parent;
                private AttributeCollection _attributes;
                private Attribute[] _rawAttributes;
                private object _defaultValue = _invalidValue;
                private object _ambientValue = _invalidValue;

                // There are simpler base ctors we can invoke that do attribute merging for us.  However,
                // they do it all up front instead of deferring.  We want to defer until someone actually
                // asks for attributes.
                internal MetadataStorePropertyDescriptor(Type objectType, PropertyDescriptor parent)
                    : base(parent.Name, null)
                {
                    _objectType = objectType;
                    _parent = parent;
                }

                //
                // Return our attribute collection.  We cache it
                // for speed.
                //
                public override AttributeCollection Attributes
                {
                    get
                    {
                        CheckAttributesValid();
                        if (_attributes == null)
                        {
                            _attributes = MetadataStoreProvider.CreateAttributeCollection(_rawAttributes);
                        }

                        return _attributes;
                    }
                }

                //
                // DefaultValue and AmbientValue are two values that are used by
                // ReflectTypeDescriptionProvider in ResetValue(), CanResetValue(),
                // and ShouldSerializeValue() in such a way that by-passes any additional
                // values provided by the MetadataStore.  As such, we manually look up
                // these two attributes ourselves to keep the MetadataStore functionality
                // intact.
                //
                private object DefaultValue
                {
                    get
                    {
                        if (_defaultValue == _invalidValue)
                        {
                            DefaultValueAttribute dva = (DefaultValueAttribute)Attributes[typeof(DefaultValueAttribute)];
                            if (dva != null)
                            {
                                _defaultValue = dva.Value;
                            }
                            else
                            {
                                _defaultValue = _noValue;
                            }
                        }
                        return _defaultValue;
                    }
                }

                //
                // See comment for DefaultValue
                //
                private object AmbientValue
                {
                    get
                    {
                        if (_ambientValue == _invalidValue)
                        {
                            AmbientValueAttribute ava = (AmbientValueAttribute)Attributes[typeof(AmbientValueAttribute)];
                            if (ava != null)
                            {
                                _ambientValue = ava.Value;
                            }
                            else
                            {
                                _ambientValue = _noValue;
                            }
                        }
                        return _ambientValue;
                    }
                }

                //
                // This is a little strange and merits some explanation.
                // We want to cache our attribute collection, but we need
                // to know when to invalidate the cache. MemberDescriptor
                // has an internal version stamp that it compares with
                // TypeDescriptor's metadata version and automatically invalidates
                // its own cached values for attributes.  The trick is 
                // we need to know when MemberDescriptor invalidated its
                // cached values.  We do this by keying off the fact that
                // MemberDescriptor will call FillAttributes when its cache
                // needs to be repopulated.  But, in oder to get MemberDescriptor
                // to look at its cache we must access an API that requires
                // cached data.  AttributeArray does this in the lightest
                // possible way.  The actual set of attributes in the array
                // is always an empty set, but we don't care; we only need
                // to access the property.  The return value of the API
                // and the check for length are only here so the compiler
                // and FXCop do not complain about us calling a property
                // and not using the return value.  The attribute array
                // always contains zero elements.
                //
                private bool CheckAttributesValid()
                {
                    Fx.Assert(AttributeArray.Length == 0, "Attribute array should always contain zero elements");
                    return AttributeArray.Length == 0 && _attributes != null;
                }

                //
                // This method is called when we need to populate the raw set of
                // attributes for this property.  
                //
                protected override void FillAttributes(IList attributeList)
                {
                    _attributes = null;
                    _defaultValue = _invalidValue;
                    _ambientValue = _invalidValue;

                    _rawAttributes = MetadataStoreProvider.GetRawAttributes(_objectType, Name, _parent, false);
                    base.FillAttributes(attributeList);
                }

                // PropertyDescriptor API.  
                public override bool CanResetValue(object component)
                {
                    if (DefaultValue != _noValue)
                    {
                        object currentValue = GetValue(component);
                        return !object.Equals(currentValue, DefaultValue);
                    }
                    else if (AmbientValue != _noValue)
                    {
                        object currentValue = GetValue(component);
                        return !object.Equals(currentValue, AmbientValue);
                    }
                    else
                    {
                        return _parent.CanResetValue(component);
                    }
                }

                public override void ResetValue(object component)
                {
                    if (DefaultValue != _noValue)
                    {
                        SetValue(component, DefaultValue);
                    }
                    else if (AmbientValue != _noValue)
                    {
                        SetValue(component, AmbientValue);
                    }
                    else
                    {
                        _parent.ResetValue(component);
                    }
                }

                public override bool ShouldSerializeValue(object component)
                {
                    if (DefaultValue != _noValue)
                    {
                        object currentValue = GetValue(component);
                        return !object.Equals(currentValue, DefaultValue);
                    }
                    else if (AmbientValue != _noValue)
                    {
                        object currentValue = GetValue(component);
                        return !object.Equals(currentValue, AmbientValue);
                    }
                    else
                    {
                        return _parent.ShouldSerializeValue(component);
                    }
                }

                public override void AddValueChanged(object component, EventHandler handler) { _parent.AddValueChanged(component, handler); }
                public override Type ComponentType { get { return _parent.ComponentType; } }
                [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
                [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
                public override object GetValue(object component)
                {
                    object retValue = null;

                    try
                    {
                        retValue = _parent.GetValue(component);
                    }
                    catch (System.Exception)
                    {
                        // GetValue throws an exception if Value is not available
                    }

                    return retValue;
                }
                public override bool IsReadOnly { get { return _parent.IsReadOnly || Attributes.Contains(ReadOnlyAttribute.Yes); } }
                public override Type PropertyType { get { return _parent.PropertyType; } }
                public override void SetValue(object component, object value) { _parent.SetValue(component, value); }
                public override void RemoveValueChanged(object component, EventHandler handler) { _parent.RemoveValueChanged(component, handler); }
                public override bool SupportsChangeEvents { get { return _parent.SupportsChangeEvents; } }
            }

            //
            // An event descriptor that adds additional metadata to an existing 
            // event descriptor.
            //
            private class MetadataStoreEventDescriptor : EventDescriptor
            {
                private Type _objectType;
                private EventDescriptor _parent;

                // There are simpler base ctors we can invoke that do attribute merging for us.  However,
                // they do it all up front instead of deferring.  We want to defer until someone actually
                // asks for attributes.
                internal MetadataStoreEventDescriptor(Type objectType, EventDescriptor parent)
                    : base(parent.Name, null)
                {
                    _objectType = objectType;
                    _parent = parent;
                }

                //
                // We override this so we can merge in our additional attributes.
                // By overriding here, we wait until someone actually asks for
                // attributes before merging.
                //
                protected override AttributeCollection CreateAttributeCollection()
                {
                    Attribute[] attrs = MetadataStoreProvider.GetRawAttributes(_objectType, Name, _parent, true);

                    // We must let MemberDescriptor build its attributes, even if we're going
                    // to replace them.  The reason for this is that MemberDescriptor keeps an
                    // internal version stamp.  This version stamp changes when someone adds
                    // a new TypeDescriptionProvider.  If we don't let MemberDescriptor maintain
                    // this version stamp it won't invalidate metadata when someone adds or removes
                    // a TypeDescriptionProvider, which can cause us to return stale data.

                    AttributeArray = attrs;
                    AttributeCollection attributes = base.CreateAttributeCollection(); // do not delete this
                    attributes = MetadataStoreProvider.CreateAttributeCollection(attrs);
                    return attributes;
                }

                // EventDescriptor API
                public override void AddEventHandler(object component, Delegate value) { _parent.AddEventHandler(component, value); }
                public override Type ComponentType { get { return _parent.ComponentType; } }
                public override Type EventType { get { return _parent.EventType; } }
                public override bool IsMulticast { get { return _parent.IsMulticast; } }
                public override void RemoveEventHandler(object component, Delegate value) { _parent.RemoveEventHandler(component, value); }
            }
        }

        //
        // This type is the core of our metadata store.  We offer this through our type 
        // description provider as the "reflection type" that should be used during
        // reflection operations.  All things within TypeDescriptor will use this
        // type to get metadata, which gives us a big hook into their mechanism.
        // By hooking at this low level we can be sure to cover all of our bases.
        //
        private class MetadataType : Type
        {

            private Type _baseReflectionType;
            private AttributeTable[] _tables;
            private MetadataStoreProvider _provider;
            private Dictionary<MemberInfo, MemberInfo> _memberCache;
            private Hashtable _seenAttributes;
            private AttributeMergeCache _cache;
            private object _syncLock = new object();

            internal MetadataType(Type baseReflectionType, AttributeTable[] tables, MetadataStoreProvider provider)
            {
                _baseReflectionType = baseReflectionType;
                _tables = tables;
                _provider = provider;
            }

            // Type Forward APIs
            //
            // The majority of Type's API is just forwarded to our base reflection 
            // type.
            //
            public override Guid GUID { get { return _baseReflectionType.GUID; } }
            public override Assembly Assembly { get { return _baseReflectionType.Assembly; } }
            public override string AssemblyQualifiedName { get { return _baseReflectionType.AssemblyQualifiedName; } }
            public override bool ContainsGenericParameters { get { return _baseReflectionType.ContainsGenericParameters; } }
            public override MethodBase DeclaringMethod { get { return _baseReflectionType.DeclaringMethod; } }
            public override RuntimeTypeHandle TypeHandle { get { return _baseReflectionType.TypeHandle; } }
            public override int GetHashCode() { return _baseReflectionType.GetHashCode(); }
            public override string FullName { get { return _baseReflectionType.FullName; } }
            public override GenericParameterAttributes GenericParameterAttributes { get { return _baseReflectionType.GenericParameterAttributes; } }
            public override int GenericParameterPosition { get { return _baseReflectionType.GenericParameterPosition; } }
            public override int GetArrayRank() { return _baseReflectionType.GetArrayRank(); }
            protected override TypeAttributes GetAttributeFlagsImpl() { return _baseReflectionType.Attributes; }
            protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) { return _baseReflectionType.GetConstructor(bindingAttr, binder, callConvention, types, modifiers); }
            public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) { return _baseReflectionType.GetConstructors(bindingAttr); }
            public override FieldInfo GetField(string name, BindingFlags bindingAttr) { return _baseReflectionType.GetField(name, bindingAttr); }
            public override FieldInfo[] GetFields(BindingFlags bindingAttr) { return _baseReflectionType.GetFields(bindingAttr); }
            protected override bool HasElementTypeImpl() { return _baseReflectionType.HasElementType; }
            public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters) { return _baseReflectionType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters); }
            protected override bool IsArrayImpl() { return _baseReflectionType.IsArray; }
            protected override bool IsByRefImpl() { return _baseReflectionType.IsByRef; }
            protected override bool IsCOMObjectImpl() { return _baseReflectionType.IsCOMObject; }
            protected override bool IsPointerImpl() { return _baseReflectionType.IsPointer; }
            protected override bool IsPrimitiveImpl() { return _baseReflectionType.IsPrimitive; }
            public override Module Module { get { return _baseReflectionType.Module; } }
            public override string Namespace { get { return _baseReflectionType.Namespace; } }
            public override Type UnderlyingSystemType { get { return _baseReflectionType.UnderlyingSystemType; } }
            public override string Name { get { return _baseReflectionType.Name; } }
            public override InterfaceMapping GetInterfaceMap(Type interfaceType) { return _baseReflectionType.GetInterfaceMap(interfaceType); }
            public override bool IsAssignableFrom(Type c) { return _baseReflectionType.IsAssignableFrom(c); }
            protected override bool IsContextfulImpl() { return _baseReflectionType.IsContextful; }
            public override bool IsGenericParameter { get { return _baseReflectionType.IsGenericParameter; } }
            public override bool IsGenericType { get { return _baseReflectionType.IsGenericType; } }
            public override bool IsGenericTypeDefinition { get { return _baseReflectionType.IsGenericTypeDefinition; } }
            public override bool IsInstanceOfType(object o) { return _baseReflectionType.IsInstanceOfType(o); }
            protected override bool IsMarshalByRefImpl() { return _baseReflectionType.IsMarshalByRef; }
            public override bool IsSubclassOf(Type c) { return _baseReflectionType.IsSubclassOf(c); }
            protected override bool IsValueTypeImpl() { return _baseReflectionType.IsValueType; }
            public override Type MakeArrayType() { return _baseReflectionType.MakeArrayType(); }
            public override Type MakeArrayType(int rank) { return _baseReflectionType.MakeArrayType(rank); }
            public override Type MakeByRefType() { return _baseReflectionType.MakeByRefType(); }
            public override Type MakeGenericType(params Type[] typeArguments) { return _baseReflectionType.MakeGenericType(typeArguments); }
            public override Type MakePointerType() { return _baseReflectionType.MakePointerType(); }
            public override MemberTypes MemberType { get { return _baseReflectionType.MemberType; } }
            public override int MetadataToken { get { return _baseReflectionType.MetadataToken; } }
            public override Type ReflectedType { get { return _baseReflectionType.ReflectedType; } }
            public override System.Runtime.InteropServices.StructLayoutAttribute StructLayoutAttribute { get { return _baseReflectionType.StructLayoutAttribute; } }
            public override string ToString() { return _baseReflectionType.ToString(); }


            // Type APIs

            //
            // Wrap returned types
            //
            public override Type GetElementType()
            {
                return _provider.MergeType(_baseReflectionType.GetElementType());
            }

            //
            // Wrap returned types
            //
            public override Type DeclaringType
            {
                get { return _provider.MergeType(_baseReflectionType.DeclaringType); }
            }

            //
            // Wrap returned types
            //
            public override Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
            {
                Type[] ifaces = _baseReflectionType.FindInterfaces(filter, filterCriteria);
                for (int idx = 0; idx < ifaces.Length; idx++)
                {
                    ifaces[idx] = _provider.MergeType(ifaces[idx]);
                }
                return ifaces;
            }

            //
            // Wrap returned types
            //
            public override Type BaseType
            {
                get { return _provider.MergeType(_baseReflectionType.BaseType); }
            }

            //
            // Wrap returned types
            //
            public override Type GetInterface(string name, bool ignoreCase)
            {
                return _provider.MergeType(_baseReflectionType.GetInterface(name, ignoreCase));
            }

            //
            // Wrap returned types
            //
            public override Type[] GetInterfaces()
            {
                Type[] ifaces = _baseReflectionType.GetInterfaces();
                for (int idx = 0; idx < ifaces.Length; idx++)
                {
                    ifaces[idx] = _provider.MergeType(ifaces[idx]);
                }
                return ifaces;
            }

            //
            // Wrap returned types
            //
            public override Type GetNestedType(string name, BindingFlags bindingAttr)
            {
                return _provider.MergeType(_baseReflectionType.GetNestedType(name, bindingAttr));
            }

            //
            // Wrap returned types
            //
            public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            {
                Type[] ifaces = _baseReflectionType.GetNestedTypes(bindingAttr);
                for (int idx = 0; idx < ifaces.Length; idx++)
                {
                    ifaces[idx] = _provider.MergeType(ifaces[idx]);
                }
                return ifaces;
            }

            //
            // Wrap returned types
            //
            public override Type[] GetGenericArguments()
            {
                Type[] ifaces = _baseReflectionType.GetGenericArguments();
                for (int idx = 0; idx < ifaces.Length; idx++)
                {
                    ifaces[idx] = _provider.MergeType(ifaces[idx]);
                }
                return ifaces;
            }

            //
            // Wrap returned types
            //
            public override Type[] GetGenericParameterConstraints()
            {
                Type[] ifaces = _baseReflectionType.GetGenericParameterConstraints();
                for (int idx = 0; idx < ifaces.Length; idx++)
                {
                    ifaces[idx] = _provider.MergeType(ifaces[idx]);
                }
                return ifaces;
            }

            //
            // Wrap returned types
            //
            public override Type GetGenericTypeDefinition()
            {
                return _provider.MergeType(_baseReflectionType.GetGenericTypeDefinition());
            }

            //
            // Custom attribute access
            //
            public override object[] GetCustomAttributes(bool inherit)
            {
                return MergeAttributes(null, _baseReflectionType, inherit, ref _cache);
            }

            //
            // Custom attribute access
            //
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return MergeAttributes(attributeType, _baseReflectionType, inherit, ref _cache);
            }

            //
            // Custom attribute access
            //
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                bool isDefined = _baseReflectionType.IsDefined(attributeType, inherit);
                if (!isDefined) isDefined = IsDefinedInTable(attributeType, null, inherit);
                return isDefined;
            }

            //
            // Member access
            //
            public override MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
            {
                MemberInfo[] infos = _baseReflectionType.FindMembers(memberType, bindingAttr, filter, filterCriteria);
                return MergeMembers(infos);
            }

            //
            // Member access
            //
            public override MemberInfo[] GetDefaultMembers()
            {
                MemberInfo[] infos = _baseReflectionType.GetDefaultMembers();
                return MergeMembers(infos);
            }

            //
            // Member access
            //
            public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
            {
                MemberInfo[] infos = _baseReflectionType.GetMember(name, type, bindingAttr);
                return MergeMembers(infos);
            }

            //
            // Member access
            //
            public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
            {
                MemberInfo[] infos = _baseReflectionType.GetMember(name, bindingAttr);
                return MergeMembers(infos);
            }

            //
            // Member access
            //
            public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            {
                MemberInfo[] infos = _baseReflectionType.GetMembers(bindingAttr);
                return MergeMembers(infos);
            }

            //
            // Event access
            //
            public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            {
                EventInfo info = _baseReflectionType.GetEvent(name, bindingAttr);
                return MergeEvent(info);
            }

            //
            // Event access
            //
            public override EventInfo[] GetEvents()
            {
                EventInfo[] infos = _baseReflectionType.GetEvents();
                return MergeEvents(infos);
            }

            //
            // Event access
            //
            public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            {
                EventInfo[] infos = _baseReflectionType.GetEvents(bindingAttr);
                return MergeEvents(infos);
            }

            //
            // Method access
            //
            protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            {
                MethodInfo info;

                if (types == null)
                {
                    info = _baseReflectionType.GetMethod(name, bindingAttr);
                }
                else
                {
                    info = _baseReflectionType.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
                }

                if (info != null)
                {
                    info = MergeMethod(info);
                }

                return info;
            }

            //
            // Method access
            //
            public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            {
                MethodInfo[] infos = _baseReflectionType.GetMethods(bindingAttr);
                return MergeMethods(infos);
            }

            //
            // Property access
            //
            public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            {
                PropertyInfo[] infos = _baseReflectionType.GetProperties(bindingAttr);
                return MergeProperties(infos);
            }

            //
            // Property access
            //
            protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
            {
                PropertyInfo info;
                if (types != null)
                {
                    info = _baseReflectionType.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
                }
                else
                {
                    info = _baseReflectionType.GetProperty(name, bindingAttr);
                }

                return MergeProperty(info);
            }


            // Merging APIs

            //
            // Returns true if the given attribute type is defined in any attribute table.
            // This can take null for the member, in which case it searches type attributes.
            // Table is assigned at construction and read-only, so no locking required.
            //
            private bool IsDefinedInTable(Type attributeType, MemberInfo member, bool inherit)
            {
                bool isDefined = false;
                for (Type t = UnderlyingSystemType; t != null && !isDefined; t = t.BaseType)
                {
                    foreach (AttributeTable table in _tables)
                    {
                        IEnumerable attrEnum;
                        if (member == null)
                        {
                            attrEnum = table.GetCustomAttributes(t);
                        }
                        else
                        {
                            attrEnum = table.GetCustomAttributes(t, member);
                        }

                        foreach (object a in attrEnum)
                        {
                            if (attributeType.IsInstanceOfType(a))
                            {
                                isDefined = true;
                                break;
                            }
                        }

                        if (isDefined) break;
                    }

                    if (!inherit) break;
                }
                return isDefined;
            }

            //
            // Merging of an array of attributes.  This is called from 
            // GetCustomAttributes for both types and members.  Attributes
            // must be merged so the most derived and highest priority
            // attributes are first in the list.  Metadata store attributes
            // always take higher precidence than CLR attributes.
            //
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            private object[] MergeAttributes(Type filterType, MemberInfo member, bool inherit, ref AttributeMergeCache cache)
            {

                Type currentType = UnderlyingSystemType;
                MemberInfo currentMember = member;

                if (cache == null) cache = new AttributeMergeCache();
                if (cache.FilterType != filterType ||
                    cache.Member != member ||
                    cache.Inherit != inherit)
                {

                    cache.FilterType = filterType;
                    cache.Member = member;
                    cache.Inherit = inherit;
                    if (cache.Cache != null) cache.Cache.Clear();
                }
                else if (cache.Cache != null)
                {
                    // Cache is valid; return it
                    return (object[])cache.Cache.ToArray(filterType ?? typeof(object));
                }


                // Cache is invalid or null.  Walk attributes.

                if (cache.Cache == null) cache.Cache = new ArrayList();
                ArrayList compiledAttributes = cache.Cache;

                lock (_syncLock)
                {

                    if (_seenAttributes == null)
                    {
                        _seenAttributes = new Hashtable();
                    }
                    else
                    {
                        _seenAttributes.Clear();
                    }

                    bool firstIteration = true;
                    bool isType = member is Type;
                    bool includeClrAttributes = (isType || currentMember.DeclaringType == currentType);

                    while (currentType != null && currentMember != null)
                    {

                        foreach (object attr in MergeAttributesIterator(currentType, currentMember, includeClrAttributes))
                        {

                            if (filterType != null && !filterType.IsAssignableFrom(attr.GetType()))
                                continue;

                            AttributeData attrData = AttributeDataCache.GetAttributeData(attr.GetType());
                            bool haveSeenBefore = _seenAttributes.ContainsKey(attrData.AttributeType);

                            // Can we have more than one?
                            if (haveSeenBefore &&
                                !attrData.AllowsMultiple)
                                continue;

                            // Is this attribute inheritable?
                            if (!firstIteration &&
                                !attrData.IsInheritable)
                                continue;

                            // We do a scan here for TypeDescriptionProviderAttribute.  Because we
                            // are creating a custom type here and gathering metadata from 
                            // our base type, it is very important that we don't offer this attribute.
                            // Doing so could cause TypeDescriptor to add a provider for this type
                            // and create a cycle
                            if (attrData.AttributeType.Equals(typeof(TypeDescriptionProviderAttribute)))
                                continue;

                            compiledAttributes.Add(attr);
                            _seenAttributes[attrData.AttributeType] = attr;
                        }

                        // Continue?
                        if (!inherit)
                            break;

                        // Advance up the type hierarchy
                        // 
                        // Note: CLR attributes key off of currentMember.  MetadataStore attributes key
                        // off of currentType and memberName.  Because we advance currentType independent of 
                        // currentMember, it is possible for the currentMember to belong to some
                        // ancestor of currentType.  As such, we wait with advancing currentMember until
                        // currentType catches up.  While we wait, we do not include the CLR attributes in
                        // the mix because they would be added multiple times as a result.

                        // Is currentType caught up to currentMember.DeclaringType?
                        if (isType || currentMember.DeclaringType == currentType)
                        {
                            currentMember = AttributeDataCache.GetBaseMemberInfo(currentMember);
                        }

                        currentType = currentType.BaseType;

                        if (isType || currentMember == null || currentMember.DeclaringType == currentType)
                        {
                            includeClrAttributes = true;
                        }
                        else
                        {
                            includeClrAttributes = false;
                        }

                        firstIteration = false;
                    }
                }

                return (object[])compiledAttributes.ToArray(filterType ?? typeof(object));
            }

            // 
            // Enumerates through values contained in the attribute cache in 
            // an order that returns the MetadataStore attributes before returning
            // the CLR attributes (if requested)
            //
            private IEnumerable<object> MergeAttributesIterator(Type type, MemberInfo member, bool includeClrAttributes)
            {
                // MetadataStore uses member name string, instead of MemberInfo to store custom attributes,
                // so figure out what the member name is.  For types, it will be null.
                string memberName = GetMemberName(member);

                // Go through the MetadataStore attributes first, as they take precedence over CLR attributes
                foreach (object attr in AttributeDataCache.GetMetadataStoreAttributes(type, memberName, _tables))
                    yield return attr;

                if (type.IsGenericType && !type.IsGenericTypeDefinition && !string.IsNullOrEmpty(memberName))
                {
                    foreach (object attr in AttributeDataCache.GetMetadataStoreAttributes(type.GetGenericTypeDefinition(), memberName, _tables))
                        yield return attr;
                }

                if (includeClrAttributes)
                {
                    // Go through the CLR attributes second
                    foreach (object attr in AttributeDataCache.GetClrAttributes(member))
                        yield return attr;
                }
            }

            //
            // Gets the member name from the specified MemberInfo that we use as key
            // to get custom attributes from the MetadataStore.  Types will return null.
            //
            private static string GetMemberName(MemberInfo member)
            {
                if (member is Type)
                    return null;

                // The only methods supported in metadata store are those related to
                // Get methods for DependencyProperties and they start with a "Get..."
                if (member.MemberType == MemberTypes.Method)
                {
                    Fx.Assert(
                        member.Name.StartsWith("Get", StringComparison.Ordinal) &&
                        member.Name.Length > 3,
                        "MetadataStore expects to only see Get[Foo] or Set[Foo] MethodInfos");

                    return member.Name.Substring(3);
                }

                return member.Name;
            }

            //
            // Merging of an event.
            //
            private EventInfo MergeEvent(EventInfo info)
            {
                if (info == null) return null;

                MemberInfo cache;

                lock (_syncLock)
                {
                    if (_memberCache == null || !_memberCache.TryGetValue(info, out cache))
                    {

                        if (_memberCache == null)
                        {
                            _memberCache = new Dictionary<MemberInfo, MemberInfo>();
                        }

                        EventInfo newInfo = new MetadataEventInfo(info, this);
                        _memberCache[info] = newInfo;
                        info = newInfo;
                    }
                    else
                    {
                        info = (EventInfo)cache;
                    }
                }

                return info;
            }

            //
            // Merging of an array of events.
            //
            private EventInfo[] MergeEvents(EventInfo[] infos)
            {
                for (int idx = 0; idx < infos.Length; idx++)
                {
                    infos[idx] = MergeEvent(infos[idx]);
                }
                return infos;
            }

            //
            // Merging a generic member calls down to specific
            // merge functions based on the type of the member.
            // 
            private MemberInfo MergeMember(MemberInfo info)
            {
                MethodInfo m;
                PropertyInfo p;
                EventInfo e;

                if ((m = info as MethodInfo) != null)
                {
                    return MergeMethod(m);
                }
                else if ((p = info as PropertyInfo) != null)
                {
                    return MergeProperty(p);
                }
                else if ((e = info as EventInfo) != null)
                {
                    return MergeEvent(e);
                }

                return info;
            }

            //
            // Merging a generic member calls down to specific
            // merge functions based on the type of the member.
            // 
            private MemberInfo[] MergeMembers(MemberInfo[] infos)
            {
                for (int idx = 0; idx < infos.Length; idx++)
                {
                    infos[idx] = MergeMember(infos[idx]);
                }
                return infos;
            }

            //
            // Merging of a method.  We only deal with 
            // static methods in the format Get<member name>
            // so we can support attached properties.  Otherwise,
            // we let the method be.
            //
            private MethodInfo MergeMethod(MethodInfo info)
            {
                if (info == null) return null;

                if (info.IsStatic && info.Name.StartsWith("Get", StringComparison.Ordinal))
                {
                    MemberInfo cache;

                    lock (_syncLock)
                    {
                        if (_memberCache == null || !_memberCache.TryGetValue(info, out cache))
                        {

                            if (_memberCache == null)
                            {
                                _memberCache = new Dictionary<MemberInfo, MemberInfo>();
                            }

                            MethodInfo newInfo = new MetadataMethodInfo(info, this);
                            _memberCache[info] = newInfo;
                            info = newInfo;
                        }
                        else
                        {
                            info = (MethodInfo)cache;
                        }
                    }
                }
                return info;
            }

            //
            // Merging of an array of methods.
            //
            private MethodInfo[] MergeMethods(MethodInfo[] infos)
            {
                for (int idx = 0; idx < infos.Length; idx++)
                {
                    infos[idx] = MergeMethod(infos[idx]);
                }
                return infos;
            }

            //
            // Merging of a property.
            //
            private PropertyInfo MergeProperty(PropertyInfo info)
            {
                if (info == null) return null;

                MemberInfo cache;

                lock (_syncLock)
                {
                    if (_memberCache == null || !_memberCache.TryGetValue(info, out cache))
                    {

                        if (_memberCache == null)
                        {
                            _memberCache = new Dictionary<MemberInfo, MemberInfo>();
                        }

                        PropertyInfo newInfo = new MetadataPropertyInfo(info, this);
                        _memberCache[info] = newInfo;
                        info = newInfo;
                    }
                    else
                    {
                        info = (PropertyInfo)cache;
                    }
                }

                return info;
            }

            //
            // Merging of an array of properties.
            //
            private PropertyInfo[] MergeProperties(PropertyInfo[] infos)
            {
                for (int idx = 0; idx < infos.Length; idx++)
                {
                    infos[idx] = MergeProperty(infos[idx]);
                }
                return infos;
            }


            //
            // A simple class that holds merged attribute data.
            //
            private class AttributeMergeCache
            {
                internal Type FilterType;
                internal bool Inherit;
                internal ArrayList Cache;
                internal MemberInfo Member;
            }

            //
            // A property info that contains our custom metadata.
            //
            private class MetadataPropertyInfo : PropertyInfo
            {

                private PropertyInfo _info;
                private MetadataType _type;
                private AttributeMergeCache _cache;

                internal MetadataPropertyInfo(PropertyInfo info, MetadataType type)
                {
                    _info = info;
                    _type = type;
                }

                //
                // PropertyInfo overrides
                //
                public override PropertyAttributes Attributes { get { return _info.Attributes; } }
                public override bool CanRead { get { return _info.CanRead; } }
                public override bool CanWrite { get { return _info.CanWrite; } }
                public override MethodInfo[] GetAccessors(bool nonPublic) { return _info.GetAccessors(nonPublic); }
                public override MethodInfo GetGetMethod(bool nonPublic) { return _info.GetGetMethod(nonPublic); }
                public override ParameterInfo[] GetIndexParameters() { return _info.GetIndexParameters(); }
                public override MethodInfo GetSetMethod(bool nonPublic) { return _info.GetSetMethod(nonPublic); }
                public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) { return _info.GetValue(obj, invokeAttr, binder, index, culture); }
                public override Type PropertyType { get { return _info.PropertyType; } }
                public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) { _info.SetValue(obj, value, invokeAttr, binder, index, culture); }
                public override Type DeclaringType { get { return _info.DeclaringType; } }
                public override string Name { get { return _info.Name; } }
                public override Type ReflectedType { get { return _info.ReflectedType; } }
                public override bool Equals(object obj) { return object.ReferenceEquals(this, obj); }
                public override int GetHashCode() { return _info.GetHashCode(); }
                public override object GetConstantValue() { return _info.GetConstantValue(); }
                public override Type[] GetOptionalCustomModifiers() { return _info.GetOptionalCustomModifiers(); }
                public override object GetRawConstantValue() { return _info.GetRawConstantValue(); }
                public override Type[] GetRequiredCustomModifiers() { return _info.GetRequiredCustomModifiers(); }
                public override object GetValue(object obj, object[] index) { return _info.GetValue(obj, index); }
                public override MemberTypes MemberType { get { return _info.MemberType; } }
                public override int MetadataToken { get { return _info.MetadataToken; } }
                public override Module Module { get { return _info.Module; } }
                public override void SetValue(object obj, object value, object[] index) { _info.SetValue(obj, value, index); }
                public override string ToString() { return _info.ToString(); }

                //
                // Merges our custom attributes in with those of the member info.
                //
                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    return _type.MergeAttributes(attributeType, _info, inherit, ref _cache);
                }

                //
                // Merges our custom attributes in with those of the member info.
                //
                public override object[] GetCustomAttributes(bool inherit)
                {
                    return _type.MergeAttributes(null, _info, inherit, ref _cache);
                }

                //
                // Determines if an attribute exists in the member info
                // or in our own code.
                //
                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    bool isDefined = _info.IsDefined(attributeType, inherit);
                    if (!isDefined) isDefined = _type.IsDefinedInTable(attributeType, _info, inherit);
                    return isDefined;
                }
            }

            //
            // An EventInfo that provides our custom attributes.
            //
            private class MetadataEventInfo : EventInfo
            {

                private EventInfo _info;
                private MetadataType _type;
                private AttributeMergeCache _cache;

                internal MetadataEventInfo(EventInfo info, MetadataType type)
                {
                    _info = info;
                    _type = type;
                }

                //
                // EventInfo overrides
                //
                public override EventAttributes Attributes { get { return _info.Attributes; } }
                public override string Name { get { return _info.Name; } }
                public override Type ReflectedType { get { return _info.ReflectedType; } }
                public override bool Equals(object obj) { return object.ReferenceEquals(this, obj); }
                public override int GetHashCode() { return _info.GetHashCode(); }
                public override MethodInfo[] GetOtherMethods(bool nonPublic) { return _info.GetOtherMethods(nonPublic); }
                public override MemberTypes MemberType { get { return _info.MemberType; } }
                public override int MetadataToken { get { return _info.MetadataToken; } }
                public override Module Module { get { return _info.Module; } }
                public override string ToString() { return _info.ToString(); }
                public override MethodInfo GetAddMethod(bool nonPublic) { return _info.GetAddMethod(nonPublic); }
                public override MethodInfo GetRaiseMethod(bool nonPublic) { return _info.GetRaiseMethod(nonPublic); }
                public override MethodInfo GetRemoveMethod(bool nonPublic) { return _info.GetRemoveMethod(nonPublic); }
                public override Type DeclaringType { get { return _info.DeclaringType; } }

                //
                // Merges our custom attributes in with those of the member info.
                //
                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    return _type.MergeAttributes(attributeType, _info, inherit, ref _cache);
                }

                //
                // Merges our custom attributes in with those of the member info.
                //
                public override object[] GetCustomAttributes(bool inherit)
                {
                    return _type.MergeAttributes(null, _info, inherit, ref _cache);
                }

                //
                // Determines if an attribute exists in the member info
                // or in our own code.
                //
                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    bool isDefined = _info.IsDefined(attributeType, inherit);
                    if (!isDefined) isDefined = _type.IsDefinedInTable(attributeType, _info, inherit);
                    return isDefined;
                }
            }

            //
            // A MethodInfo that provides our custom attributes.
            //
            private class MetadataMethodInfo : MethodInfo
            {

                private MethodInfo _info;
                private MetadataType _type;
                private AttributeMergeCache _cache;

                internal MetadataMethodInfo(MethodInfo info, MetadataType type)
                {
                    _info = info;
                    _type = type;
                }

                //
                // MethodInfo overrides
                //
                public override ICustomAttributeProvider ReturnTypeCustomAttributes { get { return _info.ReturnTypeCustomAttributes; } }
                public override MethodAttributes Attributes { get { return _info.Attributes; } }
                public override MethodImplAttributes GetMethodImplementationFlags() { return _info.GetMethodImplementationFlags(); }
                public override ParameterInfo[] GetParameters() { return _info.GetParameters(); }
                public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) { return _info.Invoke(obj, invokeAttr, binder, parameters, culture); }
                public override RuntimeMethodHandle MethodHandle { get { return _info.MethodHandle; } }
                public override Type DeclaringType { get { return _info.DeclaringType; } }
                public override string Name { get { return _info.Name; } }
                public override Type ReflectedType { get { return _info.ReflectedType; } }
                public override bool Equals(object obj) { return object.ReferenceEquals(this, obj); }
                public override int GetHashCode() { return _info.GetHashCode(); }
                public override CallingConventions CallingConvention { get { return _info.CallingConvention; } }
                public override bool ContainsGenericParameters { get { return _info.ContainsGenericParameters; } }
                public override MethodInfo GetBaseDefinition() { return _info.GetBaseDefinition(); }
                public override Type[] GetGenericArguments() { return _info.GetGenericArguments(); }
                public override MethodInfo GetGenericMethodDefinition() { return _info.GetGenericMethodDefinition(); }
                public override MethodBody GetMethodBody() { return _info.GetMethodBody(); }
                public override bool IsGenericMethod { get { return _info.IsGenericMethod; } }
                public override bool IsGenericMethodDefinition { get { return _info.IsGenericMethodDefinition; } }
                public override MethodInfo MakeGenericMethod(params Type[] typeArguments) { return _info.MakeGenericMethod(typeArguments); }
                public override MemberTypes MemberType { get { return _info.MemberType; } }
                public override int MetadataToken { get { return _info.MetadataToken; } }
                public override Module Module { get { return _info.Module; } }
                public override ParameterInfo ReturnParameter { get { return _info.ReturnParameter; } }
                public override Type ReturnType { get { return _info.ReturnType; } }
                public override string ToString() { return _info.ToString(); }

                //
                // Merges our custom attributes in with those of the member info.
                //
                public override object[] GetCustomAttributes(Type attributeType, bool inherit)
                {
                    return _type.MergeAttributes(attributeType, _info, inherit, ref _cache);
                }

                //
                // Merges our custom attributes in with those of the member info.
                //
                public override object[] GetCustomAttributes(bool inherit)
                {
                    return _type.MergeAttributes(null, _info, inherit, ref _cache);
                }

                //
                // Determines if an attribute exists in the member info
                // or in our own code.
                //
                public override bool IsDefined(Type attributeType, bool inherit)
                {
                    bool isDefined = _info.IsDefined(attributeType, inherit);
                    if (!isDefined) isDefined = _type.IsDefinedInTable(attributeType, _info, inherit);
                    return isDefined;
                }
            }
        }
    }
}
