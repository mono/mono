// 
// System.Web.Services.Description.SoapProtocolImporter.cs
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

using System.CodeDom;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Services.Configuration;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Configuration;
using System.Collections;

namespace System.Web.Services.Description {
	public class SoapProtocolImporter : ProtocolImporter {

		#region Fields

		SoapBinding soapBinding;
		SoapCodeExporter soapExporter;
		SoapSchemaImporter soapImporter;
		XmlCodeExporter xmlExporter;
		XmlSchemaImporter xmlImporter;
		CodeIdentifiers memberIds;
		ArrayList extensionImporters;
		Hashtable headerVariables;
		XmlSchemas xmlSchemas;
		XmlSchemas soapSchemas;
		
		#endregion // Fields

		#region Constructors

		public SoapProtocolImporter ()
		{
			extensionImporters = ExtensionManager.BuildExtensionImporters ();
		}
		
		void SetBinding (SoapBinding soapBinding)
		{
			this.soapBinding = soapBinding;
		}
		
		#endregion // Constructors

		#region Properties

		public override string ProtocolName {
			get { return "Soap"; }
		}

		public SoapBinding SoapBinding {
			get { return soapBinding; }
		}

		public SoapCodeExporter SoapExporter {
			get { return soapExporter; }
		}

		public SoapSchemaImporter SoapImporter {
			get { return soapImporter; }
		}

		public XmlCodeExporter XmlExporter {
			get { return xmlExporter; }
		}

		public XmlSchemaImporter XmlImporter {
			get { return xmlImporter; }
		}

		#endregion // Properties

		#region Methods

		protected override CodeTypeDeclaration BeginClass ()
		{
			soapBinding = (SoapBinding) Binding.Extensions.Find (typeof(SoapBinding));
			
			CodeTypeDeclaration codeClass = new CodeTypeDeclaration (ClassName);
			
			string location = null;
			string url = null;
			
			if (Port != null) {
				SoapAddressBinding sab = (SoapAddressBinding) Port.Extensions.Find (typeof(SoapAddressBinding));
				if (sab != null) location = sab.Location;
				url = GetServiceUrl (location); 
			}
			
			string namspace = (Port != null ? Port.Binding.Namespace : Binding.ServiceDescription.TargetNamespace);
			string name = (Port != null ? Port.Name : Binding.Name);

			if (Style == ServiceDescriptionImportStyle.Client) {
				CodeTypeReference ctr = new CodeTypeReference ("System.Web.Services.Protocols.SoapHttpClientProtocol");
				codeClass.BaseTypes.Add (ctr);
			}
			else {
				CodeTypeReference ctr = new CodeTypeReference ("System.Web.Services.WebService");
				codeClass.BaseTypes.Add (ctr);
				CodeAttributeDeclaration attws = new CodeAttributeDeclaration ("System.Web.Services.WebServiceAttribute");
				attws.Arguments.Add (GetArg ("Namespace", namspace));
				AddCustomAttribute (codeClass, attws, true);
			}
			
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.WebServiceBinding");
			att.Arguments.Add (GetArg ("Name", name));
			att.Arguments.Add (GetArg ("Namespace", namspace));
			AddCustomAttribute (codeClass, att, true);
	
			if (Style == ServiceDescriptionImportStyle.Client) {
				CodeConstructor cc = new CodeConstructor ();
				cc.Attributes = MemberAttributes.Public;
				if (url != null) {
					CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
					CodeAssignStatement cas = new CodeAssignStatement (ce, new CodePrimitiveExpression (url));
					cc.Statements.Add (cas);
				}
				codeClass.Members.Add (cc);
			}
			
			memberIds = new CodeIdentifiers ();
			headerVariables = new Hashtable ();
			return codeClass;
		}
		
		protected override void BeginNamespace ()
		{
#if NET_2_0
			xmlImporter = new XmlSchemaImporter (LiteralSchemas, base.CodeGenerationOptions, base.CodeGenerator, base.ImportContext);
			soapImporter = new SoapSchemaImporter (EncodedSchemas, base.CodeGenerationOptions, base.CodeGenerator, base.ImportContext);
			xmlExporter = new XmlCodeExporter (CodeNamespace, null, base.CodeGenerator, base.CodeGenerationOptions, null);
			soapExporter = new SoapCodeExporter (CodeNamespace, null, base.CodeGenerator, base.CodeGenerationOptions, null);
#else
			xmlImporter = new XmlSchemaImporter (LiteralSchemas, ClassNames);
			soapImporter = new SoapSchemaImporter (EncodedSchemas, ClassNames);
			xmlExporter = new XmlCodeExporter (CodeNamespace, null);
			soapExporter = new SoapCodeExporter (CodeNamespace, null);
#endif
		}

