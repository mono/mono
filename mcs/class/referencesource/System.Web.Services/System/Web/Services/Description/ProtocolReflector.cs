//------------------------------------------------------------------------------
// <copyright file="ProtocolReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------


namespace System.Web.Services.Description {
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Collections.Generic;

    /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public abstract class ProtocolReflector {
        ServiceDescriptionReflector reflector;
        LogicalMethodInfo method;
        Operation operation;
        OperationBinding operationBinding;
        Port port;
        PortType portType;
        Binding binding;
        WebMethodAttribute methodAttr;
        Message inputMessage;
        Message outputMessage;
        MessageCollection headerMessages;
        ServiceDescription bindingServiceDescription;
        CodeIdentifiers portNames;
        bool emptyBinding;

        internal void Initialize(ServiceDescriptionReflector reflector) {
            this.reflector = reflector;
        }

        internal bool IsEmptyBinding { get { return emptyBinding; } }

            /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Service"]/*' />
            /// <devdoc>
            ///    <para>[To be supplied.]</para>
            /// </devdoc>
            public Service Service {
            get { return reflector.Service; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription ServiceDescription {
            get { return reflector.ServiceDescription; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ServiceDescriptions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionCollection ServiceDescriptions {
            get { return reflector.ServiceDescriptions; }
        }

        internal List<Action<Uri>> UriFixups { get { return this.reflector.UriFixups; } }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Schemas"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemas Schemas {
            get { return reflector.Schemas; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.SchemaExporter"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemaExporter SchemaExporter {
            get { return reflector.SchemaExporter; }
        }
        
        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ReflectionImporter"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlReflectionImporter ReflectionImporter {
            get { return reflector.ReflectionImporter; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.DefaultNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string DefaultNamespace {
            get { return reflector.ServiceAttribute.Namespace; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ServiceUrl"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ServiceUrl {
            get { return reflector.ServiceUrl; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ServiceType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type ServiceType {
            get { return reflector.ServiceType; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Method"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public LogicalMethodInfo Method {
            get { return method; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Binding Binding {
            get { return binding; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.PortType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PortType PortType {
            get { return portType; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Port"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Port Port {
            get { return port; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Operation"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Operation Operation {
            get { return operation; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.OperationBinding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationBinding OperationBinding {
            get { return operationBinding; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.MethodAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebMethodAttribute MethodAttribute {
            get { return methodAttr; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.Methods"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public LogicalMethodInfo[] Methods {
            get { return reflector.Methods; }
        }

        internal Hashtable ReflectionContext {
            get { return reflector.ReflectionContext; }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.InputMessage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message InputMessage {
            get {
                if (inputMessage == null) {
                    string messageName = XmlConvert.EncodeLocalName(methodAttr.MessageName.Length == 0 ? Method.Name : methodAttr.MessageName);
                    bool diffNames = messageName != Method.Name;

                    inputMessage = new Message();
                    inputMessage.Name = messageName + ProtocolName + "In";

                    OperationInput input = new OperationInput();
                    if (diffNames) input.Name = messageName;
                    input.Message = new XmlQualifiedName(inputMessage.Name, bindingServiceDescription.TargetNamespace);
                    operation.Messages.Add(input);

                    OperationBinding.Input = new InputBinding();
                    if (diffNames) OperationBinding.Input.Name = messageName;
                }
                return inputMessage;
            }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.OutputMessage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message OutputMessage {
            get {
                if (outputMessage == null) {
                    string messageName = XmlConvert.EncodeLocalName(methodAttr.MessageName.Length == 0 ? Method.Name : methodAttr.MessageName);
                    bool diffNames = messageName != Method.Name;

                    outputMessage = new Message();
                    outputMessage.Name = messageName + ProtocolName + "Out";

                    OperationOutput output = new OperationOutput();
                    if (diffNames) output.Name = messageName;
                    output.Message = new XmlQualifiedName(outputMessage.Name, bindingServiceDescription.TargetNamespace);
                    operation.Messages.Add(output);

                    OperationBinding.Output = new OutputBinding();
                    if (diffNames) OperationBinding.Output.Name = messageName;
                }
                return outputMessage;
            }
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.HeaderMessages"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public MessageCollection HeaderMessages {
            get {
                if (headerMessages == null) {
                    headerMessages = new MessageCollection(bindingServiceDescription);
                }
                return headerMessages;
            }
        }

        void MoveToMethod(LogicalMethodInfo method) {
            this.method = method;
            this.methodAttr = method.MethodAttribute;
        }

        class ReflectedBinding {
            internal ReflectedBinding() { }
            internal ReflectedBinding(WebServiceBindingAttribute bindingAttr) {
                this.bindingAttr = bindingAttr;
            }
            public WebServiceBindingAttribute bindingAttr;
            public ArrayList methodList;
        }


        internal void Reflect() {
            emptyBinding = false;
            Hashtable bindings = new Hashtable();
            Hashtable reflectedBindings = new Hashtable();
            for (int i = 0; i < reflector.Methods.Length; i++) {
                MoveToMethod(reflector.Methods[i]);
                string bindingName = ReflectMethodBinding();
                if (bindingName == null) bindingName = string.Empty;
                ReflectedBinding reflectedBinding = (ReflectedBinding)reflectedBindings[bindingName];
                if (reflectedBinding == null) {
                    reflectedBinding = new ReflectedBinding();
                    reflectedBinding.bindingAttr = WebServiceBindingReflector.GetAttribute(method, bindingName);
                    if (reflectedBinding.bindingAttr == null || (bindingName.Length == 0 && reflectedBinding.bindingAttr.Location.Length > 0)) {
                        reflectedBinding.bindingAttr = new WebServiceBindingAttribute();
                    }
                    reflectedBindings.Add(bindingName, reflectedBinding);
                }
                if (reflectedBinding.bindingAttr.Location.Length == 0) {
                    if (reflectedBinding.methodList == null)
                        reflectedBinding.methodList = new ArrayList();
                    reflectedBinding.methodList.Add(method);
                    bindings[reflectedBinding.bindingAttr.Name] = method;
                }
                else {
                    AddImport(reflectedBinding.bindingAttr.Namespace, reflectedBinding.bindingAttr.Location);
                }
            }
            foreach (ReflectedBinding reflectedBinding in reflectedBindings.Values) {
                ReflectBinding(reflectedBinding);
            }

            // Only check for empty binding if we do not have real bindings
            if (reflectedBindings.Count == 0) {
                // It should be possible to get the value for WebReference.ServiceLocationUrl even if the web service has no web methods. 
                // This is a common scenario for Whitehorse during the early stages of development when a user is defining the web 
                // components and their inter-connections, but not the details of whatmethods will be present on each web service.
                // get all WebServiceBindings
                emptyBinding = true;
                ReflectedBinding binding = null;
                object[] attrs = ServiceType.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
                for (int i = 0; i < attrs.Length; i++) {
                    WebServiceBindingAttribute bindingAttribute = (WebServiceBindingAttribute)attrs[i];
                    if (bindings[bindingAttribute.Name] != null)
                        continue;
                    if (binding != null) {
                        binding = null;
                        break;
                    }
                    binding = new ReflectedBinding(bindingAttribute);
                }
                if (binding != null) ReflectBinding(binding);
            }

            Type[] interfaces = ServiceType.GetInterfaces();
            // iterate through all the interfaces for this type
            foreach (Type bindingInterface in interfaces) {
                object[] attrs = bindingInterface.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
                for (int i = 0; i < attrs.Length; i++) {
                    WebServiceBindingAttribute bindingAttribute = (WebServiceBindingAttribute)attrs[i];
                    if (bindings[bindingAttribute.Name] != null)
                        continue;
                    ReflectBinding(new ReflectedBinding(bindingAttribute));
                }
            }
            ReflectDescription();
        }

        void AddImport(string ns, string location) {
            foreach (Import import in ServiceDescription.Imports) {
                if (import.Namespace == ns && import.Location == location) {
                    return;
                }
            }
            Import newImport = new Import();
            newImport.Namespace = ns;
            newImport.Location = location;
            ServiceDescription.Imports.Add(newImport);
        }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.GetServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription GetServiceDescription(string ns) {
            ServiceDescription description = ServiceDescriptions[ns];
            if (description == null) {
                description = new ServiceDescription();
                description.TargetNamespace = ns;
                ServiceDescriptions.Add(description);
            }
            return description;
        }

        void ReflectBinding(ReflectedBinding reflectedBinding) {
            string bindingName = XmlConvert.EncodeLocalName(reflectedBinding.bindingAttr.Name);
            string bindingNamespace = reflectedBinding.bindingAttr.Namespace;
            if (bindingName.Length == 0) bindingName = Service.Name + ProtocolName;
            if (bindingNamespace.Length == 0) bindingNamespace = ServiceDescription.TargetNamespace;
            WsiProfiles claims = WsiProfiles.None;
            
            if (reflectedBinding.bindingAttr.Location.Length > 0) {
                // If a URL is specified for the WSDL, file, then we just import the
                // binding from there instead of generating it in this WSDL file.
                portType = null;
                binding = null;
            }
            else {
                bindingServiceDescription = GetServiceDescription(bindingNamespace);
                CodeIdentifiers bindingNames = new CodeIdentifiers();
                foreach (Binding b in bindingServiceDescription.Bindings)
                    bindingNames.AddReserved(b.Name);

                bindingName = bindingNames.AddUnique(bindingName, binding);

                portType = new PortType();
                binding = new Binding();
                portType.Name = bindingName;
                binding.Name = bindingName;
                binding.Type = new XmlQualifiedName(portType.Name, bindingNamespace);
                claims = reflectedBinding.bindingAttr.ConformsTo & this.ConformsTo;
                if (reflectedBinding.bindingAttr.EmitConformanceClaims && claims != WsiProfiles.None) {
                    ServiceDescription.AddConformanceClaims(binding.GetDocumentationElement(), claims);
                }
                bindingServiceDescription.Bindings.Add(binding);
                bindingServiceDescription.PortTypes.Add(portType);
            }
            
            if (portNames == null) {
                portNames = new CodeIdentifiers();
                foreach (Port p in Service.Ports)
                    portNames.AddReserved(p.Name);
            }

            port = new Port();
            port.Binding = new XmlQualifiedName(bindingName, bindingNamespace);
            port.Name = portNames.AddUnique(bindingName, port);
            Service.Ports.Add(port);
            
            BeginClass();

            if (reflectedBinding.methodList != null && reflectedBinding.methodList.Count > 0) {
                foreach (LogicalMethodInfo method in reflectedBinding.methodList) {

                    MoveToMethod(method);

                    operation = new Operation();
                    operation.Name = XmlConvert.EncodeLocalName(method.Name);
                    if (methodAttr.Description != null && methodAttr.Description.Length > 0)
                        operation.Documentation = methodAttr.Description;

                    operationBinding = new OperationBinding();
                    operationBinding.Name = operation.Name;

                    inputMessage = null;
                    outputMessage = null;
                    headerMessages = null;

                    if (ReflectMethod()) {
                        if (inputMessage != null) bindingServiceDescription.Messages.Add(inputMessage);
                        if (outputMessage != null) bindingServiceDescription.Messages.Add(outputMessage);
                        if (headerMessages != null) {
                            foreach (Message headerMessage in headerMessages) {
                                bindingServiceDescription.Messages.Add(headerMessage);
                            }
                        }
                        binding.Operations.Add(operationBinding);
                        portType.Operations.Add(operation);
                    }
                }
            }
            if (binding != null && claims == WsiProfiles.BasicProfile1_1 && ProtocolName == "Soap") {
                BasicProfileViolationCollection warnings = new BasicProfileViolationCollection();
                WebServicesInteroperability.AnalyzeBinding(binding, bindingServiceDescription, ServiceDescriptions, warnings);
                if (warnings.Count > 0) {
                    throw new InvalidOperationException(Res.GetString(Res.WebWsiViolation, ServiceType.FullName, warnings.ToString()));
                }
            }
            EndClass();
        }

        internal virtual WsiProfiles ConformsTo { get { return WsiProfiles.None; } }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ProtocolName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract string ProtocolName { get; }

        // These overridable methods have no parameters.  The subclass uses properties on this
        // base object to obtain the information.  This allows us to grow the set of
        // information passed to the methods over time w/o breaking anyone.   They are protected
        // instead of public because this object is passed to extensions and we don't want
        // those calling these methods.
        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.BeginClass"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void BeginClass() { }
        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ReflectMethod"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract bool ReflectMethod();
        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ReflectMethodBinding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual string ReflectMethodBinding() { return string.Empty; }
        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.EndClass"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void EndClass() { }

        /// <include file='doc\ProtocolReflector.uex' path='docs/doc[@for="ProtocolReflector.ReflectDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void ReflectDescription() { 
        }
    }
}
