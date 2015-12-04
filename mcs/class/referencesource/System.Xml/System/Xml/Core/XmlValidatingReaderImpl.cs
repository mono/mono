
//------------------------------------------------------------------------------
// <copyright file="XmlValidatingReaderImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.Policy;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.Versioning;

namespace System.Xml
{
    internal sealed partial class XmlValidatingReaderImpl : XmlReader, IXmlLineInfo, IXmlNamespaceResolver {

//
// Private helper types
//
        // ParsingFunction = what should the reader do when the next Read() is called
        enum ParsingFunction {
            Read = 0,
            Init,
            ParseDtdFromContext,
            ResolveEntityInternally,
            InReadBinaryContent,
            ReaderClosed,
            Error,
            None,
        }

        internal class ValidationEventHandling : IValidationEventHandling {
            // Fields
            XmlValidatingReaderImpl reader;
            ValidationEventHandler eventHandler;

            // Constructor
            internal ValidationEventHandling(XmlValidatingReaderImpl reader) {
                this.reader = reader;
            }

            // IValidationEventHandling interface
#region IValidationEventHandling interface
            object IValidationEventHandling.EventHandler {
                get { return eventHandler; }
            }

            void IValidationEventHandling.SendEvent(Exception /*XmlSchemaException*/ exception, XmlSeverityType severity) {
                if (eventHandler != null) {
                    eventHandler(reader, new ValidationEventArgs((XmlSchemaException)exception, severity));
                }
                else if (reader.ValidationType != ValidationType.None && severity == XmlSeverityType.Error) {
                    throw exception;
                }
            }
#endregion

            // XmlValidatingReaderImpl helper methods
            internal void AddHandler(ValidationEventHandler handler) {
                eventHandler += handler;
            }

            internal void RemoveHandler(ValidationEventHandler handler) {
                eventHandler -= handler;
            }
        }

//
// Fields
//
        // core text reader
        XmlReader               coreReader;
        XmlTextReaderImpl       coreReaderImpl;
        IXmlNamespaceResolver   coreReaderNSResolver;

        // validation
        ValidationType      validationType;
        BaseValidator       validator;

#pragma warning disable 618
        XmlSchemaCollection schemaCollection;
#pragma warning restore 618
        bool                processIdentityConstraints;

        // parsing function (state)
        ParsingFunction     parsingFunction = ParsingFunction.Init;

        // event handling
        ValidationEventHandling eventHandling;

        // misc
        XmlParserContext    parserContext;

        // helper for Read[Element]ContentAs{Base64,BinHex} methods
        ReadContentAsBinaryHelper  readBinaryHelper;

        // Outer XmlReader exposed to the user - either XmlValidatingReader or XmlValidatingReaderImpl (when created via XmlReader.Create).
        // Virtual methods called from within XmlValidatingReaderImpl must be called on the outer reader so in case the user overrides
        // some of the XmlValidatingReader methods we will call the overriden version.
        XmlReader       outerReader;

//
// Constructors
//
        // Initializes a new instance of XmlValidatingReaderImpl class with the specified XmlReader.
        // This constructor is used when creating XmlValidatingReaderImpl for V1 XmlValidatingReader
        internal XmlValidatingReaderImpl( XmlReader reader ) {
            XmlAsyncCheckReader asyncCheckReader = reader as XmlAsyncCheckReader;
            if (asyncCheckReader != null) {
                reader = asyncCheckReader.CoreReader;
            }
            outerReader = this;
            coreReader = reader;
            coreReaderNSResolver = reader as IXmlNamespaceResolver;
            coreReaderImpl = reader as XmlTextReaderImpl;
            if ( coreReaderImpl == null ) {
                XmlTextReader tr = reader as XmlTextReader;
                if ( tr != null ) {
                    coreReaderImpl = tr.Impl;
                }
            }
            if ( coreReaderImpl == null ) {
                throw new ArgumentException( Res.GetString( Res.Arg_ExpectingXmlTextReader ), "reader" );
            }
            coreReaderImpl.EntityHandling = EntityHandling.ExpandEntities;
            coreReaderImpl.XmlValidatingReaderCompatibilityMode = true;
            this.processIdentityConstraints = true;

#pragma warning disable 618
            schemaCollection = new XmlSchemaCollection( coreReader.NameTable );
            schemaCollection.XmlResolver = GetResolver();

            eventHandling = new ValidationEventHandling(this);
            coreReaderImpl.ValidationEventHandling = eventHandling;
            coreReaderImpl.OnDefaultAttributeUse = new XmlTextReaderImpl.OnDefaultAttributeUseDelegate(ValidateDefaultAttributeOnUse);

            validationType = ValidationType.Auto;
            SetupValidation( ValidationType.Auto );
#pragma warning restore 618

        }

