//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#region Using directives

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Workflow.Runtime;
using System.Xml;

#endregion

namespace System.Workflow.Activities
{
    internal static class CorrelationResolver
    {
        static Dictionary<Type, CorrelationMethodResolver> cachedTypeResolver = new Dictionary<Type, CorrelationMethodResolver>();
        static object mutex = new object();

        internal static bool IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (memberName == null)
                throw new ArgumentNullException("memberName");
            if (memberName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EventNameMissing));

            ICorrelationProvider correlationProvider = CorrelationResolver.GetCorrelationProvider(interfaceType);
            return correlationProvider.IsInitializingMember(interfaceType, memberName, methodArgs);
        }

        internal static ICollection<CorrelationProperty> ResolveCorrelationValues(Type interfaceType, string eventName, object[] eventArgs, bool provideInitializerTokens)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (eventName == null)
                throw new ArgumentNullException("eventName");
            if (eventName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_EventNameMissing));

            ICorrelationProvider correlationProvider = CorrelationResolver.GetCorrelationProvider(interfaceType);
            return correlationProvider.ResolveCorrelationPropertyValues(interfaceType, eventName, eventArgs, provideInitializerTokens);
        }

        internal static ICorrelationProvider GetCorrelationProvider(Type interfaceType)
        {
            CorrelationMethodResolver resolver = GetResolver(interfaceType);
            return resolver.CorrelationProvider;
        }

        private static CorrelationMethodResolver GetResolver(Type interfaceType)
        {
            CorrelationMethodResolver resolver = null;
            cachedTypeResolver.TryGetValue(interfaceType, out resolver);
            if (resolver == null)
            {
                lock (mutex)
                {
                    cachedTypeResolver.TryGetValue(interfaceType, out resolver);
                    if (resolver == null)
                    {
                        resolver = new CorrelationMethodResolver(interfaceType);
                        cachedTypeResolver.Add(interfaceType, resolver);
                    }
                }
            }
            return resolver;
        }
    }

    internal sealed class CorrelationMethodResolver
    {
        Type interfaceType;

        // a correlation provider for each interface type
        ICorrelationProvider correlationProvider;
        object corrProviderSync = new object();

        internal CorrelationMethodResolver(Type interfaceType)
        {
            this.interfaceType = interfaceType;
        }

        internal ICorrelationProvider CorrelationProvider
        {
            get
            {
                if (this.correlationProvider == null)
                {
                    lock (this.corrProviderSync)
                    {
                        if (this.correlationProvider == null)
                        {
                            ICorrelationProvider provider = null;
                            object[] corrProviderAttribs = this.interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), true);
                            if (corrProviderAttribs.Length == 0)
                            {
                                corrProviderAttribs = this.interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), true);
                                object[] corrParameterAttribs = this.interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), true);
                                if (corrProviderAttribs.Length != 0 && corrParameterAttribs.Length != 0)
                                {
                                    // no provider specified but it is a data exchange correlation service 
                                    // hence use our default correlation
                                    provider = new DefaultCorrelationProvider(this.interfaceType);
                                }
                                else
                                {
                                    // opaque interface with no correlation
                                    provider = new NonCorrelatedProvider();
                                }
                            }
                            else
                            {
                                CorrelationProviderAttribute cpattrib = corrProviderAttribs[0] as CorrelationProviderAttribute;
                                Type providerType = cpattrib.CorrelationProviderType;
                                provider = Activator.CreateInstance(providerType) as ICorrelationProvider;
                            }

                            System.Threading.Thread.MemoryBarrier();
                            this.correlationProvider = provider;
                        }
                    }
                }

                return this.correlationProvider;
            }
        }
    }

    internal sealed class CorrelationPropertyValue
    {
        string name;
        string locationPath;
        int signaturePosition;

        internal CorrelationPropertyValue(string name, string locationPath, int signaturePosition)
        {
            this.name = name;
            this.locationPath = locationPath;
            this.signaturePosition = signaturePosition;
        }

        internal object GetValue(object[] args)
        {
            if (args.Length <= this.signaturePosition)
                throw new ArgumentOutOfRangeException("args");

            object arg = args[this.signaturePosition];
            if (arg == null)
                return arg;
            Type type = arg.GetType();
            object val = arg;
            if (this.locationPath.Length != 0)
            {
                string[] split = locationPath.Split(new Char[] { '.' });
                for (int i = 1; i < split.Length; i++)
                {
                    string s = split[i];
                    if (null == arg)
                        break;
                    val = type.InvokeMember(s, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty, null, arg, null, null);

                    MemberInfo[] mInfos = type.GetMember(s, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty);

                    type = GetMemberType(mInfos[0]);
                    arg = val;
                }
            }
            return val;
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        private Type GetMemberType(MemberInfo mInfo)
        {
            Type type = null;
            switch (mInfo.MemberType)
            {
                case MemberTypes.Field:
                    type = ((FieldInfo)mInfo).FieldType;
                    break;

                case MemberTypes.Property:
                    type = ((PropertyInfo)mInfo).PropertyType;
                    break;

                default:
                    Debug.Assert(false, "locationPath points to something other than a Field/Property");
                    return null;
            }

            return type;
        }
    }

    internal sealed class DefaultCorrelationProvider : ICorrelationProvider
    {
        Type interfaceType;

        // map of method name to correlation properties
        Dictionary<string, CorrelationPropertyValue[]> cachedCorrelationProperties;
        object cachedCorrelationPropertiesSync = new object();

        // cached initializers
        // map operation to bool flag to indicate events
        Dictionary<string, bool> initializerCorrelationPropertys = null;
        object initializerCorrelationPropertysSync = new object();

        internal DefaultCorrelationProvider(Type interfaceType)
        {
            this.cachedCorrelationProperties = new Dictionary<string, CorrelationPropertyValue[]>();
            this.interfaceType = interfaceType;
        }

        ICollection<CorrelationProperty> ICorrelationProvider.ResolveCorrelationPropertyValues(Type interfaceType, string methodName, object[] methodArgs, bool provideInitializerTokens)
        {
            CorrelationPropertyValue[] correlationProperties = null;

            if (methodArgs == null || provideInitializerTokens)
            {
                return null; // no initializer specific token to return
            }

            this.cachedCorrelationProperties.TryGetValue(methodName, out correlationProperties);
            if (correlationProperties == null)
            {
                lock (this.cachedCorrelationPropertiesSync)
                {
                    this.cachedCorrelationProperties.TryGetValue(methodName, out correlationProperties);
                    if (correlationProperties == null)
                    {
                        correlationProperties = GetCorrelationProperties(interfaceType, methodName);
                        this.cachedCorrelationProperties.Add(methodName, correlationProperties);
                    }
                }
            }

            List<CorrelationProperty> predicates = new List<CorrelationProperty>();
            for (int i = 0; i < correlationProperties.Length; i++)
            {
                predicates.Add(new CorrelationProperty(correlationProperties[i].Name, correlationProperties[i].GetValue(methodArgs)));
            }

            return predicates;
        }

        private Dictionary<string, bool> InitializerCorrelationPropertys
        {
            get
            {
                if (this.initializerCorrelationPropertys == null)
                {
                    lock (this.initializerCorrelationPropertysSync)
                    {
                        if (this.initializerCorrelationPropertys == null)
                        {
                            Dictionary<string, bool> members = new Dictionary<string, bool>();
                            // note this is separated out since we may need to distinguish between events & methods
                            foreach (EventInfo member in this.interfaceType.GetEvents())
                            {
                                if ((member.GetCustomAttributes(typeof(CorrelationInitializerAttribute), true)).Length > 0)
                                {
                                    members.Add(member.Name, true);
                                }
                            }
                            foreach (MethodInfo member in this.interfaceType.GetMethods())
                            {
                                if ((member.GetCustomAttributes(typeof(CorrelationInitializerAttribute), true)).Length > 0)
                                {
                                    members.Add(member.Name, false);
                                }
                            }

                            this.initializerCorrelationPropertys = members;
                        }
                    }
                }
                return this.initializerCorrelationPropertys;
            }
        }

        bool ICorrelationProvider.IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs)
        {
            return InitializerCorrelationPropertys.ContainsKey(memberName);
        }

        private CorrelationPropertyValue[] GetCorrelationProperties(Type interfaceType, string methodName)
        {
            CorrelationPropertyValue[] correlationProperties = null;

            if (interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), true).Length == 0)
                throw new InvalidOperationException(SR.GetString(SR.Error_ExternalDataExchangeException, interfaceType.AssemblyQualifiedName));

            List<Object> correlationParamAttributes = new List<Object>();
            correlationParamAttributes.AddRange(GetCorrelationParameterAttributes(interfaceType));

            if (correlationParamAttributes.Count == 0)
                throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationParameterException, interfaceType.AssemblyQualifiedName));

            correlationProperties = new CorrelationPropertyValue[correlationParamAttributes.Count];

            Dictionary<String, CorrelationAliasAttribute> corrAliases = null;
            MethodInfo methodInfo = null;

            GetMethodInfo(interfaceType, methodName, out methodInfo, out corrAliases);

            if (methodInfo == null)
            {
                throw new MissingMethodException(interfaceType.AssemblyQualifiedName, methodName);
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();

            int i = 0;
            foreach (CorrelationParameterAttribute paramAttribute in correlationParamAttributes)
            {
                String location = paramAttribute.Name;
                CorrelationAliasAttribute aliasAttribute = GetMatchingCorrelationAlias(paramAttribute, corrAliases, correlationParamAttributes.Count == 1);

                if (aliasAttribute != null)
                    location = aliasAttribute.Path;

                CorrelationPropertyValue value = GetCorrelationProperty(parameters, paramAttribute.Name, location);
                if (value == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationParameterException, interfaceType.AssemblyQualifiedName, paramAttribute.Name, methodName));

                correlationProperties[i++] = value;
            }
            return correlationProperties;
        }

        private CorrelationAliasAttribute GetMatchingCorrelationAlias(CorrelationParameterAttribute paramAttribute, Dictionary<String, CorrelationAliasAttribute> correlationAliases, bool defaultParameter)
        {
            CorrelationAliasAttribute corrAlias = null;

            if (correlationAliases == null) return null;

            if (defaultParameter)
            {
                if (correlationAliases.TryGetValue("", out corrAlias))
                {
                    return corrAlias;
                }
            }

            correlationAliases.TryGetValue(paramAttribute.Name, out corrAlias);
            return corrAlias;
        }

        private CorrelationPropertyValue GetCorrelationProperty(ParameterInfo[] parameters, String propertyName, String location)
        {
            string[] split = location.Split(new Char[] { '.' });

            if (split.Length == 1 && parameters.Length == 2)
            {
                if (typeof(ExternalDataEventArgs).IsAssignableFrom(parameters[1].ParameterType))
                {
                    string aliasedLocation = "e." + location;
                    return GetCorrelationProperty(parameters, propertyName, "e", aliasedLocation);
                }

            }
            string parameterName = split[0];

            return GetCorrelationProperty(parameters, propertyName, parameterName, location);
        }

        private void GetMethodInfo(Type interfaceType, string methodName, out MethodInfo methodInfo, out Dictionary<String, CorrelationAliasAttribute> correlationAliases)
        {
            correlationAliases = new Dictionary<String, CorrelationAliasAttribute>();
            Object[] customAttrs = null;
            methodInfo = null;
            // check events
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            EventInfo eventInfo = interfaceType.GetEvent(methodName, bindingFlags);
            if (eventInfo != null)
            {
                customAttrs = eventInfo.GetCustomAttributes(typeof(CorrelationAliasAttribute), true);
                if (customAttrs == null || customAttrs.Length == 0)
                {
                    customAttrs = eventInfo.EventHandlerType.GetCustomAttributes(typeof(CorrelationAliasAttribute), true);
                }
                MethodInfo[] methInfo = eventInfo.EventHandlerType.GetMethods();
                methodInfo = methInfo[0];
            }
            else
            {
                // check methods
                methodInfo = interfaceType.GetMethod(methodName, bindingFlags);
                if (methodInfo == null)
                {
                    throw new MissingMethodException(interfaceType.AssemblyQualifiedName, methodName);
                }
                customAttrs = methodInfo.GetCustomAttributes(typeof(CorrelationAliasAttribute), true);
            }

            foreach (CorrelationAliasAttribute aliasAttribute in customAttrs)
            {
                if (customAttrs.Length > 1)
                {
                    Debug.Assert(aliasAttribute.Name != null);
                    if (aliasAttribute.Name == null)
                        throw new ArgumentNullException("ParameterName");
                }
                correlationAliases.Add(aliasAttribute.Name == null ? "" : aliasAttribute.Name, aliasAttribute);
            }
        }

        private CorrelationPropertyValue GetCorrelationProperty(ParameterInfo[] parameters, string propertyName, string parameterName, string location)
        {
            for (int j = 0; parameters != null && j < parameters.Length; j++)
            {
                ParameterInfo param = parameters[j];
                if (param.Name == parameterName)
                {
                    // parameter match
                    return new CorrelationPropertyValue(propertyName, location, param.Position);
                }
            }

            return null;
        }

        private object[] GetCorrelationParameterAttributes(Type type)
        {
            return type.GetCustomAttributes(typeof(CorrelationParameterAttribute), true);
        }
    }

    internal sealed class NonCorrelatedProvider : ICorrelationProvider
    {
        internal NonCorrelatedProvider()
        {
        }

        ICollection<CorrelationProperty> ICorrelationProvider.ResolveCorrelationPropertyValues(Type interfaceType, string methodName, object[] methodArgs, bool provideInitializerTokens)
        {
            // non correlated 
            // no values to return
            return null;
        }

        bool ICorrelationProvider.IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs)
        {
            return true;
        }
    }
}
