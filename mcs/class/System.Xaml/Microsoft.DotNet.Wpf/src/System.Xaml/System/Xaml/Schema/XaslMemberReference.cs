using System;
using System.Collections.Generic;
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
    [TypeConverter(typeof(XaslTypeReferenceConverter))]
#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslMemberRefContext
    {
        public XamlType ResolveXamlType(string name)
        {
            return null;
        }
    }

#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslMemberReference : XamlProperty
    {
        private string _name;
        private XaslMember _ref;
        internal XaslMemberReference(string name)
        {
            _name = name;
        }

        public XaslMember Ref
        {
            get
            {
                if (_ref == null)
                {
                    XamlProperty xamlMember = DeclaringType.GetProperty(_name);
                    // This will throw if the cast doesn't work
                    // and we do want some sort of error here if it fails.
                    _ref = (XaslMember)xamlMember;
                }
                return _ref;
            }
        }

        public override String BoundName
        {
            get { return "XASL member " + _name; }
        }

        public override bool IsImplicit { get { return false; } }
        public override bool IsUnknown { get { return false; } }

        protected override String NameCore
        {
            get { return _name; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter));  }
        }

        protected override bool IsPublicCore
        {
            get { return Ref.IsPublic; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter));  }
        }

        protected override bool IsBrowsableCore
        {
            get { return Ref.IsBrowsable; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsObsoleteCore
        {
            get { return Ref.IsObsolete; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType DeclaringTypeCore
        {
            get { return Ref.DeclaringType; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType TypeCore
        {
            get { return Ref.Type; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlTextSyntax TextSyntaxCore
        {
            get { return Ref.TextSyntax; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsReadOnlyCore
        {
            get { return Ref.IsReadOnly; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsStaticCore
        {
            get { return Ref.IsStatic; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsAttachableCore
        {
            get { return Ref.IsAttachable; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsEventCore
        {
            get { return Ref.IsEvent; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsDirectiveCore
        {
            get { return Ref.IsDirective; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType TargetTypeCore
        {
            get { return Ref.TargetType; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override AllowedMemberLocation AllowedLocationCore
        {
            get { return Ref.AllowedLocation; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlProperty DependsOnCore
        {
            get { return Ref.DependsOn; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsAmbientCore
        {
            get { return Ref.IsAmbient; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        public override IList<string> GetXmlNamespaces()
        {
            throw new NotImplementedException("ignoring this, since this type will go away soon.");
        }
    }
}
