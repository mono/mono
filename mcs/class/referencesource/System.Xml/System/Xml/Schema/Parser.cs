
//------------------------------------------------------------------------------
// <copyright file="Parser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">Microsoft</owner>                                                              
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.IO;
    using System.Diagnostics;

    internal sealed partial class Parser {

        SchemaType schemaType;
        XmlNameTable nameTable;
        SchemaNames schemaNames; 
        ValidationEventHandler eventHandler;
        XmlNamespaceManager namespaceManager;
        XmlReader reader;
        PositionInfo positionInfo;
        bool isProcessNamespaces;
        int schemaXmlDepth = 0;
        int markupDepth;
        SchemaBuilder builder;
        XmlSchema schema;
        SchemaInfo xdrSchema;
        XmlResolver xmlResolver = null; //to be used only by XDRBuilder

        //xs:Annotation perf fix
        XmlDocument dummyDocument;
        bool processMarkup;
        XmlNode parentNode;
        XmlNamespaceManager annotationNSManager;
        string xmlns;

        //Whitespace check for text nodes
        XmlCharType xmlCharType = XmlCharType.Instance;

        public Parser(SchemaType schemaType, XmlNameTable nameTable, SchemaNames schemaNames, ValidationEventHandler eventHandler) {
            this.schemaType = schemaType;
            this.nameTable = nameTable;
            this.schemaNames = schemaNames;
            this.eventHandler = eventHandler;
            this.xmlResolver = System.Xml.XmlConfiguration.XmlReaderSection.CreateDefaultResolver();
            processMarkup = true;
            dummyDocument = new XmlDocument();
        }

        public  SchemaType  Parse(XmlReader reader, string targetNamespace) {
            StartParsing(reader, targetNamespace);
            while(ParseReaderNode() && reader.Read()) {}
            return FinishParsing();
        }

        public void StartParsing(XmlReader reader, string targetNamespace) {
            this.reader = reader;
            positionInfo = PositionInfo.GetPositionInfo(reader);
            namespaceManager = reader.NamespaceManager;
            if (namespaceManager == null) {
                namespaceManager = new XmlNamespaceManager(nameTable);
                isProcessNamespaces = true;
            } 
            else {
                isProcessNamespaces = false;
            }
            while (reader.NodeType != XmlNodeType.Element && reader.Read()) {}

            markupDepth = int.MaxValue;
            schemaXmlDepth = reader.Depth;
            SchemaType rootType = schemaNames.SchemaTypeFromRoot(reader.LocalName, reader.NamespaceURI);
            
            string code;
            if (!CheckSchemaRoot(rootType, out code)) {
                throw new XmlSchemaException(code, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition);
            }
            
            if (schemaType == SchemaType.XSD) {
                schema = new XmlSchema();
                schema.BaseUri = new Uri(reader.BaseURI, UriKind.RelativeOrAbsolute);
                builder = new XsdBuilder(reader, namespaceManager, schema, nameTable, schemaNames, eventHandler);
            }
            else {  
                Debug.Assert(schemaType == SchemaType.XDR);
                xdrSchema = new SchemaInfo();
                xdrSchema.SchemaType = SchemaType.XDR;
                builder = new XdrBuilder(reader, namespaceManager, xdrSchema, targetNamespace, nameTable, schemaNames, eventHandler);
                ((XdrBuilder)builder).XmlResolver = xmlResolver;
            }
        }

        private bool CheckSchemaRoot(SchemaType rootType, out string code) {
            code = null;
            if (schemaType == SchemaType.None) {
                schemaType = rootType;
            }
            switch (rootType) {
                case SchemaType.XSD:
                    if (schemaType != SchemaType.XSD) {
                        code = Res.Sch_MixSchemaTypes;
                        return false;
                    }
                break;

                case SchemaType.XDR:
                    if (schemaType == SchemaType.XSD) {
                        code = Res.Sch_XSDSchemaOnly;
                        return false;
                    }
                    else if (schemaType != SchemaType.XDR) {
                        code = Res.Sch_MixSchemaTypes;
                        return false;
                    }
                break;
        
                case SchemaType.DTD: //Did not detect schema type that can be parsed by this parser
                case SchemaType.None:
                    code = Res.Sch_SchemaRootExpected;
                    if (schemaType == SchemaType.XSD) {
                        code = Res.Sch_XSDSchemaRootExpected;
                    }
                    return false;
    
                default:
                    Debug.Assert(false);
                    break;
            }
            return true;
        }

        public SchemaType FinishParsing() {
            return schemaType;
        }

        public XmlSchema XmlSchema {
            get { return schema; }
        }

        internal XmlResolver XmlResolver {
            set {
                xmlResolver = value;
            }
        }

        public SchemaInfo XdrSchema {
            get { return xdrSchema; }
        }

        public bool ParseReaderNode() {
            if (reader.Depth > markupDepth) {
                if (processMarkup) {
                    ProcessAppInfoDocMarkup(false);
                }
                return true;
            }
            else if (reader.NodeType == XmlNodeType.Element) {
                if (builder.ProcessElement(reader.Prefix, reader.LocalName, reader.NamespaceURI)) {
                    namespaceManager.PushScope();
                    if (reader.MoveToFirstAttribute()) {
                        do {
                            builder.ProcessAttribute(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
                            if (Ref.Equal(reader.NamespaceURI, schemaNames.NsXmlNs) && isProcessNamespaces) {                        
                                namespaceManager.AddNamespace(reader.Prefix.Length == 0 ? string.Empty : reader.LocalName, reader.Value);
                            }
                        }
                        while (reader.MoveToNextAttribute());
                        reader.MoveToElement(); // get back to the element
                    }
                    builder.StartChildren();
                    if (reader.IsEmptyElement) {
                        namespaceManager.PopScope();
                        builder.EndChildren();
                        if (reader.Depth == schemaXmlDepth) {
                            return false; // done
                        }
                    } 
                    else if (!builder.IsContentParsed()) { //AppInfo and Documentation
                        markupDepth = reader.Depth;
                        processMarkup = true;
                        if (annotationNSManager == null) {
                            annotationNSManager = new XmlNamespaceManager(nameTable);
                            xmlns = nameTable.Add("xmlns");
                        }
                        ProcessAppInfoDocMarkup(true);
                    }
                } 
                else if (!reader.IsEmptyElement) { //UnsupportedElement in that context
                    markupDepth = reader.Depth;
                    processMarkup = false; //Hack to not process unsupported elements
                }
            } 
            else if (reader.NodeType == XmlNodeType.Text) { //Check for whitespace
                if (!xmlCharType.IsOnlyWhitespace(reader.Value)) {
                    builder.ProcessCData(reader.Value);
                }
            }
            else if (reader.NodeType == XmlNodeType.EntityReference ||
                reader.NodeType == XmlNodeType.SignificantWhitespace ||
                reader.NodeType == XmlNodeType.CDATA) {
                builder.ProcessCData(reader.Value);
            }
            else if (reader.NodeType == XmlNodeType.EndElement) {

                if (reader.Depth == markupDepth) {
                    if (processMarkup) {
                        Debug.Assert(parentNode != null);
                        XmlNodeList list = parentNode.ChildNodes;
                        XmlNode[] markup = new XmlNode[list.Count];
                        for (int i = 0; i < list.Count; i ++) {
                            markup[i] = list[i];
                        }
                        builder.ProcessMarkup(markup);
                        namespaceManager.PopScope();
                        builder.EndChildren();
                    }
                    markupDepth = int.MaxValue;
                } 
                else {
                    namespaceManager.PopScope();
                    builder.EndChildren();
                }
                if(reader.Depth == schemaXmlDepth) {
                    return false; // done
                }
            }
            return true;
        }
        
        private void ProcessAppInfoDocMarkup(bool root) {
            //First time reader is positioned on AppInfo or Documentation element
            XmlNode currentNode = null; 
            
            switch (reader.NodeType) {
                case XmlNodeType.Element:
                    annotationNSManager.PushScope();
                    currentNode = LoadElementNode(root);
                    //  Dev10 (TFS) #479761: The following code was to address the issue of where an in-scope namespace delaration attribute
                    //      was not added when an element follows an empty element. This fix will result in persisting schema in a consistent form
                    //      although it does not change the semantic meaning of the schema.
                    //      Since it is as a breaking change and Dev10 needs to maintain the backward compatibility, this fix is being reverted.
                    //  if (reader.IsEmptyElement) {
                    //      annotationNSManager.PopScope();
                    //  }
                    break;
                
                case XmlNodeType.Text:
                    currentNode = dummyDocument.CreateTextNode( reader.Value );
                    goto default;

                case XmlNodeType.SignificantWhitespace:
                    currentNode = dummyDocument.CreateSignificantWhitespace( reader.Value );
                    goto default;

                case XmlNodeType.CDATA:
                    currentNode = dummyDocument.CreateCDataSection( reader.Value );
                    goto default;

                case XmlNodeType.EntityReference:
                    currentNode = dummyDocument.CreateEntityReference( reader.Name );
                    goto default;

                case XmlNodeType.Comment:    
                    currentNode = dummyDocument.CreateComment( reader.Value );
                    goto default;

                case XmlNodeType.ProcessingInstruction:
                    currentNode = dummyDocument.CreateProcessingInstruction( reader.Name, reader.Value );
                    goto default;
                
                case XmlNodeType.EndEntity:
                    break;
                
                case XmlNodeType.Whitespace:
                    break;

                case XmlNodeType.EndElement:
                    annotationNSManager.PopScope();
                    parentNode = parentNode.ParentNode;
                    break;
                
                default: //other possible node types: Document/DocType/DocumentFrag/Entity/Notation/Xmldecl cannot appear as children of xs:appInfo or xs:doc
                    Debug.Assert(currentNode != null);
                    Debug.Assert(parentNode != null);
                    parentNode.AppendChild(currentNode);
                    break;
            }
        }

        private XmlElement LoadElementNode(bool root) {
            Debug.Assert( reader.NodeType == XmlNodeType.Element );
            
            XmlReader r = reader;
            bool fEmptyElement = r.IsEmptyElement;

            XmlElement element = dummyDocument.CreateElement( r.Prefix, r.LocalName, r.NamespaceURI );
            element.IsEmpty = fEmptyElement;
            
            if (root) {
                parentNode = element;
            }
            else {
                XmlAttributeCollection attributes = element.Attributes;
                if (r.MoveToFirstAttribute()) {
                    do {
                        if (Ref.Equal(r.NamespaceURI, schemaNames.NsXmlNs)) { //Namespace Attribute
                            annotationNSManager.AddNamespace(r.Prefix.Length == 0 ? string.Empty : reader.LocalName, reader.Value);
                        }
                        XmlAttribute attr = LoadAttributeNode();
                        attributes.Append( attr );
                    } while(r.MoveToNextAttribute());
                }
                r.MoveToElement();
                string ns = annotationNSManager.LookupNamespace(r.Prefix);
                if (ns == null) {
                    XmlAttribute attr = CreateXmlNsAttribute(r.Prefix, namespaceManager.LookupNamespace(r.Prefix)); 
                    attributes.Append(attr);
                }
                else if (ns.Length == 0) { //string.Empty prefix is mapped to string.Empty NS by default
                    string elemNS = namespaceManager.LookupNamespace(r.Prefix);
                    if (elemNS != string.Empty) {
                        XmlAttribute attr = CreateXmlNsAttribute(r.Prefix, elemNS); 
                        attributes.Append(attr);
                    }
                }

                while (r.MoveToNextAttribute()) {
                    if (r.Prefix.Length != 0) {
                        string attNS = annotationNSManager.LookupNamespace(r.Prefix);
                        if (attNS == null) {
                            XmlAttribute attr = CreateXmlNsAttribute(r.Prefix, namespaceManager.LookupNamespace(r.Prefix)); 
                            attributes.Append(attr);
                        }
                    }
                }
                r.MoveToElement();

                parentNode.AppendChild(element);
                if (!r.IsEmptyElement) {
                    parentNode = element;
                }
            }
            return element;
        }
        
        private XmlAttribute CreateXmlNsAttribute(string prefix, string value) {
            XmlAttribute attr;
            if (prefix.Length == 0) {
                attr = dummyDocument.CreateAttribute(string.Empty, xmlns, XmlReservedNs.NsXmlNs);
            }
            else {
                attr = dummyDocument.CreateAttribute(xmlns, prefix, XmlReservedNs.NsXmlNs);
            }
            attr.AppendChild(dummyDocument.CreateTextNode(value));
            annotationNSManager.AddNamespace(prefix, value);
            return attr;
        }

        private XmlAttribute LoadAttributeNode() {
            Debug.Assert(reader.NodeType == XmlNodeType.Attribute);

            XmlReader r = reader;

            XmlAttribute attr = dummyDocument.CreateAttribute(r.Prefix, r.LocalName, r.NamespaceURI);

            while (r.ReadAttributeValue() ) {
                switch (r.NodeType) {
                    case XmlNodeType.Text:
                        attr.AppendChild(dummyDocument.CreateTextNode(r.Value));
                        continue;
                    case XmlNodeType.EntityReference:
                        attr.AppendChild(LoadEntityReferenceInAttribute());
                        continue;
                    default:
                        throw XmlLoader.UnexpectedNodeType( r.NodeType );
                }
            }

            return attr;
        }

        private XmlEntityReference LoadEntityReferenceInAttribute() {
            Debug.Assert(reader.NodeType == XmlNodeType.EntityReference);

            XmlEntityReference eref = dummyDocument.CreateEntityReference( reader.LocalName );
            if ( !reader.CanResolveEntity ) {
                return eref;
            }
            reader.ResolveEntity();

            while (reader.ReadAttributeValue()) {
                switch (reader.NodeType) {
                    case XmlNodeType.Text:
                        eref.AppendChild(dummyDocument.CreateTextNode(reader.Value));
                        continue;
                    case XmlNodeType.EndEntity:
                        if ( eref.ChildNodes.Count == 0 ) {
                            eref.AppendChild(dummyDocument.CreateTextNode(String.Empty));
                        }
                        return eref;
                    case XmlNodeType.EntityReference: 
                        eref.AppendChild(LoadEntityReferenceInAttribute());
                        break;
                    default:
                        throw XmlLoader.UnexpectedNodeType( reader.NodeType );
                }
            }

            return eref;
        }

    };

} // namespace System.Xml