        // Initializes a new instance of XmlValidatingReaderImpl class for parsing fragments with the specified string, fragment type and parser context
        // This constructor is used when creating XmlValidatingReaderImpl for V1 XmlValidatingReader
        // SxS: This method resolves an Uri but does not expose it to the caller. It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        internal XmlValidatingReaderImpl( string xmlFragment, XmlNodeType fragType, XmlParserContext context )
            : this( new XmlTextReader( xmlFragment, fragType, context ) )
        {
            if ( coreReader.BaseURI.Length > 0 ) {
                validator.BaseUri = GetResolver().ResolveUri( null, coreReader.BaseURI );
            }

            if ( context != null ) {
                parsingFunction = ParsingFunction.ParseDtdFromContext;
                parserContext = context;
            }
        }

        // Initializes a new instance of XmlValidatingReaderImpl class for parsing fragments with the specified stream, fragment type and parser context
        // This constructor is used when creating XmlValidatingReaderImpl for V1 XmlValidatingReader
        // SxS: This method resolves an Uri but does not expose it to the caller. It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        internal XmlValidatingReaderImpl( Stream xmlFragment, XmlNodeType fragType, XmlParserContext context )
            : this( new XmlTextReader( xmlFragment, fragType, context ) )
        {
            if ( coreReader.BaseURI.Length > 0 ) {
                validator.BaseUri = GetResolver().ResolveUri( null, coreReader.BaseURI );
            }

            if ( context != null ) {
                parsingFunction = ParsingFunction.ParseDtdFromContext;
                parserContext = context;
            }
        }
        
        // Initializes a new instance of XmlValidatingReaderImpl class with the specified arguments.
        // This constructor is used when creating XmlValidatingReaderImpl reader via "XmlReader.Create(..)"
        internal XmlValidatingReaderImpl( XmlReader reader, ValidationEventHandler settingsEventHandler, bool processIdentityConstraints) {
            XmlAsyncCheckReader asyncCheckReader = reader as XmlAsyncCheckReader;
            if (asyncCheckReader != null) {
                reader = asyncCheckReader.CoreReader;
            }
            outerReader = this;
            coreReader = reader;
            coreReaderImpl = reader as XmlTextReaderImpl;
            if ( coreReaderImpl == null ) {
                XmlTextReader tr = reader as XmlTextReader;
                if ( tr != null ) {
                    coreReaderImpl = tr.Impl;
                }
            }
            if ( coreReaderImpl == null ) {
                throw new ArgumentException( Res.GetString( Res.Arg_ExpectingXmlTextReader ), "reader" );
            }
            coreReaderImpl.XmlValidatingReaderCompatibilityMode = true;
            coreReaderNSResolver = reader as IXmlNamespaceResolver;
            this.processIdentityConstraints = processIdentityConstraints;

#pragma warning disable 618

            schemaCollection = new XmlSchemaCollection( coreReader.NameTable );

#pragma warning restore 618

            schemaCollection.XmlResolver = GetResolver();
            
            eventHandling = new ValidationEventHandling(this);
            if (settingsEventHandler != null) {
                eventHandling.AddHandler(settingsEventHandler);
            }
            coreReaderImpl.ValidationEventHandling = eventHandling;
            coreReaderImpl.OnDefaultAttributeUse = new XmlTextReaderImpl.OnDefaultAttributeUseDelegate(ValidateDefaultAttributeOnUse);

            validationType = ValidationType.DTD;
            SetupValidation( ValidationType.DTD );
        }

//
// XmlReader members
//
        // Returns the current settings of the reader
        public override XmlReaderSettings Settings {
            get {
                XmlReaderSettings settings;
                if (coreReaderImpl.V1Compat) {
                    settings = null;
                }
                else {
                    settings = coreReader.Settings;
                }
                if (settings != null) {
                    settings = settings.Clone();
                }
                else {
                    settings = new XmlReaderSettings();
                }
                settings.ValidationType = ValidationType.DTD;
                if (!processIdentityConstraints) {
                    settings.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessIdentityConstraints;
                }
                settings.ReadOnly = true;
                return settings;
            }
        }

