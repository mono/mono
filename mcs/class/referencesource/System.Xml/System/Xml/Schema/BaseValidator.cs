//------------------------------------------------------------------------------
// <copyright file="BaseValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                              
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.IO;
    using System.Diagnostics;
    using System.Xml;
    using System.Text;
    using System.Collections;

#pragma warning disable 618

    internal class BaseValidator {
        XmlSchemaCollection schemaCollection;
        IValidationEventHandling eventHandling;
        XmlNameTable nameTable;
        SchemaNames schemaNames;
        PositionInfo positionInfo;
        XmlResolver xmlResolver;
        Uri baseUri;

        protected SchemaInfo schemaInfo;
        protected XmlValidatingReaderImpl reader;
        protected XmlQualifiedName elementName;
        protected ValidationState context;
        protected StringBuilder    textValue;
        protected string           textString;
        protected bool             hasSibling;
        protected bool             checkDatatype;

        public BaseValidator(BaseValidator other) {
            reader = other.reader;
            schemaCollection = other.schemaCollection;
            eventHandling = other.eventHandling;
            nameTable = other.nameTable;
            schemaNames = other.schemaNames;
            positionInfo = other.positionInfo;
            xmlResolver = other.xmlResolver;
            baseUri = other.baseUri;
            elementName = other.elementName;
        }

        public BaseValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling) {
            Debug.Assert(schemaCollection == null || schemaCollection.NameTable == reader.NameTable);
            this.reader = reader;
            this.schemaCollection = schemaCollection;
            this.eventHandling = eventHandling;
            nameTable = reader.NameTable;
            positionInfo = PositionInfo.GetPositionInfo(reader);
            elementName = new XmlQualifiedName();
        }

        public XmlValidatingReaderImpl Reader {
            get { return reader; }
        }

        public XmlSchemaCollection SchemaCollection {
            get { return schemaCollection; }
        }

        public XmlNameTable NameTable {
            get { return nameTable; }
        }
        
        public SchemaNames SchemaNames {
            get { 
                if (schemaNames != null) {
                    return schemaNames;
                }
                if (schemaCollection != null) {
                    schemaNames = schemaCollection.GetSchemaNames(nameTable);
                }
                else {
                    schemaNames = new SchemaNames(nameTable);
                }
                return schemaNames; 
            }
        }

        public PositionInfo PositionInfo {
            get { return positionInfo; }
        }

        public XmlResolver XmlResolver {
            get { return xmlResolver; }
            set { xmlResolver = value; }
        }

        public Uri BaseUri {
            get { return baseUri; }
            set { baseUri = value; }
        }

        public ValidationEventHandler EventHandler {
            get { return (ValidationEventHandler)eventHandling.EventHandler; }
        }

        public SchemaInfo SchemaInfo {
            get {
                return schemaInfo;
            }
            set {
                schemaInfo = value;
            }
        }

        public IDtdInfo DtdInfo {
            get {
                return schemaInfo;
            }
            set {
                SchemaInfo tmpSchemaInfo  = value as SchemaInfo;
                if (tmpSchemaInfo == null) {
                    throw new XmlException(Res.Xml_InternalError, string.Empty);
                }
                this.schemaInfo = tmpSchemaInfo;
            }
        }

        public virtual bool PreserveWhitespace { 
            get {
                return false;
            }
        }

        public virtual void Validate() {
        }

        public virtual void CompleteValidation() {
        }
        
        public virtual object  FindId(string name) {
            return null;
        }
        
        public void ValidateText() {
            if (context.NeedValidateChildren) {
                if (context.IsNill) {
                    SendValidationEvent(Res.Sch_ContentInNill, XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
                    return;
                }
                ContentValidator contentValidator = context.ElementDecl.ContentValidator;
                XmlSchemaContentType contentType = contentValidator.ContentType;
                if (contentType == XmlSchemaContentType.ElementOnly) {
                    ArrayList names = contentValidator.ExpectedElements(context, false); 
                    if (names == null) {
                        SendValidationEvent(Res.Sch_InvalidTextInElement, XmlSchemaValidator.BuildElementName(context.LocalName, context.Namespace));
                    }
                    else {
                        Debug.Assert(names.Count > 0);
                        SendValidationEvent(Res.Sch_InvalidTextInElementExpecting, new string[] { XmlSchemaValidator.BuildElementName(context.LocalName, context.Namespace), XmlSchemaValidator.PrintExpectedElements(names, false) });
                    }
                }
                else if (contentType == XmlSchemaContentType.Empty) {
                    SendValidationEvent(Res.Sch_InvalidTextInEmpty, string.Empty);
                }
                if (checkDatatype) {
                    SaveTextValue(reader.Value);
                }
            }
        }
        
        public void ValidateWhitespace() {
            if (context.NeedValidateChildren) {
                XmlSchemaContentType contentType = context.ElementDecl.ContentValidator.ContentType;
                if (context.IsNill) {
                    SendValidationEvent(Res.Sch_ContentInNill, XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
                }
                if (contentType == XmlSchemaContentType.Empty) {
                    SendValidationEvent(Res.Sch_InvalidWhitespaceInEmpty, string.Empty);
                }
                if (checkDatatype) {
                    SaveTextValue(reader.Value);
                }
            }
        }

        private void SaveTextValue(string value) {
            if (textString.Length == 0) {
                textString = value;
            }
            else {
                if (!hasSibling) {
                    textValue.Append(textString);
                    hasSibling = true;
                }
                textValue.Append(value);
            }
        }

        protected void SendValidationEvent(string code) {
            SendValidationEvent(code, string.Empty);
        }

        protected void SendValidationEvent(string code, string[] args) {
            SendValidationEvent(new XmlSchemaException(code, args, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition));
        }

        protected void SendValidationEvent(string code, string arg) {
            SendValidationEvent(new XmlSchemaException(code, arg, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition));
        }

        protected void SendValidationEvent(string code, string arg1, string arg2) {
            SendValidationEvent(new XmlSchemaException(code, new string[] { arg1, arg2 }, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition));
        }

        protected void SendValidationEvent(XmlSchemaException e) {
            SendValidationEvent(e, XmlSeverityType.Error);
        }
        
        protected void SendValidationEvent(string code, string msg, XmlSeverityType severity) {
            SendValidationEvent(new XmlSchemaException(code, msg, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition), severity);
        }

        protected void SendValidationEvent(string code, string[] args, XmlSeverityType severity) {
            SendValidationEvent(new XmlSchemaException(code, args, reader.BaseURI, positionInfo.LineNumber, positionInfo.LinePosition), severity);
        }

        protected void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity) {
            if (eventHandling != null) {
                eventHandling.SendEvent(e, severity);
            }
            else if (severity == XmlSeverityType.Error) {
                throw e;
            }
        }

        protected static void ProcessEntity(SchemaInfo sinfo, string name, object sender, ValidationEventHandler eventhandler, string baseUri, int lineNumber, int linePosition) {
            SchemaEntity en;
            XmlSchemaException e = null;
            if (!sinfo.GeneralEntities.TryGetValue(new XmlQualifiedName(name), out en)) {
                // validation error, see xml spec [68]
                e = new XmlSchemaException(Res.Sch_UndeclaredEntity, name, baseUri, lineNumber, linePosition);
            }
            else if (en.NData.IsEmpty) {
                e = new XmlSchemaException(Res.Sch_UnparsedEntityRef, name, baseUri, lineNumber, linePosition);
            }
            if (e != null) {
                if (eventhandler != null) {
                    eventhandler(sender, new ValidationEventArgs(e));
                }
                else {
                    throw e;
                }
            }
        }

        protected static void ProcessEntity(SchemaInfo sinfo, string name, IValidationEventHandling eventHandling, string baseUriStr, int lineNumber, int linePosition) {
            SchemaEntity en;
            string errorResId = null;
            if (!sinfo.GeneralEntities.TryGetValue(new XmlQualifiedName(name), out en)) {
                // validation error, see xml spec [68]
                errorResId = Res.Sch_UndeclaredEntity;

            }
            else if (en.NData.IsEmpty) {
                errorResId = Res.Sch_UnparsedEntityRef;
            }
            if (errorResId != null) {
                XmlSchemaException e = new XmlSchemaException(errorResId, name, baseUriStr, lineNumber, linePosition);

                if (eventHandling != null) {
                    eventHandling.SendEvent(e, XmlSeverityType.Error);
                }
                else {
                    throw e;
                }
            }
        }

        public static BaseValidator CreateInstance(ValidationType valType, XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling, bool processIdentityConstraints) {
            switch(valType) {
                case ValidationType.XDR:
                    return new XdrValidator(reader, schemaCollection, eventHandling);

                case ValidationType.Schema:
                    return new XsdValidator(reader, schemaCollection, eventHandling);
                   
                case ValidationType.DTD:
                    return new DtdValidator(reader, eventHandling, processIdentityConstraints);
                    
                case ValidationType.Auto:
                    return new AutoValidator(reader, schemaCollection, eventHandling);

                case ValidationType.None:
                    return new BaseValidator(reader, schemaCollection, eventHandling);

                default:
                        break;
            }
            return null;
        }

    }
#pragma warning restore 618

}

        
