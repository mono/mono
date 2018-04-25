using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Util;

/// <summary>
/// This is a helper class lives in the VS appdomain where the CBM is created, and passed into the CBM-hosted appdomain
/// to allow cross-appdomain calls to get filtered runtime member information.
/// </summary>
internal class ClientBuildManagerTypeDescriptionProviderBridge : MarshalByRefObject {
    private TypeDescriptionProvider _targetFrameworkProvider;

    internal ClientBuildManagerTypeDescriptionProviderBridge(TypeDescriptionProvider typeDescriptionProvider) {
        _targetFrameworkProvider = typeDescriptionProvider;
    }

    public override Object InitializeLifetimeService() {
        return null; // never expire lease
    }

    private Type GetReflectionType(Type type) {
        Debug.Assert(_targetFrameworkProvider != null, "_targetFrameworkProvider should not be null");
        if (type == null) {
            return null;
        }
        return _targetFrameworkProvider.GetReflectionType(type);
    }

    private Type[] GetReflectionTypes(Type[] types) {
        if (types == null) {
            return null;
        }
        var reflectionTypes = from t in types select GetReflectionType(t);
        return reflectionTypes.ToArray();
    }

    internal bool HasProperty(Type type, string name, BindingFlags bindingAttr, Type returnType, Type[] types)
    {
        if (_targetFrameworkProvider == null) {
            PropertyInfo runtimePropInfo = type.GetProperty(name, bindingAttr, null /* binder */, returnType, types, null /* modifiers */);
            return runtimePropInfo != null;
        }

        Type reflectionType = GetReflectionType(type);
        Type[] reflectionTypes = GetReflectionTypes(types);

        PropertyInfo reflectionPropertyInfo = reflectionType.GetProperty(name, bindingAttr, null /* binder */, returnType, reflectionTypes, null /* modifiers */);

        return reflectionPropertyInfo != null;
    }

    internal bool HasField(Type type, string name, BindingFlags bindingAttr) {
        
        if (_targetFrameworkProvider == null) {
            FieldInfo runtimeFieldInfo = type.GetField(name, bindingAttr);
            return runtimeFieldInfo != null;
        }

        Type reflectionType = _targetFrameworkProvider.GetReflectionType(type);
        FieldInfo reflectionFieldInfo = reflectionType.GetField(name, bindingAttr);

        return reflectionFieldInfo != null;
    }

    internal bool HasEvent(Type type, string name) {
        
        if (_targetFrameworkProvider == null) {
            EventInfo runtimeEventInfo = type.GetEvent(name);
            return runtimeEventInfo != null;
        }

        Type reflectionType = _targetFrameworkProvider.GetReflectionType(type);
        EventInfo reflectionEventInfo = reflectionType.GetEvent(name);

        return reflectionEventInfo != null;
    }

    private string[] GetMemberNames(MemberInfo[] members) {
        var names = from m in members select m.Name;
        return names.ToArray();
    }

    internal bool HasMethod(Type type, string name, BindingFlags bindingAttr) {
        Type reflectionType = type;
        if (_targetFrameworkProvider != null) {
            reflectionType = GetReflectionType(type);
        }
        MethodInfo methodInfo = reflectionType.GetMethod(name, bindingAttr);
        return methodInfo != null;
    }

    internal string[] GetFilteredProperties(Type type, BindingFlags bindingFlags) {
        PropertyInfo[] runtimeProperties = type.GetProperties(bindingFlags);

        if (_targetFrameworkProvider == null) {
            return GetMemberNames(runtimeProperties);
        }

        Type reflectionType = _targetFrameworkProvider.GetReflectionType(type);
        PropertyInfo[] reflectionProperties = reflectionType.GetProperties(bindingFlags);

        var reflectionPropertyNames = from p in reflectionProperties select p.Name;
        return (from p in runtimeProperties where reflectionPropertyNames.Contains(p.Name) select p.Name).ToArray();
    }

    internal string[] GetFilteredEvents(Type type, BindingFlags bindingFlags) {
        EventInfo[] runtimeEvents= type.GetEvents(bindingFlags);

        if (_targetFrameworkProvider == null) {
            return GetMemberNames(runtimeEvents);
        }

        Type reflectionType = _targetFrameworkProvider.GetReflectionType(type);
        EventInfo[] reflectionEvents= reflectionType.GetEvents(bindingFlags);

        var reflectionEventNames = from e in reflectionEvents select e.Name;
        return (from e in runtimeEvents where reflectionEventNames.Contains(e.Name) select e.Name).ToArray();
    }

}
