//------------------------------------------------------------------------------
// <copyright file="XmlValidatingReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;

namespace System.Xml
{
    [PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
    [Obsolete("Use XmlReader created by XmlReader.Create() method using appropriate XmlReaderSettings instead. http://go.microsoft.com/fwlink/?linkid=14202")]
    public class XmlValidatingReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver {
//
// Member fields
//
        XmlValidatingReaderImpl impl;
//
// Constructors
//
        public XmlValidatingReader( XmlReader reader ) {
            impl = new XmlValidatingReaderImpl( reader );
            impl.OuterReader = this;
        }
        
        public XmlValidatingReader( string xmlFragment, XmlNodeType fragType, XmlParserContext context ) {
            if (xmlFragment == null) {
                throw new ArgumentNullException("xmlFragment");
            }
            impl = new XmlValidatingReaderImpl( xmlFragment, fragType, context );
            impl.OuterReader = this;
        }

        public XmlValidatingReader( Stream xmlFragment, XmlNodeType fragType, XmlParserContext context ) {
            if (xmlFragment == null) {
                throw new ArgumentNullException("xmlFragment");
            }
            impl = new XmlValidatingReaderImpl(xmlFragment, fragType, context);
            impl.OuterReader = this;
        }

//
// XmlReader members
//
        public override XmlNodeType NodeType { 
            get { return impl.NodeType; } 
        }

        public override string Name { 
            get { return impl.Name; } 
        }

        public override string LocalName { 
            get { return impl.LocalName; } 
        }

        public override string NamespaceURI { 
            get { return impl.NamespaceURI; } 
        }

        public override string Prefix { 
            get { return impl.Prefix; } 
        }

        public override bool HasValue { 
            get { return impl.HasValue; } 
        }

        public override string Value { 
            get { return impl.Value; } 
        }

        public override int Depth { 
            get { return impl.Depth; } 
        }

        public override string BaseURI { 
            get { return impl.BaseURI; } 
        }

        public override bool IsEmptyElement { 
            get { return impl.IsEmptyElement; } 
        }

        public override bool IsDefault { 
            get { return impl.IsDefault; } 
        }

        public override char QuoteChar { 
            get { return impl.QuoteChar; } 
        }

        public override XmlSpace XmlSpace { 
            get { return impl.XmlSpace; } 
        }

        public override string XmlLang { 
            get { return impl.XmlLang; } 
        }

        // XmlTextReader does not override SchemaInfo, ValueType and ReadTypeValue

        public override int AttributeCount { get { return impl.AttributeCount; } }

        public override string GetAttribute( string name ) {
            return impl.GetAttribute( name );
        }

        public override string GetAttribute( string localName, string namespaceURI ) {
            return impl.GetAttribute( localName, namespaceURI );
        }

        public override string GetAttribute( int i ) {
            return impl.GetAttribute( i );
        }

        public override bool MoveToAttribute( string name ) {
            return impl.MoveToAttribute( name );
        }

        public override bool MoveToAttribute( string localName, string namespaceURI ) {
            return impl.MoveToAttribute( localName, namespaceURI );
        }

        public override void MoveToAttribute( int i ) {
            impl.MoveToAttribute( i );
        }

        public override bool MoveToFirstAttribute() {
            return impl.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute() {
            return impl.MoveToNextAttribute();
        }

        public override bool MoveToElement() {
            return impl.MoveToElement();
        }

        public override bool ReadAttributeValue() {
            return impl.ReadAttributeValue();
        }

        public override bool Read() {
            return impl.Read();
        }

        public override bool EOF { 
            get { return impl.EOF; } 
        }
        
        public override void Close() {
            impl.Close();
        }

        public override ReadState ReadState { 
            get { return impl.ReadState; } 
        }
        
        public override XmlNameTable NameTable { 
            get { return impl.NameTable; } 
        }

        public override String LookupNamespace( String prefix ) {
            string ns = impl.LookupNamespace( prefix );
            if ( ns != null && ns.Length == 0 ) {
                ns = null;
            }
            return ns;
        }

        public override bool CanResolveEntity  { 
            get { return true; } 
        }

        public override void ResolveEntity() {
            impl.ResolveEntity();
        }

    // Binary content access methods
        public override bool CanReadBinaryContent {
            get { return true; }
        }

        public override int ReadContentAsBase64( byte[] buffer, int index, int count ) {
            return impl.ReadContentAsBase64( buffer, index, count );
        }

        public override int ReadElementContentAsBase64( byte[] buffer, int index, int count ) {
            return impl.ReadElementContentAsBase64( buffer, index, count );
        }

        public override int ReadContentAsBinHex( byte[] buffer, int index, int count ) {
            return impl.ReadContentAsBinHex( buffer, index, count );
        }

        public override int ReadElementContentAsBinHex( byte[] buffer, int index, int count ) {
            return impl.ReadElementContentAsBinHex( buffer, index, count );
        }

        // Overriden helper methods

        public override string ReadString() {
            impl.MoveOffEntityReference();
            return base.ReadString();
        }
        
//
// IXmlLineInfo members
//
        public bool HasLineInfo() { return true; }

        public int LineNumber { get { return impl.LineNumber; } }

        public int LinePosition { get { return impl.LinePosition; } }

//
// IXmlNamespaceResolver members
//
        IDictionary<string,string> IXmlNamespaceResolver.GetNamespacesInScope( XmlNamespaceScope scope ) {
            return impl.GetNamespacesInScope( scope );
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix) {
            return impl.LookupNamespace( prefix );
        }

        string IXmlNamespaceResolver.LookupPrefix( string namespaceName ) {
            return impl.LookupPrefix( namespaceName );
        }

//
// XmlValidatingReader 
//
        public event ValidationEventHandler ValidationEventHandler {
            add    { impl.ValidationEventHandler += value; }
            remove { impl.ValidationEventHandler -= value; }
        }

        public object SchemaType {
            get { return impl.SchemaType; }
        }

        public XmlReader Reader {
            get { return impl.Reader; }
        }

        public ValidationType ValidationType {
            get { return impl.ValidationType; }
            set { impl.ValidationType = value; }
        }

        public XmlSchemaCollection Schemas {
            get { return impl.Schemas; }
        }

        public EntityHandling EntityHandling {
            get { return impl.EntityHandling; }
            set { impl.EntityHandling = value; }
        }
        
        public XmlResolver XmlResolver {
            set { impl.XmlResolver = value; }
        }

        public bool Namespaces {
            get { return impl.Namespaces; }
            set { impl.Namespaces = value; }
        }

        public object ReadTypedValue() {
            return impl.ReadTypedValue();
        }

        public Encoding Encoding {
            get { return impl.Encoding; }
        }
//
// Internal helper methods
//
        internal XmlValidatingReaderImpl Impl {
            get { return impl; }
        }

        internal override IDtdInfo DtdInfo {
            get { return impl.DtdInfo; }
        }
    }
}
