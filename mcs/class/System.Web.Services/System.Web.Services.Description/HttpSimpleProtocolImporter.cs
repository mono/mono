// 
// System.Web.Services.Description.HttpSimpleProtocolImporter.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
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
using System.Xml.Serialization;
using System.Configuration;
using System.Collections;

namespace System.Web.Services.Description 
{
	internal abstract class HttpSimpleProtocolImporter : ProtocolImporter 
	{

		#region Fields

		HttpBinding httpBinding;
		
		SoapCodeExporter soapExporter;
		SoapSchemaImporter soapImporter;
		XmlCodeExporter xmlExporter;
		XmlSchemaImporter xmlImporter;
		CodeIdentifiers memberIds;
		XmlReflectionImporter xmlReflectionImporter;
		
		#endregion // Fields

		#region Constructors

		public HttpSimpleProtocolImporter ()
		{
		}
		
		#endregion // Constructors

		#region Methods

		protected override CodeTypeDeclaration BeginClass ()
		{
			httpBinding = (HttpBinding) Binding.Extensions.Find (typeof(HttpBinding));

			CodeTypeDeclaration codeClass = new CodeTypeDeclaration (ClassName);

			string location = null;
			string url = null;
			if (Port != null) {
				HttpAddressBinding sab = (HttpAddressBinding) Port.Extensions.Find (typeof(HttpAddressBinding));
				if (sab != null) location = sab.Location;
				url = GetServiceUrl (location); 
			}
			
			CodeConstructor cc = new CodeConstructor ();
			cc.Attributes = MemberAttributes.Public;
			if (url != null) {
				CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
				CodeAssignStatement cas = new CodeAssignStatement (ce, new CodePrimitiveExpression (url));
				cc.Statements.Add (cas);
			}
			codeClass.Members.Add (cc);
			
			memberIds = new CodeIdentifiers ();
			return codeClass;
		}

		protected override void BeginNamespace ()
		{
			xmlImporter = new XmlSchemaImporter (LiteralSchemas, ClassNames);
			soapImporter = new SoapSchemaImporter (EncodedSchemas, ClassNames);
			xmlExporter = new XmlCodeExporter (CodeNamespace, null);
			xmlReflectionImporter = new XmlReflectionImporter ();
		}

		protected override void EndClass ()
		{
			if (xmlExporter.IncludeMetadata.Count > 0)
			{
				if (CodeTypeDeclaration.CustomAttributes == null)
					CodeTypeDeclaration.CustomAttributes = new CodeAttributeDeclarationCollection ();
				CodeTypeDeclaration.CustomAttributes.AddRange (xmlExporter.IncludeMetadata);
			}
		}

		protected override void EndNamespace ()
		{
		}

		protected override bool IsBindingSupported ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool IsOperationFlowSupported (OperationFlow flow)
		{
			throw new NotImplementedException ();
		}

		protected override CodeMemberMethod GenerateMethod ()
		{
			try
			{
				HttpOperationBinding httpOper = OperationBinding.Extensions.Find (typeof (HttpOperationBinding)) as HttpOperationBinding;
				if (httpOper == null) throw new Exception ("Http operation binding not found");
				
				XmlMembersMapping inputMembers = ImportInMembersMapping (InputMessage);
				XmlTypeMapping outputMember = ImportOutMembersMapping (OutputMessage);
				
				CodeMemberMethod met = GenerateMethod (memberIds, httpOper, inputMembers, outputMember);
				
				xmlExporter.ExportMembersMapping (inputMembers);
				if (outputMember != null)
					xmlExporter.ExportTypeMapping (outputMember);

				return met;
			}
			catch (Exception ex)
			{
				UnsupportedOperationBindingWarning (ex.Message);
				return null;
			}
		}

		XmlMembersMapping ImportInMembersMapping (Message msg)
		{
			SoapSchemaMember[] mems = new SoapSchemaMember [msg.Parts.Count];
			for (int n=0; n<mems.Length; n++)
			{
				SoapSchemaMember mem = new SoapSchemaMember();
				mem.MemberName = msg.Parts[n].Name;
				mem.MemberType = msg.Parts[n].Type;
				mems[n] = mem;
			}
			return soapImporter.ImportMembersMapping (Operation.Name, "", mems);
		}
		
		XmlTypeMapping ImportOutMembersMapping (Message msg)
		{
			if (msg.Parts.Count == 0) return null;
			
			if (msg.Parts[0].Name == "Body" && msg.Parts[0].Element == XmlQualifiedName.Empty)
				return xmlReflectionImporter.ImportTypeMapping (typeof(XmlNode));
			else
				return xmlImporter.ImportTypeMapping (msg.Parts[0].Element);
		}
		
		CodeMemberMethod GenerateMethod (CodeIdentifiers memberIds, HttpOperationBinding httpOper, XmlMembersMapping inputMembers, XmlTypeMapping outputMember)
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

			string varAsyncResult = pids.AddUnique ("asyncResult","asyncResult");
			string varResults = pids.AddUnique ("results","results");
			string varCallback = pids.AddUnique ("callback","callback");
			string varAsyncState = pids.AddUnique ("asyncState","asyncState");

			string messageName = memberIds.AddUnique(CodeIdentifier.MakeValid(Operation.Name),method);

