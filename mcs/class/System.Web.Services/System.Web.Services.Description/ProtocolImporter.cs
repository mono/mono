// 
// System.Web.Services.Description.ProtocolImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.CodeDom;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Configuration;

namespace System.Web.Services.Description {
	public abstract class ProtocolImporter {

		#region Fields

		Binding binding;
		string className;
		CodeIdentifiers classNames;
		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		CodeTypeDeclaration codeTypeDeclaration;
		Message inputMessage;
		string methodName;
		Operation operation;
		OperationBinding operationBinding;
		Message outputMessage;		
		Port port;
		PortType portType;
		string protocolName;
		Service service;
		ServiceDescriptionImportWarnings warnings = (ServiceDescriptionImportWarnings)0;	
		ServiceDescriptionImporter descriptionImporter;
		ImportInfo iinfo;

		#endregion // Fields

		#region Constructors
	
		protected ProtocolImporter ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public XmlSchemas AbstractSchemas {
			get { return descriptionImporter.Schemas; }
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

		[MonoTODO]
		public XmlSchemas ConcreteSchemas {
			get { return descriptionImporter.Schemas; }
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
			get { return descriptionImporter.Schemas; }
		}

		public Service Service {
			get { return service; } 
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return descriptionImporter.ServiceDescriptions; }
		}

		public ServiceDescriptionImportStyle Style {
			get { return descriptionImporter.Style; }
		}

		public ServiceDescriptionImportWarnings Warnings {
			get { return warnings; }
			set { warnings = value; }
		}
		
		internal ImportInfo ImportInfo
		{
			get { return iinfo; }
		}

		#endregion // Properties

		#region Methods
		
		internal bool Import (ServiceDescriptionImporter descriptionImporter, CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit, ArrayList importInfo)
		{
			this.descriptionImporter = descriptionImporter;
			this.classNames = new CodeIdentifiers();;
			this.codeNamespace = codeNamespace;
			this.codeCompileUnit = codeCompileUnit;
			
			warnings = (ServiceDescriptionImportWarnings) 0;
			
			bool found = false;

			BeginNamespace ();
			
			foreach (ImportInfo info in importInfo)
			{
				foreach (Service service in info.ServiceDescription.Services)
				{
					this.service = service;
					foreach (Port port in service.Ports)
					{
						this.iinfo = info;
						this.port = port;
						binding = ServiceDescriptions.GetBinding (port.Binding);
						if (!IsBindingSupported ()) continue;
						
						found = true;
						ImportPortBinding ();
					}
				}
			}

			EndNamespace ();
			
			return true;
		}

		void ImportPortBinding ()
		{
			if (service.Ports.Count > 1) className = port.Name;
			else className = service.Name;
			
			className = classNames.AddUnique (CodeIdentifier.MakeValid (className), port);
			
			try
			{
				portType = ServiceDescriptions.GetPortType (binding.Type);
				if (portType == null) throw new Exception ("Port type not found: " + binding.Type);

				CodeTypeDeclaration codeClass = BeginClass ();
				codeTypeDeclaration = codeClass;
				AddCodeType (codeClass, port.Documentation);
				codeClass.Attributes = MemberAttributes.Public;
				
				if (service.Documentation != null && service.Documentation != "")
					AddComments (codeClass, service.Documentation);

				CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Diagnostics.DebuggerStepThroughAttribute");
				AddCustomAttribute (codeClass, att, true);

				att = new CodeAttributeDeclaration ("System.ComponentModel.DesignerCategoryAttribute");
				att.Arguments.Add (GetArg ("code"));
				AddCustomAttribute (codeClass, att, true);

				if (binding.Operations.Count == 0) {
					warnings |= ServiceDescriptionImportWarnings.NoMethodsGenerated;
					return;
				}
				
				foreach (OperationBinding oper in binding.Operations) 
				{
					operationBinding = oper;
					operation = FindPortOperation ();
					if (operation == null) throw new Exception ("Operation " + operationBinding.Name + " not found in portType " + PortType.Name);

					foreach (OperationMessage omsg in operation.Messages)
					{
						Message msg = ServiceDescriptions.GetMessage (omsg.Message);
						if (msg == null) throw new Exception ("Message not found: " + omsg.Message);
						
						if (omsg is OperationInput)
							inputMessage = msg;
						else
							outputMessage = msg;
					}
					
					CodeMemberMethod method = GenerateMethod ();
					
					if (method != null)
					{
						methodName = method.Name;
						if (operation.Documentation != null && operation.Documentation != "")
							AddComments (method, operation.Documentation);
					}
				}
				
				EndClass ();
			}
			catch (InvalidOperationException ex)
			{
				warnings |= ServiceDescriptionImportWarnings.NoCodeGenerated;
				UnsupportedBindingWarning (ex.Message);
			}
		}

