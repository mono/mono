using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

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
#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    abstract class XaslMember : XamlProperty
    {
        string _name;
        bool _isPublic = true;
        bool _isBrowsable = true;
        bool _isObsolete = false;
        XamlType _valueType;
        XamlTextSyntax _textSyntax;
        bool _isReadOnly;
        bool _isStatic;
        AllowedMemberLocation _allowedLocation;
        XamlType _declaringType;
        XamlProperty _dependsOn;
        bool _isAmbient;

        new public String Name
        {
            get { return NameCore; }
            set { NameCore = value; }
        }

        [DefaultValue(true)]
        new public bool IsPublic
        {
            get { return IsPublicCore; }
            set { IsPublicCore = value; }
        }

        [DefaultValue(true)]
        new public bool IsBrowsable
        {
            get { return IsBrowsableCore; }
            set { IsBrowsableCore = value; }
        }

        [DefaultValue(false)]
        new public bool IsObsolete
        {
            get { return IsObsoleteCore; }
            set { IsObsoleteCore = value; }
        }

        new public XamlType DeclaringType
        {
            get { return DeclaringTypeCore; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        // Should the default be "x:String"?
        // We don't currently have a type converter to support: <XaslProperty Name="blah" Type="x:String" />
        [TypeConverter(typeof(XaslTypeReferenceConverter))]
        new public XamlType Type
        {
            get { return TypeCore; }
            set { TypeCore = value; }
        }

        [DefaultValue(null)]
        new public XamlTextSyntax TextSyntax
        {
            get { return TextSyntaxCore; }
            set { TextSyntaxCore = value; }
        }

        [DefaultValue(false)]
        new public bool IsReadOnly
        {
            get { return IsReadOnlyCore; }
            set { IsReadOnlyCore = value; }
        }

        [DefaultValue(false)]
        new public bool IsStatic
        {
            get { return IsStaticCore; }
            set { IsStaticCore = value; }
        }

        new public bool IsAttachable
        {
            get { return IsAttachableCore; }
            set { IsAttachableCore = value; }
        }

        new public bool IsEvent
        {
            get { return IsEventCore; }
            set { IsEventCore = value; }
        }

        new public bool IsDirective
        {
            get { return IsDirectiveCore; }
            set { IsDirectiveCore = value; }
        }

        new public XamlType TargetType
        {
            get { return TargetTypeCore; }
            set { TargetTypeCore = value; }
        }

        [DefaultValue(AllowedMemberLocation.Any)]
        new public AllowedMemberLocation AllowedLocation
        {
            get { return AllowedLocationCore; }
            set { AllowedLocationCore = value; }
        }

        [DefaultValue(null)]
        new public XamlProperty DependsOn
        {
            get { return DependsOnCore; }
            set { DependsOnCore = value; }
        }

        [DefaultValue(false)]
        new public bool IsAmbient
        {
            get { return IsAmbientCore; }
            set { IsAmbientCore = value; }
        }

        public override IList<string> GetXmlNamespaces()
        {
            throw new NotImplementedException("ignoring this, since this type will go away soon.");
        }

        // ----- Protected Overrides -----
        protected override String NameCore
        {
            get { return _name; }
            set { _name = value; }
        }

        protected override bool IsPublicCore
        {
            get { return _isPublic; }
            set { _isPublic = value; }
        }

        protected override bool IsBrowsableCore
        {
            get { return _isBrowsable; }
            set { _isBrowsable = value; }
        }

        protected override bool IsObsoleteCore
        {
            get { return _isObsolete; }
            set { _isObsolete = value; }
        }

        protected override XamlType DeclaringTypeCore
        {
            get { return _declaringType; }
            set { _declaringType = value; }
        }

        protected override XamlType TypeCore
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        protected override XamlTextSyntax TextSyntaxCore
        {
            get { return _textSyntax; }
            set { _textSyntax = value; }
        }

        protected override bool IsReadOnlyCore
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; }
        }

        protected override bool IsStaticCore
        {
            get { return _isStatic; }
            set { _isStatic = value; }
        }

        // Leave these to derived types.
        //protected override bool IsAttachableCore { get; set; }
        //protected override bool IsEventCore { get; set; }
        //protected override bool IsDirectiveCore { get; set; }
        protected override XamlType TargetTypeCore
        {
            get { throw new InvalidOperationException(SR.Get(SRID.GetTargetTypeOnNonAttachableMember)); }
            set { throw new InvalidOperationException(SR.Get(SRID.SetTargetTypeOnNonAttachableMember)); }
        }

        protected override AllowedMemberLocation AllowedLocationCore
        {
            get { return _allowedLocation; }
            set { _allowedLocation = value; }
        }

        protected override XamlProperty DependsOnCore
        {
            get { return _dependsOn; }
            set { _dependsOn = value; }
        }

        protected override bool IsAmbientCore
        {
            get { return _isAmbient; }
            set { _isAmbient = value; }
        }

        public void SetParentLink(XamlType parent)
        {
            _declaringType = parent;
        }

        // --------- Implement XamlProperty ---------
        public override String BoundName { get { return "XASL:" + this.Name; } }
        public override bool IsImplicit { get { return true; } }
        public override bool IsUnknown { get { return false; } }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslProperty : XaslMember
    {
        protected override bool IsAttachableCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslDirective : XaslMember
    {
        protected override bool IsAttachableCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslEvent : XaslMember
    {
        protected override bool IsAttachableCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    abstract class XaslAttachableMember : XaslMember
    {
        XamlType _targetType;

        protected override XamlType TargetTypeCore        // only if Attachable
        {
            get { return _targetType; }
            set { _targetType = value; }
        }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslAttachableProperty : XaslAttachableMember
    {
        protected override bool IsAttachableCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslAttachableEvent : XaslAttachableMember
    {
        protected override bool IsAttachableCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
    }

}
