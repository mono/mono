
//------------------------------------------------------------------------------
// <copyright file="XmlTextReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Collections;
using System.Security.Policy;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.Versioning;

namespace System.Xml {

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
    public class XmlTextReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver {
//
// Member fields
//
        XmlTextReaderImpl impl;
//
//
// Constructors
//
        protected XmlTextReader() {
            impl = new XmlTextReaderImpl();
            impl.OuterReader = this;
        }

        protected XmlTextReader( XmlNameTable nt ) {
            impl = new XmlTextReaderImpl( nt );
            impl.OuterReader = this;
        }

        public XmlTextReader( Stream input ) {
            impl = new XmlTextReaderImpl( input );
            impl.OuterReader = this;
        }

        public XmlTextReader( string url, Stream input ) {
            impl = new XmlTextReaderImpl( url, input );
            impl.OuterReader = this;
        }

        public XmlTextReader( Stream input, XmlNameTable nt ) { 
            impl = new XmlTextReaderImpl( input, nt );
            impl.OuterReader = this;
        }

        public XmlTextReader( string url, Stream input, XmlNameTable nt ) {
            impl = new XmlTextReaderImpl( url, input, nt );
            impl.OuterReader = this;
        }

        public XmlTextReader( TextReader input ) {
            impl = new XmlTextReaderImpl( input );
            impl.OuterReader = this;
        }

        public XmlTextReader( string url, TextReader input ) { 
            impl = new XmlTextReaderImpl( url, input );
            impl.OuterReader = this;
        }

        public XmlTextReader( TextReader input, XmlNameTable nt ) {
            impl = new XmlTextReaderImpl( input, nt );
            impl.OuterReader = this;
        }

        public XmlTextReader( string url, TextReader input, XmlNameTable nt ) {
            impl = new XmlTextReaderImpl( url, input, nt );
            impl.OuterReader = this;
        }

        public XmlTextReader( Stream xmlFragment, XmlNodeType fragType, XmlParserContext context ) {
            impl = new XmlTextReaderImpl( xmlFragment, fragType, context );
            impl.OuterReader = this;
        }

        public XmlTextReader( string xmlFragment, XmlNodeType fragType, XmlParserContext context ) {
            impl = new XmlTextReaderImpl( xmlFragment, fragType, context );
            impl.OuterReader = this;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public XmlTextReader( string url ) {
            impl = new XmlTextReaderImpl( url, new NameTable() );
            impl.OuterReader = this;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public XmlTextReader( String url, XmlNameTable nt ) {
            impl = new XmlTextReaderImpl( url, nt );
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
        
        public override void Skip() {
            impl.Skip();
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

    // Text streaming methods

        // XmlTextReader does do support streaming of Value (there are backwards compatibility issues when enabled)
        public override bool CanReadValueChunk { 
            get { return false; }
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

// This pragma disables a warning that the return type is not CLS-compliant, but generics are part of CLS in Whidbey. 
#pragma warning disable 3002
        // FXCOP: ExplicitMethodImplementationsInUnsealedClassesHaveVisibleAlternates
        // public versions of IXmlNamespaceResolver methods, so that XmlTextReader subclasses can access them
        public IDictionary<string,string> GetNamespacesInScope( XmlNamespaceScope scope ) {
            return impl.GetNamespacesInScope( scope );
        }
#pragma warning restore 3002

//
// XmlTextReader 
//
        public bool Namespaces {
            get { return impl.Namespaces; }
            set { impl.Namespaces = value; }
        }

        public bool Normalization {
            get { return impl.Normalization; }
            set { impl.Normalization = value; }
        }

        public Encoding Encoding {
            get { return impl.Encoding; } 
        }

        public WhitespaceHandling WhitespaceHandling {
            get { return impl.WhitespaceHandling; }
            set { impl.WhitespaceHandling = value; }
        }

        [Obsolete("Use DtdProcessing property instead.")]
        public bool ProhibitDtd {
            get { return impl.DtdProcessing == DtdProcessing.Prohibit; }
            set { impl.DtdProcessing = value ? DtdProcessing.Prohibit : DtdProcessing.Parse; }
        }

        public DtdProcessing DtdProcessing {
            get { return impl.DtdProcessing; }
            set { impl.DtdProcessing = value; }
        }

        public EntityHandling EntityHandling {
            get { return impl.EntityHandling; }
            set { impl.EntityHandling = value; }
        }

        public XmlResolver XmlResolver {
            set { impl.XmlResolver = value; }
        }

        public void ResetState() {
            impl.ResetState();
        }

        public TextReader GetRemainder() {
            return impl.GetRemainder();
        }

        public int ReadChars( char[] buffer, int index, int count ) {
            return impl.ReadChars( buffer, index, count );
        }

        public int ReadBase64( byte[] array, int offset, int len ) {
            return impl.ReadBase64( array, offset, len );
        }

        public int ReadBinHex( byte[] array, int offset, int len ) {
            return impl.ReadBinHex( array, offset, len );
        }
//
// Internal helper methods
//
        internal XmlTextReaderImpl Impl {
            get { return impl; }
        }

        internal override XmlNamespaceManager NamespaceManager {
            get { return impl.NamespaceManager; }
        }

        // NOTE: System.Data.SqlXml.XmlDataSourceResolver accesses this property via reflection
        internal bool XmlValidatingReaderCompatibilityMode {
            set { impl.XmlValidatingReaderCompatibilityMode = value; }
        }

        internal override IDtdInfo DtdInfo {
            get { return impl.DtdInfo; }
        }
    }
}

