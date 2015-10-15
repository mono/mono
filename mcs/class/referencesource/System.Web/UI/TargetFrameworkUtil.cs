using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;
using System.Reflection;

namespace System.Web.UI {

    // Helper class to retrieve filtered members from the target framework type using TargetFrameworkProvider.
    // We need to be careful not to expose faux/LMR types or memberInfo as they don't work properly when mixed
    // with their runtime counter parts.
    internal static class TargetFrameworkUtil {

        private class MemberCache {
            private ConcurrentDictionary<string, EventInfo> _events;
            private ConcurrentDictionary<Tuple<string, int>, FieldInfo> _fields;
            private ConcurrentDictionary<Tuple<string, int>, PropertyInfo> _properties;

            internal MemberCache() {
            }

            internal ConcurrentDictionary<string, EventInfo> Events {
                get {
                    if (_events == null) {
                        _events = new ConcurrentDictionary<string, EventInfo>();
                    }
                    return _events;
                }
            }
            internal ConcurrentDictionary<Tuple<string, int>, FieldInfo> Fields {
                get {
                    if (_fields == null) {
                        _fields = new ConcurrentDictionary<Tuple<string, int>, FieldInfo>();
                    }
                    return _fields;
                }
            }
            internal ConcurrentDictionary<Tuple<string, int>, PropertyInfo> Properties {
                get {
                    if (_properties == null) {
                        _properties = new ConcurrentDictionary<Tuple<string, int>, PropertyInfo>();
                    }
                    return _properties;
                }
            }
        }

        private static ConcurrentDictionary<Type, MemberCache> s_memberCache = new ConcurrentDictionary<Type, MemberCache>();

        private static ClientBuildManagerTypeDescriptionProviderBridge s_cbmTdpBridge;

        private static ConcurrentDictionary<Type, PropertyDescriptorCollection> s_typePropertyDescriptorCollectionDict =
            new ConcurrentDictionary<Type, PropertyDescriptorCollection>();
        private static ConcurrentDictionary<object, PropertyDescriptorCollection> s_objectPropertyDescriptorCollectionDict =
            new ConcurrentDictionary<object, PropertyDescriptorCollection>();
        private static ConcurrentDictionary<Type, EventDescriptorCollection> s_eventDescriptorCollectionDict =
            new ConcurrentDictionary<Type, EventDescriptorCollection>();
        
        private static ConcurrentDictionary<Type, bool> s_isFrameworkType = new ConcurrentDictionary<Type, bool>();
        
        private static MemberCache GetMemberCache(Type type){
            MemberCache memberCache = null;
            if (!s_memberCache.TryGetValue(type, out memberCache)) {
                memberCache = new MemberCache();
                s_memberCache.TryAdd(type, memberCache);
            }
            return memberCache;
        }

        private static Tuple<string, int> MakeTuple(string name, BindingFlags bindingAttr) {
            return new Tuple<string, int>(name, (int)bindingAttr);
        }

        private static TypeDescriptionProviderService TypeDescriptionProviderService {
            get {
                if (DesignerHost == null) {
                    return null;
                }
                TypeDescriptionProviderService tdpService = DesignerHost.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
                return tdpService;
            }
        }

        /// <summary>
        /// The DesignerHost is only available within the context of DesignTimeTemplateParser.ParseControl, which is called
        /// from the design view.
        /// </summary>
        internal static IDesignerHost DesignerHost { get; set; }

        /// <summary>
        /// The CBMTypeDescriptionProviderBridge is only available when building using the ClientBuildManager in VS.
        /// </summary>
        internal static ClientBuildManagerTypeDescriptionProviderBridge CBMTypeDescriptionProviderBridge {
            set {
                s_cbmTdpBridge = value;
            }
        }

        // The provider needs not be cached because the TFP service 
        // returns light-weight providers that delegate to the same 
        // underlying TFP instance.  (Dev10 
        private static TypeDescriptionProvider GetTargetFrameworkProvider(object obj) {
            TypeDescriptionProviderService service = TargetFrameworkUtil.TypeDescriptionProviderService;
            if (service != null) {
                return service.GetProvider(obj);
            }
            return null;
        }

        private static TypeDescriptionProvider GetTargetFrameworkProvider(Type type) {
            TypeDescriptionProviderService service = TargetFrameworkUtil.TypeDescriptionProviderService;
            if (service != null) {
                return service.GetProvider(type);
            }
            return null;
        }

        private static ICustomTypeDescriptor GetTypeDescriptor(Type type) {
            TypeDescriptionProvider tdp = GetTargetFrameworkProvider(type);
            if (tdp != null) {
                ICustomTypeDescriptor descriptor = tdp.GetTypeDescriptor(type);
                if (descriptor != null) {
                    return descriptor;
                }
            }
            return null;
        }

