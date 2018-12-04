// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

#if !TARGETTING35SP1
using System.Collections.Concurrent;
#endif

using System.Xaml;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;
using System.Collections.ObjectModel;
using System.Security;
using MS.Internal.Xaml.Parser;

namespace System.Xaml
{
    /// <SecurityNote>
    /// SchemaContexts can potentially be shared between multiple callers in an AppDomain, including
    /// both full and partial trust callers. To be safe for sharing, the default implementation should
    /// be idempotent and order-independent--i.e. functionally immutable.
    ///
    /// Technically, we don't guarantee these properties (they're not enforced by the runtime), but
    /// we should never knowingly break them.
    ///
    /// This means two things:
    /// 1. No public mutability.
    ///    Derived classes can potentially be mutable, but the base implementation should not be.
    /// 2. No externally observable side effects from lookups.
    ///    This means that all cached data should be generally applicable; it can't depend on any
    ///    input other than the ctor arguments and AppDomain state.
    ///    (For a subtle example of this, see the security note in Initialize().)
    /// These principles apply to all classes in the schema hierarchy (XamlType, XamlMember, etc).
    /// </SecurityNote>
    /// <remarks>
    /// This class, and the closure of its references (i.e. XamlType, XamlMember, etc) are all
    /// thread-safe in their base implementations. Derived implementations can choose whether or not
    /// to be thread-safe.
    /// </remarks>
    public class XamlSchemaContext
    {
        #region Common Fields

        // We don't expect a lot of contention on our dictionaries, so avoid the overhead of
        // extra lock partitioning
        const int ConcurrencyLevel = 1;
        const int DictionaryCapacity = 17;

        // Immutable, initialized in constructor
        private readonly ReadOnlyCollection<Assembly> _referenceAssemblies;

        // take this lock when iterating new assemblies in the AppDomain/RefAssm
        object _syncExaminingAssemblies;               
        
        #endregion

        #region Constructors

        public XamlSchemaContext()
            : this(null, null) { }

        public XamlSchemaContext(XamlSchemaContextSettings settings)
            : this(null, settings) { }

        public XamlSchemaContext(IEnumerable<Assembly> referenceAssemblies)
            : this(referenceAssemblies, null) { }

        public XamlSchemaContext(IEnumerable<Assembly> referenceAssemblies, XamlSchemaContextSettings settings)
        {
            if (referenceAssemblies != null)
            {
                // ReadOnlyCollection wants an IList but we have an IEnumerable.
                List<Assembly> listOfAssemblies = new List<Assembly>(referenceAssemblies);
                _referenceAssemblies = new ReadOnlyCollection<Assembly>(listOfAssemblies);
            }
            _settings = (settings != null)
                ? new XamlSchemaContextSettings(settings)
                : new XamlSchemaContextSettings();
            _syncExaminingAssemblies = new Object();
            InitializeAssemblyLoadHook();
        }

        // Deliberate implementation of Finalize without Dispose:
        // We don't actually hold any unmanaged resources, the purpose of this finalizer is to unhook
        // the AppDomain.AssemblyLoad event, and thus allow the AssemblyLoadHandler to be GCed.
        // We don't implement IDisposable, because if we did, users would probably try to dispose us
        // right after the XAML load completes, which is wrong if there were any XamlDeferLoad properties.
        ~XamlSchemaContext()
        {
            try
            {
                if (_assemblyLoadHandler != null && !Environment.HasShutdownStarted)
                {
                    _assemblyLoadHandler.Unhook();
                }
            }
            catch
            {
                // Finalizers shouldn't throw
            }
        }

        #endregion

        #region Enumeration methods

        // This list is valid when it is non-null. If it gets invalidated (e.g. by the addition of
        // new namespaces), it is reset to null.
        private IList<string> _nonClrNamespaces;

        public virtual ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
        {
            UpdateXmlNsInfo();
            XamlNamespace ns = GetXamlNamespace(xamlNamespace);
            return ns.GetAllXamlTypes();
        }

        public virtual IEnumerable<string> GetAllXamlNamespaces()
        {
            UpdateXmlNsInfo();
            IList<string> result = _nonClrNamespaces;
            if (result == null)
            {
                // To avoid a race condition when assigning this list, don't allow any additional
                // namespaces to be added while we're iterating the current list
                lock (_syncExaminingAssemblies)
                {
                    result = new List<string>();
                    foreach (KeyValuePair<string, XamlNamespace> ns in NamespaceByUriList)
                    {
                        if (ns.Value.IsResolved && !ns.Value.IsClrNamespace)
                        {
                            result.Add(ns.Key);
                        }
                    }
                    result = new ReadOnlyCollection<string>(result);
                    this._nonClrNamespaces = result;
                }
            }
            return result;
        }

        #endregion

        #region Prefix lookup

        // This dictionary defaults to null. The first time it is used, it needs to be initialized
        // in full. After that, it will be updated as needed by UpdateXmlNsInfo.
        private ConcurrentDictionary<string, string> _preferredPrefixes;

        public virtual string GetPreferredPrefix(string xmlns)
        {
            if (xmlns == null)
            {
                throw new ArgumentNullException("xmlns");
            }
            UpdateXmlNsInfo();
            if (_preferredPrefixes == null)
            {
                InitializePreferredPrefixes();
            }
            string result;
            if (!this._preferredPrefixes.TryGetValue(xmlns, out result))
            {
                if (XamlLanguage.XamlNamespaces.Contains(xmlns))
                {
                    result = XamlLanguage.PreferredPrefix;
                }
                else
                {
                    string clrNs, assemblyName;
                    if (ClrNamespaceUriParser.TryParseUri(xmlns, out clrNs, out assemblyName))
                    {
                        result = GetPrefixForClrNs(clrNs, assemblyName);
                    }
                    else
                    {
                        result = KnownStrings.DefaultPrefix;
                    }
                }
                result = TryAdd(_preferredPrefixes, xmlns, result);
            }
            return result;
        }

