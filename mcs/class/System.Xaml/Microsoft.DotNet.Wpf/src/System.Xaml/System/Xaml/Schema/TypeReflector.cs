// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Xaml;
using XAML3 = System.Windows.Markup;
using System.Xaml.MS.Impl;

namespace System.Xaml.Schema
{
    class TypeReflector : Reflector
    {
        private const XamlCollectionKind XamlCollectionKindInvalid = (XamlCollectionKind)byte.MaxValue;

        private const BindingFlags AllProperties_BF
            = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags AttachableProperties_BF
            = BindingFlags.Static | BindingFlags.FlattenHierarchy
            | BindingFlags.Public | BindingFlags.NonPublic;

        private static TypeReflector s_UnknownReflector;

        // Thread safety: MemberDictionary implements its own thread-safety.
        // Lazy init: These fields are null when uninitialized, and must only be initialized once.
        private ThreadSafeDictionary<string, XamlMember> _nonAttachableMemberCache;
        private ThreadSafeDictionary<string, XamlMember> _attachableMemberCache;

        // Thread safety: never access directly (outside of ctor); always call GetFlag() or SetFlag()
        private int _boolTypeBits;

        // Thread safety: These dictionaries are thread-safe; always use TryAdd/TryUpdate to write
        // Lazy init: These fields are null when uninitialized, and must only be initialized once.
        private ThreadSafeDictionary<int, IList<XamlType>> _positionalParameterTypes;
        private ConcurrentDictionary<XamlDirective, XamlMember> _aliasedProperties;

        // Lazy init: set to XamlCollectionKindInvalid when uninitialized
        private XamlCollectionKind _collectionKind;

        // Thread safety:
        // All fields below should either be either uninitialized or complete;
        // never assign intermediate values to them.
        // All fields below should be idempotent; assignment race conditions are harmless.

        // Lazy init: Check NullableReference.IsSet to determine if these fields have been initialized
        private NullableReference<XamlMember> _contentProperty;
        private NullableReference<XamlMember> _runtimeNameProperty;
        private NullableReference<XamlMember> _xmlLangProperty;
        private NullableReference<XamlMember> _dictionaryKeyProperty;
        private NullableReference<XamlMember> _uidProperty;

        private NullableReference<MethodInfo> _isReadOnlyMethod;
        private NullableReference<XamlValueConverter<TypeConverter>> _typeConverter;
        private NullableReference<XamlValueConverter<XAML3.ValueSerializer>> _valueSerializer;
        private NullableReference<XamlValueConverter<XamlDeferringLoader>> _deferringLoader;

        private NullableReference<EventHandler<XAML3.XamlSetMarkupExtensionEventArgs>> _xamlSetMarkupExtensionHandler;
        private NullableReference<EventHandler<XAML3.XamlSetTypeConverterEventArgs>> _xamlSetTypeConverterHandler;

        private NullableReference<MethodInfo> _addMethod;
        private NullableReference<XamlType> _baseType;
        private NullableReference<MethodInfo> _getEnumeratorMethod;

        // Used for UnknownReflector only
        private TypeReflector()
        {
            _nonAttachableMemberCache = new ThreadSafeDictionary<string, XamlMember>();
            _nonAttachableMemberCache.IsComplete = true;
            _attachableMemberCache = new ThreadSafeDictionary<string, XamlMember>();
            _attachableMemberCache.IsComplete = true;
            
            _baseType.Value = XamlLanguage.Object;
            _boolTypeBits = (int)BoolTypeBits.Default | (int)BoolTypeBits.Unknown | (int)BoolTypeBits.WhitespaceSignificantCollection | (int)BoolTypeBits.AllValid;
            _collectionKind = XamlCollectionKind.None;

            // Set all the nullable references explicitly so that IsSet will be equal to true
            _addMethod.Value = null;
            _contentProperty.Value = null;
            _deferringLoader.Value = null;
            _dictionaryKeyProperty.Value = null;
            _getEnumeratorMethod.Value = null;
            _isReadOnlyMethod.Value = null;
            _runtimeNameProperty.Value = null;
            _typeConverter.Value = null;
            _uidProperty.Value = null;
            _valueSerializer.Value = null;
            _xamlSetMarkupExtensionHandler.Value = null;
            _xamlSetTypeConverterHandler.Value = null;
            _xmlLangProperty.Value = null;
            CustomAttributeProvider = null;

            Invoker = XamlTypeInvoker.UnknownInvoker;
        }

        public TypeReflector(Type underlyingType)
        {
            UnderlyingType = underlyingType;
            _collectionKind = XamlCollectionKindInvalid;
        }

