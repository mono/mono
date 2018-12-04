using System;
using System.Collections.Generic;
using System.Text;

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
    class XmlNamespace: XamlNamespace
    {
        XamlSchemaContext SchemaContext;
        XamlDirectiveCollection _directives;

        public XmlNamespace(XamlSchemaContext context)
        {
            SchemaContext = context;
        }

        public override string BoundName
        {
            get { return "XML-XamlNamespace"; }
        }

        public override bool IsResolved
        {
            get { return true; }
        }

        public override XamlType GetXamlType(string typeName, XamlType[] typeArgs)
        {
            return null;
        }

        public override ICollection<XamlType> GetAllXamlTypes()
        {
            throw new NotImplementedException();
        }

        //  ----- Directives handling -----

        public override XamlType GetDirectiveType(string name)
        {
            if (_directives == null)
            {
                _directives = LoadDirectives();
            }
            XamlType directive = _directives.GetDirectiveElement(name);
            return directive;
        }

        public override IEnumerable<XamlType> GetAllDirectiveTypes()
        {
            throw new NotImplementedException();
        }

        public override XamlProperty GetDirectiveProperty(string name)
        {
            if (_directives == null)
            {
                _directives = LoadDirectives();
            }
            XamlProperty directive = _directives.GetDirectiveProperty(name);
            if (directive == null)
            {
                directive = new UnknownProperty(name,
                                                null, /*declaringType - xml directives don't have one. */
                                                XmlDirectives.Uri);
            }
            return directive;
        }

        public override IEnumerable<XamlProperty> GetAllDirectiveProperties()
        {
            throw new NotImplementedException();
        }

        // --------------- protected ----------------------

        protected override string TargetNamespaceCore
        {
            get { return XmlDirectives.Uri; }
            set { throw new InvalidOperationException(SR.Get(SRID.MustNotCallSetter)); }
        }

        // -------------------------------------

        private XamlDirectiveCollection LoadDirectives()
        {
            XamlDirectiveCollection directives = null;

            switch (TargetNamespace)
            {
            case XmlDirectives.Uri:  // Uri == "http://www.w3.org/XML/1998/namespace"
                directives = new XamlDirectiveCollection(SchemaContext, XmlDirectives.DirectiveInfoTable);
                break;

            default:
                directives = new XamlDirectiveCollection(SchemaContext, null);
                break;
            }
            return directives;
        }
    }
}
