// 
// System.Web.Services.Description.ProtocolImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;
using System.Web.Services;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	public abstract class ProtocolImporter {

		#region Fields

		XmlSchemas abstractSchemas;
		Binding binding;
		string className;
		CodeIdentifiers classNames;
		CodeNamespace codeNamespace;
		CodeTypeDeclaration codeTypeDeclaration;
		XmlSchemas concreteSchemas;
		Message inputMessage;
		string methodName;
		Operation operation;
		OperationBinding operationBinding;
		Message outputMessage;		
		Port port;
		PortType portType;
		string protocolName;
		XmlSchemas schemas;
		Service service;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceDescriptionImportStyle style;
		ServiceDescriptionImportWarnings warnings;	

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]	
		protected ProtocolImporter ()
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public XmlSchemas AbstractSchemas {
			get { return abstractSchemas; }
		}

		public Binding Binding {
			get { return binding; }
		}

		public string ClassName {
			get { return className; }
		}

		public CodeIdentifiers ClassNames {
			get { return classNames; }
		}

		public CodeNamespace CodeNamespace {
			get { return codeNamespace; }
		}

		public CodeTypeDeclaration CodeTypeDeclaration {
			get { return codeTypeDeclaration; }
		}

		public XmlSchemas ConcreteSchemas {
			get { return concreteSchemas; }
		}

		public Message InputMessage {
			get { return inputMessage; }
		}

		public string MethodName {
			get { return methodName; }
		}

		public Operation Operation {
			get { return operation; }
		}

		public OperationBinding OperationBinding {
			get { return operationBinding; }
		}

		public Message OutputMessage {
			get { return outputMessage; }
		}

		public Port Port {
			get { return port; }
		}

		public PortType PortType {
			get { return portType; }
		}

		public abstract string ProtocolName {
			get; 
		}

		public XmlSchemas Schemas {
			get { return schemas; }
		}

		public Service Service {
			get { return service; } 
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}

		public ServiceDescriptionImportStyle Style {
			get { return style; }
		}

		public ServiceDescriptionImportWarnings Warnings {
			get { return warnings; }
			set { warnings = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void AddExtensionWarningComments (CodeCommentStatementCollection comments, ServiceDescriptionFormatExtensionCollection extensions) 
		{
			throw new NotImplementedException ();
		}

		protected abstract CodeTypeDeclaration BeginClass ();

		[MonoTODO]
		protected virtual void BeginNamespace ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void EndClass ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void EndNamespace ()
		{
			throw new NotImplementedException ();
		}

		protected abstract CodeMemberMethod GenerateMethod ();
		protected abstract bool IsBindingSupported ();
		protected abstract bool IsOperationFlowSupported (OperationFlow flow);
		
		[MonoTODO]
		public Exception OperationBindingSyntaxException (string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Exception OperationSyntaxException (string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UnsupportedBindingWarning (string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UnsupportedOperationBindingWarning (string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UnsupportedOperationWarning (string text)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
