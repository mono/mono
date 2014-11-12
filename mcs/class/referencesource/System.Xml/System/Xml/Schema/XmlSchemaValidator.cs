//------------------------------------------------------------------------------
// <copyright file="XmlSchemaValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Threading;
using System.Runtime.Versioning;

namespace System.Xml.Schema {

    public delegate object XmlValueGetter();

    [Flags]
    public enum XmlSchemaValidationFlags {
        None                         = 0x0000,
        ProcessInlineSchema          = 0x0001,
        ProcessSchemaLocation        = 0x0002,
        ReportValidationWarnings     = 0x0004,
        ProcessIdentityConstraints   = 0x0008,
        AllowXmlAttributes           = 0x0010,
    }

    internal enum ValidatorState {
        None,
        Start,
        TopLevelAttribute,
        TopLevelTextOrWS,
        Element,
        Attribute,
        EndOfAttributes,
        Text,
        Whitespace,
        EndElement,
        SkipToEndElement,
        Finish,
    }
    internal class IdRefNode {
        internal string Id;
        internal int LineNo;
        internal int LinePos;
        internal IdRefNode Next;

        internal IdRefNode(IdRefNode next, string id, int lineNo, int linePos) {
            this.Id = id;
            this.LineNo = lineNo;
            this.LinePos = linePos;
            this.Next = next;
        }
    }

    public sealed class XmlSchemaValidator {

        //Schema Set
        private XmlSchemaSet        schemaSet;

        //Validation Settings
        private XmlSchemaValidationFlags validationFlags;

        //Validation
        private int                 startIDConstraint = -1;
        private const int           STACK_INCREMENT = 10;
        private bool                isRoot;
        private bool                rootHasSchema;

        //PSVI
        private bool attrValid;
        private bool checkEntity;

        private SchemaInfo          compiledSchemaInfo;
        private IDtdInfo            dtdSchemaInfo;
        private Hashtable           validatedNamespaces;

        private HWStack             validationStack;  // validaton contexts
        private ValidationState     context;          // current context
        private ValidatorState      currentState;

        //Attributes & IDS
        private Hashtable           attPresence;         //(AttName Vs AttIndex)
        private SchemaAttDef        wildID;

        private Hashtable           IDs;
        private IdRefNode           idRefListHead;

        //Parsing
        XmlQualifiedName            contextQName;

        //Avoid SchemaNames creation
        private string              NsXs;
        private string              NsXsi;
        private string              NsXmlNs;
        private string              NsXml;

        //PartialValidation
        private XmlSchemaObject     partialValidationType;

        //text to typedValue
        private StringBuilder       textValue;

        //Other state
        private ValidationEventHandler eventHandler;
        private object validationEventSender;
        private XmlNameTable nameTable;
        private IXmlLineInfo positionInfo;
        private IXmlLineInfo dummyPositionInfo;

        private XmlResolver xmlResolver;
        private Uri sourceUri;
        private string sourceUriString;
        private IXmlNamespaceResolver nsResolver;

        private XmlSchemaContentProcessing processContents = XmlSchemaContentProcessing.Strict;

        private static XmlSchemaAttribute xsiTypeSO;
        private static XmlSchemaAttribute xsiNilSO;
        private static XmlSchemaAttribute xsiSLSO;
        private static XmlSchemaAttribute xsiNoNsSLSO;

        //Xsi Attributes that are atomized
        private string xsiTypeString;
        private string xsiNilString;
        private string xsiSchemaLocationString;
        private string xsiNoNamespaceSchemaLocationString;

        //Xsi Attributes parsing
        private static readonly XmlSchemaDatatype dtQName = XmlSchemaDatatype.FromXmlTokenizedTypeXsd(XmlTokenizedType.QName);
        private static readonly XmlSchemaDatatype dtCDATA = XmlSchemaDatatype.FromXmlTokenizedType(XmlTokenizedType.CDATA);
        private static readonly XmlSchemaDatatype dtStringArray = dtCDATA.DeriveByList(null);

        //Error message constants
        private const string Quote = "'";

        //Empty arrays
        private static XmlSchemaParticle[] EmptyParticleArray = new XmlSchemaParticle[0];
        private static XmlSchemaAttribute[] EmptyAttributeArray = new XmlSchemaAttribute[0];

        //Whitespace check for text nodes
        XmlCharType xmlCharType = XmlCharType.Instance;

        internal static bool[,] ValidStates = new bool[12,12] {
                                               /*ValidatorState.None*/      /*ValidatorState.Start  /*ValidatorState.TopLevelAttribute*/     /*ValidatorState.TopLevelTOrWS*/ /*ValidatorState.Element*/      /*ValidatorState.Attribute*/    /*ValidatorState.EndAttributes*/    /*ValidatorState.Text/      /*ValidatorState.WS/*       /*ValidatorState.EndElement*/   /*ValidatorState.SkipToEndElement*/         /*ValidatorState.Finish*/
        /*ValidatorState.None*/             {  true,                        true,                     false,                                 false,                           false,                          false,                          false,                              false,                      false,                      false,                          false,                                      false},
        /*ValidatorState.Start*/            {  false,                       true,                     true,                                  true,                            true,                           false,                          false,                              false,                      false,                      false,                          false,                                      true },
        /*ValidatorState.TopLevelAttribute*/{  false,                       false,                    false,                                 false,                           false,                          false,                          false,                              false,                      false,                      false,                          false,                                      true },
        /*ValidatorState.TopLevelTextOrWS*/ {  false,                       false,                    false,                                 true,                            true,                           false,                          false,                              false,                      false,                      false,                          false,                                      true },
        /*ValidatorState.Element*/          {  false,                       false,                    false,                                 true,                            false,                          true,                           true,                               false,                      false,                      true,                           true,                                       false},
        /*ValidatorState.Attribute*/        {  false,                       false,                    false,                                 false,                           false,                          true,                           true,                               false,                      false,                      true,                           true,                                       false},
        /*ValidatorState.EndAttributes*/    {  false,                       false,                    false,                                 false,                           true,                           false,                          false,                              true,                       true,                       true,                           true,                                       false},
        /*ValidatorState.Text*/             {  false,                       false,                    false,                                 false,                           true,                           false,                          false,                              true,                       true,                       true,                           true,                                       false},
        /*ValidatorState.Whitespace*/       {  false,                       false,                    false,                                 false,                           true,                           false,                          false,                              true,                       true,                       true,                           true,                                       false},
        /*ValidatorState.EndElement*/       {  false,                       false,                    false,                                 true,                            true,                           false,                          false,                              true,                       true,                       true,                           true /*?*/,                                 true },
        /*ValidatorState.SkipToEndElement*/ {  false,                       false,                    false,                                 true,                            true,                           false,                          false,                              true,                       true,                       true,                           true,                                       true },
        /*ValidatorState.Finish*/           {  false,                       true,                     false,                                 false,                           false,                          false,                          false,                              false,                      false,                      false,                          false,                                      false},
        };

        private static string[] MethodNames = new string[12] {"None", "Initialize", "top-level ValidateAttribute", "top-level ValidateText or ValidateWhitespace", "ValidateElement", "ValidateAttribute", "ValidateEndOfAttributes", "ValidateText", "ValidateWhitespace", "ValidateEndElement", "SkipToEndElement", "EndValidation" };

        public XmlSchemaValidator(XmlNameTable nameTable, XmlSchemaSet schemas, IXmlNamespaceResolver namespaceResolver, XmlSchemaValidationFlags validationFlags) {
            if (nameTable == null) {
                throw new ArgumentNullException("nameTable");
            }
            if (schemas == null) {
                throw new ArgumentNullException("schemas");
            }
            if (namespaceResolver == null) {
                throw new ArgumentNullException("namespaceResolver");
            }
            this.nameTable = nameTable;
            this.nsResolver = namespaceResolver;
            this.validationFlags = validationFlags;


            if ( ((validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != 0) || ((validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != 0) ) { //Process schema hints in xml document, hence user's set might change
                this.schemaSet = new XmlSchemaSet(nameTable);
                this.schemaSet.ValidationEventHandler += schemas.GetEventHandler();
                this.schemaSet.CompilationSettings = schemas.CompilationSettings;
                this.schemaSet.XmlResolver = schemas.GetResolver();
                this.schemaSet.Add(schemas);
                validatedNamespaces = new Hashtable();
            }
            else { //Use the same set from the user
                this.schemaSet = schemas;
            }
            Init();
        }

        private void Init() {
            validationStack = new HWStack(STACK_INCREMENT);
            attPresence = new Hashtable();
            Push(XmlQualifiedName.Empty);

            dummyPositionInfo = new PositionInfo(); //Dummy position info, will return (0,0) if user does not set the LineInfoProvider property
            positionInfo = dummyPositionInfo;
            validationEventSender = this;
            currentState = ValidatorState.None;
            textValue = new StringBuilder(100);
            xmlResolver = System.Xml.XmlConfiguration.XmlReaderSection.CreateDefaultResolver();
            contextQName = new XmlQualifiedName(); //Re-use qname
            Reset();

            RecompileSchemaSet(); //Gets compiled info from set as well
            //Get already Atomized strings
            NsXs = nameTable.Add(XmlReservedNs.NsXs);
            NsXsi = nameTable.Add(XmlReservedNs.NsXsi);
            NsXmlNs = nameTable.Add(XmlReservedNs.NsXmlNs);
            NsXml = nameTable.Add(XmlReservedNs.NsXml);
            xsiTypeString = nameTable.Add("type");
            xsiNilString = nameTable.Add("nil");
            xsiSchemaLocationString = nameTable.Add("schemaLocation");
            xsiNoNamespaceSchemaLocationString = nameTable.Add("noNamespaceSchemaLocation");
        }

        private void Reset() {
            isRoot = true;
            rootHasSchema = true;
            while(validationStack.Length > 1) { //Clear all other context from stack
                validationStack.Pop();
            }
            startIDConstraint = -1;
            partialValidationType = null;

            //Clear previous tables
            if (IDs != null) {
                IDs.Clear();
            }
            if (ProcessSchemaHints) {
                validatedNamespaces.Clear();
            }
        }

//Properties
        public XmlResolver XmlResolver {
            set {
                xmlResolver = value;
            }
        }

        public IXmlLineInfo LineInfoProvider {
            get {
                return positionInfo;
            }
            set {
                if (value == null) { //If value is null, retain the default dummy line info
                    this.positionInfo = dummyPositionInfo;
                }
                else {
                    this.positionInfo = value;
                }
            }
        }

        public Uri SourceUri {
            get {
                return sourceUri;
            }
            set {
                sourceUri = value;
                sourceUriString = sourceUri.ToString();
            }
        }

        public object ValidationEventSender {
            get {
                return validationEventSender;
            }
            set {
                validationEventSender = value;
            }
        }

        public event ValidationEventHandler ValidationEventHandler {
            add {
                eventHandler += value;
            }
            remove {
                eventHandler -= value;
            }
        }

//Methods

        public void AddSchema(XmlSchema schema) {
            if (schema == null) {
                throw new ArgumentNullException("schema");
            }
            if ((validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) == 0) { //Do not process schema if processInlineSchema is not set
                return;
            }
            string tns = schema.TargetNamespace;
            if (tns == null) {
                tns = string.Empty;
            }
            //Store the previous locations
            Hashtable schemaLocations = schemaSet.SchemaLocations;
            DictionaryEntry[] oldLocations = new DictionaryEntry[schemaLocations.Count];
            schemaLocations.CopyTo(oldLocations, 0);

            //
            Debug.Assert(validatedNamespaces != null);
            if (validatedNamespaces[tns] != null && schemaSet.FindSchemaByNSAndUrl(schema.BaseUri, tns, oldLocations) == null) {
                SendValidationEvent(Res.Sch_ComponentAlreadySeenForNS, tns, XmlSeverityType.Error);
            }
            if (schema.ErrorCount == 0) {
                try {
                    schemaSet.Add(schema);
                    RecompileSchemaSet();
                }
                catch(XmlSchemaException e) {
                    SendValidationEvent(Res.Sch_CannotLoadSchema, new string[] {schema.BaseUri.ToString(), e.Message},e);
                }
                for (int i = 0; i < schema.ImportedSchemas.Count; ++i) {     //Check for its imports
                    XmlSchema impSchema = (XmlSchema)schema.ImportedSchemas[i];
                    tns = impSchema.TargetNamespace;
                    if (tns == null) {
                        tns = string.Empty;
                    }
                    if (validatedNamespaces[tns] != null && schemaSet.FindSchemaByNSAndUrl(impSchema.BaseUri, tns, oldLocations) == null) {
                        SendValidationEvent(Res.Sch_ComponentAlreadySeenForNS, tns, XmlSeverityType.Error);
                        schemaSet.RemoveRecursive(schema);
                        break;
                    }
                }
            }
        }

        public void Initialize() {
            if (currentState != ValidatorState.None && currentState != ValidatorState.Finish) {
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidStateTransition, new string[] { MethodNames[(int)currentState], MethodNames[(int)ValidatorState.Start] }));
            }
            currentState = ValidatorState.Start;
            Reset();
        }