        string GetPrefixForClrNs(string clrNs, string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return KnownStrings.LocalPrefix;
            }
            var sb = new StringBuilder();
            foreach (string segment in clrNs.Split('.'))
            {
                if (!string.IsNullOrEmpty(segment))
                {
                    sb.Append(Char.ToLower(segment[0], TypeConverterHelper.InvariantEnglishUS));
                }
            }
            if (sb.Length > 0)
            {
                string result = sb.ToString();
                
                if (KS.Eq(result, XamlLanguage.PreferredPrefix))
                {
                    // Prevent this algorithm from stealing 'x'
                    return KnownStrings.DefaultPrefix;
                }
                else if (KS.Eq(result, KnownStrings.XmlPrefix))
                {
                    // Prevent this algorithm from stealing 'xml'
                    return KnownStrings.DefaultPrefix;
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return KnownStrings.LocalPrefix;
            }
        }

        void InitializePreferredPrefixes()
        {
            // To avoid an assignment race condition, prevent new assemblies from being processed while we're
            // iterating the existing list
            lock (_syncExaminingAssemblies)
            {
                ConcurrentDictionary<string, string> preferredPrefixes = CreateDictionary<string, string>();
                foreach (XmlNsInfo nsInfo in EnumerateXmlnsInfos())
                {
                    UpdatePreferredPrefixes(nsInfo, preferredPrefixes);
                }
                _preferredPrefixes = preferredPrefixes;
            }
        }

        void UpdatePreferredPrefixes(XmlNsInfo newNamespaces, ConcurrentDictionary<string, string> prefixDict)
        {
            foreach (KeyValuePair<string, string> nsToPrefix in newNamespaces.Prefixes)
            {
                string existingPrefix;
                string preferredPrefix = nsToPrefix.Value;
                if (!prefixDict.TryGetValue(nsToPrefix.Key, out existingPrefix))
                {
                    existingPrefix = TryAdd(prefixDict, nsToPrefix.Key, preferredPrefix);
                }
                while (existingPrefix != preferredPrefix)
                {
                    preferredPrefix = XmlNsInfo.GetPreferredPrefix(existingPrefix, preferredPrefix);
                    if (!KS.Eq(preferredPrefix, existingPrefix))
                    {
                        existingPrefix = TryUpdate(prefixDict, nsToPrefix.Key, preferredPrefix, existingPrefix);
                    }
                }
            }
        }

        #endregion

        #region Name -> Type/Directive mapping

        public virtual XamlDirective GetXamlDirective(string xamlNamespace, string name)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (XamlLanguage.XamlNamespaces.Contains(xamlNamespace))
            {
                return XamlLanguage.LookupXamlDirective(name);
            }
            else if (XamlLanguage.XmlNamespaces.Contains(xamlNamespace))
            {
                return XamlLanguage.LookupXmlDirective(name);
            }
            return null;
        }

        public XamlType GetXamlType(XamlTypeName xamlTypeName)
        {
            if (xamlTypeName == null)
            {
                throw new ArgumentNullException("xamlTypeName");
            }
            if (xamlTypeName.Name == null)
            {
                throw new ArgumentException(SR.Get(SRID.ReferenceIsNull, "xamlTypeName.Name"), "xamlTypeName");
            }
            if (xamlTypeName.Namespace == null)
            {
                throw new ArgumentException(SR.Get(SRID.ReferenceIsNull, "xamlTypeName.Namespace"), "xamlTypeName");
            }

            XamlType[] typeArgs = null;
            if (xamlTypeName.HasTypeArgs)
            {
                typeArgs = new XamlType[xamlTypeName.TypeArguments.Count];
                for (int i = 0; i < xamlTypeName.TypeArguments.Count; i++)
                {
                    if (xamlTypeName.TypeArguments[i] == null)
                    {
                        throw new ArgumentException(SR.Get(SRID.CollectionCannotContainNulls, "xamlTypeName.TypeArguments"));
                    }
                    typeArgs[i] = GetXamlType(xamlTypeName.TypeArguments[i]);
                    if (typeArgs[i] == null)
                    {
                        return null;
                    }
                }
            }
            return GetXamlType(xamlTypeName.Namespace, xamlTypeName.Name, typeArgs);
        }

        protected internal virtual XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (typeArguments != null)
            {
                foreach (XamlType typeArg in typeArguments)
                {
                    if (typeArg == null)
                    {
                        throw new ArgumentException(SR.Get(SRID.CollectionCannotContainNulls, "typeArguments"));
                    }
                    if (typeArg.UnderlyingType == null)
                    {
                        return null;
                    }
                }
            }

            XamlType result = null;
            if (typeArguments == null || typeArguments.Length == 0)
            {
                result = XamlLanguage.LookupXamlType(xamlNamespace, name);
                if (result != null)
                {
                    if (FullyQualifyAssemblyNamesInClrNamespaces)
                    {
                        // We can't return types from XamlLanguage directly because their SchemaContext
                        // has FullyQualifyAssemblyNamesInClrNamespaces set to false
                        result = GetXamlType(result.UnderlyingType);
                    }
                    return result;
                }
            }

            XamlNamespace ns = GetXamlNamespace(xamlNamespace);
            int revision = ns.RevisionNumber;
            result = ns.GetXamlType(name, typeArguments);
            if (result == null && !ns.IsClrNamespace)
            {
                UpdateXmlNsInfo();
                if (ns.RevisionNumber > revision)
                {
                    result = ns.GetXamlType(name, typeArguments);
                }
            }