        internal static TypeReflector UnknownReflector
        {
            get
            {
                if (s_UnknownReflector == null)
                {
                    s_UnknownReflector = new TypeReflector();
                }
                return s_UnknownReflector;
            }
        }

        #region Static visbility helpers

        internal static bool IsVisibleTo(Type type, Assembly accessingAssembly, XamlSchemaContext schemaContext)
        {
            TypeVisibility visibility = GetVisibility(type);
            if (visibility == TypeVisibility.NotVisible)
            {
                return false;
            }
            if (visibility == TypeVisibility.Internal &&
                !schemaContext.AreInternalsVisibleTo(type.Assembly, accessingAssembly))
            {
                return false;
            }
            if (type.IsGenericType)
            {
                foreach (Type typeArg in type.GetGenericArguments())
                {
                    if (!IsVisibleTo(typeArg, accessingAssembly, schemaContext))
                    {
                        return false;
                    }
                }
            }
            else if (type.HasElementType)
            {
                return IsVisibleTo(type.GetElementType(), accessingAssembly, schemaContext);
            }
            return true;
        }

        internal static bool IsInternal(Type type)
        {
            return GetVisibility(type) == TypeVisibility.Internal;
        }

        internal static bool IsPublicOrInternal(MethodBase method)
        {
            return method.IsPublic || method.IsAssembly || method.IsFamilyOrAssembly;
        }

        #endregion

        #region Data storage

        internal IList<XamlType> AllowedContentTypes { get; set; }

        internal ThreadSafeDictionary<string, XamlMember> AttachableMembers
        {
            get
            {
                if (_attachableMemberCache == null)
                {
                    Interlocked.CompareExchange(ref _attachableMemberCache,
                        new ThreadSafeDictionary<string, XamlMember>(), null);
                }
                return _attachableMemberCache;
            }
        }

        internal XamlType BaseType
        {
            get { return _baseType.Value; }
            set { _baseType.Value = value; }
        }

        internal bool BaseTypeIsSet
        {
            get { return _baseType.IsSet; }
        }

        internal XamlCollectionKind CollectionKind
        {
            get { return _collectionKind; }
            set { _collectionKind = value; }
        }

        internal bool CollectionKindIsSet { get { return _collectionKind != XamlCollectionKindInvalid; } }

        internal XamlMember ContentProperty
        {
            get { return _contentProperty.Value; }
            set { _contentProperty.Value = value; }
        }

        internal bool ContentPropertyIsSet { get { return _contentProperty.IsSet; } }

        internal IList<XamlType> ContentWrappers { get; set; }

        internal XamlValueConverter<XamlDeferringLoader> DeferringLoader
        {
            get { return _deferringLoader.Value; }
            set { _deferringLoader.Value = value; }
        }

        internal bool DeferringLoaderIsSet { get { return _deferringLoader.IsSet; } }

        internal ICollection<XamlMember> ExcludedReadOnlyMembers { get; set; }

        internal XamlType KeyType { get; set; }

        internal XamlTypeInvoker Invoker { get; set; }

        internal MethodInfo IsReadOnlyMethod
        {
            get { return _isReadOnlyMethod.Value; }
            set { _isReadOnlyMethod.Value = value; }
        }

        internal bool IsReadOnlyMethodIsSet { get { return _isReadOnlyMethod.IsSet; } }

        // No need to check valid flag, this is set in constructor
        internal bool IsUnknown { get { return (_boolTypeBits & (int)BoolTypeBits.Unknown) != 0; } }

        internal XamlType ItemType { get; set; }

        internal XamlType MarkupExtensionReturnType { get; set; }

        internal ThreadSafeDictionary<string, XamlMember> Members
        {
            get
            {
                if (_nonAttachableMemberCache == null)
                {
                    Interlocked.CompareExchange(ref _nonAttachableMemberCache,
                        new ThreadSafeDictionary<string, XamlMember>(), null);
                }
                return _nonAttachableMemberCache;
            }
        }

        internal Dictionary<int, IList<XamlType>> ReflectedPositionalParameters { get; set; }

        internal XamlValueConverter<TypeConverter> TypeConverter
        {
            get { return _typeConverter.Value; }
            set { _typeConverter.Value = value; }
        }

        internal bool TypeConverterIsSet { get { return _typeConverter.IsSet; } }

        internal Type UnderlyingType { get; set; }

        internal XamlValueConverter<XAML3.ValueSerializer> ValueSerializer
        {
            get { return _valueSerializer.Value; }
            set { _valueSerializer.Value = value; }
        }

        internal bool ValueSerializerIsSet { get { return _valueSerializer.IsSet; } }

        internal EventHandler<XAML3.XamlSetMarkupExtensionEventArgs> XamlSetMarkupExtensionHandler
        {
            get { return _xamlSetMarkupExtensionHandler.Value; }
            set { _xamlSetMarkupExtensionHandler.Value = value; }
        }