		protected override void EndClass ()
		{
			SoapTransportImporter transportImporter = SoapTransportImporter.FindTransportImporter (soapBinding.Transport);
			if (transportImporter == null) throw new InvalidOperationException ("Transport '" + soapBinding.Transport + "' not supported");
			transportImporter.ImportContext = this;
			transportImporter.ImportClass ();
			
			if (xmlExporter.IncludeMetadata.Count > 0 || soapExporter.IncludeMetadata.Count > 0)
			{
				if (CodeTypeDeclaration.CustomAttributes == null)
					CodeTypeDeclaration.CustomAttributes = new CodeAttributeDeclarationCollection ();
				CodeTypeDeclaration.CustomAttributes.AddRange (xmlExporter.IncludeMetadata);
				CodeTypeDeclaration.CustomAttributes.AddRange (soapExporter.IncludeMetadata);
			}
		}

		protected override void EndNamespace ()
		{
		}

		protected override bool IsBindingSupported ()
		{
			return Binding.Extensions.Find (typeof(SoapBinding)) != null;
		}

		[MonoTODO]
		protected override bool IsOperationFlowSupported (OperationFlow flow)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual bool IsSoapEncodingPresent (string uriList)
		{
			throw new NotImplementedException ();
		}

		protected override CodeMemberMethod GenerateMethod ()
		{
			try
			{
				SoapOperationBinding soapOper = OperationBinding.Extensions.Find (typeof (SoapOperationBinding)) as SoapOperationBinding;
				if (soapOper == null) throw new InvalidOperationException ("Soap operation binding not found");

				SoapBindingStyle style = soapOper.Style != SoapBindingStyle.Default ? soapOper.Style : soapBinding.Style;
			
				SoapBodyBinding isbb = null;
				XmlMembersMapping inputMembers = null;
				
				isbb = OperationBinding.Input.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
				if (isbb == null) throw new InvalidOperationException ("Soap body binding not found");
			
				inputMembers = ImportMembersMapping (InputMessage, isbb, style, false);
				if (inputMembers == null) throw new InvalidOperationException ("Input message not declared");
				
				// If OperationBinding.Output is null, it is an OneWay operation
				
				SoapBodyBinding osbb = null;
				XmlMembersMapping outputMembers = null;
				
				if (OperationBinding.Output != null) {
					osbb = OperationBinding.Output.Extensions.Find (typeof(SoapBodyBinding)) as SoapBodyBinding;
					if (osbb == null) throw new InvalidOperationException ("Soap body binding not found");

					outputMembers = ImportMembersMapping (OutputMessage, osbb, style, true);
					if (outputMembers == null) throw new InvalidOperationException ("Output message not declared");
				}
				
				CodeMemberMethod met = GenerateMethod (memberIds, soapOper, isbb, inputMembers, outputMembers);
				
				if (isbb.Use == SoapBindingUse.Literal)
					xmlExporter.ExportMembersMapping (inputMembers);
				else
					soapExporter.ExportMembersMapping (inputMembers);
				
				if (osbb != null) {
					if (osbb.Use == SoapBindingUse.Literal)
						xmlExporter.ExportMembersMapping (outputMembers);
					else
						soapExporter.ExportMembersMapping (outputMembers);
				}
				
				foreach (SoapExtensionImporter eximporter in extensionImporters)
				{
					eximporter.ImportContext = this;
					eximporter.ImportMethod (met.CustomAttributes);
				}
				
				return met;
			}
			catch (InvalidOperationException ex)
			{
				UnsupportedOperationBindingWarning (ex.Message);
				return null;
			}
		}
		