			method.Name = Operation.Name;
			methodBegin.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("Begin" + Operation.Name),method);
			methodEnd.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("End" + Operation.Name),method);

			method.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.Parameters.Add (new CodeParameterDeclarationExpression (typeof (IAsyncResult),varAsyncResult));

			CodeExpression[] paramArray = new CodeExpression [inputMembers.Count];

			for (int n=0; n<inputMembers.Count; n++)
			{
				string ptype = GetSimpleType (inputMembers[n]);
				CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (ptype, inputMembers[n].MemberName);
				
				param.Direction = FieldDirection.In;
				method.Parameters.Add (param);
				methodBegin.Parameters.Add (param);
				paramArray [n] = new CodeVariableReferenceExpression (param.Name);
			}

			bool isVoid = true;
			if (outputMember != null)
			{
				method.ReturnType = new CodeTypeReference (outputMember.TypeFullName);
				methodEnd.ReturnType = new CodeTypeReference (outputMember.TypeFullName);
				xmlExporter.AddMappingMetadata (method.ReturnTypeCustomAttributes, outputMember, "");
				isVoid = false;
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

			// Generate method url
			
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			
			CodeExpression thisURlExp = new CodeFieldReferenceExpression (ethis, "Url");
			CodePrimitiveExpression metUrl = new CodePrimitiveExpression (httpOper.Location);
			CodeBinaryOperatorExpression expMethodLocation = new CodeBinaryOperatorExpression (thisURlExp, CodeBinaryOperatorType.Add, metUrl);
			
			// Invoke call
			
			CodePrimitiveExpression varMsgName = new CodePrimitiveExpression (messageName);
			CodeMethodInvokeExpression inv;

			inv = new CodeMethodInvokeExpression (ethis, "Invoke", varMsgName, expMethodLocation, methodParams);
			if (!isVoid)
				method.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (method.ReturnType, inv)));
			else
				method.Statements.Add (inv);
			
			// Begin Invoke Call
			
			CodeExpression expCallb = new CodeVariableReferenceExpression (varCallback);
			CodeExpression expAsyncs = new CodeVariableReferenceExpression (varAsyncState);
			inv = new CodeMethodInvokeExpression (ethis, "BeginInvoke", varMsgName, expMethodLocation, methodParams, expCallb, expAsyncs);
			methodBegin.Statements.Add (new CodeMethodReturnStatement (inv));
			
			// End Invoke call
			
			CodeExpression varAsyncr = new CodeVariableReferenceExpression (varAsyncResult);
			inv = new CodeMethodInvokeExpression (ethis, "EndInvoke", varAsyncr);
			if (!isVoid)
				methodEnd.Statements.Add (new CodeMethodReturnStatement (new CodeCastExpression (methodEnd.ReturnType, inv)));
			else
				methodEnd.Statements.Add (inv);
			
			// Attributes

			CodeAttributeDeclaration att = new CodeAttributeDeclaration ("System.Web.Services.Protocols.HttpMethodAttribute");
			att.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(GetOutMimeFormatter ())));
			att.Arguments.Add (new CodeAttributeArgument (new CodeTypeOfExpression(GetInMimeFormatter ())));
			AddCustomAttribute (method, att, true);
		
			CodeTypeDeclaration.Members.Add (method);
			CodeTypeDeclaration.Members.Add (methodBegin);
			CodeTypeDeclaration.Members.Add (methodEnd);
			
			return method;
		}		

#if NET_2_0
		internal override CodeExpression BuildInvokeAsync (string messageName, CodeArrayCreateExpression paramsArray, CodeExpression delegateField, CodeExpression userStateVar)
		{
			HttpOperationBinding httpOper = OperationBinding.Extensions.Find (typeof (HttpOperationBinding)) as HttpOperationBinding;
			
			CodeThisReferenceExpression ethis = new CodeThisReferenceExpression();
			
			CodeExpression thisURlExp = new CodeFieldReferenceExpression (ethis, "Url");
			CodePrimitiveExpression metUrl = new CodePrimitiveExpression (httpOper.Location);
			CodeBinaryOperatorExpression expMethodLocation = new CodeBinaryOperatorExpression (thisURlExp, CodeBinaryOperatorType.Add, metUrl);
			
			CodeMethodInvokeExpression inv2 = new CodeMethodInvokeExpression (ethis, "InvokeAsync");
			inv2.Parameters.Add (new CodePrimitiveExpression (messageName));
			inv2.Parameters.Add (expMethodLocation);
			inv2.Parameters.Add (paramsArray);
			inv2.Parameters.Add (delegateField);
			inv2.Parameters.Add (userStateVar);
			return inv2;
		}
#endif
		
		protected virtual Type GetInMimeFormatter ()
		{
			return null;
		}

		protected virtual Type GetOutMimeFormatter ()
		{
			if (OperationBinding.Output.Extensions.Find (typeof(MimeXmlBinding)) != null)
				return typeof (XmlReturnReader);
				
			MimeContentBinding bin = (MimeContentBinding) OperationBinding.Output.Extensions.Find (typeof(MimeContentBinding));
			if (bin != null && bin.Type == "text/xml")
				return typeof (XmlReturnReader);
				
			return typeof(NopReturnReader);
		}
		
		string GetSimpleType (XmlMemberMapping member)
		{
			// MS seems to always use System.String for input parameters, except for byte[]
			
			switch (member.TypeName)
			{
				case "hexBinary":
				case "base64Binary":
					return "System.String";
				
				default:
					string ptype = member.TypeFullName;
					int i = ptype.IndexOf ('[');
					if (i == -1)
						return "System.String";
					else 
						return "System.String" + ptype.Substring (i);
			}
		}

		#endregion
	}
}