        private static ICustomTypeDescriptor GetTypeDescriptor(object obj) {
            TypeDescriptionProvider tdp = GetTargetFrameworkProvider(obj);
            if (tdp != null) {
                ICustomTypeDescriptor descriptor = tdp.GetTypeDescriptor(obj);
                if (descriptor != null) {
                    return descriptor;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the target type if it is available, otherwise returns back the original type
        /// </summary>
        private static Type GetReflectionType(Type type) {
            if (type == null) {
                return null;
            }
            TypeDescriptionProvider provider = GetTargetFrameworkProvider(type);
            if (provider != null) {
                return provider.GetReflectionType(type);
            }
            return type;
        }

        private static Type[] GetReflectionTypes(Type[] types) {
            if (types == null) {
                return null;
            }
            var reflectionTypes = from t in types select GetReflectionType(t);
            return reflectionTypes.ToArray();
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingAttr,
            Type returnType = null,
            Type[] types = null,
            bool throwAmbiguousMatchException = false) {

            if (types == null) {
                types = Type.EmptyTypes;
            }

            if (SkipCache || returnType != null || types != Type.EmptyTypes) {
                // Don't cache if any of the values are non-default
                return GetPropertyHelper(type, name, bindingAttr, returnType, types, throwAmbiguousMatchException);
            }

            PropertyInfo result = null;
            MemberCache memberCache = GetMemberCache(type);
            Tuple<string, int> key = MakeTuple(name, bindingAttr);
            if (!memberCache.Properties.TryGetValue(key, out result)) {
                result = GetPropertyHelper(type, name, bindingAttr, returnType, types, throwAmbiguousMatchException);
                memberCache.Properties.TryAdd(key, result);
            }

            return result;
        }

        private static PropertyInfo GetPropertyHelper(Type type, string name, BindingFlags bindingAttr,
            Type returnType, Type[] types, bool throwAmbiguousMatchException) {
            try {

                bool hasProperty = false;
                if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                    Type typeToUse = GetTypeToUseForCBMBridge(type);
                    hasProperty = s_cbmTdpBridge.HasProperty(typeToUse, name, bindingAttr, returnType, types);
                }
                else {
                    Type reflectionType = GetReflectionType(type);
                    Type reflectionReturnType = GetReflectionType(returnType);
                    Type[] reflectionTypes = GetReflectionTypes(types);
                    PropertyInfo propInfo = reflectionType.GetProperty(name, bindingAttr, null /* binder */, 
                        reflectionReturnType, reflectionTypes, null /* modifiers */);

                    hasProperty = propInfo != null;
                }
                
                // Return the actual runtime PropertyInfo only if it was found in the target type.
                if (hasProperty) {
                    return type.GetProperty(name, bindingAttr, null /* binder */, returnType, types, null /* modifiers */);
                }
            } catch (AmbiguousMatchException) {
                if (throwAmbiguousMatchException) {
                    throw;
                } 
                return GetMostSpecificProperty(type, name, bindingAttr, returnType, types);
            }
            return null;
        }

        internal static FieldInfo GetField(Type type, string name, BindingFlags bindingAttr) {
            if (SkipCache) {
                return GetFieldInfo(type, name, bindingAttr);
            }

            FieldInfo result = null;
            MemberCache memberCache = GetMemberCache(type);

            Tuple<string, int> key = MakeTuple(name, bindingAttr);
            if (!memberCache.Fields.TryGetValue(key, out result)) {
                result = GetFieldInfo(type, name, bindingAttr);
                memberCache.Fields.TryAdd(key, result);
            }

            return result;
        }

        private static FieldInfo GetFieldInfo(Type type, string name, BindingFlags bindingAttr) {
            if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                Type typeToUse = GetTypeToUseForCBMBridge(type);
                bool hasField = s_cbmTdpBridge.HasField(typeToUse, name, bindingAttr);
                if (hasField) {
                    return type.GetField(name, bindingAttr);
                }
                return null;
            }

            Type targetFrameworkType = GetReflectionType(type);
            FieldInfo fieldInfo = targetFrameworkType.GetField(name, bindingAttr);

            // Return the actual runtime FieldInfo only if it was found in the target type.
            if (fieldInfo != null) {
                return type.GetField(name, bindingAttr);
            }
            return null;
        }

        internal static EventInfo GetEvent(Type type, string name) {
            if (SkipCache) {
                return GetEventInfo(type, name);
            }

            EventInfo result = null;
            MemberCache memberCache = GetMemberCache(type);

            if (!memberCache.Events.TryGetValue(name, out result)) {
                result = GetEventInfo(type, name);
                memberCache.Events.TryAdd(name, result);
            }

            return result;
        }

        private static EventInfo GetEventInfo(Type type, string name) {
            if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                Type typeToUse = GetTypeToUseForCBMBridge(type);
                bool hasEvent = s_cbmTdpBridge.HasEvent(typeToUse, name);
                if (hasEvent) {
                    return type.GetEvent(name);
                }
                return null;
            }

            Type targetFrameworkType = GetReflectionType(type);
            EventInfo eventInfo = targetFrameworkType.GetEvent(name);

            // Return the actual runtime EventInfo only if it was found in the target type.
            if (eventInfo != null) {
                return type.GetEvent(name);
            }
            return null;
        }

