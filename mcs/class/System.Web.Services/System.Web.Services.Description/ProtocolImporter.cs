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
using System.CodeDom.Compiler;
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
		
#if NET_2_0
		ArrayList asyncTypes = new ArrayList ();
#endif

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
		
#if NET_2_0
		internal CodeGenerationOptions CodeGenerationOptions {
			get { return descriptionImporter.CodeGenerationOptions; }
		}
		
		internal ICodeGenerator CodeGenerator {
			get { return descriptionImporter.CodeGenerator; }
		}

		internal ImportContext ImportContext {
			get { return descriptionImporter.Context; }
		}
#endif

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
			
			if (!found)
			{
				// Looks like MS.NET generates classes for all bindings if
				// no services are present
				
				foreach (ImportInfo info in importInfo)
				{
					this.iinfo = info;
					foreach (Binding b in info.ServiceDescription.Bindings)
					{
						this.binding = b;
						this.service = null;
						this.port = null;
						if (!IsBindingSupported ()) continue;
						found = true;
						ImportPortBinding (true);
					}
				}
			}

			EndNamespace ();
			
			if (!found) warnings = ServiceDescriptionImportWarnings.NoCodeGenerated;
			return true;
		}

		void ImportPortBinding (bool multipleBindings)
		{
			if (port != null) {
				if (multipleBindings) className = port.Name;
				else className = service.Name;
			}
			else
				className = binding.Name;
			
			className = classNames.AddUnique (CodeIdentifier.MakeValid (className), port);
			className = className.Replace ("_x0020_", "");	// MS.NET seems to do this
			
			try
			{
				portType = ServiceDescriptions.GetPortType (binding.Type);
				if (portType == null) throw new Exception ("Port type not found: " + binding.Type);

				CodeTypeDeclaration codeClass = BeginClass ();
				codeTypeDeclaration = codeClass;
				AddCodeType (codeClass, port != null ? port.Documentation : null);
				codeClass.Attributes = MemberAttributes.Public;
			
				if (service != null && service.Documentation != null && service.Documentation != "")
					AddComments (codeClass, service.Documentation);

				if (Style == ServiceDescriptionImportStyle.Client) {
					CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Diagnostics.DebuggerStepThroughAttribute");
					AddCustomAttribute (codeClass, att, true);
	
					att = new CodeAttributeDeclaration ("System.ComponentModel.DesignerCategoryAttribute");
					att.Arguments.Add (GetArg ("code"));
					AddCustomAttribute (codeClass, att, true);
				}
				else
					codeClass.TypeAttributes = System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Public;

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
#if NET_2_0
						if (Style == ServiceDescriptionImportStyle.Client)
							AddAsyncMembers (method.Name, method);
#endif
					}
				}
				
#if NET_2_0
			if (Style == ServiceDescriptionImportStyle.Client)
				AddAsyncTypes ();
#endif
				
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
		