		XmlMembersMapping ImportMembersMapping (Message msg, SoapBodyBinding sbb, SoapBindingStyle style, bool output)
		{
			string elemName = Operation.Name;
			if (output) elemName += "Response";

			if (msg.Parts.Count == 1 && msg.Parts[0].Name == "parameters")
			{
				// Wrapped parameter style
				
				MessagePart part = msg.Parts[0];
				if (sbb.Use == SoapBindingUse.Encoded)
				{
					SoapSchemaMember ssm = new SoapSchemaMember ();
					ssm.MemberName = part.Name;
					ssm.MemberType = part.Type;
					return soapImporter.ImportMembersMapping (elemName, part.Type.Namespace, ssm);
				}
				else
					return xmlImporter.ImportMembersMapping (part.Element);
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
					
					// Rpc messages always have a wrapping element
					if (style == SoapBindingStyle.Rpc)
						return soapImporter.ImportMembersMapping (elemName, sbb.Namespace, mems, true);
					else
						return soapImporter.ImportMembersMapping ("", "", mems, false);
				}
				else
				{
					if (style == SoapBindingStyle.Rpc)
						throw new InvalidOperationException ("The combination of style=rpc with use=literal is not supported");
					
					if (msg.Parts.Count == 1 && msg.Parts[0].Type != XmlQualifiedName.Empty)
						return xmlImporter.ImportAnyType (msg.Parts[0].Type, null);
					else
					{
						XmlQualifiedName[] pnames = new XmlQualifiedName [msg.Parts.Count];
						for (int n=0; n<pnames.Length; n++)
							pnames[n] = msg.Parts[n].Element;
						return xmlImporter.ImportMembersMapping (pnames);
					}
				}
			}
		}
		
