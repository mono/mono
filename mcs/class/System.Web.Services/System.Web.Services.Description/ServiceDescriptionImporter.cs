// 
// System.Web.Services.Description.ServiceDescriptionImporter.cs
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
using System.Web.Services.Description;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Configuration;

namespace System.Web.Services.Description {
	public class ServiceDescriptionImporter {

		#region Fields

		string protocolName;
		XmlSchemas schemas;
		ServiceDescriptionCollection serviceDescriptions;
		ServiceDescriptionImportStyle style;
		CodeIdentifiers classIds;
		XmlSchemaImporter schemaImporter;
		SoapSchemaImporter soapSchemaImporter;
		XmlCodeExporter codeExporter;
		
		CodeNamespace codeNamespace;
		CodeCompileUnit codeCompileUnit;
		ServiceDescriptionImportWarnings warnings;

		ArrayList importInfo = new ArrayList ();
		
		class ImportInfo
		{
			public ServiceDescription ServiceDescription;
			public string AppSettingUrlKey;
			public string AppSettingBaseUrl;
		}


		#endregion // Fields

		#region Constructors
	
		public ServiceDescriptionImporter ()
		{
			protocolName = String.Empty;
			schemas = new XmlSchemas ();
			serviceDescriptions = new ServiceDescriptionCollection ();
			style = ServiceDescriptionImportStyle.Client;
		}
		
		#endregion // Constructors

		#region Properties

		public string ProtocolName {
			get { return protocolName; }
			set { protocolName = value; }
		}

		public XmlSchemas Schemas {
			get { return schemas; }
		}

		public ServiceDescriptionCollection ServiceDescriptions {
			get { return serviceDescriptions; }
		}

		public ServiceDescriptionImportStyle Style {
			get { return style; }
			set { style = value; }
		}
	
		#endregion // Properties

		#region Methods

		public void AddServiceDescription (ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl)
		{
			if (appSettingUrlKey != null && appSettingUrlKey == string.Empty && style == ServiceDescriptionImportStyle.Server)
				throw new InvalidOperationException ("Cannot set appSettingUrlKey if Style is Server");

			ImportInfo info = new ImportInfo ();
			info.ServiceDescription = serviceDescription;
			info.AppSettingUrlKey = appSettingUrlKey;
			info.AppSettingBaseUrl = appSettingBaseUrl;
			importInfo.Add (info);
			serviceDescriptions.Add (serviceDescription);
			
			if (serviceDescription.Types != null)
				schemas.Add (serviceDescription.Types.Schemas);
		}

		public ServiceDescriptionImportWarnings Import (CodeNamespace codeNamespace, CodeCompileUnit codeCompileUnit)
		{
			warnings = (ServiceDescriptionImportWarnings) 0;
			
			schemaImporter = new XmlSchemaImporter (schemas);
			soapSchemaImporter = new SoapSchemaImporter (schemas);
			codeExporter = new XmlCodeExporter (codeNamespace, codeCompileUnit);
			
			this.codeNamespace = codeNamespace;
			this.codeCompileUnit = codeCompileUnit;

			bool classesGenerated = false;
			classIds = new CodeIdentifiers();
			
			ArrayList services = new ArrayList ();
			foreach (ImportInfo info in importInfo)
				foreach (Service service in info.ServiceDescription.Services)
					foreach (Port port in service.Ports)
						classesGenerated = ImportPortBinding (service, info, port) || classesGenerated;
						
			if (!classesGenerated) SetWarning (ServiceDescriptionImportWarnings.NoCodeGenerated);
			return warnings;
		}
		
