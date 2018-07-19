//------------------------------------------------------------------------------
// <copyright file="ServiceDescriptionImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System;
    using System.Reflection;
    using System.CodeDom.Compiler;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.CodeDom;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Flags]
    public enum ServiceDescriptionImportWarnings {
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.NoCodeGenerated"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        NoCodeGenerated = 0x1,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.OptionalExtensionsIgnored"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        OptionalExtensionsIgnored = 0x2,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.RequiredExtensionsIgnored"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        RequiredExtensionsIgnored = 0x4,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        UnsupportedOperationsIgnored = 0x8,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        UnsupportedBindingsIgnored = 0x10,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.NoMethodsGenerated"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        NoMethodsGenerated = 0x20,

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.SchemaValidation"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        SchemaValidation = 0x40,

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportWarnings.WSIConformance"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        WsiConformance = 0x80,
    }

    /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportStyle"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public enum ServiceDescriptionImportStyle {
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportStyle.Client"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlEnum("client")]
        Client,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportStyle.Server"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlEnum("server")]
        Server,
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImportStyle.ServerInterface"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlEnum("serverInterface")]
        ServerInterface,
    }

    /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public class ServiceDescriptionImporter {
        ServiceDescriptionImportStyle style = ServiceDescriptionImportStyle.Client;
        ServiceDescriptionCollection serviceDescriptions = new ServiceDescriptionCollection();
        XmlSchemas schemas = new XmlSchemas(); // those external to SDLs
        XmlSchemas allSchemas = new XmlSchemas(); // all schemas, incl. those inside SDLs
        string protocolName;
        CodeGenerationOptions options = CodeGenerationOptions.GenerateOldAsync;
        CodeCompileUnit codeCompileUnit;
        CodeDomProvider codeProvider;
        ProtocolImporter[] importers;
        XmlSchemas abstractSchemas = new XmlSchemas(); // all schemas containing abstract types
        XmlSchemas concreteSchemas = new XmlSchemas(); // all "real" xml schemas 
        List<Type> extensions;

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.ServiceDescriptionImporter"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionImporter() {
            Type[] importerTypes = WebServicesSection.Current.ProtocolImporterTypes;
            importers = new ProtocolImporter[importerTypes.Length];
            for (int i = 0; i < importers.Length; i++) {
                importers[i] = (ProtocolImporter)Activator.CreateInstance(importerTypes[i]);
                importers[i].Initialize(this);
            }
        }

        internal ServiceDescriptionImporter(CodeCompileUnit codeCompileUnit) : this() {
            this.codeCompileUnit = codeCompileUnit;
        }
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.ServiceDescriptions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionCollection ServiceDescriptions {
            get { return serviceDescriptions; }
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.Schemas"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemas Schemas {
            get { return schemas; }
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.Style"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionImportStyle Style {
            get { return style; }
            set { style = value; }
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.CodeGenerationOptions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public CodeGenerationOptions CodeGenerationOptions {
            get { return options; }
            set { options = value; }
        }

        internal CodeCompileUnit CodeCompileUnit {
            get { return codeCompileUnit; }
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.CodeGenerator"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public CodeDomProvider CodeGenerator {
            get {
                if (codeProvider == null)
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                return codeProvider; 
            }
            set { codeProvider = value; }
        }

        internal List<Type> Extensions {
            get {
                if (extensions == null) {
                    extensions = new List<Type>();
                }
                return extensions;
            }
        }
        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.ProtocolName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public String ProtocolName {
            get { return protocolName == null ? string.Empty : protocolName; }
            set { protocolName = value; }
        }

        ProtocolImporter FindImporterByName(string protocolName) {
            for (int i = 0; i < importers.Length; i++) {
                ProtocolImporter importer = importers[i];
                if (string.Compare(ProtocolName, importer.ProtocolName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return importer;
                }
            }
            throw new ArgumentException(Res.GetString(Res.ProtocolWithNameIsNotRecognized1, protocolName), "protocolName");
        }

        internal XmlSchemas AllSchemas {
            get { return allSchemas; }
        }

        internal XmlSchemas AbstractSchemas {
            get { return abstractSchemas; }
        }

        internal XmlSchemas ConcreteSchemas {
            get { return concreteSchemas; }
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.AddServiceDescription"]/*' />
        public void AddServiceDescription(ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl) {
            if (serviceDescription == null)
                throw new ArgumentNullException("serviceDescription");

            serviceDescription.AppSettingUrlKey = appSettingUrlKey;
            serviceDescription.AppSettingBaseUrl = appSettingBaseUrl;
            ServiceDescriptions.Add(serviceDescription);
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.Import"]/*' />
        public ServiceDescriptionImportWarnings Import(CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit) {
            if (codeCompileUnit != null) {
                codeCompileUnit.ReferencedAssemblies.Add("System.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Xml.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Web.Services.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.EnterpriseServices.dll");
            }
            return Import(codeNamespace, new ImportContext(new CodeIdentifiers(), false), new Hashtable(), new StringCollection());
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="ServiceDescriptionImporter.GenerateWebReferences1"]/*' />
        public static StringCollection GenerateWebReferences(WebReferenceCollection webReferences, CodeDomProvider codeProvider, CodeCompileUnit codeCompileUnit, WebReferenceOptions options) {
            if (codeCompileUnit != null) {
                codeCompileUnit.ReferencedAssemblies.Add("System.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Xml.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Web.Services.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.EnterpriseServices.dll");
            }
            Hashtable namespaces = new Hashtable();
            Hashtable exportedMappings = new Hashtable();
            foreach (WebReference webReference in webReferences) {
                ServiceDescriptionImporter importer = new ServiceDescriptionImporter(codeCompileUnit);

                // separate descriptions and schemas
                XmlSchemas schemas = new XmlSchemas();
                ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();

                foreach (DictionaryEntry entry in webReference.Documents) {
                    AddDocument((string)entry.Key, entry.Value, schemas, descriptions, webReference.ValidationWarnings);
                }

                importer.Schemas.Add(schemas);
                foreach (ServiceDescription source in descriptions)
                    importer.AddServiceDescription(source, webReference.AppSettingUrlKey, webReference.AppSettingBaseUrl);
                importer.CodeGenerator = codeProvider;
                importer.ProtocolName = webReference.ProtocolName;
                importer.Style = options.Style;
                importer.CodeGenerationOptions = options.CodeGenerationOptions;
                foreach (string extensionType in options.SchemaImporterExtensions) {
                    importer.Extensions.Add(Type.GetType(extensionType, true /*throwOnError*/));
                }
                ImportContext context = Context(webReference.ProxyCode, namespaces, options.Verbose);

                webReference.Warnings = importer.Import(webReference.ProxyCode, context, exportedMappings, webReference.ValidationWarnings);
                if (webReference.ValidationWarnings.Count != 0) {
                    webReference.Warnings |= ServiceDescriptionImportWarnings.SchemaValidation;
                }
            }

            StringCollection shareWarnings = new StringCollection();

            if (options.Verbose) {
                foreach (ImportContext context in namespaces.Values) {
                    foreach (string warning in context.Warnings) {
                        shareWarnings.Add(warning);
                    }
                }
            }
            return shareWarnings;
        }

        internal static ImportContext Context(CodeNamespace ns, Hashtable namespaces, bool verbose) {
            if (namespaces[ns.Name] == null) {
                namespaces[ns.Name] = new ImportContext(new CodeIdentifiers(), true);
            }
            return (ImportContext)namespaces[ns.Name];
        }

        internal static void AddDocument(string path, object document, XmlSchemas schemas, ServiceDescriptionCollection descriptions, StringCollection warnings) {
            ServiceDescription serviceDescription = document as ServiceDescription;
            if (serviceDescription != null) {
                descriptions.Add(serviceDescription);
            }
            else {
                XmlSchema schema = document as XmlSchema;
                if (schema != null) {
                    schemas.Add(schema);
                }
            }
        }

        private void FindUse(MessagePart part, out bool isEncoded, out bool isLiteral) {
            isEncoded = false;
            isLiteral = false;
            string messageName = part.Message.Name;
            Operation associatedOperation = null;
            ServiceDescription description = part.Message.ServiceDescription;
            foreach (PortType portType in description.PortTypes) {
                foreach (Operation operation in portType.Operations) {
                    foreach (OperationMessage message in operation.Messages) {
                        if (message.Message.Equals(new XmlQualifiedName(part.Message.Name, description.TargetNamespace))) {
                            associatedOperation = operation;
                            FindUse(associatedOperation, description, messageName, ref isEncoded, ref isLiteral);
                        }
                    }
                }
            }
            if (associatedOperation == null)
                FindUse(null, description, messageName, ref isEncoded, ref isLiteral);
        }

        private void FindUse(Operation operation, ServiceDescription description, string messageName, ref bool isEncoded, ref bool isLiteral) {
            string targetNamespace = description.TargetNamespace;
            foreach (Binding binding in description.Bindings) {
                if (operation != null && !new XmlQualifiedName(operation.PortType.Name, targetNamespace).Equals(binding.Type))
                    continue;
                foreach (OperationBinding bindingOperation in binding.Operations) {
                    if (bindingOperation.Input != null) foreach (object extension in bindingOperation.Input.Extensions) {
                        if (operation != null) {
                            SoapBodyBinding body = extension as SoapBodyBinding;
                            if (body != null && operation.IsBoundBy(bindingOperation)) {
                                if (body.Use == SoapBindingUse.Encoded)
                                    isEncoded = true;
                                else if (body.Use == SoapBindingUse.Literal)
                                    isLiteral = true;
                            }
                        }
                        else {
                            SoapHeaderBinding header = extension as SoapHeaderBinding;
                            if (header != null && header.Message.Name == messageName) {
                                if (header.Use == SoapBindingUse.Encoded)
                                    isEncoded = true;
                                else if (header.Use == SoapBindingUse.Literal)
                                    isLiteral = true;
                            }
                        }
                    }
                    if (bindingOperation.Output != null) foreach (object extension in bindingOperation.Output.Extensions) {
                        if (operation != null) {
                            if (operation.IsBoundBy(bindingOperation)) {
                                SoapBodyBinding body = extension as SoapBodyBinding;
                                if (body != null) {
                                    if (body.Use == SoapBindingUse.Encoded)
                                        isEncoded = true;
                                    else if (body.Use == SoapBindingUse.Literal)
                                        isLiteral = true;
                                }
                                else if (extension is MimeXmlBinding)
                                    isLiteral = true;
                            }
                        }
                        else {
                            SoapHeaderBinding header = extension as SoapHeaderBinding;
                            if (header != null && header.Message.Name == messageName) {
                                if (header.Use == SoapBindingUse.Encoded)
                                    isEncoded = true;
                                else if (header.Use == SoapBindingUse.Literal)
                                    isLiteral = true;
                            }
                        }
                    }
                }
            }
        }

        private void AddImport(XmlSchema schema, Hashtable imports) {
            if (schema == null || imports[schema] != null)
                return;
            imports.Add(schema, schema);
            foreach (XmlSchemaExternal external in schema.Includes) {
                if (external is XmlSchemaImport) {
                    XmlSchemaImport import = (XmlSchemaImport)external;
                    foreach (XmlSchema s in allSchemas.GetSchemas(import.Namespace)) {
                        AddImport(s, imports);
                    }
                }
            }
        }

        private ServiceDescriptionImportWarnings Import(CodeNamespace codeNamespace, ImportContext importContext, Hashtable exportContext, StringCollection warnings) {
            allSchemas = new XmlSchemas();
            foreach (XmlSchema schema in schemas) {
                allSchemas.Add(schema);
            }
            foreach (ServiceDescription description in serviceDescriptions) {
                foreach (XmlSchema schema in description.Types.Schemas) {
                    allSchemas.Add(schema);
                }
            }
            Hashtable references = new Hashtable();
            if (!allSchemas.Contains(ServiceDescription.Namespace)) {
                allSchemas.AddReference(ServiceDescription.Schema);
                references[ServiceDescription.Schema] = ServiceDescription.Schema;
            }
            if (!allSchemas.Contains(Soap.Encoding)) { 
                allSchemas.AddReference(ServiceDescription.SoapEncodingSchema);
                references[ServiceDescription.SoapEncodingSchema] = ServiceDescription.SoapEncodingSchema;
            }
            allSchemas.Compile(null, false);

            // Segregate the schemas containing abstract types from those 
            // containing regular XML definitions.  This is important because
            // when you import something returning the ur-type (object), then
            // you need to import ALL types/elements within ALL schemas.  We
            // don't want the RPC-based types leaking over into the XML-based
            // element definitions.  This also occurs when you have derivation:
            // we need to search the schemas for derived types: but WHICH schemas
            // should we search.
            foreach (ServiceDescription description in serviceDescriptions) {
                foreach (Message message in description.Messages) {
                    foreach (MessagePart part in message.Parts) {
                        bool isEncoded;
                        bool isLiteral;
                        FindUse(part, out isEncoded, out isLiteral);
                        if (part.Element != null && !part.Element.IsEmpty) {
                            if (isEncoded) throw new InvalidOperationException(Res.GetString(Res.CanTSpecifyElementOnEncodedMessagePartsPart, part.Name, message.Name));
                            XmlSchemaElement element = (XmlSchemaElement)allSchemas.Find(part.Element, typeof(XmlSchemaElement));
                            if (element != null) {
                                AddSchema(element.Parent as XmlSchema, isEncoded, isLiteral, abstractSchemas, concreteSchemas, references);
                                if (element.SchemaTypeName != null && !element.SchemaTypeName.IsEmpty) {
                                    XmlSchemaType type = (XmlSchemaType)allSchemas.Find(element.SchemaTypeName, typeof(XmlSchemaType));
                                    if (type != null) {
                                        AddSchema(type.Parent as XmlSchema, isEncoded, isLiteral, abstractSchemas, concreteSchemas, references);
                                    }
                                }
                            }
                        }
                        if (part.Type != null && !part.Type.IsEmpty) {
                            XmlSchemaType type = (XmlSchemaType)allSchemas.Find(part.Type, typeof(XmlSchemaType));
                            if (type != null) {
                                AddSchema(type.Parent as XmlSchema, isEncoded, isLiteral, abstractSchemas, concreteSchemas, references);
                            }
                        }
                    }
                }
            }

            Hashtable imports;
            foreach (XmlSchemas xmlschemas in new XmlSchemas[] { abstractSchemas, concreteSchemas }) {
                // collect all imports
                imports = new Hashtable();
                foreach (XmlSchema schema in xmlschemas) {
                    AddImport(schema, imports);
                }
                // make sure we add them to the corresponding schema collections
                foreach (XmlSchema schema in imports.Keys) {
                    if (references[schema] == null && !xmlschemas.Contains(schema)) {
                        xmlschemas.Add(schema);
                    }
                }
            }

            // If a schema was not referenced by either a literal or an encoded message part,
            // add it to both collections. There's no way to tell which it should be.
            imports = new Hashtable();
            foreach (XmlSchema schema in allSchemas) {
                if (!abstractSchemas.Contains(schema) && !concreteSchemas.Contains(schema)) {
                    AddImport(schema, imports);
                }
            }

            // make sure we add them to the corresponding schema collections
            foreach (XmlSchema schema in imports.Keys) {
                if (references[schema] != null)
                    continue;
                if (!abstractSchemas.Contains(schema)) {
                    abstractSchemas.Add(schema);
                }
                if (!concreteSchemas.Contains(schema)) {
                    concreteSchemas.Add(schema);
                }
            }
            if (abstractSchemas.Count > 0) {
                foreach (XmlSchema schema in references.Values) {
                    abstractSchemas.AddReference(schema);
                }
                StringCollection schemaWarnings = SchemaCompiler.Compile(abstractSchemas);
                foreach (string warning in schemaWarnings)
                    warnings.Add(warning);
            }
            if (concreteSchemas.Count > 0) {
                foreach (XmlSchema schema in references.Values) {
                    concreteSchemas.AddReference(schema);
                }
                StringCollection schemaWarnings = SchemaCompiler.Compile(concreteSchemas);
                foreach (string warning in schemaWarnings)
                    warnings.Add(warning);
            }
            if (ProtocolName.Length > 0) {
                // If a protocol was specified, only try that one
                ProtocolImporter importer = FindImporterByName(ProtocolName);
                if (importer.GenerateCode(codeNamespace, importContext, exportContext)) return importer.Warnings;
            }
            else {
                // Otherwise, do "best" protocol (first one that generates something)
                for (int i = 0; i < importers.Length; i++) {
                    ProtocolImporter importer = importers[i];
                    if (importer.GenerateCode(codeNamespace, importContext, exportContext)) {
                        return importer.Warnings;
                    }
                }
            }
            return ServiceDescriptionImportWarnings.NoCodeGenerated;
        }

        private static void AddSchema(XmlSchema schema, bool isEncoded, bool isLiteral, XmlSchemas abstractSchemas, XmlSchemas concreteSchemas, Hashtable references) {
            if (schema != null) {
                if (isEncoded && !abstractSchemas.Contains(schema)) {
                    if (references.Contains(schema)) {
                        abstractSchemas.AddReference(schema);
                    }
                    else {
                        abstractSchemas.Add(schema);                                        
                    }
                }
                if (isLiteral && !concreteSchemas.Contains(schema)) {
                    if (references.Contains(schema)) {
                        concreteSchemas.AddReference(schema);
                    }
                    else {
                        concreteSchemas.Add(schema);                                        
                    }
                }
            }
        }
    }
}