        public void Initialize(XmlSchemaObject partialValidationType) {
            if (currentState != ValidatorState.None && currentState != ValidatorState.Finish) {
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidStateTransition, new string[] { MethodNames[(int)currentState], MethodNames[(int)ValidatorState.Start] }));
            }
            if (partialValidationType == null) {
                throw new ArgumentNullException("partialValidationType");
            }
            if (!(partialValidationType is XmlSchemaElement || partialValidationType is XmlSchemaAttribute || partialValidationType is XmlSchemaType)) {
                throw new ArgumentException(Res.GetString(Res.Sch_InvalidPartialValidationType));
            }
            currentState = ValidatorState.Start;
            Reset();
            this.partialValidationType = partialValidationType;
        }

        // SxS: This method passes null as resource names and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        public void ValidateElement(string localName, string namespaceUri, XmlSchemaInfo schemaInfo)  {
            ValidateElement(localName, namespaceUri, schemaInfo, null, null, null, null);
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public void ValidateElement(string localName, string namespaceUri, XmlSchemaInfo schemaInfo, string xsiType, string xsiNil, string xsiSchemaLocation, string xsiNoNamespaceSchemaLocation)  {
            if (localName == null) {
                throw new ArgumentNullException("localName");
            }
            if (namespaceUri == null) {
                throw new ArgumentNullException("namespaceUri");
            }

            CheckStateTransition(ValidatorState.Element, MethodNames[(int)ValidatorState.Element]);

            ClearPSVI();
            contextQName.Init(localName, namespaceUri);
            XmlQualifiedName elementName = contextQName;

            bool invalidElementInContext;
            object particle = ValidateElementContext(elementName, out invalidElementInContext); //Check element name is allowed in current position
            SchemaElementDecl elementDecl = FastGetElementDecl(elementName, particle);

            //Change context to current element and update element decl
            Push(elementName);

            //Change current context's error state depending on whether this element was validated in its context correctly
            if (invalidElementInContext) {
                context.Validity = XmlSchemaValidity.Invalid;
            }

            //Check if there are Xsi attributes
            if ((validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != 0 && xmlResolver != null) { //we should process schema location
                ProcessSchemaLocations(xsiSchemaLocation, xsiNoNamespaceSchemaLocation);
            }

            if (processContents != XmlSchemaContentProcessing.Skip) {
                if (elementDecl == null && partialValidationType == null) { //Since new schemaLocations might have been added, try for decl from the set again only if no PVType is set
                    elementDecl = compiledSchemaInfo.GetElementDecl(elementName);
                }
                bool declFound = elementDecl != null;
                if (xsiType != null || xsiNil != null) {
                    elementDecl = CheckXsiTypeAndNil(elementDecl, xsiType, xsiNil, ref declFound);
                }
                if (elementDecl == null) {
                    ThrowDeclNotFoundWarningOrError(declFound); //This updates processContents
                }
            }

            context.ElementDecl = elementDecl;
            XmlSchemaElement localSchemaElement = null;
            XmlSchemaType localSchemaType = null;
            if (elementDecl != null) {
                CheckElementProperties();
                attPresence.Clear(); //Clear attributes hashtable for every element
                context.NeedValidateChildren = processContents != XmlSchemaContentProcessing.Skip;
                ValidateStartElementIdentityConstraints();  //Need attr collection validated here
                elementDecl.ContentValidator.InitValidation(context);

                localSchemaType = elementDecl.SchemaType;
                localSchemaElement = GetSchemaElement();
            }

            if (schemaInfo != null) {
                schemaInfo.SchemaType = localSchemaType;
                schemaInfo.SchemaElement = localSchemaElement;
                schemaInfo.IsNil = context.IsNill;
                schemaInfo.Validity = context.Validity;
            }
            if (ProcessSchemaHints) {
                if (validatedNamespaces[namespaceUri] == null) {
                    validatedNamespaces.Add(namespaceUri, namespaceUri);
                }
            }

            if (isRoot) {
                isRoot = false;
            }
        }

        public object ValidateAttribute(string localName, string namespaceUri, string attributeValue, XmlSchemaInfo schemaInfo) {
            if (attributeValue == null) {
                throw new ArgumentNullException("attributeValue");
            }
            return ValidateAttribute(localName, namespaceUri, null, attributeValue, schemaInfo);
        }

        public object ValidateAttribute(string localName, string namespaceUri, XmlValueGetter attributeValue, XmlSchemaInfo schemaInfo) {
            if (attributeValue == null) {
                throw new ArgumentNullException("attributeValue");
            }
            return ValidateAttribute(localName, namespaceUri, attributeValue, null, schemaInfo);
        }

        private object ValidateAttribute(string lName, string ns, XmlValueGetter attributeValueGetter, string attributeStringValue, XmlSchemaInfo schemaInfo) {
            if (lName == null) {
                throw new ArgumentNullException("localName");
            }
            if (ns == null) {
                throw new ArgumentNullException("namespaceUri");
            }

            ValidatorState toState = validationStack.Length > 1 ? ValidatorState.Attribute : ValidatorState.TopLevelAttribute;
            CheckStateTransition(toState, MethodNames[(int)toState]);

            object typedVal = null;
            attrValid = true;
            XmlSchemaValidity localValidity = XmlSchemaValidity.NotKnown;
            XmlSchemaAttribute localAttribute = null;
            XmlSchemaSimpleType localMemberType = null;

            ns = nameTable.Add(ns);
            if(Ref.Equal(ns,NsXmlNs)) {
                return null;
            }

            SchemaAttDef attributeDef = null;
            SchemaElementDecl currentElementDecl = context.ElementDecl;
            XmlQualifiedName attQName = new XmlQualifiedName(lName, ns);
            if (attPresence[attQName] != null) { //this attribute already checked as it is duplicate;
                SendValidationEvent(Res.Sch_DuplicateAttribute, attQName.ToString());
                if (schemaInfo != null) {
                    schemaInfo.Clear();
                }
                return null;
            }

            if (!Ref.Equal(ns,NsXsi)) { //
                XmlSchemaObject pvtAttribute = currentState == ValidatorState.TopLevelAttribute ? partialValidationType : null;
                AttributeMatchState attributeMatchState;
                attributeDef = compiledSchemaInfo.GetAttributeXsd(currentElementDecl, attQName, pvtAttribute, out attributeMatchState);

                switch (attributeMatchState) {
                    case AttributeMatchState.UndeclaredElementAndAttribute:
                        if((attributeDef = CheckIsXmlAttribute(attQName)) != null) { //Try for xml attribute
                            goto case AttributeMatchState.AttributeFound;
                        }
                        if (currentElementDecl == null
                            && processContents == XmlSchemaContentProcessing.Strict
                            && attQName.Namespace.Length != 0
                            && compiledSchemaInfo.Contains(attQName.Namespace)
                        ) {
                            attrValid = false;
                            SendValidationEvent(Res.Sch_UndeclaredAttribute, attQName.ToString());
                        }
                        else if (processContents != XmlSchemaContentProcessing.Skip) {
                            SendValidationEvent(Res.Sch_NoAttributeSchemaFound, attQName.ToString(), XmlSeverityType.Warning);
                        }
                        break;

                    case AttributeMatchState.UndeclaredAttribute:
                        if((attributeDef = CheckIsXmlAttribute(attQName)) != null) {
                            goto case AttributeMatchState.AttributeFound;
                        }
                        else {
                            attrValid = false;
                            SendValidationEvent(Res.Sch_UndeclaredAttribute, attQName.ToString());
                        }
                        break;

                    case AttributeMatchState.ProhibitedAnyAttribute:
                        if((attributeDef = CheckIsXmlAttribute(attQName)) != null) {
                            goto case AttributeMatchState.AttributeFound;
                        }
                        else {
                            attrValid = false;
                            SendValidationEvent(Res.Sch_ProhibitedAttribute, attQName.ToString());
                        }
                        break;

                    case AttributeMatchState.ProhibitedAttribute:
                        attrValid = false;
                        SendValidationEvent(Res.Sch_ProhibitedAttribute, attQName.ToString());
                        break;

                    case AttributeMatchState.AttributeNameMismatch:
                        attrValid = false;
                        SendValidationEvent(Res.Sch_SchemaAttributeNameMismatch, new string[] { attQName.ToString(), ((XmlSchemaAttribute)pvtAttribute).QualifiedName.ToString()});
                        break;

                    case AttributeMatchState.ValidateAttributeInvalidCall:
                        Debug.Assert(currentState == ValidatorState.TopLevelAttribute); //Re-set state back to start on error with partial validation type
                        currentState = ValidatorState.Start;
                        attrValid = false;
                        SendValidationEvent(Res.Sch_ValidateAttributeInvalidCall, string.Empty);
                        break;

                    case AttributeMatchState.AnyIdAttributeFound:
                        if (wildID == null) {
                            wildID = attributeDef;
                            Debug.Assert(currentElementDecl != null);
                            XmlSchemaComplexType ct = currentElementDecl.SchemaType as XmlSchemaComplexType;
                            Debug.Assert(ct != null);
                            if (ct.ContainsIdAttribute(false)) {
                                SendValidationEvent(Res.Sch_AttrUseAndWildId, string.Empty);
                            }
                            else {
                                goto case AttributeMatchState.AttributeFound;
                            }
                        }
                        else { //More than one attribute per element cannot match wildcard if both their types are derived from ID
                            SendValidationEvent(Res.Sch_MoreThanOneWildId, string.Empty);
                        }
                        break;

                    case AttributeMatchState.AttributeFound:
                        Debug.Assert(attributeDef != null);
                        localAttribute = attributeDef.SchemaAttribute;
                        if (currentElementDecl != null) { //Have to add to hashtable to check whether to add default attributes
                            attPresence.Add(attQName, attributeDef);
                        }
                        object attValue;
                        if (attributeValueGetter != null) {
                            attValue = attributeValueGetter();
                        }
                        else {
                            attValue = attributeStringValue;
                        }
                        typedVal = CheckAttributeValue(attValue, attributeDef);
                        XmlSchemaDatatype datatype = attributeDef.Datatype;
                        if (datatype.Variety == XmlSchemaDatatypeVariety.Union && typedVal != null) { //Unpack the union
                            XsdSimpleValue simpleValue = typedVal as XsdSimpleValue;
                            Debug.Assert(simpleValue != null);

                            localMemberType = simpleValue.XmlType;
                            datatype = simpleValue.XmlType.Datatype;
                            typedVal = simpleValue.TypedValue;
                        }
                        CheckTokenizedTypes(datatype, typedVal, true);
                        if (HasIdentityConstraints) {
                            AttributeIdentityConstraints(attQName.Name, attQName.Namespace, typedVal, attValue.ToString(), datatype);
                        }
                        break;

                    case AttributeMatchState.AnyAttributeLax:
                        SendValidationEvent(Res.Sch_NoAttributeSchemaFound, attQName.ToString(), XmlSeverityType.Warning);
                        break;

                    case AttributeMatchState.AnyAttributeSkip:
                        break;

                    default:
                        break;
                }
            }
            else { //Attribute from xsi namespace
                lName = nameTable.Add(lName);
                if (Ref.Equal(lName, xsiTypeString) || Ref.Equal(lName, xsiNilString) || Ref.Equal(lName, xsiSchemaLocationString) || Ref.Equal(lName, xsiNoNamespaceSchemaLocationString)) {
                    attPresence.Add(attQName, SchemaAttDef.Empty);
                }
                else {
                    attrValid = false;
                    SendValidationEvent(Res.Sch_NotXsiAttribute, attQName.ToString());
                }
            }

            if (!attrValid) {
                localValidity = XmlSchemaValidity.Invalid;
            }
            else if (attributeDef != null) {
                localValidity = XmlSchemaValidity.Valid;
            }
            if (schemaInfo != null) {
                schemaInfo.SchemaAttribute = localAttribute;
                schemaInfo.SchemaType = localAttribute == null ? null : localAttribute.AttributeSchemaType;
                schemaInfo.MemberType = localMemberType;
                schemaInfo.IsDefault = false;
                schemaInfo.Validity = localValidity;
            }
            if (ProcessSchemaHints) {
                if (validatedNamespaces[ns] == null) {
                    validatedNamespaces.Add(ns, ns);
                }
            }
            return typedVal;
        }

        public void GetUnspecifiedDefaultAttributes(ArrayList defaultAttributes) {
            if (defaultAttributes == null)  {
                throw new ArgumentNullException("defaultAttributes");
            }
            CheckStateTransition(ValidatorState.Attribute, "GetUnspecifiedDefaultAttributes");
            GetUnspecifiedDefaultAttributes(defaultAttributes, false);
        }

        public void ValidateEndOfAttributes(XmlSchemaInfo schemaInfo) {
            CheckStateTransition(ValidatorState.EndOfAttributes, MethodNames[(int)ValidatorState.EndOfAttributes]);
            //Check required attributes
            SchemaElementDecl currentElementDecl = context.ElementDecl;
            if (currentElementDecl != null && currentElementDecl.HasRequiredAttribute) {
                context.CheckRequiredAttribute = false;
                CheckRequiredAttributes(currentElementDecl);
            }
            if (schemaInfo != null) { //set validity depending on whether all required attributes were validated successfully
                schemaInfo.Validity = context.Validity;
            }
        }

        public void ValidateText(string elementValue) {
            if (elementValue == null) {
                throw new ArgumentNullException("elementValue");
            }
            ValidateText(elementValue, null);
        }

        public void ValidateText(XmlValueGetter elementValue) {
            if (elementValue == null) {
                throw new ArgumentNullException("elementValue");
            }
            ValidateText(null, elementValue);
        }

        private void ValidateText(string elementStringValue, XmlValueGetter elementValueGetter) {
            ValidatorState toState = validationStack.Length > 1 ? ValidatorState.Text : ValidatorState.TopLevelTextOrWS;
            CheckStateTransition(toState, MethodNames[(int)toState]);

            if (context.NeedValidateChildren) {
                if (context.IsNill) {
                    SendValidationEvent(Res.Sch_ContentInNill, QNameString(context.LocalName, context.Namespace));
                    return;
                }
                XmlSchemaContentType contentType = context.ElementDecl.ContentValidator.ContentType;
                switch(contentType) {
                    case XmlSchemaContentType.Empty:
                        SendValidationEvent(Res.Sch_InvalidTextInEmpty, string.Empty);
                        break;

                    case XmlSchemaContentType.TextOnly:
                        if (elementValueGetter != null) {
                            SaveTextValue(elementValueGetter());
                        }
                        else {
                            SaveTextValue(elementStringValue);
                        }
                        break;

                    case XmlSchemaContentType.ElementOnly:
                        string textValue = elementValueGetter != null ? elementValueGetter().ToString() : elementStringValue;
                        if(xmlCharType.IsOnlyWhitespace(textValue)) {
                            break;
                        }
                        ArrayList names = context.ElementDecl.ContentValidator.ExpectedParticles(context, false, schemaSet);
                        if (names == null ||  names.Count == 0) {
                            SendValidationEvent(Res.Sch_InvalidTextInElement, BuildElementName(context.LocalName, context.Namespace));
                        }
                        else {
                            Debug.Assert(names.Count > 0);
                            SendValidationEvent(Res.Sch_InvalidTextInElementExpecting, new string[] { BuildElementName(context.LocalName, context.Namespace), PrintExpectedElements(names, true) });
                        }
                        break;

                    case XmlSchemaContentType.Mixed:
                        if (context.ElementDecl.DefaultValueTyped != null) {
                            if (elementValueGetter != null) {
                                SaveTextValue(elementValueGetter());
                            }
                            else {
                                SaveTextValue(elementStringValue);
                            }
                        }
                        break;
                }
            }
        }

        public void ValidateWhitespace(string elementValue) {
            if (elementValue == null) {
                throw new ArgumentNullException("elementValue");
            }
            ValidateWhitespace(elementValue, null);
        }

        public void ValidateWhitespace(XmlValueGetter elementValue) {
            if (elementValue == null) {
                throw new ArgumentNullException("elementValue");
            }
            ValidateWhitespace(null, elementValue);
        }

        private void ValidateWhitespace(string elementStringValue, XmlValueGetter elementValueGetter) {
            ValidatorState toState = validationStack.Length > 1 ? ValidatorState.Whitespace : ValidatorState.TopLevelTextOrWS;
            CheckStateTransition(toState, MethodNames[(int)toState]);

            if (context.NeedValidateChildren) {
                if (context.IsNill) {
                    SendValidationEvent(Res.Sch_ContentInNill, QNameString(context.LocalName, context.Namespace));
                }
                XmlSchemaContentType contentType = context.ElementDecl.ContentValidator.ContentType;
                switch (contentType) {
                    case XmlSchemaContentType.Empty:
                        SendValidationEvent(Res.Sch_InvalidWhitespaceInEmpty, string.Empty);
                        break;

                    case XmlSchemaContentType.TextOnly:
                        if (elementValueGetter != null) {
                            SaveTextValue(elementValueGetter());
                        }
                        else {
                            SaveTextValue(elementStringValue);
                        }
                        break;

                    case XmlSchemaContentType.Mixed:
                        if (context.ElementDecl.DefaultValueTyped != null) {
                            if (elementValueGetter != null) {
                                SaveTextValue(elementValueGetter());
                            }
                            else {
                                SaveTextValue(elementStringValue);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }


        public object ValidateEndElement(XmlSchemaInfo schemaInfo) {
            return InternalValidateEndElement(schemaInfo, null);
        }

        public object ValidateEndElement(XmlSchemaInfo schemaInfo, object typedValue) {
            if (typedValue == null) {
                throw new ArgumentNullException("typedValue");
            }
            if (textValue.Length > 0) {
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidEndElementCall));
            }
            return InternalValidateEndElement(schemaInfo, typedValue);
        }

        public void SkipToEndElement(XmlSchemaInfo schemaInfo) {
            if (validationStack.Length <= 1) {
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidEndElementMultiple, MethodNames[(int)ValidatorState.SkipToEndElement]));
            }
            CheckStateTransition(ValidatorState.SkipToEndElement, MethodNames[(int)ValidatorState.SkipToEndElement]);

            if (schemaInfo != null) {
                SchemaElementDecl currentElementDecl = context.ElementDecl;
                if (currentElementDecl != null) {
                    schemaInfo.SchemaType = currentElementDecl.SchemaType;
                    schemaInfo.SchemaElement = GetSchemaElement();
                }
                else {
                    schemaInfo.SchemaType = null;
                    schemaInfo.SchemaElement = null;
                }
                schemaInfo.MemberType = null;
                schemaInfo.IsNil = context.IsNill;
                schemaInfo.IsDefault = context.IsDefault;
                Debug.Assert(context.Validity != XmlSchemaValidity.Valid);
                schemaInfo.Validity = context.Validity;
            }
            context.ValidationSkipped = true;
            currentState = ValidatorState.SkipToEndElement;
            Pop();
        }

        public void EndValidation() {
            if (validationStack.Length > 1) { //We have pending elements in the stack to call ValidateEndElement
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidEndValidation));
            }
            CheckStateTransition(ValidatorState.Finish, MethodNames[(int)ValidatorState.Finish]);
            CheckForwardRefs();
        }

        public XmlSchemaParticle[] GetExpectedParticles() {
            if (currentState == ValidatorState.Start || currentState == ValidatorState.TopLevelTextOrWS) { //Right after initialize
                if (partialValidationType != null) {
                    XmlSchemaElement element = partialValidationType as XmlSchemaElement;
                    if (element != null) {
                        return new XmlSchemaParticle[1] {element};
                    }
                    return EmptyParticleArray;
                }
                else { //Should return all global elements
                    ICollection elements = schemaSet.GlobalElements.Values;
                    ArrayList expected = new ArrayList(elements.Count);
                    foreach(XmlSchemaElement element in elements) { //Check for substitutions
                        ContentValidator.AddParticleToExpected(element, schemaSet, expected, true);
                    }
                    return expected.ToArray(typeof(XmlSchemaParticle)) as XmlSchemaParticle[];
                }
            }
            if (context.ElementDecl != null) {
                ArrayList expected = context.ElementDecl.ContentValidator.ExpectedParticles(context, false, schemaSet);
                if (expected != null) {
                    return expected.ToArray(typeof(XmlSchemaParticle)) as XmlSchemaParticle[];
                }
            }
            return EmptyParticleArray;
        }

        public XmlSchemaAttribute[] GetExpectedAttributes() {
            if (currentState == ValidatorState.Element || currentState == ValidatorState.Attribute) {
                SchemaElementDecl elementDecl = context.ElementDecl;
                ArrayList attList = new ArrayList();
                if (elementDecl != null) {
                    foreach(SchemaAttDef attDef in elementDecl.AttDefs.Values) {
                        if (attPresence[attDef.Name] == null) {
                            attList.Add(attDef.SchemaAttribute);
                        }
                    }
                }
                if (nsResolver.LookupPrefix(NsXsi) != null) { //Xsi namespace defined
                    AddXsiAttributes(attList);
                }
                return attList.ToArray(typeof(XmlSchemaAttribute)) as XmlSchemaAttribute[];
            }
            else if (currentState == ValidatorState.Start) {
                if (partialValidationType != null) {
                    XmlSchemaAttribute attribute = partialValidationType as XmlSchemaAttribute;
                    if (attribute != null) {
                        return new XmlSchemaAttribute[1] {attribute};
                    }
                }
            }
            return EmptyAttributeArray;
        }

        internal void GetUnspecifiedDefaultAttributes(ArrayList defaultAttributes, bool createNodeData) {
            currentState = ValidatorState.Attribute;
            SchemaElementDecl currentElementDecl = context.ElementDecl;

            if (currentElementDecl != null && currentElementDecl.HasDefaultAttribute) {
                for (int i = 0; i < currentElementDecl.DefaultAttDefs.Count; ++i) {
                    SchemaAttDef attdef = (SchemaAttDef)currentElementDecl.DefaultAttDefs[i];
                    if (!attPresence.Contains(attdef.Name)) {
                        if (attdef.DefaultValueTyped == null) { //Invalid attribute default in the schema
                            continue;
                        }

                        //Check to see default attributes WILL be qualified if attributeFormDefault = qualified in schema
                        string attributeNS = nameTable.Add(attdef.Name.Namespace);
                        string defaultPrefix = string.Empty;
                        if (attributeNS.Length > 0) {
                            defaultPrefix = GetDefaultAttributePrefix(attributeNS);
                            if (defaultPrefix == null || defaultPrefix.Length == 0) {
                                SendValidationEvent(Res.Sch_DefaultAttributeNotApplied, new string[2] { attdef.Name.ToString(), QNameString(context.LocalName, context.Namespace)});
                                continue;
                            }
                        }
                        XmlSchemaDatatype datatype = attdef.Datatype;
                        if (createNodeData) {
                            ValidatingReaderNodeData attrData = new ValidatingReaderNodeData();
                            attrData.LocalName = nameTable.Add(attdef.Name.Name);
                            attrData.Namespace = attributeNS;
                            attrData.Prefix = nameTable.Add(defaultPrefix);
                            attrData.NodeType = XmlNodeType.Attribute;

                            //set PSVI properties
                            AttributePSVIInfo attrValidInfo = new AttributePSVIInfo();
                            XmlSchemaInfo attSchemaInfo = attrValidInfo.attributeSchemaInfo;
                            Debug.Assert(attSchemaInfo != null);
                            if (attdef.Datatype.Variety == XmlSchemaDatatypeVariety.Union) {
                                XsdSimpleValue simpleValue = attdef.DefaultValueTyped as XsdSimpleValue;
                                attSchemaInfo.MemberType = simpleValue.XmlType;
                                datatype = simpleValue.XmlType.Datatype;
                                attrValidInfo.typedAttributeValue = simpleValue.TypedValue;
                            }
                            else {
                                attrValidInfo.typedAttributeValue = attdef.DefaultValueTyped;
                            }
                            attSchemaInfo.IsDefault = true;
                            attSchemaInfo.Validity = XmlSchemaValidity.Valid;
                            attSchemaInfo.SchemaType = attdef.SchemaType;
                            attSchemaInfo.SchemaAttribute = attdef.SchemaAttribute;
                            attrData.RawValue = attSchemaInfo.XmlType.ValueConverter.ToString(attrValidInfo.typedAttributeValue);

                            attrData.AttInfo = attrValidInfo;
                            defaultAttributes.Add(attrData);
                        }
                        else {
                            defaultAttributes.Add(attdef.SchemaAttribute);
                        }
                        CheckTokenizedTypes(datatype, attdef.DefaultValueTyped, true);
                        if (HasIdentityConstraints) {
                            AttributeIdentityConstraints(attdef.Name.Name, attdef.Name.Namespace, attdef.DefaultValueTyped, attdef.DefaultValueRaw, datatype);
                        }
                    }
                }
            }
            return;
        }

        internal XmlSchemaSet SchemaSet {
            get {
                return schemaSet;
            }
        }

        internal XmlSchemaValidationFlags ValidationFlags {
            get {
                return validationFlags;
            }
        }

        internal XmlSchemaContentType CurrentContentType {
            get {
                if (context.ElementDecl == null) {
                    return XmlSchemaContentType.Empty;
                }
                return context.ElementDecl.ContentValidator.ContentType;
            }
        }

        internal XmlSchemaContentProcessing CurrentProcessContents {
            get {
                return processContents;
            }
        }

        internal void SetDtdSchemaInfo(IDtdInfo dtdSchemaInfo) {
            this.dtdSchemaInfo = dtdSchemaInfo;
            this.checkEntity = true;
        }

        private bool StrictlyAssessed {
            get {
                return (processContents == XmlSchemaContentProcessing.Strict || processContents == XmlSchemaContentProcessing.Lax) && context.ElementDecl != null && !context.ValidationSkipped;
            }
        }

        private bool HasSchema {
            get {
                if (isRoot) {
                    isRoot = false;
                    if (!compiledSchemaInfo.Contains(context.Namespace)) {
                        rootHasSchema = false;
                    }
                }
                return rootHasSchema;
            }
        }

        internal string GetConcatenatedValue() {
            return textValue.ToString();
        }

        private object InternalValidateEndElement(XmlSchemaInfo schemaInfo, object typedValue) {
            if (validationStack.Length <= 1) {
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidEndElementMultiple, MethodNames[(int)ValidatorState.EndElement]));
            }
            CheckStateTransition(ValidatorState.EndElement, MethodNames[(int)ValidatorState.EndElement]);

            SchemaElementDecl contextElementDecl = context.ElementDecl;
            XmlSchemaSimpleType memberType = null;
            XmlSchemaType localSchemaType = null;
            XmlSchemaElement localSchemaElement = null;

            string stringValue = string.Empty;

            if (contextElementDecl != null) {
                if (context.CheckRequiredAttribute && contextElementDecl.HasRequiredAttribute) {
                    CheckRequiredAttributes(contextElementDecl);
                }
                if (!context.IsNill) {
                    if (context.NeedValidateChildren) {
                        XmlSchemaContentType contentType = contextElementDecl.ContentValidator.ContentType;
                        switch (contentType) {
                            case XmlSchemaContentType.TextOnly:
                                if (typedValue == null) {
                                    stringValue = textValue.ToString();
                                    typedValue = ValidateAtomicValue(stringValue, out memberType);
                                }
                                else { //Parsed object passed in, need to verify only facets
                                    typedValue = ValidateAtomicValue(typedValue, out memberType);
                                }
                                break;

                            case XmlSchemaContentType.Mixed:
                                if (contextElementDecl.DefaultValueTyped != null) {
                                    if (typedValue == null) {
                                        stringValue = textValue.ToString();
                                        typedValue = CheckMixedValueConstraint(stringValue);
                                    }
                                }
                                break;

                            case XmlSchemaContentType.ElementOnly:
                                if (typedValue != null) { //Cannot pass in typedValue for complex content
                                    throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidEndElementCallTyped));
                                }
                                break;

                            default:
                                break;
                        }
                        if(!contextElementDecl.ContentValidator.CompleteValidation(context)) {
                            CompleteValidationError(context, eventHandler, nsResolver, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition, schemaSet);
                            context.Validity = XmlSchemaValidity.Invalid;
                        }
                    }
                }
                // for each level in the stack, endchildren and fill value from element
                if (HasIdentityConstraints) {
                    XmlSchemaType xmlType = memberType == null ? contextElementDecl.SchemaType : memberType;
                    EndElementIdentityConstraints(typedValue, stringValue, xmlType.Datatype);
                }
                localSchemaType = contextElementDecl.SchemaType;
                localSchemaElement = GetSchemaElement();
            }
            if (schemaInfo != null) { //SET SchemaInfo
                schemaInfo.SchemaType = localSchemaType;
                schemaInfo.SchemaElement = localSchemaElement;
                schemaInfo.MemberType = memberType;
                schemaInfo.IsNil = context.IsNill;
                schemaInfo.IsDefault = context.IsDefault;
                if (context.Validity == XmlSchemaValidity.NotKnown && StrictlyAssessed) {
                    context.Validity = XmlSchemaValidity.Valid;
                }
                schemaInfo.Validity = context.Validity;
            }
            Pop();
            return typedValue;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        private void ProcessSchemaLocations(string xsiSchemaLocation, string xsiNoNamespaceSchemaLocation) {
            bool compile = false;
            if (xsiNoNamespaceSchemaLocation != null) {
                compile = true;
                LoadSchema(string.Empty, xsiNoNamespaceSchemaLocation);
            }
            if (xsiSchemaLocation != null) {

                object typedValue;
                Exception exception = dtStringArray.TryParseValue(xsiSchemaLocation, nameTable, nsResolver, out typedValue);
                if (exception != null) {
                    SendValidationEvent(Res.Sch_InvalidValueDetailedAttribute, new string[] { "schemaLocation", xsiSchemaLocation, dtStringArray.TypeCodeString, exception.Message }, exception);
                    return;
                }
                string[] locations = (string[])typedValue;
                compile = true;
                try {
                    for (int j = 0; j < locations.Length - 1; j += 2) {
                        LoadSchema((string)locations[j], (string)locations[j + 1]);
                    }
                }
                catch (XmlSchemaException schemaException) {
                    SendValidationEvent(schemaException);
                }
            }
            if (compile) {
                RecompileSchemaSet();
            }
        }


        private object ValidateElementContext(XmlQualifiedName elementName, out bool invalidElementInContext) {
            object particle = null;
            int errorCode = 0;
            XmlQualifiedName head;
            XmlSchemaElement headElement = null;
            invalidElementInContext = false;

            if (context.NeedValidateChildren) {
                if (context.IsNill) {
                    SendValidationEvent(Res.Sch_ContentInNill, QNameString(context.LocalName, context.Namespace));
                    return null;
                }
                ContentValidator contentValidator = context.ElementDecl.ContentValidator;
                if (contentValidator.ContentType == XmlSchemaContentType.Mixed && context.ElementDecl.Presence == SchemaDeclBase.Use.Fixed) { //Mixed with default or fixed
                    SendValidationEvent(Res.Sch_ElementInMixedWithFixed, QNameString(context.LocalName, context.Namespace));
                    return null;
                }

                head = elementName;
                bool substitution = false;

                while (true) {
                    particle = context.ElementDecl.ContentValidator.ValidateElement(head, context, out errorCode);
                    if (particle != null) { //Match found
                        break;
                    }
                    if (errorCode == -2) { //ContentModel all group error
                        SendValidationEvent(Res.Sch_AllElement, elementName.ToString());
                        invalidElementInContext = true;
                        processContents = context.ProcessContents = XmlSchemaContentProcessing.Skip;
                        return null;
                    }
                    //Match not found; check for substitutionGroup
                    substitution = true;
                    headElement = GetSubstitutionGroupHead(head);
                    if (headElement == null) {
                        break;
                    }
                    else {
                        head = headElement.QualifiedName;
                    }
                }

                if (substitution) {
                    XmlSchemaElement matchedElem = particle as XmlSchemaElement;
                    if (matchedElem == null) { //It matched an xs:any in that position
                        particle = null;
                    }
                    else if (matchedElem.RefName.IsEmpty) { //It is not element ref but a local element
                        //If the head and matched particle are not hte same, then this is not substitutable, duped by a localElement with same QName
                        SendValidationEvent(Res.Sch_InvalidElementSubstitution, BuildElementName(elementName), BuildElementName(matchedElem.QualifiedName));
                        invalidElementInContext = true;
                        processContents = context.ProcessContents = XmlSchemaContentProcessing.Skip;
                    }
                    else { //Correct substitution head found
                        particle = compiledSchemaInfo.GetElement(elementName); //Re-assign correct particle
                        context.NeedValidateChildren = true; //This will be reset to false once member match is not found
                    }
                }
                if (particle == null) {
                    ElementValidationError(elementName, context, eventHandler, nsResolver, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition, schemaSet);
                    invalidElementInContext = true;
                    processContents = context.ProcessContents = XmlSchemaContentProcessing.Skip;
                }
            }
            return particle;
        }


        private XmlSchemaElement GetSubstitutionGroupHead(XmlQualifiedName member) {
            XmlSchemaElement memberElem = compiledSchemaInfo.GetElement(member);
            if (memberElem != null) {
                XmlQualifiedName head = memberElem.SubstitutionGroup;
                if(!head.IsEmpty) {
                    XmlSchemaElement headElem = compiledSchemaInfo.GetElement(head);
                    if (headElem != null) {
                        if ((headElem.BlockResolved & XmlSchemaDerivationMethod.Substitution) != 0) {
                            SendValidationEvent(Res.Sch_SubstitutionNotAllowed, new string[] {member.ToString(), head.ToString()});
                            return null;
                        }
                        if (!XmlSchemaType.IsDerivedFrom(memberElem.ElementSchemaType, headElem.ElementSchemaType, headElem.BlockResolved)) {
                            SendValidationEvent(Res.Sch_SubstitutionBlocked, new string[] {member.ToString(), head.ToString()});
                            return null;
                        }
                        return headElem;
                    }
                }
            }
            return null;
        }

        private object ValidateAtomicValue(string stringValue, out XmlSchemaSimpleType memberType) {
            object typedVal = null;
            memberType = null;
            SchemaElementDecl currentElementDecl = context.ElementDecl;
            if (!context.IsNill) {
                if (stringValue.Length == 0 && currentElementDecl.DefaultValueTyped != null) { //default value maybe present
                    SchemaElementDecl declBeforeXsi = context.ElementDeclBeforeXsi;
                    if (declBeforeXsi != null && declBeforeXsi != currentElementDecl) { //There was xsi:type
                        Debug.Assert(currentElementDecl.Datatype != null);
                        Exception exception = currentElementDecl.Datatype.TryParseValue(currentElementDecl.DefaultValueRaw, nameTable, nsResolver, out typedVal);
                        if (exception != null) {
                            SendValidationEvent(Res.Sch_InvalidElementDefaultValue, new string[] { currentElementDecl.DefaultValueRaw, QNameString(context.LocalName, context.Namespace) });
                        }
                        else {
                            context.IsDefault = true;
                        }
                    }
                    else {
                        context.IsDefault = true;
                        typedVal = currentElementDecl.DefaultValueTyped;
                    }
                }
                else {
                    typedVal = CheckElementValue(stringValue);
                }
                XsdSimpleValue simpleValue = typedVal as XsdSimpleValue;
                XmlSchemaDatatype dtype = currentElementDecl.Datatype;
                if (simpleValue != null) {
                    memberType = simpleValue.XmlType;
                    typedVal = simpleValue.TypedValue;
                    dtype = memberType.Datatype;
                }
                CheckTokenizedTypes(dtype, typedVal, false);
            }
            return typedVal;
        }

        private object ValidateAtomicValue(object parsedValue, out XmlSchemaSimpleType memberType) {
            memberType = null;
            SchemaElementDecl currentElementDecl = context.ElementDecl;
            object typedValue = null;
            if (!context.IsNill) {
                SchemaDeclBase decl = currentElementDecl as SchemaDeclBase;
                XmlSchemaDatatype dtype = currentElementDecl.Datatype;
                Exception exception = dtype.TryParseValue(parsedValue, nameTable, nsResolver, out typedValue);
                if (exception != null) {
                    string stringValue = parsedValue as string;
                    if (stringValue == null) {
                        stringValue = XmlSchemaDatatype.ConcatenatedToString(parsedValue);
                    }
                    SendValidationEvent(Res.Sch_ElementValueDataTypeDetailed, new string[] { QNameString(context.LocalName, context.Namespace), stringValue, GetTypeName(decl), exception.Message }, exception);
                    return null;
                }
                if (!decl.CheckValue(typedValue)) {
                    SendValidationEvent(Res.Sch_FixedElementValue, QNameString(context.LocalName, context.Namespace));
                }
                if (dtype.Variety == XmlSchemaDatatypeVariety.Union) {
                    XsdSimpleValue simpleValue = typedValue as XsdSimpleValue;
                    Debug.Assert(simpleValue != null);
                    memberType = simpleValue.XmlType;
                    typedValue = simpleValue.TypedValue;
                    dtype = memberType.Datatype;
                }
                CheckTokenizedTypes(dtype, typedValue, false);
            }
            return typedValue;
        }

        private string GetTypeName(SchemaDeclBase decl) {
            Debug.Assert(decl != null && decl.SchemaType != null);
            string typeName = decl.SchemaType.QualifiedName.ToString();
            if (typeName.Length == 0) {
                typeName = decl.Datatype.TypeCodeString;
            }
            return typeName;
        }

        private void SaveTextValue(object value) {
            string s = value.ToString(); //For strings, which will mostly be the case, ToString() will return this. For other typedValues, need to go through value converter (eg: TimeSpan, DateTime etc)
            textValue.Append(s);
        }

        private void Push(XmlQualifiedName elementName) {
            context = (ValidationState)validationStack.Push();
            if (context == null) {
                context = new ValidationState();
                validationStack.AddToTop(context);
            }
            context.LocalName = elementName.Name;
            context.Namespace = elementName.Namespace;
            context.HasMatched = false;
            context.IsNill = false;
            context.IsDefault = false;
            context.CheckRequiredAttribute = true;
            context.ValidationSkipped = false;
            context.Validity = XmlSchemaValidity.NotKnown;
            context.NeedValidateChildren = false;
            context.ProcessContents = processContents;
            context.ElementDeclBeforeXsi = null;
            context.Constr = null; //resetting the constraints to be null incase context != null
                                   // when pushing onto stack;
        }

        private    void Pop() {
            Debug.Assert(validationStack.Length > 1);
            ValidationState previousContext = (ValidationState)validationStack.Pop();

            if (startIDConstraint == validationStack.Length) {
                startIDConstraint = -1;
            }
            context = (ValidationState)validationStack.Peek();
            if (previousContext.Validity == XmlSchemaValidity.Invalid) { //Should set current context's validity to that of what was popped now in case of Invalid
                context.Validity = XmlSchemaValidity.Invalid;
            }
            if (previousContext.ValidationSkipped) {
                context.ValidationSkipped = true;
            }
            processContents = context.ProcessContents;
        }

        private void AddXsiAttributes(ArrayList attList) {
            BuildXsiAttributes();
            if (attPresence[xsiTypeSO.QualifiedName] == null) {
                attList.Add(xsiTypeSO);
            }
            if (attPresence[xsiNilSO.QualifiedName] == null) {
                attList.Add(xsiNilSO);
            }
            if (attPresence[xsiSLSO.QualifiedName] == null) {
                attList.Add(xsiSLSO);
            }
            if (attPresence[xsiNoNsSLSO.QualifiedName] == null) {
                attList.Add(xsiNoNsSLSO);
            }
        }

        private SchemaElementDecl FastGetElementDecl(XmlQualifiedName elementName, object particle) {
            SchemaElementDecl elementDecl = null;
            if (particle != null) {
                XmlSchemaElement element = particle as XmlSchemaElement;
                if (element != null) {
                    elementDecl = element.ElementDecl;
                }
                else {
                    XmlSchemaAny any = (XmlSchemaAny)particle;
                    processContents = any.ProcessContentsCorrect;
                }
            }
            if (elementDecl == null && processContents != XmlSchemaContentProcessing.Skip) {
                if (isRoot && partialValidationType != null) {
                    if (partialValidationType is XmlSchemaElement) {
                        XmlSchemaElement element = (XmlSchemaElement)partialValidationType;
                        if (elementName.Equals(element.QualifiedName)) {
                            elementDecl = element.ElementDecl;
                        }
                        else {
                            SendValidationEvent(Res.Sch_SchemaElementNameMismatch, elementName.ToString(), element.QualifiedName.ToString());
                        }
                    }
                    else if (partialValidationType is XmlSchemaType) { //Element name is wildcard
                        XmlSchemaType type = (XmlSchemaType)partialValidationType;
                        elementDecl = type.ElementDecl;
                    }
                    else { //its XmlSchemaAttribute
                        Debug.Assert(partialValidationType is XmlSchemaAttribute);
                        SendValidationEvent(Res.Sch_ValidateElementInvalidCall, string.Empty);
                    }
                }
                else {
                    elementDecl = compiledSchemaInfo.GetElementDecl(elementName);
                }
            }
            return elementDecl;
        }

        private SchemaElementDecl CheckXsiTypeAndNil(SchemaElementDecl elementDecl, string xsiType, string xsiNil, ref bool declFound) {
            XmlQualifiedName xsiTypeName = XmlQualifiedName.Empty;
            if (xsiType != null) {
                object typedVal = null;
                Exception exception = dtQName.TryParseValue(xsiType, nameTable, nsResolver, out typedVal);
                if (exception != null) {
                    SendValidationEvent(Res.Sch_InvalidValueDetailedAttribute, new string[] { "type", xsiType, dtQName.TypeCodeString, exception.Message }, exception);
                }
                else {
                    xsiTypeName = typedVal as XmlQualifiedName;
                }
            }
            if (elementDecl != null) { //nillable is not dependent on xsi:type.
                if (elementDecl.IsNillable) {
                    if (xsiNil != null) {
                        context.IsNill = XmlConvert.ToBoolean(xsiNil);
                        if (context.IsNill && elementDecl.Presence == SchemaDeclBase.Use.Fixed) {
                            Debug.Assert(elementDecl.DefaultValueTyped != null);				
                            SendValidationEvent(Res.Sch_XsiNilAndFixed);
                        }
                    }
                }
                else if (xsiNil != null) {
                    SendValidationEvent(Res.Sch_InvalidXsiNill);
                }
            }
            if (xsiTypeName.IsEmpty) {
                if (elementDecl != null && elementDecl.IsAbstract) {
                    SendValidationEvent(Res.Sch_AbstractElement, QNameString(context.LocalName, context.Namespace));
                    elementDecl = null;
                }
            }
            else {
                SchemaElementDecl elementDeclXsi = compiledSchemaInfo.GetTypeDecl(xsiTypeName);
                XmlSeverityType severity = XmlSeverityType.Warning;
                if (HasSchema && processContents == XmlSchemaContentProcessing.Strict) {
                    severity = XmlSeverityType.Error;
                }
                if (elementDeclXsi == null && xsiTypeName.Namespace == NsXs) {
                    XmlSchemaType schemaType = DatatypeImplementation.GetSimpleTypeFromXsdType(xsiTypeName);
                    if (schemaType == null) { //try getting complexType - xs:anyType
                        schemaType = XmlSchemaType.GetBuiltInComplexType(xsiTypeName);
                    }
                    if (schemaType != null) {
                        elementDeclXsi = schemaType.ElementDecl;
                    }

                }
                if (elementDeclXsi == null) {
                    SendValidationEvent(Res.Sch_XsiTypeNotFound, xsiTypeName.ToString(), severity);
                    elementDecl = null;
                }
                else {
                    declFound = true;
                    if (elementDeclXsi.IsAbstract) {
                        SendValidationEvent(Res.Sch_XsiTypeAbstract, xsiTypeName.ToString(), severity);
                        elementDecl = null;
                    }
                    else if (elementDecl != null && !XmlSchemaType.IsDerivedFrom(elementDeclXsi.SchemaType,elementDecl.SchemaType,elementDecl.Block)) {
                        SendValidationEvent(Res.Sch_XsiTypeBlockedEx, new string[] { xsiTypeName.ToString(), QNameString(context.LocalName, context.Namespace) });
                        elementDecl = null;
                    }
                    else {
                        if (elementDecl != null) { //Get all element decl properties before assigning xsi:type decl; nillable already checked
                            elementDeclXsi = elementDeclXsi.Clone(); //Before updating properties onto xsi:type decl, clone it
                            elementDeclXsi.Constraints = elementDecl.Constraints;
                            elementDeclXsi.DefaultValueRaw = elementDecl.DefaultValueRaw;
                            elementDeclXsi.DefaultValueTyped = elementDecl.DefaultValueTyped;
                            elementDeclXsi.Block = elementDecl.Block;
                        }
                        context.ElementDeclBeforeXsi = elementDecl;
                        elementDecl = elementDeclXsi;
                    }
                }
            }
            return elementDecl;
        }

        private void ThrowDeclNotFoundWarningOrError(bool declFound) {
            if (declFound) { //But invalid, so discontinue processing of children
                processContents = context.ProcessContents = XmlSchemaContentProcessing.Skip;
                context.NeedValidateChildren = false;
            }
            else if (HasSchema && processContents == XmlSchemaContentProcessing.Strict) { //Error and skip validation for children
                processContents = context.ProcessContents = XmlSchemaContentProcessing.Skip;
                context.NeedValidateChildren = false;
                SendValidationEvent(Res.Sch_UndeclaredElement, QNameString(context.LocalName, context.Namespace));
            }
            else {
                SendValidationEvent(Res.Sch_NoElementSchemaFound, QNameString(context.LocalName, context.Namespace), XmlSeverityType.Warning);
            }
        }

        private void CheckElementProperties () {
          if (context.ElementDecl.IsAbstract) {
              SendValidationEvent(Res.Sch_AbstractElement, QNameString(context.LocalName, context.Namespace));
          }
        }

        private void ValidateStartElementIdentityConstraints() {
            // added on June 15, set the context here, so the stack can have them
            if (ProcessIdentityConstraints && context.ElementDecl.Constraints != null) {
                AddIdentityConstraints();
            }
            //foreach constraint in stack (including the current one)
            if (HasIdentityConstraints) {
                ElementIdentityConstraints();
            }
        }

        private SchemaAttDef CheckIsXmlAttribute(XmlQualifiedName attQName) {
            SchemaAttDef attdef = null;
            if (Ref.Equal(attQName.Namespace, NsXml) && (validationFlags & XmlSchemaValidationFlags.AllowXmlAttributes) != 0) {  //Need to check if this attribute is an xml attribute
                if (!compiledSchemaInfo.Contains(NsXml)) { //We dont have a schema for xml namespace
                    // It can happen that the schemaSet already contains the schema for xml namespace
                    //   and we just have a stale compiled schema info (for example if the same schema set is used
                    //   by two validators at the same time and the one before us added the xml namespace schema
                    //   via this code here)
                    // In that case it is actually OK to try to add the schema for xml namespace again
                    //   since we're adding the exact same instance (the built in xml namespace schema is a singleton)
                    //   The addition on the schemaset is an effective no-op plus it's thread safe, so it's better to leave
                    //   that up to the schema set. The result of the below call will be simply that we update the
                    //   reference to the comipledSchemaInfo - which is exactly what we want in that case.
                    // In theory it can actually happen that there is some other schema registered for the xml namespace
                    //   (other than our built in one), and we don't know about it. In that case we don't support such scenario
                    //   as the user is modifying the schemaset as we're using it, which we don't support
                    //   for bunch of other reasons, so trying to add our built-in schema won't make it worse.
                    AddXmlNamespaceSchema();
                }
                compiledSchemaInfo.AttributeDecls.TryGetValue(attQName, out attdef); //the xml attributes are all global attributes
            }
            return attdef;
        }

        private void AddXmlNamespaceSchema() {
            XmlSchemaSet localSet = new XmlSchemaSet(); //Avoiding cost of incremental compilation checks by compiling schema in a seperate set and adding compiled set
            localSet.Add(Preprocessor.GetBuildInSchema());
            localSet.Compile();
            schemaSet.Add(localSet);
            RecompileSchemaSet();
        }

        internal object CheckMixedValueConstraint(string elementValue) {
            SchemaElementDecl elementDecl = context.ElementDecl;
            Debug.Assert(elementDecl.ContentValidator.ContentType == XmlSchemaContentType.Mixed && elementDecl.DefaultValueTyped != null);
            if (context.IsNill) { //Nil and fixed is error; Nil and default is compile time error
                return null;
            }
            if (elementValue.Length == 0) {
                context.IsDefault = true;
                return elementDecl.DefaultValueTyped;
            }
            else {
                SchemaDeclBase decl = elementDecl as SchemaDeclBase;
                Debug.Assert(decl != null);
                if (decl.Presence == SchemaDeclBase.Use.Fixed && !elementValue.Equals(elementDecl.DefaultValueRaw)) { //check string equality for mixed as it is untyped.
                    SendValidationEvent(Res.Sch_FixedElementValue, elementDecl.Name.ToString());
                }
                return elementValue;
            }
        }

        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        private void LoadSchema(string uri, string url) {
            Debug.Assert(xmlResolver != null);
            XmlReader Reader = null;
            try {
                Uri ruri = xmlResolver.ResolveUri(sourceUri, url);
                Stream stm = (Stream)xmlResolver.GetEntity(ruri,null,null);
                XmlReaderSettings readerSettings = schemaSet.ReaderSettings;
                readerSettings.CloseInput = true;
                readerSettings.XmlResolver = xmlResolver;
                Reader = XmlReader.Create(stm, readerSettings, ruri.ToString());
                schemaSet.Add(uri, Reader, validatedNamespaces);
                while(Reader.Read());// wellformness check
            }
            catch(XmlSchemaException e) {
                SendValidationEvent(Res.Sch_CannotLoadSchema, new string[] {uri, e.Message}, e);
            }
            catch(Exception e) {
                SendValidationEvent(Res.Sch_CannotLoadSchema, new string[] {uri, e.Message}, e, XmlSeverityType.Warning);
            }
            finally {
                if (Reader != null) {
                    Reader.Close();
                }
            }
        }


        internal void RecompileSchemaSet() {
            if (!schemaSet.IsCompiled) {
                try {
                    schemaSet.Compile();
                }
                catch(XmlSchemaException e) {
                    SendValidationEvent(e);
                }
            }
            compiledSchemaInfo = schemaSet.CompiledInfo; //Fetch compiled info from set
        }

        private void ProcessTokenizedType(XmlTokenizedType ttype, string name, bool attrValue) {
            switch(ttype) {
                case XmlTokenizedType.ID:
                    if (ProcessIdentityConstraints) {
                        if (FindId(name) != null) {
                            if (attrValue) {
                                attrValid = false;
                            }
                            SendValidationEvent(Res.Sch_DupId, name);
                        }
                        else {
                            if (IDs == null) { //ADD ID
                                IDs = new Hashtable();
                            }
                            IDs.Add(name, context.LocalName);
                        }
                    }
                    break;
                case XmlTokenizedType.IDREF:
                    if (ProcessIdentityConstraints) {
                        object p = FindId(name);
                        if (p == null) { // add it to linked list to check it later
                            idRefListHead = new IdRefNode(idRefListHead, name, positionInfo.LineNumber, positionInfo.LinePosition);
                        }
                    }
                    break;
                case XmlTokenizedType.ENTITY:
                    ProcessEntity(name);
                    break;
                default:
                    break;
            }
        }

        private object CheckAttributeValue(object value, SchemaAttDef attdef) {
            object typedValue = null;
            SchemaDeclBase decl = attdef as SchemaDeclBase;

            XmlSchemaDatatype dtype = attdef.Datatype;
            Debug.Assert(dtype != null);
            string stringValue = value as string;
            Exception exception = null;

            if (stringValue != null) { //
                exception = dtype.TryParseValue(stringValue, nameTable, nsResolver, out typedValue);
                if (exception != null) goto Error;
            }
            else { //Calling object ParseValue for checking facets
                exception = dtype.TryParseValue(value, nameTable, nsResolver, out typedValue);
                if (exception != null) goto Error;
            }
            if (!decl.CheckValue(typedValue)) {
                attrValid = false;
                SendValidationEvent(Res.Sch_FixedAttributeValue, attdef.Name.ToString());
            }
            return typedValue;

        Error:
            attrValid = false;
            if (stringValue == null) {
                stringValue = XmlSchemaDatatype.ConcatenatedToString(value);
            }
            SendValidationEvent(Res.Sch_AttributeValueDataTypeDetailed, new string[] { attdef.Name.ToString(), stringValue, GetTypeName(decl), exception.Message }, exception);
            return null;
        }

        private object CheckElementValue(string stringValue) {
            object typedValue = null;
            SchemaDeclBase decl = context.ElementDecl as SchemaDeclBase;

            XmlSchemaDatatype dtype = decl.Datatype;
            Debug.Assert(dtype != null);

            Exception exception = dtype.TryParseValue(stringValue, nameTable, nsResolver, out typedValue);
            if (exception != null) {
                SendValidationEvent(Res.Sch_ElementValueDataTypeDetailed, new string[] { QNameString(context.LocalName, context.Namespace), stringValue, GetTypeName(decl), exception.Message }, exception);
                return null;
            }
            if (!decl.CheckValue(typedValue)) {
                SendValidationEvent(Res.Sch_FixedElementValue, QNameString(context.LocalName, context.Namespace));
            }
            return typedValue;
        }

        private void CheckTokenizedTypes(XmlSchemaDatatype dtype, object typedValue, bool attrValue) {
            // Check special types
            if (typedValue == null) {
                return;
            }
            XmlTokenizedType ttype = dtype.TokenizedType;
            if (ttype == XmlTokenizedType.ENTITY || ttype == XmlTokenizedType.ID || ttype == XmlTokenizedType.IDREF) {
                if (dtype.Variety == XmlSchemaDatatypeVariety.List) {
                    string[] ss = (string[])typedValue;
                    for (int i = 0; i < ss.Length; ++i) {
                        ProcessTokenizedType(dtype.TokenizedType, ss[i], attrValue);
                    }
                }
                else {
                    ProcessTokenizedType(dtype.TokenizedType, (string)typedValue, attrValue);
                }
            }
        }

        private object  FindId(string name) {
            return IDs == null ? null : IDs[name];
        }

        private void CheckForwardRefs() {
            IdRefNode next = idRefListHead;
            while (next != null) {
                if(FindId(next.Id) == null) {
                    SendValidationEvent(new XmlSchemaValidationException(Res.Sch_UndeclaredId, next.Id, this.sourceUriString, next.LineNo, next.LinePos), XmlSeverityType.Error);
                }
                IdRefNode ptr = next.Next;
                next.Next = null; // unhook each object so it is cleaned up by Garbage Collector
                next = ptr;
            }
            // not needed any more.
            idRefListHead = null;
        }


        private bool HasIdentityConstraints {
            get { return ProcessIdentityConstraints && startIDConstraint != -1; }
        }

        internal bool ProcessIdentityConstraints {
            get {
                return (validationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) != 0;
            }
        }

        internal bool ReportValidationWarnings {
            get {
                return (validationFlags & XmlSchemaValidationFlags.ReportValidationWarnings) != 0;
            }
        }

        internal bool ProcessInlineSchema {
            get {
                return (validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != 0;
            }
        }

        internal bool ProcessSchemaLocation {
            get {
                return (validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != 0;
            }
        }

        internal bool ProcessSchemaHints {
            get {
                return (validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != 0 ||
                       (validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != 0;
            }
        }

        private void CheckStateTransition(ValidatorState toState, string methodName) {
            if (!ValidStates[(int)currentState,(int)toState]) {
                if (currentState == ValidatorState.None) {
                    throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidStartTransition, new string[] { methodName, MethodNames[(int)ValidatorState.Start] }));
                }
                throw new InvalidOperationException(Res.GetString(Res.Sch_InvalidStateTransition, new string[] { MethodNames[(int)currentState], methodName }));
            }
            currentState = toState;
        }

        private void ClearPSVI() {
            if (textValue != null) {
                textValue.Length = 0;
            }
            attPresence.Clear(); //Clear attributes hashtable for every element
            wildID = null; //clear it for every element
        }

        private void CheckRequiredAttributes(SchemaElementDecl currentElementDecl) {
            Debug.Assert(currentElementDecl != null);
            Dictionary<XmlQualifiedName, SchemaAttDef> attributeDefs = currentElementDecl.AttDefs;
            foreach(SchemaAttDef attdef in attributeDefs.Values) {
                if (attPresence[attdef.Name] == null) {
                    if (attdef.Presence == SchemaDeclBase.Use.Required || attdef.Presence == SchemaDeclBase.Use.RequiredFixed) {
                        SendValidationEvent(Res.Sch_MissRequiredAttribute, attdef.Name.ToString());
                    }
                }
            }
        }

        private XmlSchemaElement GetSchemaElement() {
            SchemaElementDecl beforeXsiDecl = context.ElementDeclBeforeXsi;
            SchemaElementDecl currentDecl = context.ElementDecl;

            if (beforeXsiDecl != null) { //Have a xsi:type
                if (beforeXsiDecl.SchemaElement != null) {
                    XmlSchemaElement xsiElement = (XmlSchemaElement)beforeXsiDecl.SchemaElement.Clone(null);
                    xsiElement.SchemaTypeName = XmlQualifiedName.Empty; //Reset typeName on element as this might be different
                    xsiElement.SchemaType = currentDecl.SchemaType;
                    xsiElement.SetElementType(currentDecl.SchemaType);
                    xsiElement.ElementDecl = currentDecl;
                    return xsiElement;
                }
            }
            return currentDecl.SchemaElement;
        }

        internal string GetDefaultAttributePrefix(string attributeNS) {
            IDictionary<string,string> namespaceDecls = nsResolver.GetNamespacesInScope(XmlNamespaceScope.All);
            string defaultPrefix = null;
            string defaultNS;

            foreach (KeyValuePair<string,string> pair in namespaceDecls) {
                defaultNS = nameTable.Add(pair.Value);
                if (Ref.Equal(defaultNS, attributeNS)) {
                    defaultPrefix = pair.Key;
                    if (defaultPrefix.Length != 0) { //Locate first non-empty prefix
                        return defaultPrefix;
                    }
                }
            }
            return defaultPrefix;
        }

        private void AddIdentityConstraints() {
            SchemaElementDecl currentElementDecl = context.ElementDecl;
            context.Constr = new ConstraintStruct[currentElementDecl.Constraints.Length];
            int id = 0;
            for (int i = 0; i < currentElementDecl.Constraints.Length; ++i)
            {
                context.Constr[id++] = new ConstraintStruct(currentElementDecl.Constraints[i]);
            } // foreach constraint /constraintstruct

            // added on June 19, make connections between new keyref tables with key/unique tables in stack
            // i can't put it in the above loop, coz there will be key on the same level
            for (int i = 0; i < context.Constr.Length; ++i) {
                if ( context.Constr[i].constraint.Role == CompiledIdentityConstraint.ConstraintRole.Keyref ) {
                    bool find = false;
                    // go upwards checking or only in this level
                    for (int level = this.validationStack.Length - 1; level >= ((this.startIDConstraint >= 0) ? this.startIDConstraint : this.validationStack.Length - 1); level --) {
                        // no constraint for this level
                        if (((ValidationState)(this.validationStack[level])).Constr == null) {
                            continue;
                        }
                        // else
                        ConstraintStruct[] constraintStructures = ((ValidationState)this.validationStack[level]).Constr;
                        for (int j = 0; j < constraintStructures.Length; ++j) {
                            if (constraintStructures[j].constraint.name == context.Constr[i].constraint.refer) {
                                find = true;
                                if (constraintStructures[j].keyrefTable == null) {
                                    constraintStructures[j].keyrefTable = new Hashtable();
                                }
                                context.Constr[i].qualifiedTable = constraintStructures[j].keyrefTable;
                                break;
                            }
                        }

                        if (find) {
                            break;
                        }
                    }
                    if (!find) {
                        // didn't find connections, throw exceptions
                        SendValidationEvent(Res.Sch_RefNotInScope, QNameString(context.LocalName, context.Namespace));
                    }
                } // finished dealing with keyref

            }  // end foreach

            // initial set
            if (this.startIDConstraint == -1) {
                this.startIDConstraint = this.validationStack.Length - 1;
            }
        }

        private void ElementIdentityConstraints () {
            SchemaElementDecl currentElementDecl = context.ElementDecl;
            string localName = context.LocalName;
            string namespaceUri = context.Namespace;

            for (int i = this.startIDConstraint; i < this.validationStack.Length; i ++) {
                // no constraint for this level
                if (((ValidationState)(this.validationStack[i])).Constr == null) {
                    continue;
                }

                // else
                ConstraintStruct[] constraintStructures = ((ValidationState)this.validationStack[i]).Constr;
                for (int j = 0; j < constraintStructures.Length; ++j) {
                    // check selector from here
                    if (constraintStructures[j].axisSelector.MoveToStartElement(localName, namespaceUri)) {
                        // selector selects new node, activate a new set of fields
                        Debug.WriteLine("Selector Match!");
                        Debug.WriteLine("Name: " + localName + "\t|\tURI: " + namespaceUri + "\n");

                        // in which axisFields got updated
                        constraintStructures[j].axisSelector.PushKS(positionInfo.LineNumber, positionInfo.LinePosition);
                    }

                    // axisFields is not null, but may be empty
                    for (int k = 0; k < constraintStructures[j].axisFields.Count; ++k) {
                        LocatedActiveAxis laxis = (LocatedActiveAxis)constraintStructures[j].axisFields[k];

                        // check field from here
                        if (laxis.MoveToStartElement(localName, namespaceUri)) {
                            Debug.WriteLine("Element Field Match!");
                            // checking simpleType / simpleContent
                            if (currentElementDecl != null) {      // nextElement can be null when xml/xsd are not valid
                                if (currentElementDecl.Datatype == null || currentElementDecl.ContentValidator.ContentType == XmlSchemaContentType.Mixed) {
                                    SendValidationEvent(Res.Sch_FieldSimpleTypeExpected, localName);
                                }
                                else {
                                    // can't fill value here, wait till later....
                                    // fill type : xsdType
                                    laxis.isMatched = true;
                                    // since it's simpletyped element, the endchildren will come consequently... don't worry
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AttributeIdentityConstraints(string name, string ns, object obj, string sobj, XmlSchemaDatatype datatype) {
            for (int ci = this.startIDConstraint; ci < this.validationStack.Length; ci ++) {
                // no constraint for this level
                if (((ValidationState)(this.validationStack[ci])).Constr == null) {
                    continue;
                }

                // else
                ConstraintStruct[] constraintStructures = ((ValidationState)this.validationStack[ci]).Constr;
                for (int i = 0; i < constraintStructures.Length; ++i) {
                    // axisFields is not null, but may be empty
                    for (int j = 0; j < constraintStructures[i].axisFields.Count; ++j) {
                        LocatedActiveAxis laxis = (LocatedActiveAxis)constraintStructures[i].axisFields[j];

                        // check field from here
                        if (laxis.MoveToAttribute(name, ns)) {
                            Debug.WriteLine("Attribute Field Match!");
                            //attribute is only simpletype, so needn't checking...
                            // can fill value here, yeah!!
                            Debug.WriteLine("Attribute Field Filling Value!");
                            Debug.WriteLine("Name: " + name + "\t|\tURI: " + ns + "\t|\tValue: " + obj + "\n");
                            if (laxis.Ks[laxis.Column] != null) {
                                // should be evaluated to either an empty node-set or a node-set with exactly one member
                                // two matches...
                                SendValidationEvent (Res.Sch_FieldSingleValueExpected, name);
                            }
                            else {
                                Debug.Assert(datatype != null);
                                laxis.Ks[laxis.Column] = new TypedObject (obj, sobj, datatype);
                            }
                        }
                    }
                }
            }
        }

        private void EndElementIdentityConstraints(object typedValue, string stringValue, XmlSchemaDatatype datatype) {
            string localName = context.LocalName;
            string namespaceUri = context.Namespace;
            for (int ci = this.validationStack.Length - 1; ci >= this.startIDConstraint; ci --) {
                // no constraint for this level
                if (((ValidationState)(this.validationStack[ci])).Constr == null) {
                    continue;
                }

                // else
                ConstraintStruct[] constraints = ((ValidationState)this.validationStack[ci]).Constr;
                for (int i = 0; i < constraints.Length; ++i) {
                    // EndChildren
                    // axisFields is not null, but may be empty
                    for (int j = 0; j < constraints[i].axisFields.Count; ++j) {
                        LocatedActiveAxis laxis = (LocatedActiveAxis)constraints[i].axisFields[j];

                        // check field from here
                        // isMatched is false when nextElement is null. so needn't change this part.
                        if (laxis.isMatched) {
                            Debug.WriteLine("Element Field Filling Value!");
                            Debug.WriteLine("Name: " + localName + "\t|\tURI: " + namespaceUri + "\t|\tValue: " + typedValue + "\n");
                            // fill value
                            laxis.isMatched = false;
                            if (laxis.Ks[laxis.Column] != null) {
                                // [field...] should be evaluated to either an empty node-set or a node-set with exactly one member
                                // two matches... already existing field value in the table.
                                SendValidationEvent (Res.Sch_FieldSingleValueExpected, localName);
                            }
                            else {
                                // for element, Reader.Value = "";
                                if(typedValue != null && stringValue.Length != 0) {
                                    laxis.Ks[laxis.Column] = new TypedObject(typedValue, stringValue, datatype);
                                }
                            }
                        }
                        // EndChildren
                        laxis.EndElement(localName, namespaceUri);
                    }

                    if (constraints[i].axisSelector.EndElement(localName, namespaceUri)) {
                        // insert key sequence into hash (+ located active axis tuple leave for later)
                        KeySequence ks = constraints[i].axisSelector.PopKS();
                        // unqualified keysequence are not allowed
                        switch (constraints[i].constraint.Role) {
                        case CompiledIdentityConstraint.ConstraintRole.Key:
                            if (! ks.IsQualified()) {
                                //Key's fields can't be null...  if we can return context node's line info maybe it will be better
                                //only keymissing & keyduplicate reporting cases are necessary to be dealt with... 3 places...
                                SendValidationEvent(new XmlSchemaValidationException(Res.Sch_MissingKey, constraints[i].constraint.name.ToString(), sourceUriString, ks.PosLine, ks.PosCol));
                            }
                            else if (constraints[i].qualifiedTable.Contains (ks)) {
                                // unique or key checking value confliction
                                // for redundant key, reporting both occurings
                                // doesn't work... how can i retrieve value out??
//                                        KeySequence ks2 = (KeySequence) conuct.qualifiedTable[ks];
                                SendValidationEvent(new XmlSchemaValidationException(Res.Sch_DuplicateKey,
                                    new string[2] {ks.ToString(), constraints[i].constraint.name.ToString()},
                                    sourceUriString, ks.PosLine, ks.PosCol));
                            }
                            else {
                                constraints[i].qualifiedTable.Add (ks, ks);
                            }
                            break;

                        case CompiledIdentityConstraint.ConstraintRole.Unique:
                            if (! ks.IsQualified()) {
                                continue;
                            }
                            if (constraints[i].qualifiedTable.Contains (ks)) {
                                // unique or key checking confliction
//                                        KeySequence ks2 = (KeySequence) conuct.qualifiedTable[ks];
                                SendValidationEvent(new XmlSchemaValidationException(Res.Sch_DuplicateKey,
                                    new string[2] {ks.ToString(), constraints[i].constraint.name.ToString()},
                                    sourceUriString, ks.PosLine, ks.PosCol));
                            }
                            else {
                                constraints[i].qualifiedTable.Add (ks, ks);
                            }
                            break;
                        case CompiledIdentityConstraint.ConstraintRole.Keyref:
                            // is there any possibility:
                            // 2 keyrefs: value is equal, type is not
                            // both put in the hashtable, 1 reference, 1 not
                            if (constraints[i].qualifiedTable != null) { //Will be null in cases when the keyref is outside the scope of the key, that is not allowed by our impl
                                if (! ks.IsQualified() || constraints[i].qualifiedTable.Contains (ks)) {
                                    continue;
                                }
                                constraints[i].qualifiedTable.Add (ks, ks);
                            }
                        break;
                        }
                    }
                }
            }


            // current level's constraint struct
            ConstraintStruct[] vcs = ((ValidationState)(this.validationStack[this.validationStack.Length - 1])).Constr;
            if ( vcs != null) {
                // validating all referencing tables...

                for (int i = 0; i < vcs.Length; ++i) {
                    if (( vcs[i].constraint.Role == CompiledIdentityConstraint.ConstraintRole.Keyref)
                        || (vcs[i].keyrefTable == null)) {
                        continue;
                    }
                    foreach (KeySequence ks in vcs[i].keyrefTable.Keys) {
                        if (!vcs[i].qualifiedTable.Contains(ks)) {
                            SendValidationEvent(new XmlSchemaValidationException(Res.Sch_UnresolvedKeyref, new string[2] { ks.ToString(), vcs[i].constraint.name.ToString() },
                                sourceUriString, ks.PosLine, ks.PosCol));
                        }
                    }
                }
            }

          } //End of method

        private static void BuildXsiAttributes() {
            if (xsiTypeSO == null) { //xsi:type attribute
                XmlSchemaAttribute tempXsiTypeSO = new XmlSchemaAttribute();
                tempXsiTypeSO.Name = "type";
                tempXsiTypeSO.SetQualifiedName(new XmlQualifiedName("type", XmlReservedNs.NsXsi));
                tempXsiTypeSO.SetAttributeType(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.QName));
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiTypeSO, tempXsiTypeSO, null);
            }
            if (xsiNilSO == null) { //xsi:nil
                XmlSchemaAttribute tempxsiNilSO = new XmlSchemaAttribute();
                tempxsiNilSO.Name = "nil";
                tempxsiNilSO.SetQualifiedName(new XmlQualifiedName("nil", XmlReservedNs.NsXsi));
                tempxsiNilSO.SetAttributeType(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean));
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiNilSO, tempxsiNilSO, null);
            }
            if (xsiSLSO == null) { //xsi:schemaLocation
                XmlSchemaSimpleType stringType = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
                XmlSchemaAttribute tempxsiSLSO = new XmlSchemaAttribute();
                tempxsiSLSO.Name = "schemaLocation";
                tempxsiSLSO.SetQualifiedName(new XmlQualifiedName("schemaLocation", XmlReservedNs.NsXsi));
                tempxsiSLSO.SetAttributeType(stringType);
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiSLSO, tempxsiSLSO, null);
            }
            if (xsiNoNsSLSO == null) {
                XmlSchemaSimpleType stringType = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
                XmlSchemaAttribute tempxsiNoNsSLSO = new XmlSchemaAttribute();
                tempxsiNoNsSLSO.Name = "noNamespaceSchemaLocation";
                tempxsiNoNsSLSO.SetQualifiedName(new XmlQualifiedName("noNamespaceSchemaLocation", XmlReservedNs.NsXsi));
                tempxsiNoNsSLSO.SetAttributeType(stringType);
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiNoNsSLSO, tempxsiNoNsSLSO, null);
            }
        }

        internal static void ElementValidationError(XmlQualifiedName name, ValidationState context, ValidationEventHandler eventHandler, object sender, string sourceUri, int lineNo, int linePos, XmlSchemaSet schemaSet) {
            ArrayList names = null;
            if (context.ElementDecl != null) {
                ContentValidator contentValidator = context.ElementDecl.ContentValidator;
                XmlSchemaContentType contentType = contentValidator.ContentType;
                if (contentType == XmlSchemaContentType.ElementOnly || (contentType == XmlSchemaContentType.Mixed && contentValidator != ContentValidator.Mixed && contentValidator != ContentValidator.Any)) {
                    Debug.Assert(contentValidator is DfaContentValidator || contentValidator is NfaContentValidator || contentValidator is RangeContentValidator || contentValidator is AllElementsContentValidator);
                    bool getParticles = schemaSet != null;
                    if (getParticles) {
                        names = contentValidator.ExpectedParticles(context, false, schemaSet);
                    }
                    else {
                        names = contentValidator.ExpectedElements(context, false);
                    }

                    if (names == null || names.Count == 0) {
                        if (context.TooComplex) {
                            SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_InvalidElementContentComplex, new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name), Res.GetString(Res.Sch_ComplexContentModel) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                        }
                        else {
                            SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_InvalidElementContent, new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                        }
                    }
                    else {
                        Debug.Assert(names.Count > 0);
                        if (context.TooComplex) {
                            SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_InvalidElementContentExpectingComplex, new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name), PrintExpectedElements(names, getParticles), Res.GetString(Res.Sch_ComplexContentModel) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                        }
                        else {
                            SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_InvalidElementContentExpecting, new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name), PrintExpectedElements(names, getParticles) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                        }
                    }
                }
                else { //Base ContentValidator: Empty || TextOnly || Mixed || Any
                    if (contentType == XmlSchemaContentType.Empty) {
                        SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_InvalidElementInEmptyEx, new string[] { QNameString(context.LocalName, context.Namespace), name.ToString() }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                    }
                    else if (!contentValidator.IsOpen) {
                        SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_InvalidElementInTextOnlyEx, new string[] { QNameString(context.LocalName, context.Namespace), name.ToString() }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                    }
                }
            }
        }

        internal static void CompleteValidationError(ValidationState context, ValidationEventHandler eventHandler, object sender, string sourceUri, int lineNo, int linePos, XmlSchemaSet schemaSet)
        {
            ArrayList names = null;
            bool getParticles = schemaSet != null;
            if (context.ElementDecl != null) {
                if (getParticles) {
                    names = context.ElementDecl.ContentValidator.ExpectedParticles(context, true, schemaSet);
                }
                else {
                    names = context.ElementDecl.ContentValidator.ExpectedElements(context, true);
                }
            }
            if (names == null || names.Count == 0) {
                if (context.TooComplex) {
                    SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_IncompleteContentComplex, new string[] { BuildElementName(context.LocalName, context.Namespace), Res.GetString(Res.Sch_ComplexContentModel) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                }
                SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_IncompleteContent, BuildElementName(context.LocalName, context.Namespace), sourceUri, lineNo, linePos), XmlSeverityType.Error);
            }
            else {
                Debug.Assert(names.Count > 0);
                if (context.TooComplex) {
                    SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_IncompleteContentExpectingComplex, new string[] { BuildElementName(context.LocalName, context.Namespace), PrintExpectedElements(names, getParticles), Res.GetString(Res.Sch_ComplexContentModel) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                }
                else {
                    SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException(Res.Sch_IncompleteContentExpecting, new string[] { BuildElementName(context.LocalName, context.Namespace), PrintExpectedElements(names, getParticles) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                }
            }
        }

        internal static string PrintExpectedElements(ArrayList expected, bool getParticles) {
            if (getParticles) {
                string ContinuationString = Res.GetString(Res.Sch_ContinuationString, new string[] {" "});
                XmlSchemaParticle currentParticle = null;
                XmlSchemaParticle nextParticle = null;
                XmlQualifiedName currentQName;
                ArrayList expectedNames = new ArrayList();
                StringBuilder builder = new StringBuilder();

                if (expected.Count == 1) {
                    nextParticle = expected[0] as XmlSchemaParticle;
                }
                else {
                    for (int i=1; i < expected.Count; i++) {
                        currentParticle = expected[i-1] as XmlSchemaParticle;
                        nextParticle = expected[i] as XmlSchemaParticle;
                        currentQName = currentParticle.GetQualifiedName();
                        if (currentQName.Namespace != nextParticle.GetQualifiedName().Namespace) {
                            expectedNames.Add(currentQName);
                            PrintNamesWithNS(expectedNames, builder);
                            expectedNames.Clear();
                            Debug.Assert(builder.Length != 0);
                            builder.Append(ContinuationString);
                        }
                        else {
                            expectedNames.Add(currentQName);
                        }
                    }
                }
                //Add last one.
                expectedNames.Add(nextParticle.GetQualifiedName());
                PrintNamesWithNS(expectedNames, builder);

                return builder.ToString();
            }
            else {
                return PrintNames(expected);
            }
        }

        private static string PrintNames(ArrayList expected) {
            StringBuilder builder = new StringBuilder();
            builder.Append(Quote);
            builder.Append(expected[0].ToString());
            for (int i = 1; i < expected.Count; ++i) {
                builder.Append(" ");
                builder.Append(expected[i].ToString());
            }
            builder.Append(Quote);
            return builder.ToString();
        }

        private static void PrintNamesWithNS(ArrayList expected, StringBuilder builder) {
            XmlQualifiedName name = null;
            name = expected[0] as XmlQualifiedName;
            if (expected.Count == 1) { //In case of one element in a namespace or any
                if (name.Name == "*") { //Any
                    EnumerateAny(builder, name.Namespace);
                }
                else {
                    if (name.Namespace.Length != 0) {
                        builder.Append(Res.GetString(Res.Sch_ElementNameAndNamespace, name.Name, name.Namespace));
                    }
                    else {
                        builder.Append(Res.GetString(Res.Sch_ElementName, name.Name));
                    }
                }
            }
            else {
                bool foundAny = false;
                bool first = true;
                StringBuilder subBuilder = new StringBuilder();
                for (int i = 0; i < expected.Count; i++) {
                    name = expected[i] as XmlQualifiedName;
                    if (name.Name == "*") { //rare case where ns of element and that of Any match
                        foundAny = true;
                        continue;
                    }
                    if (first) {
                        first = false;
                    }
                    else {
                        subBuilder.Append(", ");
                    }
                    subBuilder.Append(name.Name);
                }
                if (foundAny) {
                    subBuilder.Append(", ");
                    subBuilder.Append(Res.GetString(Res.Sch_AnyElement));
                }
                else {
                    if (name.Namespace.Length != 0) {
                        builder.Append(Res.GetString(Res.Sch_ElementNameAndNamespace, subBuilder.ToString(), name.Namespace));
                    }
                    else {
                        builder.Append(Res.GetString(Res.Sch_ElementName, subBuilder.ToString()));
                    }
                }
            }
        }

        private static void EnumerateAny(StringBuilder builder, string namespaces) {
            StringBuilder subBuilder = new StringBuilder();
            if (namespaces == "##any" || namespaces == "##other") {
                subBuilder.Append(namespaces);
            }
            else {
                string[] nsList = XmlConvert.SplitString(namespaces);
                Debug.Assert(nsList.Length > 0);
                subBuilder.Append(nsList[0]);
                for (int i = 1; i < nsList.Length; i++) {
                    subBuilder.Append(", ");
                    subBuilder.Append(nsList[i]);
                }
            }
            builder.Append(Res.GetString(Res.Sch_AnyElementNS, subBuilder.ToString()));
        }

        internal static string QNameString(string localName, string ns) {
            return (ns.Length != 0) ? string.Concat(ns, ":", localName) : localName;
        }

        internal static string BuildElementName(XmlQualifiedName qname) {
            return BuildElementName(qname.Name, qname.Namespace);
        }

        internal static string BuildElementName(string localName, string ns) {
            if (ns.Length != 0) {
                return Res.GetString(Res.Sch_ElementNameAndNamespace, localName, ns);
            }
            else {
                return Res.GetString(Res.Sch_ElementName, localName);
            }
        }

        private void ProcessEntity(string name) {
            if (!this.checkEntity) {
                return;
            }
            IDtdEntityInfo entityInfo = null;
            if (dtdSchemaInfo != null) {
                entityInfo = dtdSchemaInfo.LookupEntity(name);
            }
            if (entityInfo == null) {
                // validation error, see xml spec [68]
                SendValidationEvent(Res.Sch_UndeclaredEntity, name);
            }
            else if (entityInfo.IsUnparsedEntity) {
                // validation error, see xml spec [68]
                SendValidationEvent(Res.Sch_UnparsedEntityRef, name);
            }
        }

        private void SendValidationEvent(string code) {
            SendValidationEvent(code, string.Empty);
        }

        private void SendValidationEvent(string code, string[] args) {
            SendValidationEvent(new XmlSchemaValidationException(code, args, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string arg) {
            SendValidationEvent(new XmlSchemaValidationException(code, arg, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string arg1, string arg2) {
            SendValidationEvent(new XmlSchemaValidationException(code, new string[] { arg1, arg2 }, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string[] args, Exception innerException, XmlSeverityType severity) {
            if (severity != XmlSeverityType.Warning || ReportValidationWarnings) {
                SendValidationEvent(new XmlSchemaValidationException(code, args, innerException, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition), severity);
            }
        }

        private void SendValidationEvent(string code, string[] args, Exception innerException) {
            SendValidationEvent(new XmlSchemaValidationException(code, args, innerException, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition), XmlSeverityType.Error);
        }

        private void SendValidationEvent(XmlSchemaValidationException e) {
            SendValidationEvent(e, XmlSeverityType.Error);
        }

        private void SendValidationEvent(XmlSchemaException e) {
            SendValidationEvent(new XmlSchemaValidationException(e.GetRes,e.Args,e.SourceUri,e.LineNumber,e.LinePosition), XmlSeverityType.Error);
        }

        private void SendValidationEvent(string code, string msg, XmlSeverityType severity) {
            if (severity != XmlSeverityType.Warning || ReportValidationWarnings) {
                SendValidationEvent(new XmlSchemaValidationException(code, msg, sourceUriString, positionInfo.LineNumber, positionInfo.LinePosition), severity);
            }
        }

        private void SendValidationEvent(XmlSchemaValidationException e, XmlSeverityType severity) {
            bool errorSeverity = false;
            if (severity == XmlSeverityType.Error) {
                errorSeverity = true;
                context.Validity = XmlSchemaValidity.Invalid;
            }
            if (errorSeverity) {
                if (eventHandler != null) {
                    eventHandler(validationEventSender, new ValidationEventArgs(e, severity));
                }
                else {
                    throw e;
                }
            }
            else if (ReportValidationWarnings && eventHandler != null) {
                eventHandler(validationEventSender, new ValidationEventArgs(e, severity));
            }
        }

        internal static void SendValidationEvent(ValidationEventHandler eventHandler, object sender, XmlSchemaValidationException e, XmlSeverityType severity) {
            if (eventHandler != null) {
                eventHandler(sender, new ValidationEventArgs(e, severity));
            }
            else if (severity == XmlSeverityType.Error) {
                throw e;
            }
        }
     } //End of class

} //End of namespace