		bool ImportPortBinding (Service service, ImportInfo iinfo, Port port)
		{
			string className = classIds.AddUnique (CodeIdentifier.MakeValid (port.Name), port);
			Binding binding = serviceDescriptions.GetBinding (port.Binding);
			
			try
			{
				PortType portType = serviceDescriptions.GetPortType (binding.Type);
				if (portType == null) throw new Exception ("Port type not found: " + binding.Type);

				SoapBinding sb = (SoapBinding) binding.Extensions.Find (typeof(SoapBinding));

				if (sb == null) throw new Exception ("None of the supported bindings was found");
				
				if (sb.Style != SoapBindingStyle.Document) throw new Exception ("Binding style not supported");
				if (sb.Transport != SoapBinding.HttpTransport) throw new Exception ("Transport namespace '" + sb.Transport + "' not supported");
	
				string url = GetServiceUrl (iinfo, port); 
				
				CodeTypeDeclaration codeClass = new CodeTypeDeclaration (className);
				AddCodeType (codeClass, port.Documentation);
				codeClass.Attributes = MemberAttributes.Public;
				
				if (service.Documentation != null && service.Documentation != "")
					AddComments (codeClass, service.Documentation);

				CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Diagnostics.DebuggerStepThroughAttribute");
				AddCustomAttribute (codeClass, att, true);

				att = new CodeAttributeDeclaration ("System.ComponentModel.DesignerCategoryAttribute");
				att.Arguments.Add (GetArg ("code"));
				AddCustomAttribute (codeClass, att, true);

				att = new CodeAttributeDeclaration ("System.Web.Services.WebServiceBinding");
				att.Arguments.Add (GetArg ("Name", port.Name));
				att.Arguments.Add (GetArg ("Namespace", port.Binding.Namespace));
				AddCustomAttribute (codeClass, att, true);
	
				CodeTypeReference ctr = new CodeTypeReference ("System.Web.Services.Protocols.SoapHttpClientProtocol");
				codeClass.BaseTypes.Add (ctr);
				
				CodeConstructor cc = new CodeConstructor ();
				cc.Attributes = MemberAttributes.Public;
				CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
				CodeAssignStatement cas = new CodeAssignStatement (ce, new CodePrimitiveExpression (url));
				cc.Statements.Add (cas);
				codeClass.Members.Add (cc);
				
				if (binding.Operations.Count == 0) {
					SetWarning (ServiceDescriptionImportWarnings.NoMethodsGenerated);
					return true;
				}
				
				CodeIdentifiers memberIds = new CodeIdentifiers ();
				foreach (OperationBinding oper in binding.Operations)
					ImportOperationBinding (codeClass, memberIds, portType, oper);
			}
			catch (Exception ex)
			{
				GenerateErrorComment (binding, ex.Message);
				return false;
			}
			return true;
		}
		
		void ImportOperationBinding (CodeTypeDeclaration codeClass, CodeIdentifiers memberIds, PortType portType, OperationBinding oper)
		{
			try
			{
				SoapOperationBinding soapOper = oper.Extensions.Find (typeof (SoapOperationBinding)) as SoapOperationBinding;
				if (soapOper == null) throw new Exception ("Soap operation binding not found in operation " + oper.Name);
				if (soapOper.Style != SoapBindingStyle.Document) throw new Exception ("Operation binding style not supported in operation " + oper.Name);

				SoapBodyBinding isbb = oper.Input.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
				if (isbb == null) throw new Exception ("Soap body binding not found in operation " + oper.Name);
				string iname = (oper.Input.Name != "") ? oper.Input.Name : oper.Name;
				
				SoapBodyBinding osbb = oper.Output.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
				if (osbb == null) throw new Exception ("Soap body binding not found in operation " + oper.Name);
				string oname = (oper.Output.Name != "") ? oper.Output.Name : oper.Name;
				
				Operation ptoper = FindPortOperation (portType, oper.Name, iname, oname);
				if (ptoper == null) throw new Exception ("Operation " + oper.Name + " not found in portType " + portType.Name);

				XmlMembersMapping inputMembers = null;
				XmlMembersMapping outputMembers = null;
	
				foreach (OperationMessage omsg in ptoper.Messages)
				{
					if (omsg is OperationInput)
						inputMembers = ImportMembersMapping (omsg, ptoper, isbb, soapOper);
					else
						outputMembers = ImportMembersMapping (omsg, ptoper, osbb, soapOper);
				}
				
				if (inputMembers == null) throw new Exception ("Input message not declared in operation " + oper.Name);
				if (outputMembers == null) throw new Exception ("Output message not declared in operation " + oper.Name);
				
				GenerateMethod (codeClass, memberIds, ptoper, soapOper, isbb, inputMembers, outputMembers);
				
				codeExporter.ExportMembersMapping (inputMembers);
				codeExporter.ExportMembersMapping (outputMembers);
			}
			catch (Exception ex)
			{
				GenerateErrorComment (oper, ex.Message);
				Console.WriteLine (ex);
			}
		}
		