#if NET_2_0

		void AddAsyncTypes ()
		{
			foreach (CodeTypeDeclaration type in asyncTypes)
				codeNamespace.Types.Add (type);
			asyncTypes.Clear ();
		}

		void AddAsyncMembers (string messageName, CodeMemberMethod method)
		{
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			CodePrimitiveExpression enull = new CodePrimitiveExpression (null);
			
			CodeMemberField codeField = new CodeMemberField (typeof(System.Threading.SendOrPostCallback), messageName + "OperationCompleted");
			codeField.Attributes = MemberAttributes.Private;
			CodeTypeDeclaration.Members.Add (codeField);
			
			// Event arguments class
			
			string argsClassName = classNames.AddUnique (messageName + "CompletedEventArgs", null);
			CodeTypeDeclaration argsClass = new CodeTypeDeclaration (argsClassName);
			argsClass.BaseTypes.Add (new CodeTypeReference ("System.ComponentModel.AsyncCompletedEventArgs"));

			CodeMemberField resultsField = new CodeMemberField (typeof(object[]), "results");
			resultsField.Attributes = MemberAttributes.Private;
			argsClass.Members.Add (resultsField);
			
			CodeConstructor cc = new CodeConstructor ();
			cc.Attributes = MemberAttributes.Assembly;
			cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object[]), "results"));
			cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof(System.Exception), "exception"));
			cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof(bool), "cancelled"));
			cc.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "userState"));
			cc.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("exception"));
			cc.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("cancelled"));
			cc.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("userState"));
			CodeExpression thisResults = new CodeFieldReferenceExpression (ethis, "results");
			cc.Statements.Add (new CodeAssignStatement (thisResults, new CodeVariableReferenceExpression ("results")));
			argsClass.Members.Add (cc);
			
			int ind = 0;
			
			if (method.ReturnType.BaseType != "System.Void")
				argsClass.Members.Add (CreateArgsProperty (method.ReturnType, "Result", ind++));
			
			foreach (CodeParameterDeclarationExpression par in method.Parameters) 
			{
				if (par.Direction == FieldDirection.Out || par.Direction == FieldDirection.Ref)
					argsClass.Members.Add (CreateArgsProperty (par.Type, par.Name, ind++));
			}
			
			bool needsArgsClass = (ind > 0);
			if (needsArgsClass)
				asyncTypes.Add (argsClass);
			else
				argsClassName = "System.ComponentModel.AsyncCompletedEventArgs";
			
			// Event delegate type
			
			CodeTypeDelegate delegateType = new CodeTypeDelegate (messageName + "CompletedEventHandler");
			delegateType.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "sender"));
			delegateType.Parameters.Add (new CodeParameterDeclarationExpression (argsClassName, "args"));
			
			// Event member
			
			CodeMemberEvent codeEvent = new CodeMemberEvent ();
			codeEvent.Name = messageName + "Completed";
			codeEvent.Type = new CodeTypeReference (delegateType.Name);
			CodeTypeDeclaration.Members.Add (codeEvent);
			
			// Async method (without user state param)
			
			CodeMemberMethod am = new CodeMemberMethod ();
			am.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			am.Name = method.Name + "Async";
			am.ReturnType = new CodeTypeReference (typeof(void));
			CodeMethodInvokeExpression inv;
			inv = new CodeMethodInvokeExpression (ethis, am.Name);
			am.Statements.Add (inv);
			
			// On...Completed method
			
			CodeMemberMethod onCompleted = new CodeMemberMethod ();
			onCompleted.Name = "On" + messageName + "Completed";
			onCompleted.Attributes = MemberAttributes.Private | MemberAttributes.Final;
			onCompleted.ReturnType = new CodeTypeReference (typeof(void));
			onCompleted.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), "arg"));
			
			CodeConditionStatement anIf = new CodeConditionStatement ();
			
			CodeExpression eventField = new CodeEventReferenceExpression (ethis, codeEvent.Name);
			anIf.Condition = new CodeBinaryOperatorExpression (eventField, CodeBinaryOperatorType.IdentityInequality, enull);
			CodeExpression castedArg = new CodeCastExpression (typeof(System.Web.Services.Protocols.InvokeCompletedEventArgs), new CodeVariableReferenceExpression ("arg"));
			CodeStatement invokeArgs = new CodeVariableDeclarationStatement (typeof(System.Web.Services.Protocols.InvokeCompletedEventArgs), "invokeArgs", castedArg);
			anIf.TrueStatements.Add (invokeArgs);
			
			CodeDelegateInvokeExpression delegateInvoke = new CodeDelegateInvokeExpression ();
			delegateInvoke.TargetObject = eventField;
			delegateInvoke.Parameters.Add (ethis);
			CodeObjectCreateExpression argsInstance = new CodeObjectCreateExpression (argsClassName);
			CodeExpression invokeArgsVar = new CodeVariableReferenceExpression ("invokeArgs");
			if (needsArgsClass) argsInstance.Parameters.Add (new CodeFieldReferenceExpression (invokeArgsVar, "Results"));
			argsInstance.Parameters.Add (new CodeFieldReferenceExpression (invokeArgsVar, "Error"));
			argsInstance.Parameters.Add (new CodeFieldReferenceExpression (invokeArgsVar, "Cancelled"));
			argsInstance.Parameters.Add (new CodeFieldReferenceExpression (invokeArgsVar, "UserState"));
			delegateInvoke.Parameters.Add (argsInstance);
			anIf.TrueStatements.Add (delegateInvoke);
			
			onCompleted.Statements.Add (anIf);
			
			// Async method
			
			CodeMemberMethod asyncMethod = new CodeMemberMethod ();
			asyncMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			asyncMethod.Name = method.Name + "Async";
			asyncMethod.ReturnType = new CodeTypeReference (typeof(void));
			
			CodeExpression delegateField = new CodeFieldReferenceExpression (ethis, codeField.Name);
			anIf = new CodeConditionStatement ();
			anIf.Condition = new CodeBinaryOperatorExpression (delegateField, CodeBinaryOperatorType.IdentityEquality, enull);;
			CodeExpression delegateRef = new CodeMethodReferenceExpression (ethis, onCompleted.Name);
			CodeExpression newDelegate = new CodeObjectCreateExpression (typeof(System.Threading.SendOrPostCallback), delegateRef);
			CodeAssignStatement cas = new CodeAssignStatement (delegateField, newDelegate);
			anIf.TrueStatements.Add (cas);
			asyncMethod.Statements.Add (anIf);
			
			CodeArrayCreateExpression paramsArray = new CodeArrayCreateExpression (typeof(object));
			
			// Assign parameters
			
			CodeIdentifiers paramsIds = new CodeIdentifiers ();
			
			foreach (CodeParameterDeclarationExpression par in method.Parameters) 
			{
				paramsIds.Add (par.Name, null);
				if (par.Direction == FieldDirection.In || par.Direction == FieldDirection.Ref) {
					CodeParameterDeclarationExpression inpar = new CodeParameterDeclarationExpression (par.Type, par.Name);
					am.Parameters.Add (inpar);
					asyncMethod.Parameters.Add (inpar);
					inv.Parameters.Add (new CodeVariableReferenceExpression (par.Name));
					paramsArray.Initializers.Add (new CodeVariableReferenceExpression (par.Name));
				}
			}


			inv.Parameters.Add (enull);
			
			string userStateName = paramsIds.AddUnique ("userState", null);
			asyncMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof(object), userStateName));
			
			CodeExpression userStateVar = new CodeVariableReferenceExpression (userStateName);
			asyncMethod.Statements.Add (BuildInvokeAsync (messageName, paramsArray, delegateField, userStateVar));
			
			CodeTypeDeclaration.Members.Add (am);
			CodeTypeDeclaration.Members.Add (asyncMethod);
			CodeTypeDeclaration.Members.Add (onCompleted);
			
			asyncTypes.Add (delegateType);
		}
		
		CodeMemberProperty CreateArgsProperty (CodeTypeReference type, string name, int ind)
		{
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			prop.HasGet = true;
			prop.HasSet = false;
			prop.Name = name;
			prop.Type = type;
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			CodeExpression thisResults = new CodeFieldReferenceExpression (ethis, "results");
			prop.GetStatements.Add (new CodeMethodInvokeExpression (ethis, "RaiseExceptionIfNecessary"));
			CodeArrayIndexerExpression arrValue = new CodeArrayIndexerExpression (thisResults, new CodePrimitiveExpression (ind));
			CodeExpression retval = new CodeCastExpression (type, arrValue);
			prop.GetStatements.Add (new CodeMethodReturnStatement (retval));
			return prop;
		}
		
		internal virtual CodeExpression BuildInvokeAsync (string messageName, CodeArrayCreateExpression paramsArray, CodeExpression delegateField, CodeExpression userStateVar)
		{
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			CodeMethodInvokeExpression inv2 = new CodeMethodInvokeExpression (ethis, "InvokeAsync");
			inv2.Parameters.Add (new CodePrimitiveExpression (messageName));
			inv2.Parameters.Add (paramsArray);
			inv2.Parameters.Add (delegateField);
			inv2.Parameters.Add (userStateVar);
			return inv2;
		}
#endif
		
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