        // Returns the type of the current node.
        public override XmlNodeType NodeType { 
            get {
                return coreReader.NodeType;
            }
        }

        // Returns the name of the current node, including prefix.
        public override string Name { 
            get {
                return coreReader.Name;
            }
        }

        // Returns local name of the current node (without prefix)
        public override string LocalName { 
            get {
                return coreReader.LocalName;
            }
        }

        // Returns namespace name of the current node.
        public override string NamespaceURI { 
            get {
                return coreReader.NamespaceURI;
            }
        }

        // Returns prefix associated with the current node.
        public override string Prefix { 
            get {
                return coreReader.Prefix;
            }
        }

        // Returns true if the current node can have Value property != string.Empty.
        public override bool HasValue { 
            get {
                return coreReader.HasValue;
            }
        }

        // Returns the text value of the current node.
        public override string Value { 
            get {
                return coreReader.Value;
            }
        }

        // Returns the depth of the current node in the XML element stack
        public override int Depth { 
            get {
                return coreReader.Depth;
            }
        }

        // Returns the base URI of the current node.
        public override string BaseURI { 
            get {
                return coreReader.BaseURI;
            }
        }

        // Returns true if the current node is an empty element (for example, <MyElement/>).
        public override bool IsEmptyElement { 
            get {
                return coreReader.IsEmptyElement;
            }
        }

        // Returns true of the current node is a default attribute declared in DTD.
        public override bool IsDefault { 
            get {
                return coreReader.IsDefault;
            }
        }

        // Returns the quote character used in the current attribute declaration
        public override char QuoteChar { 
            get {
                return coreReader.QuoteChar;
            }
        }

        // Returns the current xml:space scope.
        public override XmlSpace XmlSpace { 
            get {
                return coreReader.XmlSpace;
            }
        }

        // Returns the current xml:lang scope.</para>
        public override string XmlLang { 
            get {
                return coreReader.XmlLang;
            }
        }

        // Returns the current read state of the reader
        public override ReadState ReadState { 
            get {
                return ( parsingFunction == ParsingFunction.Init ) ? ReadState.Initial : coreReader.ReadState;
            }
        }
        
        // Returns true if the reader reached end of the input data
        public override bool EOF { 
            get {
                return coreReader.EOF;
            }
        }
        
        // Returns the XmlNameTable associated with this XmlReader
        public override XmlNameTable NameTable { 
            get {
                return coreReader.NameTable;
            }
        }

        // Returns encoding of the XML document
        internal Encoding Encoding {
            get {
                return coreReaderImpl.Encoding;
            }
        }

        // Returns the number of attributes on the current node.
        public override int AttributeCount { 
            get {
                return coreReader.AttributeCount;
            }
        }

        // Returns value of an attribute with the specified Name
        public override string GetAttribute( string name ) {
            return coreReader.GetAttribute( name );
        }

        // Returns value of an attribute with the specified LocalName and NamespaceURI
        public override string GetAttribute( string localName, string namespaceURI ) {
            return coreReader.GetAttribute( localName, namespaceURI );
        }

        // Returns value of an attribute at the specified index (position)
        public override string GetAttribute( int i ) {
            return coreReader.GetAttribute( i );
        }

        // Moves to an attribute with the specified Name
        public override bool MoveToAttribute( string name ) {
            if ( !coreReader.MoveToAttribute( name ) ) {
                return false;
            }
            parsingFunction = ParsingFunction.Read;
            return true;
        }

