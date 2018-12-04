using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.MS.Impl;
using MS.Internal.Xaml.Schema;
#else
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using System.Collections.Generic;
#endif

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml
#else
namespace System.Xaml
#endif
{
    [DebuggerDisplay("DIRPROP {Name}")]
    class DirectiveProperty : XamlProperty
    {
        private string _directiveName;
        private XamlTextSyntax _textSyntax;
        private AllowedMemberLocation _allowedLocations;
        private XamlType _xamlType;
        private IList<string> _xmlNamespaces;

        internal DirectiveProperty(XamlSchemaContext context, DirectivePropertyInfo dpi)
        {
            _directiveName = dpi.Name;
            _textSyntax = dpi.TextSyntax;
            _allowedLocations = dpi.AllowedLocation;
            _xamlType = context.GetXamlType(dpi.Type);
            _xmlNamespaces = dpi.NamespaceList;
        }

        // ----- Protected Overrides -----

        public override string BoundName
        {
            get { return "Directive:" + Name; }
        }

        public override bool IsImplicit { get { return false; } }
        public override bool IsUnknown { get { return false; } }

        public override IList<string> GetXamlNamespaces()
        {
            Debug.Assert(_xmlNamespaces is ReadOnlyCollection<string>, "Namespace collection must be read-only");
            return _xmlNamespaces;
        }

        protected override string NameCore
        {
            get { return _directiveName; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsPublicCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsBrowsableCore
        {
            get { return true; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsObsoleteCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType DeclaringTypeCore
        {
            get { return null; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType TypeCore
        {
            get { return _xamlType; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlTextSyntax TextSyntaxCore
        {
            get { return _textSyntax; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsReadOnlyCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsStaticCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

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

        protected override XamlType TargetTypeCore
        {
            get { return null; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override AllowedMemberLocation AllowedLocationCore
        {
            get { return _allowedLocations; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlProperty DependsOnCore
        {
            get { return null; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsAmbientCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }
    }
}
