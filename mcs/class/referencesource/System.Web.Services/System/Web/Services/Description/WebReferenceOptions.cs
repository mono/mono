//------------------------------------------------------------------------------
// <copyright file="WebReferenceOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {
    using System;
    using System.Globalization;
    using System.IO;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Specialized;

    /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlType("webReferenceOptions", Namespace = WebReferenceOptions.TargetNamespace)]
    [XmlRoot("webReferenceOptions", Namespace = WebReferenceOptions.TargetNamespace)]
    public class WebReferenceOptions {
        public const string TargetNamespace = "http://microsoft.com/webReference/";
        static XmlSchema schema = null;
        CodeGenerationOptions codeGenerationOptions = CodeGenerationOptions.GenerateOldAsync;
        ServiceDescriptionImportStyle style = ServiceDescriptionImportStyle.Client;
        StringCollection schemaImporterExtensions;
        bool verbose;

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.CodeGenerationOptions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("codeGenerationOptions")]
        [DefaultValue(CodeGenerationOptions.GenerateOldAsync)]
        public CodeGenerationOptions CodeGenerationOptions {
            get {
                return codeGenerationOptions;
            }
            set {
                codeGenerationOptions = value;
            }
        }

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.SchemaImporterExtensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlArray("schemaImporterExtensions")]
        [XmlArrayItem("type")]
        public StringCollection SchemaImporterExtensions {
            get {
                if (schemaImporterExtensions == null)
                    schemaImporterExtensions = new StringCollection();
                return schemaImporterExtensions;
            }
        }

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.Style"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [DefaultValue(ServiceDescriptionImportStyle.Client)]
        [XmlElement("style")]
        public ServiceDescriptionImportStyle Style {
            get {
                return style;
            }
            set {
                style = value;
            }
        }

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.Verbose"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("verbose")]
        public bool Verbose {
            get {
                return verbose;
            }
            set {
                verbose = value;
            }
        }

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.Schema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchema Schema {
            get {
                if (schema == null) {
                    using (XmlTextReader reader = new XmlTextReader(new StringReader(Schemas.WebRef)))
                    {
                        reader.DtdProcessing = DtdProcessing.Ignore;
                        schema = XmlSchema.Read(reader, null);
                    }
                }
                return schema;
            }
        }
        
        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static WebReferenceOptions Read(TextReader reader, ValidationEventHandler validationEventHandler) {
            XmlTextReader readerNew = new XmlTextReader(reader);
            readerNew.XmlResolver = null;
            readerNew.DtdProcessing = DtdProcessing.Prohibit;
            return Read(readerNew, validationEventHandler);
        }

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="WebReferenceOptions.Read1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static WebReferenceOptions Read(Stream stream, ValidationEventHandler validationEventHandler) {
            XmlTextReader readerNew = new XmlTextReader(stream);
            readerNew.XmlResolver = null;
            readerNew.DtdProcessing = DtdProcessing.Prohibit;
            return Read(readerNew, validationEventHandler);
        }

        /// <include file='doc\WebReferenceOptions.uex' path='docs/doc[@for="XmlSchema.Read2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static WebReferenceOptions Read(XmlReader xmlReader, ValidationEventHandler validationEventHandler) {
            XmlValidatingReader validatingReader = new XmlValidatingReader(xmlReader);
            validatingReader.ValidationType = ValidationType.Schema;
            if (validationEventHandler != null) {
                validatingReader.ValidationEventHandler += validationEventHandler;
            }
            else {
                validatingReader.ValidationEventHandler += new ValidationEventHandler(SchemaValidationHandler);
            }
            validatingReader.Schemas.Add(Schema);
            webReferenceOptionsSerializer ser = new webReferenceOptionsSerializer();
            try {
                return (WebReferenceOptions)ser.Deserialize(validatingReader);
            }
            catch (Exception e) {
                throw e;
            }
            finally {
                validatingReader.Close();
            }
        }

        private static void SchemaValidationHandler(object sender, ValidationEventArgs args) {
            if (args.Severity != XmlSeverityType.Error)
                return;
            throw new InvalidOperationException(Res.GetString(Res.WsdlInstanceValidationDetails, args.Message, args.Exception.LineNumber.ToString(CultureInfo.InvariantCulture), args.Exception.LinePosition.ToString(CultureInfo.InvariantCulture)));
        }
    }

    internal class WebReferenceOptionsSerializationWriter : XmlSerializationWriter {
        string Write1_CodeGenerationOptions(System.Xml.Serialization.CodeGenerationOptions v) {
            string s = null;
            switch (v) {
                case System.Xml.Serialization.CodeGenerationOptions.@GenerateProperties: s = @"properties"; break;
                case System.Xml.Serialization.CodeGenerationOptions.@GenerateNewAsync: s = @"newAsync"; break;
                case System.Xml.Serialization.CodeGenerationOptions.@GenerateOldAsync: s = @"oldAsync"; break;
                case System.Xml.Serialization.CodeGenerationOptions.@GenerateOrder: s = @"order"; break;
                case System.Xml.Serialization.CodeGenerationOptions.@EnableDataBinding: s = @"enableDataBinding"; break;
                default: s = FromEnum(((System.Int64)v), new string[] { @"properties", 
                    @"newAsync", 
                    @"oldAsync", 
                    @"order", 
                    @"enableDataBinding" }, new System.Int64[] { (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateProperties, 
                    (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateNewAsync, 
                    (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateOldAsync, 
                    (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateOrder, 
                    (long)System.Xml.Serialization.CodeGenerationOptions.@EnableDataBinding }, @"System.Xml.Serialization.CodeGenerationOptions"); break;
            }
            return s;
        }

        string Write2_ServiceDescriptionImportStyle(System.Web.Services.Description.ServiceDescriptionImportStyle v) {
            string s = null;
            switch (v) {
                case System.Web.Services.Description.ServiceDescriptionImportStyle.@Client: s = @"client"; break;
                case System.Web.Services.Description.ServiceDescriptionImportStyle.@Server: s = @"server"; break;
                case System.Web.Services.Description.ServiceDescriptionImportStyle.@ServerInterface: s = @"serverInterface"; break;
                default: throw CreateInvalidEnumValueException(((System.Int64)v).ToString(System.Globalization.CultureInfo.InvariantCulture), @"System.Web.Services.Description.ServiceDescriptionImportStyle");
            }
            return s;
        }

        void Write4_WebReferenceOptions(string n, string ns, WebReferenceOptions o, bool isNullable, bool needType) {
            if ((object)o == null) {
                if (isNullable) WriteNullTagLiteral(n, ns);
                return;
            }
            if (!needType) {
                System.Type t = o.GetType();
                if (t == typeof(WebReferenceOptions)) {
                }
                else {
                    throw CreateUnknownTypeException(o);
                }
            }
            EscapeName = false;
            WriteStartElement(n, ns, o);
            if (needType) WriteXsiType(@"webReferenceOptions", @"http://microsoft.com/webReference/");
            if (((CodeGenerationOptions)o.@CodeGenerationOptions) != (CodeGenerationOptions.@GenerateOldAsync)) {
                WriteElementString(@"codeGenerationOptions", @"http://microsoft.com/webReference/", Write1_CodeGenerationOptions(((CodeGenerationOptions)o.@CodeGenerationOptions)));
            } {
                System.Collections.Specialized.StringCollection a = (System.Collections.Specialized.StringCollection)((System.Collections.Specialized.StringCollection)o.@SchemaImporterExtensions);
                if (a != null) {
                    WriteStartElement(@"schemaImporterExtensions", @"http://microsoft.com/webReference/");
                    for (int ia = 0; ia < a.Count; ia++) {
                        WriteNullableStringLiteral(@"type", @"http://microsoft.com/webReference/", ((System.String)a[ia]));
                    }
                    WriteEndElement();
                }
            }
            if (((System.Web.Services.Description.ServiceDescriptionImportStyle)o.@Style) != System.Web.Services.Description.ServiceDescriptionImportStyle.@Client) {
                WriteElementString(@"style", @"http://microsoft.com/webReference/", Write2_ServiceDescriptionImportStyle(((System.Web.Services.Description.ServiceDescriptionImportStyle)o.@Style)));
            }
            WriteElementStringRaw(@"verbose", @"http://microsoft.com/webReference/", System.Xml.XmlConvert.ToString((System.Boolean)((System.Boolean)o.@Verbose)));
            WriteEndElement(o);
        }

        protected override void InitCallbacks() {
        }

        internal void Write5_webReferenceOptions(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"webReferenceOptions", @"http://microsoft.com/webReference/");
                return;
            }
            TopLevelElement();
            Write4_WebReferenceOptions(@"webReferenceOptions", @"http://microsoft.com/webReference/", ((System.Web.Services.Description.WebReferenceOptions)o), true, false);
        }
    }

    internal class WebReferenceOptionsSerializationReader : XmlSerializationReader {
        System.Collections.Hashtable _CodeGenerationOptionsValues;

        internal System.Collections.Hashtable CodeGenerationOptionsValues {
            get {
                if ((object)_CodeGenerationOptionsValues == null) {
                    System.Collections.Hashtable h = new System.Collections.Hashtable();
                    h.Add(@"properties", (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateProperties);
                    h.Add(@"newAsync", (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateNewAsync);
                    h.Add(@"oldAsync", (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateOldAsync);
                    h.Add(@"order", (long)System.Xml.Serialization.CodeGenerationOptions.@GenerateOrder);
                    h.Add(@"enableDataBinding", (long)System.Xml.Serialization.CodeGenerationOptions.@EnableDataBinding);
                    _CodeGenerationOptionsValues = h;
                }
                return _CodeGenerationOptionsValues;
            }
        }

        System.Xml.Serialization.CodeGenerationOptions Read1_CodeGenerationOptions(string s) {
            return (System.Xml.Serialization.CodeGenerationOptions)ToEnum(s, CodeGenerationOptionsValues, @"System.Xml.Serialization.CodeGenerationOptions");
        }

        System.Web.Services.Description.ServiceDescriptionImportStyle Read2_ServiceDescriptionImportStyle(string s) {
            switch (s) {
                case @"client": return System.Web.Services.Description.ServiceDescriptionImportStyle.@Client;
                case @"server": return System.Web.Services.Description.ServiceDescriptionImportStyle.@Server;
                case @"serverInterface": return System.Web.Services.Description.ServiceDescriptionImportStyle.@ServerInterface;
                default: throw CreateUnknownConstantException(s, typeof(System.Web.Services.Description.ServiceDescriptionImportStyle));
            }
        }

        System.Web.Services.Description.WebReferenceOptions Read4_WebReferenceOptions(bool isNullable, bool checkType) {
            System.Xml.XmlQualifiedName xsiType = checkType ? GetXsiType() : null;
            bool isNull = false;
            if (isNullable) isNull = ReadNull();
            if (checkType) {
            if (xsiType == null || ((object)((System.Xml.XmlQualifiedName)xsiType).Name == (object)id1_webReferenceOptions && (object) ((System.Xml.XmlQualifiedName)xsiType).Namespace == (object)id2_Item)) {
            }
            else
                throw CreateUnknownTypeException((System.Xml.XmlQualifiedName)xsiType);
            }
            if (isNull) return null;
            System.Web.Services.Description.WebReferenceOptions o;
            o = new System.Web.Services.Description.WebReferenceOptions();
            System.Collections.Specialized.StringCollection a_1 = (System.Collections.Specialized.StringCollection)o.@SchemaImporterExtensions;
            bool[] paramsRead = new bool[4];
            while (Reader.MoveToNextAttribute()) {
                if (!IsXmlnsAttribute(Reader.Name)) {
                    UnknownNode((object)o);
                }
            }
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
                return o;
            }
            Reader.ReadStartElement();
            Reader.MoveToContent();
            int whileIterations0 = 0;
            int readerCount0 = ReaderCount;
            while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                    if (!paramsRead[0] && ((object) Reader.LocalName == (object)id3_codeGenerationOptions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (Reader.IsEmptyElement) {
                            Reader.Skip();
                        }
                        else {
                            o.@CodeGenerationOptions = Read1_CodeGenerationOptions(Reader.ReadElementString());
                        }
                        paramsRead[0] = true;
                    }
                    else if (((object) Reader.LocalName == (object)id4_schemaImporterExtensions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (!ReadNull()) {
                            System.Collections.Specialized.StringCollection a_1_0 = (System.Collections.Specialized.StringCollection)o.@SchemaImporterExtensions;
                            if (((object)(a_1_0) == null) || (Reader.IsEmptyElement)) {
                                Reader.Skip();
                            }
                            else {
                                Reader.ReadStartElement();
                                Reader.MoveToContent();
                                int whileIterations1 = 0;
                                int readerCount1 = ReaderCount;
                                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement && Reader.NodeType != System.Xml.XmlNodeType.None) {
                                    if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                                        if (((object) Reader.LocalName == (object)id5_type && (object) Reader.NamespaceURI == (object)id2_Item)) {
                                            if (ReadNull()) {
                                                a_1_0.Add(null);
                                            }
                                            else {
                                                a_1_0.Add(Reader.ReadElementString());
                                            }
                                        }
                                        else {
                                            UnknownNode(null, @"http://microsoft.com/webReference/:type");
                                        }
                                    }
                                    else {
                                        UnknownNode(null, @"http://microsoft.com/webReference/:type");
                                    }
                                    Reader.MoveToContent();
                                    CheckReaderCount(ref whileIterations1, ref readerCount1);
                                }
                            ReadEndElement();
                            }
                        }
                    }
                    else if (!paramsRead[2] && ((object) Reader.LocalName == (object)id6_style && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        if (Reader.IsEmptyElement) {
                            Reader.Skip();
                        }
                        else {
                            o.@Style = Read2_ServiceDescriptionImportStyle(Reader.ReadElementString());
                        }
                        paramsRead[2] = true;
                    }
                    else if (!paramsRead[3] && ((object) Reader.LocalName == (object)id7_verbose && (object) Reader.NamespaceURI == (object)id2_Item)) {
                        {
                            o.@Verbose = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                        }
                        paramsRead[3] = true;
                    }
                    else {
                        UnknownNode((object)o, @"http://microsoft.com/webReference/:codeGenerationOptions, http://microsoft.com/webReference/:schemaImporterExtensions, http://microsoft.com/webReference/:style, http://microsoft.com/webReference/:verbose");
                    }
                }
                else {
                    UnknownNode((object)o, @"http://microsoft.com/webReference/:codeGenerationOptions, http://microsoft.com/webReference/:schemaImporterExtensions, http://microsoft.com/webReference/:style, http://microsoft.com/webReference/:verbose");
                }
                Reader.MoveToContent();
                CheckReaderCount(ref whileIterations0, ref readerCount0);
            }
            ReadEndElement();
            return o;
        }

        protected override void InitCallbacks() {
        }

        internal object Read5_webReferenceOptions() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_webReferenceOptions && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    o = Read4_WebReferenceOptions(true, true);
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null, @"http://microsoft.com/webReference/:webReferenceOptions");
            }
            return (object)o;
        }

        string id2_Item;
        string id5_type;
        string id4_schemaImporterExtensions;
        string id3_codeGenerationOptions;
        string id6_style;
        string id7_verbose;
        string id1_webReferenceOptions;

        protected override void InitIDs() {
            id2_Item = Reader.NameTable.Add(@"http://microsoft.com/webReference/");
            id5_type = Reader.NameTable.Add(@"type");
            id4_schemaImporterExtensions = Reader.NameTable.Add(@"schemaImporterExtensions");
            id3_codeGenerationOptions = Reader.NameTable.Add(@"codeGenerationOptions");
            id6_style = Reader.NameTable.Add(@"style");
            id7_verbose = Reader.NameTable.Add(@"verbose");
            id1_webReferenceOptions = Reader.NameTable.Add(@"webReferenceOptions");
        }
    }

    internal sealed class webReferenceOptionsSerializer : XmlSerializer {
        protected override XmlSerializationReader CreateReader() {
            return new WebReferenceOptionsSerializationReader();
        }
        protected override XmlSerializationWriter CreateWriter() {
            return new WebReferenceOptionsSerializationWriter();
        }
        public override System.Boolean CanDeserialize(System.Xml.XmlReader xmlReader) {
            return true;
        }

        protected override void Serialize(System.Object objectToSerialize, XmlSerializationWriter writer) {
            ((WebReferenceOptionsSerializationWriter)writer).Write5_webReferenceOptions(objectToSerialize);
        }
        protected override System.Object Deserialize(XmlSerializationReader reader) {
            return ((WebReferenceOptionsSerializationReader)reader).Read5_webReferenceOptions();
        }
    }
}
