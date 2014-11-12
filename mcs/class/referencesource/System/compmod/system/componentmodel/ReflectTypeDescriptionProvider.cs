//------------------------------------------------------------------------------
// <copyright file="ReflectTypeDescriptionProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
   
    /// <devdoc>
    ///     This type description provider provides type information through 
    ///     reflection.  Unless someone has provided a custom type description
    ///     provider for a type or instance, or unless an instance implements
    ///     ICustomTypeDescriptor, any query for type information will go through
    ///     this class.  There should be a single instance of this class associated
    ///     with "object", as it can provide all type information for any type.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    internal sealed class ReflectTypeDescriptionProvider : TypeDescriptionProvider {

        // Hastable of Type -> ReflectedTypeData.  ReflectedTypeData contains all
        // of the type information we have gathered for a given type.
        //
        private Hashtable _typeData;

        // This is the signature we look for when creating types that are generic, but
        // want to know what type they are dealing with.  Enums are a good example of this;
        // there is one enum converter that can work with all enums, but it needs to know
        // the type of enum it is dealing with.
        //
        private static Type[] _typeConstructor = new Type[] {typeof(Type)};

        // This is where we store the various converters, etc for the intrinsic types.
        //
        private static volatile Hashtable _editorTables;
        private static volatile Hashtable _intrinsicTypeConverters;

        // For converters, etc that are bound to class attribute data, rather than a class
        // type, we have special key sentinel values that we put into the hash table.
        //
        private static object _intrinsicReferenceKey = new object();
        private static object _intrinsicNullableKey = new object();

        // The key we put into IDictionaryService to store our cache dictionary.
        //
        private static object _dictionaryKey = new object();

        // This is a cache on top of core reflection.  The cache
        // builds itself recursively, so if you ask for the properties
        // on Control, Component and object are also automatically filled
        // in.  The keys to the property and event caches are types.
        // The keys to the attribute cache are either MemberInfos or types.
        //
        private static volatile Hashtable _propertyCache;
        private static volatile Hashtable _eventCache;
        private static volatile Hashtable _attributeCache;
        private static volatile Hashtable _extendedPropertyCache;

        // These are keys we stuff into our object cache.  We use this
        // cache data to store extender provider info for an object.
        //
        private static readonly Guid _extenderProviderKey = Guid.NewGuid();
        private static readonly Guid _extenderPropertiesKey = Guid.NewGuid();
        private static readonly Guid _extenderProviderPropertiesKey = Guid.NewGuid();

        // These are attribute that, when we discover them on interfaces, we do
        // not merge them into the attribute set for a class.
        private static readonly Type[] _skipInterfaceAttributeList = new Type[]
        {
            typeof(System.Runtime.InteropServices.GuidAttribute),
            typeof(System.Runtime.InteropServices.ComVisibleAttribute),
            typeof(System.Runtime.InteropServices.InterfaceTypeAttribute)
        };


        internal static Guid ExtenderProviderKey {
            get {
                return _extenderProviderKey;
            }
        }


        private static object _internalSyncObject = new object();
        /// <devdoc>
        ///     Creates a new ReflectTypeDescriptionProvider.  The type is the
        ///     type we will obtain type information for.
        /// </devdoc>
        internal ReflectTypeDescriptionProvider()
        {
            TypeDescriptor.Trace("Reflect : Creating ReflectTypeDescriptionProvider");
        }

        /// <devdoc> 
        ///      This is a table we create for intrinsic types. 
        ///      There should be entries here ONLY for intrinsic 
        ///      types, as all other types we should be able to 
        ///      add attributes directly as metadata. 
        /// </devdoc> 
        private static Hashtable IntrinsicTypeConverters {
            get {
                // It is not worth taking a lock for this -- worst case of a collision
                // would build two tables, one that garbage collects very quickly.
                //
                if (_intrinsicTypeConverters == null) {
                    Hashtable temp = new Hashtable();

                    // Add the intrinsics
                    //
                    temp[typeof(bool)] = typeof(BooleanConverter);
                    temp[typeof(byte)] = typeof(ByteConverter);
                    temp[typeof(SByte)] = typeof(SByteConverter);
                    temp[typeof(char)] = typeof(CharConverter);
                    temp[typeof(double)] = typeof(DoubleConverter);
                    temp[typeof(string)] = typeof(StringConverter);
                    temp[typeof(int)] = typeof(Int32Converter);
                    temp[typeof(short)] = typeof(Int16Converter);
                    temp[typeof(long)] = typeof(Int64Converter);
                    temp[typeof(float)] = typeof(SingleConverter);
                    temp[typeof(UInt16)] = typeof(UInt16Converter);
                    temp[typeof(UInt32)] = typeof(UInt32Converter);
                    temp[typeof(UInt64)] = typeof(UInt64Converter);
                    temp[typeof(object)] = typeof(TypeConverter);
                    temp[typeof(void)] = typeof(TypeConverter);
                    temp[typeof(CultureInfo)] = typeof(CultureInfoConverter);
                    temp[typeof(DateTime)] = typeof(DateTimeConverter);
                    temp[typeof(DateTimeOffset)] = typeof(DateTimeOffsetConverter);
                    temp[typeof(Decimal)] = typeof(DecimalConverter);
                    temp[typeof(TimeSpan)] = typeof(TimeSpanConverter);
                    temp[typeof(Guid)] = typeof(GuidConverter);
                    temp[typeof(Array)] = typeof(ArrayConverter);
                    temp[typeof(ICollection)] = typeof(CollectionConverter);
                    temp[typeof(Enum)] = typeof(EnumConverter);

                    // Special cases for things that are not bound to a specific type
                    //
                    temp[_intrinsicReferenceKey] = typeof(ReferenceConverter);
                    temp[_intrinsicNullableKey] = typeof(NullableConverter);

                    _intrinsicTypeConverters = temp;
                }
                return _intrinsicTypeConverters;
            }
        }

        /// <devdoc>
        ///     Adds an editor table for the given editor base type.
        ///     ypically, editors are specified as metadata on an object. If no metadata for a
        ///     equested editor base type can be found on an object, however, the
        ///     ypeDescriptor will search an editor
        ///     able for the editor type, if one can be found.
        /// </devdoc>
        internal static void AddEditorTable(Type editorBaseType, Hashtable table) 
        {
            if (editorBaseType == null)
            {
                throw new ArgumentNullException("editorBaseType");
            }

            if (table == null)
            {
                Debug.Fail("COMPAT: Editor table should not be null");
                // don't throw; RTM didn't so we can't do it either.
            }

            lock(_internalSyncObject) 
            {
                if (_editorTables == null)
                {
                    _editorTables = new Hashtable(4);
                }

                if (!_editorTables.ContainsKey(editorBaseType)) 
                {
                    _editorTables[editorBaseType] = table;
                }
            }
        }

        /// <devdoc>
        ///     CreateInstance implementation.  We delegate to Activator.
        /// </devdoc>
        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            Debug.Assert(objectType != null, "Should have arg-checked before coming in here");

            object obj = null;
            
            if (argTypes != null)
            {
                obj = SecurityUtils.SecureConstructorInvoke(objectType, argTypes, args, true, BindingFlags.ExactBinding);
            }
            else {
                if (args != null) {
                    argTypes = new Type[args.Length];
                    for(int idx = 0; idx < args.Length; idx++) {
                        if (args[idx] != null) {
                            argTypes[idx] = args[idx].GetType();
                        }
                        else {
                            argTypes[idx] = typeof(object);
                        }
                    }
                }
                else {
                    argTypes = new Type[0];
                }

                obj = SecurityUtils.SecureConstructorInvoke(objectType, argTypes, args, true);
            }

            if (obj == null) {
                obj = SecurityUtils.SecureCreateInstance(objectType, args);
            }

            return obj;
        }


        /// <devdoc> 
        ///     Helper method to create editors and type converters. This checks to see if the
        ///     type implements a Type constructor, and if it does it invokes that ctor. 
        ///     Otherwise, it just tries to create the type.
        /// </devdoc> 
        private static object CreateInstance(Type objectType, Type callingType) {
            object obj = SecurityUtils.SecureConstructorInvoke(objectType, _typeConstructor, new object[] {callingType}, false);

            if (obj == null) {
                obj = SecurityUtils.SecureCreateInstance(objectType);
            }

            return obj;
        }

        /// <devdoc>
        ///     Retrieves custom attributes.
        /// </devdoc>
        internal AttributeCollection GetAttributes(Type type)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetAttributes();
        }

        /// <devdoc>
        ///     Our implementation of GetCache sits on top of IDictionaryService.
        /// </devdoc>
        public override IDictionary GetCache(object instance)
        {
            IComponent comp = instance as IComponent;
            if (comp != null && comp.Site != null)
            {
                IDictionaryService ds = comp.Site.GetService(typeof(IDictionaryService)) as IDictionaryService;
                if (ds != null)
                {
                    IDictionary dict = ds.GetValue(_dictionaryKey) as IDictionary;
                    if (dict == null)
                    {
                        dict = new Hashtable();
                        ds.SetValue(_dictionaryKey, dict);
                    }
                    return dict;
                }
            }

            return null;
        }

        /// <devdoc>
        ///     Retrieves the class name for our type.
        /// </devdoc>
        internal string GetClassName(Type type)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetClassName(null);
        }

        /// <devdoc>
        ///     Retrieves the component name from the site.
        /// </devdoc>
        internal string GetComponentName(Type type, object instance)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetComponentName(instance);
        }

        /// <devdoc>
        ///     Retrieves the type converter.  If instance is non-null,
        ///     it will be used to retrieve attributes.  Otherwise, _type
        ///     will be used.
        /// </devdoc>
        internal TypeConverter GetConverter(Type type, object instance)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetConverter(instance);
        }

        /// <devdoc>
        ///     Return the default event. The default event is determined by the
        ///     presence of a DefaultEventAttribute on the class.
        /// </devdoc>
        internal EventDescriptor GetDefaultEvent(Type type, object instance)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetDefaultEvent(instance);
        }

        /// <devdoc>
        ///     Return the default property.
        /// </devdoc>
        internal PropertyDescriptor GetDefaultProperty(Type type, object instance)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetDefaultProperty(instance);
        }

        /// <devdoc>
        ///     Retrieves the editor for the given base type.
        /// </devdoc>
        internal object GetEditor(Type type, object instance, Type editorBaseType)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetEditor(instance, editorBaseType);
        }

        /// <devdoc> 
        ///      Retrieves a default editor table for the given editor base type. 
        /// </devdoc> 
        private static Hashtable GetEditorTable(Type editorBaseType) {

            if (_editorTables == null)
            {
                lock(_internalSyncObject)
                {
                    if (_editorTables == null)
                    {
                        _editorTables = new Hashtable(4);
                    }
                }
            }

            object table = _editorTables[editorBaseType];
            
            if (table == null) 
            {
                // Before we give up, it is possible that the
                // class initializer for editorBaseType hasn't 
                // actually run.
                //
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(editorBaseType.TypeHandle);
                table = _editorTables[editorBaseType];

                // If the table is still null, then throw a
                // sentinel in there so we don't
                // go through this again.
                //
                if (table == null)
                {
                    lock (_internalSyncObject)
                    {
                        table = _editorTables[editorBaseType];
                        if (table == null) 
                        {
                            _editorTables[editorBaseType] = _editorTables;
                        }
                    }
                }
            }
            
            // Look for our sentinel value that indicates
            // we have already tried and failed to get
            // a table.
            //
            if (table == _editorTables) 
            {
                table = null;
            }
            
            return (Hashtable)table;
        }
        
        /// <devdoc>
        ///     Retrieves the events for this type.
        /// </devdoc>
        internal EventDescriptorCollection GetEvents(Type type)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetEvents();
        }

        /// <devdoc>
        ///     Retrieves custom extender attributes. We don't support
        ///     extender attributes, so we always return an empty collection.
        /// </devdoc>
        internal AttributeCollection GetExtendedAttributes(object instance)
        {
            return AttributeCollection.Empty;
        }

        /// <devdoc>
        ///     Retrieves the class name for our type.
        /// </devdoc>
        internal string GetExtendedClassName(object instance)
        {
            return GetClassName(instance.GetType());
        }

        /// <devdoc>
        ///     Retrieves the component name from the site.
        /// </devdoc>
        internal string GetExtendedComponentName(object instance)
        {
            return GetComponentName(instance.GetType(), instance);
        }

        /// <devdoc>
        ///     Retrieves the type converter.  If instance is non-null,
        ///     it will be used to retrieve attributes.  Otherwise, _type
        ///     will be used.
        /// </devdoc>
        internal TypeConverter GetExtendedConverter(object instance)
        {
            return GetConverter(instance.GetType(), instance);
        }

        /// <devdoc>
        ///     Return the default event. The default event is determined by the
        ///     presence of a DefaultEventAttribute on the class.
        /// </devdoc>
        internal EventDescriptor GetExtendedDefaultEvent(object instance)
        {
            return null; // we don't support extended events.
        }

        /// <devdoc>
        ///     Return the default property.
        /// </devdoc>
        internal PropertyDescriptor GetExtendedDefaultProperty(object instance)
        {
            return null; // extender properties are never the default.
        }

        /// <devdoc>
        ///     Retrieves the editor for the given base type.
        /// </devdoc>
        internal object GetExtendedEditor(object instance, Type editorBaseType)
        {
            return GetEditor(instance.GetType(), instance, editorBaseType);
        }
        
        /// <devdoc>
        ///     Retrieves the events for this type.
        /// </devdoc>
        internal EventDescriptorCollection GetExtendedEvents(object instance)
        {
            return EventDescriptorCollection.Empty;
        }

        /// <devdoc>
        ///     Retrieves the properties for this type.
        /// </devdoc>
        internal PropertyDescriptorCollection GetExtendedProperties(object instance)
        {
            // Is this object a sited component?  If not, then it
            // doesn't have any extender properties.
            //
            Type componentType = instance.GetType();

            // Check the component for extender providers.  We prefer
            // IExtenderListService, but will use the container if that's
            // all we have.  In either case, we check the list of extenders
            // against previously stored data in the object cache.  If
            // the cache is up to date, we just return the extenders in the
            // cache.
            //
            IExtenderProvider[] extenders = GetExtenderProviders(instance);
            IDictionary cache = TypeDescriptor.GetCache(instance);

            if (extenders.Length == 0)
            {
                return PropertyDescriptorCollection.Empty;
            }

            // Ok, we have a set of extenders.  Now, check to see if there
            // are properties already in our object cache.  If there aren't,
            // then we will need to create them.
            //
            PropertyDescriptorCollection properties = null;

            if (cache != null)
            {
                properties = cache[_extenderPropertiesKey] as PropertyDescriptorCollection;
            }

            if (properties != null)
            {
                return properties;
            }

            // Unlike normal properties, it is fine for there to be properties with
            // duplicate names here.  
            //
            ArrayList propertyList = null;

            for (int idx = 0; idx < extenders.Length; idx++)
            {
                PropertyDescriptor[] propertyArray = ReflectGetExtendedProperties(extenders[idx]);

                if (propertyList == null)
                {
                    propertyList = new ArrayList(propertyArray.Length * extenders.Length);
                }

                for (int propIdx = 0; propIdx < propertyArray.Length; propIdx++)
                {
                    PropertyDescriptor prop = propertyArray[propIdx];
                    ExtenderProvidedPropertyAttribute eppa = prop.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;

                    Debug.Assert(eppa != null, "Extender property " + prop.Name + " has no provider attribute.  We will skip it.");
                    if (eppa != null)
                    {
                        Type receiverType = eppa.ReceiverType;
                        if (receiverType != null) 
                        {

                            if (receiverType.IsAssignableFrom(componentType)) 
                            {
                                propertyList.Add(prop);
                            }
                        }
                    }
                }
            }

            // propertyHash now contains ExtendedPropertyDescriptor objects
            // for each extended property.
            //
            if (propertyList != null)
            {
                TypeDescriptor.Trace("Extenders : Allocating property collection for {0} properties", propertyList.Count);
                PropertyDescriptor[] fullArray = new PropertyDescriptor[propertyList.Count];
                propertyList.CopyTo(fullArray, 0);
                properties = new PropertyDescriptorCollection(fullArray, true);
            }
            else
            {
                properties = PropertyDescriptorCollection.Empty;
            }

            if (cache != null)
            {
                TypeDescriptor.Trace("Extenders : caching extender results");
                cache[_extenderPropertiesKey] = properties;
            }

            return properties;
        }

        protected internal override IExtenderProvider[] GetExtenderProviders(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            IComponent component = instance as IComponent;
            if (component != null && component.Site != null)
            {
                IExtenderListService extenderList = component.Site.GetService(typeof(IExtenderListService)) as IExtenderListService;
                IDictionary cache = TypeDescriptor.GetCache(instance);

                if (extenderList != null)
                {
                    return GetExtenders(extenderList.GetExtenderProviders(), instance, cache);
                }
                else
                {
                    IContainer cont = component.Site.Container;
                    if (cont != null)
                    {
                        return GetExtenders(cont.Components, instance, cache);
                    }
                }
            }
            return new IExtenderProvider[0];
        }

        /// <devdoc>
        ///     GetExtenders builds a list of extender providers from
        ///     a collection of components.  It validates the extenders
        ///     against any cached collection of extenders in the 
        ///     cache.  If there is a discrepancy, this will erase
        ///     any cached extender properties from the cache and
        ///     save the updated extender list.  If there is no
        ///     discrepancy this will simply return the cached list.
        /// </devdoc>
        private static IExtenderProvider[] GetExtenders(ICollection components, object instance, IDictionary cache)
        {
            bool newExtenders = false;
            int extenderCount = 0;
            IExtenderProvider[] existingExtenders = null;
    
            //CanExtend is expensive. We will remember results of CanExtend for the first 64 extenders and using "long canExtend" as a bit vector.
            // we want to avoid memory allocation as well so we don't use some more sophisticated data structure like an array of booleans
            UInt64 canExtend = 0;
            int maxCanExtendResults = 64;
            // currentExtenders is what we intend to return.  If the caller passed us
            // the return value from IExtenderListService, components will already be
            // an IExtenderProvider[].  If not, then we must treat components as an
            // opaque collection.  We spend a great deal of energy here to avoid
            // copying or allocating memory because this method is called every
            // time a component is asked for its properties.
            IExtenderProvider[] currentExtenders = components as IExtenderProvider[];

            if (cache != null)
            {
                existingExtenders = cache[_extenderProviderKey] as IExtenderProvider[];
            }

            if (existingExtenders == null)
            {
                newExtenders = true;
            }

            int curIdx = 0;
            int idx = 0;
           
            if (currentExtenders != null)
            {
                for (curIdx = 0; curIdx < currentExtenders.Length; curIdx++)
                {
                    if (currentExtenders[curIdx].CanExtend(instance))
                    {
                        extenderCount++;
                        // Performance:We would like to call CanExtend as little as possible therefore we remember its result
                        if (curIdx < maxCanExtendResults)
                            canExtend |= (UInt64)1 << curIdx;
                        if (!newExtenders && (idx >= existingExtenders.Length || currentExtenders[curIdx] != existingExtenders[idx++]))
                        {
                            newExtenders = true;
                        }
                    }
                }
            }
            else if (components != null)
            {
                foreach(object obj in components)
                {
                    IExtenderProvider prov = obj as IExtenderProvider;
                    if (prov != null && prov.CanExtend(instance))
                    {
                        extenderCount++;
                        if (curIdx < maxCanExtendResults)
                            canExtend |= (UInt64)1<<curIdx;
                        if (!newExtenders && (idx >= existingExtenders.Length || prov != existingExtenders[idx++]))
                        {
                            newExtenders = true;
                        }
                    }
                    curIdx++;
                }
            }
            if (existingExtenders != null && extenderCount != existingExtenders.Length)
            {
                newExtenders = true;
            }
            if (newExtenders)
            {
                TypeDescriptor.Trace("Extenders : object has new extenders : {0}", instance.GetType().Name);
                TypeDescriptor.Trace("Extenders : Identified {0} extender providers", extenderCount);
                if (currentExtenders == null || extenderCount != currentExtenders.Length)
                {
                    IExtenderProvider[] newExtenderArray = new IExtenderProvider[extenderCount];

                    curIdx = 0;
                    idx = 0;

                    if (currentExtenders != null && extenderCount > 0)
                    {
                        while(curIdx < currentExtenders.Length)
                        { 
                            if ((curIdx < maxCanExtendResults && (canExtend & ((UInt64)1 << curIdx)) != 0 )|| 
                                            (curIdx >= maxCanExtendResults && currentExtenders[curIdx].CanExtend(instance)))
                            {
                                Debug.Assert(idx < extenderCount, "There are more extenders than we expect");
                                newExtenderArray[idx++] = currentExtenders[curIdx];
                            }
                            curIdx++;                      
                        }
                        Debug.Assert(idx == extenderCount, "Wrong number of extenders");
                    }
                    else if (extenderCount > 0)
                    {
                        IEnumerator componentEnum = components.GetEnumerator();
                        while(componentEnum.MoveNext())
                        {
                            IExtenderProvider p = componentEnum.Current as IExtenderProvider;

                            if (p != null && ((curIdx < maxCanExtendResults && (canExtend & ((UInt64)1 << curIdx)) != 0) ||
                                                (curIdx >= maxCanExtendResults && p.CanExtend(instance))))
                            {
                                Debug.Assert(idx < extenderCount, "There are more extenders than we expect");
                                newExtenderArray[idx++] = p;
                            }
                            curIdx++;
                        }
                        Debug.Assert(idx == extenderCount, "Wrong number of extenders");
                    }
                    currentExtenders = newExtenderArray;
                }

                if (cache != null)
                {
                    TypeDescriptor.Trace("Extenders : caching extender provider results");
                    cache[_extenderProviderKey] = currentExtenders;
                    cache.Remove(_extenderPropertiesKey);
                }
            }
            else
            {
                currentExtenders = existingExtenders;
            }
            return currentExtenders;
        }

        /// <devdoc>
        ///     Retrieves the owner for a property.
        /// </devdoc>
        internal object GetExtendedPropertyOwner(object instance, PropertyDescriptor pd)
        {
            return GetPropertyOwner(instance.GetType(), instance, pd);
        }

        //////////////////////////////////////////////////////////
        /// <devdoc>
        ///     Provides a type descriptor for the given object.  We only support this
        ///     if the object is a component that 
        /// </devdoc>
        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            Debug.Fail("This should never be invoked.  TypeDescriptionNode should wrap for us.");
            return null;
        }

        /// <devdoc>
        ///     The name of the specified component, or null if the component has no name.
        ///     In many cases this will return the same value as GetComponentName. If the
        ///     component resides in a nested container or has other nested semantics, it may
        ///     return a different fully qualfied name.
        ///
        ///     If not overridden, the default implementation of this method will call
        ///     GetComponentName.
        /// </devdoc>
        public override string GetFullComponentName(object component) {
            IComponent comp = component as IComponent;
            if (comp != null) {
                INestedSite ns = comp.Site as INestedSite;
                if (ns != null) {
                    return ns.FullName;
                }
            }

            return TypeDescriptor.GetComponentName(component);
        }

        /// <devdoc>
        ///     Returns an array of types we have populated metadata for that live
        ///     in the current module.
        /// </devdoc>
        internal Type[] GetPopulatedTypes(Module module) {
            ArrayList typeList = new ArrayList();;

            foreach(DictionaryEntry de in _typeData) {
                Type type = (Type)de.Key;
                ReflectedTypeData typeData = (ReflectedTypeData)de.Value;

                if (type.Module == module && typeData.IsPopulated) {
                    typeList.Add(type);
                }
            }

            return (Type[])typeList.ToArray(typeof(Type));
        }

        /// <devdoc>
        ///     Retrieves the properties for this type.
        /// </devdoc>
        internal PropertyDescriptorCollection GetProperties(Type type)
        {
            ReflectedTypeData td = GetTypeData(type, true);
            return td.GetProperties();
        }

        /// <devdoc>
        ///     Retrieves the owner for a property.
        /// </devdoc>
        internal object GetPropertyOwner(Type type, object instance, PropertyDescriptor pd)
        {
            return TypeDescriptor.GetAssociation(type, instance);
        }

        /// <devdoc>
        ///     Returns an Type for the given type.  Since type implements IReflect,
        ///     we just return objectType.
        /// </devdoc>
        public override Type GetReflectionType(Type objectType, object instance)
        {
            Debug.Assert(objectType != null, "Should have arg-checked before coming in here");
            return objectType;
        }

        /// <devdoc>
        ///     Returns the type data for the given type, or
        ///     null if there is no type data for the type yet and
        ///     createIfNeeded is false.
        /// </devdoc>
        private ReflectedTypeData GetTypeData(Type type, bool createIfNeeded) {

            ReflectedTypeData td = null;

            if (_typeData != null) {
                td = (ReflectedTypeData)_typeData[type];
                if (td != null) {
                    return td;
                }
            }

            lock (_internalSyncObject) {
                if (_typeData != null) {
                    td = (ReflectedTypeData)_typeData[type];
                }

                if (td == null && createIfNeeded) {
                    td = new ReflectedTypeData(type);
                    if (_typeData == null) {
                        _typeData = new Hashtable();
                    }
                    _typeData[type] = td;
                }
            }

            return td;
        }

        /// <devdoc>
        ///     This method returns a custom type descriptor for the given type / object.  
        ///     The objectType parameter is always valid, but the instance parameter may 
        ///     be null if no instance was passed to TypeDescriptor.  The method should 
        ///     return a custom type descriptor for the object.  If the method is not 
        ///     interested in providing type information for the object it should 
        ///     return null.
        /// </devdoc>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            Debug.Fail("This should never be invoked.  TypeDescriptionNode should wrap for us.");
            return null;
        }
    
        /// <devdoc>
        ///     Retrieves a type from a name.
        /// </devdoc>
        private static Type GetTypeFromName(string typeName) 
        {
            Type t = Type.GetType(typeName);

            if (t == null) 
            {
                int commaIndex = typeName.IndexOf(',');

                if (commaIndex != -1)
                {
                    // At design time, it's possible for us to reuse
                    // an assembly but add new types.  The app domain
                    // will cache the assembly based on identity, however,
                    // so it could be looking in the previous version
                    // of the assembly and not finding the type.  We work
                    // around this by looking for the non-assembly qualified
                    // name, which causes the domain to raise a type 
                    // resolve event.
                    //
                    t = Type.GetType(typeName.Substring(0, commaIndex));
                }
            }

            return t;
        }

        /// <devdoc>
        ///     This method returns true if the data cache in this reflection 
        ///     type descriptor has data in it.
        /// </devdoc>
        internal bool IsPopulated(Type type)
        {
            ReflectedTypeData td = GetTypeData(type, false);
            if (td != null) {
                return td.IsPopulated;
            }
            return false;
        }

        /// <devdoc>
        ///     Static helper API around reflection to get and cache
        ///     custom attributes.  This does not recurse, but it will
        ///     walk interfaces on the type.  Interfaces are added 
        ///     to the end, so merging should be done from length - 1
        ///     to 0.
        /// </devdoc>
        private static Attribute[] ReflectGetAttributes(Type type)
        {
            if (_attributeCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_attributeCache == null)
                    {
                        _attributeCache = new Hashtable();
                    }
                }
            }

            Attribute[] attrs = (Attribute[])_attributeCache[type];
            if (attrs != null)
            {
                return attrs;
            }

            lock (_internalSyncObject)
            {
                attrs = (Attribute[])_attributeCache[type];
                if (attrs == null)
                {
                    TypeDescriptor.Trace("Attributes : Building attributes for {0}", type.Name);

                    // Get the type's attributes.
                    //
                    object[] typeAttrs = type.GetCustomAttributes(typeof(Attribute), false);

                    attrs = new Attribute[typeAttrs.Length];
                    typeAttrs.CopyTo(attrs, 0);

                    _attributeCache[type] = attrs;
                }
            }

            return attrs;
        }

        /// <devdoc>
        ///     Static helper API around reflection to get and cache
        ///     custom attributes.  This does not recurse to the base class.
        /// </devdoc>
        internal static Attribute[] ReflectGetAttributes(MemberInfo member)
        {
            if (_attributeCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_attributeCache == null)
                    {
                        _attributeCache = new Hashtable();
                    }
                }
            }

            Attribute[] attrs = (Attribute[])_attributeCache[member];
            if (attrs != null)
            {
                return attrs;
            }

            lock (_internalSyncObject)
            {
                attrs = (Attribute[])_attributeCache[member];
                if (attrs == null)
                {
                    // Get the member's attributes.
                    //
                    object[] memberAttrs = member.GetCustomAttributes(typeof(Attribute), false);
                    attrs = new Attribute[memberAttrs.Length];
                    memberAttrs.CopyTo(attrs, 0);
                    _attributeCache[member] = attrs;
                }
            }

            return attrs;
        }

        /// <devdoc>
        ///     Static helper API around reflection to get and cache
        ///     events.  This does not recurse to the base class.
        /// </devdoc>
        private static EventDescriptor[] ReflectGetEvents(Type type)
        {
            if (_eventCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_eventCache == null)
                    {
                        _eventCache = new Hashtable();
                    }
                }
            }

            EventDescriptor[] events = (EventDescriptor[])_eventCache[type];
            if (events != null)
            {
                return events;
            }

            lock (_internalSyncObject)
            {
                events = (EventDescriptor[])_eventCache[type];
                if (events == null)
                {
                    BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
                    TypeDescriptor.Trace("Events : Building events for {0}", type.Name);

                    // Get the type's events.  Events may have their add and
                    // remove methods individually overridden in a derived
                    // class, but at some point in the base class chain both
                    // methods must exist.  If we find an event that doesn't
                    // have both add and remove, we skip it here, because it
                    // will be picked up in our base class scan.
                    //
                    EventInfo[] eventInfos = type.GetEvents(bindingFlags);
                    events = new EventDescriptor[eventInfos.Length];
                    int eventCount = 0;

                    for (int idx = 0; idx < eventInfos.Length; idx++)
                    {
                        EventInfo eventInfo = eventInfos[idx];

                        // GetEvents returns events that are on nonpublic types
                        // if those types are from our assembly.  Screen these.
                        // 
                        if ((!(eventInfo.DeclaringType.IsPublic || eventInfo.DeclaringType.IsNestedPublic)) && (eventInfo.DeclaringType.Assembly == typeof(ReflectTypeDescriptionProvider).Assembly)) {
                            Debug.Fail("Hey, assumption holds true.  Rip this assert.");
                            continue;
                        }

                        MethodInfo addMethod = eventInfo.GetAddMethod();
                        MethodInfo removeMethod = eventInfo.GetRemoveMethod();

                        if (addMethod != null && removeMethod != null)
                        {
                            events[eventCount++] = new ReflectEventDescriptor(type, eventInfo);
                        }
                    }

                    if (eventCount != events.Length)
                    {
                        EventDescriptor[] newEvents = new EventDescriptor[eventCount];
                        Array.Copy(events, 0, newEvents, 0, eventCount);
                        events = newEvents;
                    }

                    #if DEBUG
                    foreach(EventDescriptor dbgEvent in events)
                    {
                        Debug.Assert(dbgEvent != null, "Holes in event array for type " + type);
                    }
                    #endif
                    _eventCache[type] = events;
                }
            }

            return events;
        }

        /// <devdoc>
        ///     This performs the actual reflection needed to discover
        ///     extender properties.  If object caching is supported this
        ///     will maintain a cache of property descriptors on the
        ///     extender provider.  Extender properties are actually two
        ///     property descriptors in one.  There is a chunk of per-class
        ///     data in a ReflectPropertyDescriptor that defines the
        ///     parameter types and get and set methods of the extended property,
        ///     and there is an ExtendedPropertyDescriptor that combines this
        ///     with an extender provider object to create what looks like a
        ///     normal property.  ReflectGetExtendedProperties maintains two 
        ///     separate caches for these two sets:  a static one for the
        ///     ReflectPropertyDescriptor values that don't change for each
        ///     provider instance, and a per-provider cache that contains
        ///     the ExtendedPropertyDescriptors.
        /// </devdoc>
        private static PropertyDescriptor[] ReflectGetExtendedProperties(IExtenderProvider provider)
        {
            IDictionary cache = TypeDescriptor.GetCache(provider);
            PropertyDescriptor[] properties;

            if (cache != null)
            {
                properties = cache[_extenderProviderPropertiesKey] as PropertyDescriptor[];
                if (properties != null)
                {
                    return properties;
                }
            }

            // Our per-instance cache missed.  We have never seen this instance of the
            // extender provider before.  See if we can find our class-based
            // property store.
            //
            if (_extendedPropertyCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_extendedPropertyCache == null)
                    {
                        _extendedPropertyCache = new Hashtable();
                    }
                }
            }

            Type providerType = provider.GetType();
            ReflectPropertyDescriptor[] extendedProperties = (ReflectPropertyDescriptor[])_extendedPropertyCache[providerType];
            if (extendedProperties == null)
            {
                lock (_internalSyncObject)
                {
                    extendedProperties = (ReflectPropertyDescriptor[])_extendedPropertyCache[providerType];

                    // Our class-based property store failed as well, so we need to build up the set of
                    // extended properties here.
                    //
                    if (extendedProperties == null)
                    {
                        AttributeCollection attributes = TypeDescriptor.GetAttributes(providerType);
                        ArrayList extendedList = new ArrayList(attributes.Count);

                        foreach(Attribute attr in attributes) 
                        {
                            ProvidePropertyAttribute provideAttr = attr as ProvidePropertyAttribute;

                            if (provideAttr != null) 
                            {
                                Type receiverType = GetTypeFromName(provideAttr.ReceiverTypeName);

                                if (receiverType != null) 
                                {
                                    MethodInfo getMethod = providerType.GetMethod("Get" + provideAttr.PropertyName, new Type[] {receiverType});

                                    if (getMethod != null && !getMethod.IsStatic && getMethod.IsPublic) 
                                    {
                                        MethodInfo setMethod = providerType.GetMethod("Set" + provideAttr.PropertyName, new Type[] {receiverType, getMethod.ReturnType});

                                        if (setMethod != null && (setMethod.IsStatic || !setMethod.IsPublic)) 
                                        {
                                            setMethod = null;
                                        }

                                        extendedList.Add(new ReflectPropertyDescriptor(providerType, provideAttr.PropertyName, getMethod.ReturnType, receiverType, getMethod, setMethod, null));
                                    }
                                }
                            }
                        }

                        extendedProperties = new ReflectPropertyDescriptor[extendedList.Count];
                        extendedList.CopyTo(extendedProperties, 0);
                        _extendedPropertyCache[providerType] = extendedProperties;
                    }
                }
            }

            // Now that we have our extended properties we can build up a list of callable properties.  These can be 
            // returned to the user.
            //
            properties = new PropertyDescriptor[extendedProperties.Length];
            for (int idx = 0; idx < extendedProperties.Length; idx++)
            {
                Attribute[] attrs = null;
                IComponent comp = provider as IComponent;
                if (comp == null || comp.Site == null)
                {
                    attrs = new Attribute[] {DesignOnlyAttribute.Yes};
                }

                ReflectPropertyDescriptor  rpd = extendedProperties[idx];
                ExtendedPropertyDescriptor epd = new ExtendedPropertyDescriptor(rpd, rpd.ExtenderGetReceiverType(), provider, attrs);
                properties[idx] = epd;
            }

            if (cache != null)
            {
                cache[_extenderProviderPropertiesKey] = properties;
            }

            return properties;
        }

        /// <devdoc>
        ///     Static helper API around reflection to get and cache
        ///     properties. This does not recurse to the base class.
        /// </devdoc>
        private static PropertyDescriptor[] ReflectGetProperties(Type type)
        {
            if (_propertyCache == null)
            {
                lock(_internalSyncObject)
                {
                    if (_propertyCache == null)
                    {
                        _propertyCache = new Hashtable();
                    }
                }
            }

            PropertyDescriptor[] properties = (PropertyDescriptor[])_propertyCache[type];
            if (properties != null)
            {
                return properties;
            }

            lock (_internalSyncObject)
            {
                properties = (PropertyDescriptor[])_propertyCache[type];

                if (properties == null)
                {
                    BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
                    TypeDescriptor.Trace("Properties : Building properties for {0}", type.Name);

                    // Get the type's properties.  Properties may have their
                    // get and set methods individually overridden in a derived
                    // class, so if we find a missing method we need to walk
                    // down the base class chain to find it.  We actually merge
                    // "new" properties of the same name, so we must preserve
                    // the member info for each method individually.
                    //
                    PropertyInfo[] propertyInfos = type.GetProperties(bindingFlags);
                    properties = new PropertyDescriptor[propertyInfos.Length];
                    int propertyCount = 0;

               
                    for (int idx = 0; idx < propertyInfos.Length; idx++)
                    {
                        PropertyInfo propertyInfo = propertyInfos[idx];

                        // Today we do not support parameterized properties.
                        // 
                        if (propertyInfo.GetIndexParameters().Length > 0) {
                            continue;
                        } 

                        MethodInfo getMethod = propertyInfo.GetGetMethod();
                        MethodInfo setMethod = propertyInfo.GetSetMethod();
                        string name = propertyInfo.Name;

                        // If the property only overrode "set", then we don't
                        // pick it up here.  Rather, we just merge it in from
                        // the base class list.
      

                        // If a property had at least a get method, we consider it.  We don't
                        // consider write-only properties.
                        //
                        if (getMethod != null)
                        {
                            properties[propertyCount++] = new ReflectPropertyDescriptor(type, name, 
                                                                                    propertyInfo.PropertyType, 
                                                                                    propertyInfo, getMethod, 
                                                                                    setMethod, null);
                        }
                    }

               
                    if (propertyCount != properties.Length)
                    {
                        PropertyDescriptor[] newProperties = new PropertyDescriptor[propertyCount];
                        Array.Copy(properties, 0, newProperties, 0, propertyCount);
                        properties = newProperties;
                    }

                    #if DEBUG
                    foreach(PropertyDescriptor dbgProp in properties)
                    {
                        Debug.Assert(dbgProp != null, "Holes in property array for type " + type);
                    }
                    #endif
                    _propertyCache[type] = properties;
                }
            }

            return properties;
        }

        /// <devdoc>
        ///     Refreshes the contents of this type descriptor.  This does not
        ///     actually requery, but it will clear our state so the next
        ///     query re-populates.
        /// </devdoc>
        internal void Refresh(Type type)
        {
            ReflectedTypeData td = GetTypeData(type, false);
            if (td != null) {
                td.Refresh();
            }
        }

        /// <devdoc> 
        ///      Searches the provided intrinsic hashtable for a match with the object type. 
        ///      At the beginning, the hashtable contains types for the various converters. 
        ///      As this table is searched, the types for these objects 
        ///      are replaced with instances, so we only create as needed.  This method 
        ///      does the search up the base class hierarchy and will create instances 
        ///      for types as needed.  These instances are stored back into the table 
        ///      for the base type, and for the original component type, for fast access. 
        /// </devdoc> 
        private static object SearchIntrinsicTable(Hashtable table, Type callingType) 
        {
            object hashEntry = null;

            // We take a lock on this table.  Nothing in this code calls out to
            // other methods that lock, so it should be fairly safe to grab this
            // lock.  Also, this allows multiple intrinsic tables to be searched
            // at once.
            //
            lock(table) 
            {
                Type baseType = callingType;
                while (baseType != null && baseType != typeof(object)) 
                {
                    hashEntry = table[baseType];

                    // If the entry is a late-bound type, then try to
                    // resolve it.
                    //
                    string typeString = hashEntry as string;
                    if (typeString != null) 
                    {
                        hashEntry = Type.GetType(typeString);
                        if (hashEntry != null) 
                        {
                            table[baseType] = hashEntry;
                        }
                    }

                    if (hashEntry != null) 
                    {
                        break;
                    }

                    baseType = baseType.BaseType;
                }

                // Now make a scan through each value in the table, looking for interfaces.
                // If we find one, see if the object implements the interface.
                //
                if (hashEntry == null) 
                {

                    foreach(DictionaryEntry de in table)
                    {
                        Type keyType = de.Key as Type;

                        if (keyType != null && keyType.IsInterface && keyType.IsAssignableFrom(callingType)) 
                        {

                            hashEntry = de.Value;
                            string typeString = hashEntry as string;

                            if (typeString != null) 
                            {
                                hashEntry = Type.GetType(typeString);
                                if (hashEntry != null) 
                                {
                                    table[callingType] = hashEntry;
                                }
                            }

                            if (hashEntry != null) 
                            {
                                break;
                            }
                        }
                    }
                }

                // Special case converters
                //
                if (hashEntry == null)
                {
                    if (callingType.IsGenericType && callingType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        // Check if it is a nullable value
                        hashEntry = table[_intrinsicNullableKey];
                    }
                    else if (callingType.IsInterface)
                    {
                        // Finally, check to see if the component type is some unknown interface.
                        // We have a custom converter for that.
                        hashEntry = table[_intrinsicReferenceKey];
                    }
                }

                // Interfaces do not derive from object, so we
                // must handle the case of no hash entry here.
                //
                if (hashEntry == null) 
                {
                    hashEntry = table[typeof(object)];
                }

                // If the entry is a type, create an instance of it and then
                // replace the entry.  This way we only need to create once.
                // We can only do this if the object doesn't want a type
                // in its constructor.
                //
                Type type = hashEntry as Type;

                if (type != null) 
                {
                    hashEntry = CreateInstance(type, callingType);
                    if (type.GetConstructor(_typeConstructor) == null) 
                    {
                        table[callingType] = hashEntry;
                    }
                }
            }

            return hashEntry;
        }

        /// <devdoc>
        ///     This class contains all the reflection information for a
        ///     given type.
        /// </devdoc>
        private class ReflectedTypeData {

            private Type                            _type;
            private AttributeCollection             _attributes;
            private EventDescriptorCollection       _events;
            private PropertyDescriptorCollection    _properties;
            private TypeConverter                   _converter;
            private object[]                        _editors;
            private Type[]                          _editorTypes;
            private int                             _editorCount;

            internal ReflectedTypeData(Type type) {
                _type = type;
                TypeDescriptor.Trace("Reflect : Creating ReflectedTypeData for {0}", type.Name);
            }

            /// <devdoc>
            ///     This method returns true if the data cache in this reflection 
            ///     type descriptor has data in it.
            /// </devdoc>
            internal bool IsPopulated
            {
                get
                {
                    return (_attributes != null) | (_events != null) | (_properties != null);
                }
            }

            /// <devdoc>
            ///     Retrieves custom attributes.
            /// </devdoc>
            internal AttributeCollection GetAttributes()
            {
                // Worst case collision scenario:  we don't want the perf hit
                // of taking a lock, so if we collide we will query for
                // attributes twice.  Not a big deal.
                //
                if (_attributes == null)
                {
                    TypeDescriptor.Trace("Attributes : Building collection for {0}", _type.Name);

                    // Obtaining attributes follows a very critical order: we must take care that
                    // we merge attributes the right way.  Consider this:
                    //
                    // [A4]
                    // interface IBase;
                    //
                    // [A3]
                    // interface IDerived;
                    //
                    // [A2]
                    // class Base : IBase;
                    //
                    // [A1]
                    // class Derived : Base, IDerived
                    //
                    // Calling GetAttributes on type Derived must merge attributes in the following
                    // order:  A1 - A4.  Interfaces always lose to types, and interfaces and types
                    // must be merged in the same order.  At the same time, we must be careful
                    // that we don't always go through reflection here, because someone could have
                    // created a custom provider for a type.  Because there is only one instance
                    // of ReflectTypeDescriptionProvider created for typeof(object), if our code
                    // is invoked here we can be sure that there is no custom provider for
                    // _type all the way up the base class chain.
                    // We cannot be sure that there is no custom provider for
                    // interfaces that _type implements, however, because they are not derived
                    // from _type.  So, for interfaces, we must go through TypeDescriptor
                    // again to get the interfaces attributes.  

                    // Get the type's attributes. This does not recurse up the base class chain.
                    // We append base class attributes to this array so when walking we will
                    // walk from Length - 1 to zero.
                    //
                    Attribute[] attrArray = ReflectTypeDescriptionProvider.ReflectGetAttributes(_type);
                    Type baseType = _type.BaseType;

                    while (baseType != null && baseType != typeof(object))
                    {
                        Attribute[] baseArray = ReflectTypeDescriptionProvider.ReflectGetAttributes(baseType);
                        Attribute[] temp = new Attribute[attrArray.Length + baseArray.Length];
                        Array.Copy(attrArray, 0, temp, 0, attrArray.Length);
                        Array.Copy(baseArray, 0, temp, attrArray.Length, baseArray.Length);
                        attrArray = temp;
                        baseType = baseType.BaseType;
                    }

                    // Next, walk the type's interfaces.  We append these to
                    // the attribute array as well.
                    //
                    int ifaceStartIdx = attrArray.Length;
                    Type[] interfaces = _type.GetInterfaces(); 
                    TypeDescriptor.Trace("Attributes : Walking {0} interfaces", interfaces.Length);
                    for(int idx = 0; idx < interfaces.Length; idx++)
                    {
                        Type iface = interfaces[idx];

                        // only do this for public interfaces.
                        //
                        if ((iface.Attributes & (TypeAttributes.Public | TypeAttributes.NestedPublic)) != 0) {
                            // No need to pass an instance into GetTypeDescriptor here because, if someone provided a custom
                            // provider based on object, it already would have hit.
                            AttributeCollection ifaceAttrs = TypeDescriptor.GetAttributes(iface);
                            if (ifaceAttrs.Count > 0) {
                                Attribute[] temp = new Attribute[attrArray.Length + ifaceAttrs.Count];
                                Array.Copy(attrArray, 0, temp, 0, attrArray.Length);
                                ifaceAttrs.CopyTo(temp, attrArray.Length);
                                attrArray = temp;
                            }
                        }
                    }

                    // Finally, put all these attributes in a dictionary and filter out the duplicates.
                    //
                    OrderedDictionary attrDictionary = new OrderedDictionary(attrArray.Length);

                    for (int idx = 0; idx < attrArray.Length; idx++)
                    {
                        bool addAttr = true;
                        if (idx >= ifaceStartIdx) {
                            for (int ifaceSkipIdx = 0; ifaceSkipIdx < _skipInterfaceAttributeList.Length; ifaceSkipIdx++)
                            {
                                if (_skipInterfaceAttributeList[ifaceSkipIdx].IsInstanceOfType(attrArray[idx]))
                                {
                                    addAttr = false;
                                    break;
                                }
                            }

                        }

                        if (addAttr && !attrDictionary.Contains(attrArray[idx].TypeId)) {
                            attrDictionary[attrArray[idx].TypeId] = attrArray[idx];
                        }
                    }

                    attrArray = new Attribute[attrDictionary.Count];
                    attrDictionary.Values.CopyTo(attrArray, 0);
                    _attributes = new AttributeCollection(attrArray);
                }

                return _attributes;
            }

            /// <devdoc>
            ///     Retrieves the class name for our type.
            /// </devdoc>
            internal string GetClassName(object instance)
            {
                return _type.FullName;
            }

            /// <devdoc>
            ///     Retrieves the component name from the site.
            /// </devdoc>
            internal string GetComponentName(object instance)
            {
                IComponent comp = instance as IComponent;
                if (comp != null) 
                {
                    ISite site = comp.Site;
                    if (site != null) 
                    {
                        INestedSite nestedSite = site as INestedSite;
                        if (nestedSite != null) 
                        {
                            return nestedSite.FullName;
                        }
                        else 
                        {
                            return site.Name;
                        }
                    }
                }

                return null;
            }

            /// <devdoc>
            ///     Retrieves the type converter.  If instance is non-null,
            ///     it will be used to retrieve attributes.  Otherwise, _type
            ///     will be used.
            /// </devdoc>
            internal TypeConverter GetConverter(object instance)
            {
                TypeConverterAttribute typeAttr = null;

                // For instances, the design time object for them may want to redefine the
                // attributes.  So, we search the attribute here based on the instance.  If found,
                // we then search on the same attribute based on type.  If the two don't match, then
                // we cannot cache the value and must re-create every time.  It is rare for a designer
                // to override these attributes, so we want to be smart here.
                //
                if (instance != null)
                {
                    typeAttr = (TypeConverterAttribute)TypeDescriptor.GetAttributes(_type)[typeof(TypeConverterAttribute)];
                    TypeConverterAttribute instanceAttr = (TypeConverterAttribute)TypeDescriptor.GetAttributes(instance)[typeof(TypeConverterAttribute)];
                    if (typeAttr != instanceAttr)
                    {
                        Type converterType = GetTypeFromName(instanceAttr.ConverterTypeName);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType)) 
                        {
                            try {
                                IntSecurity.FullReflection.Assert();
                                return (TypeConverter)ReflectTypeDescriptionProvider.CreateInstance(converterType, _type);
                            } finally {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                }

                // If we got here, we return our type-based converter.
                //
                if (_converter == null)
                {
                    TypeDescriptor.Trace("Converters : Building converter for {0}", _type.Name);

                    if (typeAttr == null)
                    {
                        typeAttr = (TypeConverterAttribute)TypeDescriptor.GetAttributes(_type)[typeof(TypeConverterAttribute)];
                    }

                    if (typeAttr != null)
                    {
                        Type converterType = GetTypeFromName(typeAttr.ConverterTypeName);
                        if (converterType != null && typeof(TypeConverter).IsAssignableFrom(converterType)) 
                        {
                            try {
                                IntSecurity.FullReflection.Assert();
                                _converter = (TypeConverter)ReflectTypeDescriptionProvider.CreateInstance(converterType, _type);
                            } finally {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }

                    if (_converter == null)
                    {
                        // We did not get a converter.  Traverse up the base class chain until
                        // we find one in the stock hashtable.
                        //
                        _converter = (TypeConverter)ReflectTypeDescriptionProvider.SearchIntrinsicTable(IntrinsicTypeConverters, _type);
                        Debug.Assert(_converter != null, "There is no intrinsic setup in the hashtable for the Object type");
                    }
                }

                return _converter;
            }

            /// <devdoc>
            ///     Return the default event. The default event is determined by the
            ///     presence of a DefaultEventAttribute on the class.
            /// </devdoc>
            internal EventDescriptor GetDefaultEvent(object instance)
            {
                AttributeCollection attributes;

                if (instance != null)
                {
                    attributes = TypeDescriptor.GetAttributes(instance);
                }
                else
                {
                    attributes = TypeDescriptor.GetAttributes(_type);
                }

                DefaultEventAttribute attr = (DefaultEventAttribute)attributes[typeof(DefaultEventAttribute)];
                if (attr != null && attr.Name != null)
                {
                    if (instance != null)
                    {
                        return TypeDescriptor.GetEvents(instance)[attr.Name];
                    }
                    else
                    {
                        return TypeDescriptor.GetEvents(_type)[attr.Name];
                    }
                }

                return null;
            }

            /// <devdoc>
            ///     Return the default property.
            /// </devdoc>
            internal PropertyDescriptor GetDefaultProperty(object instance)
            {
                AttributeCollection attributes;

                if (instance != null)
                {
                    attributes = TypeDescriptor.GetAttributes(instance);
                }
                else
                {
                    attributes = TypeDescriptor.GetAttributes(_type);
                }

                DefaultPropertyAttribute attr = (DefaultPropertyAttribute)attributes[typeof(DefaultPropertyAttribute)];
                if (attr != null && attr.Name != null)
                {
                    if (instance != null)
                    {
                        return TypeDescriptor.GetProperties(instance)[attr.Name];
                    }
                    else
                    {
                        return TypeDescriptor.GetProperties(_type)[attr.Name];
                    }
                }

                return null;
            }

            /// <devdoc>
            ///     Retrieves the editor for the given base type.
            /// </devdoc>
            internal object GetEditor(object instance, Type editorBaseType)
            {
                EditorAttribute typeAttr;

                // For instances, the design time object for them may want to redefine the
                // attributes.  So, we search the attribute here based on the instance.  If found,
                // we then search on the same attribute based on type.  If the two don't match, then
                // we cannot cache the value and must re-create every time.  It is rare for a designer
                // to override these attributes, so we want to be smart here.
                //
                if (instance != null)
                {
                    typeAttr = GetEditorAttribute(TypeDescriptor.GetAttributes(_type), editorBaseType);
                    EditorAttribute instanceAttr = GetEditorAttribute(TypeDescriptor.GetAttributes(instance), editorBaseType);
                    if (typeAttr != instanceAttr)
                    {
                        Type editorType = GetTypeFromName(instanceAttr.EditorTypeName);
                        if (editorType != null && editorBaseType.IsAssignableFrom(editorType)) 
                        {
                            return ReflectTypeDescriptionProvider.CreateInstance(editorType, _type);
                        }
                    }
                }

                // If we got here, we return our type-based editor.
                //
                lock(this)
                {
                    for (int idx = 0; idx < _editorCount; idx++)
                    {
                        if (_editorTypes[idx] == editorBaseType)
                        {
                            return _editors[idx];
                        }
                    }
                }

                // Editor is not cached yet.  Look in the attributes.
                //
                object editor = null;

                typeAttr = GetEditorAttribute(TypeDescriptor.GetAttributes(_type), editorBaseType);
                if (typeAttr != null)
                {
                    Type editorType = GetTypeFromName(typeAttr.EditorTypeName);
                    if (editorType != null && editorBaseType.IsAssignableFrom(editorType)) 
                    {
                        editor = ReflectTypeDescriptionProvider.CreateInstance(editorType, _type);
                    }
                }

                // Editor is not in the attributes.  Search intrinsic tables.
                //
                if (editor == null)
                {
                    Hashtable intrinsicEditors = ReflectTypeDescriptionProvider.GetEditorTable(editorBaseType);
                    if (intrinsicEditors != null) 
                    {
                        editor = ReflectTypeDescriptionProvider.SearchIntrinsicTable(intrinsicEditors, _type);
                    }

                    // As a quick sanity check, check to see that the editor we got back is of 
                    // the correct type.
                    //
                    if (editor != null && !editorBaseType.IsInstanceOfType(editor)) {
                        Debug.Fail("Editor " + editor.GetType().FullName + " is not an instance of " + editorBaseType.FullName + " but it is in that base types table.");
                        editor = null;
                    }
                }

                if (editor != null)
                {
                    lock(this)
                    {
                        if (_editorTypes == null || _editorTypes.Length == _editorCount)
                        {
                            int newLength = (_editorTypes == null ? 4 : _editorTypes.Length * 2);

                            Type[] newTypes = new Type[newLength];
                            object[] newEditors = new object[newLength];

                            if (_editorTypes != null)
                            {
                                _editorTypes.CopyTo(newTypes, 0);
                                _editors.CopyTo(newEditors, 0);
                            }

                            _editorTypes = newTypes;
                            _editors = newEditors;

                            _editorTypes[_editorCount] = editorBaseType;
                            _editors[_editorCount++] = editor;
                        }
                    }
                }

                return editor;
            }

            /// <devdoc>
            ///     Helper method to return an editor attribute of the correct base type.
            /// </devdoc>
            private static EditorAttribute GetEditorAttribute(AttributeCollection attributes, Type editorBaseType)
            {
                foreach(Attribute attr in attributes)
                {
                    EditorAttribute edAttr = attr as EditorAttribute;
                    if (edAttr != null)
                    {
                        Type attrEditorBaseType = Type.GetType(edAttr.EditorBaseTypeName);

                        if (attrEditorBaseType != null && attrEditorBaseType == editorBaseType) 
                        {
                            return edAttr;
                        }
                    }
                }

                return null;
            }
        
            /// <devdoc>
            ///     Retrieves the events for this type.
            /// </devdoc>
            internal EventDescriptorCollection GetEvents()
            {
                // Worst case collision scenario:  we don't want the perf hit
                // of taking a lock, so if we collide we will query for
                // events twice.  Not a big deal.
                //
                if (_events == null)
                {
                    TypeDescriptor.Trace("Events : Building collection for {0}", _type.Name);

                    EventDescriptor[] eventArray;
                    Dictionary<string, EventDescriptor> eventList = new Dictionary<string, EventDescriptor>(16);
                    Type baseType = _type;
                    Type objType = typeof(object);

                    do {
                        eventArray = ReflectGetEvents(baseType);
                        foreach(EventDescriptor ed in eventArray) {
                            if (!eventList.ContainsKey(ed.Name)) {
                                eventList.Add(ed.Name, ed);
                            }    
                        }
                        baseType = baseType.BaseType;
                    }
                    while(baseType != null && baseType != objType);

                    eventArray = new EventDescriptor[eventList.Count];
                    eventList.Values.CopyTo(eventArray, 0);
                    _events = new EventDescriptorCollection(eventArray, true);
                }

                return _events;
            }
        
            /// <devdoc>
            ///     Retrieves the properties for this type.
            /// </devdoc>
            internal PropertyDescriptorCollection GetProperties()
            {
                // Worst case collision scenario:  we don't want the perf hit
                // of taking a lock, so if we collide we will query for
                // properties twice.  Not a big deal.
                //
                if (_properties == null)
                {
                    TypeDescriptor.Trace("Properties : Building collection for {0}", _type.Name);

                    PropertyDescriptor[] propertyArray;
                    Dictionary<string, PropertyDescriptor> propertyList = new Dictionary<string, PropertyDescriptor>(10);
                    Type baseType = _type;
                    Type objType = typeof(object);

                    do {
                        propertyArray = ReflectGetProperties(baseType);
                        foreach(PropertyDescriptor p in propertyArray) {
                            if (!propertyList.ContainsKey(p.Name)) {
                                propertyList.Add(p.Name, p);
                            }    
                        }
                        baseType = baseType.BaseType;
                    }
                    while(baseType != null && baseType != objType);

                    propertyArray = new PropertyDescriptor[propertyList.Count];
                    propertyList.Values.CopyTo(propertyArray, 0);
                    _properties = new PropertyDescriptorCollection(propertyArray, true);
                }

                return _properties;
            }
        
            /// <devdoc>
            ///     Retrieves a type from a name.  The Assembly of the type
            ///     that this PropertyDescriptor came from is first checked,
            ///     then a global Type.GetType is performed.
            /// </devdoc>
            private Type GetTypeFromName(string typeName) 
            {

                if (typeName == null || typeName.Length == 0) 
                {
                     return null;
                }

                int commaIndex = typeName.IndexOf(',');
                Type t = null;

                if (commaIndex == -1) 
                {
                    t = _type.Assembly.GetType(typeName);
                }

                if (t == null) 
                {
                    t = Type.GetType(typeName);
                }

                if (t == null && commaIndex != -1)
                {
                    // At design time, it's possible for us to reuse
                    // an assembly but add new types.  The app domain
                    // will cache the assembly based on identity, however,
                    // so it could be looking in the previous version
                    // of the assembly and not finding the type.  We work
                    // around this by looking for the non-assembly qualified
                    // name, which causes the domain to raise a type 
                    // resolve event.
                    //
                    t = Type.GetType(typeName.Substring(0, commaIndex));
                }

                return t;
            }

            /// <devdoc>
            ///     Refreshes the contents of this type descriptor.  This does not
            ///     actually requery, but it will clear our state so the next
            ///     query re-populates.
            /// </devdoc>
            internal void Refresh()
            {
                _attributes = null;
                _events = null;
                _properties = null;
                _converter = null;
                _editors = null;
                _editorTypes = null;
                _editorCount = 0;
            }
        }
    }
}

