// 
// System.Web.Services.Description.ProtocolImporter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.CodeDom;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
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
		XmlSchemas xmlSchemas;
		XmlSchemas soapSchemas;

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
		
		internal XmlSchemas LiteralSchemas
		{
			get { return xmlSchemas; }
		}
		
		internal XmlSchemas EncodedSchemas
		{
			get { return soapSchemas; }
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
			
			ClasifySchemas (importInfo);

			BeginNamespace ();
			
			foreach (ImportInfo info in importInfo)
			{
				foreach (Service service in info.ServiceDescription.Services)
				{
					this.service = service;
					int bindingCount = 0;
					foreach (Port port in service.Ports)
					{
						binding = ServiceDescriptions.GetBinding (port.Binding);
						if (IsBindingSupported ()) bindingCount ++;
					}
					
					foreach (Port port in service.Ports)
					{
						this.iinfo = info;
						this.port = port;
						binding = ServiceDescriptions.GetBinding (port.Binding);
						if (!IsBindingSupported ()) continue;
						
						found = true;
						ImportPortBinding (bindingCount > 1);
					}
				}
			}

			EndNamespace ();
			
			if (!found) warnings = ServiceDescriptionImportWarnings.NoCodeGenerated;
			return true;
		}

		void ImportPortBinding (bool multipleBindings)
		{
			if (multipleBindings) className = port.Name;
			else className = service.Name;
			
			className = classNames.AddUnique (CodeIdentifier.MakeValid (className), port);
			className = className.Replace ("_x0020_", "");	// MS.NET seems to do this
			
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
		
		internal void GenerateServiceUrl (string location, CodeStatementCollection stms)
		{
			if (ImportInfo.AppSettingUrlKey == null || ImportInfo.AppSettingUrlKey == string.Empty) {
				if (location != null) {
					CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
					CodeAssignStatement cas = new CodeAssignStatement (ce, new CodePrimitiveExpression (location));
					stms.Add (cas);
				}
			}
			else
			{
				CodeExpression prop = new CodePropertyReferenceExpression (new CodeTypeReferenceExpression ("System.Configuration.ConfigurationSettings"), "AppSettings");
				prop = new CodeIndexerExpression (prop, new CodePrimitiveExpression (ImportInfo.AppSettingUrlKey));
				stms.Add (new CodeVariableDeclarationStatement (typeof(string), "urlSetting", prop));
				
				CodeExpression urlSetting = new CodeVariableReferenceExpression ("urlSetting");
				CodeExpression thisUrl = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
				
				CodeStatement[] trueStms = new CodeStatement [1];
				CodeExpression ce = urlSetting;
				CodeExpression cond = new CodeBinaryOperatorExpression (urlSetting, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression (null));
				
				if (ImportInfo.AppSettingBaseUrl != null)
					ce = new CodeMethodInvokeExpression (new CodeTypeReferenceExpression (typeof(string)), "Concat", ce, new CodePrimitiveExpression (ImportInfo.AppSettingBaseUrl));
				trueStms [0] = new CodeAssignStatement (thisUrl, ce);
				
				if (location != null) {
					CodeStatement[] falseStms = new CodeStatement [1];
					falseStms [0] = new CodeAssignStatement (thisUrl, new CodePrimitiveExpression (location));
					stms.Add (new CodeConditionStatement (cond, trueStms, falseStms));
				}
				else
					stms.Add (new CodeConditionStatement (cond, trueStms));
			}
		}
		
		void ClasifySchemas (ArrayList importInfo)
		{
			// I don't like this, but I could not find any other way of clasifying
			// schemas between encoded and literal.
			
			xmlSchemas = new XmlSchemas ();
			soapSchemas = new XmlSchemas ();
			
			foreach (ImportInfo info in importInfo)
			{
				foreach (Service service in info.ServiceDescription.Services)
				{
					foreach (Port port in service.Ports)
					{
						this.iinfo = info;
						this.port = port;
						binding = ServiceDescriptions.GetBinding (port.Binding);
						if (binding == null) continue;
						portType = ServiceDescriptions.GetPortType (binding.Type);
						if (portType == null) continue;
						
						foreach (OperationBinding oper in binding.Operations) 
						{
							operationBinding = oper;
							operation = FindPortOperation ();
							if (operation == null) continue;
		
							foreach (OperationMessage omsg in operation.Messages)
							{
								Message msg = ServiceDescriptions.GetMessage (omsg.Message);
								if (msg == null) continue;
								
								if (omsg is OperationInput)
									inputMessage = msg;
								else
									outputMessage = msg;
							}
							
							if (GetMessageEncoding (oper.Input) == SoapBindingUse.Encoded)
								AddMessageSchema (soapSchemas, oper.Input, inputMessage);
							else
								AddMessageSchema (xmlSchemas, oper.Input, inputMessage);
							
							if (oper.Output != null) {
								if (GetMessageEncoding (oper.Output) == SoapBindingUse.Encoded)
									AddMessageSchema (soapSchemas, oper.Output, outputMessage);
								else
									AddMessageSchema (xmlSchemas, oper.Output, outputMessage);
							}
						}
					}
				}
			}
			
			XmlSchemas defaultList = xmlSchemas;
				
			if (xmlSchemas.Count == 0 && soapSchemas.Count > 0)
				defaultList = soapSchemas;
				
			// Schemas not referenced by any message
			foreach (XmlSchema sc in Schemas)
			{
				if (!soapSchemas.Contains (sc) && !xmlSchemas.Contains (sc)) {
					if (ImportsEncodedNamespace (sc))
						soapSchemas.Add (sc);
					else
						defaultList.Add (sc);
				}
			}
		}
			
		void AddMessageSchema (XmlSchemas schemas, MessageBinding mb, Message msg)
		{
			foreach (MessagePart part in msg.Parts)
			{
				if (part.Element != XmlQualifiedName.Empty)
					AddIncludingSchema (schemas, part.Element.Namespace);
				else if (part.Type != XmlQualifiedName.Empty)
					AddIncludingSchema (schemas, part.Type.Namespace);
			}
			SoapBodyBinding sbb = mb.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
			if (sbb != null) AddIncludingSchema (schemas, sbb.Namespace);
		}
		
		void AddIncludingSchema (XmlSchemas list, string ns)
		{
			XmlSchema sc = Schemas [ns];
			if (sc == null || list.Contains (sc)) return;
			list.Add (sc);
			foreach (XmlSchemaObject ob in sc.Includes)
			{
				XmlSchemaImport import = ob as XmlSchemaImport;
				if (import != null) AddIncludingSchema (list, import.Namespace);
			}
		}
		
		SoapBindingUse GetMessageEncoding (MessageBinding mb)
		{
			SoapBodyBinding sbb = mb.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
			if (sbb == null)
			{
				if (mb is InputBinding) return SoapBindingUse.Encoded;
				else return SoapBindingUse.Literal;
			}
			else 
				if (sbb.Use == SoapBindingUse.Encoded) return SoapBindingUse.Encoded;
			else
				return SoapBindingUse.Literal;
		}
		
		bool ImportsEncodedNamespace (XmlSchema sc)
		{
			foreach (XmlSchemaObject ob in sc.Includes)
			{
				XmlSchemaImport import = ob as XmlSchemaImport;
				if (import.Namespace == SoapProtocolReflector.EncodingNamespace) return true;
			}
			return false;
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
