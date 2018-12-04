using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.MS.Impl;
#else
using System.Xaml.MS.Impl;
using System.Collections.ObjectModel;
#endif

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml.Schema
#else
namespace System.Xaml.Schema
#endif
{
    [DebuggerDisplay("UNKNOWN {Name}")]
    class UnknownProperty : XamlProperty
    {
        string _name;
        private bool _isPublic;
        private XamlType _declaringType;
        private XamlType _type;
        private XamlTextSyntax _textSyntax;
        private Exception _exception;
        private IList<string> _XmlNamespaces;
        public bool _isAttachable;
        #region Constructors

        public UnknownProperty(string name, XamlType declaringType, string xmlns) 
            : this(name, declaringType, xmlns, false, null)
        {
        }

        public UnknownProperty(string name, XamlType declaringType, string xmlns, bool isAttachable)
            : this(name, declaringType, xmlns, isAttachable, null)
        {
        }

        public UnknownProperty(string name, XamlType declaringType, string xmlns, Exception ex)
            : this(name, declaringType, xmlns, false, ex)
        {
        }

        private UnknownProperty(string name, XamlType declaringType, string xmlns, bool isAttachable, Exception ex)
        {
            _name = name;
            _declaringType = declaringType;
            List<string> xmlnsList = new List<string>() { xmlns };
            _XmlNamespaces = xmlnsList.AsReadOnly();
            _isAttachable = isAttachable;
            _exception = ex;
        }

        #endregion
        public Exception Exception
        {
            get { return _exception; }
        }

        new public XamlType DeclaringType
        {
            get { return DeclaringTypeCore; }
            set { DeclaringTypeCore = value; }
        }

        new public XamlType Type
        {
            get { return TypeCore; }
            set { TypeCore = value; }
        }

        new public XamlTextSyntax TextSyntax
        {
            get { return TextSyntaxCore; }
            set { TextSyntaxCore = value; }
        }

        public override string BoundName
        {
            get { return "UNKNOWN:" + Name; }
        }

        public override bool IsImplicit { get { return false; } }
        public override bool IsUnknown { get { return true; } }

        public override IList<string> GetXamlNamespaces()
        {
            return _XmlNamespaces;
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
            get { return _isAttachable; }
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