		Operation FindPortOperation ()
		{
			string inMessage = null;
			string outMessage = null;
			int numMsg = 1;
			
			if (operationBinding.Input == null) throw new InvalidOperationException ("Input operation binding not found");
			inMessage = (operationBinding.Input.Name != null) ? operationBinding.Input.Name : operationBinding.Name;
				
			if (operationBinding.Output != null) {
				outMessage = (operationBinding.Output.Name != null) ? operationBinding.Output.Name : operationBinding.Name;
				numMsg++;
			}
			
			string operName = operationBinding.Name;
			
			Operation foundOper = null;
			foreach (Operation oper in PortType.Operations)
			{
				if (oper.Name == operName)
				{
					int hits = 0;
					foreach (OperationMessage omsg in oper.Messages)
					{
						if (omsg is OperationInput && GetOperMessageName (omsg, operName) == inMessage) hits++;
						if (omsg is OperationOutput && GetOperMessageName (omsg, operName) == outMessage) hits++;
					}
					if (hits == numMsg) return oper;
					foundOper = oper;
				}
			}
			return foundOper;
		}
		
		string GetOperMessageName (OperationMessage msg, string operName)
		{
			if (msg.Name == null) return operName;
			else return msg.Name;
		}
		
		internal string GetServiceUrl (string location)
		{
			if (ImportInfo.AppSettingUrlKey == null || ImportInfo.AppSettingUrlKey == string.Empty)
				return location;
			else
			{
				string url;
				if (Style == ServiceDescriptionImportStyle.Server) throw new InvalidOperationException ("Cannot set appSettingUrlKey if Style is Server");
				url = ConfigurationSettings.AppSettings [ImportInfo.AppSettingUrlKey];
				if (ImportInfo.AppSettingBaseUrl != null && ImportInfo.AppSettingBaseUrl != string.Empty)
					url += "/" + ImportInfo.AppSettingBaseUrl + "/" + location;
				return url;
			}
		}

		
		[MonoTODO]
		public void AddExtensionWarningComments (CodeCommentStatementCollection comments, ServiceDescriptionFormatExtensionCollection extensions) 
		{
			throw new NotImplementedException ();
		}

		protected abstract CodeTypeDeclaration BeginClass ();

		protected virtual void BeginNamespace ()
		{
		}

		protected virtual void EndClass ()
		{
		}

		protected virtual void EndNamespace ()
		{
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

		public void UnsupportedBindingWarning (string text)
		{
			warnings |= ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored;
			AddGlobalComments ("WARNING: Could not generate proxy for binding " + binding.Name + ". " + text);
		}

		public void UnsupportedOperationBindingWarning (string text)
		{
			warnings |= ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored;
			AddGlobalComments ("WARNING: Could not generate operation " + OperationBinding.Name + ". " + text);
		}

		public void UnsupportedOperationWarning (string text)
		{
			warnings |= ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored;
			AddGlobalComments ("WARNING: Could not generate operation " + OperationBinding.Name + ". " + text);
		}

		void AddGlobalComments (string comments)
		{
			codeNamespace.Comments.Add (new CodeCommentStatement (comments, false));
		}

		void AddComments (CodeTypeMember member, string comments)
		{
			if (comments == null || comments == "") member.Comments.Add (new CodeCommentStatement ("<remarks/>", true));
			else member.Comments.Add (new CodeCommentStatement ("<remarks>\n" + comments + "\n</remarks>", true));
		}

		void AddCodeType (CodeTypeDeclaration type, string comments)
		{
			AddComments (type, comments);
			codeNamespace.Types.Add (type);
		}

		internal void AddCustomAttribute (CodeTypeMember ctm, CodeAttributeDeclaration att, bool addIfNoParams)
		{
			if (att.Arguments.Count == 0 && !addIfNoParams) return;
			
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (att);
		}

		internal void AddCustomAttribute (CodeTypeMember ctm, string name, params CodeAttributeArgument[] args)
		{
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (new CodeAttributeDeclaration (name, args));
		}

		internal CodeAttributeArgument GetArg (string name, object value)
		{
			return new CodeAttributeArgument (name, new CodePrimitiveExpression(value));
		}

		internal CodeAttributeArgument GetEnumArg (string name, string enumType, string enumValue)
		{
			return new CodeAttributeArgument (name, new CodeFieldReferenceExpression (new CodeTypeReferenceExpression(enumType), enumValue));
		}

		internal CodeAttributeArgument GetArg (object value)
		{
			return new CodeAttributeArgument (new CodePrimitiveExpression(value));
		}

		internal CodeAttributeArgument GetTypeArg (string name, string typeName)
		{
			return new CodeAttributeArgument (name, new CodeTypeOfExpression(typeName));
		}
		
		#endregion
	}
}
