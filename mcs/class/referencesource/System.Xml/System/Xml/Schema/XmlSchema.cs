//------------------------------------------------------------------------------
// <copyright file="XmlSchema.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {
#if SILVERLIGHT
    public class XmlSchema : XmlSchemaObject
    {
        //Empty XmlSchema class to enable backward compatibility of interface method IXmlSerializable.GetSchema()        
        //Add private ctor to prevent constructing of this class
        XmlSchema() { }
    }
#else
    using System.IO;
    using System.Collections;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using System.Threading;
    using System.Diagnostics;

    /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlRoot("schema", Namespace=XmlSchema.Namespace)]
    public class XmlSchema : XmlSchemaObject {

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string Namespace = XmlReservedNs.NsXs;
        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.InstanceNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string InstanceNamespace = XmlReservedNs.NsXsi;

        XmlSchemaForm attributeFormDefault = XmlSchemaForm.None;
        XmlSchemaForm elementFormDefault = XmlSchemaForm.None;
        XmlSchemaDerivationMethod blockDefault = XmlSchemaDerivationMethod.None;
        XmlSchemaDerivationMethod finalDefault = XmlSchemaDerivationMethod.None;
        string targetNs;
        string version;
        XmlSchemaObjectCollection includes = new XmlSchemaObjectCollection();
        XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();
        string id;
        XmlAttribute[] moreAttributes;

        // compiled info
        bool isCompiled = false;
        bool isCompiledBySet = false;
        bool isPreprocessed = false;
        bool isRedefined = false;
        int errorCount = 0;
        XmlSchemaObjectTable attributes;
        XmlSchemaObjectTable attributeGroups = new XmlSchemaObjectTable();
        XmlSchemaObjectTable elements = new XmlSchemaObjectTable();
        XmlSchemaObjectTable types = new XmlSchemaObjectTable();
        XmlSchemaObjectTable groups = new XmlSchemaObjectTable();
        XmlSchemaObjectTable notations = new XmlSchemaObjectTable();
        XmlSchemaObjectTable identityConstraints = new XmlSchemaObjectTable();
        
        static int globalIdCounter = -1;
        ArrayList importedSchemas;
        ArrayList importedNamespaces;
        
        int schemaId = -1; //Not added to a set
        Uri baseUri;
        bool    isChameleon;
        Hashtable ids = new Hashtable();
        XmlDocument document;
        XmlNameTable nameTable;

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.XmlSchema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchema() {}

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchema Read(TextReader reader, ValidationEventHandler validationEventHandler) {
            return Read(new XmlTextReader(reader), validationEventHandler);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Read1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchema Read(Stream stream, ValidationEventHandler validationEventHandler) {
            return Read(new XmlTextReader(stream), validationEventHandler);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Read2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchema Read(XmlReader reader, ValidationEventHandler validationEventHandler) {
            XmlNameTable nameTable = reader.NameTable;
            Parser parser = new Parser(SchemaType.XSD, nameTable, new SchemaNames(nameTable), validationEventHandler);
            try {
                parser.Parse(reader, null);
            }
            catch(XmlSchemaException e) {
                if (validationEventHandler != null) {
                    validationEventHandler(null, new ValidationEventArgs(e));
                } 
                else {
                    throw e;
                }
                return null;
            }
            return parser.XmlSchema;
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Write"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(Stream stream) {
            Write(stream, null);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Write1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(Stream stream, XmlNamespaceManager namespaceManager) {
            XmlTextWriter xmlWriter = new XmlTextWriter(stream, null);
            xmlWriter.Formatting = Formatting.Indented;
            Write(xmlWriter, namespaceManager);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Write2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(TextWriter writer) {
            Write(writer, null);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Write3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(TextWriter writer, XmlNamespaceManager namespaceManager) {
            XmlTextWriter xmlWriter = new XmlTextWriter(writer);
            xmlWriter.Formatting = Formatting.Indented;
            Write(xmlWriter, namespaceManager);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Write4"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(XmlWriter writer) {
            Write(writer, null);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Write5"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(XmlWriter writer, XmlNamespaceManager namespaceManager) {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlSchema));
            XmlSerializerNamespaces ns;
            
            if (namespaceManager != null) {
                ns = new XmlSerializerNamespaces();
                bool ignoreXS = false;
                if (this.Namespaces != null) { //User may have set both nsManager and Namespaces property on the XmlSchema object
                    ignoreXS = this.Namespaces.Namespaces["xs"] != null || this.Namespaces.Namespaces.ContainsValue(XmlReservedNs.NsXs);

                }
                if (!ignoreXS && namespaceManager.LookupPrefix(XmlReservedNs.NsXs) == null && 
                    namespaceManager.LookupNamespace("xs") == null ) {
                        ns.Add("xs", XmlReservedNs.NsXs);
                }
                foreach(string prefix in namespaceManager) {
                    if (prefix != "xml" && prefix != "xmlns") {
                        ns.Add(prefix, namespaceManager.LookupNamespace(prefix));
                    }
                }

            } else if (this.Namespaces != null && this.Namespaces.Count > 0) {
                Hashtable serializerNS = this.Namespaces.Namespaces;
                if (serializerNS["xs"] == null && !serializerNS.ContainsValue(XmlReservedNs.NsXs)) { //Prefix xs not defined AND schema namespace not already mapped to a prefix
                    serializerNS.Add("xs", XmlReservedNs.NsXs);
                }
                ns = this.Namespaces;
            }
            else {
                ns = new XmlSerializerNamespaces();
                ns.Add("xs", XmlSchema.Namespace);
                if (targetNs != null && targetNs.Length != 0) {
                    ns.Add("tns", targetNs);
                }
            }
            serializer.Serialize(writer, this, ns);
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Compile"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Obsolete("Use System.Xml.Schema.XmlSchemaSet for schema compilation and validation. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void Compile(ValidationEventHandler validationEventHandler) {
            SchemaInfo sInfo = new SchemaInfo();
            sInfo.SchemaType = SchemaType.XSD;
            CompileSchema(null, System.Xml.XmlConfiguration.XmlReaderSection.CreateDefaultResolver(), sInfo, null, validationEventHandler, NameTable, false);
        }

       /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Compileq"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Obsolete("Use System.Xml.Schema.XmlSchemaSet for schema compilation and validation. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void Compile(ValidationEventHandler validationEventHandler, XmlResolver resolver) {
            SchemaInfo sInfo = new SchemaInfo();
            sInfo.SchemaType = SchemaType.XSD;
            CompileSchema(null, resolver, sInfo, null, validationEventHandler, NameTable, false);
        }

#pragma warning disable 618
        internal bool CompileSchema(XmlSchemaCollection xsc, XmlResolver resolver, SchemaInfo schemaInfo, string ns, ValidationEventHandler validationEventHandler, XmlNameTable nameTable, bool CompileContentModel) {

            //Need to lock here to prevent multi-threading problems when same schema is added to set and compiled
            lock (this) {
                //Preprocessing
                SchemaCollectionPreprocessor prep = new SchemaCollectionPreprocessor(nameTable, null, validationEventHandler);
                prep.XmlResolver = resolver;
                if (!prep.Execute(this, ns, true, xsc)) {
                    return false;
                }
            
                //Compilation
                SchemaCollectionCompiler compiler = new SchemaCollectionCompiler(nameTable, validationEventHandler);
                isCompiled = compiler.Execute(this, schemaInfo, CompileContentModel);
                this.SetIsCompiled(isCompiled);
                //
                return isCompiled;
            }
        }
#pragma warning restore 618

        internal void CompileSchemaInSet(XmlNameTable nameTable, ValidationEventHandler eventHandler, XmlSchemaCompilationSettings compilationSettings) {
            Debug.Assert(this.isPreprocessed);
            Compiler setCompiler = new Compiler(nameTable, eventHandler, null, compilationSettings);
            setCompiler.Prepare(this, true);
            this.isCompiledBySet = setCompiler.Compile();
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.AttributeFormDefault"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("attributeFormDefault"), DefaultValue(XmlSchemaForm.None)]
        public XmlSchemaForm AttributeFormDefault {
             get { return attributeFormDefault; }
             set { attributeFormDefault = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.BlockDefault"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("blockDefault"), DefaultValue(XmlSchemaDerivationMethod.None)]
        public XmlSchemaDerivationMethod BlockDefault {
             get { return blockDefault; }
             set { blockDefault = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.FinalDefault"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("finalDefault"), DefaultValue(XmlSchemaDerivationMethod.None)]
        public XmlSchemaDerivationMethod FinalDefault {
             get { return finalDefault; }
             set { finalDefault = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.ElementFormDefault"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("elementFormDefault"), DefaultValue(XmlSchemaForm.None)]
        public XmlSchemaForm ElementFormDefault {
             get { return elementFormDefault; }
             set { elementFormDefault = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.TargetNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("targetNamespace", DataType="anyURI")]
        public string TargetNamespace {
             get { return targetNs; }
             set { targetNs = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Version"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("version", DataType="token")]
        public string Version {
             get { return version; }
             set { version = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Includes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("include", typeof(XmlSchemaInclude)),
         XmlElement("import", typeof(XmlSchemaImport)),
         XmlElement("redefine", typeof(XmlSchemaRedefine))]
        public XmlSchemaObjectCollection Includes {
            get { return includes; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Items"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("annotation", typeof(XmlSchemaAnnotation)),
         XmlElement("attribute", typeof(XmlSchemaAttribute)),
         XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroup)),
         XmlElement("complexType", typeof(XmlSchemaComplexType)),
         XmlElement("simpleType", typeof(XmlSchemaSimpleType)),
         XmlElement("element", typeof(XmlSchemaElement)),
         XmlElement("group", typeof(XmlSchemaGroup)),
         XmlElement("notation", typeof(XmlSchemaNotation))]
        public XmlSchemaObjectCollection Items {
            get { return items; }
        }

        // Compiled info
        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.IsCompiled"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public bool IsCompiled {
            get { 
                return isCompiled || isCompiledBySet ; 
            }
        }
        
        [XmlIgnore]
        internal bool IsCompiledBySet {
            get { return isCompiledBySet; }
            set { isCompiledBySet = value; }
        }

        [XmlIgnore]
        internal bool IsPreprocessed {
            get { return isPreprocessed; }
            set { isPreprocessed = value; }
        }
        
        [XmlIgnore]
        internal bool IsRedefined {
            get { return isRedefined; }
            set { isRedefined = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Attributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable Attributes {
            get {
                if (attributes == null) {
                    attributes = new XmlSchemaObjectTable();
                }
                return attributes;
            }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.AttributeGroups"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable AttributeGroups {
            get {
                if (attributeGroups == null) {
                    attributeGroups = new XmlSchemaObjectTable();
                }
                return attributeGroups;
            }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.SchemaTypes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable SchemaTypes {
            get {
                if (types == null) {
                    types = new XmlSchemaObjectTable();
                }
                return types;                
            }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Elements"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable Elements {
            get {
                if (elements == null) {
                    elements = new XmlSchemaObjectTable();
                }
                return elements;                
            }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Id"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("id", DataType="ID")]
        public string Id {
            get { return id; }
            set { id = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.UnhandledAttributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAnyAttribute]
        public XmlAttribute[] UnhandledAttributes {
            get { return moreAttributes; }
            set { moreAttributes = value; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Groups"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable Groups {
            get { return groups; }
        }

        /// <include file='doc\XmlSchema.uex' path='docs/doc[@for="XmlSchema.Notations"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable Notations {
            get { return notations; }
        }

        [XmlIgnore]
        internal XmlSchemaObjectTable IdentityConstraints { 
            get { return identityConstraints; }
        }

        [XmlIgnore]
        internal Uri BaseUri {
            get { return baseUri; }
            set { 
                baseUri = value; 
            }
        }
        
        [XmlIgnore]
        // Please be careful with this property. Since it lazy initialized and its value depends on a global state
        //   if it gets called on multiple schemas in a different order the schemas will end up with different IDs
        //   Unfortunately the IDs are used to sort the schemas in the schema set and thus changing the IDs might change
        //   the order which would be a breaking change!!
        // Simply put if you are planning to add or remove a call to this getter you need to be extra carefull
        //   or better don't do it at all.
        internal int SchemaId {
            get { 
                if (schemaId == -1) {
                    schemaId = Interlocked.Increment(ref globalIdCounter);
                }
                return schemaId;
            }
        }
        
        [XmlIgnore]
        internal bool IsChameleon {
            get { return isChameleon; }
            set { isChameleon = value; }
        }

        [XmlIgnore]
        internal Hashtable Ids {
            get { return ids; }
        }

        [XmlIgnore]
        internal XmlDocument Document {
            get { if (document == null) document = new XmlDocument(); return document; }
        }

        [XmlIgnore]
        internal int ErrorCount {
            get { return errorCount; }
            set { errorCount = value; }
        }
        
        internal new XmlSchema Clone() {
            XmlSchema that = new XmlSchema();
            that.attributeFormDefault   = this.attributeFormDefault;
            that.elementFormDefault     = this.elementFormDefault;
            that.blockDefault           = this.blockDefault;
            that.finalDefault           = this.finalDefault;
            that.targetNs               = this.targetNs;
            that.version                = this.version;
            that.includes               = this.includes;

            that.Namespaces             = this.Namespaces;
            that.items                  = this.items;   
            that.BaseUri                = this.BaseUri;

            SchemaCollectionCompiler.Cleanup(that);
            return that;
        }
        
        internal XmlSchema DeepClone() {
            XmlSchema that = new XmlSchema();
            that.attributeFormDefault   = this.attributeFormDefault;
            that.elementFormDefault     = this.elementFormDefault;
            that.blockDefault           = this.blockDefault;
            that.finalDefault           = this.finalDefault;
            that.targetNs               = this.targetNs;
            that.version                = this.version;
            that.isPreprocessed         = this.isPreprocessed;
            //that.IsProcessing           = this.IsProcessing; //Not sure if this is needed

            //Clone its Items
            for (int i = 0; i < this.items.Count; ++i) {
                XmlSchemaObject newItem;

                XmlSchemaComplexType complexType;
                XmlSchemaElement element;
                XmlSchemaGroup group;

                if ((complexType = items[i] as XmlSchemaComplexType) != null) {
                    newItem = complexType.Clone(this);
                }
                else if ((element = items[i] as XmlSchemaElement) != null) {
                    newItem = element.Clone(this);
                }
                else if ((group = items[i] as XmlSchemaGroup) != null) {
                    newItem = group.Clone(this);
                }
                else {
                    newItem = items[i].Clone();
                }
                that.Items.Add(newItem);
            }
            
            //Clone Includes
            for (int i = 0; i < this.includes.Count; ++i) {
                XmlSchemaExternal newInclude = (XmlSchemaExternal)this.includes[i].Clone();
                that.Includes.Add(newInclude);
            }
            that.Namespaces             = this.Namespaces;
            //that.includes               = this.includes; //Need to verify this is OK for redefines
            that.BaseUri                = this.BaseUri;
            return that;
        }

        [XmlIgnore]
        internal override string IdAttribute {
            get { return Id; }
            set { Id = value; }
        }

        internal void SetIsCompiled(bool isCompiled) {
            this.isCompiled = isCompiled;
        }

        internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes) {
            this.moreAttributes = moreAttributes;
        }
        internal override void AddAnnotation(XmlSchemaAnnotation annotation) {
            items.Add(annotation);
        }
        
        internal XmlNameTable NameTable {
            get { if (nameTable == null) nameTable = new System.Xml.NameTable(); return nameTable; }
        }
        
        internal ArrayList ImportedSchemas {
            get {
                if (importedSchemas == null) {
                    importedSchemas = new ArrayList();
                }
                return importedSchemas;
            }
        }
        
        internal ArrayList ImportedNamespaces {
            get {
                if (importedNamespaces == null) {
                    importedNamespaces = new ArrayList();
                }
                return importedNamespaces;
            }
        }

        internal void GetExternalSchemasList(IList extList, XmlSchema schema) {
            Debug.Assert(extList != null && schema != null);
            if (extList.Contains(schema)) {
                return;
            }
            extList.Add(schema);
            for (int i = 0; i < schema.Includes.Count; ++i) {
                XmlSchemaExternal ext = (XmlSchemaExternal)schema.Includes[i];
                if (ext.Schema != null) {
                    GetExternalSchemasList(extList, ext.Schema);
                }
            }
        }

#if TRUST_COMPILE_STATE
        internal void AddCompiledInfo(SchemaInfo schemaInfo) {
            XmlQualifiedName itemName;
            foreach (XmlSchemaElement element in elements.Values) {
                itemName = element.QualifiedName;
                schemaInfo.TargetNamespaces[itemName.Namespace] = true;
                if (schemaInfo.ElementDecls[itemName] == null) {
                    schemaInfo.ElementDecls.Add(itemName, element.ElementDecl);
                }
            }
            foreach (XmlSchemaAttribute attribute in attributes.Values) {
                itemName = attribute.QualifiedName;
                schemaInfo.TargetNamespaces[itemName.Namespace] = true;
                if (schemaInfo.ElementDecls[itemName] == null) {
                    schemaInfo.AttributeDecls.Add(itemName, attribute.AttDef);
                }
            }    
            foreach (XmlSchemaType type in types.Values) {
                itemName = type.QualifiedName;
                schemaInfo.TargetNamespaces[itemName.Namespace] = true;
                XmlSchemaComplexType complexType = type as XmlSchemaComplexType;
                if ((complexType == null || type != XmlSchemaComplexType.AnyType) && schemaInfo.ElementDeclsByType[itemName] == null) {
                    schemaInfo.ElementDeclsByType.Add(itemName, type.ElementDecl);
                }
            }
            foreach (XmlSchemaNotation notation in notations.Values) {
                itemName = notation.QualifiedName;
                schemaInfo.TargetNamespaces[itemName.Namespace] = true;
                SchemaNotation no = new SchemaNotation(itemName);
                no.SystemLiteral = notation.System;
                no.Pubid = notation.Public;
                if (schemaInfo.Notations[itemName.Name] == null) {
                    schemaInfo.Notations.Add(itemName.Name, no);
                }
            }
        }
#endif//TRUST_COMPILE_STATE
    }

#endif//!SILVERLIGHT
}
