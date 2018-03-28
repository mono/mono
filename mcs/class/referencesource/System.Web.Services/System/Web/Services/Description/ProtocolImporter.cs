//------------------------------------------------------------------------------
// <copyright file="ProtocolImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Text;
    using System.Xml;
    using System.Web.Services.Configuration;
    using System.Configuration; 
    using System.Security.Permissions;
    using System.Threading;
    using System.Diagnostics;

    /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public abstract class ProtocolImporter {
        ServiceDescriptionImporter importer;
        CodeNamespace codeNamespace;
        CodeIdentifiers methodNames;
        CodeTypeDeclaration codeClass;
        CodeTypeDeclarationCollection classes;
        ServiceDescriptionImportWarnings warnings;
        Port port;
        PortType portType;
        Binding binding;
        Operation operation;
        OperationBinding operationBinding;
        bool encodedBinding;
        ImportContext importContext;
        Hashtable exportContext;
        Service service;
        Message inputMessage;
        Message outputMessage;
        string className;
        int bindingCount;
        bool anyPorts;

        internal void Initialize(ServiceDescriptionImporter importer) {
            this.importer = importer;
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.ServiceDescriptions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionCollection ServiceDescriptions {
            get { return importer.ServiceDescriptions; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Schemas"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemas Schemas {
            get { return importer.AllSchemas; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.AbstractSchemas"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemas AbstractSchemas {
            get { return importer.AbstractSchemas; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.ConcreteSchemas"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemas ConcreteSchemas {
            get { return importer.ConcreteSchemas; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.CodeNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeNamespace CodeNamespace {
            get { return codeNamespace; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.CodeTypeDeclaration"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeTypeDeclaration CodeTypeDeclaration {
            get { return codeClass; }
        }

        internal CodeTypeDeclarationCollection ExtraCodeClasses {
            get {
                if (classes == null)
                    classes = new CodeTypeDeclarationCollection();
                return classes;
            }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Style"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionImportStyle Style {
            get { return importer.Style; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Warnings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionImportWarnings Warnings {
            get { return warnings; }
            set { warnings = value; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.ClassNames"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeIdentifiers ClassNames {
            get { return importContext.TypeIdentifiers; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.MethodName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string MethodName {
            get {
                // We don't attempt to make this unique because of method overloading
                return CodeIdentifier.MakeValid(XmlConvert.DecodeName(Operation.Name)); 
            }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.ClassName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ClassName {
            get { return className; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Port"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Port Port {
            get { return port; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.PortType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PortType PortType {
            get { return portType; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Binding Binding {
            get { return binding; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Service"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Service Service {
            get { return service; }
        }

        internal ServiceDescriptionImporter ServiceImporter {
            get { return importer; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.Operation"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Operation Operation {
            get { return operation; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.OperationBinding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationBinding OperationBinding {
            get { return operationBinding; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.InputMessage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message InputMessage {
            get { return inputMessage; }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.OutputMessage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message OutputMessage {
            get { return outputMessage; }
        }

        internal ImportContext ImportContext {
            get { return importContext; }
        }

        internal bool IsEncodedBinding {
            get { return encodedBinding; }
            set { encodedBinding = value; }
        }
        
        internal Hashtable ExportContext {
            get { 
                if (exportContext == null)
                    exportContext = new Hashtable();
                return exportContext; 
            }
        }

        internal CodeIdentifiers MethodNames {
            get { 
                if (methodNames == null)
                    methodNames = new CodeIdentifiers();
                return methodNames; 
            }
        }

        internal bool GenerateCode(CodeNamespace codeNamespace, ImportContext importContext, Hashtable exportContext) {
            bindingCount = 0;
            anyPorts = false;

            this.codeNamespace = codeNamespace;

            Hashtable supportedBindings = new Hashtable();
            Hashtable unsupportedBindings = new Hashtable();

            // look for ports with bindings
            foreach (ServiceDescription serviceDescription in ServiceDescriptions) {
                foreach (Service service in serviceDescription.Services) {
                    foreach (Port port in service.Ports) {
                        Binding binding = ServiceDescriptions.GetBinding(port.Binding);
                        if (supportedBindings.Contains(binding))
                            continue;
                        PortType portType = ServiceDescriptions.GetPortType(binding.Type);
                        MoveToBinding(service, port, binding, portType);
                        if (IsBindingSupported()) {
                            bindingCount++;
                            anyPorts = true;
                            supportedBindings.Add(binding, binding);
                        }
                        else if (binding != null) unsupportedBindings[binding] = binding;
                    }
                }
            }

            // no ports, look for bindings
            if (bindingCount == 0) {
                foreach (ServiceDescription serviceDescription in ServiceDescriptions) {
                    foreach (Binding binding in serviceDescription.Bindings) {
                        if (unsupportedBindings.Contains(binding)) continue;
                        PortType portType = ServiceDescriptions.GetPortType(binding.Type);
                        MoveToBinding(binding, portType);
                        if (IsBindingSupported()) {
                            bindingCount++;
                        }
                    }
                }
            }

            // give up if no bindings
            if (bindingCount == 0) {
                // if we generated comments return true so that the comments get written
                return codeNamespace.Comments.Count > 0;
            }

            this.importContext = importContext;
            this.exportContext = exportContext;
            BeginNamespace();

            supportedBindings.Clear();
            foreach (ServiceDescription serviceDescription in ServiceDescriptions) {
                if (anyPorts) {
                    foreach (Service service in serviceDescription.Services) {
                        foreach (Port port in service.Ports) {
                            Binding binding = ServiceDescriptions.GetBinding(port.Binding);
                            PortType portType = ServiceDescriptions.GetPortType(binding.Type);
                            MoveToBinding(service, port, binding, portType);
                            if (IsBindingSupported() && !supportedBindings.Contains(binding)) {
                                GenerateClassForBinding();
                                supportedBindings.Add(binding, binding);
                            }
                        }
                    }
                }
                else {
                    foreach (Binding binding in serviceDescription.Bindings) {
                        PortType portType = ServiceDescriptions.GetPortType(binding.Type);
                        MoveToBinding(binding, portType);
                        if (IsBindingSupported()) {
                            GenerateClassForBinding();
                        }
                    }
                }
            }

            EndNamespace();
            return true;
        }

        void MoveToBinding(Binding binding, PortType portType) {
            MoveToBinding(null, null, binding, portType);
        }

        void MoveToBinding(Service service, Port port, Binding binding, PortType portType) {
            this.service = service;
            this.port = port;
            this.portType = portType;
            this.binding = binding;
            this.encodedBinding = false;
        }

        void MoveToOperation(Operation operation) {
            this.operation = operation;

            operationBinding = null;
            foreach (OperationBinding b in binding.Operations) {
                if (operation.IsBoundBy(b)) {
                    if (operationBinding != null) throw OperationSyntaxException(Res.GetString(Res.DuplicateInputOutputNames0));
                    operationBinding = b;
                }
            }
            if (operationBinding == null) {
                throw OperationSyntaxException(Res.GetString(Res.MissingBinding0));
            }
            //NOTE: The following two excepions would never happen since IsBoundBy checks these conditions already.
            if (operation.Messages.Input != null && operationBinding.Input == null) {
                throw OperationSyntaxException(Res.GetString(Res.MissingInputBinding0));
            }
            if (operation.Messages.Output != null && operationBinding.Output == null) {
                throw OperationSyntaxException(Res.GetString(Res.MissingOutputBinding0));
            }

            this.inputMessage = operation.Messages.Input == null ? null : ServiceDescriptions.GetMessage(operation.Messages.Input.Message);
            this.outputMessage = operation.Messages.Output == null ? null : ServiceDescriptions.GetMessage(operation.Messages.Output.Message);
        }

        void GenerateClassForBinding() {
            try {
                if (bindingCount == 1 && service != null && Style != ServiceDescriptionImportStyle.ServerInterface) {
                    // If there is only one binding, then use the name of the service
                    className = XmlConvert.DecodeName(service.Name);
                }
                else {
                    // If multiple bindings, then use the name of the binding
                    className = binding.Name;
                    if (Style == ServiceDescriptionImportStyle.ServerInterface)
                    {
                        // append "I" if we are generating interfaces
                        className = "I" + CodeIdentifier.MakePascal(className);
                    }
                }
                className = XmlConvert.DecodeName(className);
                className = ClassNames.AddUnique(CodeIdentifier.MakeValid(className), null);
                this.codeClass = BeginClass();
                int methodCount = 0;
                for (int i = 0; i < portType.Operations.Count; i++) {
                    MoveToOperation(portType.Operations[i]);

                    if (!IsOperationFlowSupported(operation.Messages.Flow)) {
                        // 
                        switch (operation.Messages.Flow) {
                            case OperationFlow.SolicitResponse:
                                UnsupportedOperationWarning(Res.GetString(Res.SolicitResponseIsNotSupported0));
                                continue;
                            case OperationFlow.RequestResponse:
                                UnsupportedOperationWarning(Res.GetString(Res.RequestResponseIsNotSupported0));
                                continue;
                            case OperationFlow.OneWay:
                                UnsupportedOperationWarning(Res.GetString(Res.OneWayIsNotSupported0));
                                continue;
                            case OperationFlow.Notification:
                                UnsupportedOperationWarning(Res.GetString(Res.NotificationIsNotSupported0));
                                continue;
                        }
                    }

                    CodeMemberMethod method;
                    try {
                        method = GenerateMethod();
                    }
                    catch (Exception e) {
                        if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                            throw;
                        }
                        throw new InvalidOperationException(Res.GetString(Res.UnableToImportOperation1, operation.Name), e);
                    }

                    if (method != null) {
                        AddExtensionWarningComments(codeClass.Comments, operationBinding.Extensions);
                        if (operationBinding.Input != null) AddExtensionWarningComments(codeClass.Comments, operationBinding.Input.Extensions);
                        if (operationBinding.Output != null) AddExtensionWarningComments(codeClass.Comments, operationBinding.Output.Extensions);
                        methodCount++;
                    }
                }
                bool newAsync = (ServiceImporter.CodeGenerationOptions & CodeGenerationOptions.GenerateNewAsync) != 0 && 
                    ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareEvents) && 
                    ServiceImporter.CodeGenerator.Supports(GeneratorSupport.DeclareDelegates);
                if (newAsync && methodCount > 0 && Style == ServiceDescriptionImportStyle.Client) {
                    CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
                    string cancelAsync = "CancelAsync";
                    string cancelMethodName = MethodNames.AddUnique(cancelAsync, cancelAsync);
                    CodeMemberMethod asyncCancelMethod = WebCodeGenerator.AddMethod(this.CodeTypeDeclaration, cancelMethodName, 
                        new CodeFlags[1], new string[] { typeof(object).FullName }, new string[] { "userState" }, 
                        typeof(void).FullName,
                        metadata, 
                        CodeFlags.IsPublic | (cancelAsync != cancelMethodName ? 0 : CodeFlags.IsNew));

                    asyncCancelMethod.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
                    CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), cancelAsync);
                    invoke.Parameters.Add(new CodeArgumentReferenceExpression("userState"));
                    asyncCancelMethod.Statements.Add(invoke);
                }

                EndClass();

                if (portType.Operations.Count == 0)
                    NoMethodsGeneratedWarning();

                AddExtensionWarningComments(codeClass.Comments, binding.Extensions);
                if (port != null) AddExtensionWarningComments(codeClass.Comments, port.Extensions);

                codeNamespace.Types.Add(codeClass);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                throw new InvalidOperationException(Res.GetString(Res.UnableToImportBindingFromNamespace2, binding.Name, binding.ServiceDescription.TargetNamespace), e);
            }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.AddExtensionWarningComments"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddExtensionWarningComments(CodeCommentStatementCollection comments, ServiceDescriptionFormatExtensionCollection extensions) {
            foreach (object item in extensions) {
                if (!extensions.IsHandled(item)) {
                    string name = null;
                    string ns = null;
                    if (item is XmlElement) {
                        XmlElement element = (XmlElement)item;
                        name = element.LocalName;
                        ns = element.NamespaceURI;
                    }
                    else if (item is ServiceDescriptionFormatExtension) {
                        XmlFormatExtensionAttribute[] attrs = (XmlFormatExtensionAttribute[])item.GetType().GetCustomAttributes(typeof(XmlFormatExtensionAttribute), false);
                        if (attrs.Length > 0) {
                            name = attrs[0].ElementName;
                            ns = attrs[0].Namespace;
                        }
                    }
                    if (name != null) {
                        if (extensions.IsRequired(item)) {
                            warnings |= ServiceDescriptionImportWarnings.RequiredExtensionsIgnored;
                            AddWarningComment(comments, Res.GetString(Res.WebServiceDescriptionIgnoredRequired, name, ns));
                        }
                        else {
                            warnings |= ServiceDescriptionImportWarnings.OptionalExtensionsIgnored;
                            AddWarningComment(comments, Res.GetString(Res.WebServiceDescriptionIgnoredOptional, name, ns));
                        }
                    }
                }
            }
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.UnsupportedBindingWarning"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void UnsupportedBindingWarning(string text) {
            AddWarningComment(codeClass == null ? codeNamespace.Comments : codeClass.Comments, Res.GetString(Res.TheBinding0FromNamespace1WasIgnored2, Binding.Name, Binding.ServiceDescription.TargetNamespace, text));
            warnings |= ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored;
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.UnsupportedOperationWarning"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void UnsupportedOperationWarning(string text) {
            AddWarningComment(codeClass == null ? codeNamespace.Comments : codeClass.Comments, Res.GetString(Res.TheOperation0FromNamespace1WasIgnored2, operation.Name, operation.PortType.ServiceDescription.TargetNamespace, text));
            warnings |= ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored;
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.UnsupportedOperationBindingWarning"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void UnsupportedOperationBindingWarning(string text) {
            AddWarningComment(codeClass == null ? codeNamespace.Comments : codeClass.Comments, Res.GetString(Res.TheOperationBinding0FromNamespace1WasIgnored, operationBinding.Name, operationBinding.Binding.ServiceDescription.TargetNamespace, text));
            warnings |= ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored;
        }

        void NoMethodsGeneratedWarning() {
            AddWarningComment(codeClass.Comments, Res.GetString(Res.NoMethodsWereFoundInTheWSDLForThisProtocol));
            warnings |= ServiceDescriptionImportWarnings.NoMethodsGenerated;
        }

        internal static void AddWarningComment(CodeCommentStatementCollection comments, string text) {
            comments.Add(new CodeCommentStatement(Res.GetString(Res.CodegenWarningDetails, text)));
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.OperationSyntaxException"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Exception OperationSyntaxException(string text) {
            return new Exception(Res.GetString(Res.TheOperationFromNamespaceHadInvalidSyntax3,
                                               operation.Name,
                                               operation.PortType.Name,
                                               operation.PortType.ServiceDescription.TargetNamespace,
                                               text));
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.OperationBindingSyntaxException"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Exception OperationBindingSyntaxException(string text) {
            return new Exception(Res.GetString(Res.TheOperationBindingFromNamespaceHadInvalid3, operationBinding.Name, operationBinding.Binding.ServiceDescription.TargetNamespace, text));
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.ProtocolName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract string ProtocolName { get; }

        // These overridable methods have no parameters.  The subclass uses properties on this
        // base object to obtain the information.  This allows us to grow the set of
        // information passed to the methods over time w/o breaking anyone.   They are protected
        // instead of public because this object is passed to extensions and we don't want
        // those calling these methods.
        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.BeginNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void BeginNamespace() {
            MethodNames.Clear();
        }

        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.IsBindingSupported"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract bool IsBindingSupported();
        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.IsOperationFlowSupported"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract bool IsOperationFlowSupported(OperationFlow flow);
        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.BeginClass"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract CodeTypeDeclaration BeginClass();
        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.GenerateMethod"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract CodeMemberMethod GenerateMethod();
        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.EndClass"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void EndClass() { }
        /// <include file='doc\ProtocolImporter.uex' path='docs/doc[@for="ProtocolImporter.EndNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void EndNamespace() { 
            if (classes != null) {
                foreach (CodeTypeDeclaration declaration in classes) {
                    codeNamespace.Types.Add(declaration);
                }
            }
            CodeGenerator.ValidateIdentifiers(codeNamespace);
        }

        internal static string UniqueName(string baseName, string[] scope) {
            CodeIdentifiers identifiers = new CodeIdentifiers();
            for (int i = 0; i < scope.Length; i++) {
                identifiers.AddUnique(scope[i], scope[i]);
            }
            return identifiers.AddUnique(baseName, baseName);
        }
        internal static string MethodSignature(string methodName, string returnType, CodeFlags[] parameterFlags, string[] parameterTypes) {
            Debug.Assert(parameterFlags.Length == parameterTypes.Length, "parameterFlags.Length !=  parameterTypes.Length");
            StringBuilder sb = new StringBuilder();
            sb.Append(returnType);
            sb.Append(" ");
            sb.Append(methodName);
            sb.Append(" (");
            for (int i = 0; i < parameterTypes.Length; i++) {
                if ((parameterFlags[i] & CodeFlags.IsByRef) != 0)
                    sb.Append("ref ");
                else if ((parameterFlags[i] & CodeFlags.IsOut) != 0)
                    sb.Append("out ");

                sb.Append(parameterTypes[i]);
                if (i > 0)
                    sb.Append(",");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }

    internal class ProtocolImporterUtil {
        private ProtocolImporterUtil() { }

        internal static void GenerateConstructorStatements(CodeConstructor ctor, string url, string appSettingUrlKey, string appSettingBaseUrl, bool soap11) {
            CodeExpression value; 
            bool generateFixedUrlAssignment = (url != null && url.Length > 0);
            bool generateConfigUrlAssignment = appSettingUrlKey != null && appSettingUrlKey.Length > 0;
            CodeAssignStatement assignUrlStatement = null;

            if (!generateFixedUrlAssignment && !generateConfigUrlAssignment)
                return;

            // this.Url property
            CodePropertyReferenceExpression urlPropertyReference = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Url");

            if (generateFixedUrlAssignment) {
                value = new CodePrimitiveExpression(url);
                assignUrlStatement = new CodeAssignStatement(urlPropertyReference, value);
            }

            if (generateFixedUrlAssignment && !generateConfigUrlAssignment) {
                ctor.Statements.Add(assignUrlStatement);
            }
            else if (generateConfigUrlAssignment) {
                // urlSetting local variable
                CodeVariableReferenceExpression urlSettingReference = new CodeVariableReferenceExpression("urlSetting");                

                // Generate: string urlSetting = System.Configuration.ConfigurationManager.AppSettings["<appSettingUrlKey>"];
                CodeTypeReferenceExpression codeTypeReference = new CodeTypeReferenceExpression(typeof(ConfigurationManager));
                CodePropertyReferenceExpression propertyReference = new CodePropertyReferenceExpression(codeTypeReference, "AppSettings");
                value = new CodeIndexerExpression(propertyReference, new CodeExpression[] { new CodePrimitiveExpression(appSettingUrlKey) });
                ctor.Statements.Add(new CodeVariableDeclarationStatement(typeof(string), "urlSetting", value));

                if (appSettingBaseUrl == null || appSettingBaseUrl.Length == 0) {
                    // Generate: this.Url = urlSetting;
                    value = urlSettingReference;
                }
                else {
                    // Generate: this.Url = "http://localhost/mywebapplication/simple.asmx";
                    if (url == null || url.Length == 0)
                        throw new ArgumentException(Res.GetString(Res.IfAppSettingBaseUrlArgumentIsSpecifiedThen0));
                    string relativeUrl = new Uri(appSettingBaseUrl).MakeRelative(new Uri(url));
                    CodeExpression[] parameters = new CodeExpression[] { urlSettingReference, new CodePrimitiveExpression(relativeUrl) };
                    value = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(System.String)), "Concat", parameters);                
                }
                CodeStatement[] trueStatements = new CodeStatement[] { new CodeAssignStatement(urlPropertyReference, value) };        

                // Generate: if (urlSetting != null) { <truestatement> } else { <falsestatement> }
                CodeBinaryOperatorExpression checkIfNull = new CodeBinaryOperatorExpression(urlSettingReference, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                if (generateFixedUrlAssignment)
                    ctor.Statements.Add(new CodeConditionStatement(checkIfNull, trueStatements, new CodeStatement[] { assignUrlStatement }));
                else
                    ctor.Statements.Add(new CodeConditionStatement(checkIfNull, trueStatements));
            }
        }
    }

    internal class DelegateInfo {
        internal string handlerType;
        internal string handlerArgs;

        internal DelegateInfo(string handlerType, string handlerArgs) {
            this.handlerType = handlerType;
            this.handlerArgs = handlerArgs;
        }
    }
}