            return result;
        }

        #endregion

        #region Namespace Compatibility Lookup

        // Laziy init, always access through property
        private ConcurrentDictionary<string, string> _xmlNsCompatDict;

        // Thread-safe cache - always use TryAdd or TryUpdate to write 
        private ConcurrentDictionary<string, string> XmlNsCompatDict
        {
            get
            {
                if (_xmlNsCompatDict == null)
                    Interlocked.CompareExchange(ref _xmlNsCompatDict, CreateDictionary<string, string>(), null);
                return _xmlNsCompatDict;
            }
        }

        // Note: this method doesn't apply transitive subsuming, the caller is responsible for doing that.
        public virtual bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }

            // Note: this method has order-dependent behavior for backcompat.
            // When we look up a namespace, we throw if it has conflicting XmlnsCompatAttributes.
            // However, once we've looked up a namespace, we cache the result, and if an assembly with
            // conflicting attributes is loaded later, we just ignore it.

            // First try to load from our cache
            if (XmlNsCompatDict.TryGetValue(xamlNamespace, out compatibleNamespace))
            {
                return true;
            }
            
            // Then look for XmlnsCompatAttributes
            UpdateXmlNsInfo();
            compatibleNamespace = GetCompatibleNamespace(xamlNamespace);
            
            // Fall back to just using the requested namespace;
            if (compatibleNamespace == null)
            {
                compatibleNamespace = xamlNamespace;
            }

            // Make sure that the namespace actually exists
            XamlNamespace ns = GetXamlNamespace(compatibleNamespace);
            if (ns.IsResolved)
            {
                compatibleNamespace = TryAdd(XmlNsCompatDict, xamlNamespace, compatibleNamespace);
                return true;
            }
            else
            {
                // We don't cache negative results, since a lookup might succeed later after a new assembly
                // is loaded. XmlCompatibilityReader already caches all results (positive and negative) within
                // the scope of a given document.
                compatibleNamespace = null;
                return false;
            }
        }

        // Find a subsuming namespace within the current processed assemblies
        private string GetCompatibleNamespace(string oldNs)
        {
            string result = null;
            Assembly resultAssembly = null;
            // locking to prevent new loaded assemblies being processed while we iterate list
            lock (_syncExaminingAssemblies)
            {
                foreach (XmlNsInfo nsInfo in EnumerateXmlnsInfos())
                {
                    Assembly curAssembly = nsInfo.Assembly;
                    if (curAssembly == null)
                    {
                        continue;
                    }
                    IDictionary<string, string> oldToNewNs = null;

                    // When trawling the entire AppDomain, suppress exceptions from assemblies with bad attributes
                    if (ReferenceAssemblies == null)
                    {
                        try
                        {
                            oldToNewNs = nsInfo.OldToNewNs;
                        }
                        catch (Exception ex)
                        {
                            if (CriticalExceptions.IsCriticalException(ex))
                            {
                                throw;
                            }
                            // just skip to the next assembly
                            continue;
                        }
                    }
                    else
                    {
                        oldToNewNs = nsInfo.OldToNewNs;
                    }

                    string newNs;
                    if (oldToNewNs.TryGetValue(oldNs, out newNs))
                    {
                        if (result != null && result != newNs)
                        {
                            throw new XamlSchemaException(SR.Get(SRID.DuplicateXmlnsCompatAcrossAssemblies,
                                resultAssembly.FullName, curAssembly.FullName, oldNs));
                        }
                        result = newNs;
                        resultAssembly = curAssembly;
                    }
                }
            }
            return result;
        }

        #endregion

        #region Type and member cache

        // Lazy init, access these fields through the properties
        private ConcurrentDictionary<Type, XamlType> _masterTypeList;
        private ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object> _masterValueConverterList;
        private ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember> _masterMemberList;
        private ConcurrentDictionary<XamlType, Dictionary<string,SpecialBracketCharacters> > _masterBracketCharacterCache;

        // Security note: all of these ConcurrentDictionaries use Reference Equality to prevent spoofing of 
        // RuntimeTypes/Members by other custom derived descendants of System.Type/MemberInfo.
        // E.g. if a user called GetXamlType(Type) and passed in a custom descendant of System.Type
        // that reported itself as Equal to some real type, then when we went to look up the real
        // type, we would get the spoofed one instead. Using reference equality avoids that.

        // Thread-safe cache - always use TryAdd or TryUpdate to write 
        private ConcurrentDictionary<XamlType, Dictionary<string, SpecialBracketCharacters> > MasterBracketCharacterCache
        {
            get
            {
                if (_masterBracketCharacterCache == null)
                    Interlocked.CompareExchange(ref _masterBracketCharacterCache, CreateDictionary<XamlType, Dictionary<string, SpecialBracketCharacters>>(), null);
                return _masterBracketCharacterCache;
            }
        }

        // Thread-safe cache - always use TryAdd or TryUpdate to write 
        private ConcurrentDictionary<Type, XamlType> MasterTypeList
        {
            get
            {
                if (_masterTypeList == null)
                    Interlocked.CompareExchange(ref _masterTypeList, CreateDictionary<Type, XamlType>(ReferenceEqualityComparer<Type>.Singleton), null);
                return _masterTypeList;
            }
        }

        // Thread-safe cache - always use TryAdd or TryUpdate to write 
        private ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object> MasterValueConverterList
        {
            get
            {
                if (_masterValueConverterList == null)
                    Interlocked.CompareExchange(ref _masterValueConverterList, CreateDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object>(), null);
                return _masterValueConverterList;
            }
        }

        // Thread-safe cache - always use TryAdd or TryUpdate to write 
        private ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember> MasterMemberList
        {
            get
            {
                if (_masterMemberList == null)
                    Interlocked.CompareExchange(ref _masterMemberList, CreateDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(), null);
                return _masterMemberList;
            }
        }

        public virtual XamlType GetXamlType(Type type)
        {
            return GetXamlType(type, XamlLanguage.TypeAlias(type));
        }

        internal XamlType GetXamlType(Type type, string alias)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            XamlType xamlType = null;
            if (!MasterTypeList.TryGetValue(type, out xamlType))
            {
                xamlType = new XamlType(alias, type, this, null, null);
                xamlType = TryAdd(MasterTypeList, type, xamlType);
            }
            return xamlType;
        }

        /// <summary>
        /// Constructs a cache of all the members in this particular type that have 
        /// MarkupExtensionBracketCharactersAttribute set on them. This cache is added to a master
        /// cache which stores the BracketCharacter cache for each type.
        /// </summary>
        internal Dictionary<string, SpecialBracketCharacters> InitBracketCharacterCacheForType(XamlType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Dictionary<string, SpecialBracketCharacters> bracketCharacterCache = null;
            if (type.IsMarkupExtension)
            {
                if (!MasterBracketCharacterCache.TryGetValue(type, out bracketCharacterCache))
                {
                    bracketCharacterCache = BuildBracketCharacterCacheForType(type);
                    bracketCharacterCache = TryAdd(MasterBracketCharacterCache, type, bracketCharacterCache);
                }
            }
            
            return bracketCharacterCache;
        }

        /// <summary>
        /// Looks up all properties via reflection on the given type, and scans through the attributes on all of them
        /// to build a cache of properties which have MarkupExtensionBracketCharactersAttribute set on them.
        /// </summary>
        private Dictionary<string, SpecialBracketCharacters> BuildBracketCharacterCacheForType(XamlType type)
        {
            Dictionary<string, SpecialBracketCharacters> map = new Dictionary<string, SpecialBracketCharacters>(StringComparer.OrdinalIgnoreCase);
            ICollection<XamlMember> members = type.GetAllMembers();
            foreach (XamlMember member in members)
            {
                string constructorArgumentName = member.ConstructorArgument;
                string propertyName = member.Name;
                IReadOnlyDictionary<char,char> markupExtensionBracketCharactersList = member.MarkupExtensionBracketCharacters;
                SpecialBracketCharacters splBracketCharacters = markupExtensionBracketCharactersList != null && markupExtensionBracketCharactersList.Count > 0
                    ? new SpecialBracketCharacters(markupExtensionBracketCharactersList)
                    : null;
                if (splBracketCharacters != null)
                {
                    splBracketCharacters.EndInit();
                    map.Add(propertyName, splBracketCharacters);
                    if (!string.IsNullOrEmpty(constructorArgumentName))
                    {
                        map.Add(constructorArgumentName, splBracketCharacters);
                    }
                }
            }

            return map.Count > 0 ? map : null;
        } 

        protected internal XamlValueConverter<TConverterBase> GetValueConverter<TConverterBase>(
            Type converterType, XamlType targetType)
            where TConverterBase : class
        {
            var key = new ReferenceEqualityTuple<Type, XamlType, Type>(converterType, targetType, typeof(TConverterBase));
            object result;
            if (!MasterValueConverterList.TryGetValue(key, out result))
            {
                result = new XamlValueConverter<TConverterBase>(converterType, targetType);
                result = TryAdd(MasterValueConverterList, key, result);
            }
            return (XamlValueConverter<TConverterBase>)result;
        }

        internal virtual XamlMember GetProperty(PropertyInfo pi)
        {
            var xpik = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(pi, null);
            XamlMember member;
            if (!MasterMemberList.TryGetValue(xpik, out member))
            {
                member = new XamlMember(pi, this);
                member = TryAdd(MasterMemberList, xpik, member);
            }
            return member;
        }

        internal virtual XamlMember GetEvent(EventInfo ei)
        {
            var xpik = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(ei, null);
            XamlMember member;
            if (!MasterMemberList.TryGetValue(xpik, out member))
            {
                member = new XamlMember(ei, this);
                member = TryAdd(MasterMemberList, xpik, member);
            }
            return member;
        }

        // Caller responsible for ensuring getter and setter not null
        internal virtual XamlMember GetAttachableProperty(string name, MethodInfo getter, MethodInfo setter)
        {
            XamlMember property;
            var xpik = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(getter, setter);
            if (!MasterMemberList.TryGetValue(xpik, out property))
            {
                property = new XamlMember(name, getter, setter, this);
                property = TryAdd(MasterMemberList, xpik, property);
            }
            return property;
        }

        internal virtual XamlMember GetAttachableEvent(string name, MethodInfo adder)
        {
            XamlMember property;
            var xpik = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(adder, null);
            if (!MasterMemberList.TryGetValue(xpik, out property))
            {
                property = new XamlMember(name, adder, this);
                property = TryAdd(MasterMemberList, xpik, property);
            }
            return property;
        }

        #endregion

        #region Settings

        // Unchanging, initialized in ctor
        private readonly XamlSchemaContextSettings _settings = null;

        public bool SupportMarkupExtensionsWithDuplicateArity
        {
            get { return _settings.SupportMarkupExtensionsWithDuplicateArity; }
        }

        public bool FullyQualifyAssemblyNamesInClrNamespaces
        {
            get { return _settings.FullyQualifyAssemblyNamesInClrNamespaces; }
        }

        public IList<Assembly> ReferenceAssemblies
        {
            get { return _referenceAssemblies; }
        }

        #endregion

        #region Namespace Mapping and Assembly Attribute (XmlNsInfo) caches

        // Lazy init, access these fields through the properties.
        private ConcurrentDictionary<String, XamlNamespace> _namespaceByUriList;
        private ConcurrentDictionary<Assembly, XmlNsInfo> _xmlnsInfo;
        private ConcurrentDictionary<WeakRefKey, XmlNsInfo> _xmlnsInfoForDynamicAssemblies;
        private ConcurrentDictionary<Assembly, XmlNsInfo> _xmlnsInfoForUnreferencedAssemblies;

        // immutable, initialized in ctor
        AssemblyLoadHandler _assemblyLoadHandler;

        // tracks new assemblies seen by the AssemblyLoad handler, but not yet reflected
        private IList<Assembly> _unexaminedAssemblies;
        bool _isGCCallbackPending;

        // take this lock when modifying _unexaminedAssemblies or _isGCCallbackPending
        // Acquisition order: If also taking _syncExaminingAssemblies, take it first
        object _syncAccessingUnexaminedAssemblies;     

        // This dictionary is also thread-safe for single reads and writes, but if you're
        // iterating them, lock on _syncExaminingAssemblies to ensure consistent results
        private ConcurrentDictionary<Assembly, XmlNsInfo> XmlnsInfo
        {
            get
            {
                if (_xmlnsInfo == null)
                    Interlocked.CompareExchange(ref _xmlnsInfo, CreateDictionary<Assembly, XmlNsInfo>(ReferenceEqualityComparer<Assembly>.Singleton), null);
                return _xmlnsInfo;
            }
        }

        // Same thread-safety as XmlnsInfo
        private ConcurrentDictionary<WeakRefKey, XmlNsInfo> XmlnsInfoForDynamicAssemblies
        {
            get
            {
                if (_xmlnsInfoForDynamicAssemblies == null)
                    Interlocked.CompareExchange(ref _xmlnsInfoForDynamicAssemblies, CreateDictionary<WeakRefKey, XmlNsInfo>(), null);
                return _xmlnsInfoForDynamicAssemblies;
            }
        }

        // This dictionary is also thread-safe for single reads and writes, but if you're
        // iterating them, lock on _syncExaminingAssemblies to ensure consistent results
        private ConcurrentDictionary<String, XamlNamespace> NamespaceByUriList
        {
            get
            {
                if (_namespaceByUriList == null)
                    Interlocked.CompareExchange(ref _namespaceByUriList,  CreateDictionary<string, XamlNamespace>(), null);
                return _namespaceByUriList;
            }
        }

        // The dictionary is used for storing xmlnsInfo for assemblies that are not 'referenced',
        // by that, we mean unreferenced assemblies when the schema context is passed in a set of referenced assemblies,
        // or assemblies not loaded in the app-domain when the schema context is not passed in a set of assemblies.
        // The dictionary is thread-safe for single reads and writes, and has no requirement for iteration.
        private ConcurrentDictionary<Assembly, XmlNsInfo> XmlnsInfoForUnreferencedAssemblies
        {
            get
            {
                if (_xmlnsInfoForUnreferencedAssemblies == null)
                {
                    Interlocked.CompareExchange(ref _xmlnsInfoForUnreferencedAssemblies, CreateDictionary<Assembly, XmlNsInfo>(ReferenceEqualityComparer<Assembly>.Singleton), null);
                }
                return _xmlnsInfoForUnreferencedAssemblies;
            }
        }

        internal bool AreInternalsVisibleTo(Assembly fromAssembly, Assembly toAssembly)
        {
            if (fromAssembly.Equals(toAssembly))
            {
                return true;
            }
            XmlNsInfo nsInfo = GetXmlNsInfo(fromAssembly);
            ICollection<AssemblyName> friends = nsInfo.InternalsVisibleTo;
            if (friends.Count == 0)
            {
                return false;
            }
            // Not using Assembly.GetName() because it doesn't work in partial-trust
            AssemblyName toAssemblyName = new AssemblyName(toAssembly.FullName);
            foreach (AssemblyName friend in friends)
            {
                if (friend.Name == toAssemblyName.Name)
                {
                    byte[] expectedToken = friend.GetPublicKeyToken();
                    if (expectedToken == null)
                    {
                        // InternalsVisibleToAttribute doesn't specify a public key, so don't check it
                        return true;
                    }
                    byte[] actualToken = toAssemblyName.GetPublicKeyToken();
                    return SafeSecurityHelper.IsSameKeyToken(expectedToken, actualToken);
                }
            }
            return false;
        }

        // Entry point for GC callback. We indirect this through a static method that takes a weakref,
        // so that the callback doesn't keep the SchemaContext alive
        private static void CleanupCollectedAssemblies(object schemaContextWeakRef)
        {
            WeakReference weakRef = (WeakReference)schemaContextWeakRef;
            XamlSchemaContext schemaContext = weakRef.Target as XamlSchemaContext;
            if (schemaContext != null)
            {
                schemaContext.CleanupCollectedAssemblies();
            }
        }

        // Iterate through any weak references we hold to dynamic assemblies, cleaning up references
        // that have been collected. This prevents our caches from growing unboundedly in the case
        // where dynamic assemblies are continually getting created and disposed.
        private void CleanupCollectedAssemblies()
        {
            bool foundLiveDynamicAssemblies = false;
            lock (_syncAccessingUnexaminedAssemblies)
            {
                // setting _isGCCallbackPending inside the lock; see comment in RegisterAssemblyCleanup
                _isGCCallbackPending = false;

                if (_unexaminedAssemblies is WeakReferenceList<Assembly>)
                {
                    for (int i = _unexaminedAssemblies.Count - 1; i >= 0; i--)
                    {
                        Assembly assembly = _unexaminedAssemblies[i];
                        if (assembly == null)
                        {
                            _unexaminedAssemblies.RemoveAt(i);
                        }
                        else if (assembly.IsDynamic)
                        {
                            foundLiveDynamicAssemblies = true;
                        }
                    }
                }
            }
            lock (_syncExaminingAssemblies)
            {
                if (_xmlnsInfoForDynamicAssemblies != null)
                {
                    foreach (WeakRefKey weakRefKey in _xmlnsInfoForDynamicAssemblies.Keys)
                    {
                        if (weakRefKey.IsAlive)
                        {
                            foundLiveDynamicAssemblies = true;
                        }
                        else
                        {
                            // ConcurrentDictionary returns a copy of its keys, so it's safe to delete while enumerating
                            XmlNsInfo value;
                            _xmlnsInfoForDynamicAssemblies.TryRemove(weakRefKey, out value);
                        }
                    }
                }
            }
            if (foundLiveDynamicAssemblies)
            {
                RegisterAssemblyCleanup();
            }
        }

        private void RegisterAssemblyCleanup()
        {
            // Locking around this check prevents multiple threads from redundantly registering
            // callbacks at the same time. 
            // We could use either the examining or unexamined lock, since clenaup touches both;
            // we use the unexamined because it has a shorter blocking time.
            lock (_syncAccessingUnexaminedAssemblies)
            {
                if (!_isGCCallbackPending)
                {
                    GCNotificationToken.RegisterCallback(CleanupCollectedAssemblies, new WeakReference(this));
                    _isGCCallbackPending = true;
                }
            }
        }

        private IEnumerable<XmlNsInfo> EnumerateXmlnsInfos()
        {
            if (_xmlnsInfoForDynamicAssemblies == null)
            {
                return XmlnsInfo.Values;
            }
            else
            {
                return EnumerateStaticAndDynamicXmlnsInfos();
            }
        }

        private IEnumerable<XmlNsInfo> EnumerateStaticAndDynamicXmlnsInfos()
        {
            foreach (XmlNsInfo result in XmlnsInfo.Values)
            {
                yield return result;
            }
            foreach (XmlNsInfo result in XmlnsInfoForDynamicAssemblies.Values)
            {
                yield return result;
            }
        }

        internal string GetRootNamespace(Assembly asm)
        {
            XmlNsInfo nsInfo = GetXmlNsInfo(asm);
            return nsInfo.RootNamespace;
        }

        internal ReadOnlyCollection<string> GetXamlNamespaces(XamlType type)
        {
            Type clrType = type.UnderlyingType;
            if (clrType == null || clrType.Assembly == null)
            {
                return null;
            }

            if (XamlLanguage.AllTypes.Contains(type))
            {
                // We need a read-only list which combines the directive namespace(s) with the 
                // the namespaces(s) that this type supports through standard CLR binding rules
                IList<string> clrBoundNamespaces = GetXmlNsMappings(clrType.Assembly, clrType.Namespace);
                List<string> combinedList = new List<string>();
                combinedList.AddRange(XamlLanguage.XamlNamespaces);
                combinedList.AddRange(clrBoundNamespaces);
                return combinedList.AsReadOnly();
            }
            else
            {
                return GetXmlNsMappings(clrType.Assembly, clrType.Namespace);
            }
        }

        private XamlNamespace GetXamlNamespace(string xmlns)
        {
            XamlNamespace xamlNamespace = null;

            if (NamespaceByUriList.TryGetValue(xmlns, out xamlNamespace))
            {
                return xamlNamespace;
            }

            string clrNs, assemblyName;
            if (ClrNamespaceUriParser.TryParseUri(xmlns, out clrNs, out assemblyName))
            {
                xamlNamespace = new XamlNamespace(this, clrNs, assemblyName);
            }
            else
            {
                // unresolved namespace
                xamlNamespace = new XamlNamespace(this);
            }
            xamlNamespace = TryAdd(NamespaceByUriList, xmlns, xamlNamespace);
            return xamlNamespace;
        }

        private XmlNsInfo GetXmlNsInfo(Assembly assembly)
        {
            XmlNsInfo result;

            if (XmlnsInfo.TryGetValue(assembly, out result) ||
                (_xmlnsInfoForDynamicAssemblies != null && assembly.IsDynamic &&
                 _xmlnsInfoForDynamicAssemblies.TryGetValue(new WeakRefKey(assembly), out result)) ||
                (_xmlnsInfoForUnreferencedAssemblies != null && _xmlnsInfoForUnreferencedAssemblies.TryGetValue(assembly, out result)))
            {
                return result;
            }

            // We store XmlnsInfo in three separate caches.
            //
            // If there is a hard-coded list of reference assemblies, then:
            // 1. Referenced assemblies are in XmlnsInfo
            // 2. Everything else is in XmlnsInfoForUnreferencedAssemblies, so that it doesn't
            //    pollute the 'real' cache.
            //
            // If we are using all AppDomain-loaded assemblies, then:
            // 1. Static RuntimeAssemblies are in XmlnsInfo
            // 2. Dynamic RuntimeAssemblies are in XmlnsInfoForDynamicAssemblies, so that
            //    collectible assemblies are weakrefed.
            // 3. ReflectionOnly assemblies and custom derivations of System.Assembly are in
            //    XmlnsInfoForUnreferencedAssemblies, so that they don't pollute the 'real' cache.
            bool isReferenced = false;
            if (_referenceAssemblies != null)
            {
                foreach (var asm in _referenceAssemblies)
                {
                    if (Object.ReferenceEquals(asm, assembly))
                    {
                        isReferenced = true;
                        break;
                    }
                }
            }
            else
            {
                isReferenced = !assembly.ReflectionOnly &&
                    typeof(object).Assembly.GetType().IsAssignableFrom(assembly.GetType());
            }

            // Add the assembly to the cache
            result = new XmlNsInfo(assembly, FullyQualifyAssemblyNamesInClrNamespaces);
            if (isReferenced)
            {
                if (assembly.IsDynamic && _referenceAssemblies == null)
                {
                    result = TryAdd(XmlnsInfoForDynamicAssemblies, new WeakRefKey(assembly), result);
                    // Ensure we clean up the cache if dynamic assemblies are collected
                    RegisterAssemblyCleanup();
                }
                else
                {
                    result = TryAdd(XmlnsInfo, assembly, result);
                }
            }
            else
            {
                result = TryAdd(XmlnsInfoForUnreferencedAssemblies, assembly, result);
            }

            return result;
        }

        private ReadOnlyCollection<string> GetXmlNsMappings(Assembly assembly, string clrNs)
        {
            XmlNsInfo nsInfo = GetXmlNsInfo(assembly);
            ConcurrentDictionary<string, IList<string>> assemblyMappings = nsInfo.ClrToXmlNs;
            IList<string> result;

            clrNs = clrNs ?? string.Empty;

            if (!assemblyMappings.TryGetValue(clrNs, out result))
            {
                string assemblyName = FullyQualifyAssemblyNamesInClrNamespaces ?
                    assembly.FullName : GetAssemblyShortName(assembly);
                string xmlns = ClrNamespaceUriParser.GetUri(clrNs, assemblyName);
                List<string> list = new List<string>();
                list.Add(xmlns);
                result = list.AsReadOnly();
                TryAdd(assemblyMappings, clrNs, result);
            }
            return (ReadOnlyCollection<string>)result;
        }

        private void InitializeAssemblyLoadHook()
        {
            _syncAccessingUnexaminedAssemblies = new Object();
            if (ReferenceAssemblies == null)
            {

                _assemblyLoadHandler = new AssemblyLoadHandler(this);
                _assemblyLoadHandler.Hook();
                lock (_syncAccessingUnexaminedAssemblies)
                {
                    Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    _unexaminedAssemblies = new WeakReferenceList<Assembly>(currentAssemblies.Length);
                    bool foundDynamic = false;
                    foreach (Assembly assembly in currentAssemblies)
                    {
                        _unexaminedAssemblies.Add(assembly);
                        if (assembly.IsDynamic)
                        {
                            foundDynamic = true;
                        }
                    }
                    if (foundDynamic)
                    {
                        // Ensure we clean up the cache if dynamic assemblies are collected
                        RegisterAssemblyCleanup();
                    }
                }
            }
            else
            {
                _unexaminedAssemblies = new List<Assembly>(ReferenceAssemblies);
            }
        }

        void SchemaContextAssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
        {
            lock (_syncAccessingUnexaminedAssemblies)
            {
                if (!args.LoadedAssembly.ReflectionOnly && !_unexaminedAssemblies.Contains(args.LoadedAssembly))
                {
                    _unexaminedAssemblies.Add(args.LoadedAssembly);
                    if (args.LoadedAssembly.IsDynamic)
                    {
                        // Ensure we clean up the cache if a dynamic assemblies is collected
                        RegisterAssemblyCleanup();
                    }
                }
            }
        }

        // When this method returns:
        // - _xmlnsInfo contains all assemblies in the AppDomain or reference assemblies
        // - NamespaceByUriList contains all xmlnsdefs in _xmlnsInfo
        private void UpdateXmlNsInfo()
        {
            bool foundNew = false;
            lock (_syncExaminingAssemblies)
            {
                IList<Assembly> unexaminedAssembliesCopy;
                lock (_syncAccessingUnexaminedAssemblies)
                {
                    unexaminedAssembliesCopy = _unexaminedAssemblies;
                    _unexaminedAssemblies = new WeakReferenceList<Assembly>(0);
                }

                // If we're trawling thorugh all assemblies in the AppDomain, then we'll ignore
                //  any exceptions from invalid attributes
                bool throwOnError = (ReferenceAssemblies != null);
                for (int i = 0; i < unexaminedAssembliesCopy.Count; i++)
                {
                    var assembly = unexaminedAssembliesCopy[i];
                    if (assembly == null)
                    {
                        // The assembly is a collectible dynamic assembly, and has been GC'ed. Ignore it.
                        continue;
                    }
                    XmlNsInfo nsInfo = GetXmlNsInfo(assembly);
                    bool foundNewInThisAssembly = false;
                    try
                    {
                        foundNewInThisAssembly = UpdateXmlNsInfo(nsInfo);
                        if (foundNewInThisAssembly)
                        {
                            foundNew = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (throwOnError || CriticalExceptions.IsCriticalException(ex))
                        {
                            // The assemblies after the i'th one in unexaminedAssembliesCopy have not been examined (including the i'th assembly).
                            // So we need to add them back to _unexaminedAssemblies while also keeping the assemblies that might have been added 
                            // to _unexaminedAssemblies in parallel.
                            lock (_syncAccessingUnexaminedAssemblies)
                            {
                                for (int j = i; j < unexaminedAssembliesCopy.Count; j++)
                                {
                                    _unexaminedAssemblies.Add(unexaminedAssembliesCopy[j]);
                                }
                            }
                            throw;
                        }
                    }
                }

                if (foundNew && _nonClrNamespaces != null)
                {
                    // invalidate this and force it to be re-evaluated
                    _nonClrNamespaces = null;
                }
            }
        }

        // This method should be called inside _syncExaminingAssemblies lock
        private bool UpdateXmlNsInfo(XmlNsInfo nsInfo)
        {
            bool foundNew = UpdateNamespaceByUriList(nsInfo);
            if (_preferredPrefixes != null)
            {
                UpdatePreferredPrefixes(nsInfo, _preferredPrefixes);
            }
            return foundNew;
        }

        bool UpdateNamespaceByUriList(XmlNsInfo nsInfo)
        {
            bool foundNew = false;
            IList<XmlNsInfo.XmlNsDefinition> xmlnsDefs = nsInfo.NsDefs;
            foreach (XmlNsInfo.XmlNsDefinition xmlnsDef in xmlnsDefs)
            {
                AssemblyNamespacePair pair = new AssemblyNamespacePair(nsInfo.Assembly, xmlnsDef.ClrNamespace);
                XamlNamespace ns = GetXamlNamespace(xmlnsDef.XmlNamespace);
                ns.AddAssemblyNamespacePair(pair);
                foundNew = true;
            }
            return foundNew;
        }

        #endregion

        #region Helper Methods

        // Given an assembly, return the assembly short name.  We need to avoid Assembly.GetName() so we run in PartialTrust without asserting.
        internal static string GetAssemblyShortName(Assembly assembly)
        {
            string assemblyLongName = assembly.FullName;
            string assemblyShortName = assemblyLongName.Substring(0, assemblyLongName.IndexOf(','));
            return assemblyShortName;
        }

        internal static ConcurrentDictionary<K, V> CreateDictionary<K, V>()
        {
            return new ConcurrentDictionary<K, V>(ConcurrencyLevel, DictionaryCapacity);
        }

        internal static ConcurrentDictionary<K, V> CreateDictionary<K, V>(IEqualityComparer<K> comparer)
        {
            return new ConcurrentDictionary<K, V>(ConcurrencyLevel, DictionaryCapacity, comparer);
        }

        internal static V TryAdd<K, V>(ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.TryAdd(key, value))
            {
                return value;
            }
            else
            {
                return dictionary[key];
            }
        }

        internal static V TryUpdate<K, V>(ConcurrentDictionary<K, V> dictionary, K key, V value, V comparand)
        {
            if (dictionary.TryUpdate(key, value, comparand))
            {
                return value;
            }
            else
            {
                return dictionary[key];
            }
        }

        #endregion

        #region Assembly resolution

        // Both the array itself and each item in it are lazily initialized.
        // The indexes should match _referenceAssemblies
        private AssemblyName[] _referenceAssemblyNames;

        protected internal virtual Assembly OnAssemblyResolve(string assemblyName)
        {
            if (String.IsNullOrEmpty(assemblyName))
            {
                return null;
            }
            if (_referenceAssemblies != null)
            {
                return ResolveReferenceAssembly(assemblyName);
            }
            else
            {
                return ResolveAssembly(assemblyName);
            }
        }

        private Assembly ResolveReferenceAssembly(string assemblyName)
        {
            AssemblyName parsedAsmName = new AssemblyName(assemblyName);
            if (_referenceAssemblyNames == null)
            {
                AssemblyName[] asmNames = new AssemblyName[_referenceAssemblies.Count];
                Interlocked.CompareExchange(ref _referenceAssemblyNames, asmNames, null);
            }
            for (int i = 0; i < _referenceAssemblies.Count; i++)
            {
                AssemblyName refAsmName = _referenceAssemblyNames[i];
                if (_referenceAssemblyNames[i] == null)
                {
                    // Multiple threads may be simultaneously populating this array.
                    // That's okay; we're inserting identical data, so duplicate writes are harmless.
                    _referenceAssemblyNames[i] = new AssemblyName(_referenceAssemblies[i].FullName);
                }
                if (AssemblySatisfiesReference(_referenceAssemblyNames[i], parsedAsmName))
                {
                    return _referenceAssemblies[i];
                }
            }
            return null;
        }

        private static bool AssemblySatisfiesReference(AssemblyName assemblyName, AssemblyName reference)
        {
            if (reference.Name != assemblyName.Name)
            {
                return false;
            }
            if (reference.Version != null && !reference.Version.Equals(assemblyName.Version))
            {
                return false;
            }
            if (reference.CultureInfo != null && !reference.CultureInfo.Equals(assemblyName.CultureInfo))
            {
                return false;
            }
            byte[] requiredToken = reference.GetPublicKeyToken();
            if (requiredToken != null)
            {
                byte[] actualToken = assemblyName.GetPublicKeyToken();
                if (!SafeSecurityHelper.IsSameKeyToken(requiredToken, actualToken))
                {
                    return false;
                }
            }
            return true;
        }

        private Assembly ResolveAssembly(string assemblyName)
        {
            // First see if the assembly is already loaded. This is necessary because Assembly.Load
            // won't match to assemblies in the LoadFile or LoadFrom contexts.
            // We only allow exact matches at this point. Version-tolerant matching happens
            // below, using Assembly.Load.
            AssemblyName parsedAsmName = new AssemblyName(assemblyName);
            Assembly result = SafeSecurityHelper.GetLoadedAssembly(parsedAsmName);
            if (result != null)
            {
                return result;
            }

            try
            {
                byte[] publicKeyToken = parsedAsmName.GetPublicKeyToken();
                if (parsedAsmName.Version != null || parsedAsmName.CultureInfo != null || publicKeyToken != null)
                {
                    try
                    {
                        // First try to load the exact requested version.
                        // This will throw if fusion can't find the assembly.
                        return Assembly.Load(assemblyName);
                    }
                    catch (Exception ex)
                    {
                        if (CriticalExceptions.IsCriticalException(ex))
                        {
                            throw;
                        }
                        // Version tolerance: fall back to the short name (+ public key, if specified)
                        AssemblyName shortName = new AssemblyName(parsedAsmName.Name);
                        if (publicKeyToken != null)
                        {
                            shortName.SetPublicKeyToken(publicKeyToken);
                        }
                        return Assembly.Load(shortName);
                    }
                }
                else
                {
                    // Use LWPN because Load won't look in the GAC if it's given a short name.
                    return Assembly.LoadWithPartialName(assemblyName);
                }
            }
            catch (Exception ex)
            {
                if (CriticalExceptions.IsCriticalException(ex))
                {
                    throw;
                }
                // We don't want to throw if the assembly can't be found, we just treat it as unresolved
                return null;
            }
        }

        #endregion

        // WeakRef wrapper around XSC so that it can hook AppDomain event without getting rooted
        private class AssemblyLoadHandler
        {
            WeakReference schemaContextRef;

            public AssemblyLoadHandler(XamlSchemaContext schemaContext)
            {
                schemaContextRef = new WeakReference(schemaContext);
            }

            private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                XamlSchemaContext schemaContext = (XamlSchemaContext)schemaContextRef.Target;
                if (schemaContext != null)
                {
                    schemaContext.SchemaContextAssemblyLoadEventHandler(sender, args);
                }
            }

            /// <SecurityNote>
            /// Critical: Accesses Critical event AppDomain.AssemblyLoad.
            /// Safe: We only use the event to track what assemblies are loaded in to the AppDomain.
            ///       This is not privileged info, as it is available via AppDomain.GetAssemblies().
            /// </SecurityNote>