        internal static PropertyDescriptorCollection GetProperties(Type type) {
            if (SkipCache) {
                return GetPropertyDescriptorCollection(type);
            }

            PropertyDescriptorCollection result = null;
            
            if (!s_typePropertyDescriptorCollectionDict.TryGetValue(type, out result)) {
                result = GetPropertyDescriptorCollection(type);
                s_typePropertyDescriptorCollectionDict.TryAdd(type, result);
            }

            return result;
        }

        private static PropertyDescriptorCollection GetPropertyDescriptorCollection(Type type) {
            if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                return GetFilteredPropertyDescriptorCollection(type, null);
            }

            ICustomTypeDescriptor descriptor = GetTypeDescriptor(type);
            if (descriptor != null) {
                return descriptor.GetProperties();
            }
            else {
                return TypeDescriptor.GetProperties(type);
            }
        }

        internal static PropertyDescriptorCollection GetProperties(object obj) {
            if (SkipCache) {
                return GetPropertyDescriptorCollection(obj);
            }

            PropertyDescriptorCollection result = null;

            if (!s_objectPropertyDescriptorCollectionDict.TryGetValue(obj, out result)) {
                result = GetPropertyDescriptorCollection(obj);
                s_objectPropertyDescriptorCollectionDict.TryAdd(obj, result);
            }

            return result;
        }
        
        private static PropertyDescriptorCollection GetPropertyDescriptorCollection(object obj) {
            Type type = obj.GetType();
            if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                return GetFilteredPropertyDescriptorCollection(type, obj);
            }