        internal bool XamlSetMarkupExtensionHandlerIsSet { get { return _xamlSetMarkupExtensionHandler.IsSet; } }

        internal EventHandler<XAML3.XamlSetTypeConverterEventArgs> XamlSetTypeConverterHandler
        {
            get { return _xamlSetTypeConverterHandler.Value; }
            set { _xamlSetTypeConverterHandler.Value = value; }
        }

        internal bool XamlSetTypeConverterHandlerIsSet { get { return _xamlSetTypeConverterHandler.IsSet; } }

        internal bool TryGetPositionalParameters(int paramCount, out IList<XamlType> result)
        {
            result = null;
            if (_positionalParameterTypes == null)
            {
                if (IsUnknown)
                {
                    return true;
                }

                Interlocked.CompareExchange(ref _positionalParameterTypes,
                    new ThreadSafeDictionary<int, IList<XamlType>>(), null);
            }
            return _positionalParameterTypes.TryGetValue(paramCount, out result);
        }

        internal IList<XamlType> TryAddPositionalParameters(int paramCount, IList<XamlType> paramList)
        {
            Debug.Assert(_positionalParameterTypes != null, "TryGetPositionalParameters should have been called first");
            return _positionalParameterTypes.TryAdd(paramCount, paramList);
        }

        internal bool TryGetAliasedProperty(XamlDirective directive, out XamlMember member)
        {
            member = null;
            if (IsUnknown)
            {
                return true;
            }
            bool result = false;
            if (directive == XamlLanguage.Key)
            {
                result = _dictionaryKeyProperty.IsSet;
                member = _dictionaryKeyProperty.Value;
            }
            else if (directive == XamlLanguage.Name)
            {
                result = _runtimeNameProperty.IsSet;
                member = _runtimeNameProperty.Value;
            }
            else if (directive == XamlLanguage.Uid)
            {
                result = _uidProperty.IsSet;
                member = _uidProperty.Value;
            }
            else if (directive == XamlLanguage.Lang)
            {
                result = _xmlLangProperty.IsSet;
                member = _xmlLangProperty.Value;
            }
            else if (_aliasedProperties != null)
            {
                result = _aliasedProperties.TryGetValue(directive, out member);
            }
            return result;
        }

        internal void TryAddAliasedProperty(XamlDirective directive, XamlMember member)
        {
            Debug.Assert(!IsUnknown);
            if (directive == XamlLanguage.Key)
            {
                _dictionaryKeyProperty.Value = member;
            }
            else if (directive == XamlLanguage.Name)
            {
                _runtimeNameProperty.Value = member;
            }
            else if (directive == XamlLanguage.Uid)
            {
                _uidProperty.Value = member;
            }
            else if (directive == XamlLanguage.Lang)
            {
                _xmlLangProperty.Value = member;
            }
            else
            {
                if (_aliasedProperties == null)
                {
                    var dict = XamlSchemaContext.CreateDictionary<XamlDirective, XamlMember>();
                    Interlocked.CompareExchange(ref _aliasedProperties, dict, null);
                }
                _aliasedProperties.TryAdd(directive, member);
            }
        }

        internal MethodInfo AddMethod
        {
            get { return _addMethod.Value; }
            set { _addMethod.Value = value; }
        }

        internal bool AddMethodIsSet { get { return _addMethod.IsSet; } }

        internal MethodInfo GetEnumeratorMethod
        {
            get { return _getEnumeratorMethod.Value; }
            set { _getEnumeratorMethod.Value = value; }
        }

        internal bool GetEnumeratorMethodIsSet { get { return _getEnumeratorMethod.IsSet; } }

        #endregion

        // We don't cache NameScopeProperty because it's only used at the root.
        // But we have the lookup logic here so that we don't need to do reflection in ObjectWriter.
        internal static XamlMember LookupNameScopeProperty(XamlType xamlType)
        {
            if (xamlType.UnderlyingType == null)
            {
                return null;
            }
            // We only check this once, at the root of the doc, and only in ObjectWriter.
            // So it's fine to use live reflection here.
            object obj = GetCustomAttribute(typeof(XAML3.NameScopePropertyAttribute), xamlType.UnderlyingType);
            XAML3.NameScopePropertyAttribute nspAttr = obj as XAML3.NameScopePropertyAttribute;
            if (nspAttr != null)
            {
                Type ownerType = nspAttr.Type;
                string propertyName = nspAttr.Name;
                XamlMember prop;
                if (ownerType != null)
                {
                    XamlType ownerXamlType = xamlType.SchemaContext.GetXamlType(ownerType);
                    prop = ownerXamlType.GetAttachableMember(propertyName);
                }
                else
                {
                    prop = xamlType.GetMember(propertyName);
                }
                return prop;
            }
            return null;
        }

