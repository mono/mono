// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlWrappingReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        private XmlReader baseReader;
        private IXmlLineInfo baseReaderAsLineInfo;
        private IXmlNamespaceResolver baseReaderAsNamespaceResolver;

        public override XmlReaderSettings Settings
        {
            get { return this.baseReader.Settings; }
        }

        public override XmlNodeType NodeType
        {
            get { return this.baseReader.NodeType; }
        }

        public override string Name
        {
            get { return this.baseReader.Name; }
        }

        public override string LocalName
        {
            get { return this.baseReader.LocalName; }
        }

        public override string NamespaceURI
        {
            get { return this.baseReader.NamespaceURI; }
        }

        public override string Prefix
        {
            get { return this.baseReader.Prefix; }
        }

        public override bool HasValue
        {
            get { return this.baseReader.HasValue; }
        }

        public override string Value
        {
            get { return this.baseReader.Value; }
        }

        public override int Depth
        {
            get { return this.baseReader.Depth; }
        }

        public override string BaseURI
        {
            get { return this.baseReader.BaseURI; }
        }

        public override bool IsEmptyElement
        {
            get { return this.baseReader.IsEmptyElement; }
        }

        public override bool IsDefault
        {
            get { return this.baseReader.IsDefault; }
        }

        public override char QuoteChar
        {
            get { return this.baseReader.QuoteChar; }
        }

        public override XmlSpace XmlSpace
        {
            get { return this.baseReader.XmlSpace; }
        }

        public override string XmlLang
        {
            get { return this.baseReader.XmlLang; }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get { return this.baseReader.SchemaInfo; }
        }

        public override Type ValueType
        {
            get { return this.baseReader.ValueType; }
        }

        public override int AttributeCount
        {
            get { return this.baseReader.AttributeCount; }
        }

        public override bool CanResolveEntity
        {
            get { return this.baseReader.CanResolveEntity; }
        }

        public override bool EOF
        {
            get { return this.baseReader.EOF; }
        }

        public override ReadState ReadState
        {
            get { return this.baseReader.ReadState; }
        }

        public override bool HasAttributes
        {
            get { return this.baseReader.HasAttributes; }
        }

        public override XmlNameTable NameTable
        {
            get { return this.baseReader.NameTable; }
        }

        public virtual int LineNumber
        {
            get
            {
                return (this.baseReaderAsLineInfo == null) ? 0 : this.baseReaderAsLineInfo.LineNumber;
            }
        }

        public virtual int LinePosition
        {
            get
            {
                return (this.baseReaderAsLineInfo == null) ? 0 : this.baseReaderAsLineInfo.LinePosition;
            }
        }

        protected XmlReader BaseReader
        {
            set
            {
                this.baseReader = value;
                this.baseReaderAsLineInfo = value as IXmlLineInfo;
                this.baseReaderAsNamespaceResolver = value as IXmlNamespaceResolver;
            }
        }

        protected IXmlLineInfo BaseReaderAsLineInfo
        {
            get
            {
                return this.baseReaderAsLineInfo;
            }
        }

        public override string this[int i]
        {
            get { return this.baseReader[i]; }
        }

        public override string this[string name]
        {
            get { return this.baseReader[name]; }
        }

        public override string this[string name, string namespaceURI]
        {
            get { return this.baseReader[name, namespaceURI]; }
        }

        public override string GetAttribute(string name)
        {
            return this.baseReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.baseReader.GetAttribute(name, namespaceURI);
        }

        public override string GetAttribute(int i)
        {
            return this.baseReader.GetAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.baseReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this.baseReader.MoveToAttribute(name, ns);
        }

        public override void MoveToAttribute(int i)
        {
            this.baseReader.MoveToAttribute(i);
        }

        public override bool MoveToFirstAttribute()
        {
            return this.baseReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.baseReader.MoveToNextAttribute();
        }

        public override bool MoveToElement()
        {
            return this.baseReader.MoveToElement();
        }

        public override bool Read()
        {
            return this.baseReader.Read();
        }

        public override void Close()
        {
            this.baseReader.Close();
        }

        public override void Skip()
        {
            this.baseReader.Skip();
        }

        public override string LookupNamespace(string prefix)
        {
            return this.baseReader.LookupNamespace(prefix);
        }

        public override void ResolveEntity()
        {
            this.baseReader.ResolveEntity();
        }

        public override bool ReadAttributeValue()
        {
            return this.baseReader.ReadAttributeValue();
        }

        public virtual bool HasLineInfo()
        {
            return (this.baseReaderAsLineInfo == null) ? false : this.baseReaderAsLineInfo.HasLineInfo();
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return (this.baseReaderAsNamespaceResolver == null) ? null : this.baseReaderAsNamespaceResolver.LookupPrefix(namespaceName);
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return (this.baseReaderAsNamespaceResolver == null) ? null : this.baseReaderAsNamespaceResolver.GetNamespacesInScope(scope);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.baseReader != null)
                {
                    ((IDisposable)this.baseReader).Dispose();
                }

                this.baseReader = null;
            }
        }
    }
}
