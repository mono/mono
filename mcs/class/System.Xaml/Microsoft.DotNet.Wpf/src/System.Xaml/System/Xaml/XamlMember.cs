// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Xaml.Schema;
using System.Windows.Markup;
using System.Security;
using System.Xaml;
using System.Xaml.MS.Impl;
using MS.Internal.Xaml.Parser;

namespace System.Xaml
{
    /// <SecurityNote>
    /// This class is extensible; various members which could be used for visibility evaluation--
    /// IsReadPublic, IsWritePublic, DeclaringType--get their data either from virtual methods or
    /// from constructor arguments.
    /// For security-critical data, always check the underlying CLR member.
    /// </SecurityNote>
    public class XamlMember : IEquatable<XamlMember>
    {
        // Initialized in constructor
        private string _name;
        private XamlType _declaringType;
        private MemberType _memberType;

        // Idempotent
        private ThreeValuedBool _isNameValid;

        // Thread safety: if setting outside ctor, do an interlocked compare against null
        private MemberReflector _reflector;

        /// <summary>
        /// Lazy init: NullableReference.IsSet is null when not initialized
        /// </summary>
        /// <SecurityNote>
        /// Critical: Ensuring idempotence for consistency with UnderlyingGetter/Setter
        /// </SecurityNote>
        [SecurityCritical]
        private NullableReference<MemberInfo> _underlyingMember;

        public XamlMember(string name, XamlType declaringType, bool isAttachable)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }

