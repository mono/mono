//---------------------------------------------------------------------
// <copyright file="SchemaElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Summary description for SchemaElement.
    /// </summary>
    [DebuggerDisplay("Name={Name}")]
    internal abstract class SchemaElement
    {
        // see http://www.w3.org/TR/2006/REC-xml-names-20060816/
        internal const string XmlNamespaceNamespace = "http://www.w3.org/2000/xmlns/";


        #region Instance Fields
        private SchemaElement _parentElement = null;
        private Schema _schema = null;
        private int _lineNumber = 0;
        private int _linePosition = 0;
        private string _name = null;
        private DocumentationElement _documentation = null;

        private List<MetadataProperty> _otherContent;

        #endregion

        #region Static Fields
        /// <summary></summary>
        protected const int MaxValueVersionComponent = short.MaxValue;
        #endregion

        #region Public Properties
        /// <summary>
        /// 
        /// </summary>
        internal  int LineNumber
        {
            get
            {
                return _lineNumber;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal  int LinePosition
        {
            get
            {
                return _linePosition;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal  DocumentationElement Documentation
        {
            get
            {
                return _documentation;
            }
            set
            {
                _documentation = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal  SchemaElement ParentElement
        {
            get
            {
                return _parentElement;
            }
            private set
            {
                _parentElement = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal Schema Schema
        {
            get
            {
                return _schema;
            }
            set
            {
                _schema = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public  virtual string FQName
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Identity
        {
            get
            {
                return Name;
            }
        }

       
        public List<MetadataProperty> OtherContent
        {
            get 
            {
                if (_otherContent == null)
                {
                    _otherContent = new List<MetadataProperty>();
                }

                return _otherContent; 
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Validates this element and its children
        /// </summary>
        
        internal virtual void Validate()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="lineNumber"></param>
        /// <param name="linePosition"></param>
        /// <param name="message"></param>
        internal void AddError( ErrorCode errorCode, EdmSchemaErrorSeverity severity, int lineNumber, int linePosition, object message )
        {
            AddError(errorCode,severity,SchemaLocation,lineNumber,linePosition,message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="reader"></param>
        /// <param name="message"></param>
        internal void AddError( ErrorCode errorCode, EdmSchemaErrorSeverity severity, XmlReader reader, object message )
        {
            int lineNumber;
            int linePosition;
            GetPositionInfo(reader, out lineNumber, out linePosition);
            AddError(errorCode,severity,SchemaLocation,lineNumber,linePosition,message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="message"></param>
        internal void AddError( ErrorCode errorCode, EdmSchemaErrorSeverity severity, object message )
        {
            AddError(errorCode,severity,SchemaLocation,LineNumber,LinePosition,message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="element"></param>
        /// <param name="message"></param>
        internal void AddError( ErrorCode errorCode, EdmSchemaErrorSeverity severity, SchemaElement element, object message )
        {
            AddError(errorCode,severity,element.Schema.Location,element.LineNumber,element.LinePosition,message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal void Parse(XmlReader reader)
        {
            GetPositionInfo(reader);

            bool hasEndElement = !reader.IsEmptyElement;

            Debug.Assert(reader.NodeType == XmlNodeType.Element);
            for ( bool more = reader.MoveToFirstAttribute(); more; more = reader.MoveToNextAttribute() )
            {
                ParseAttribute(reader);
            }
            HandleAttributesComplete();

            bool done = !hasEndElement;
            bool skipToNextElement = false;
            while ( !done )
            {
                if ( skipToNextElement )
                {
                    skipToNextElement = false;
                    reader.Skip();
                    if ( reader.EOF )
                        break;
                }
                else
                {
                    if ( !reader.Read() )
                        break;
                }
                switch ( reader.NodeType )
                {
                    case XmlNodeType.Element:
                        skipToNextElement = ParseElement(reader);
                        break;

                    case XmlNodeType.EndElement:
                    {
                        done = true;
                        break;
                    }

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        ParseText(reader);
                        break;

                        // we ignore these childless elements
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Notation:
                    case XmlNodeType.ProcessingInstruction:
                    {
                        break;
                    }

                        // we ignore these elements that can have children
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.EntityReference:
                    {
                        skipToNextElement = true;
                        break;
                    }

                    default:
                    {
                        AddError( ErrorCode.UnexpectedXmlNodeType, EdmSchemaErrorSeverity.Error, reader,
                            System.Data.Entity.Strings.UnexpectedXmlNodeType(reader.NodeType));
                        skipToNextElement = true;
                        break;
                    }
                }
            }
            HandleChildElementsComplete();
            if ( reader.EOF && reader.Depth > 0 )
            {
                AddError( ErrorCode.MalformedXml, EdmSchemaErrorSeverity.Error, 0, 0,
                    System.Data.Entity.Strings.MalformedXml(LineNumber,LinePosition));
            }
        }

        /// <summary>
        /// Set the current line number and position for an XmlReader
        /// </summary>
        /// <param name="reader">the reader whose position is desired</param>
        internal void GetPositionInfo(XmlReader reader)
        {
            GetPositionInfo(reader,out _lineNumber,out _linePosition);
        }

        /// <summary>
        /// Get the current line number and position for an XmlReader
        /// </summary>
        /// <param name="reader">the reader whose position is desired</param>
        /// <param name="lineNumber">the line number</param>
        /// <param name="linePosition">the line position</param>
        internal static void GetPositionInfo(XmlReader reader, out int lineNumber, out int linePosition)
        {
            IXmlLineInfo xmlLineInfo = reader as IXmlLineInfo;
            if ( xmlLineInfo != null && xmlLineInfo.HasLineInfo() )
            {
                lineNumber = xmlLineInfo.LineNumber;
                linePosition = xmlLineInfo.LinePosition;
            }
            else
            {
                lineNumber = 0;
                linePosition = 0;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        internal virtual void ResolveTopLevelNames()
        {
        }
        internal virtual void ResolveSecondLevelNames()
        {
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal SchemaElement(SchemaElement parentElement)
        {
            if ( parentElement != null )
            {
                ParentElement = parentElement;
                for ( SchemaElement element = parentElement; element != null; element = element.ParentElement )
                {
                    Schema schema = element as Schema;
                    if ( schema != null )
                    {
                        Schema = schema;
                        break;
                    }
                }
                
                if (Schema == null)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.AllElementsMustBeInSchema);
                }
            }
        }

        internal SchemaElement(SchemaElement parentElement, string name)
            : this(parentElement)
        {
            _name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void HandleAttributesComplete()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void HandleChildElementsComplete()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected string HandleUndottedNameAttribute(XmlReader reader, string field)
        {
            string name = field;
            Debug.Assert(string.IsNullOrEmpty(field), string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            bool success = Utils.GetUndottedName(Schema, reader, out name);
            if ( !success )
                return name;

            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="field"></param>
        /// <param name="errorMessageId"></param>
        /// <returns></returns>
        protected ReturnValue<string> HandleDottedNameAttribute(XmlReader reader, string field, Func<object, string> errorFormat)
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();
            Debug.Assert(string.IsNullOrEmpty(field), string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            string value;
            if ( !Utils.GetDottedName(Schema,reader,out value) )
                return returnValue;

            returnValue.Value = value;
            return returnValue;
        }

        /// <summary>
        /// Use to handle an attribute with an int data type
        /// </summary>
        /// <param name="reader">the reader positioned at the int attribute</param>
        /// <param name="field">The int field to be given the value found</param>
        /// <returns>true if an int value was successfuly extracted from the attribute, false otherwise.</returns>
        internal bool HandleIntAttribute(XmlReader reader, ref int field)
        {
            int value;
            if ( !Utils.GetInt(Schema, reader, out value) )
                return false;

            field = value;
            return true;
        }

        /// <summary>
        /// Use to handle an attribute with an int data type
        /// </summary>
        /// <param name="reader">the reader positioned at the int attribute</param>
        /// <param name="field">The int field to be given the value found</param>
        /// <returns>true if an int value was successfuly extracted from the attribute, false otherwise.</returns>
        internal bool HandleByteAttribute(XmlReader reader, ref byte field)
        {
            byte value;
            if ( !Utils.GetByte(Schema, reader, out value) )
                return false;

            field = value;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        internal bool HandleBoolAttribute(XmlReader reader, ref bool field)
        {
            bool value;
            if ( !Utils.GetBool(Schema,reader,out value) )
                return false;

            field = value;
            return true;
        }

        /// <summary>
        /// Use this to jump through an element that doesn't need any processing
        /// </summary>
        /// <param name="reader">xml reader currently positioned at an element</param>
        protected virtual void SkipThroughElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            Parse(reader);
        }

        protected void SkipElement(XmlReader reader)
        {
            using (XmlReader subtree = reader.ReadSubtree())
            {
                while (subtree.Read()) ;
            }
        }

        #endregion

        #region Protected Properties
        /// <summary>
        /// 
        /// </summary>
        protected string SchemaLocation
        {
            get
            {
                if ( Schema != null )
                    return Schema.Location;
                return null;
            }
        }

        protected virtual bool HandleText(XmlReader reader)
        {
            return false;
        }

        internal virtual SchemaElement Clone(SchemaElement parentElement)
        {
            throw Error.NotImplemented();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private void HandleDocumentationElement(XmlReader reader)
        {
            Documentation = new DocumentationElement(this);
            Documentation.Parse(reader);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        protected virtual void HandleNameAttribute(XmlReader reader)
        {
            Name = HandleUndottedNameAttribute(reader, Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="source"></param>
        /// <param name="lineNumber"></param>
        /// <param name="linePosition"></param>
        /// <param name="message"></param>
        private void AddError( ErrorCode errorCode, EdmSchemaErrorSeverity severity, string sourceLocation, int lineNumber, int linePosition, object message )
        {
            EdmSchemaError error = null;
            string messageString = message as string;
            if ( messageString != null )
                error = new EdmSchemaError( messageString, (int)errorCode, severity, sourceLocation, lineNumber, linePosition );
            else
            {
                Exception ex = message as Exception;
                if ( ex != null )
                    error = new EdmSchemaError( ex.Message, (int)errorCode, severity, sourceLocation, lineNumber, linePosition, ex );
                else
                    error = new EdmSchemaError( message.ToString(), (int)errorCode, severity, sourceLocation, lineNumber, linePosition );
            }
            Schema.AddError(error);
        }

        /// <summary>
        /// Call handler for the current attribute
        /// </summary>
        /// <param name="reader">XmlReader positioned at the attribute</param>
        private void ParseAttribute(XmlReader reader)
        {
#if false
            // the attribute value is schema invalid, just skip it; this avoids some duplicate errors at the expense of better error messages...
            if ( reader.SchemaInfo != null && reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid )
                continue;
#endif
            string attributeNamespace = reader.NamespaceURI;
            if (attributeNamespace == XmlConstants.AnnotationNamespace 
                && reader.LocalName == XmlConstants.UseStrongSpatialTypes
                && !ProhibitAttribute(attributeNamespace, reader.LocalName) 
                && HandleAttribute(reader))
            {
                return;
            }
            else if (!Schema.IsParseableXmlNamespace(attributeNamespace, true))
            {
                AddOtherContent(reader);                
            }
            else if (!ProhibitAttribute(attributeNamespace, reader.LocalName)&&
                     HandleAttribute(reader))
            {
                return;
            }
            else if (reader.SchemaInfo == null || reader.SchemaInfo.Validity != System.Xml.Schema.XmlSchemaValidity.Invalid)
            {
                // there's no handler for (namespace,name) and there wasn't a validation error. 
                // Report an error of our own if the node is in no namespace or if it is in one of our xml schemas tartget namespace.
                if (string.IsNullOrEmpty(attributeNamespace) || Schema.IsParseableXmlNamespace(attributeNamespace, true))
                {
                    AddError(ErrorCode.UnexpectedXmlAttribute, EdmSchemaErrorSeverity.Error, reader, System.Data.Entity.Strings.UnexpectedXmlAttribute(reader.Name));
                }
            }
        }

        protected virtual bool ProhibitAttribute(string namespaceUri, string localName)
        {
            return false;
        }

        /// <summary>
        /// This overload assumes the default namespace
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="localName"></param>
        /// <returns></returns>
        internal static bool CanHandleAttribute(XmlReader reader, string localName)
        {
            Debug.Assert(reader.NamespaceURI != null);
            return reader.NamespaceURI.Length == 0 && reader.LocalName == localName;
        }

        protected virtual bool HandleAttribute(XmlReader reader)
        {
            if(CanHandleAttribute(reader, XmlConstants.Name))
            {
                HandleNameAttribute(reader);
                return true;
            }

            return false;
        }

        private bool AddOtherContent(XmlReader reader)
        {
            int lineNumber;
            int linePosition;
            GetPositionInfo(reader, out lineNumber, out linePosition);

            MetadataProperty property;
            if (reader.NodeType == XmlNodeType.Element)
            {

                if (this._schema.SchemaVersion == XmlConstants.EdmVersionForV1 ||
                    this._schema.SchemaVersion == XmlConstants.EdmVersionForV1_1)
                {
                    // skip this element
                    // we don't support element annotations in v1 and v1.1
                    return true;
                }

                // in V1 and V1.1 the codegen can only appear as the attribute annotation and we want to maintain
                // the same behavior for V2, thus we throw if we encounter CodeGen namespace 
                // in structural annotation in V2 and furthur version
                if (this._schema.SchemaVersion >= XmlConstants.EdmVersionForV2 
                    && reader.NamespaceURI == XmlConstants.CodeGenerationSchemaNamespace)
                {
                    Debug.Assert(
                        XmlConstants.SchemaVersionLatest == XmlConstants.EdmVersionForV3, 
                        "Please add checking for the latest namespace");

                    AddError(ErrorCode.NoCodeGenNamespaceInStructuralAnnotation, EdmSchemaErrorSeverity.Error, lineNumber, linePosition, Strings.NoCodeGenNamespaceInStructuralAnnotation(XmlConstants.CodeGenerationSchemaNamespace));
                    return true;
                }

                Debug.Assert(
                        !Schema.IsParseableXmlNamespace(reader.NamespaceURI, false),
                        "Structural annotation cannot use any edm reserved namespaces");

                // using this subtree aproach because when I call 
                // reader.ReadOuterXml() it positions me at the Node beyond
                // the end of the node I am starting on
                // which doesn't work with the parsing logic
                using (XmlReader subtree = reader.ReadSubtree())
                {
                    subtree.Read();
                    XElement element = XElement.Load(new StringReader(subtree.ReadOuterXml()));

                    property = CreateMetadataPropertyFromOtherNamespaceXmlArtifact(element.Name.NamespaceName, element.Name.LocalName, element);
                }
            }
            else
            {
                if (reader.NamespaceURI == XmlNamespaceNamespace)
                {
                    // we don't bring in namespace definitions
                    return true;
                }

                Debug.Assert(reader.NodeType == XmlNodeType.Attribute, "called an attribute function when not on an attribute");
                property = CreateMetadataPropertyFromOtherNamespaceXmlArtifact(reader.NamespaceURI, reader.LocalName, reader.Value);
            }

            if (!OtherContent.Exists(mp => mp.Identity == property.Identity))
            {
                OtherContent.Add(property);
            }
            else
            {
                AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, lineNumber, linePosition, Strings.DuplicateAnnotation(property.Identity, this.FQName));
            }
            return false;
        }

        internal static MetadataProperty CreateMetadataPropertyFromOtherNamespaceXmlArtifact(string xmlNamespaceUri, string artifactName, object value)
        {
            MetadataProperty property;
            property = new MetadataProperty(xmlNamespaceUri + ":" + artifactName,
                                       TypeUsage.Create(EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.String)),
                                       value);
            return property;
        }

        /// <summary>
        /// Call handler for the current element
        /// </summary>
        /// <param name="reader">XmlReader positioned at the element</param>
        /// <returns>true if element content should be skipped</returns>
        private bool ParseElement(XmlReader reader)
        {
            string elementNamespace = reader.NamespaceURI;
            // for schema element that right under the schema, we just ignore them, since schema does not
            // have metadataproperties
            if (!Schema.IsParseableXmlNamespace(elementNamespace, true) && this.ParentElement != null)
            {
                return AddOtherContent(reader);
            }
            if (HandleElement(reader))
            {
                return false;
            }
            else
            {

                // we need to report an error if the namespace for this element is a target namespace for the xml schemas we are parsing against.
                // otherwise we assume that this is either a valid 'any' element or that the xsd validator has generated an error
                if (string.IsNullOrEmpty(elementNamespace) || Schema.IsParseableXmlNamespace(reader.NamespaceURI, false))
                {
                    AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, reader, System.Data.Entity.Strings.UnexpectedXmlElement(reader.Name));
                }
                return true;
            }
        }

        protected bool CanHandleElement(XmlReader reader, string localName)
        {
            return reader.NamespaceURI == Schema.SchemaXmlNamespace && reader.LocalName == localName;
        }

        protected virtual bool HandleElement(XmlReader reader)
        {
            if (CanHandleElement(reader, XmlConstants.Documentation))
            {
                HandleDocumentationElement(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle text data.
        /// </summary>
        /// <param name="reader">XmlReader positioned at Text, CData, or SignificantWhitespace </param>
        private void ParseText(XmlReader reader)
        {
            if (HandleText(reader))
            {
                return;
            }
            else if (reader.Value != null && reader.Value.Trim().Length == 0)
            {
                // just ignore this text.  Don't add an error, since the value is just whitespace.
            }
            else
            {
                AddError( ErrorCode.TextNotAllowed, EdmSchemaErrorSeverity.Error, reader, System.Data.Entity.Strings.TextNotAllowed(reader.Value ) );
            }
        }
        #endregion

        [Conditional("DEBUG")]
        internal static void AssertReaderConsidersSchemaInvalid(XmlReader reader)
        {
            Debug.Assert(reader.SchemaInfo == null ||
                         reader.SchemaInfo.Validity != System.Xml.Schema.XmlSchemaValidity.Valid, "The xsd should see this as not acceptable");
        }

    }
}
