// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using System.Windows.Markup;
using MS.Internal.Xaml.Parser;

namespace System.Xaml
{
    /// <SecurityNote>
    /// This class is extensible; various members which could be used for visibility evaluation--
    /// IsPublic, BaseType, CanAssignTo--are either virtual, or get their data from virtual methods.
    /// For security-critical data, always check the underlying CLR type.
    /// </SecurityNote>
    public class XamlType : IEquatable<XamlType>
    {
        // Initialized in constructor
        string _name;
        XamlSchemaContext _schemaContext;
        IList<XamlType> _typeArguments;

        // Thread safety: if setting outside ctor, do an interlocked compare against null
        TypeReflector _reflector;

        /// <summary>
        /// Lazy init: NullableReference.IsSet is null when not initialized
        /// </summary>
        /// <SecurityNote>
        /// We cache a visibility check based on this value, so it must be idempotent
        /// </SecurityNote>
        [SecurityCritical]
        private NullableReference<Type> _underlyingType;
        
        // Lazy init: null until initialized
        // Thread safety: idempotent, assignment races are okay; do not assign incomplete values
        ReadOnlyCollection<string> _namespaces;
        ThreeValuedBool _isNameValid;

        protected XamlType(string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _name = typeName;
            _schemaContext = schemaContext;
            _typeArguments = GetTypeArguments(typeArguments);
        }