		CodeMemberMethod GenerateMethod (CodeIdentifiers memberIds, SoapOperationBinding soapOper, SoapBodyBinding bodyBinding, XmlMembersMapping inputMembers, XmlMembersMapping outputMembers)
		{
			CodeIdentifiers pids = new CodeIdentifiers ();
			CodeMemberMethod method = new CodeMemberMethod ();
			CodeMemberMethod methodBegin = new CodeMemberMethod ();
			CodeMemberMethod methodEnd = new CodeMemberMethod ();
			method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			methodBegin.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			methodEnd.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			
			SoapBindingStyle style = soapOper.Style != SoapBindingStyle.Default ? soapOper.Style : soapBinding.Style;
			
			// Find unique names for temporary variables
			
			for (int n=0; n<inputMembers.Count; n++)
				pids.AddUnique (inputMembers[n].MemberName, inputMembers[n]);

			if (outputMembers != null)
				for (int n=0; n<outputMembers.Count; n++)
					pids.AddUnique (outputMembers[n].MemberName, outputMembers[n]);
				
			string varAsyncResult = pids.AddUnique ("asyncResult","asyncResult");
			string varResults = pids.AddUnique ("results","results");
			string varCallback = pids.AddUnique ("callback","callback");
			string varAsyncState = pids.AddUnique ("asyncState","asyncState");

			string messageName = memberIds.AddUnique(CodeIdentifier.MakeValid(Operation.Name),method);

			method.Name = CodeIdentifier.MakeValid(Operation.Name);
			if (method.Name == ClassName) method.Name += "1";
			methodBegin.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("Begin" + method.Name),method);
			methodEnd.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("End" + method.Name),method);

			method.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.Parameters.Add (new CodeParameterDeclarationExpression (typeof (IAsyncResult),varAsyncResult));

			CodeExpression[] paramArray = new CodeExpression [inputMembers.Count];
			CodeParameterDeclarationExpression[] outParams = new CodeParameterDeclarationExpression [outputMembers != null ? outputMembers.Count : 0];

			for (int n=0; n<inputMembers.Count; n++)
			{
				CodeParameterDeclarationExpression param = GenerateParameter (inputMembers[n], FieldDirection.In);
				method.Parameters.Add (param);
				GenerateMemberAttributes (inputMembers, inputMembers[n], bodyBinding.Use, param);
				methodBegin.Parameters.Add (GenerateParameter (inputMembers[n], FieldDirection.In));
				paramArray [n] = new CodeVariableReferenceExpression (param.Name);
			}

			if (outputMembers != null)
			{
				bool hasReturn = false;
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
	
					if (!hasReturn) 
					{
						hasReturn = true;
						method.ReturnType = cpd.Type;
						methodEnd.ReturnType = cpd.Type;
						GenerateReturnAttributes (outputMembers, outputMembers[n], bodyBinding.Use, method);
						outParams [n] = null;
						continue;
					}
					
					method.Parameters.Add (cpd);
					GenerateMemberAttributes (outputMembers, outputMembers[n], bodyBinding.Use, cpd);
					methodEnd.Parameters.Add (GenerateParameter (outputMembers[n], FieldDirection.Out));
				}
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
			
			if (Style == ServiceDescriptionImportStyle.Client) 
			{
				// Invoke call
				
				CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
				CodePrimitiveExpression varMsgName = new CodePrimitiveExpression (messageName);
				CodeMethodInvokeExpression inv;
				CodeVariableDeclarationStatement dec;
	
				inv = new CodeMethodInvokeExpression (ethis, "Invoke", varMsgName, methodParams);
				if (outputMembers != null && outputMembers.Count > 0)
				{
					dec = new CodeVariableDeclarationStatement (typeof(object[]), varResults, inv);
					method.Statements.Add (dec);
					method.Statements.AddRange (outAssign);
				}
				else
					method.Statements.Add (inv);
				
				// Begin Invoke Call
				
				CodeExpression expCallb = new CodeVariableReferenceExpression (varCallback);
				CodeExpression expAsyncs = new CodeVariableReferenceExpression (varAsyncState);
				inv = new CodeMethodInvokeExpression (ethis, "BeginInvoke", varMsgName, methodParams, expCallb, expAsyncs);
				methodBegin.Statements.Add (new CodeMethodReturnStatement (inv));
				
				// End Invoke call
				
				CodeExpression varAsyncr = new CodeVariableReferenceExpression (varAsyncResult);
				inv = new CodeMethodInvokeExpression (ethis, "EndInvoke", varAsyncr);
				if (outputMembers != null && outputMembers.Count > 0)
				{
					dec = new CodeVariableDeclarationStatement (typeof(object[]), varResults, inv);
					methodEnd.Statements.Add (dec);
					methodEnd.Statements.AddRange (outAssign);
				}
				else
					methodEnd.Statements.Add (inv);
			}
			else {
				method.Attributes = MemberAttributes.Public | MemberAttributes.Abstract;
			}
			
			// Attributes
			
			ImportHeaders (method);
			
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.WebMethodAttribute");
			if (messageName != method.Name) att.Arguments.Add (GetArg ("MessageName",messageName));
			AddCustomAttribute (method, att, (Style == ServiceDescriptionImportStyle.Server));
			
			if (style == SoapBindingStyle.Rpc)
			{
				att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.SoapRpcMethodAttribute");
				att.Arguments.Add (GetArg (soapOper.SoapAction));
				if (inputMembers.ElementName != method.Name) att.Arguments.Add (GetArg ("RequestElementName", inputMembers.ElementName));
				if (outputMembers != null && outputMembers.ElementName != (method.Name + "Response")) att.Arguments.Add (GetArg ("ResponseElementName", outputMembers.ElementName));
				att.Arguments.Add (GetArg ("RequestNamespace", inputMembers.Namespace));
				if (outputMembers != null) att.Arguments.Add (GetArg ("ResponseNamespace", outputMembers.Namespace));
				if (outputMembers == null) att.Arguments.Add (GetArg ("OneWay", true));
			}
			else
			{
				if (outputMembers != null && (inputMembers.ElementName == "" && outputMembers.ElementName != "" || 
					inputMembers.ElementName != "" && outputMembers.ElementName == ""))
					throw new InvalidOperationException ("Parameter style is not the same for the input message and output message");
	
				att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.SoapDocumentMethodAttribute");
				att.Arguments.Add (GetArg (soapOper.SoapAction));
				if (inputMembers.ElementName != "") {
					if (inputMembers.ElementName != method.Name) att.Arguments.Add (GetArg ("RequestElementName", inputMembers.ElementName));
					if (outputMembers != null && outputMembers.ElementName != (method.Name + "Response")) att.Arguments.Add (GetArg ("ResponseElementName", outputMembers.ElementName));
					att.Arguments.Add (GetArg ("RequestNamespace", inputMembers.Namespace));
					if (outputMembers != null) att.Arguments.Add (GetArg ("ResponseNamespace", outputMembers.Namespace));
					att.Arguments.Add (GetEnumArg ("ParameterStyle", "System.Web.Services.Protocols.SoapParameterStyle", "Wrapped"));
				}
				else
					att.Arguments.Add (GetEnumArg ("ParameterStyle", "System.Web.Services.Protocols.SoapParameterStyle", "Bare"));
					
				if (outputMembers == null) att.Arguments.Add (GetArg ("OneWay", true));
					
				att.Arguments.Add (GetEnumArg ("Use", "System.Web.Services.Description.SoapBindingUse", bodyBinding.Use.ToString()));
			}
			
			AddCustomAttribute (method, att, true);
			
			CodeTypeDeclaration.Members.Add (method);
			
			if (Style == ServiceDescriptionImportStyle.Client) {
				CodeTypeDeclaration.Members.Add (methodBegin);
				CodeTypeDeclaration.Members.Add (methodEnd);
			}
			
			return method;
		}
		
		CodeParameterDeclarationExpression GenerateParameter (XmlMemberMapping member, FieldDirection dir)
		{
			CodeParameterDeclarationExpression par = new CodeParameterDeclarationExpression (member.TypeFullName, member.MemberName);
			par.Direction = dir;
			return par;
		}
		
		void GenerateMemberAttributes (XmlMembersMapping members, XmlMemberMapping member, SoapBindingUse use, CodeParameterDeclarationExpression param)
		{
			if (use == SoapBindingUse.Literal)
				xmlExporter.AddMappingMetadata (param.CustomAttributes, member, members.Namespace);
			else
				soapExporter.AddMappingMetadata (param.CustomAttributes, member);
		}
		
		void GenerateReturnAttributes (XmlMembersMapping members, XmlMemberMapping member, SoapBindingUse use, CodeMemberMethod method)
		{
			if (use == SoapBindingUse.Literal)
				xmlExporter.AddMappingMetadata (method.ReturnTypeCustomAttributes, member, members.Namespace, (member.ElementName != method.Name + "Result"));
			else
				soapExporter.AddMappingMetadata (method.ReturnTypeCustomAttributes, member, (member.ElementName != method.Name + "Result"));
		}
		
		void ImportHeaders (CodeMemberMethod method)
		{
			foreach (object ob in OperationBinding.Input.Extensions)
			{
				SoapHeaderBinding hb = ob as SoapHeaderBinding;
				if (hb == null) continue;
				if (HasHeader (OperationBinding.Output, hb)) 
					ImportHeader (method, hb, SoapHeaderDirection.In | SoapHeaderDirection.Out);
				else
					ImportHeader (method, hb, SoapHeaderDirection.In);
			}
			
			if (OperationBinding.Output == null) return;
			
			foreach (object ob in OperationBinding.Output.Extensions)
			{
				SoapHeaderBinding hb = ob as SoapHeaderBinding;
				if (hb == null) continue;
				if (!HasHeader (OperationBinding.Input, hb)) 
					ImportHeader (method, hb, SoapHeaderDirection.Out);
			}
		}
		
		bool HasHeader (MessageBinding msg, SoapHeaderBinding hb)
		{
			if (msg == null) return false;
			
			foreach (object ob in msg.Extensions) 
			{
				SoapHeaderBinding mhb = ob as SoapHeaderBinding;
				if ((mhb != null) && (mhb.Message == hb.Message) && (mhb.Part == hb.Part)) 
					return true;
			}
			return false;
		}
		
		void ImportHeader (CodeMemberMethod method, SoapHeaderBinding hb, SoapHeaderDirection direction)
		{
			Message msg = ServiceDescriptions.GetMessage (hb.Message);
			if (msg == null) throw new InvalidOperationException ("Message " + hb.Message + " not found");
			MessagePart part = msg.Parts [hb.Part];
			if (part == null) throw new InvalidOperationException ("Message part " + hb.Part + " not found in message " + hb.Message);

			XmlTypeMapping map;
			if (hb.Use == SoapBindingUse.Literal)
			{
				map = xmlImporter.ImportDerivedTypeMapping (part.Element, typeof (SoapHeader));
				xmlExporter.ExportTypeMapping (map);
			}
			else
			{
				map = soapImporter.ImportDerivedTypeMapping (part.Type, typeof (SoapHeader), true);
				soapExporter.ExportTypeMapping (map);
			}

			bool required = false;

			string varName = headerVariables [map] as string;
			if (varName == null) 
			{
				varName = memberIds.AddUnique(CodeIdentifier.MakeValid (map.TypeName + "Value"),hb);
				headerVariables.Add (map, varName);
				CodeMemberField codeField = new CodeMemberField (map.TypeFullName, varName);
				codeField.Attributes = MemberAttributes.Public;
				CodeTypeDeclaration.Members.Add (codeField);
			}
			
			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.SoapHeaderAttribute");
			att.Arguments.Add (GetArg (varName));
			att.Arguments.Add (GetArg ("Required", required));
			if (direction != SoapHeaderDirection.In) att.Arguments.Add (GetEnumArg ("Direction", "System.Web.Services.Protocols.SoapHeaderDirection", direction.ToString ()));
			AddCustomAttribute (method, att, true);
		}
		
		#endregion
	}
}