        // Moves to an attribute with the specified LocalName and NamespceURI
        public override bool MoveToAttribute( string localName, string namespaceURI ) {
            if ( !coreReader.MoveToAttribute( localName, namespaceURI ) ) {
                return false;
            }
            parsingFunction = ParsingFunction.Read;
            return true;
        }

        // Moves to an attribute at the specified index (position)
        public override void MoveToAttribute( int i ) {
            coreReader.MoveToAttribute( i );
            parsingFunction = ParsingFunction.Read;
        }

        // Moves to the first attribute of the current node
        public override bool MoveToFirstAttribute() {
            if ( !coreReader.MoveToFirstAttribute() ) {
                return false;
            }
            parsingFunction = ParsingFunction.Read;
            return true;
        }

        // Moves to the next attribute of the current node
        public override bool MoveToNextAttribute() {
            if ( !coreReader.MoveToNextAttribute() ) {
                return false;
            }
            parsingFunction = ParsingFunction.Read;
            return true;
        }

        // If on attribute, moves to the element that contains the attribute node
        public override bool MoveToElement() {
            if ( !coreReader.MoveToElement() ) {
                return false;
            }
            parsingFunction = ParsingFunction.Read;
            return true;
        }

        // Reads and validated next node from the input data
        public override  bool  Read() {
            switch ( parsingFunction ) {
                case ParsingFunction.Read:
                    if ( coreReader.Read() ) {
                        ProcessCoreReaderEvent();
                        return true;
                    }
                    else {
                        validator.CompleteValidation();
                        return false;
                    }
                case ParsingFunction.ParseDtdFromContext:
                    parsingFunction = ParsingFunction.Read;
                    ParseDtdFromParserContext();
                    goto case ParsingFunction.Read;
                case ParsingFunction.Error:
                case ParsingFunction.ReaderClosed:
                    return false;
                case ParsingFunction.Init:
                    parsingFunction = ParsingFunction.Read; // this changes the value returned by ReadState
                    if ( coreReader.ReadState == ReadState.Interactive ) {
                        ProcessCoreReaderEvent();
                        return true;
                    }
                    else {
                        goto case ParsingFunction.Read;
                    }
                case ParsingFunction.ResolveEntityInternally:
                    parsingFunction = ParsingFunction.Read;
                    ResolveEntityInternally();
                    goto case ParsingFunction.Read;
                case ParsingFunction.InReadBinaryContent:
                    parsingFunction = ParsingFunction.Read;
                    readBinaryHelper.Finish();
                    goto case ParsingFunction.Read;
                default:
                    Debug.Assert( false );
                    return false;
            }
        }

        // Closes the input stream ot TextReader, changes the ReadState to Closed and sets all properties to zero/string.Empty
        public override void Close() {
            coreReader.Close();
            parsingFunction = ParsingFunction.ReaderClosed;
        }

        // Returns NamespaceURI associated with the specified prefix in the current namespace scope.
        public override String LookupNamespace( String prefix ) {
            return coreReaderImpl.LookupNamespace( prefix );
        }

        // Iterates through the current attribute value's text and entity references chunks.
        public override bool ReadAttributeValue() {
            if ( parsingFunction == ParsingFunction.InReadBinaryContent ) {
                parsingFunction = ParsingFunction.Read;
                readBinaryHelper.Finish();
            }
            if (!coreReader.ReadAttributeValue()) {
                return false;
            }
            parsingFunction = ParsingFunction.Read;
            return true;
        }

        public override bool CanReadBinaryContent {
            get { 
                return true;
            }
        }

