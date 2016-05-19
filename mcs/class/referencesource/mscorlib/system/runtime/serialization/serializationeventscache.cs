// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: SerializationEventsCache
**
**
** Purpose: Caches the various serialization events such as On(De)Seriliz(ed)ing
**
**
============================================================*/

namespace System.Runtime.Serialization {

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Diagnostics.Contracts;

    internal class SerializationEvents
    {
        private List<MethodInfo> m_OnSerializingMethods = null;
        private List<MethodInfo> m_OnSerializedMethods = null;
        private List<MethodInfo> m_OnDeserializingMethods = null;
        private List<MethodInfo> m_OnDeserializedMethods = null;

        private List<MethodInfo> GetMethodsWithAttribute(Type attribute, Type t)
        {
            List<MethodInfo> mi = new List<MethodInfo>();
            Type baseType = t;
            // Traverse the hierarchy to find all methods with the particular attribute
            while (baseType != null && baseType != typeof(Object))
            {
                RuntimeType rt = (RuntimeType)baseType;
                // Get all methods which are declared on this type, instance and public or nonpublic
                MethodInfo[] mis = baseType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#if _DEBUG                
                bool found = false;
#endif
                foreach(MethodInfo m in mis)
                {
                    // For each method find if attribute is present, the return type is void and the method is not virtual
                    if (m.IsDefined(attribute, false))
                    {
#if _DEBUG
                        Contract.Assert(m.ReturnType == typeof(void) && !m.IsVirtual, "serialization events methods cannot be virtual and need to have void return");
                            
                        ParameterInfo[] paramInfo = m.GetParameters();
                        // Only add it if this method has one parameter of type StreamingContext
                        if (paramInfo.Length == 1 && paramInfo[0].ParameterType == typeof(StreamingContext))
                        {
                            if (found)
                                Contract.Assert(false, "Mutliple methods with same serialization attribute");
#endif                            
                            mi.Add(m);
#if _DEBUG
                            found = true;
                        }
                        else
                            Contract.Assert(false, "Incorrect serialization event signature");
#endif                        
                    }
                }
#if _DEBUG
                found = false;
#endif
                baseType = baseType.BaseType;
            }
            mi.Reverse();  // We should invoke the methods starting from base

            return (mi.Count == 0) ? null : mi;
        }

        internal SerializationEvents(Type t)
        {
            // Initialize all events
            m_OnSerializingMethods = GetMethodsWithAttribute(typeof(OnSerializingAttribute), t);
            m_OnSerializedMethods = GetMethodsWithAttribute(typeof(OnSerializedAttribute), t);
            m_OnDeserializingMethods = GetMethodsWithAttribute(typeof(OnDeserializingAttribute), t);
            m_OnDeserializedMethods = GetMethodsWithAttribute(typeof(OnDeserializedAttribute), t);
        }

        internal bool HasOnSerializingEvents
        {
            get { return m_OnSerializingMethods != null || m_OnSerializedMethods != null; }
        }

        [System.Security.SecuritySafeCritical]
        internal void InvokeOnSerializing(Object obj, StreamingContext context)
        {
            Contract.Assert(obj != null, "object should have been initialized");
            // Invoke all OnSerializingMethods
            if (m_OnSerializingMethods != null)
            {
                Object[] p = new Object[] {context};
                SerializationEventHandler handler = null;
                foreach(MethodInfo m in m_OnSerializingMethods)
                {
                    SerializationEventHandler onSerializing =
                        (SerializationEventHandler)Delegate.CreateDelegateNoSecurityCheck((RuntimeType)typeof(SerializationEventHandler), obj, m);
                    handler = (SerializationEventHandler)Delegate.Combine(handler, onSerializing);
                }
                handler(context);
            }
        }

        [System.Security.SecuritySafeCritical]
        internal void InvokeOnDeserializing(Object obj, StreamingContext context)
        {
            Contract.Assert(obj != null, "object should have been initialized");
            // Invoke all OnDeserializingMethods
            if (m_OnDeserializingMethods != null)
            {
                Object[] p = new Object[] {context};
                SerializationEventHandler handler = null;
                foreach(MethodInfo m in m_OnDeserializingMethods)
                {
                    SerializationEventHandler onDeserializing =
                        (SerializationEventHandler)Delegate.CreateDelegateNoSecurityCheck((RuntimeType)typeof(SerializationEventHandler), obj, m);
                    handler = (SerializationEventHandler)Delegate.Combine(handler, onDeserializing);
                }
                handler(context);
            }
        }

        [System.Security.SecuritySafeCritical]
        internal void InvokeOnDeserialized(Object obj, StreamingContext context)
        {
            Contract.Assert(obj != null, "object should have been initialized");
            // Invoke all OnDeserializingMethods
            if (m_OnDeserializedMethods != null)
            {
                Object[] p = new Object[] {context};
                SerializationEventHandler handler = null;
                foreach(MethodInfo m in m_OnDeserializedMethods)
                {
                    SerializationEventHandler onDeserialized =
                        (SerializationEventHandler)Delegate.CreateDelegateNoSecurityCheck((RuntimeType)typeof(SerializationEventHandler), obj, m);
                    handler = (SerializationEventHandler)Delegate.Combine(handler, onDeserialized);
                }
                handler(context);
            }
        }

        [System.Security.SecurityCritical]
        internal SerializationEventHandler AddOnSerialized(Object obj, SerializationEventHandler handler)
        {
            // Add all OnSerialized methods to a delegate
            if (m_OnSerializedMethods != null)
            {
                foreach(MethodInfo m in m_OnSerializedMethods)
                {
                    SerializationEventHandler onSerialized =
                        (SerializationEventHandler)Delegate.CreateDelegateNoSecurityCheck((RuntimeType)typeof(SerializationEventHandler), obj, m);
                    handler = (SerializationEventHandler)Delegate.Combine(handler, onSerialized);
                }
            }
            return handler;
        }

        [System.Security.SecurityCritical]
        internal SerializationEventHandler AddOnDeserialized(Object obj, SerializationEventHandler handler)
        {
            // Add all OnDeserialized methods to a delegate
            if (m_OnDeserializedMethods != null)
            {
                foreach(MethodInfo m in m_OnDeserializedMethods)
                {
                    SerializationEventHandler onDeserialized =
                        (SerializationEventHandler)Delegate.CreateDelegateNoSecurityCheck((RuntimeType)typeof(SerializationEventHandler), obj, m);
                    handler = (SerializationEventHandler)Delegate.Combine(handler, onDeserialized);
                }
            }
            return handler;
        }
    }

    internal static class SerializationEventsCache
    {
        private static Hashtable cache = new Hashtable();

        internal static SerializationEvents GetSerializationEventsForType(Type t)
        {
            SerializationEvents events;
            if ((events = (SerializationEvents)cache[t]) == null)
            {
                lock(cache.SyncRoot)
                {
                    if ((events = (SerializationEvents)cache[t]) == null)
                    {
                        events = new SerializationEvents(t);
                        cache[t] = events; // Add this to the cache.
                    }
                }
            }
            return events;

        }
    }
}

