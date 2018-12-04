using System;
using System.Collections.Generic;
using System.Text;
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
    enum ImplicitPropertyType
    {
        Initialization,
        Items,
        PositionalParameters
    }

    [DebuggerDisplay("IMPLICIT {Name}")]
    class ImplicitProperty: XamlProperty
    {
        private string _name;
        private bool _isPublic;
        private XamlType _declaringType;
        private XamlType _type;
        private XamlTextSyntax _textSyntax;
        private ImplicitPropertyType _implicitType;

        public ImplicitProperty()
        {
            _isPublic = true;
        }

        public ImplicitProperty(ImplicitPropertyType implicitType, XamlType propertyType, XamlTextSyntax syntax, XamlType declaringType) :
            this()
        {
            _implicitType = implicitType;   
            TypeCore = propertyType;
            TextSyntaxCore = syntax;
            DeclaringTypeCore = declaringType;
            NameCore = ImplicitPropertyTypeName;
        }


        public ImplicitPropertyType ImplicitType
        {
            get { return _implicitType; }
        }

        // This is a performance optimization to avoid
        // calling ToString on ImplicitPropertyType and allocating
        // many strings
        private string ImplicitPropertyTypeName
        {
            get
            {
                switch(_implicitType)
                {
                    case ImplicitPropertyType.Initialization:
                        return "_Initialization";
                    case ImplicitPropertyType.Items:
                        return "_Items";
                    case ImplicitPropertyType.PositionalParameters:
                        return "_PositionalParameters";
                    default:
                        Debug.Assert(false);
                        return "_Unknown";                
                }
            }
        }

        // ===== XamlProperty properties =====

        public override string BoundName
        {
            get { return "IMPLICIT:" + Name; }
        }

        public override bool IsImplicit { get { return true; } }
        public override bool IsUnknown { get { return false; } }

        public override IList<string> GetXamlNamespaces()
        {
            return Xaml2006Directives.NamespaceList;
        }

        protected override string NameCore
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
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override bool IsObsoleteCore
        {
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType DeclaringTypeCore
        {
            get { return _declaringType; }
            set { _declaringType = value; }
        }

        protected override XamlType TypeCore
        {
            get { return _type; }
            set { _type = value; }
        }

        protected override XamlTextSyntax TextSyntaxCore
        {
            get { return _textSyntax; }
            set { _textSyntax = value; }
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
            get { return false; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override XamlType TargetTypeCore
        {
            get { return null; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        protected override AllowedMemberLocation AllowedLocationCore
        {
            get { return AllowedMemberLocation.Any; }
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