        public XamlType(string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
        {
            if (unknownTypeNamespace == null)
            {
                throw new ArgumentNullException("unknownTypeNamespace");
            }
            if (unknownTypeName == null)
            {
                throw new ArgumentNullException("unknownTypeName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _name = unknownTypeName;
            _namespaces = new ReadOnlyCollection<string>(new string[] { unknownTypeNamespace });
            _schemaContext = schemaContext;
            _typeArguments = GetTypeArguments(typeArguments);
            _reflector = TypeReflector.UnknownReflector;
        }        

        public XamlType(Type underlyingType, XamlSchemaContext schemaContext)
            :this(underlyingType, schemaContext, null)
        {
        }

        public XamlType(Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker)
            : this(null, underlyingType, schemaContext, invoker, null)
        {
        }

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingType
        /// Safe: Constructor is single-threaded, so idempotence is assured
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal XamlType(string alias, Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker, TypeReflector reflector)
        {
            if (underlyingType == null)
            {
                throw new ArgumentNullException("underlyingType");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _reflector = reflector ?? new TypeReflector(underlyingType);
            _name = alias ?? GetTypeName(underlyingType);
            _schemaContext = schemaContext;
            _typeArguments = GetTypeArguments(underlyingType, schemaContext);
            _underlyingType.Value = underlyingType;
            _reflector.Invoker = invoker;
        }

        public XamlType BaseType
        {
            get
            {
                EnsureReflector();
                if (!_reflector.BaseTypeIsSet)
                {
                    _reflector.BaseType = LookupBaseType();
                }
                return _reflector.BaseType;
            }
        }

        public XamlTypeInvoker Invoker
        {
            get
            {
                EnsureReflector();
                if (_reflector.Invoker == null)
                {
                    _reflector.Invoker = LookupInvoker() ?? XamlTypeInvoker.UnknownInvoker;
                }
                return _reflector.Invoker;
            }
        }

        public bool IsNameValid
        {
            get
            {
                if (_isNameValid == ThreeValuedBool.NotSet)
                {
                    _isNameValid = XamlName.IsValidXamlName(_name) ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return _isNameValid == ThreeValuedBool.True;
            }
        }

        public bool IsUnknown
        {
            get
            {
                EnsureReflector();
                return _reflector.IsUnknown;
            }
        }

        public string Name { get { return _name; } }

        public string PreferredXamlNamespace
        {
            get
            {
                IList<string> namespaces = GetXamlNamespaces();
                if (namespaces.Count > 0)
                {
                    return namespaces[0];
                }
                return null;
            }
        }

        public IList<XamlType> TypeArguments
        {
            get
            {
                // If this is an open generic, some of these will be placeholders (e.g. T)
                // The only way to get an open generic is to explicitly ask for it via
                // XamlSchameContext.Get(Type), so the caller is responsible for handling it
                return _typeArguments;
            }
        }

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingType
        /// Safe: Ensures idempotence via NullableReference.SetIfNull, which uses CompareExchange
        /// </SecurityNote>
        public Type UnderlyingType
        {
            [SecuritySafeCritical]
            get
            {
                if (!_underlyingType.IsSet)
                {
                    _underlyingType.SetIfNull(LookupUnderlyingType());
                }
                return _underlyingType.Value;
            }
        }

        /// <summary>
        /// Accesses UnderlyingType without initializing it
        /// </summary>
        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingType
        /// Safe: Doesn't modify field value. Field is value type, so caller cannot modify it.
        /// </SecurityNote>
        internal NullableReference<Type> UnderlyingTypeInternal
        {
            [SecuritySafeCritical]
            get { return _underlyingType; }
        }

        public bool ConstructionRequiresArguments { get { return GetFlag(BoolTypeBits.ConstructionRequiresArguments); } }
        public bool IsArray { get { return GetCollectionKind() == XamlCollectionKind.Array; } }
        public bool IsCollection { get { return GetCollectionKind() == XamlCollectionKind.Collection; } }
        public bool IsConstructible { get { return GetFlag(BoolTypeBits.Constructible); } }
        public bool IsDictionary { get { return GetCollectionKind() == XamlCollectionKind.Dictionary; } }
        public bool IsGeneric { get { return TypeArguments != null; } }
        public bool IsMarkupExtension { get { return GetFlag(BoolTypeBits.MarkupExtension); } }
        public bool IsNameScope { get { return GetFlag(BoolTypeBits.NameScope); } }
        public bool IsNullable { get { return GetFlag(BoolTypeBits.Nullable); } }
        public bool IsPublic { get { return GetFlag(BoolTypeBits.Public); } }
        public bool IsUsableDuringInitialization { get { return GetFlag(BoolTypeBits.UsableDuringInitialization); } }
        public bool IsWhitespaceSignificantCollection { get { return GetFlag(BoolTypeBits.WhitespaceSignificantCollection); } }
        public bool IsXData { get { return GetFlag(BoolTypeBits.XmlData); } }
        public bool TrimSurroundingWhitespace { get { return GetFlag(BoolTypeBits.TrimSurroundingWhitespace); } }
        public bool IsAmbient { get { return GetFlag(BoolTypeBits.Ambient); } }

        public XamlType KeyType
        {
            get
            {
                if (!IsDictionary)
                {
                    return null;
                }
                Debug.Assert(_reflector != null, "_reflector should have been initialized by IsDictionary");
                if (_reflector.KeyType == null)
                {
                    _reflector.KeyType = LookupKeyType() ?? XamlLanguage.Object;
                }
                return _reflector.KeyType;
            }
        }

        public XamlType ItemType
        {
            get
            {
                if (GetCollectionKind() == XamlCollectionKind.None)
                {
                    return null;
                }
                Debug.Assert(_reflector != null, "_reflector should have been initialized by GetCollectionKind");
                if (_reflector.ItemType == null)
                {
                    _reflector.ItemType = LookupItemType() ?? XamlLanguage.Object;
                }
                return _reflector.ItemType;
            }
        }

        public IList<XamlType> AllowedContentTypes
        {
            get
            {
                XamlCollectionKind collectionKind = GetCollectionKind();
                if (collectionKind != XamlCollectionKind.Collection && 
                    collectionKind != XamlCollectionKind.Dictionary)
                {
                    return null;
                }
                Debug.Assert(_reflector != null, "_reflector should have been initialized by GetCollectionKind");
                if (_reflector.AllowedContentTypes == null)
                {
                    _reflector.AllowedContentTypes = LookupAllowedContentTypes() ?? 
                        EmptyList<XamlType>.Value;
                }
                return _reflector.AllowedContentTypes;
            }
        }

        public IList<XamlType> ContentWrappers
        {
            get
            {
                if (!IsCollection)
                {
                    return null;
                }
                Debug.Assert(_reflector != null, "_reflector should have been initialized by IsCollection");
                if (_reflector.ContentWrappers == null)
                {
                    _reflector.ContentWrappers = LookupContentWrappers() ?? 
                        EmptyList<XamlType>.Value;
                }
                return _reflector.ContentWrappers;
            }
        }

        public XamlValueConverter<TypeConverter> TypeConverter
        {
            get
            {
                EnsureReflector();
                if (!_reflector.TypeConverterIsSet)
                {
                    _reflector.TypeConverter = LookupTypeConverter();
                }
                return _reflector.TypeConverter;
            }
        }

        public XamlValueConverter<ValueSerializer> ValueSerializer
        {
            get
            {
                EnsureReflector();
                if (!_reflector.ValueSerializerIsSet)
                {
                    _reflector.ValueSerializer = LookupValueSerializer();
                }
                return _reflector.ValueSerializer;
            }
        }

        public XamlMember ContentProperty
        {
            get
            {
                EnsureReflector();
                if (!_reflector.ContentPropertyIsSet)
                {
                    _reflector.ContentProperty = LookupContentProperty();
                }
                return _reflector.ContentProperty;
            }
        }

        public XamlValueConverter<XamlDeferringLoader> DeferringLoader
        {
            get
            {
                EnsureReflector();
                if (!_reflector.DeferringLoaderIsSet)
                {
                    _reflector.DeferringLoader = LookupDeferringLoader();
                }
                return _reflector.DeferringLoader;
            }
        }

        public XamlType MarkupExtensionReturnType
        {
            get
            {
                if (!IsMarkupExtension)
                {
                    return null;
                }
                Debug.Assert(_reflector != null, "_reflector should have been initialized by IsMarkupExtension");
                if (_reflector.MarkupExtensionReturnType == null)
                {
                    _reflector.MarkupExtensionReturnType = LookupMarkupExtensionReturnType() ?? XamlLanguage.Object;
                }
                return _reflector.MarkupExtensionReturnType;
            }
        }

        public XamlSchemaContext SchemaContext { get { return _schemaContext; } }

        public XamlMember GetMember(string name)
        {
            EnsureReflector();
            XamlMember result;
            if (!_reflector.Members.TryGetValue(name, out result) && !_reflector.Members.IsComplete)
            {
                result = LookupMember(name, false /*skipReadOnlyCheck*/);
                result = _reflector.Members.TryAdd(name, result);
            }
            return result;
        }

        public ICollection<XamlMember> GetAllMembers()
        {
            EnsureReflector();
            if (!_reflector.Members.IsComplete)
            {
                IEnumerable<XamlMember> members = LookupAllMembers();
                if (members != null)
                {
                    foreach (XamlMember member in members)
                    {
                        _reflector.Members.TryAdd(member.Name, member);
                    }

                    _reflector.Members.IsComplete = true;
                }
            }

            return _reflector.Members.Values;
        }

        public XamlMember GetAliasedProperty(XamlDirective directive)
        {
            EnsureReflector();
            // Perf note: would be nice to optimize this. We currently have to do the same mapping of 
            // the directive to one of the four known directives 3 times for each type in the hierarchy
            XamlMember result;
            if (!_reflector.TryGetAliasedProperty(directive, out result))
            {
                result = LookupAliasedProperty(directive);
                _reflector.TryAddAliasedProperty(directive, result);
            }
            return result;
        }

        public XamlMember GetAttachableMember(string name)
        {
            EnsureReflector();
            XamlMember result;
            if (!_reflector.AttachableMembers.TryGetValue(name, out result) &&
                !_reflector.AttachableMembers.IsComplete)
            {
                result = LookupAttachableMember(name);
                result = _reflector.AttachableMembers.TryAdd(name, result);
            }
            return result;
        }

        public ICollection<XamlMember> GetAllAttachableMembers()
        {
            EnsureReflector();
            if (!_reflector.AttachableMembers.IsComplete)
            {
                IEnumerable<XamlMember> members = LookupAllAttachableMembers();
                if (members != null)
                {
                    foreach (XamlMember member in members)
                    {
                        _reflector.AttachableMembers.TryAdd(member.Name, member);
                    }
                }
                _reflector.AttachableMembers.IsComplete = true;
            }
            return _reflector.AttachableMembers.Values;
        }

        public virtual bool CanAssignTo(XamlType xamlType)
        {
            if (object.ReferenceEquals(xamlType, null))
            {
                return false;
            }

            Type baseUnderlyingType = xamlType.UnderlyingType;
            XamlType curType = this;
            do
            {
                Type curUnderlyingType = curType.UnderlyingType;
                if (baseUnderlyingType != null && curUnderlyingType != null)
                {
                    if (curUnderlyingType.Assembly.ReflectionOnly &&
                        baseUnderlyingType.Assembly == typeof(XamlType).Assembly)
                    {
                        // Need to be version-tolerant of reference to built-in XAML language types,
                        // so it is possible to statically analyze assemblies built against older versions
                        return LooseTypeExtensions.IsAssemblyQualifiedNameAssignableFrom(
                            baseUnderlyingType, curUnderlyingType);
                    }
                    return baseUnderlyingType.IsAssignableFrom(curUnderlyingType);
                }
                if (curType == xamlType)
                {
                    return true;
                }
                curType = curType.BaseType;
            }
            while (curType != null);
            return false;
        }

        public IList<XamlType> GetPositionalParameters(int parameterCount)
        {
            EnsureReflector();
            IList<XamlType> result;
            if (!_reflector.TryGetPositionalParameters(parameterCount, out result))
            {
                result = LookupPositionalParameters(parameterCount);
                result = _reflector.TryAddPositionalParameters(parameterCount, result);
            }
            return result;
        }

        public virtual IList<string> GetXamlNamespaces()
        {
            if (_namespaces == null)
            {
                _namespaces = _schemaContext.GetXamlNamespaces(this);
                if (_namespaces == null)
                {
                    _namespaces = new ReadOnlyCollection<string>(new string[] { string.Empty });
                }
            }
            return _namespaces;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            AppendTypeName(sb, false);
            return sb.ToString();
        }
        
        internal bool IsUsableAsReadOnly
        {
            get
            {
                XamlCollectionKind collectionKind = GetCollectionKind();
                return 
                    (collectionKind == XamlCollectionKind.Collection) ||
                    (collectionKind == XamlCollectionKind.Dictionary) ||
                    IsXData;
            }
        }
       
        internal MethodInfo IsReadOnlyMethod
        {
            get
            {
                if (ItemType == null || UnderlyingType == null)
                {
                    return null;
                }

                Debug.Assert(_reflector != null, "_reflector should have been initialized by ItemType");
                if (!_reflector.IsReadOnlyMethodIsSet)
                {
                    if (UnderlyingType != null && ItemType.UnderlyingType != null)
                    {
                        _reflector.IsReadOnlyMethod = CollectionReflector.
                            GetIsReadOnlyMethod(UnderlyingType, ItemType.UnderlyingType);
                    }
                    else
                    {
                        _reflector.IsReadOnlyMethod = null;
                    }
                }
                return _reflector.IsReadOnlyMethod;
            }
        }

        internal EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
        {
            get
            {
                if (!_reflector.XamlSetMarkupExtensionHandlerIsSet)
                {
                    _reflector.XamlSetMarkupExtensionHandler = LookupSetMarkupExtensionHandler();
                }
                return _reflector.XamlSetMarkupExtensionHandler;
            }
        }

        internal EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
        {
            get
            {
                EnsureReflector();
                if (!_reflector.XamlSetTypeConverterHandlerIsSet)
                {
                    _reflector.XamlSetTypeConverterHandler = LookupSetTypeConverterHandler();
                }
                return _reflector.XamlSetTypeConverterHandler;
            }
        }

        internal MethodInfo AddMethod
        {
            get
            {
                if (UnderlyingType == null)
                {
                    return null;
                }
                EnsureReflector();
                if (!_reflector.AddMethodIsSet)
                {
                    XamlCollectionKind collectionKind = GetCollectionKind();
                    _reflector.AddMethod = CollectionReflector.LookupAddMethod(UnderlyingType, collectionKind);
                }
                return _reflector.AddMethod;
            }
        }

        internal MethodInfo GetEnumeratorMethod
        {
            get
            {
                if (GetCollectionKind() == XamlCollectionKind.None || UnderlyingType == null)
                {
                    return null;
                }
                Debug.Assert(_reflector != null, "_reflector should have been initialized by GetCollectionKind");
                if (!_reflector.GetEnumeratorMethodIsSet)
                {
                    _reflector.GetEnumeratorMethod = CollectionReflector.GetEnumeratorMethod(UnderlyingType);
                }
                return _reflector.GetEnumeratorMethod;
            }
        }

        internal string GetQualifiedName()
        {
            StringBuilder sb = new StringBuilder();
            AppendTypeName(sb, true);
            return sb.ToString();
        }

        // Security note:
        // Keep this internal so that people don't use it for real security decisions.
        // This is only for convenience filtering, we still depend on the CLR for our real security.
        //
        // Extensibility note:
        // This is not overridable since it does not make sense in a non-CLR context.
        internal bool IsVisibleTo(Assembly accessingAssembly)
        {
            if (IsPublic)
            {
                return true;
            }
            Type underlyingType = UnderlyingType;
            return accessingAssembly != null && underlyingType != null &&
                TypeReflector.IsVisibleTo(underlyingType, accessingAssembly, SchemaContext);
        }

        internal ICollection<XamlMember> GetAllExcludedReadOnlyMembers()
        {
            EnsureReflector();
            if (_reflector.ExcludedReadOnlyMembers == null)
            {
                _reflector.ExcludedReadOnlyMembers = LookupAllExcludedReadOnlyMembers() ?? EmptyList<XamlMember>.Value;
            }
            return _reflector.ExcludedReadOnlyMembers;
        }

        // If a type is public, returns only its public constructors. Otherwise, returns its
        // public and internal constructors.
        internal IEnumerable<ConstructorInfo> GetConstructors()
        {
            if (UnderlyingType == null)
            {
                return EmptyList<ConstructorInfo>.Value;
            }
            if (IsPublic)
            {
                return UnderlyingType.GetConstructors();
            }
            return GetPublicAndInternalConstructors();
        }

        // Looks up a ctor overload from the set provided by GetConstructors.
        internal ConstructorInfo GetConstructor(Type[] paramTypes)
        {
            if (UnderlyingType == null)
            {
                return null;
            }
            IEnumerable<ConstructorInfo> ctors = GetConstructors();
            ConstructorInfo[] ctorArray = ctors as ConstructorInfo[];
            if (ctorArray == null)
            {
                ctorArray = new List<ConstructorInfo>(ctors).ToArray();
            }
            MethodBase result = Type.DefaultBinder.SelectMethod(ConstructorBindingFlags, ctorArray, paramTypes, null);
            return (ConstructorInfo)result;
        }

        protected virtual XamlMember LookupAliasedProperty(XamlDirective directive)
        {
            if (AreAttributesAvailable)
            {
                Type attributeType = null;
                bool skipReadOnlyCheck = false;
                if (directive == XamlLanguage.Key)
                {
                    attributeType = typeof(DictionaryKeyPropertyAttribute);
                    skipReadOnlyCheck = true;
                }
                else if (directive == XamlLanguage.Name)
                {
                    attributeType = typeof(RuntimeNamePropertyAttribute);
                }
                else if (directive == XamlLanguage.Uid)
                {
                    attributeType = typeof(UidPropertyAttribute);
                }
                else if (directive == XamlLanguage.Lang)
                {
                    attributeType = typeof(XmlLangPropertyAttribute);
                }
                if (attributeType != null)
                {
                    string propertyName;
                    if (TryGetAttributeString(attributeType, out propertyName))
                    {
                        if (string.IsNullOrEmpty(propertyName))
                        {
                            return null;
                        }
                        return GetPropertyOrUnknown(propertyName, skipReadOnlyCheck);
                    }
                }
            }
            if (BaseType != null)
            {
                return BaseType.GetAliasedProperty(directive);
            }
            return null;
        }

        protected virtual IList<XamlType> LookupAllowedContentTypes()
        {
            IList<XamlType> contentWrappers = ContentWrappers ?? EmptyList<XamlType>.Value;
            List<XamlType> result = new List<XamlType>(contentWrappers.Count + 1);
            result.Add(ItemType);

            foreach (XamlType contentWrapper in contentWrappers)
            {
                if (contentWrapper.ContentProperty != null &&
                    !contentWrapper.ContentProperty.IsUnknown)
                {
                    XamlType contentType = contentWrapper.ContentProperty.Type;
                    if (!result.Contains(contentType))
                    {
                        result.Add(contentType);
                    }
                }
            }

            return result.AsReadOnly();
        }

        protected virtual XamlType LookupBaseType()
        {
            Type underlyingType = UnderlyingType;
            if (underlyingType == null)
            {
                return XamlLanguage.Object;
            }
            if (underlyingType.BaseType != null)
            {
                return SchemaContext.GetXamlType(underlyingType.BaseType);
            }
            return null;
        }

        protected virtual XamlCollectionKind LookupCollectionKind()
        {
            if (UnderlyingType == null)
            {
                return (BaseType != null) ? BaseType.GetCollectionKind() : XamlCollectionKind.None;
            }

            MethodInfo addMethod = null;
            XamlCollectionKind result = CollectionReflector.LookupCollectionKind(UnderlyingType, out addMethod);
            if (addMethod != null)
            {
                _reflector.AddMethod = addMethod;
            }
            return result;
        }

        protected virtual bool LookupConstructionRequiresArguments()
        {
            Type underlyingType = UnderlyingType;
            if (underlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.ConstructionRequiresArguments);
            }
            if (underlyingType.IsValueType)
            {
                // Value types have built-in default constructor
                return false;
            }
            ConstructorInfo defaultCtor = underlyingType.GetConstructor(ConstructorBindingFlags, null, Type.EmptyTypes, null);
            return (defaultCtor == null) || !TypeReflector.IsPublicOrInternal(defaultCtor);
        }

        protected virtual XamlMember LookupContentProperty()
        {
            string contentPropertyName;
            if (TryGetAttributeString(typeof(ContentPropertyAttribute), out contentPropertyName))
            {
                if (string.IsNullOrEmpty(contentPropertyName))
                {
                    return null;
                }
                return GetPropertyOrUnknown(contentPropertyName, false /*skipReadOnlyCheck*/);
            }
            if (BaseType != null)
            {
                return BaseType.ContentProperty;
            }
            return null;
        }

        protected virtual IList<XamlType> LookupContentWrappers()
        {
            List<XamlType> contentWrappers = null;
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                List<Type> wrapperTypes = _reflector.GetAllAttributeContents<Type>(typeof(ContentWrapperAttribute));
                if (wrapperTypes != null)
                {
                    contentWrappers = new List<XamlType>(wrapperTypes.Count);
                    foreach (Type wrapperType in wrapperTypes)
                    {
                        contentWrappers.Add(SchemaContext.GetXamlType(wrapperType));
                    }
                }
            }

            if (BaseType != null)
            {
                IList<XamlType> baseWrappers = BaseType.ContentWrappers;
                if (contentWrappers == null)
                {
                    return baseWrappers;
                }
                else if (baseWrappers != null)
                {
                    contentWrappers.AddRange(baseWrappers);
                }
            }

            return GetReadOnly(contentWrappers);
        }

        protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
        {
            return null;
        }

        protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                Type[] loaderTypes = _reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
                if (loaderTypes != null)
                {
                    return SchemaContext.GetValueConverter<XamlDeferringLoader>(loaderTypes[0], null);
                }
            }
            if (BaseType != null)
            {
                return BaseType.DeferringLoader;
            }
            return null;
        }

        protected virtual bool LookupIsConstructible()
        {
            Type underlyingType = UnderlyingType;
            if (underlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.Constructible);
            }
            // Type must be concrete, non-nested, and closed
            if (underlyingType.IsAbstract ||
                underlyingType.IsInterface ||
                underlyingType.IsNested ||
                underlyingType.IsGenericParameter ||
                underlyingType.IsGenericTypeDefinition)
            {
                return false;
            }
            // Value types have built-in default constructor
            if (underlyingType.IsValueType)
            {
                return true;
            }
            if (!ConstructionRequiresArguments)
            {
                return true;
            }
            // Look for constructors
            foreach (ConstructorInfo ctor in GetConstructors())
            {
                // found one, that's all we need
                return true;
            }
            return false;
        }