		XmlMembersMapping ImportMembersMapping (OperationMessage omsg, Operation ptoper, SoapBodyBinding sbb, SoapOperationBinding soapOper)
		{
			Message msg = serviceDescriptions.GetMessage (omsg.Message);
			if (msg == null) throw new Exception ("Message not found: " + omsg.Message);

			XmlQualifiedName elem = null;
			if (msg.Parts.Count == 1 && msg.Parts[0].Name == "parameters")
			{
				// Wrapped parameter style
				
				MessagePart part = msg.Parts[0];
				if (sbb.Use == SoapBindingUse.Encoded)
				{
					SoapSchemaMember ssm = new SoapSchemaMember ();
					ssm.MemberName = part.Name;
					ssm.MemberType = part.Type;
					return soapSchemaImporter.ImportMembersMapping (ptoper.Name, omsg.Message.Namespace, ssm);
				}
				else
					return schemaImporter.ImportMembersMapping (part.Element);				
			}
			else
			{
				if (sbb.Use == SoapBindingUse.Encoded)
				{
					SoapSchemaMember[] mems = new SoapSchemaMember [msg.Parts.Count];
					for (int n=0; n<mems.Length; n++)
					{
						SoapSchemaMember mem = new SoapSchemaMember();
						mem.MemberName = msg.Parts[n].Name;
						mem.MemberType = msg.Parts[n].Type;
						mems[n] = mem;
					}
					return soapSchemaImporter.ImportMembersMapping (ptoper.Name, omsg.Message.Namespace, mems);
				}
				else
				{
					XmlQualifiedName[] pnames = new XmlQualifiedName [msg.Parts.Count];
					for (int n=0; n<pnames.Length; n++)
						pnames[n] = msg.Parts[n].Element;
					return schemaImporter.ImportMembersMapping (pnames);
				}
			}
		}
		
		
		void GenerateMethod (CodeTypeDeclaration codeClass, CodeIdentifiers memberIds, Operation oper, SoapOperationBinding soapOper, SoapBodyBinding bodyBinding, XmlMembersMapping inputMembers, XmlMembersMapping outputMembers)
		{
			CodeIdentifiers pids = new CodeIdentifiers ();
			CodeMemberMethod method = new CodeMemberMethod ();
			CodeMemberMethod methodBegin = new CodeMemberMethod ();
			CodeMemberMethod methodEnd = new CodeMemberMethod ();
			method.Attributes = MemberAttributes.Public;
			methodBegin.Attributes = MemberAttributes.Public;
			methodEnd.Attributes = MemberAttributes.Public;
			
			// Find unique names for temporary variables
			
			for (int n=0; n<inputMembers.Count; n++)
				pids.AddUnique (inputMembers[n].MemberName, inputMembers[n]);

			for (int n=0; n<outputMembers.Count; n++)
				pids.AddUnique (outputMembers[n].MemberName, outputMembers[n]);
				
			string varAsyncResult = pids.AddUnique ("asyncResult","asyncResult");
			string varResults = pids.AddUnique ("results","results");
			string varCallback = pids.AddUnique ("callback","callback");
			string varAsyncState = pids.AddUnique ("asyncState","asyncState");

			string messageName = memberIds.AddUnique(CodeIdentifier.MakeValid(oper.Name),method);

			method.Name = oper.Name;
			methodBegin.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("Begin" + memberIds.MakeRightCase(oper.Name)),method);
			methodEnd.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("End" + memberIds.MakeRightCase(oper.Name)),method);

			method.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.Parameters.Add (new CodeParameterDeclarationExpression (typeof (IAsyncResult),varAsyncResult));

			CodeExpression[] paramArray = new CodeExpression [inputMembers.Count];
			CodeParameterDeclarationExpression[] outParams = new CodeParameterDeclarationExpression [outputMembers.Count];

