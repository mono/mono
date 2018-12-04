using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.MS.Impl;
#else
using System.Xaml.MS.Impl;
#endif

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml.Schema
#else
namespace System.Xaml.Schema
#endif
{
    [DebuggerDisplay("{Name}")]
    class ClrProperty : XamlProperty
    {
        public readonly PropertyInfo ClrBindingPropertyInfo;


        private readonly XamlType _declaringType;
        private readonly string _name;
        protected bool _isPublic;
        protected bool _isReadOnly;
        protected bool _isStatic;
        protected bool _isAttachable;
        protected bool _isEvent;

        private Type _systemTypeOfProperty;
        protected XamlType _xamlTypeOfProperty;
        private XamlTextSyntax _textSyntax;
        private XamlProperty _dependsOn;
        private XamlType _targetType;

        private int _cachedPropertyBits;
        private int _validPropertyBits;
        private int _nonBoolValidPropertyBits;

        protected ClrProperty(string name, XamlType declaringType)
        {
#if DEBUG
            if (declaringType == null)
            {
                throw new XamlInternalException("Asserting that the declaringType should never be null");
            }
#endif
            _name = name;
            _declaringType = declaringType;

            OtherInitialization();
        }

        public ClrProperty(string name, PropertyInfo pi, XamlType declaringType)
            : this(name, pi, declaringType, false)
        {
        }

        private ClrProperty(string name, PropertyInfo pi, XamlType declaringType, bool isStatic)
        {
            Debug.Assert(pi != null);
            Debug.Assert(pi.Name == name);
#if DEBUG
            if (declaringType == null)
            {
                throw new XamlInternalException("Asserting that the declaringType should never be null");
            }
#endif
            MethodInfo mi = pi.GetGetMethod(true);
            if (mi == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.SetOnlyProperty, declaringType.Name, name));
            }

            _declaringType = declaringType;
            _name = pi.Name;
            _isPublic = mi.IsPublic;
            _isReadOnly = !pi.CanWrite;
            _isStatic = isStatic;
            ClrBindingPropertyInfo = pi;

            _isAttachable = false;
            _isEvent = false;

            OtherInitialization();
        }



        private void OtherInitialization()
        {
            _cachedPropertyBits = 0; // bit flags;
            _validPropertyBits = 0;  // bit flags
        }

        // ----- Protected Overrides -----

        public override string BoundName
        {
            get { return Name; }
        }

        public override bool IsImplicit { get { return false; } }
        public override bool IsUnknown { get { return false; } }

        public override IList<string> GetXamlNamespaces()
        {
            return DeclaringType.GetXamlNamespaces();
        }

        protected override string NameCore
        {
            get { return _name; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsPublicCore
        {
            get { return _isPublic; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsBrowsableCore
        {
            get
            {
                bool ret = CheckPropertyBit(BoolPropertyBits.IsBrowsable);
                return ret;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType DeclaringTypeCore
        {
            get { return _declaringType; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType TypeCore
        {
            get
            {
                if (_xamlTypeOfProperty == null)
                {
                    _xamlTypeOfProperty = LookupTypeOfProperty();
                }
                return _xamlTypeOfProperty;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlTextSyntax TextSyntaxCore
        {
            get
            {
                if (_textSyntax == null)
                {
                    _textSyntax = LookupTextSyntax();
                }

                if (_textSyntax == XamlTextSyntax.NoSyntax)
                {
                    return null;
                }
                return _textSyntax;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlProperty DependsOnCore
        {
            get
            {
                if (!CheckNonBoolValidBit(NonBoolPropertyValidBits.DependsOn))
                {
                    SetNonBoolValidBit(NonBoolPropertyValidBits.DependsOn);
                    _dependsOn = LookupDependsOn();
                }
                return _dependsOn;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsReadOnlyCore
        {
            get { return _isReadOnly; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsStaticCore
        {
            get { return _isStatic; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsAttachableCore
        {
            get { return _isAttachable; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return _isEvent; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType TargetTypeCore
        {
            get
            {
                if (!CheckNonBoolValidBit(NonBoolPropertyValidBits.TargetType))
                {
                    SetNonBoolValidBit(NonBoolPropertyValidBits.TargetType);
                    _targetType = LookupTargetType();
                }
                return _targetType;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override AllowedMemberLocation AllowedLocationCore
        {
            get { return AllowedMemberLocation.Any; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsAmbientCore
        {
            get
            {
                bool ret = CheckPropertyBit(BoolPropertyBits.IsAmbient);
                return ret;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsObsoleteCore
        {
            get
            {
                bool ret = CheckPropertyBit(BoolPropertyBits.IsObsolete);
                return ret;
            }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        // ==============================================

        public override string ToString()
        {
            return Name;
        }

        // ==============================================

        public XamlSchemaContext SchemaContext
        {
            get { return _declaringType.SchemaContext; }
        }

        // ==============================================

        internal Type ClrSystemTypeOfProperty
        {
            get
            {
                if (_systemTypeOfProperty == null)
                {
                    _systemTypeOfProperty = LookupSystemTypeOfProperty();
                }
                return _systemTypeOfProperty;
            }
        }

        // ==============================================

        protected virtual Type LookupSystemTypeOfProperty()
        {
            return ClrBindingPropertyInfo.PropertyType;
        }

        private object LookupCustomAttribute(Type attrType)
        {
            object[] objs = LookupCustomAttributes(attrType);
            if (objs.Length == 0)
            {
                return null;
            }
            if (objs.Length > 1)
            {
                string message = SR.Get(SRID.TooManyAttributes, DeclaringType.Name, Name, attrType.Name);
                throw new XamlSchemaException(message);
            }
            return objs[0];
        }

        protected virtual object[] LookupCustomAttributes(Type attrType)
        {
            return ClrBindingPropertyInfo.GetCustomAttributes(attrType, true);
        }

        // ==============================================

        private bool LookupIsBrowsable()
        {
            Object attr = LookupCustomAttribute(typeof(EditorBrowsableAttribute));
            if (null != attr)
            {
                EditorBrowsableAttribute eba = (EditorBrowsableAttribute)attr;
                if (eba.State == EditorBrowsableState.Never)
                    return false;
            }
            return true;
        }

        private bool LookupIsObsolete()
        {
            Object attr = LookupCustomAttribute(typeof(ObsoleteAttribute));
            if (null != attr)
            {
                return true;
            }
            return false;
        }

        // Value Types don't neccessary come from a declared namespace.  For example
        // "Double" or "String" or "Object".  So we create a XamlType w/o a namespace.
        private XamlType LookupTypeOfProperty()
        {
            XamlType xamlType = this.SchemaContext.GetXamlType(ClrSystemTypeOfProperty);
            return xamlType;
        }

        protected virtual XamlTextSyntax LookupTextSyntax()
        {
            // Look for a type converter on the Property.
            object[] objs = LookupCustomAttributes(typeof(TypeConverterAttribute));

            XamlTextSyntax representer;

            // If there were attributes on the Property then get that
            // TypeConverter, else look on the Type.
            if (objs.Length > 0)
            {
                TypeConverterAttribute[] tcAttrs = (TypeConverterAttribute[])objs;
                representer = ClrNamespace.GetOrCreateTextSyntaxFromAttributes(this.SchemaContext, tcAttrs);
            }
            else
            {
                XamlType xamlType = this.Type;
                representer = xamlType.TextSyntax;
            }
            if (representer == null)
            {
                representer = XamlTextSyntax.NoSyntax;
            }
            return representer;
        }

        private XamlProperty LookupDependsOn()
        {
            object obj = LookupCustomAttribute(typeof(DependsOnAttribute));
            if (obj == null)
            {
                return null;
            }
            else
            {
                string doPropertyName = ((DependsOnAttribute)obj).Name;
                XamlProperty xp = _declaringType.GetProperty(doPropertyName);
                if (xp != null)
                    return xp;
                else
                {
                    string err = SR.Get(SRID.UnknownAttributeProperty, typeof(DependsOnAttribute).Name, doPropertyName, _name);
                    throw new XamlSchemaException(err);
                }
            }
        }

        protected virtual XamlType LookupTargetType()
        {
            return null;
        }

        private bool LookupIsAmbient()
        {
            object obj = LookupCustomAttribute(typeof(AmbientAttribute));
            return (obj == null) ? false : true;
        }

        // ===========================

        private bool CheckPropertyBit(BoolPropertyBits propertyBit)
        {
            int bit = (int)propertyBit;
            if (0 != (_validPropertyBits & bit))
            {
                return 0 != (_cachedPropertyBits & bit);
            }

            bool isBitTrue = LookupPropertyBit(propertyBit);
            _validPropertyBits |= bit;
            if (isBitTrue)
                _cachedPropertyBits |= bit;

            return isBitTrue;
        }

        private bool LookupPropertyBit(BoolPropertyBits propertyBit)
        {
            bool bit;
            switch (propertyBit)
            {
            case BoolPropertyBits.IsAmbient:
                bit = LookupIsAmbient();
                break;

            case BoolPropertyBits.IsBrowsable:
                bit = LookupIsBrowsable();
                break;

            case BoolPropertyBits.IsObsolete:
                bit = LookupIsObsolete();
                break;

            default:
                throw new XamlInternalException(SR.Get(SRID.MissingLookPropertyBit));
            }
            return bit;
        }

        private bool CheckNonBoolValidBit(NonBoolPropertyValidBits propBit)
        {
            int bit = (int)propBit;
            bool result = (0 != (_nonBoolValidPropertyBits & bit));

            return result;
        }

        private void SetNonBoolValidBit(NonBoolPropertyValidBits propBit)
        {
            _nonBoolValidPropertyBits |= (int)propBit;
        }
    }
}
