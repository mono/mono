// 
// System.Web.Services.Description.HttpSimpleProtocolImporter.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
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
	public abstract class HttpSimpleProtocolImporter : ProtocolImporter 
	{

		#region Fields

		HttpBinding httpBinding;
		
		SoapCodeExporter soapExporter;
		SoapSchemaImporter soapImporter;
		XmlCodeExporter xmlExporter;
		XmlSchemaImporter xmlImporter;
		CodeIdentifiers memberIds;
		
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
			HttpAddressBinding sab = (HttpAddressBinding) Port.Extensions.Find (typeof(HttpAddressBinding));
			if (sab != null) location = sab.Location;
			string url = GetServiceUrl (location); 
			
			CodeConstructor cc = new CodeConstructor ();
			cc.Attributes = MemberAttributes.Public;
			CodeExpression ce = new CodeFieldReferenceExpression (new CodeThisReferenceExpression(), "Url");
			CodeAssignStatement cas = new CodeAssignStatement (ce, new CodePrimitiveExpression (url));
			cc.Statements.Add (cas);
			codeClass.Members.Add (cc);
			
			memberIds = new CodeIdentifiers ();
			return codeClass;
		}

		protected override void BeginNamespace ()
		{
			xmlImporter = new XmlSchemaImporter (Schemas, ClassNames);
			soapImporter = new SoapSchemaImporter (Schemas, ClassNames);
			xmlExporter = new XmlCodeExporter (CodeNamespace, null);
		}

		protected override void EndClass ()
		{
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
				XmlMembersMapping outputMembers = ImportOutMembersMapping (OutputMessage);
				
				CodeMemberMethod met = GenerateMethod (memberIds, httpOper, inputMembers, outputMembers);
				
				xmlExporter.ExportMembersMapping (inputMembers);
				xmlExporter.ExportMembersMapping (outputMembers);

				return met;
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex);
				UnsupportedOperationBindingWarning (ex.Message);
				return null;
			}
		}

		XmlMembersMapping ImportInMembersMapping (Message msg)
		{
			XmlQualifiedName elem = null;
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
		
		XmlMembersMapping ImportOutMembersMapping (Message msg)
		{
			if (msg.Parts.Count == 1 && msg.Parts[0].Name == "Body" && msg.Parts[0].Element == XmlQualifiedName.Empty)
				return xmlImporter.ImportAnyType (XmlQualifiedName.Empty,"");
			
			XmlQualifiedName[] pnames = new XmlQualifiedName [msg.Parts.Count];
			for (int n=0; n<pnames.Length; n++)
				pnames[n] = msg.Parts[n].Element;
			return xmlImporter.ImportMembersMapping (pnames);
		}
		
		CodeMemberMethod GenerateMethod (CodeIdentifiers memberIds, HttpOperationBinding httpOper, XmlMembersMapping inputMembers, XmlMembersMapping outputMembers)
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

			string messageName = memberIds.AddUnique(CodeIdentifier.MakeValid(Operation.Name),method);

			method.Name = Operation.Name;
			methodBegin.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("Begin" + memberIds.MakeRightCase(Operation.Name)),method);
			methodEnd.Name = memberIds.AddUnique(CodeIdentifier.MakeValid("End" + memberIds.MakeRightCase(Operation.Name)),method);

			method.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.ReturnType = new CodeTypeReference (typeof(void));
			methodEnd.Parameters.Add (new CodeParameterDeclarationExpression (typeof (IAsyncResult),varAsyncResult));

			CodeExpression[] paramArray = new CodeExpression [inputMembers.Count];
			CodeParameterDeclarationExpression[] outParams = new CodeParameterDeclarationExpression [outputMembers.Count];

			for (int n=0; n<inputMembers.Count; n++)
			{
				CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (inputMembers[n].TypeFullName, inputMembers[n].MemberName);
				param.Direction = FieldDirection.In;
				method.Parameters.Add (param);
				methodBegin.Parameters.Add (param);
				paramArray [n] = new CodeVariableReferenceExpression (param.Name);
			}

			bool isVoid = true;
			if (outputMembers.Count == 1)
			{
				method.ReturnType = new CodeTypeReference (outputMembers[0].TypeFullName);
				methodEnd.ReturnType = new CodeTypeReference (outputMembers[0].TypeFullName);
				xmlExporter.AddMappingMetadata (method.ReturnTypeCustomAttributes, outputMembers[0], outputMembers.Namespace, (outputMembers[0].ElementName != method.Name + "Result"));
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

		#endregion
	}
}
