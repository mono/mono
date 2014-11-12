// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: TypeInfo
**
** <OWNER>[....]</OWNER>
**
**
** Purpose: Notion of a type definition
**
**
=============================================================================*/

namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Diagnostics.Tracing;

    //all today's runtime Type derivations derive now from TypeInfo
    //we make TypeInfo implement IRCT - simplifies work
    [System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public abstract class TypeInfo:Type,IReflectableType
    {
        [FriendAccessAllowed]
        internal TypeInfo() { }

        TypeInfo IReflectableType.GetTypeInfo(){
            return this;
        }
        public virtual Type AsType(){
            return (Type)this;
        }

        public virtual Type[] GenericTypeParameters{
            get{
                if(IsGenericTypeDefinition){
                    return GetGenericArguments();
                }
                else{
                    return Type.EmptyTypes;
                }

            }
        }
        //a re-implementation of ISAF from Type, skipping the use of UnderlyingType
        [Pure]
        public virtual bool IsAssignableFrom(TypeInfo typeInfo)
        {
            if (typeInfo == null)
                return false;

            if (this == typeInfo)
                return true;

            // If c is a subclass of this class, then c can be cast to this type.
            if (typeInfo.IsSubclassOf(this))
                return true;

            if (this.IsInterface)
            {
                return typeInfo.ImplementInterface(this);
            }
            else if (IsGenericParameter)
            {
                Type[] constraints = GetGenericParameterConstraints();
                for (int i = 0; i < constraints.Length; i++)
                    if (!constraints[i].IsAssignableFrom(typeInfo))
                        return false;

                return true;
            }

            return false;
        }
#region moved over from Type
   // Fields

        public virtual EventInfo GetDeclaredEvent(String name)
        {
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.BeginGetRuntimeEvent(GetFullNameForEtw(), name != null ? name : "");
            }
#endif
            EventInfo ei = GetEvent(name, Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.EndGetRuntimeEvent(GetFullNameForEtw(), ei != null ? ei.GetFullNameForEtw() : "");
            }
#endif
            return ei;
        }
        public virtual FieldInfo GetDeclaredField(String name)
        {
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.BeginGetRuntimeField(GetFullNameForEtw(), name != null ? name : "");
            }
#endif
            FieldInfo fi = GetField(name, Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.EndGetRuntimeField(GetFullNameForEtw(), fi != null ? fi.GetFullNameForEtw() : "");
            }
#endif
            return fi;
        }
        public virtual MethodInfo GetDeclaredMethod(String name)
        {
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.BeginGetRuntimeMethod(GetFullNameForEtw(), name != null ? name : "");
            }
#endif
            MethodInfo mi = GetMethod(name, Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.EndGetRuntimeMethod(GetFullNameForEtw(), mi != null ? mi.GetFullNameForEtw() : "");
            }
#endif
            return mi;
        }

        public virtual IEnumerable<MethodInfo> GetDeclaredMethods(String name)
        {
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.BeginGetRuntimeMethods(GetFullNameForEtw());
            }
#endif
            
            foreach (MethodInfo method in GetMethods(Type.DeclaredOnlyLookup))
            {
                if (method.Name == name)
                    yield return method;
            }

#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.EndGetRuntimeMethods(GetFullNameForEtw());
            }
#endif
        }
        public virtual System.Reflection.TypeInfo GetDeclaredNestedType(String name)
        {
            var nt=GetNestedType(name, Type.DeclaredOnlyLookup);
            if(nt == null){
                return null; //the extension method GetTypeInfo throws for null
            }else{
                return nt.GetTypeInfo();
            }
        }
        public virtual PropertyInfo GetDeclaredProperty(String name)
        {
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.BeginGetRuntimeProperty(GetFullNameForEtw(), name != null ? name : "");
            }
#endif
            PropertyInfo pi = GetProperty(name, Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
            {
                FrameworkEventSource.Log.EndGetRuntimeProperty(GetFullNameForEtw(), pi != null ? pi.GetFullNameForEtw() : "");
            }
#endif
            return pi;
        }





    // Properties

        public virtual IEnumerable<ConstructorInfo> DeclaredConstructors
        {
            get
            {
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.BeginGetRuntimeConstructors(GetFullNameForEtw());
                }
#endif
                IEnumerable<ConstructorInfo> constructors = GetConstructors(Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.EndGetRuntimeConstructors(GetFullNameForEtw());
                }
#endif
                return constructors;
            }
        }

        public virtual IEnumerable<EventInfo> DeclaredEvents
        {
            get
            {
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.BeginGetRuntimeEvents(GetFullNameForEtw());
                }
#endif
                IEnumerable<EventInfo> events = GetEvents(Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.EndGetRuntimeEvents(GetFullNameForEtw());
                }
#endif
                return events;
            }
        }

        public virtual IEnumerable<FieldInfo> DeclaredFields
        {
            get
            {
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.BeginGetRuntimeFields(GetFullNameForEtw());
                }
#endif
                IEnumerable<FieldInfo> fields = GetFields(Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.EndGetRuntimeFields(GetFullNameForEtw());
                }
#endif
                return fields;
            }
        }

        public virtual IEnumerable<MemberInfo> DeclaredMembers
        {
            get
            {
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.BeginGetRuntimeMembers(GetFullNameForEtw());
                }
#endif
                IEnumerable<MemberInfo> members = GetMembers(Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.EndGetRuntimeMembers(GetFullNameForEtw());
                }
#endif
                return members;
            }
        }

        public virtual IEnumerable<MethodInfo> DeclaredMethods
        {
            get
            {
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.BeginGetRuntimeMethods(GetFullNameForEtw());
                }
#endif
                IEnumerable<MethodInfo> methods = GetMethods(Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.EndGetRuntimeMethods(GetFullNameForEtw());
                }
#endif
                return methods;
            }
        }
        public virtual IEnumerable<System.Reflection.TypeInfo> DeclaredNestedTypes
        {
            get
            {
                foreach (var t in GetNestedTypes(Type.DeclaredOnlyLookup)){
	        		yield return t.GetTypeInfo();
    		    }
            }
        }

        public virtual IEnumerable<PropertyInfo> DeclaredProperties
        {
            get
            {
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.BeginGetRuntimeProperties(GetFullNameForEtw());
                }
#endif
                IEnumerable<PropertyInfo> properties = GetProperties(Type.DeclaredOnlyLookup);
#if !FEATURE_CORECLR
                if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled(EventLevel.Informational, FrameworkEventSource.Keywords.DynamicTypeUsage))
                {
                    FrameworkEventSource.Log.EndGetRuntimeProperties(GetFullNameForEtw());
                }
#endif
                return properties;
            }
        }


        public virtual IEnumerable<Type> ImplementedInterfaces
        {
            get
            {
                return GetInterfaces();
            }
        }

 
#endregion        

    }
}

