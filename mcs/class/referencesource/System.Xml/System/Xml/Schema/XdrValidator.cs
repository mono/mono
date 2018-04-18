//------------------------------------------------------------------------------
// <copyright file="XdrValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Versioning;

#pragma warning disable 618
    internal sealed class XdrValidator : BaseValidator {
        
        private const int        STACK_INCREMENT = 10;
        private HWStack          validationStack;  // validaton contexts
        private Hashtable        attPresence;
        private XmlQualifiedName name = XmlQualifiedName.Empty;
        private XmlNamespaceManager  nsManager;
        private bool isProcessContents = false;
        private Hashtable       IDs;
        private IdRefNode       idRefListHead;
        private Parser  inlineSchemaParser = null;
        private const string     x_schema = "x-schema:";

        internal XdrValidator(BaseValidator validator) : base(validator) {
            Init();
        }

        internal XdrValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling) : base(reader, schemaCollection, eventHandling) {
            Init();
        }

        private void Init() {
            nsManager = reader.NamespaceManager;
            if (nsManager == null) {
                nsManager = new XmlNamespaceManager(NameTable);
                isProcessContents = true;
            }
            validationStack = new HWStack(STACK_INCREMENT);
            textValue = new StringBuilder();
            name = XmlQualifiedName.Empty;
            attPresence = new Hashtable();
            Push(XmlQualifiedName.Empty);
            schemaInfo = new SchemaInfo();
            checkDatatype = false;
        }

        public override void Validate() {
            if (IsInlineSchemaStarted) {
                ProcessInlineSchema();
            }
            else {
                switch (reader.NodeType) {
                    case XmlNodeType.Element:
                        ValidateElement();
                        if (reader.IsEmptyElement) {
                            goto case XmlNodeType.EndElement;
                        }
                        break;
                    case XmlNodeType.Whitespace:
                        ValidateWhitespace();
                        break;
                    case XmlNodeType.Text:          // text inside a node
                    case XmlNodeType.CDATA:         // <![CDATA[...]]>
                    case XmlNodeType.SignificantWhitespace:
                        ValidateText();
                        break;
                    case XmlNodeType.EndElement:
                        ValidateEndElement();
                        break;
                }
            }
        }   

        private void ValidateElement() {
            elementName.Init(reader.LocalName, XmlSchemaDatatype.XdrCanonizeUri(reader.NamespaceURI, NameTable, SchemaNames));
            ValidateChildElement();
            if (SchemaNames.IsXDRRoot(elementName.Name, elementName.Namespace) && reader.Depth > 0) {
                inlineSchemaParser = new Parser(SchemaType.XDR, NameTable, SchemaNames, EventHandler);
                inlineSchemaParser.StartParsing(reader, null);
                inlineSchemaParser.ParseReaderNode();
            }
            else {
                ProcessElement();
            }
        }
        
        private void ValidateChildElement() {
            if (context.NeedValidateChildren) {
                int errorCode = 0;
                context.ElementDecl.ContentValidator.ValidateElement(elementName, context, out errorCode);
                if (errorCode < 0) {
                    XmlSchemaValidator.ElementValidationError(elementName, context, EventHandler, reader, reader.BaseURI, PositionInfo.LineNumber, PositionInfo.LinePosition, null);
                }
            }
        }

        private bool IsInlineSchemaStarted {
            get { return inlineSchemaParser != null; }
        }
        
        private void ProcessInlineSchema() {
            if (!inlineSchemaParser.ParseReaderNode()) { // Done
                    inlineSchemaParser.FinishParsing();
                    SchemaInfo xdrSchema = inlineSchemaParser.XdrSchema;
                    if (xdrSchema != null && xdrSchema.ErrorCount == 0) {
                        foreach(string inlineNS in xdrSchema.TargetNamespaces.Keys) {
                            if (!this.schemaInfo.HasSchema(inlineNS)) {
                                schemaInfo.Add(xdrSchema, EventHandler);
                                SchemaCollection.Add(inlineNS, xdrSchema, null, false);
                                break;
                            }
                        }
                    }
                    inlineSchemaParser = null;
            }
        }
        
        private void ProcessElement() {
            Push(elementName);
            if (isProcessContents) {
                nsManager.PopScope();
            }
            context.ElementDecl = ThoroughGetElementDecl();
            if (context.ElementDecl != null) {
                ValidateStartElement();
                ValidateEndStartElement();
                context.NeedValidateChildren = true;
                context.ElementDecl.ContentValidator.InitValidation(context);
            }
        }
        
         private void ValidateEndElement() {
            if (isProcessContents) {
                nsManager.PopScope();
            }
            if (context.ElementDecl != null) {
                if (context.NeedValidateChildren) {
                    if(!context.ElementDecl.ContentValidator.CompleteValidation(context)) {
                        XmlSchemaValidator.CompleteValidationError(context, EventHandler, reader, reader.BaseURI, PositionInfo.LineNumber, PositionInfo.LinePosition, null);
                    }
                }
                if (checkDatatype) {
                    string stringValue = !hasSibling ? textString : textValue.ToString();  // only for identity-constraint exception reporting
                    CheckValue(stringValue, null);
                    checkDatatype = false;
                    textValue.Length = 0; // cleanup
                    textString = string.Empty;
                }
            }
            Pop();

        }

         // SxS: This method processes resource names read from the source document and does not expose
         // any resources to the caller. It is fine to suppress the SxS warning. 
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private SchemaElementDecl ThoroughGetElementDecl() {
            if (reader.Depth == 0) {
                LoadSchema(string.Empty);
            }
            if (reader.MoveToFirstAttribute()) {
                do {
                    string objectNs = reader.NamespaceURI;
                    string objectName = reader.LocalName;
                    if (Ref.Equal(objectNs, SchemaNames.NsXmlNs)) {
                        LoadSchema(reader.Value);
                        if (isProcessContents) {
                            nsManager.AddNamespace(reader.Prefix.Length == 0 ? string.Empty : reader.LocalName, reader.Value);
                        }
                    }
                    if (             
                        Ref.Equal(objectNs, SchemaNames.QnDtDt.Namespace) &&
                        Ref.Equal(objectName, SchemaNames.QnDtDt.Name)
                    ) {
                        reader.SchemaTypeObject = XmlSchemaDatatype.FromXdrName(reader.Value);
                    }
                    
                } while(reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
            SchemaElementDecl elementDecl = schemaInfo.GetElementDecl(elementName);
            if(elementDecl == null) {
                if(schemaInfo.TargetNamespaces.ContainsKey(context.Namespace)) {
                    SendValidationEvent(Res.Sch_UndeclaredElement, XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
                }
            }
            return elementDecl;
        }
        
        private void ValidateStartElement() {
            if (context.ElementDecl != null) {
                if (context.ElementDecl.SchemaType != null) {
                    reader.SchemaTypeObject =  context.ElementDecl.SchemaType;
                }
                else {
                    reader.SchemaTypeObject =  context.ElementDecl.Datatype;
                }
                if (reader.IsEmptyElement && !context.IsNill && context.ElementDecl.DefaultValueTyped != null) {
                   reader.TypedValueObject = context.ElementDecl.DefaultValueTyped;
                   context.IsNill = true; // reusing IsNill
                }
                if (this.context.ElementDecl.HasRequiredAttribute) {
                    attPresence.Clear();
                }   
            }

            if (reader.MoveToFirstAttribute()) {
                do {
                    if ((object)reader.NamespaceURI == (object)SchemaNames.NsXmlNs) {
                        continue;
                    }
                    
                    try {
                        reader.SchemaTypeObject = null;
                        SchemaAttDef attnDef = schemaInfo.GetAttributeXdr(context.ElementDecl, QualifiedName(reader.LocalName, reader.NamespaceURI));
                        if (attnDef != null) {
                            if (context.ElementDecl != null && context.ElementDecl.HasRequiredAttribute) {
                                attPresence.Add(attnDef.Name, attnDef);
                            }
                            reader.SchemaTypeObject = (attnDef.SchemaType != null) ? (object)attnDef.SchemaType : (object)attnDef.Datatype;
                            if (attnDef.Datatype != null) {
                                string attributeValue = reader.Value;
                                // need to check the contents of this attribute to make sure
                                // it is valid according to the specified attribute type.
                                CheckValue(attributeValue, attnDef);
                            }
                        }
                    }
                    catch (XmlSchemaException e) {
                        e.SetSource(reader.BaseURI, PositionInfo.LineNumber, PositionInfo.LinePosition);
                        SendValidationEvent(e);
                    }
                } while(reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
        }
        
        private void ValidateEndStartElement() {

            if (context.ElementDecl.HasDefaultAttribute) {
                for (int i = 0; i < context.ElementDecl.DefaultAttDefs.Count; ++i) {
                    reader.AddDefaultAttribute((SchemaAttDef)context.ElementDecl.DefaultAttDefs[i]); 
               }
            }
            if (context.ElementDecl.HasRequiredAttribute) {
                try {
                    context.ElementDecl.CheckAttributes(attPresence, reader.StandAlone);
                }
                catch (XmlSchemaException e) {
                    e.SetSource(reader.BaseURI, PositionInfo.LineNumber, PositionInfo.LinePosition);
                    SendValidationEvent(e);
                }

            }
            if (context.ElementDecl.Datatype != null) {
                checkDatatype = true;
                hasSibling = false;
                textString = string.Empty;
                textValue.Length = 0;
            }
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        private void LoadSchemaFromLocation(string uri) {
            // is x-schema
            if (!XdrBuilder.IsXdrSchema(uri)) {
                return;
            }
            string url = uri.Substring(x_schema.Length);
            XmlReader reader = null;
            SchemaInfo xdrSchema = null;
            try {
                Uri ruri = this.XmlResolver.ResolveUri(BaseUri, url);
                Stream stm = (Stream)this.XmlResolver.GetEntity(ruri,null,null);
                reader = new XmlTextReader(ruri.ToString(), stm, NameTable);
                ((XmlTextReader)reader).XmlResolver = this.XmlResolver;
                Parser parser = new Parser(SchemaType.XDR, NameTable, SchemaNames, EventHandler);
                parser.XmlResolver = this.XmlResolver;
                parser.Parse(reader, uri);
                while(reader.Read());// wellformness check
                xdrSchema = parser.XdrSchema;
            }
            catch(XmlSchemaException e) {
                SendValidationEvent(Res.Sch_CannotLoadSchema, new string[] {uri, e.Message}, XmlSeverityType.Error);
            }
            catch(Exception e) {
                SendValidationEvent(Res.Sch_CannotLoadSchema, new string[] {uri, e.Message}, XmlSeverityType.Warning);
            }
            finally {
                if (reader != null) {
                    reader.Close();
                }
            }
            if (xdrSchema != null && xdrSchema.ErrorCount == 0) {
                schemaInfo.Add(xdrSchema, EventHandler);
                SchemaCollection.Add(uri, xdrSchema, null, false);
            }
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        private void LoadSchema(string uri) {
            if (this.schemaInfo.TargetNamespaces.ContainsKey(uri)) {
                return;
            }
            if (this.XmlResolver == null) {
                return;
            }

            SchemaInfo schemaInfo = null;
            if (SchemaCollection != null)
                schemaInfo = SchemaCollection.GetSchemaInfo(uri); 
            if (schemaInfo != null) {
                if(schemaInfo.SchemaType != SchemaType.XDR) {
                    throw new XmlException(Res.Xml_MultipleValidaitonTypes, string.Empty, this.PositionInfo.LineNumber, this.PositionInfo.LinePosition);
                }
                this.schemaInfo.Add(schemaInfo, EventHandler);
                return;
            }
            LoadSchemaFromLocation(uri);
        }
        
        private bool HasSchema { get { return schemaInfo.SchemaType != SchemaType.None;}}
        
        public override bool PreserveWhitespace { 
            get { return context.ElementDecl != null ? context.ElementDecl.ContentValidator.PreserveWhitespace : false; }
        }
        
        void ProcessTokenizedType(
            XmlTokenizedType    ttype,
            string              name
        ) {
            switch(ttype) {
            case XmlTokenizedType.ID:
                if (FindId(name) != null) {
                    SendValidationEvent(Res.Sch_DupId, name);
                }
                else {
                    AddID(name, context.LocalName);
                }
                break;
            case XmlTokenizedType.IDREF:
                object p = FindId(name);
                if (p == null) { // add it to linked list to check it later
                    idRefListHead = new IdRefNode(idRefListHead, name, this.PositionInfo.LineNumber, this.PositionInfo.LinePosition);
                }
                break;
            case XmlTokenizedType.ENTITY:
                ProcessEntity(schemaInfo, name, this, EventHandler, reader.BaseURI, PositionInfo.LineNumber, PositionInfo.LinePosition);
                break;
            default:
                break;
            }
        }
        
        
        public override void CompleteValidation() {
            if (HasSchema) {
                CheckForwardRefs();
            }
            else {
                SendValidationEvent(new XmlSchemaException(Res.Xml_NoValidation, string.Empty), XmlSeverityType.Warning);
            }
        }

        
        private void CheckValue(
            string              value,
            SchemaAttDef        attdef
        ) {
            try {
                reader.TypedValueObject = null;
                bool isAttn = attdef != null;
                XmlSchemaDatatype dtype = isAttn ? attdef.Datatype : context.ElementDecl.Datatype;
                if (dtype == null) {
                    return; // no reason to check
                }
                
                if (dtype.TokenizedType != XmlTokenizedType.CDATA) {
                    value = value.Trim();
                }
                if (value.Length == 0) {
                    return; // don't need to check
                }
            

                object typedValue = dtype.ParseValue(value, NameTable, nsManager);
                reader.TypedValueObject = typedValue;
                // Check special types
                XmlTokenizedType ttype = dtype.TokenizedType;
                if (ttype == XmlTokenizedType.ENTITY || ttype == XmlTokenizedType.ID || ttype == XmlTokenizedType.IDREF) {
                    if (dtype.Variety == XmlSchemaDatatypeVariety.List) {
                        string[] ss = (string[])typedValue;
                        for (int i = 0; i < ss.Length; ++i) {
                            ProcessTokenizedType(dtype.TokenizedType, ss[i]);
                        }
                    }
                    else {
                        ProcessTokenizedType(dtype.TokenizedType, (string)typedValue);
                    }
                }

                SchemaDeclBase decl = isAttn ? (SchemaDeclBase)attdef : (SchemaDeclBase)context.ElementDecl;

                if (decl.MaxLength != uint.MaxValue) {
                    if(value.Length > decl.MaxLength) {
                        SendValidationEvent(Res.Sch_MaxLengthConstraintFailed, value);
                    }
                }
                if (decl.MinLength != uint.MaxValue) {
                    if(value.Length < decl.MinLength) {
                        SendValidationEvent(Res.Sch_MinLengthConstraintFailed, value);
                    }
                }
                if (decl.Values != null && !decl.CheckEnumeration(typedValue)) {
                    if (dtype.TokenizedType == XmlTokenizedType.NOTATION) {
                        SendValidationEvent(Res.Sch_NotationValue, typedValue.ToString());
                    }
                    else {
                        SendValidationEvent(Res.Sch_EnumerationValue, typedValue.ToString());
                    }

                }
                if (!decl.CheckValue(typedValue)) {
                    if (isAttn) {
                        SendValidationEvent(Res.Sch_FixedAttributeValue, attdef.Name.ToString());
                    }
                    else {
                        SendValidationEvent(Res.Sch_FixedElementValue, XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
                    }
                }
            }
            catch (XmlSchemaException) {
                if (attdef != null) {
                    SendValidationEvent(Res.Sch_AttributeValueDataType, attdef.Name.ToString());
                }
                else {
                    SendValidationEvent(Res.Sch_ElementValueDataType, XmlSchemaValidator.QNameString(context.LocalName, context.Namespace));
                }
            }
        }

        public static void CheckDefaultValue(
            string              value,
            SchemaAttDef        attdef,
            SchemaInfo          sinfo,
            XmlNamespaceManager     nsManager,
            XmlNameTable        NameTable,
            object              sender,
            ValidationEventHandler  eventhandler,
            string              baseUri,
            int                 lineNo,
            int                 linePos
        ) {
            try {

                XmlSchemaDatatype dtype = attdef.Datatype;
                if (dtype == null) {
                    return; // no reason to check
                }
                
                if (dtype.TokenizedType != XmlTokenizedType.CDATA) {
                    value = value.Trim();
                }
                if (value.Length == 0) {
                    return; // don't need to check
                }
                object typedValue = dtype.ParseValue(value, NameTable, nsManager);

                // Check special types
                XmlTokenizedType ttype = dtype.TokenizedType;
                if (ttype == XmlTokenizedType.ENTITY) {
                    if (dtype.Variety == XmlSchemaDatatypeVariety.List) {
                        string[] ss = (string[])typedValue;
                        for (int i = 0; i < ss.Length; ++i) {
                            ProcessEntity(sinfo, ss[i], sender, eventhandler, baseUri, lineNo, linePos);
                        }
                    }
                    else {
                        ProcessEntity(sinfo, (string)typedValue, sender, eventhandler, baseUri, lineNo, linePos);
                    }
                }
                else if (ttype == XmlTokenizedType.ENUMERATION) {
                    if (!attdef.CheckEnumeration(typedValue)) {
                        XmlSchemaException e = new XmlSchemaException(Res.Sch_EnumerationValue, typedValue.ToString(), baseUri, lineNo, linePos);
                        if (eventhandler != null) {
                            eventhandler(sender, new ValidationEventArgs(e));
                        }
                        else {
                            throw e;
                        }
                    }
                }
                attdef.DefaultValueTyped = typedValue;
            }
#if DEBUG
            catch (XmlSchemaException ex) {
                Debug.WriteLineIf(DiagnosticsSwitches.XmlSchema.TraceError, ex.Message);
#else
            catch  {
#endif
                XmlSchemaException e = new XmlSchemaException(Res.Sch_AttributeDefaultDataType, attdef.Name.ToString(), baseUri, lineNo, linePos);
                if (eventhandler != null) {
                    eventhandler(sender, new ValidationEventArgs(e));
                }
                else {
                    throw e;
                }
            }
        }

        internal void AddID(string name, object node) {
            // Note: It used to be true that we only called this if _fValidate was true,
            // but due to the fact that you can now dynamically type somethign as an ID
            // that is no longer true.
            if (IDs == null) {
                IDs = new Hashtable();
            }

            IDs.Add(name, node);
        }

        public override object  FindId(string name) {
            return IDs == null ? null : IDs[name];
        }

        private    void Push(XmlQualifiedName elementName) {
            context = (ValidationState)validationStack.Push();
            if (context == null) {
                context = new ValidationState();
                validationStack.AddToTop(context);
            }
            context.LocalName = elementName.Name;
            context.Namespace = elementName.Namespace;
            context.HasMatched = false;
            context.IsNill = false;
            context.NeedValidateChildren = false;
        }

        private    void Pop() {
            if (validationStack.Length > 1) {
                validationStack.Pop();
                context = (ValidationState)validationStack.Peek();
            }
        }

        private void CheckForwardRefs() {
            IdRefNode next = idRefListHead;
            while (next != null) {
                if(FindId(next.Id) == null) {
                    SendValidationEvent(new XmlSchemaException(Res.Sch_UndeclaredId, next.Id, reader.BaseURI, next.LineNo, next.LinePos));
                }
                IdRefNode ptr = next.Next;
                next.Next = null; // unhook each object so it is cleaned up by Garbage Collector
                next = ptr;
            }
            // not needed any more.
            idRefListHead = null;
        }

        private XmlQualifiedName QualifiedName(string name, string ns) {
            return new XmlQualifiedName(name, XmlSchemaDatatype.XdrCanonizeUri(ns, NameTable, SchemaNames));
        }

    };
#pragma warning restore 618
}