        public override  int  ReadContentAsBase64( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper if called the first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = readBinaryHelper.ReadContentAsBase64( buffer, index, count );

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        public override  int  ReadContentAsBinHex( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper when called first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = readBinaryHelper.ReadContentAsBinHex( buffer, index, count );

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        public override  int  ReadElementContentAsBase64( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper if called the first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = readBinaryHelper.ReadElementContentAsBase64( buffer, index, count );

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        public override  int  ReadElementContentAsBinHex( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper when called first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = readBinaryHelper.ReadElementContentAsBinHex( buffer, index, count );

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        // Returns true if the XmlReader knows how to resolve general entities
        public override bool CanResolveEntity {
            get {
                return true;
            }
        }

        // Resolves the current entity reference node
        public override void ResolveEntity() {
            if ( parsingFunction == ParsingFunction.ResolveEntityInternally ) {
                parsingFunction = ParsingFunction.Read;
            }
            coreReader.ResolveEntity();
        }

        internal XmlReader OuterReader {
            get {
                return outerReader;
            }
            set {
#pragma warning disable 618
                Debug.Assert( value is XmlValidatingReader );
#pragma warning restore 618
                outerReader = value;
            }
        }

        internal void MoveOffEntityReference() {
            if ( outerReader.NodeType == XmlNodeType.EntityReference && parsingFunction != ParsingFunction.ResolveEntityInternally ) {
                if ( !outerReader.Read() ) {
                    throw new InvalidOperationException( Res.GetString(Res.Xml_InvalidOperation ) );
                }
            }
        }

        public override  string  ReadString() {
            MoveOffEntityReference();
            return base.ReadString();
        }

//
// IXmlLineInfo members
//
        public bool HasLineInfo() {
            return true;
        }

        // Returns the line number of the current node
        public int LineNumber {
            get {
                return ((IXmlLineInfo)coreReader).LineNumber;
            }
        }

        // Returns the line number of the current node
        public int LinePosition { 
            get {
                return ((IXmlLineInfo)coreReader).LinePosition;
            }
        }

//
// IXmlNamespaceResolver members
//
        IDictionary<string,string> IXmlNamespaceResolver.GetNamespacesInScope( XmlNamespaceScope scope ) {
            return this.GetNamespacesInScope( scope );
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix) {
            return this.LookupNamespace( prefix );
        }

        string IXmlNamespaceResolver.LookupPrefix( string namespaceName ) {
            return this.LookupPrefix( namespaceName );
        }

    // Internal IXmlNamespaceResolver methods
        internal IDictionary<string,string> GetNamespacesInScope( XmlNamespaceScope scope ) {
            return coreReaderNSResolver.GetNamespacesInScope( scope );
        }

        internal string LookupPrefix( string namespaceName ) {
            return coreReaderNSResolver.LookupPrefix( namespaceName );
        }

//
// XmlValidatingReader members
//
        // Specufies the validation event handler that wil get warnings and errors related to validation
        internal event ValidationEventHandler ValidationEventHandler {
            add {
                eventHandling.AddHandler(value);
            }
            remove {
                eventHandling.RemoveHandler(value); ;
            }
        }

        // returns the schema type of the current node
        internal object SchemaType {
            get {
                if ( validationType != ValidationType.None ) {
                    XmlSchemaType schemaTypeObj = coreReaderImpl.InternalSchemaType as XmlSchemaType;
                    if ( schemaTypeObj != null && schemaTypeObj.QualifiedName.Namespace == XmlReservedNs.NsXs ) {
                        return schemaTypeObj.Datatype;
                    }
                    return coreReaderImpl.InternalSchemaType;
                }
                else
                    return null;            
            }
        }

        // returns the underlying XmlTextReader or XmlTextReaderImpl
        internal XmlReader Reader {
            get {
                return (XmlReader) coreReader;
            }
        }

        // returns the underlying XmlTextReaderImpl
        internal XmlTextReaderImpl ReaderImpl {
            get {
                return coreReaderImpl;
            }
        }

        // specifies the validation type (None, DDT, XSD, XDR, Auto)
        internal ValidationType ValidationType {
            get {
                return validationType;
            }
            set {
                if ( ReadState != ReadState.Initial ) {
                    throw new InvalidOperationException( Res.GetString( Res.Xml_InvalidOperation ) );
                }
                validationType = value;
                SetupValidation( value );
            }
        }

        // current schema collection used for validationg
#pragma warning disable 618
        internal XmlSchemaCollection Schemas {
            get {
                return schemaCollection; 
            }
        }
#pragma warning restore 618
        
        // Spefifies whether general entities should be automatically expanded or not
        internal EntityHandling EntityHandling {
            get {
                return coreReaderImpl.EntityHandling; 
            }
            set {
                coreReaderImpl.EntityHandling = value;
            }
        }
        
        // Specifies XmlResolver used for opening the XML document and other external references
        internal XmlResolver XmlResolver {
            set {
                coreReaderImpl.XmlResolver = value;
                validator.XmlResolver = value;
                schemaCollection.XmlResolver = value;
            }
        }

        // Disables or enables support of W3C XML 1.0 Namespaces
        internal bool Namespaces {
            get { 
                return coreReaderImpl.Namespaces; 
            }
            set {
                coreReaderImpl.Namespaces = value; 
            }
        }

        // Returns typed value of the current node (based on the type specified by schema)
        public  object  ReadTypedValue() {
            if ( validationType == ValidationType.None ) {
                return null;
            }

            switch ( outerReader.NodeType ) {
                case XmlNodeType.Attribute:
                    return coreReaderImpl.InternalTypedValue;
                case XmlNodeType.Element:
                    if ( SchemaType == null ) {
                        return null;
                    }
                    XmlSchemaDatatype dtype = ( SchemaType is XmlSchemaDatatype ) ? (XmlSchemaDatatype)SchemaType : ((XmlSchemaType)SchemaType).Datatype;
                    if ( dtype != null ) {
                        if ( !outerReader.IsEmptyElement ) {
                            for (;;) {
                                if ( !outerReader.Read() ) {
                                    throw new InvalidOperationException( Res.GetString( Res.Xml_InvalidOperation ) );
                                }
                                XmlNodeType type = outerReader.NodeType;
                                if ( type != XmlNodeType.CDATA && type != XmlNodeType.Text &&
                                    type != XmlNodeType.Whitespace && type != XmlNodeType.SignificantWhitespace &&
                                    type != XmlNodeType.Comment && type != XmlNodeType.ProcessingInstruction ) {
                                    break;
                                }
                            }
                            if ( outerReader.NodeType != XmlNodeType.EndElement ) {
                                throw new XmlException( Res.Xml_InvalidNodeType, outerReader.NodeType.ToString());
                            }
                        }
                        return coreReaderImpl.InternalTypedValue;
                    }
                    return null;

                case XmlNodeType.EndElement:
                    return null;

                default:
                    if ( coreReaderImpl.V1Compat ) { //If v1 XmlValidatingReader return null
                        return null;
                    }
                    else {  
                        return Value;
                    }
            }
        }

//
// Private implementation methods
//

        private void ParseDtdFromParserContext()
        {
            Debug.Assert( parserContext != null );
            Debug.Assert( coreReaderImpl.DtdInfo == null );

            if ( parserContext.DocTypeName == null || parserContext.DocTypeName.Length == 0 ) {
                return;
            }

            IDtdParser dtdParser = DtdParser.Create();
            XmlTextReaderImpl.DtdParserProxy proxy = new XmlTextReaderImpl.DtdParserProxy(coreReaderImpl);
            IDtdInfo dtdInfo = dtdParser.ParseFreeFloatingDtd( parserContext.BaseURI, parserContext.DocTypeName, parserContext.PublicId, 
                                                              parserContext.SystemId, parserContext.InternalSubset, proxy );
            coreReaderImpl.SetDtdInfo( dtdInfo);

            ValidateDtd();
        }

        private void ValidateDtd() {
            IDtdInfo dtdInfo = coreReaderImpl.DtdInfo;
            if (dtdInfo != null) {
                switch ( validationType ) { 
#pragma warning disable 618
                    case ValidationType.Auto:
                        SetupValidation( ValidationType.DTD );
                        goto case ValidationType.DTD;
#pragma warning restore 618
                    case ValidationType.DTD:
                    case ValidationType.None:
                        validator.DtdInfo = dtdInfo;
                        break;
                }
            }
        }

        private void ResolveEntityInternally() {
            Debug.Assert( coreReader.NodeType == XmlNodeType.EntityReference );
            int initialDepth = coreReader.Depth;
            outerReader.ResolveEntity();
            while ( outerReader.Read() && coreReader.Depth > initialDepth );
        }

        // SxS: This method resolves an Uri but does not expose it to caller. It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private void SetupValidation( ValidationType valType ) {
            validator = BaseValidator.CreateInstance( valType, this, schemaCollection, eventHandling , processIdentityConstraints);

            XmlResolver resolver = GetResolver();
            validator.XmlResolver = resolver;

            if ( outerReader.BaseURI.Length > 0 ) {
                validator.BaseUri = ( resolver == null ) ? new Uri( outerReader.BaseURI, UriKind.RelativeOrAbsolute ) : resolver.ResolveUri( null, outerReader.BaseURI );
            }
            coreReaderImpl.ValidationEventHandling = (validationType == ValidationType.None) ? null : eventHandling;
        }

        static XmlResolver s_tempResolver;

        // This is needed because we can't have the setter for XmlResolver public and with internal getter.
        private XmlResolver GetResolver() {
            XmlResolver tempResolver = coreReaderImpl.GetResolver();

            if (tempResolver == null && !coreReaderImpl.IsResolverSet &&
                !System.Xml.XmlReaderSettings.EnableLegacyXmlSettings())
            {
                // it is safe to return valid resolver as it'll be used in the schema validation 
                if (s_tempResolver == null)
                    s_tempResolver = new XmlUrlResolver();
                return s_tempResolver;
            }

            return tempResolver;
        }

//
// Internal methods for validators, DOM, XPathDocument etc.
//
        private void ProcessCoreReaderEvent() {
            switch ( coreReader.NodeType ) {
                case XmlNodeType.Whitespace:
                    if ( coreReader.Depth > 0 || coreReaderImpl.FragmentType != XmlNodeType.Document ) {
                        if ( validator.PreserveWhitespace ) {
                            coreReaderImpl.ChangeCurrentNodeType( XmlNodeType.SignificantWhitespace );
                        }
                    }
                    goto default;
                case XmlNodeType.DocumentType:
                    ValidateDtd();
                    break;
                case XmlNodeType.EntityReference:
                    parsingFunction = ParsingFunction.ResolveEntityInternally;
                    goto default;
                default:
                    coreReaderImpl.InternalSchemaType = null;
                    coreReaderImpl.InternalTypedValue = null;
                    validator.Validate();
                    break;
            }
        }

        internal void Close( bool closeStream ) {
            coreReaderImpl.Close( closeStream );
            parsingFunction = ParsingFunction.ReaderClosed;
        }

        internal BaseValidator Validator {
            get {
                return validator;
            }
            set {
                validator = value;
            }
        }

        internal override XmlNamespaceManager NamespaceManager {
            get { 
                return coreReaderImpl.NamespaceManager;
            }
        }

        internal bool StandAlone {
            get { 
                return coreReaderImpl.StandAlone; 
            }
        }

        internal object SchemaTypeObject {
            set { 
                coreReaderImpl.InternalSchemaType = value;
            }
        }

        internal object TypedValueObject {
            get { 
                return coreReaderImpl.InternalTypedValue; 
            }
            set { 
                coreReaderImpl.InternalTypedValue = value;
            }
        }

        internal bool Normalization {
            get {
                return coreReaderImpl.Normalization;
            }
        }

        internal bool AddDefaultAttribute( SchemaAttDef attdef ) {
            return coreReaderImpl.AddDefaultAttributeNonDtd( attdef );
        }

        internal override IDtdInfo DtdInfo {
            get { return coreReaderImpl.DtdInfo; }
        }

        internal void ValidateDefaultAttributeOnUse(IDtdDefaultAttributeInfo defaultAttribute, XmlTextReaderImpl coreReader) {
            SchemaAttDef attdef = defaultAttribute as SchemaAttDef;
            if (attdef == null) {
                return;
            }

            if (!attdef.DefaultValueChecked) {
                SchemaInfo schemaInfo = coreReader.DtdInfo as SchemaInfo;
                if (schemaInfo == null) {
                    return;
                }
                DtdValidator.CheckDefaultValue(attdef, schemaInfo, eventHandling, coreReader.BaseURI);
            }
        }

    }
}