            _name = name;
            _declaringType = declaringType;
            _memberType = isAttachable ? MemberType.Attachable : MemberType.Instance;
        }

        // Known property
        public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext)
            :this(propertyInfo, schemaContext, null)
        {
        }

        public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
            : this(propertyInfo, schemaContext, invoker, new MemberReflector(false /*isEvent*/))
        {
        }

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingMember
        /// Safe: Constructor is single-threaded, so idempotence is assured
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _name = propertyInfo.Name;
            _declaringType = schemaContext.GetXamlType(propertyInfo.DeclaringType);
            _memberType = MemberType.Instance;
            _reflector = reflector;
            _reflector.Invoker = invoker;
            _underlyingMember.Value = propertyInfo;
        }

        // Known event
        public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext)
            :this(eventInfo, schemaContext, null)
        {
        }

        public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
            :this(eventInfo, schemaContext, invoker, new MemberReflector(true /*isEvent*/))
        {
        }

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingMember
        /// Safe: Constructor is single-threaded, so idempotence is assured
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (eventInfo == null)
            {
                throw new ArgumentNullException("eventInfo");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            _name = eventInfo.Name;
            _declaringType = schemaContext.GetXamlType(eventInfo.DeclaringType);
            _memberType = MemberType.Instance;
            _reflector = reflector;
            _reflector.Invoker = invoker;
            _underlyingMember.Value = eventInfo;
        }

        // Known attachable property
        public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter,
            XamlSchemaContext schemaContext)
            :this(attachablePropertyName, getter, setter, schemaContext, null)
        {
        }

        public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter,
            XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
            :this(attachablePropertyName, getter, setter, schemaContext, invoker, new MemberReflector(getter, setter, false /*isEvent*/))
        {
        }

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingMember
        /// Safe: Constructor is single-threaded, so idempotence is assured
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter,
            XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (attachablePropertyName == null)
            {
                throw new ArgumentNullException("attachablePropertyName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            MethodInfo accessor = getter ?? setter;
            if (accessor == null)
            {
                throw new ArgumentNullException(SR.Get(SRID.GetterOrSetterRequired), (Exception)null);
            }
            ValidateGetter(getter, "getter");
            ValidateSetter(setter, "setter");

            _name = attachablePropertyName;
            _declaringType = schemaContext.GetXamlType(accessor.DeclaringType);
            _reflector = reflector;
            _memberType = MemberType.Attachable;
            _reflector.Invoker = invoker;
            _underlyingMember.Value = getter ?? setter;
        }

        // Known attachable event
        public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext)
            :this(attachableEventName, adder, schemaContext, null)
        {
        }

        public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext,
            XamlMemberInvoker invoker)
            : this(attachableEventName, adder, schemaContext, invoker, new MemberReflector(null, adder, true /*isEvent*/))
        {
        }

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingMember
        /// Safe: Constructor is single-threaded, so idempotence is assured
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext,
            XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (attachableEventName == null)
            {
                throw new ArgumentNullException("attachableEventName");
            }
            if (adder == null)
            {
                throw new ArgumentNullException("adder");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            ValidateSetter(adder, "adder");

            _name = attachableEventName;
            _declaringType = schemaContext.GetXamlType(adder.DeclaringType);
            _reflector = reflector;
            _memberType = MemberType.Attachable;
            _reflector.Invoker = invoker;
            _underlyingMember.Value = adder;
        }

        // Directive, only called from XamlDirective
        internal XamlMember(string name, MemberReflector reflector)
        {
            _name = name;
            _declaringType = null;
            _reflector = reflector ?? MemberReflector.UnknownReflector;
            _memberType = MemberType.Directive;
        }

        public XamlType DeclaringType { get { return _declaringType; } }

        public XamlMemberInvoker Invoker
        {
            get
            {
                EnsureReflector();
                if (_reflector.Invoker == null)
                {
                    _reflector.Invoker = LookupInvoker() ?? XamlMemberInvoker.UnknownInvoker;
                }
                return _reflector.Invoker;
            }
        }

        public bool IsUnknown
        {
            get
            {
                EnsureReflector();
                return (_reflector.IsUnknown);
            }
        }

        public bool IsReadPublic
        {
            get { return IsReadPublicIgnoringType && (_declaringType == null || _declaringType.IsPublic); }
        }

        public bool IsWritePublic
        {
            get { return IsWritePublicIgnoringType && (_declaringType == null || _declaringType.IsPublic); }
        }

        public string Name { get { return _name; } }

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

        public XamlType TargetType
        {
            get
            {
                if (!IsAttachable)
                {
                    return _declaringType;
                }
                EnsureReflector();
                if (_reflector.TargetType == null)
                {
                    if (_reflector.IsUnknown)
                    {
                        return XamlLanguage.Object;
                    }
                    _reflector.TargetType = LookupTargetType() ?? XamlLanguage.Object;
                }
                return _reflector.TargetType;
            }
        }

        public XamlType Type
        {
            get
            {
                EnsureReflector();
                if (_reflector.Type == null)
                {
                    _reflector.Type = LookupType() ?? XamlLanguage.Object;
                }
                return _reflector.Type;
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

        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingMember
        /// Safe: Ensures idempotence via NullableReference.SetIfNull, which uses CompareExchange
        /// </SecurityNote>
        public MemberInfo UnderlyingMember
        {
            [SecuritySafeCritical]
            get
            {
                if (!_underlyingMember.IsSet)
                {
                    _underlyingMember.SetIfNull(LookupUnderlyingMember());
                }
                return _underlyingMember.Value;
            }
        }

        /// <summary>Accesses _underlyingMember without initializing it</summary>
        /// <SecurityNote>
        /// Critical: Accesses critical field _underlyingMember
        /// Safe: Doesn't modify field. Field is value type so caller cannot modify it.
        /// </SecurityNote>
        internal NullableReference<MemberInfo> UnderlyingMemberInternal
        {
            [SecuritySafeCritical]
            get { return _underlyingMember; }
        }

        public bool IsReadOnly
        {
            get { return GetFlag(BoolMemberBits.ReadOnly); }
        }

        public bool IsWriteOnly
        {
            get { return GetFlag(BoolMemberBits.WriteOnly); }
        }

        public bool IsAttachable
        {
            get { return _memberType == MemberType.Attachable; }
        }

        public bool IsEvent
        {
            get { return GetFlag(BoolMemberBits.Event); }
        }

        public bool IsDirective { get { return _memberType == MemberType.Directive; } }

        public virtual IList<string> GetXamlNamespaces()
        {
            return DeclaringType.GetXamlNamespaces();
        }

        public override string ToString()
        {
            Debug.Assert(_declaringType != null, "XamlDirective should not call base.ToString");
            return _declaringType.ToString() + "." + Name;
        }

        public IList<XamlMember> DependsOn
        {
            get
            {
                EnsureReflector();
                if (_reflector.DependsOn == null)
                {
                    _reflector.DependsOn = LookupDependsOn() ?? XamlType.EmptyList<XamlMember>.Value;
                }
                return _reflector.DependsOn;
            }
        }

        public bool IsAmbient
        {
            get { return GetFlag(BoolMemberBits.Ambient); }
        }

        public DesignerSerializationVisibility SerializationVisibility
        {
            get
            {
                EnsureReflector();
                if (!_reflector.DesignerSerializationVisibilityIsSet)
                {
                    _reflector.SerializationVisibility = LookupSerializationVisibility();
                }
                return _reflector.SerializationVisibility ?? DesignerSerializationVisibility.Visible;
            }
        }

        /// <summary>
        /// Returns the value of the MarkupExtensionBracketCharacterAttribute set on
        /// a property of a MarkupExtension as a ReadOnlyDictionary. Opening bracket is the
        /// key, while the value is the closing bracket.
        /// </summary>
        public IReadOnlyDictionary<char,char> MarkupExtensionBracketCharacters
        {
            get
            {
                EnsureReflector();
                if (!_reflector.MarkupExtensionBracketCharactersArgumentIsSet)
                {
                    _reflector.MarkupExtensionBracketCharactersArgument = LookupMarkupExtensionBracketCharacters();
                    _reflector.MarkupExtensionBracketCharactersArgumentIsSet = true;
                }

                return _reflector.MarkupExtensionBracketCharactersArgument;
            }
        }

        // Note: distinguishes between null (unset) and string.Empty (explicitly set to empty or null)
        internal string ConstructorArgument
        {
            get
            {
                EnsureReflector();
                if (!_reflector.ConstructorArgumentIsSet)
                {
                    _reflector.ConstructorArgument = LookupConstructorArgument();
                }
                return _reflector.ConstructorArgument;
            }
        }

        // Requires live reflection - only use from XOW/XOR
        internal object DefaultValue
        {
            get
            {
                EnsureDefaultValue();
                return _reflector.DefaultValue;
            }
        }

        internal MethodInfo Getter
        {
            get
            {
                EnsureReflector();
                if (!_reflector.GetterIsSet)
                {
                    _reflector.Getter = LookupUnderlyingGetter();
                }
                return _reflector.Getter;
            }
        }

        // Requires live reflection - only use from XOW/XOR
        internal bool HasDefaultValue
        {
            get
            {
                EnsureDefaultValue();
                return !_reflector.DefaultValueIsNotPresent;
            }
        }

        internal bool HasSerializationVisibility
        {
            get
            {
                EnsureReflector();
                if (!_reflector.DesignerSerializationVisibilityIsSet)
                {
                    _reflector.SerializationVisibility = LookupSerializationVisibility();
                }
                return _reflector.SerializationVisibility.HasValue;
            }
        }

        internal MethodInfo Setter
        {
            get
            {
                EnsureReflector();
                if (!_reflector.SetterIsSet)
                {
                    _reflector.Setter = LookupUnderlyingSetter();
                }
                return _reflector.Setter;
            }
        }

        // Security note:
        // Keep this internal so that people don't use it for real security decisions.
        // This is only for convenience filtering, we still depend on the CLR for our real security.
        //
        // Usage note: Assumes the declaring type is visible and is public or internal
        //
        // Extensibility note:
        // This is not overridable since it does not make sense in a non-CLR context.
        internal bool IsReadVisibleTo(Assembly accessingAssembly, Type accessingType)
        {
            if (IsReadPublicIgnoringType)
            {
                return true;
            }
            MethodInfo getter = Getter;
            if (getter != null)
            {
                return MemberReflector.GenericArgumentsAreVisibleTo(getter, accessingAssembly, SchemaContext) &&
                    (MemberReflector.IsInternalVisibleTo(getter, accessingAssembly, SchemaContext) ||
                    MemberReflector.IsProtectedVisibleTo(getter, accessingType, SchemaContext));
            }
            return false;
        }

        // See notes on IsReadVisibleTo
        internal bool IsWriteVisibleTo(Assembly accessingAssembly, Type accessingType)
        {
            if (IsWritePublicIgnoringType)
            {
                return true;
            }
            MethodInfo setter = Setter;
            if (setter != null)
            {
                return MemberReflector.GenericArgumentsAreVisibleTo(setter, accessingAssembly, SchemaContext) && 
                    (MemberReflector.IsInternalVisibleTo(setter, accessingAssembly, SchemaContext) ||
                    MemberReflector.IsProtectedVisibleTo(setter, accessingType, SchemaContext));
            }
            return false;
        }

        // If any new virtuals are added below, be sure to also override them in XamlDirective

        protected virtual XamlMemberInvoker LookupInvoker()
        {
            if (UnderlyingMember != null)
            {
                return new XamlMemberInvoker(this);
            }
            return null;
        }

        protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
        {
            return null;
        }

        protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
        {
            if (AreAttributesAvailable)
            {
                Type[] loaderTypes = _reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
                if (loaderTypes != null)
                {
                    return SchemaContext.GetValueConverter<XamlDeferringLoader>(loaderTypes[0], null);
                }
            }
            if (this.Type != null)
            {
                return this.Type.DeferringLoader;
            }
            return null;
        }

        protected virtual IList<XamlMember> LookupDependsOn()
        {
            if (!AreAttributesAvailable)
            {
                return null;
            }
            List<string> doPropertyNames = _reflector.GetAllAttributeContents<string>(typeof(DependsOnAttribute));
            if (doPropertyNames == null || doPropertyNames.Count == 0)
            {
                return null;
            }

            List<XamlMember> result = new List<XamlMember>();
            foreach (var name in doPropertyNames)
            {
                XamlMember member = _declaringType.GetMember(name);

                // Normally we want to throw if property lookup fails to return anything
                // but here we can not throw because v3.0 does not
                if (member != null)
                {
                    result.Add(member);
                }
            }
            return XamlType.GetReadOnly(result);
        }

        private DesignerSerializationVisibility? LookupSerializationVisibility()
        {
            DesignerSerializationVisibility? result = null;
            if (AreAttributesAvailable)
            {
                result = _reflector.GetAttributeValue<DesignerSerializationVisibility>(
                    typeof(DesignerSerializationVisibilityAttribute));
            }
            return result;
        }

        protected virtual bool LookupIsAmbient()
        {
            if (AreAttributesAvailable)
            {
                return _reflector.IsAttributePresent(typeof(AmbientAttribute));
            }
            return GetDefaultFlag(BoolMemberBits.Ambient);
        }

        protected virtual bool LookupIsEvent()
        {
            return UnderlyingMember is EventInfo;
        }

        // Note: this returns whether the member itself is public or not. The visibility of the
        // declaring type is considered in the IsReadPublic property, not here.
        protected virtual bool LookupIsReadPublic()
        {
            MethodInfo getter = Getter;
            if (getter != null && !getter.IsPublic)
            {
                return false;
            }
            return !IsWriteOnly;
        }

        protected virtual bool LookupIsReadOnly()
        {
            if (UnderlyingMember != null)
            {
                return (Setter == null);
            }
            return GetDefaultFlag(BoolMemberBits.ReadOnly);
        }

        protected virtual bool LookupIsUnknown()
        {
            if (_reflector != null)
            {
                return _reflector.IsUnknown;
            }
            return UnderlyingMember == null;
        }

        protected virtual bool LookupIsWriteOnly()
        {
            if (UnderlyingMember != null)
            {
                return (Getter == null);
            }
            return GetDefaultFlag(BoolMemberBits.WriteOnly);
        }

        // Note: this returns whether the member itself is public or not. The visibility of the
        // declaring type is considered in the IsWritePublic property, not here.
        protected virtual bool LookupIsWritePublic()
        {
            MethodInfo setter = Setter;
            if (setter != null && !setter.IsPublic)
            {
                return false;
            }
            return !IsReadOnly;
        }

        protected virtual XamlType LookupTargetType()
        {
            if (IsAttachable)
            {
                MethodInfo accessor = UnderlyingMember as MethodInfo;
                if (accessor != null)
                {
                    ParameterInfo[] parameters = accessor.GetParameters();
                    if (parameters.Length > 0)
                    {
                        Type result = parameters[0].ParameterType;
                        return SchemaContext.GetXamlType(result);
                    }
                }
                return XamlLanguage.Object;
            }
            return _declaringType;
        }

        protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            XamlValueConverter<TypeConverter> result = null;
            if (AreAttributesAvailable)
            {
                Type converterType = _reflector.GetAttributeType(typeof(TypeConverterAttribute));
                if (converterType != null)
                {
                    result = SchemaContext.GetValueConverter<TypeConverter>(converterType, null);
                }
            }
            if (result == null && this.Type != null)
            {
                result = this.Type.TypeConverter;
            }

            return result;
        }

        protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer()
        {
            XamlValueConverter<ValueSerializer> result = null;
            if (AreAttributesAvailable)
            {
                Type converterType = _reflector.GetAttributeType(typeof(ValueSerializerAttribute));
                if (converterType != null)
                {
                    result = SchemaContext.GetValueConverter<ValueSerializer>(converterType, null);
                }
            }
            if (result == null && this.Type != null)
            {
                result = this.Type.ValueSerializer;
            }
            return result;
        }

        /// <summary>
        /// Returns the value of the MarkupExtensionBracketCharacterAttribute set on
        /// a property of a MarkupExtension as a ReadOnlyDictionary. Opening bracket is the
        /// key, while the value is the closing bracket.
        /// </summary>
        protected virtual IReadOnlyDictionary<char,char> LookupMarkupExtensionBracketCharacters()
        {
            if (AreAttributesAvailable)
            {
                IReadOnlyDictionary<char, char> bracketCharactersList = _reflector.GetBracketCharacterAttributes(typeof(MarkupExtensionBracketCharactersAttribute));
                if (bracketCharactersList != null)
                {
                    _reflector.MarkupExtensionBracketCharactersArgument = bracketCharactersList;
                }
            }
            return _reflector.MarkupExtensionBracketCharactersArgument;
        }

        protected virtual XamlType LookupType()
        {
            Type systemType = LookupSystemType();
            return (systemType != null) ? SchemaContext.GetXamlType(systemType) : null;
        }

        protected virtual MethodInfo LookupUnderlyingGetter()
        {
            EnsureReflector();
            // Through normal paths, _reflector.Getter should always be null here.
            // But a user could always call this protected method directly, so check just in case
            if (_reflector.Getter != null)
            {
                return _reflector.Getter;
            }
            PropertyInfo pi = UnderlyingMember as PropertyInfo;
            return (pi != null) ? pi.GetGetMethod(true) : null;
        }

        protected virtual MethodInfo LookupUnderlyingSetter()
        {
            EnsureReflector();
            // Through normal paths, _reflector.Setter should always be null here.
            // But a user could always call this protected method directly, so check just in case
            if (_reflector.Setter != null)
            {
                return _reflector.Setter;
            }
            PropertyInfo pi = UnderlyingMember as PropertyInfo;
            if (pi != null)
            {
                return pi.GetSetMethod(true);
            }
            else
            {
                EventInfo ei = UnderlyingMember as EventInfo;
                return (ei != null) ? ei.GetAddMethod(true) : null;
            }
        }

        protected virtual MemberInfo LookupUnderlyingMember()
        {
            // If UnderlyingMember wasn't set in ctor, this will return null.
            // If UnderlyingMember was set in ctor, it shouldn't be necessary to call this
            // (UnderlyingMember property will already be set), but returning the correct
            // value here just in case a users calls it anyway.
            return UnderlyingMemberInternal.Value;
        }

        private bool IsReadPublicIgnoringType
        {
            get
            {
                EnsureReflector();
                bool? result = _reflector.GetFlag(BoolMemberBits.ReadPublic);
                if (!result.HasValue)
                {
                    result = LookupIsReadPublic();
                    _reflector.SetFlag(BoolMemberBits.ReadPublic, result.Value);
                }
                return result.Value;
            }
        }

        private bool IsWritePublicIgnoringType
        {
            get
            {
                EnsureReflector();
                bool? result = _reflector.GetFlag(BoolMemberBits.WritePublic);
                if (!result.HasValue)
                {
                    result = LookupIsWritePublic();
                    _reflector.SetFlag(BoolMemberBits.WritePublic, result.Value);
                }
                return result.Value;
            }
        }

        private static void ValidateGetter(MethodInfo method, string argumentName)
        {
            if (method == null)
            {
                return;
            }
            if ((method.GetParameters().Length != 1) || (method.ReturnType == typeof(void)))
            {
                throw new ArgumentException(SR.Get(SRID.IncorrectGetterParamNum), argumentName);
            }
        }

        private static void ValidateSetter(MethodInfo method, string argumentName)
        {
            if ((method != null) && (method.GetParameters().Length != 2))
            {
                throw new ArgumentException(SR.Get(SRID.IncorrectSetterParamNum), argumentName);
            }
        }

        // This property needs to be checked before any attribute lookups on _reflector. It's not
        // only informational, it also ensures that the right state is initialized.
        private bool AreAttributesAvailable
        {
            get
            {
                EnsureReflector();

                // Make sure that AttributeProvider is initialized
                // Note: Don't short-circuit the AttributeProvider lookup, even if UnderlyingMember
                // is non-null; a derived class can use AttributeProvider to override attribute lookup

                // Volatile read/write of CustomAttributeProvider to make sure that threads that see
                // CustomAttributeProviderIsSet == true also see the write to _reflector.UnderlyingMember
                if (!_reflector.CustomAttributeProviderIsSetVolatile)
                {
                    ICustomAttributeProvider attrProvider = LookupCustomAttributeProvider();
                    if (attrProvider == null)
                    {
                        // Set the member that _reflector will use. Note this also ensures that 
                        // _underlyingMember is initialized, so it's safe to access the field directly below.
                        _reflector.UnderlyingMember = UnderlyingMember;
                    }
                    _reflector.SetCustomAttributeProviderVolatile(attrProvider);
                }
                return _reflector.CustomAttributeProvider != null || UnderlyingMemberInternal.Value != null;
            }
        }

        private XamlSchemaContext SchemaContext { get { return _declaringType.SchemaContext; } }

        private static bool GetDefaultFlag(BoolMemberBits flagBit)
        {
            return (BoolMemberBits.Default & flagBit) == flagBit;
        }

        private void CreateReflector()
        {
            bool isUnknown = LookupIsUnknown();
            MemberReflector reflector = isUnknown ? MemberReflector.UnknownReflector : new MemberReflector();
            Interlocked.CompareExchange(ref _reflector, reflector, null);
        }

        private void EnsureDefaultValue()
        {
            EnsureReflector();
            if (!_reflector.DefaultValueIsSet)
            {
                DefaultValueAttribute defaultValueAttrib = null;
                // Unlike other component-model attributes, DefaultValueAttribute is unsealed, and the 
                // Value property is virtual. So we cannot reliably process DefaultValueAttribute in ROL. 
                // The DefaultValue property is internal and is only called from XamlObjectReader, so it
                // is safe to use live reflection.
                if (AreAttributesAvailable)
                {
                    ICustomAttributeProvider attributeProvider = _reflector.CustomAttributeProvider ?? UnderlyingMember;
                    object[] attribs = attributeProvider.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                    if (attribs.Length > 0)
                    {
                        defaultValueAttrib = (DefaultValueAttribute)attribs[0];
                    }
                }
                if (defaultValueAttrib != null)
                {
                    _reflector.DefaultValue = defaultValueAttrib.Value;
                }
                else
                {
                    _reflector.DefaultValueIsNotPresent = true;
                }
            }
        }

        // We call this method a lot. Keep it really small, to make sure it inlines.
        private void EnsureReflector()
        {
            if (_reflector == null)
            {
                CreateReflector();
            }
        }

        private bool GetFlag(BoolMemberBits flagBit)
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

        private bool LookupBooleanValue(BoolMemberBits flag)
        {
            bool result;
            switch (flag)
            {
                case BoolMemberBits.Ambient:
                    result = LookupIsAmbient();
                    break;
                case BoolMemberBits.Event:
                    result = LookupIsEvent();
                    break;
                case BoolMemberBits.ReadOnly:
                    result = LookupIsReadOnly();
                    break;
                case BoolMemberBits.ReadPublic:
                    result = LookupIsReadPublic();
                    break;
                case BoolMemberBits.WriteOnly:
                    result = LookupIsWriteOnly();
                    break;
                case BoolMemberBits.WritePublic:
                    result = LookupIsWritePublic();
                    break;
                default:
                    Debug.Fail("Enum out of range");
                    result = GetDefaultFlag(flag);
                    break;
            }
            return result;
        }

        private string LookupConstructorArgument()
        {
            string result = null;
            if (AreAttributesAvailable)
            {
                bool checkedInherited;
                result = _reflector.GetAttributeString(typeof(ConstructorArgumentAttribute), out checkedInherited);
            }
            return result;
        }

        private Type LookupSystemType()
        {
            MemberInfo underlyingMember = UnderlyingMember;
            PropertyInfo pi = underlyingMember as PropertyInfo;
            if (pi != null)
            {
                return pi.PropertyType;
            }
            EventInfo ei = underlyingMember as EventInfo;
            if (ei != null)
            {
                return ei.EventHandlerType;
            }
            MethodInfo mi = underlyingMember as MethodInfo;
            if (mi != null)
            {
                if (mi.ReturnType != null && mi.ReturnType != typeof(void))
                {
                    return mi.ReturnType;
                }
                ParameterInfo[] parameters = mi.GetParameters();
                if (parameters.Length == 2)
                {
                    return parameters[1].ParameterType;
                }
            }
            return null;
        }

        #region IEquatable<XamlMember> Members

        public override bool Equals(object obj)
        {
            XamlMember member = obj as XamlMember;
            return this == member;
        }

        public override int GetHashCode()
        {
            Debug.Assert(DeclaringType != null, "XamlDirective should not call into base.GetHashCode");
            return (Name == null ?  0 : Name.GetHashCode()) ^ (int)_memberType ^ DeclaringType.GetHashCode();
        }

        public bool Equals(XamlMember other)
        {
            return this == other;
        }

        public static bool operator ==(XamlMember xamlMember1, XamlMember xamlMember2)
        {
            if (object.ReferenceEquals(xamlMember1, xamlMember2))
            {
                return true;
            }
            if (object.ReferenceEquals(xamlMember1, null) || object.ReferenceEquals(xamlMember2, null))
            {
                return false;
            }
            if (xamlMember1._memberType != xamlMember2._memberType || xamlMember1.Name != xamlMember2.Name)
            {
                return false;
            }
            if (xamlMember1.IsDirective)
            {
                Debug.Assert(xamlMember2.IsDirective);
                // DeclaringType is null for directives, so we need to compare namespaces.
                // Known and unknown directives are equal if the names and namespaces match
                return XamlDirective.NamespacesAreEqual((XamlDirective)xamlMember1, (XamlDirective)xamlMember2);
            }
            else
            {
                // Known and unknown members are not equal, even if they otherwise match
                return xamlMember1.DeclaringType == xamlMember2.DeclaringType &&
                    xamlMember1.IsUnknown == xamlMember2.IsUnknown;
            }
        }

        public static bool operator !=(XamlMember xamlMember1, XamlMember xamlMember2)
        {
            return !(xamlMember1 == xamlMember2);
        }

        #endregion

        enum MemberType : byte
        {
            Instance,
            Attachable,
            Directive
        }
    }
}
