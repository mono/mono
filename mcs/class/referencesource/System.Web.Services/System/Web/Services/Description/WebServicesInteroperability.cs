//------------------------------------------------------------------------------
// <copyright file="WebServicesInteroperability.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;

    /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="WebServicesInteroperability"]/*' />
    /// <devdoc>
    ///
    /// </devdoc>
    // 


    public sealed class WebServicesInteroperability {
        private WebServicesInteroperability() { }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="WebServicesInteroperability.CheckConformance"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool CheckConformance(WsiProfiles claims, ServiceDescription description, BasicProfileViolationCollection violations) {
            if (description == null)
                throw new ArgumentNullException("description");
            ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
            descriptions.Add(description);
            return CheckConformance(claims, descriptions, violations);
        }
        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="WebServicesInteroperability.CheckConformance1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool CheckConformance(WsiProfiles claims, ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations) {
            if ((claims & WsiProfiles.BasicProfile1_1) == 0)
                return true;
            if (descriptions == null)
                throw new ArgumentNullException("descriptions");
            if (violations == null)
                throw new ArgumentNullException("violations");

            int count = violations.Count;
            AnalyzeDescription(descriptions, violations);
            return count == violations.Count;
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="WebServicesInteroperability.CheckConformance2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool CheckConformance(WsiProfiles claims, WebReference webReference, BasicProfileViolationCollection violations) {
            if ((claims & WsiProfiles.BasicProfile1_1) == 0)
                return true;
            if (webReference == null)
                return true;
            if (violations == null)
                throw new ArgumentNullException("violations");

            // separate descriptions and schemas
            XmlSchemas schemas = new XmlSchemas();
            ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
            StringCollection warnings = new StringCollection();
            foreach (DictionaryEntry entry in webReference.Documents) {
                ServiceDescriptionImporter.AddDocument((string)entry.Key, entry.Value, schemas, descriptions, warnings);
            }
            /* 



*/

            int count = violations.Count;
            AnalyzeDescription(descriptions,  violations);
            return count == violations.Count;
        }

        internal static bool AnalyzeBinding(Binding binding, ServiceDescription description, ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations) {
            bool inconsistentStyle = false;
            bool multipleParts = false;
            SoapBinding soapBinding = (SoapBinding)binding.Extensions.Find(typeof(SoapBinding));
            if (soapBinding == null || soapBinding.GetType() != typeof(SoapBinding))
                return false;

            SoapBindingStyle bindingStyle = soapBinding.Style == SoapBindingStyle.Default ? SoapBindingStyle.Document : soapBinding.Style;

            if (soapBinding.Transport.Length == 0) {
                // There is an inconsistency between the WSDL 1.1 specification and the WSDL 1.1 schema regarding the transport attribute. The WSDL 1.1 specification requires it; however, the schema shows it to be optional.
                // R2701 The wsdl:binding element in a DESCRIPTION MUST be constructed so that its soapbind:binding child element specifies the transport attribute.
                violations.Add("R2701", Res.GetString(Res.BindingMissingAttribute, binding.Name, description.TargetNamespace, "transport"));
            }
            else if (soapBinding.Transport != SoapBinding.HttpTransport) {
                // The profile limits the underlying transport protocol to HTTP.
                // R2702 A wsdl:binding element in a DESCRIPTION MUST specify the HTTP transport protocol with SOAP binding. Specifically, the transport attribute of its soapbind:binding child MUST have the value "http://schemas.xmlsoap.org/soap/http".
                violations.Add("R2702", Res.GetString(Res.BindingInvalidAttribute, binding.Name, description.TargetNamespace, "transport", soapBinding.Transport));
            }

            PortType portType = descriptions.GetPortType(binding.Type);
            Hashtable operations = new Hashtable();
            if (portType != null) {
                foreach (Operation op in portType.Operations) {
                    if (op.Messages.Flow == OperationFlow.Notification)
                        violations.Add("R2303", Res.GetString(Res.OperationFlowNotification, op.Name, binding.Type.Namespace, binding.Type.Namespace));
                    if (op.Messages.Flow == OperationFlow.SolicitResponse)
                        violations.Add("R2303", Res.GetString(Res.OperationFlowSolicitResponse, op.Name, binding.Type.Namespace, binding.Type.Namespace));

                    //R2304 A wsdl:portType in a DESCRIPTION MUST have operations with distinct values for their name attributes.
                    if (operations[op.Name] != null) {
                        violations.Add("R2304", Res.GetString(Res.Operation, op.Name, binding.Type.Name, binding.Type.Namespace));
                    }
                    else {
                        OperationBinding operationBinding = null;
                        foreach (OperationBinding b in binding.Operations) {
                            if (op.IsBoundBy(b)) {
                                if (operationBinding != null)
                                    violations.Add("R2304", Res.GetString(Res.OperationBinding, operationBinding.Name, operationBinding.Parent.Name, description.TargetNamespace));
                                operationBinding = b;
                            }
                        }
                        if (operationBinding == null) {
                            // The WSDL description must be consistent at both wsdl:portType and wsdl:binding levels.
                            // R2718 A wsdl:binding in a DESCRIPTION MUST have the same set of wsdl:operations as the wsdl:portType to which it refers.
                            violations.Add("R2718", Res.GetString(Res.OperationMissingBinding, op.Name, binding.Type.Name, binding.Type.Namespace));
                        }
                        else {
                            operations.Add(op.Name, op);
                        }
                    }
                }
            }
            Hashtable wireSignatures = new Hashtable();
            SoapBindingStyle style = SoapBindingStyle.Default;
            foreach (OperationBinding bindingOperation in binding.Operations) {
                SoapBindingStyle opStyle = bindingStyle;
                string name = bindingOperation.Name;
                if (name == null)
                    continue;
                if (operations[name] == null) {
                    // The WSDL description must be consistent at both wsdl:portType and wsdl:binding levels.
                    // R2718 A wsdl:binding in a DESCRIPTION MUST have the same set of wsdl:operations as the wsdl:portType to which it refers.
                    violations.Add("R2718", Res.GetString(Res.PortTypeOperationMissing, bindingOperation.Name, binding.Name, description.TargetNamespace, binding.Type.Name, binding.Type.Namespace));
                }
                Operation operation = FindOperation(portType.Operations, bindingOperation);
                SoapOperationBinding soapOpBinding = (SoapOperationBinding)bindingOperation.Extensions.Find(typeof(SoapOperationBinding));
                if (soapOpBinding != null) {
                    if (style == SoapBindingStyle.Default)
                        style = soapOpBinding.Style;
                    inconsistentStyle |= (style != soapOpBinding.Style);
                    opStyle = soapOpBinding.Style != SoapBindingStyle.Default ? soapOpBinding.Style : bindingStyle;
                }

                if (bindingOperation.Input != null) {
                    // name attribute is optional, but has to be a valis NCName
                    // R2028 A DESCRIPTION using the WSDL namespace (prefixed "wsdl" in this Profile) MUST be valid according to the XML Schema found at "http://schemas.xmlsoap.org/wsdl/2003-02-11.xsd".
                    // R2029 A DESCRIPTION using the WSDL SOAP binding namespace (prefixed "soapbind" in this Profile) MUST be valid according to the XML Schema found at "http://schemas.xmlsoap.org/wsdl/soap/2003-02-11.xsd".
                    SoapBodyBinding soapBodyBinding = FindSoapBodyBinding(true, bindingOperation.Input.Extensions, violations, opStyle == SoapBindingStyle.Document, bindingOperation.Name, binding.Name, description.TargetNamespace);
                    if (soapBodyBinding != null) {
                        if (soapBodyBinding.Use != SoapBindingUse.Encoded) {
                            Message message = operation == null ? null : operation.Messages.Input == null ? null : descriptions.GetMessage(operation.Messages.Input.Message);
                            if (opStyle == SoapBindingStyle.Rpc) {
                                CheckMessageParts(message, soapBodyBinding.Parts, false, bindingOperation.Name, binding.Name, description.TargetNamespace, wireSignatures, violations);
                            }
                            else {
                                multipleParts = multipleParts || (soapBodyBinding.Parts != null && soapBodyBinding.Parts.Length > 1);
                                int bodyParts = soapBodyBinding.Parts == null ? 0 : soapBodyBinding.Parts.Length;
                                CheckMessageParts(message, soapBodyBinding.Parts, true, bindingOperation.Name, binding.Name, description.TargetNamespace, wireSignatures, violations);
                                if (bodyParts == 0 && message != null && message.Parts.Count > 1) {
                                    // R2210 If a document-literal binding in a DESCRIPTION does not specify the parts attribute on a soapbind:body element, the corresponding abstract wsdl:message MUST define zero or one wsdl:parts.
                                    violations.Add("R2210", Res.GetString(Res.OperationBinding, bindingOperation.Name, binding.Name, description.TargetNamespace));
                                }
                            }
                        }
                    }
                }
                if (bindingOperation.Output != null) {
                    SoapBodyBinding soapBodyBinding = FindSoapBodyBinding(false, bindingOperation.Output.Extensions, violations, opStyle == SoapBindingStyle.Document, bindingOperation.Name, binding.Name, description.TargetNamespace);
                    if (soapBodyBinding != null) {
                        if (soapBodyBinding.Use != SoapBindingUse.Encoded) {
                            Message message = operation == null ? null : operation.Messages.Output == null ? null : descriptions.GetMessage(operation.Messages.Output.Message);
                            if (opStyle == SoapBindingStyle.Rpc) {
                                CheckMessageParts(message, soapBodyBinding.Parts, false, bindingOperation.Name, binding.Name, description.TargetNamespace, null, violations);
                            }
                            else {
                                multipleParts = multipleParts || (soapBodyBinding.Parts != null && soapBodyBinding.Parts.Length > 1);
                                int bodyParts = soapBodyBinding.Parts == null ? 0 : soapBodyBinding.Parts.Length;
                                CheckMessageParts(message, soapBodyBinding.Parts, true, bindingOperation.Name, binding.Name, description.TargetNamespace, null, violations);
                                if (bodyParts == 0 && message != null && message.Parts.Count > 1) {
                                    // R2210 If a document-literal binding in a DESCRIPTION does not specify the parts attribute on a soapbind:body element, the corresponding abstract wsdl:message MUST define zero or one wsdl:parts.
                                    violations.Add("R2210", Res.GetString(Res.OperationBinding, bindingOperation.Name, binding.Name, description.TargetNamespace));
                                }
                            }
                        }
                    }
                }
                foreach (FaultBinding faultBinding in bindingOperation.Faults) {
                    foreach (ServiceDescriptionFormatExtension extension in faultBinding.Extensions) {
                        if (extension is SoapFaultBinding) {
                            SoapFaultBinding fault = (SoapFaultBinding)extension;
                            if (fault.Use == SoapBindingUse.Encoded) {
                                // R2706 A wsdl:binding in a DESCRIPTION MUST use the value of "literal" for the use attribute in all soapbind:body, soapbind:fault, soapbind:header and soapbind:headerfault elements.
                                violations.Add("R2706", MessageString(fault, bindingOperation.Name, binding.Name, description.TargetNamespace, false, null));
                                continue;
                            }
                            if (fault.Name == null || fault.Name.Length == 0) {
                                // R2721 A wsdl:binding in a DESCRIPTION MUST have the name attribute specified on all contained soapbind:fault elements.
                                violations.Add("R2721", Res.GetString(Res.FaultBinding, faultBinding.Name, bindingOperation.Name, binding.Name, description.TargetNamespace));
                            }
                            else if (fault.Name != faultBinding.Name) {
                                // R2754 In a DESCRIPTION, the value of the name attribute on a soapbind:fault element MUST match the value of the name attribute on its parent wsdl:fault element.
                                violations.Add("R2754", Res.GetString(Res.FaultBinding, faultBinding.Name, bindingOperation.Name, binding.Name, description.TargetNamespace));
                            }
                            if (fault.Namespace != null && fault.Namespace.Length > 0) {
                                //R2716 A document-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:body, soapbind:header, soapbind:headerfault and soapbind:fault elements.
                                //R2726 An rpc-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:header, soapbind:headerfault and soapbind:fault elements.
                                violations.Add(opStyle == SoapBindingStyle.Document ? "R2716" : "R2726", MessageString(fault, bindingOperation.Name, binding.Name, description.TargetNamespace, false, null));
                            }
                        }
                    }
                }
                // The WSDL description must be consistent at both wsdl:portType and wsdl:binding levels.
                // R2718 A wsdl:binding in a DESCRIPTION MUST have the same set of wsdl:operations as the wsdl:portType to which it refers.
                if (operations[bindingOperation.Name] == null) {
                    violations.Add("R2718", Res.GetString(Res.PortTypeOperationMissing, bindingOperation.Name, binding.Name, description.TargetNamespace, binding.Type.Name, binding.Type.Namespace));
                }
                // 
            }
            if (multipleParts) {
                // R2201 A document-literal binding in a DESCRIPTION MUST, in each of its soapbind:body element(s), have at most one part listed in the parts attribute, if the parts attribute is specified.
                violations.Add("R2201", Res.GetString(Res.BindingMultipleParts, binding.Name, description.TargetNamespace, "parts"));
            }
            if (inconsistentStyle) {
                //R2705 A wsdl:binding in a DESCRIPTION MUST use either be a "rpc-literal binding" or a "document-literal" binding.
                violations.Add("R2705", Res.GetString(Res.Binding, binding.Name, description.TargetNamespace));
            }
            return true;
        }

        internal static void AnalyzeDescription(ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations) {
            /* Ignoring R003: A DESCRIPTION's conformance claims MUST be children of the wsdl:documentation element of each of the elements:
            *   wsdl:port,
            *   wsdl:binding,
            *   wsdl:portType,
            *   wsdl:operation (as a child element of wsdl:portType but not of wsdl:binding) and wsdl:message.
            *
            * Ignoring R2022 When they appear in a DESCRIPTION, wsdl:import elements MUST precede all other elements from the WSDL namespace except wsdl:documentation.
            * Ignoring R2023 When they appear in a DESCRIPTION, wsdl:types elements MUST precede all other elements from the WSDL namespace except wsdl:documentation and wsdl:import.
            * Ignoring R2749 A wsdl:binding in a DESCRIPTION MUST NOT use the attribute named parts on contained soapbind:header and soapbind:headerfault elements.
            * Ignoring R2206 A wsdl:message in a DESCRIPTION containing a wsdl:part that uses the element attribute MUST refer, in that attribute, to a global element declaration.
            * Ignoring R2209 A wsdl:binding in a DESCRIPTION SHOULD bind every wsdl:part of a wsdl:message in the wsdl:portType to which it refers to one of soapbind:body, soapbind:header, soapbind:fault or soapbind:headerfault.
            */
            bool foundBinding = false;
            foreach (ServiceDescription description in descriptions) {
                StringCollection compileWarnings = SchemaCompiler.Compile(description.Types.Schemas);
                CheckWsdlImports(description, violations);
                CheckTypes(description, violations);

                foreach (string warning in description.ValidationWarnings) {
                    violations.Add("R2028, R2029", warning);
                }

                foreach (Binding binding in description.Bindings) {
                    foundBinding |= AnalyzeBinding(binding, description, descriptions, violations);
                }
            }
            if (foundBinding) {
                CheckExtensions(descriptions, violations);
            }
            else {
                // Rxxxx=No SOAP 1.1 binding were found
                // WS-I's Basic Profile 1.1 consists of implementation guidelines that recommend how a set of core Web services specifications should be used together to develop interoperable Web services. For the 1.0 Profile, those specifications are SOAP 1.1, WSDL 1.1, UDDI 2.0, XML 1.0 and XML Schema.
                violations.Add("Rxxxx");
            }
        }

        // R2001: A DESCRIPTION MUST only use the WSDL 'import' statement to import another WSDL description.
        // R2002: To import XML Schema Definitions, a DESCRIPTION MUST use the XML Schema "import" statement.
        // R2004: A DESCRIPTION MUST NOT use the XML Schema "import" statement to import a Schema from any document whose root element is not "schema" from the namespace "http://www.w3.org/2001/XMLSchema".
        // R2007: A DESCRIPTION MUST specify a non-empty location attribute on the wsdl:import element.
        // R2008: In a DESCRIPTION the value of the location attribute of a wsdl:import element SHOULD be treated as a hint.
        // R2803: In a DESCRIPTION, the namespace attribute of the wsdl:import MUST NOT be a relative URI.
        static void CheckWsdlImports(ServiceDescription description, BasicProfileViolationCollection violations) {
            foreach (Import import in description.Imports) {
                if (import.Location == null || import.Location.Length == 0) {
                    violations.Add("R2007", Res.GetString(Res.Description, description.TargetNamespace));
                }
                string ns = import.Namespace;
                if (ns.Length != 0) {
                    Uri uri;
                    bool isAbsoluteUri = Uri.TryCreate(ns, UriKind.Absolute, out uri);
                    if (!isAbsoluteUri) {
                        violations.Add("R2803", Res.GetString(Res.Description, description.TargetNamespace));
                    }
                }
            }
        }

        // R2105 All xsd:schema elements contained in a wsdl:types element of a DESCRIPTION MUST have a targetNamespace attribute with a valid and non-null value, UNLESS the xsd:schema element has xsd:import and/or xsd:annotation as its only child element(s).
        static void CheckTypes(ServiceDescription description, BasicProfileViolationCollection violations) {
            foreach (XmlSchema schema in description.Types.Schemas) {
                if (schema.TargetNamespace == null || schema.TargetNamespace.Length == 0) {
                    foreach (XmlSchemaObject o in schema.Items) {
                        if (!(o is XmlSchemaAnnotation)) {
                            violations.Add("R2105", Res.GetString(Res.Element, "schema", description.TargetNamespace));
                            return;
                        }
                    }
                }
            }
        }

        static void CheckMessagePart(MessagePart part, bool element, string message, string operation, string binding, string ns, Hashtable wireSignatures, BasicProfileViolationCollection violations) {
            if (part == null) {
                // R2710=The operations in a wsdl:binding in a DESCRIPTION MUST result in wire signatures that are different from one another. An endpoint that supports multiple operations must unambiguously identify the operation being invoked based on the input message that it receives. This is only possible if all the operations specified in the wsdl:binding associated with an endpoint have a unique wire signature.
                if (!element) {
                    // WireSignature=Input message '{0}' from namespace '{1}' has wire signature '{2}:{3}'.
                    AddSignature(wireSignatures, operation, ns, message, ns, violations);
                }
                else {
                    AddSignature(wireSignatures, null, null, message, ns, violations);
                }
                return;
            }
            if (part.Type != null && !part.Type.IsEmpty && part.Element != null && !part.Element.IsEmpty) {
                // R2306 A wsdl:message in a DESCRIPTION MUST NOT specify both type and element attributes on the same wsdl:part
                violations.Add("R2306", Res.GetString(Res.Part, part.Name, message, ns));
            }
            else {
                XmlQualifiedName qname = part.Type == null || part.Type.IsEmpty ? part.Element : part.Type;
                if (qname.Namespace == null || qname.Namespace.Length == 0) {
                    //The use of unqualified element names may cause naming conflicts, therefore qualified names must be used for the children of soap:Body.
                    //R1014 The children of the soap:Body element in a MESSAGE MUST be namespace qualified.
                    violations.Add("R1014", Res.GetString(Res.Part, part.Name, message, ns));
                }
            }
            if (!element && (part.Type == null || part.Type.IsEmpty)) {
                // R2203 An rpc-literal binding in a DESCRIPTION MUST refer, in its soapbind:body element(s), only to wsdl:part element(s) that have been defined using the type attribute.
                violations.Add("R2203", Res.GetString(Res.Part, part.Name, message, ns));
            }

            if (element && (part.Element == null || part.Element.IsEmpty)) {
                // R2204 A document-literal binding in a DESCRIPTION MUST refer, in each of its soapbind:body element(s), only to wsdl:part element(s) that have been defined using the element attribute.
                violations.Add("R2204", Res.GetString(Res.Part, part.Name, message, ns));
            }

            // R2710=The operations in a wsdl:binding in a DESCRIPTION MUST result in wire signatures that are different from one another. An endpoint that supports multiple operations must unambiguously identify the operation being invoked based on the input message that it receives. This is only possible if all the operations specified in the wsdl:binding associated with an endpoint have a unique wire signature.
            if (!element) {
                AddSignature(wireSignatures, operation, ns, message, ns, violations);
            }
            else if (part.Element != null) {
                AddSignature(wireSignatures, part.Element.Name, part.Element.Namespace, message, ns, violations);
            }
        }

        static void AddSignature(Hashtable wireSignatures, string name, string ns, string message, string messageNs, BasicProfileViolationCollection violations) {
            if (wireSignatures == null)
                return;
            string key = ns + ":" + name;
            string exisiting = (string)wireSignatures[key];
            // WireSignatureEmpty=Input message '{0}' from namespace '{1}' has no elements (empty wire signature)
            string wire = ns == null && name == null ? Res.GetString(Res.WireSignatureEmpty, message, messageNs) : Res.GetString(Res.WireSignature, message, messageNs, ns, name);
            if (exisiting != null) {
                if (exisiting.Length > 0) {
                    // R2710=The operations in a wsdl:binding in a DESCRIPTION MUST result in wire signatures that are different from one another. An endpoint that supports multiple operations must unambiguously identify the operation being invoked based on the input message that it receives. This is only possible if all the operations specified in the wsdl:binding associated with an endpoint have a unique wire signature.
                    //Res.GetString(Res.OperationOverload, binding, bindingNs, exisiting, message)
                    violations.Add("R2710", exisiting);
                    violations.Add("R2710", wire);
                    wireSignatures[key] = string.Empty;
                }
            }
            else {
                wireSignatures[key] = wire;
            }
        }
        static void CheckMessageParts(Message message, string[] parts, bool element, string operation, string binding, string ns, Hashtable wireSignatures, BasicProfileViolationCollection violations) {
            if (message == null)
                return;

            if (message.Parts == null || message.Parts.Count == 0) {
                // 

                if (!element) {
                    AddSignature(wireSignatures, operation, ns, message.Name, ns, violations);
                }
                else {
                    AddSignature(wireSignatures, null, null, message.Name, ns, violations);
                }
                return;
            }
            if (parts == null || parts.Length == 0) {
                for (int i = 0; i < message.Parts.Count; i++) {
                    CheckMessagePart(message.Parts[i], element, message.Name, operation, binding, ns, i == 0 ? wireSignatures : null, violations);
                }
            }
            else {
                for (int i = 0; i < parts.Length; i++) {
                    if (parts[i] == null)
                        continue;
                    MessagePart part = message.Parts[parts[i]];
                    CheckMessagePart(message.Parts[i], element, message.Name, operation, binding, ns, i == 0 ? wireSignatures : null, violations);
                }
            }
        }

        // R2716 A document-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:body, soapbind:header, soapbind:headerfault and soapbind:fault elements.
        // R2717 An rpc-literal binding in a DESCRIPTION MUST have the namespace attribute specified, the value of which MUST be an absolute URI, on contained soapbind:body elements.
        // R2726 An rpc-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:header, soapbind:headerfault and soapbind:fault elements.
        // R2706 A wsdl:binding in a DESCRIPTION MUST use the value of "literal" for the use attribute in all soapbind:body, soapbind:fault, soapbind:header and soapbind:headerfault elements.
        static SoapBodyBinding FindSoapBodyBinding(bool input, ServiceDescriptionFormatExtensionCollection extensions, BasicProfileViolationCollection violations, bool documentBinding, string operationName, string bindingName, string bindingNs) {
            SoapBodyBinding body = null;
            for (int i = 0; i < extensions.Count; i++) {
                object item = extensions[i];
                string ns = null;
                bool knownExtension = false;
                bool encodedBinding = false;
                if (item is SoapBodyBinding) {
                    knownExtension = true;
                    body = (SoapBodyBinding)item;
                    ns = body.Namespace;
                    encodedBinding = (body.Use == SoapBindingUse.Encoded);
                }
                else if (item is SoapHeaderBinding) {
                    knownExtension = true;
                    SoapHeaderBinding header = (SoapHeaderBinding)item;
                    ns = header.Namespace;
                    encodedBinding = (header.Use == SoapBindingUse.Encoded);
                    if (!encodedBinding && (header.Part == null || header.Part.Length == 0)) {
                        // R2720 A wsdl:binding in a DESCRIPTION MUST use the attribute named part with a schema type of "NMTOKEN" on all contained soapbind:header and soapbind:headerfault elements.
                        violations.Add("R2720", MessageString(header, operationName, bindingName, bindingNs, input, null));
                    }
                    if (header.Fault != null) {
                        encodedBinding |= (header.Fault.Use == SoapBindingUse.Encoded);
                        if (!encodedBinding) {
                            if (header.Fault.Part == null || header.Fault.Part.Length == 0) {
                                // R2720 A wsdl:binding in a DESCRIPTION MUST use the attribute named part with a schema type of "NMTOKEN" on all contained soapbind:header and soapbind:headerfault elements.
                                violations.Add("R2720", MessageString(header.Fault, operationName, bindingName, bindingNs, input, null));
                            }
                            if (header.Fault.Namespace != null && header.Fault.Namespace.Length > 0) {
                                violations.Add(documentBinding ? "R2716" : "R2726", MessageString(item, operationName, bindingName, bindingNs, input, null));
                            }
                        }
                    }
                }
                if (encodedBinding) {
                    // R2706 A wsdl:binding in a DESCRIPTION MUST use the value of "literal" for the use attribute in all soapbind:body, soapbind:fault, soapbind:header and soapbind:headerfault elements.
                    violations.Add("R2706", MessageString(item, operationName, bindingName, bindingNs, input, null));
                }
                else if (knownExtension) {
                    if (ns == null || ns.Length == 0) {
                        if (!documentBinding && item is SoapBodyBinding) {
                            //R2717 An rpc-literal binding in a DESCRIPTION MUST have the namespace attribute specified, the value of which MUST be an absolute URI, on contained soapbind:body elements.
                            violations.Add("R2717", MessageString(item, operationName, bindingName, bindingNs, input, null));
                        }
                    }
                    else {
                        if (documentBinding || !(item is SoapBodyBinding)) {
                            //R2716 A document-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:body, soapbind:header, soapbind:headerfault and soapbind:fault elements.
                            //R2726 An rpc-literal binding in a DESCRIPTION MUST NOT have the namespace attribute specified on contained soapbind:header, soapbind:headerfault and soapbind:fault elements.
                            violations.Add(documentBinding ? "R2716" : "R2726", MessageString(item, operationName, bindingName, bindingNs, input, null));
                        }
                        else {
                            // this is soap:Body rpc binding
                            //R2717 An rpc-literal binding in a DESCRIPTION MUST have the namespace attribute specified, the value of which MUST be an absolute URI, on contained soapbind:body elements.
                            Uri uri;
                            bool isAbsoluteUri = Uri.TryCreate(ns, UriKind.Absolute, out uri);
                            if (!isAbsoluteUri) {
                                violations.Add("R2717", MessageString(item, operationName, bindingName, bindingNs, input, Res.GetString(Res.UriValueRelative, ns)));
                            }
                        }
                    }
                }
            }
            return body;
        }

        static string MessageString(object item, string operation, string binding, string ns, bool input, string details) {
            string message = null;
            string id = null;
            if (item is SoapBodyBinding) {
                message = input ? Res.InputElement : Res.OutputElement;
                id = "soapbind:body";
            }
            else if (item is SoapHeaderBinding) {
                message = input ? Res.InputElement : Res.OutputElement;
                id = "soapbind:header";
            }
            else if (item is SoapFaultBinding) {
                message = Res.Fault;
                id = ((SoapFaultBinding)item).Name;
            }
            else if (item is SoapHeaderFaultBinding) {
                message = Res.HeaderFault;
                id = "soapbind:headerfault";
            }
            if (message == null)
                return null;

            return Res.GetString(message, id, operation, binding, ns, details);
        }

        //R2026 A DESCRIPTION SHOULD NOT include extension elements with a wsdl:required attribute value of "true" on any WSDL construct (wsdl:binding, wsdl:portType, wsdl:message, wsdl:types or wsdl:import) that claims conformance to the Profile.
        static bool CheckExtensions(ServiceDescriptionFormatExtensionCollection extensions) {
            foreach (ServiceDescriptionFormatExtension extension in extensions) {
                if (extension.Required) {
                    return false;
                }
            }
            return true;
        }

        //R2026 A DESCRIPTION SHOULD NOT include extension elements with a wsdl:required attribute value of "true" on any WSDL construct (wsdl:binding, wsdl:portType, wsdl:message, wsdl:types or wsdl:import) that claims conformance to the Profile.
        static void CheckExtensions(Binding binding, ServiceDescription description, BasicProfileViolationCollection violations) {
            SoapBinding soapBinding = (SoapBinding)binding.Extensions.Find(typeof(SoapBinding));
            if (soapBinding == null || soapBinding.GetType() != typeof(SoapBinding))
                return;
            if (!CheckExtensions(binding.Extensions)) {
                violations.Add("R2026", Res.GetString(Res.BindingInvalidAttribute, binding.Name, description.TargetNamespace, "wsdl:required", "true"));
            }
        }

        //R2026 A DESCRIPTION SHOULD NOT include extension elements with a wsdl:required attribute value of "true" on any WSDL construct (wsdl:binding, wsdl:portType, wsdl:message, wsdl:types or wsdl:import) that claims conformance to the Profile.
        // A claim on a wsdl:port is inherited by the referenced wsdl:binding
        // A claim on a wsdl:binding is inherited by the referenced wsdl:portType
        // A claim on a wsdl:portType is inherited by the referenced wsdl:operations
        // A claim on a wsdl:operation is inherited by the referenced wsdl:messages of its child wsdl:output and/or wsdl:input
        static void CheckExtensions(ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations) {
            Hashtable bindings = new Hashtable();
            foreach (ServiceDescription description in descriptions) {
                WsiProfiles typesClaims = ServiceDescription.GetConformanceClaims(description.Types.DocumentationElement);
                if (typesClaims == WsiProfiles.BasicProfile1_1 && !CheckExtensions(description.Extensions)) {
                    violations.Add("R2026", Res.GetString(Res.Element, "wsdl:types", description.TargetNamespace));
                }
                foreach (Service service in description.Services) {
                    foreach (Port port in service.Ports) {
                        WsiProfiles portClaims = ServiceDescription.GetConformanceClaims(port.DocumentationElement);
                        if (portClaims == WsiProfiles.BasicProfile1_1) {
                            if (!CheckExtensions(port.Extensions))
                                violations.Add("R2026", Res.GetString(Res.Port, port.Name, service.Name, description.TargetNamespace));

                            Binding binding = descriptions.GetBinding(port.Binding);
                            if (bindings[binding] != null) {
                                CheckExtensions(binding, description, violations);
                                bindings.Add(binding, binding);
                            }
                        }
                    }
                }

                foreach (Binding binding in description.Bindings) {
                    SoapBinding soapBinding = (SoapBinding)binding.Extensions.Find(typeof(SoapBinding));
                    if (soapBinding == null || soapBinding.GetType() != typeof(SoapBinding))
                        continue;
                    if (bindings[binding] != null)
                        continue;

                    WsiProfiles bindingClaims = ServiceDescription.GetConformanceClaims(binding.DocumentationElement);
                    if (bindingClaims == WsiProfiles.BasicProfile1_1) {
                        CheckExtensions(binding, description, violations);
                        bindings.Add(binding, binding);
                    }
                }
            }
        }

        static Operation FindOperation(OperationCollection operations, OperationBinding bindingOperation) {
            foreach (Operation op in operations) {
                if (op.IsBoundBy(bindingOperation)) {
                    return op;
                }
            }
            return null;
        }
    }

    /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation"]/*' />
    /// <devdoc>
    ///
    /// </devdoc>
    public class BasicProfileViolation {
        WsiProfiles claims = WsiProfiles.BasicProfile1_1;
        string normativeStatement;
        string details;
        string recommendation;
        StringCollection elements;

        internal BasicProfileViolation(string normativeStatement) : this(normativeStatement, null) {
        }

        internal BasicProfileViolation(string normativeStatement, string element) {
            this.normativeStatement = normativeStatement;
            int comma = normativeStatement.IndexOf(',');
            if (comma >= 0) {
                normativeStatement = normativeStatement.Substring(0, comma);
            }
            this.details = Res.GetString("HelpGeneratorServiceConformance" + normativeStatement);
            this.recommendation = Res.GetString("HelpGeneratorServiceConformance" + normativeStatement + "_r");
            if (element != null)
                this.Elements.Add(element);
            if (this.normativeStatement == "Rxxxx") {
                this.normativeStatement = Res.GetString(Res.Rxxxx);
            }
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation.Claims"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WsiProfiles Claims { get { return claims; } }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation.Details"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Details {
            get {
                if (details == null)
                    return String.Empty;
                return details;
            }
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation.Elements"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public StringCollection Elements {
            get {
                if (elements == null)
                    elements = new StringCollection();
                return elements;
            }
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation.NormativeStatement"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string NormativeStatement { get { return normativeStatement; } }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation.NormativeStatement"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Recommendation { get { return recommendation; } }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolation.ToString"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(normativeStatement);
            sb.Append(": ");
            sb.Append(Details);
            foreach (string element in Elements) {
                sb.Append(Environment.NewLine);
                sb.Append("  -  ");
                sb.Append(element);
            }
            return sb.ToString();
        }
    }

    /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class BasicProfileViolationCollection : CollectionBase, IEnumerable<BasicProfileViolation> {
        Hashtable violations = new Hashtable();

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public BasicProfileViolation this[int index] {
            get { return (BasicProfileViolation)List[index]; }
            set { List[index] = value; }
        }

        internal int Add(BasicProfileViolation violation) {
            BasicProfileViolation src = (BasicProfileViolation)violations[violation.NormativeStatement];

            if (src == null) {
                violations[violation.NormativeStatement] = violation;
                return List.Add(violation);
            }
            foreach (string element in violation.Elements) {
                src.Elements.Add(element);
            }
            return IndexOf(src);
        }

        internal int Add(string normativeStatement) {
            return Add(new BasicProfileViolation(normativeStatement));
        }

        internal int Add(string normativeStatement, string element) {
            return Add(new BasicProfileViolation(normativeStatement, element));
        }

        IEnumerator<BasicProfileViolation> IEnumerable<BasicProfileViolation>.GetEnumerator() {
            return new BasicProfileViolationEnumerator(this);
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, BasicProfileViolation violation) {
            List.Insert(index, violation);
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(BasicProfileViolation violation) {
            return List.IndexOf(violation);
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(BasicProfileViolation violation) {
            return List.Contains(violation);
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(BasicProfileViolation violation) {
            List.Remove(violation);
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(BasicProfileViolation[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\WebServicesInteroperability.uex' path='docs/doc[@for="BasicProfileViolationCollection.ToString"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ToString() {
            if (List.Count > 0) {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < List.Count; i++) {
                    BasicProfileViolation violation = this[i];
                    if (i != 0) {
                        sb.Append(Environment.NewLine);
                    }
                    sb.Append(violation.NormativeStatement);
                    sb.Append(": ");
                    sb.Append(violation.Details);
                    foreach (string element in violation.Elements) {
                        sb.Append(Environment.NewLine);
                        sb.Append("  -  ");
                        sb.Append(element);
                    }
                    if (violation.Recommendation != null && violation.Recommendation.Length > 0) {
                        sb.Append(Environment.NewLine);
                        sb.Append(violation.Recommendation);
                    }
                }
                return sb.ToString();
            }
            return String.Empty;
        }
    }

    public class BasicProfileViolationEnumerator : IEnumerator<BasicProfileViolation>, System.Collections.IEnumerator {
        private BasicProfileViolationCollection list;
        private int idx, end;

        public BasicProfileViolationEnumerator(BasicProfileViolationCollection list) {
            this.list = list;
            this.idx = -1;
            this.end = list.Count - 1;
        }

        public void Dispose() {
        }

        public bool MoveNext() {
            if (this.idx >= this.end)
                return false;

            this.idx++;
            return true;
        }

        public BasicProfileViolation Current {
            get { return this.list[this.idx]; }
        }

        object System.Collections.IEnumerator.Current {
            get { return this.list[this.idx]; }
        }

        void System.Collections.IEnumerator.Reset() {
            this.idx = -1;
        }
    }
}