        protected virtual XamlTypeInvoker LookupInvoker()
        {
            return (UnderlyingType != null) ? new XamlTypeInvoker(this) : null;
        }

        protected virtual bool LookupIsMarkupExtension()
        {
            return CanAssignTo(XamlLanguage.MarkupExtension);
        }

        protected virtual bool LookupIsNameScope()
        {
            return CanAssignTo(XamlLanguage.INameScope);
        }

        protected virtual bool LookupIsNullable()
        {
            if (UnderlyingType != null)
            {
                return !UnderlyingType.IsValueType || IsNullableGeneric();
            }
            return GetDefaultFlag(BoolTypeBits.Nullable);
        }

        protected virtual bool LookupIsUnknown()
        {
            if (_reflector != null)
            {
                return _reflector.IsUnknown;
            }
            return UnderlyingType == null;
        }

        protected virtual bool LookupIsWhitespaceSignificantCollection()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                if (_reflector.IsAttributePresent(typeof(WhitespaceSignificantCollectionAttribute)))
                {
                    return true;
                }
            }
            if (BaseType != null)
            {
                return BaseType.IsWhitespaceSignificantCollection;
            }
            if (IsUnknown)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                return _reflector.GetFlag(BoolTypeBits.WhitespaceSignificantCollection).Value;
            }
            return GetDefaultFlag(BoolTypeBits.WhitespaceSignificantCollection);
        }

        protected virtual XamlType LookupKeyType()
        {
            MethodInfo addMethod = AddMethod;
            if (addMethod != null)
            {
                ParameterInfo[] addParams = addMethod.GetParameters();
                if (addParams.Length == 2)
                {
                    return SchemaContext.GetXamlType(addParams[0].ParameterType);
                }
            }
            else if (UnderlyingType == null && BaseType != null)
            {
                return BaseType.KeyType;
            }
            return null;
        }

        protected virtual XamlType LookupItemType()
        {
            Type result = null;
            MethodInfo addMethod = AddMethod;
            if (addMethod != null)
            {
                ParameterInfo[] addParams = addMethod.GetParameters();
                if (addParams.Length == 2)
                {
                    result = addParams[1].ParameterType;
                }
                else if (addParams.Length == 1)
                {
                    result = addParams[0].ParameterType;
                }
            }
            else if (UnderlyingType != null)
            {
                if (UnderlyingType.IsArray)
                {
                    result = UnderlyingType.GetElementType();
                }
            }
            else if (BaseType != null)
            {
                return BaseType.ItemType;
            }
            return (result != null) ? SchemaContext.GetXamlType(result) : null;
        }

        protected virtual XamlType LookupMarkupExtensionReturnType()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                Type returnType = _reflector.GetAttributeType(typeof(MarkupExtensionReturnTypeAttribute));
                if (returnType != null)
                {
                    XamlType xamlReturnType = SchemaContext.GetXamlType(returnType);
                    return xamlReturnType;
                }
            }
            if (BaseType != null)
            {
                return BaseType.MarkupExtensionReturnType;
            }
            return null;
        }

        protected virtual IEnumerable<XamlMember> LookupAllAttachableMembers()
        {
            if (UnderlyingType == null)
            {
                return (BaseType != null) ? BaseType.GetAllAttachableMembers() : null;
            }
            EnsureReflector();
            return _reflector.LookupAllAttachableMembers(SchemaContext);
        }

        protected virtual IEnumerable<XamlMember> LookupAllMembers()
        {
            if (UnderlyingType == null)
            {
                return (BaseType != null) ? BaseType.GetAllMembers() : null;
            }

            EnsureReflector();
            ICollection<PropertyInfo> properties; 
            ICollection<EventInfo> events; 
            List<XamlMember> result;
            _reflector.LookupAllMembers(out properties, out events, out result);

            if (properties != null)
            {
                foreach (PropertyInfo pi in properties)
                {
                    XamlMember member = SchemaContext.GetProperty(pi);
                    if (!member.IsReadOnly || member.Type.IsUsableAsReadOnly)
                    {
                        result.Add(member);
                    }
                }
            }
            if (events != null)
            {
                foreach (EventInfo ei in events)
                {
                    XamlMember member = SchemaContext.GetEvent(ei);
                    result.Add(member);
                }
            }
            return result;
        }

        protected virtual XamlMember LookupMember(string name, bool skipReadOnlyCheck)
        {
            if (UnderlyingType == null)
            {
                if (BaseType != null)
                {
                    return skipReadOnlyCheck ? BaseType.LookupMember(name, true) : BaseType.GetMember(name);
                }
                return null;
            }

            // Note: if an event on a derived class shadows a property on a base class, this algorithm
            // will still return the property, not the event. That is by design, for backcompat.
            EnsureReflector();
            PropertyInfo pi = _reflector.LookupProperty(name);
            if (pi != null)
            {
                XamlMember result = SchemaContext.GetProperty(pi);
                // Filter out read-only properties except for dictionaries and collections
                if (!skipReadOnlyCheck && result.IsReadOnly && !result.Type.IsUsableAsReadOnly)
                {
                    return null;
                }
                return result;
            }
            EventInfo ei = _reflector.LookupEvent(name);
            if (ei != null)
            {
                return SchemaContext.GetEvent(ei);
            }
            return null;
        }

        protected virtual XamlMember LookupAttachableMember(string name)
        {
            if (UnderlyingType == null)
            {
                return (BaseType != null) ? BaseType.GetAttachableMember(name) : null;
            }

            EnsureReflector();
            MethodInfo getter, setter;
            if (_reflector.LookupAttachableProperty(name, out getter, out setter))
            {
                XamlMember result = SchemaContext.GetAttachableProperty(name, getter, setter);
                if (result.IsReadOnly && !result.Type.IsUsableAsReadOnly)
                {
                    return null;
                }
                return result;
            }
            setter = _reflector.LookupAttachableEvent(name);
            if (setter != null)
            {
                return SchemaContext.GetAttachableEvent(name, setter);
            }
            return null;
        }

        protected virtual IList<XamlType> LookupPositionalParameters(int parameterCount)
        {
            if (UnderlyingType == null)
            {
                return null;
            }

            // When we reflect, we have to lookup all the constructors. However, it's possible
            // that a derived class may override lookup for some param counts, but no others. So we
            // save the results of our reflection in a separate dictionary, and only surface each item
            // as requested.
            EnsureReflector();
            if (_reflector.ReflectedPositionalParameters == null)
            {
                _reflector.ReflectedPositionalParameters = LookupAllPositionalParameters();
            }
            IList<XamlType> result;
            _reflector.ReflectedPositionalParameters.TryGetValue(parameterCount, out result);
            return result;
        }

        protected virtual Type LookupUnderlyingType()
        {
            return UnderlyingTypeInternal.Value;
        }

        protected virtual bool LookupIsPublic()
        {
            Type underlyingType = UnderlyingType;
            if (underlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.Public);
            }
            return underlyingType.IsVisible;
        }

        protected virtual bool LookupIsXData()
        {
            return CanAssignTo(XamlLanguage.IXmlSerializable);
        }

        protected virtual bool LookupIsAmbient()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                if (_reflector.IsAttributePresent(typeof(AmbientAttribute)))
                {
                    return true;
                }
            }
            if (BaseType != null)
            {
                return BaseType.IsAmbient;
            }
            if (IsUnknown)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                return _reflector.GetFlag(BoolTypeBits.Ambient).Value;
            }
            return GetDefaultFlag(BoolTypeBits.Ambient);
        }

        protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                Type converterType = _reflector.GetAttributeType(typeof(TypeConverterAttribute));
                if (converterType != null)
                {
                    return SchemaContext.GetValueConverter<TypeConverter>(converterType, null);
                }
            }

            if (BaseType != null)
            {
                XamlValueConverter<TypeConverter> result = BaseType.TypeConverter;
                if (result != null && result.TargetType != XamlLanguage.Object)
                {
                    return result;
                }
            }

            Type underlyingType = UnderlyingType;
            if (underlyingType != null)
            {
                if (underlyingType.IsEnum)
                {
                    return SchemaContext.GetValueConverter<TypeConverter>(typeof(EnumConverter), this);
                }
                XamlValueConverter<TypeConverter> result = BuiltInValueConverter.GetTypeConverter(underlyingType);
                if (result != null)
                {
                    return result;
                }
                if (IsNullableGeneric())
                {
                    Type[] typeArgs = underlyingType.GetGenericArguments();
                    Debug.Assert(typeArgs.Length == 1);
                    XamlType innerXamlType = SchemaContext.GetXamlType(typeArgs[0]);
                    return innerXamlType.TypeConverter;
                }
            }
            return null;
        }

        protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                Type converterType = _reflector.GetAttributeType(typeof(ValueSerializerAttribute));
                if (converterType != null)
                {
                    return SchemaContext.GetValueConverter<ValueSerializer>(converterType, null);
                }
            }

           if (BaseType != null)
            {
                XamlValueConverter<ValueSerializer> result = BaseType.ValueSerializer;
                if (result != null)
                {
                    return result;
                }
            }

            Type underlyingType = UnderlyingType;
            if (underlyingType != null)
            {
                XamlValueConverter<ValueSerializer> result = BuiltInValueConverter.GetValueSerializer(underlyingType);
                if (result != null)
                {
                    return result;
                }
                if (IsNullableGeneric())
                {
                    Type[] typeArgs = underlyingType.GetGenericArguments();
                    Debug.Assert(typeArgs.Length == 1);
                    XamlType innerXamlType = SchemaContext.GetXamlType(typeArgs[0]);
                    return innerXamlType.ValueSerializer;
                }
            }
            return null;
        }

        #region TODO, 673231, make these publicly accessible (e.g. via APs)

        protected virtual bool LookupTrimSurroundingWhitespace()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                if (_reflector.IsAttributePresent(typeof(TrimSurroundingWhitespaceAttribute)))
                {
                    return true;
                }
            }
            if (BaseType != null)
            {
                return BaseType.TrimSurroundingWhitespace;
            }
            return GetDefaultFlag(BoolTypeBits.TrimSurroundingWhitespace);
        }

        protected virtual bool LookupUsableDuringInitialization()
        {
            if (AreAttributesAvailable)
            {
                Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");
                bool? usable = _reflector.GetAttributeValue<bool>(typeof(UsableDuringInitializationAttribute));
                if (usable.HasValue)
                {
                    return usable.Value;
                }
            }
            if (BaseType != null)
            {
                return BaseType.IsUsableDuringInitialization;
            }
            return GetDefaultFlag(BoolTypeBits.UsableDuringInitialization);
        }

        protected virtual EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler()
        {
            if (UnderlyingType != null)
            {
                string methodName;
                if (TryGetAttributeString(typeof(XamlSetMarkupExtensionAttribute), out methodName))
                {
                    if (string.IsNullOrEmpty(methodName))
                    {
                        return null;
                    }
                    return (EventHandler<XamlSetMarkupExtensionEventArgs>)SafeReflectionInvoker.CreateDelegate(
                        typeof(EventHandler<XamlSetMarkupExtensionEventArgs>), UnderlyingType, methodName);
                }
            }
            if (BaseType != null)
            {
                return BaseType.SetMarkupExtensionHandler;
            }
            return null;
        }

        protected virtual EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler()
        {
            if (UnderlyingType != null)
            {
                string methodName;
                if (TryGetAttributeString(typeof(XamlSetTypeConverterAttribute), out methodName))
                {
                    if (string.IsNullOrEmpty(methodName))
                    {
                        return null;
                    }
                    return (EventHandler<XamlSetTypeConverterEventArgs>)SafeReflectionInvoker.CreateDelegate(
                        typeof(EventHandler<XamlSetTypeConverterEventArgs>), UnderlyingType, methodName);
                }
            }
            if (BaseType != null)
            {
                return BaseType.SetTypeConverterHandler;
            }
            return null;
        }

        #endregion

        private bool AreAttributesAvailable
        {
            get
            {
                EnsureReflector();

                // Make sure that AttributeProvider is initialized
                // Note: Don't short-circuit the AttributeProvider lookup, even if UnderlyingType
                // is non-null; a derived class can use AttributeProvider to override attribute lookup
                if (!_reflector.CustomAttributeProviderIsSet)
                {
                    _reflector.CustomAttributeProvider = LookupCustomAttributeProvider();
                    Debug.Assert(UnderlyingTypeInternal.IsSet, "EnsureReflector should have caused UnderlyingType to be initialized");
                }
                return _reflector.CustomAttributeProvider != null || UnderlyingTypeInternal.Value != null;
            }
        }

        private BindingFlags ConstructorBindingFlags
        {
            get
            {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
                if (!IsPublic)
                {
                    flags |= BindingFlags.NonPublic;
                }
                return flags;
            }
        }

        private void AppendTypeName(StringBuilder sb, bool forceNsInitialization)
        {
            string ns = null;
            if (forceNsInitialization)
            {
                ns = PreferredXamlNamespace;
            }
            else if (_namespaces != null && _namespaces.Count > 0)
            {
                ns = _namespaces[0];
            }
            if (!string.IsNullOrEmpty(ns))
            {
                sb.Append("{");
                sb.Append(PreferredXamlNamespace);
                sb.Append("}");
            }
            else if (UnderlyingTypeInternal.Value != null)
            {
                sb.Append(UnderlyingTypeInternal.Value.Namespace);
                sb.Append(".");
            }
            sb.Append(Name);
            if (IsGeneric)
            {
                sb.Append("(");
                for (int i = 0; i < TypeArguments.Count; i++)
                {
                    TypeArguments[i].AppendTypeName(sb, forceNsInitialization);
                    if (i < TypeArguments.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
            }
        }

        private void CreateReflector()
        {
            bool isUnknown = LookupIsUnknown();
            TypeReflector reflector;
            if (isUnknown)
            {
                reflector = TypeReflector.UnknownReflector;
            }
            else
            {
                reflector = new TypeReflector(UnderlyingType);
            }
            Interlocked.CompareExchange(ref _reflector, reflector, null);
        }

        // We call this method a lot. Keep it really small, to make sure it inlines.
        private void EnsureReflector()
        {
            if (_reflector == null)
            {
                CreateReflector();
            }
        }

        private XamlCollectionKind GetCollectionKind()
        {
            EnsureReflector();
            if (!_reflector.CollectionKindIsSet)
            {
                _reflector.CollectionKind = LookupCollectionKind();
            }
            return _reflector.CollectionKind;
        }

        private bool GetFlag(BoolTypeBits flagBit)
        {
            EnsureReflector();
            bool? result = _reflector.GetFlag(flagBit);
            if (!result.HasValue)
            {
                result = LookupBooleanValue(flagBit);
                _reflector.SetFlag(flagBit, result.Value);
            }
            return result.Value;
        }

        private XamlMember GetPropertyOrUnknown(string propertyName, bool skipReadOnlyCheck)
        {
            XamlMember result = skipReadOnlyCheck ? LookupMember(propertyName, true) : GetMember(propertyName);
            if (result == null)
            {
                result = new XamlMember(propertyName, this /*declaringType*/, false /*isAttachable*/);
            }
            return result;
        }

        private static bool GetDefaultFlag(BoolTypeBits flagBit)
        {
            return (BoolTypeBits.Default & flagBit) == flagBit;
        }

        private IEnumerable<ConstructorInfo> GetPublicAndInternalConstructors()
        {
            foreach (ConstructorInfo ctor in UnderlyingType.GetConstructors(ConstructorBindingFlags))
            {
                if (TypeReflector.IsPublicOrInternal(ctor))
                {
                    yield return ctor;
                }
            }
        }

        internal static ReadOnlyCollection<T> GetReadOnly<T>(IList<T> list)
        {
            if (list == null)
            {
                return null;
            }
            if (list.Count > 0)
            {
                return new ReadOnlyCollection<T>(list);
            }
            return EmptyList<T>.Value;
        }

        private static ReadOnlyCollection<XamlType> GetTypeArguments(IList<XamlType> typeArguments)
        {
            if (typeArguments == null || typeArguments.Count == 0)
            {
                return null;
            }
            foreach (XamlType typeArg in typeArguments)
            {
                if (typeArg == null)
                {
                    throw new ArgumentException(SR.Get(SRID.CollectionCannotContainNulls, "typeArguments"));
                }
            }
            return new List<XamlType>(typeArguments).AsReadOnly();
        }

        private static ReadOnlyCollection<XamlType> GetTypeArguments(Type type, XamlSchemaContext schemaContext)
        {
            Type genericType = type;
            while (genericType.IsArray)
            {
                genericType = genericType.GetElementType();
            }
            if (!genericType.IsGenericType)
            {
                return null;
            }
            Type[] types = genericType.GetGenericArguments();
            XamlType[] result = new XamlType[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                result[i] = schemaContext.GetXamlType(types[i]);
            }
            return GetReadOnly(result);
        }

        private static string GetTypeName(Type type)
        {
            string typeName = type.Name;
            // if generic, extract the part without the grave quote and arity
            int index = typeName.IndexOf(KnownStrings.GraveQuote);
            if (index >= 0)
            {
                // save the subscript
                string subscript;
                typeName = GenericTypeNameScanner.StripSubscript(typeName, out subscript);
                typeName = typeName.Substring(0, index) + subscript;
            }
            // if nested, add the containing name
            if (type.IsNested)
            {
                typeName = GetTypeName(type.DeclaringType) + KnownStrings.NestedTypeDelimiter + typeName;
            }
            return typeName;
        }

        private bool IsNullableGeneric()
        {
            return UnderlyingType != null
                && (KS.Eq(UnderlyingType.Name, KnownStrings.NullableOfT)
                && UnderlyingType.Assembly == typeof(Nullable<>).Assembly
                && UnderlyingType.Namespace == typeof(Nullable<>).Namespace);
        }

        private ICollection<XamlMember> LookupAllExcludedReadOnlyMembers()
        {
            if (UnderlyingType == null)
            {
                return null;
            }
            
            // Force the list of all members to populate
            ICollection<XamlMember> allMembers = GetAllMembers();

            // By default, any properties remaining will be excluded read-only members
            IList<PropertyInfo> excludedMembers = _reflector.LookupRemainingProperties();
            if (excludedMembers == null)
            {
                return null;
            }
            List<XamlMember> result = new List<XamlMember>(excludedMembers.Count);
            for (int i = 0; i < excludedMembers.Count; i++)
            {
                XamlMember excludedMember = new XamlMember(excludedMembers[i], SchemaContext);
                if (excludedMember.IsReadOnly && !excludedMember.Type.IsUsableAsReadOnly)
                {
                    result.Add(excludedMember);
                }
                else
                {
                    Debug.Assert(this.GetType() != typeof(XamlType), "Default GetAllMembers logic should have already captured all writeable properties");
                }
            }
            return new ReadOnlyCollection<XamlMember>(result);
        }

        private Dictionary<int, IList<XamlType>> LookupAllPositionalParameters()
        {
            // We need to have a consistent ordering for duplicate arity, and remove workaround for TypeExtension
            // Total workaround to work around TypeExtension having two, single argument
            // Constructors.  If the type is TypeExtension then we hard code the right answer.
            if (UnderlyingType == XamlLanguage.Type.UnderlyingType)
            {
                Dictionary<int, IList<XamlType>> result = new Dictionary<int,IList<XamlType>>();
                XamlType typeOfType = SchemaContext.GetXamlType(typeof(Type));
                XamlType[] typeVector = new XamlType[] { typeOfType };
                result.Add(1, GetReadOnly(typeVector));
                return result;
            }

            Dictionary<int, IList<XamlType>> ctorDict = new Dictionary<int, IList<XamlType>>();
            foreach (ConstructorInfo info in GetConstructors())
            {
                ParameterInfo[] parameterInfos = info.GetParameters();
                XamlType[] typeVector = new XamlType[parameterInfos.Length];
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    ParameterInfo param = parameterInfos[i];
                    Type type = param.ParameterType;
                    XamlType xamlType = SchemaContext.GetXamlType(type);
                    typeVector[i] = xamlType;
                }
                if (ctorDict.ContainsKey(typeVector.Length))
                {
                    if (!SchemaContext.SupportMarkupExtensionsWithDuplicateArity)
                    {
                        throw new XamlSchemaException(SR.Get(SRID.MarkupExtensionWithDuplicateArity, UnderlyingType, typeVector.Length));
                    }
                    // Otherwise we just ignore the dupe
                }
                else
                {
                    ctorDict.Add(typeVector.Length, GetReadOnly(typeVector));
                }
            }
            return ctorDict;
        }

        private bool LookupBooleanValue(BoolTypeBits typeBit)
        {
            bool bit;
            switch (typeBit)
            {
                case BoolTypeBits.Constructible:
                    bit = LookupIsConstructible();
                    break;

                case BoolTypeBits.ConstructionRequiresArguments:
                    bit = LookupConstructionRequiresArguments();
                    break;

                case BoolTypeBits.MarkupExtension:
                    bit = LookupIsMarkupExtension();
                    break;

                case BoolTypeBits.Nullable:
                    bit = LookupIsNullable();
                    break;

                case BoolTypeBits.NameScope:
                    bit = LookupIsNameScope();
                    break;

                case BoolTypeBits.Public:
                    bit = LookupIsPublic();
                    break;

                case BoolTypeBits.TrimSurroundingWhitespace:
                    bit = LookupTrimSurroundingWhitespace();
                    break;

                case BoolTypeBits.UsableDuringInitialization:
                    bit = LookupUsableDuringInitialization();
                    if (bit && IsMarkupExtension)
                    {
                        // MarkupExtension cannot be used during initialization.
                        string err = SR.Get(SRID.UsableDuringInitializationOnME, this);
                        throw new XamlSchemaException(err);
                    }
                    break;

                case BoolTypeBits.WhitespaceSignificantCollection:
                    bit = LookupIsWhitespaceSignificantCollection();
                    break;

                case BoolTypeBits.XmlData:
                    bit = LookupIsXData();
                    break;

                case BoolTypeBits.Ambient:
                    bit = LookupIsAmbient();
                    break;

                default:
                    Debug.Fail("Enum out of range");
                    bit = GetDefaultFlag(typeBit);
                    break;
            }
            return bit;
        }

        // Walks up the inheritance hierarchy to find the string contents of an attribute.
        // Returns true if the entire type hierarchy was walked successfully, false if not
        //   (e.g. because one of the base types doesn't have an underlying CLR type).
        // Result is null if attribute wasn't found, string.Empty if attribute string was null or empty.
        //
        // The result from this function is typically used to look up a member on the decorated type.
        // This poses a challenge: if the attribute is defined on a base type, and a derived type
        //   shadows the named member, which do we return?
        // v3 returned that derived member. Also, some attribute lookup methods (e.g. TypeDescriptor)
        //   always coalesce inheritance hierarchy, which would make it difficult to determine 
        //   whether the attribute was defined on a base or derived class.
        // So, for consistency and compat, we coalesce the hierarchy here, so the caller will always
        //   return the derived member.
        private bool TryGetAttributeString(Type attributeType, out string result)
        {
            if (!AreAttributesAvailable)
            {
                result = null;
                return false;
            }
            Debug.Assert(_reflector != null, "_reflector should have been initialized by AreAttributesAvailable");

            // Look up the attribute on this type
            bool checkedInherited;
            result = _reflector.GetAttributeString(attributeType, out checkedInherited);
            if (checkedInherited || result != null)
            {
                return true;
            }

            // Look up the attribute on our base type
            XamlType baseType = BaseType;
            if (baseType != null)
            {
                return baseType.TryGetAttributeString(attributeType, out result);
            }

            // We've reached the root of the inheritance tree
            return true;
        }

        #region IEquatable<XamlType> Members

        public override bool Equals(object obj)
        {
            XamlType objXamlType = obj as XamlType;
            return this == objXamlType;
        }

        public override int GetHashCode()
        {
            if (IsUnknown)
            {
                Debug.Assert(_namespaces != null && _namespaces.Count > 0);
                int result = _name.GetHashCode() ^ _namespaces[0].GetHashCode();
                if (_typeArguments != null && _typeArguments.Count > 0)
                {
                    foreach (XamlType typeArgument in _typeArguments)
                    {
                        result ^= typeArgument.GetHashCode();
                    }
                }
                return result;
            }
            else if (UnderlyingType != null)
            {
                // flip one bit on the hash to avoid collisions with the underlying type
                return UnderlyingType.GetHashCode() ^ 8;
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public bool Equals(XamlType other)
        {
            return this == other;
        }

        // We need to either provide overridability here, or else provide a
        // name/namespace fallback for known non-CLR types
        public static bool operator ==(XamlType xamlType1, XamlType xamlType2)
        {
            if (object.ReferenceEquals(xamlType1, xamlType2))
            {
                return true;
            }
            if (object.ReferenceEquals(xamlType1, null) || object.ReferenceEquals(xamlType2, null))
            {
                return false;
            }
            if (xamlType1.IsUnknown)
            {
                if (xamlType2.IsUnknown)
                {
                    Debug.Assert(xamlType1._namespaces != null && xamlType1._namespaces.Count > 0);
                    Debug.Assert(xamlType2._namespaces != null && xamlType2._namespaces.Count > 0);
                    return (xamlType1._name == xamlType2._name) &&
                        (xamlType1._namespaces[0] == xamlType2._namespaces[0]) &&
                        typeArgumentsAreEqual(xamlType1, xamlType2);
                }
                return false;
            }
            else if (xamlType2.IsUnknown)
            {
                return false;
            }
            // If the types are known but don't have underlying types, this will return false.
            // We don't want to get into the business of comparing custom user types, especially
            // since the one way we could possibly do that (namespaces) can have side effects
            return xamlType1.UnderlyingType == xamlType2.UnderlyingType;
        }

        public static bool operator !=(XamlType xamlType1, XamlType xamlType2)
        {
            return !(xamlType1 == xamlType2);
        }

        private static bool typeArgumentsAreEqual(XamlType xamlType1, XamlType xamlType2)
        {
            Debug.Assert(xamlType1.IsUnknown && xamlType2.IsUnknown);
            if (!xamlType1.IsGeneric)
            {
                return !xamlType2.IsGeneric;
            }
            else if (!xamlType2.IsGeneric)
            {
                return false;
            }
            if (xamlType1._typeArguments.Count != xamlType2._typeArguments.Count)
            {
                return false;
            }
            for (int i = 0; i < xamlType1._typeArguments.Count; i++)
            {
                if (xamlType1._typeArguments[i] != xamlType2._typeArguments[i])
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        internal static class EmptyList<T>
        {
            public static readonly ReadOnlyCollection<T> Value =
                new ReadOnlyCollection<T>(new T[0]);
        }
    }
}