#if TARGETTING35SP1
            [SecurityTreatAsSafe, SecurityCritical]
#else
            [SecuritySafeCritical]
#endif
            public void Hook()
            {
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            }

            /// <SecurityNote>
            /// Critical: Accesses Critical event AppDomain.AssemblyLoad.
            /// Safe: We just remove a handler that we ourselves added.
            /// </SecurityNote>
#if TARGETTING35SP1
            [SecurityTreatAsSafe, SecurityCritical]
#else
            [SecuritySafeCritical]
#endif
            public void Unhook()
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            }
        }

        private class WeakReferenceList<T> : List<WeakReference>, IList<T> where T : class
        {
            public WeakReferenceList(int capacity)
                : base(capacity)
            {
            }

            int IList<T>.IndexOf(T item)
            {
                throw new NotSupportedException();
            }

            void IList<T>.Insert(int index, T item)
            {
                Insert(index, new WeakReference(item));
            }

            T IList<T>.this[int index]
            {
                get
                {
                    return (T)this[index].Target;
                }
                set
                {
                    this[index] = new WeakReference(value);
                }
            }

            void ICollection<T>.Add(T item)
            {
                Add(new WeakReference(item));
            }

            bool ICollection<T>.Contains(T item)
            {
                foreach (WeakReference weakRef in (IEnumerable<WeakReference>)this)
                {
                    if ((object)item == weakRef.Target)
                    {
                        return true;
                    }
                }
                return false;
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[i + arrayIndex] = (T)this[i].Target;
                }
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<T>.IsReadOnly
            {
                get { return false; }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return Enumerate().GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>)this).GetEnumerator();
            }

            private IEnumerable<T> Enumerate()
            {
                foreach (WeakReference weakRef in (IEnumerable<WeakReference>)this)
                {
                    yield return (T)weakRef.Target;
                }
            }
        }
    }
}
