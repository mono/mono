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
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public Binding Binding {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string ClassName {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public CodeIdentifiers ClassNames {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public CodeNamespace CodeNamespace {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public CodeTypeDeclaration CodeTypeDeclaration {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public XmlSchemas ConcreteSchemas {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Message InputMessage {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public string MethodName {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Operation Operation {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public OperationBinding OperationBinding {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Message OutputMessage {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Port Port {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public PortType PortType {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public abstract string ProtocolName {
			get; 
		}

		public XmlSchemas Schemas {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public Service Service {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public ServiceDescriptionImportStyle Style {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
		}

		public ServiceDescriptionImportWarnings Warnings {
			[MonoTODO]	
			get { throw new NotImplementedException (); }
			[MonoTODO]	
			set { throw new NotImplementedException (); }
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
