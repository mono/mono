using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.ComponentModel;

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
    [ContentProperty("Types")]
#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class XaslNamespace: XamlNamespace
    {
        private string _targetNamespace;
        private XaslTypeCollection _types;
        private XaslTypeCollection _directiveTypes;
        private XaslMemberCollection _directiveProperties;

        [DefaultValue(null)]
        new public string TargetNamespace
        {
            get { return TargetNamespaceCore; }
            set { TargetNamespaceCore = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public XaslTypeCollection Types
        {
            get
            {
                if (_types == null)
                {
                    _types = new XaslTypeCollection();
                }
                return _types;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public XaslTypeCollection DirectiveTypes
        {
            get
            {
                if (_directiveTypes == null)
                {
                    _directiveTypes = new XaslTypeCollection();
                }
                return _directiveTypes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public XaslMemberCollection DirectiveProperties
        {
            get
            {
                if (_directiveProperties == null)
                {
                    _directiveProperties = new XaslMemberCollection(null);
                }
                return _directiveProperties;
            }
        }

        // ---------  the following is for the abstract base class -------

        public override string BoundName { get { return "XASL:" + this.TargetNamespace; } }
        public override bool IsResolved { get { return true; } }

        public override XamlType GetXamlType(string typeName, XamlType[] typeArgs)
        {
            string name1;
            string name2;
            GetTypeExtensionNames(typeName, out name1, out name2);

            XaslType xaslType;
            if (!this.Types.TryGetValue(name1, out xaslType))
            {
                if (!this.Types.TryGetValue(name2, out xaslType))
                {
                    return null;
                }
            }
            return xaslType;
        }

        public override IEnumerable<XamlType> GetAllXamlTypes()
        {
            return this.Types as IEnumerable<XamlType>;
        }

        public override XamlType GetDirectiveType(string name)
        {
            XaslType dirType = null;
            if (!DirectiveTypes.TryGetValue(name, out dirType))
            {
                // throw in the early versions, do something better for error path.
                throw new KeyNotFoundException(SR.Get(SRID.DirectiveNotFound, name, this.TargetNamespace));
            }
            return dirType;
        }

        public override IEnumerable<XamlType> GetAllDirectiveTypes()
        {
            return DirectiveTypes as IEnumerable<XamlType>;
        }

        public override XamlProperty GetDirectiveProperty(string name)
        {
            XaslMember dirProperty = null;
            if (!DirectiveProperties.TryGetValue(name, out dirProperty))
            {
                // throw in the early versions, do something better for error path.
                throw new KeyNotFoundException(SR.Get(SRID.DirectiveNotFound, name, this.TargetNamespace));
            }
            return dirProperty;
        }

        public override IEnumerable<XamlProperty> GetAllDirectiveProperties()
        {
            return DirectiveProperties as IEnumerable<XamlProperty>;
        }

        // ----- protected -----

        override protected string TargetNamespaceCore
        {
            get { return _targetNamespace; }
            set { _targetNamespace = value; }
        }
    }
}