			for (int n=0; n<inputMembers.Count; n++)
			{
				CodeParameterDeclarationExpression param = GenerateParameter (inputMembers[n], FieldDirection.In);
				method.Parameters.Add (param);
				GenerateMemberAttributes (inputMembers, inputMembers[n], param);
				methodBegin.Parameters.Add (GenerateParameter (inputMembers[n], FieldDirection.In));
				paramArray [n] = new CodeVariableReferenceExpression (param.Name);
			}

			for (int n=0; n<outputMembers.Count; n++)
			{
				CodeParameterDeclarationExpression cpd = GenerateParameter (outputMembers[n], FieldDirection.Out);
				outParams [n] = cpd;
				
				bool found = false;
				foreach (CodeParameterDeclarationExpression ip in method.Parameters)
				{
					if (ip.Name == cpd.Name && ip.Type.BaseType == cpd.Type.BaseType) {
						ip.Direction = FieldDirection.Ref;
						methodEnd.Parameters.Add (GenerateParameter (outputMembers[n], FieldDirection.Out));
						found = true;
						break;
					}
				}
				
				if (found) continue;

				if ((outputMembers [n].ElementName == oper.Name + "Result") || (inputMembers.Count==0 && outputMembers.Count==1)) {
					method.ReturnType = cpd.Type;
					methodEnd.ReturnType = cpd.Type;
					GenerateReturnAttributes (outputMembers, outputMembers[n], method);
					outParams [n] = null;
					continue;
				}
				
				method.Parameters.Add (cpd);
				GenerateMemberAttributes (outputMembers, outputMembers[n], cpd);
				methodEnd.Parameters.Add (GenerateParameter (outputMembers[n], FieldDirection.Out));
			}

			methodBegin.Parameters.Add (new CodeParameterDeclarationExpression (typeof (AsyncCallback),varCallback));
			methodBegin.Parameters.Add (new CodeParameterDeclarationExpression (typeof (object),varAsyncState));
			methodBegin.ReturnType = new CodeTypeReference (typeof(IAsyncResult));

			// Array of input parameters
			
			CodeArrayCreateExpression methodParams;
			if (paramArray.Length > 0)
				methodParams = new CodeArrayCreateExpression (typeof(object), paramArray);
			else
				methodParams = new CodeArrayCreateExpression (typeof(object), 0);

			// Assignment of output parameters
			
			CodeStatementCollection outAssign = new CodeStatementCollection ();
			CodeVariableReferenceExpression arrVar = new CodeVariableReferenceExpression (varResults);
			for (int n=0; n<outParams.Length; n++)
			{
				CodeExpression index = new CodePrimitiveExpression (n);
				if (outParams[n] == null)
				{
					CodeExpression res = new CodeCastExpression (method.ReturnType, new CodeArrayIndexerExpression (arrVar, index));
					outAssign.Add (new CodeMethodReturnStatement (res));
				}
				else
				{
					CodeExpression res = new CodeCastExpression (outParams[n].Type, new CodeArrayIndexerExpression (arrVar, index));
					CodeExpression var = new CodeVariableReferenceExpression (outParams[n].Name);
					outAssign.Insert (0, new CodeAssignStatement (var, res));
				}
			}
			
			// Invoke call
			
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			CodePrimitiveExpression varMsgName = new CodePrimitiveExpression (messageName);
			CodeMethodInvokeExpression inv;

			inv = new CodeMethodInvokeExpression (ethis, "Invoke", varMsgName, methodParams);
			CodeVariableDeclarationStatement dec = new CodeVariableDeclarationStatement (typeof(object[]), varResults, inv);
			method.Statements.Add (dec);
			method.Statements.AddRange (outAssign);
			
			// Begin Invoke Call
			
			CodeExpression expCallb = new CodeVariableReferenceExpression (varCallback);
			CodeExpression expAsyncs = new CodeVariableReferenceExpression (varAsyncState);
			inv = new CodeMethodInvokeExpression (ethis, "BeginInvoke", varMsgName, methodParams, expCallb, expAsyncs);
			methodBegin.Statements.Add (new CodeMethodReturnStatement (inv));
			