            ICustomTypeDescriptor descriptor = GetTypeDescriptor(obj);
            if (descriptor != null) {
                return descriptor.GetProperties();
            }
            else {
                return TypeDescriptor.GetProperties(obj);
            }
        }

        /// <summary>
        /// This method does filtering based on propertyInfo, and should only be used when the TargetFrameworkProvider
        /// is not directly available, for example in the CBM case where it is in another appdomain.
        /// </summary>
        private static PropertyDescriptorCollection GetFilteredPropertyDescriptorCollection(Type objectType, object instance) {
            Debug.Assert(s_cbmTdpBridge != null, "s_cbmTdpBridge should not be null");
            PropertyDescriptorCollection propertyDescriptors = null;
            if (instance != null) {
                propertyDescriptors = TypeDescriptor.GetProperties(instance);
            }
            else if (objectType != null) {
                propertyDescriptors = TypeDescriptor.GetProperties(objectType);
            }
            else {
                throw new ArgumentException("At least one argument should be non-null");
            }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance; 
            Type typeToUse = GetTypeToUseForCBMBridge(objectType);
            string[] propertyNames = s_cbmTdpBridge.GetFilteredProperties(typeToUse, bindingFlags);
            
            var filteredPropertyDescriptors = from p in propertyNames
                let d = propertyDescriptors[p]
                where d != null select d;

            return new PropertyDescriptorCollection(filteredPropertyDescriptors.ToArray());
        }

        internal static EventDescriptorCollection GetEvents(Type type) {
            if (SkipCache) {
                return GetEventDescriptorCollection(type);
            }

            EventDescriptorCollection result = null;
            if (!s_eventDescriptorCollectionDict.TryGetValue(type, out result)) {
                result = GetEventDescriptorCollection(type);
                s_eventDescriptorCollectionDict.TryAdd(type, result);
            }
            return result;

        }

        private static EventDescriptorCollection GetEventDescriptorCollection(Type type) {
            if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                return GetFilteredEventDescriptorCollection(type, null);
            }

            ICustomTypeDescriptor descriptor = GetTypeDescriptor(type);
            if (descriptor != null) {
                return descriptor.GetEvents();
            }
            else {
                return TypeDescriptor.GetEvents(type);
            }
        }

        /// <summary>
        /// This method does filtering based on eventInfo, and should only be used when the TargetFrameworkProvider
        /// is not directly available, for example in the CBM case where it is in another appdomain.
        /// </summary>
        private static EventDescriptorCollection GetFilteredEventDescriptorCollection(Type objectType, object instance) {
            Debug.Assert(s_cbmTdpBridge != null, "s_cbmTdpBridge should not be null");
            EventDescriptorCollection eventDescriptors = null;
            if (instance != null) {
                eventDescriptors = TypeDescriptor.GetEvents(instance);
            }
            else if (objectType != null) {
                eventDescriptors = TypeDescriptor.GetEvents(objectType);
            }
            else {
                throw new ArgumentException("At least one argument should be non-null");
            }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            Type typeToUse = GetTypeToUseForCBMBridge(objectType);
            string[] eventNames = s_cbmTdpBridge.GetFilteredEvents(typeToUse, bindingFlags);
            
            var filteredEventDescriptors = from e in eventNames
                let d = eventDescriptors[e]
                where d != null select d;

            return new EventDescriptorCollection(filteredEventDescriptors.ToArray());
        }

        internal static System.ComponentModel.AttributeCollection GetAttributes(Type type) {
            ICustomTypeDescriptor descriptor = GetTypeDescriptor(type);
            if (descriptor != null) {
                return descriptor.GetAttributes();
            }
            else {
                return TypeDescriptor.GetAttributes(type);
            }
        }

        internal static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit) {
            Type targetType = GetReflectionType(type);
            return targetType.GetCustomAttributes(attributeType, inherit);
        }

        /// <summary>
        /// Function to return an assembly qualified type name based on the type in the target framework
        /// </summary>
        internal static string TypeNameConverter(Type type) {
            string typeName = null;

            if (type != null) {
                Type targetFrameworkType = GetReflectionType(type);
                if (targetFrameworkType != null) {
                    typeName = targetFrameworkType.AssemblyQualifiedName;
                }
            }

            return typeName;
        }

        private static bool IsFrameworkType(Type type) {
            // We need to make sure a type is a framework type, before we try to use s_cbmTdpBridge on it.
            // Also, isFrameworkType should only be called in this specific scenario, when we are trying
            // to use s_cbmTdpBridge.
            Debug.Assert(s_cbmTdpBridge != null, "s_cbmTdpBridge should not be null in IsFrameworkType");
            bool result;
            if (!s_isFrameworkType.TryGetValue(type, out result)) {
                Assembly a = type.Assembly;
                string path;
                ReferenceAssemblyType referenceAssemblyType = AssemblyResolver.GetPathToReferenceAssembly(a, out path);
                result = (referenceAssemblyType != ReferenceAssemblyType.NonFrameworkAssembly);
                s_isFrameworkType.TryAdd(type, result);
            }
            return result;
        }

        private static PropertyInfo GetMostSpecificProperty(Type type, string name, BindingFlags additionalFlags, Type returnType, Type[] types) {
            BindingFlags flags = BindingFlags.DeclaredOnly;
            flags |= additionalFlags;
            PropertyInfo propInfo;
            Type currentType = type;

            while (currentType != null) {
                // DevDiv #425681: Even with BindingFlags.DeclaredOnly, Type.GetProperty may still throw an
                // AmbiguousMatchException, such as if there exist two properties that differ only by case.
                // If this happens, we need to let that exception propagate up, otherwise GetPropertyHelper
                // will call GetMostSpecificProperty, eventually resulting in stack overflow.
                propInfo = GetProperty(currentType, name, flags, returnType, types, throwAmbiguousMatchException: true);
                if (propInfo != null) {
                    return propInfo;
                }
                else {
                    currentType = currentType.BaseType;
                }
            }

            return null;
        }

        // If the type is a generic type, use the generic type definition instead,
        // in case it has non-framework type arguments.
        private static Type GetTypeToUseForCBMBridge(Type type) {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        internal static bool HasMethod(Type type, string name, BindingFlags bindingAttr) {
            bool hasMethod = false;
            if (s_cbmTdpBridge != null && IsFrameworkType(type)) {
                Type typeToUse = GetTypeToUseForCBMBridge(type);
                hasMethod = s_cbmTdpBridge.HasMethod(typeToUse, name, bindingAttr);
            }
            else {
                Type reflectionType = GetReflectionType(type);
                MethodInfo methodInfo = reflectionType.GetMethod(name, bindingAttr);
                hasMethod = methodInfo != null;
            }
            return hasMethod;
        }
        private static bool SkipCache {
            get {
                // We should not be statically caching items in the VS primary appdomain.
                // - If the cbm bridge is available, caching is ok since we are in a separate appdomain.
                //   (The appdomain gets reset when the project unloads or when a reference assembly is
                //   updated).
                // - Otherwise, we are either already using standard reflection, or we are using the
                //   TFP in the primary appdomain, and should not be caching statically.
                // Dev10 
                return s_cbmTdpBridge == null;
            }
        }

        internal static bool IsSupportedType(Type type) {
            TypeDescriptionProvider provider = GetTargetFrameworkProvider(type);
            if (provider == null) {
                provider = TypeDescriptor.GetProvider(type);
            }
            return provider.IsSupportedType(type);
        }
    }
}