        #region Member lookup

        internal PropertyInfo LookupProperty(string name)
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            PropertyInfo pi = GetNonIndexerProperty(name);
            if (pi != null && IsPrivate(pi))
            {
                pi = null;
            }
            return pi;
        }

        internal EventInfo LookupEvent(string name)
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            // In case of shadowing, Type.GetEvent returns the most derived Event
            EventInfo ei = UnderlyingType.GetEvent(name, AllProperties_BF);
            if (ei != null && IsPrivate(ei))
            {
                ei = null;
            }
            return ei;
        }

        internal void LookupAllMembers(out ICollection<PropertyInfo> newProperties,
            out ICollection<EventInfo> newEvents, out List<XamlMember> knownMembers)
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            Debug.Assert(_nonAttachableMemberCache != null, "Members property should have been invoked before this");

            PropertyInfo[] propList = UnderlyingType.GetProperties(AllProperties_BF);
            EventInfo[] eventList = UnderlyingType.GetEvents(AllProperties_BF);
            knownMembers = new List<XamlMember>(propList.Length + eventList.Length);
            newProperties = FilterProperties(propList, knownMembers, true);
            newEvents = FilterEvents(eventList, knownMembers);
        }

        // Returns properties that don't yet have corresponding XamlMembers
        internal IList<PropertyInfo> LookupRemainingProperties()
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            Debug.Assert(_nonAttachableMemberCache != null, "Members property should have been invoked before this");
            PropertyInfo[] propList = UnderlyingType.GetProperties(AllProperties_BF);
            return FilterProperties(propList, null, false);
        }

        private IList<PropertyInfo> FilterProperties(PropertyInfo[] propList, List<XamlMember> knownMembers, bool skipKnownNegatives)
        {
            Dictionary<string, PropertyInfo> result = new Dictionary<string, PropertyInfo>(propList.Length);
            for (int i = 0; i < propList.Length; i++)
            {
                PropertyInfo currentProp = propList[i];
                if (currentProp.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                XamlMember knownMember;
                if (_nonAttachableMemberCache.TryGetValue(currentProp.Name, out knownMember))
                {
                    if (knownMember != null)
                    {
                        if (knownMembers != null)
                        {
                            knownMembers.Add(knownMember);
                        }
                        continue;
                    }
                    else if (skipKnownNegatives)
                    {
                        continue;
                    }
                }

                PropertyInfo shadowedProp;
                if (result.TryGetValue(currentProp.Name, out shadowedProp))
                {
                    if (shadowedProp.DeclaringType.IsAssignableFrom(currentProp.DeclaringType))
                    {
                        // replace less-derived with more-derived prop
                        result[currentProp.Name] = currentProp;
                    }
                    // else currentProp is the less-derived one; ignore it
                }
                else
                {
                    result.Add(currentProp.Name, currentProp);
                }
            }

            // Remove private properties
            // Note: this needs to be done after we've walked the entire property list, because
            // a private shadowing property in a derived class should still hide the base property
            if (result.Count == 0)
            {
                return null;
            }
            List<PropertyInfo> filteredResult = new List<PropertyInfo>(result.Count);
            foreach (PropertyInfo property in result.Values)
            {
                if (!IsPrivate(property))
                {
                    filteredResult.Add(property);
                }
            }
            return filteredResult;
        }

        private ICollection<EventInfo> FilterEvents(EventInfo[] eventList, List<XamlMember> knownMembers)
        {
            Dictionary<string, EventInfo> result = new Dictionary<string, EventInfo>(eventList.Length);
            for (int i = 0; i < eventList.Length; i++)
            {
                EventInfo currentEvent = eventList[i];
                XamlMember knownMember;
                if (_nonAttachableMemberCache.TryGetValue(currentEvent.Name, out knownMember))
                {
                    if (knownMember != null)
                    {
                        knownMembers.Add(knownMember);
                    }
                    continue;
                }

                EventInfo shadowedEvent;
                if (result.TryGetValue(currentEvent.Name, out shadowedEvent))
                {
                    if (shadowedEvent.DeclaringType.IsAssignableFrom(currentEvent.DeclaringType))
                    {
                        // replace less-derived with more-derived event
                        result[currentEvent.Name] = currentEvent;
                    }
                    // else currentEvent is the less-derived one; ignore it
                }
                else
                {
                    result.Add(currentEvent.Name, currentEvent);
                }
            }

            // Remove private events
            // Note: this needs to be done after we've walked the entire event list, because
            // a private shadowing event in a derived class should still hide the base event
            if (result.Count == 0)
            {
                return null;
            }
            List<EventInfo> filteredResult = new List<EventInfo>(result.Count);
            foreach (EventInfo evt in result.Values)
            {
                if (!IsPrivate(evt))
                {
                    filteredResult.Add(evt);
                }
            }
            return filteredResult;
        }

        private PropertyInfo GetNonIndexerProperty(string name)
        {
            // Choose the most derived non-index property, in case of shadowing
            PropertyInfo mostDerived = null;
            MemberInfo[] infos = UnderlyingType.GetMember(name, MemberTypes.Property, AllProperties_BF);
            foreach (PropertyInfo pi in infos)
            {
                if (pi.GetIndexParameters().Length == 0)
                {
                    if (mostDerived == null || mostDerived.DeclaringType.IsAssignableFrom(pi.DeclaringType))
                    {
                        mostDerived = pi;
                    }
                }
            }
            return mostDerived;
        }

        private static bool IsPrivate(PropertyInfo pi)
        {
            return IsPrivateOrNull(pi.GetGetMethod(true)) &&
                IsPrivateOrNull(pi.GetSetMethod(true));
        }
        private static bool IsPrivate(EventInfo ei)
        {
            return IsPrivateOrNull(ei.GetAddMethod(true));
        }

        private static bool IsPrivateOrNull(MethodInfo mi)
        {
            return mi == null || mi.IsPrivate;
        }

        #endregion

        #region Attachable member lookup

        private void PickAttachablePropertyAccessors(List<MethodInfo> getters,
            List<MethodInfo> setters, out MethodInfo getter, out MethodInfo setter)
        {
            List<KeyValuePair<MethodInfo, MethodInfo>> candidates = 
                new List<KeyValuePair<MethodInfo, MethodInfo>>();

            if (setters != null && getters != null)
            {
                foreach (MethodInfo curSetter in setters)
                {
                    foreach (MethodInfo curGetter in getters)
                    {
                        ParameterInfo[] getterParams = curGetter.GetParameters();
                        ParameterInfo[] setterParams = curSetter.GetParameters();
                        if (getterParams[0].ParameterType == setterParams[0].ParameterType &&
                            curGetter.ReturnType == setterParams[1].ParameterType)
                        {
                            candidates.Add(new KeyValuePair<MethodInfo, MethodInfo>(curGetter, curSetter));
                        }
                    }
                }
            }

            // There perhaps should be code here to collect all the properties
            // with the given name, and their respective declaring types, and
            // check "IsAssignableFrom" to find the most derived type/property, etc.
            // OR ... we can use the undocumented fact that the most derived
            // type/properties appear to be returned first.
            // This is for cases where there are multiple overloaded get/set pairs 
            // for the same attached property name (or multiple overloaded setters with no getter, or multiple adders for an event).
            if (candidates.Count > 0)
            {
                getter = candidates[0].Key;
                setter = candidates[0].Value;
            }
            else if (setters == null || setters.Count == 0
                || (getters != null && getters.Count > 0 && UnderlyingType.IsVisible && getters[0].IsPublic && !setters[0].IsPublic))
            {
                getter = getters[0];
                setter = null;
            }
            else
            {
                getter = null;
                setter = setters[0];
            }
        }

        private MethodInfo PickAttachableEventAdder(IEnumerable<MethodInfo> adders)
        {
            if (adders != null)
            {
                // See disambiguation note in PickAttachablePropertyAccessors
                foreach (MethodInfo adder in adders)
                {
                    if (!adder.IsPrivate)
                    {
                        return adder;
                    }
                }
            }
            return null;
        }

        internal bool LookupAttachableProperty(string name, out MethodInfo getter, out MethodInfo setter)
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            List<MethodInfo> setters = LookupStaticSetters(name);
            List<MethodInfo> getters = LookupStaticGetters(name);

            if ((setters == null || setters.Count == 0) && (getters == null || getters.Count == 0))
            {
                getter = null;
                setter = null;
                return false;
            }

            PickAttachablePropertyAccessors(getters, setters, out getter, out setter);
            return true;
        }

        internal MethodInfo LookupAttachableEvent(string name)
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            List<MethodInfo> adders = LookupStaticAdders(name);
            if (adders == null || adders.Count == 0)
            {
                return null;
            }
            return PickAttachableEventAdder(adders);
        }

        private void LookupAllStaticAccessors(out Dictionary<string, List<MethodInfo>> getters, 
            out Dictionary<string, List<MethodInfo>> setters, out Dictionary<string, List<MethodInfo>> adders)
        {
            getters = new Dictionary<string,List<MethodInfo>>();
            setters = new Dictionary<string,List<MethodInfo>>();
            adders = new Dictionary<string,List<MethodInfo>>();

            MethodInfo[] allMethods = UnderlyingType.GetMethods(AttachableProperties_BF);

            if (UnderlyingType.IsVisible)
            {
                LookupAllStaticAccessorsHelper(allMethods, getters, setters, adders, true);
            }
            else
            {
                LookupAllStaticAccessorsHelper(allMethods, getters, setters, adders, false);
            }
        }

        private void LookupAllStaticAccessorsHelper(MethodInfo[] allMethods, Dictionary<string,List<MethodInfo>> getters,
            Dictionary<string, List<MethodInfo>> setters, Dictionary<string, List<MethodInfo>> adders, bool isUnderlyingTypePublic)
        {
            foreach (MethodInfo method in allMethods)
            {
                if (!method.IsPrivate)
                {
                    string name;
                    if (IsAttachablePropertyGetter(method, out name))
                    {
                        AddToMultiDict(getters, name, method, isUnderlyingTypePublic);
                    }
                    else if (IsAttachablePropertySetter(method, out name))
                    {
                        AddToMultiDict(setters, name, method, isUnderlyingTypePublic);
                    }
                    else if (IsAttachableEventAdder(method, out name))
                    {
                        AddToMultiDict(adders, name, method, isUnderlyingTypePublic);
                    }
                }
            }
        }

        private List<MethodInfo> LookupStaticAdders(string name)
        {
            string adderName = KnownStrings.Add + name + KnownStrings.Handler;
            MemberInfo[] adders = UnderlyingType.GetMember(adderName, MemberTypes.Method, AttachableProperties_BF);
            List<MethodInfo> preferredAdders, otherAdders;
            PrioritizeAccessors(adders, true /*isEvent*/, false /*isGetter*/, out preferredAdders, out otherAdders);
            return preferredAdders ?? otherAdders;
        }

        private List<MethodInfo> LookupStaticGetters(string name)
        {
            MemberInfo[] getters = UnderlyingType.GetMember(KnownStrings.Get + name, MemberTypes.Method, AttachableProperties_BF);
            List<MethodInfo> preferredGetters, otherGetters;
            PrioritizeAccessors(getters, false /*isEvent*/, true /*isGetter*/, out preferredGetters, out otherGetters);
            return preferredGetters ?? otherGetters;
        }

        private List<MethodInfo> LookupStaticSetters(string name)
        {
            MemberInfo[] setters = UnderlyingType.GetMember(KnownStrings.Set + name, MemberTypes.Method, AttachableProperties_BF);
            List<MethodInfo> preferredSetters, otherSetters;
            PrioritizeAccessors(setters, false /*isEvent*/, false /*isGetter*/, out preferredSetters, out otherSetters);
            return preferredSetters ?? otherSetters;
        }

        // this method prioritizes attachable property getters/setters when the underlying type is public
        // To conform with 3.0 behavior, public getter/setter is preferred even when it does not have a matching setter/getter
        private void PrioritizeAccessors(MemberInfo[] accessors, bool isEvent, bool isGetter, out List<MethodInfo> preferredAccessors, out List<MethodInfo> otherAccessors)
        {
            preferredAccessors = null;
            otherAccessors = null;

            if (UnderlyingType.IsVisible)
            {
                foreach (MethodInfo accessor in accessors)
                {
                    if (accessor.IsPublic && IsAttachablePropertyAccessor(isEvent, isGetter, accessor))
                    {
                        if (preferredAccessors == null)
                        {
                            preferredAccessors = new List<MethodInfo>();
                        }
                        preferredAccessors.Add(accessor);
                    }
                    else if (!accessor.IsPrivate && IsAttachablePropertyAccessor(isEvent, isGetter, accessor))
                    {
                        if (otherAccessors == null)
                        {
                            otherAccessors = new List<MethodInfo>();
                        }
                        otherAccessors.Add(accessor);
                    }
                }
            }
            else
            {
                foreach (MethodInfo accessor in accessors)
                {
                    if (!accessor.IsPrivate && IsAttachablePropertyAccessor(isEvent, isGetter, accessor))
                    {
                        if (preferredAccessors == null)
                        {
                            preferredAccessors = new List<MethodInfo>();
                        }
                        preferredAccessors.Add(accessor);
                    }   
                }
            }
        }

        private bool IsAttachablePropertyAccessor(bool isEvent, bool isGetter, MethodInfo accessor)
        {
            if (isEvent)
            {
                return IsAttachableEventAdder(accessor);
            }
            
            if (isGetter)
            {
                return IsAttachablePropertyGetter(accessor);
            }

            return IsAttachablePropertySetter(accessor);
        }

        private static void AddToMultiDict(Dictionary<string, List<MethodInfo>> dict, string name, MethodInfo value, bool isUnderlyingTypePublic)
        {
            List<MethodInfo> list;
            if (dict.TryGetValue(name, out list))
            {
                // To conform with how 3.0 behaved,
                // if the underlying type is public, public accessors are preferred over non-publics;
                // if the underlying type is non-public, we have no preferences
                if (isUnderlyingTypePublic)
                {
                    if (value.IsPublic)
                    {
                        if (!list[0].IsPublic)
                        {
                            // if the list contains non-public accessor,
                            // get rid of all of them, because we found a public accessor
                            // which we are adding to the list
                            list.Clear();
                        }
                        list.Add(value);
                    }
                    else
                    {
                        if (!list[0].IsPublic)
                        {
                            list.Add(value);
                        }
                    }
                }
                else
                {
                    list.Add(value);
                }
            }
            else
            {
                list = new List<MethodInfo>();
                dict.Add(name, list);
                list.Add(value);
            }
        }

        private bool IsAttachablePropertyGetter(MethodInfo mi, out string name)
        {
            name = null;
            if (!KS.StartsWith(mi.Name, KnownStrings.Get))
            {
                return false;
            }
            if (!IsAttachablePropertyGetter(mi))
            {
                return false;
            }
            name = mi.Name.Substring(KnownStrings.Get.Length);
            return true;
        }

        private bool IsAttachablePropertyGetter(MethodInfo mi)
        {
            // Static Getter has one argument and does not return void
            ParameterInfo[] pmi = mi.GetParameters();
            return (pmi.Length == 1) && (mi.ReturnType != typeof(void));
        }

        private bool IsAttachablePropertySetter(MethodInfo mi, out string name)
        {
            name = null;
            if (!KS.StartsWith(mi.Name, KnownStrings.Set))
            {
                return false;
            }
            if (!IsAttachablePropertySetter(mi))
            {
                return false;
            }
            name = mi.Name.Substring(KnownStrings.Set.Length);
            return true;
        }

        private bool IsAttachablePropertySetter(MethodInfo mi)
        {
            // Static Setter has two arguments 
            ParameterInfo[] pmi = mi.GetParameters();
            return (pmi.Length == 2);
        }

        private bool IsAttachableEventAdder(MethodInfo mi, out string name)
        {
            name = null;
            if (!KS.StartsWith(mi.Name, KnownStrings.Add) || !KS.EndsWith(mi.Name, KnownStrings.Handler))
            {
                return false;
            }
            if (!IsAttachableEventAdder(mi))
            {
                return false;
            }
            name = mi.Name.Substring(KnownStrings.Add.Length, 
                mi.Name.Length - KnownStrings.Add.Length - KnownStrings.Handler.Length);
            return true;
        }

        private bool IsAttachableEventAdder(MethodInfo mi)
        {
            // Static Adder has two arguments, and second is a delegate
            ParameterInfo[] pmi = mi.GetParameters();
            return (pmi.Length == 2) && typeof(Delegate).IsAssignableFrom(pmi[1].ParameterType);
        }

        // Unlike other TypeReflector methods, this one interacts direclty with SchemaContext.
        // That is the cleanest way to pass back the information we need without JITting or boxing.
        internal IList<XamlMember> LookupAllAttachableMembers(XamlSchemaContext schemaContext)
        {
            Debug.Assert(UnderlyingType != null, "Caller should check for UnderlyingType == null");
            Debug.Assert(_attachableMemberCache != null, "AttachableMembers property should have been invoked before this");

            List<XamlMember> result = new List<XamlMember>();

            Dictionary<string, List<MethodInfo>> getters;
            Dictionary<string, List<MethodInfo>> setters;
            Dictionary<string, List<MethodInfo>> adders;
            LookupAllStaticAccessors(out getters, out setters, out adders);

            GetOrCreateAttachableProperties(schemaContext, result, getters, setters);
            GetOrCreateAttachableEvents(schemaContext, result, adders);
            return result;
        }

        private void GetOrCreateAttachableProperties(XamlSchemaContext schemaContext, List<XamlMember> result, 
            Dictionary<string, List<MethodInfo>> getters, Dictionary<string, List<MethodInfo>> setters)
        {
            foreach (KeyValuePair<string, List<MethodInfo>> nameAndSetterList in setters)
            {
                string name = nameAndSetterList.Key;
                XamlMember member = null;
                if (!_attachableMemberCache.TryGetValue(name, out member))
                {
                    List<MethodInfo> getterList;
                    getters.TryGetValue(name, out getterList);
                    
                    // removing the current entry from getters dictionary because it is not needed anymore
                    getters.Remove(name);
                    MethodInfo getter, setter;
                    PickAttachablePropertyAccessors(getterList, nameAndSetterList.Value, out getter, out setter);
                    member = schemaContext.GetAttachableProperty(name, getter, setter);
                    // Filter out read-only properties except for dictionaries and collections
                    if (member.IsReadOnly && !member.Type.IsUsableAsReadOnly)
                    {
                        member = null;
                    }
                }
                if (member != null)
                {
                    result.Add(member);
                }
            }

            foreach (KeyValuePair<string, List<MethodInfo>> nameAndGetterList in getters)
            {
                string name = nameAndGetterList.Key;
                XamlMember member = null;
                if (!_attachableMemberCache.TryGetValue(name, out member))
                {
                    member = schemaContext.GetAttachableProperty(name, nameAndGetterList.Value[0], null);
                }
                result.Add(member);
            }
        }

        private void GetOrCreateAttachableEvents(XamlSchemaContext schemaContext, 
            List<XamlMember> result, Dictionary<string, List<MethodInfo>> adders)
        {
            foreach (KeyValuePair<string, List<MethodInfo>> nameAndAdderList in adders)
            {
                string name = nameAndAdderList.Key;
                XamlMember member = null;
                if (!_attachableMemberCache.TryGetValue(name, out member))
                {
                    MethodInfo adder = PickAttachableEventAdder(nameAndAdderList.Value);
                    member = schemaContext.GetAttachableEvent(name, adder);
                }
                if (member != null)
                {
                    result.Add(member);
                }
            }
        }

        #endregion

        #region Flag Management

        internal bool? GetFlag(BoolTypeBits typeBit)
        {
            return GetFlag(_boolTypeBits, (int)typeBit);
        }

        internal void SetFlag(BoolTypeBits typeBit, bool value)
        {
            SetFlag(ref _boolTypeBits, (int)typeBit, value);
        }

        #endregion

        // Used by Reflector for attribute lookups
        protected override MemberInfo Member
        {
            get { return UnderlyingType; }
        }

        private static object GetCustomAttribute(Type attrType, Type reflectedType)
        {
            object[] objs = reflectedType.GetCustomAttributes(attrType, true);
            if (objs.Length == 0)
            {
                return null;
            }
            if (objs.Length > 1)
            {
                string message = SR.Get(SRID.TooManyAttributesOnType,
                                                    reflectedType.Name, attrType.Name);
                throw new XamlSchemaException(message);
            }
            return objs[0];
        }

        private static TypeVisibility GetVisibility(Type type)
        {
            bool nestedTypeIsInternal = false;
            // If the type is nested, we need to check its entire declaring hierarchy
            while (type.IsNested)
            {
                if (type.IsNestedAssembly || type.IsNestedFamORAssem)
                {
                    nestedTypeIsInternal = true;
                }
                else if (!type.IsNestedPublic)
                {
                    // Not public or internal
                    return TypeVisibility.NotVisible;
                }
                type = type.DeclaringType;
            }
            bool outerTypeIsInternal = type.IsNotPublic;
            return (outerTypeIsInternal || nestedTypeIsInternal) ? TypeVisibility.Internal : TypeVisibility.Public;
        }

        private enum TypeVisibility
        {
            NotVisible,
            Internal,
            Public
        }

        internal class ThreadSafeDictionary<K,V> : Dictionary<K, V> where V : class
        {
            bool _isComplete;

            internal ThreadSafeDictionary()
            {
            }

            public bool IsComplete
            {
                get { return _isComplete; }
                set
                {
                    Debug.Assert(value == true);
                    // This instance is always held in a private field, safe to lock on
                    lock (this)
                    {
                        SetComplete();
                    }
                }
            }

            public new bool TryGetValue(K name, out V member)
            {
                // This instance is always held in a private field, safe to lock on
                lock (this)
                {
                    return base.TryGetValue(name, out member);
                }
            }

            public new V TryAdd(K name, V member)
            {
                // This instance is always held in a private field, safe to lock on
                lock (this)
                {
                    V result;
                    if (!base.TryGetValue(name, out result))
                    {
                        if (!IsComplete)
                        {
                            Add(name, member);
                        }
                        result = member;
                    }
                    return result;
                }
            }

            private void SetComplete()
            {
                // Delete any saved null so that they don't get returned to the user.
                // They're superfluous now that we know the list is complete.
                List<K> listOfNulls = null;
                foreach (KeyValuePair<K, V> pair in this)
                {
                    if (pair.Value == null)
                    {
                        if (listOfNulls == null)
                        {
                            listOfNulls = new List<K>();
                        }
                        listOfNulls.Add(pair.Key);
                    }
                }
                if (listOfNulls != null)
                {
                    for (int i = 0; i < listOfNulls.Count; i++)
                    {
                        Remove(listOfNulls[i]);
                    }
                }
                _isComplete = true;
            }
        }
   }
}