			// End Invoke call
			
			CodeExpression varAsyncr = new CodeVariableReferenceExpression (varAsyncResult);
			inv = new CodeMethodInvokeExpression (ethis, "EndInvoke", varAsyncr);
			dec = new CodeVariableDeclarationStatement (typeof(object[]), varResults, inv);
			methodEnd.Statements.Add (dec);
			methodEnd.Statements.AddRange (outAssign);
			
			// Attributes
			
			if (inputMembers.ElementName == "" && outputMembers.ElementName != "" || 
				inputMembers.ElementName != "" && outputMembers.ElementName == "")
				throw new Exception ("Parameter style is not the same for the input message and output message");

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.SoapDocumentMethodAttribute");
			att.Arguments.Add (GetArg (soapOper.SoapAction));
			if (inputMembers.ElementName != "") {
				if (inputMembers.ElementName != method.Name) att.Arguments.Add (GetArg ("RequestElementName", inputMembers.ElementName));
				if (outputMembers.ElementName != (method.Name + "Response")) att.Arguments.Add (GetArg ("RequestElementName", outputMembers.ElementName));
				att.Arguments.Add (GetArg ("RequestNamespace", inputMembers.Namespace));
				att.Arguments.Add (GetArg ("ResponseNamespace", outputMembers.Namespace));
				att.Arguments.Add (GetEnumArg ("ParameterStyle", "System.Web.Services.Protocols.SoapParameterStyle", "Wrapped"));
			}
			else
				att.Arguments.Add (GetEnumArg ("ParameterStyle", "System.Web.Services.Protocols.SoapParameterStyle", "Bare"));
				
			att.Arguments.Add (GetEnumArg ("Use", "System.Web.Services.Description.SoapBindingUse", bodyBinding.Use.ToString()));
			AddCustomAttribute (method, att, true);
			
			att = new CodeAttributeDeclaration ("System.Web.Services.WebMethodAttribute");
			if (messageName != method.Name) att.Arguments.Add (GetArg ("MessageName",messageName));
			AddCustomAttribute (method, att, false);
			
			if (oper.Documentation != null && oper.Documentation != "")
				AddComments (method, oper.Documentation);
			
			codeClass.Members.Add (method);
			codeClass.Members.Add (methodBegin);
			codeClass.Members.Add (methodEnd);
		}
		
		CodeParameterDeclarationExpression GenerateParameter (XmlMemberMapping member, FieldDirection dir)
		{
			CodeParameterDeclarationExpression par = new CodeParameterDeclarationExpression (member.TypeFullName, member.MemberName);
			par.Direction = dir;
			return par;
		}
		
		void GenerateMemberAttributes (XmlMembersMapping members, XmlMemberMapping member, CodeParameterDeclarationExpression param)
		{
			GenerateMemberAttributes (members, member, member.MemberName, param.CustomAttributes);
		}
		
		void GenerateReturnAttributes (XmlMembersMapping members, XmlMemberMapping member, CodeMemberMethod method)
		{
			GenerateMemberAttributes (members, member, method.Name + "Result", method.ReturnTypeCustomAttributes);
		}
		
		void GenerateMemberAttributes (XmlMembersMapping members, XmlMemberMapping member, string memberName, CodeAttributeDeclarationCollection atts)
		{
			CodeAttributeDeclaration att;
			
			if (member.TypeFullName.EndsWith ("]") && Array.IndexOf (primitiveArrays, member.TypeName) == -1)
			{
				// Array parameter
				att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlArray");
				if (member.ElementName != memberName) att.Arguments.Add (GetArg ("ElementName", member.ElementName));
				if (member.Namespace != members.Namespace) att.Arguments.Add (GetArg ("Namespace", member.Namespace));
				if (att.Arguments.Count > 0) atts.Add (att);
			}
			else
			{
				att = new CodeAttributeDeclaration ("System.Xml.Serialization.XmlElement");
				if (member.ElementName != memberName) att.Arguments.Add (GetArg ("ElementName", member.ElementName));
				if (member.Namespace != members.Namespace) att.Arguments.Add (GetArg ("Namespace", member.Namespace));
				if (member.TypeNamespace == "" && Array.IndexOf (defaultSchemaTypes, member.TypeName) == -1)  att.Arguments.Add (GetArg ("DataType", member.TypeName));
				if (att.Arguments.Count > 0) atts.Add (att);
			}
		}
		
		Operation FindPortOperation (PortType portType, string operName, string inMessage, string outMessage)
		{
			foreach (Operation oper in portType.Operations)
			{
				if (oper.Name == operName)
				{
					int hits = 0;
					foreach (OperationMessage omsg in oper.Messages)
					{
						if (omsg is OperationInput && GetOperMessageName (omsg, operName) == inMessage) hits++;
						if (omsg is OperationOutput && GetOperMessageName (omsg, operName) == outMessage) hits++;
					}
					if (hits == 2) return oper;
				}
			}
			return null;
		}
		
		string GetOperMessageName (OperationMessage msg, string operName)
		{
			if (msg.Name == null) return operName;
			else return msg.Name;
		}
		
		void GenerateErrorComment (Binding binding, string message)
		{
			AddGlobalComments ("WARNING: Could not generate proxy for binding " + binding.Name + ". " + message);
		}
		
		void GenerateErrorComment (OperationBinding oper, string message)
		{
			AddGlobalComments ("WARNING: Could not generate operation " + oper.Name + ". " + message);
		}
		
		string GetServiceUrl (ImportInfo info, Port port)
		{
			string location = null;
			
			SoapAddressBinding sab = (SoapAddressBinding) port.Extensions.Find (typeof(SoapAddressBinding));
			if (sab != null) location = sab.Location;
			
			if (info.AppSettingUrlKey == null || info.AppSettingUrlKey == string.Empty)
				return location;
			else
			{
				string url;
				if (style == ServiceDescriptionImportStyle.Server) throw new InvalidOperationException ("Cannot set appSettingUrlKey if Style is Server");
				url = ConfigurationSettings.AppSettings [info.AppSettingUrlKey];
				if (info.AppSettingBaseUrl != null && info.AppSettingBaseUrl != string.Empty)
					url += "/" + info.AppSettingBaseUrl + "/" + location;
				return url;
			}
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

		void AddCustomAttribute (CodeTypeMember ctm, CodeAttributeDeclaration att, bool addIfNoParams)
		{
			if (att.Arguments.Count == 0 && !addIfNoParams) return;
			
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (att);
		}

		void AddCustomAttribute (CodeTypeMember ctm, string name, params CodeAttributeArgument[] args)
		{
			if (ctm.CustomAttributes == null) ctm.CustomAttributes = new CodeAttributeDeclarationCollection ();
			ctm.CustomAttributes.Add (new CodeAttributeDeclaration (name, args));
		}

		CodeAttributeArgument GetArg (string name, object value)
		{
			return new CodeAttributeArgument (name, new CodePrimitiveExpression(value));
		}

		CodeAttributeArgument GetEnumArg (string name, string enumType, string enumValue)
		{
			return new CodeAttributeArgument (name, new CodeFieldReferenceExpression (new CodeTypeReferenceExpression(enumType), enumValue));
		}

		CodeAttributeArgument GetArg (object value)
		{
			return new CodeAttributeArgument (new CodePrimitiveExpression(value));
		}

		CodeAttributeArgument GetTypeArg (string name, string typeName)
		{
			return new CodeAttributeArgument (name, new CodeTypeOfExpression(typeName));
		}
		
		void SetWarning (ServiceDescriptionImportWarnings w)
		{
			warnings |= w;
		}

		static string[] defaultSchemaTypes = new string[] 
		{
			"string", "int", "long", "short", "boolean", "dateTime", "float", "unsignedShort",
			"unsignedInt", "unsignedLong", "double", "decimal", "QName", "unsignedByte", "byte",
			"char", "duration", "base64Binary", "anyURI", "guid"
		};
		
		static string[] primitiveArrays = new string[]
		{
			"NMTOKENS", "ENTITIES", "hexBinary", "IDREFS", "base64Binary"
		};

#endregion
	}
}
