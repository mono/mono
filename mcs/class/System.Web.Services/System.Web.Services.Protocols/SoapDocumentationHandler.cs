//
// System.Web.Services.Protocols.SoapDocumentationHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Web;
using System.IO;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Web.Services.Description;
using System.Web.Services.Configuration;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace System.Web.Services.Protocols
{
	internal class SoapDocumentationHandler: WebServiceHandler
	{
		Type _type;
		TypeStubInfo _typeStubInfo;
		ServiceDescriptionCollection _descriptions;
		XmlSchemas _schemas;
		string _url;

		public SoapDocumentationHandler (Type type, HttpContext context)
		{
			_type = type;
			_url = context.Request.Url.ToString();
			int i = _url.LastIndexOf ('?');
			if (i != -1) _url = _url.Substring (0,i);
			_typeStubInfo = TypeStubManager.GetTypeStub (_type);
		}

		public override bool IsReusable 
		{
			get { return false; }
		}

		public override void ProcessRequest (HttpContext context)
		{
			HttpRequest req = context.Request;
			if (req.QueryString.Count == 1)
			{
				string key = req.QueryString.GetKey(0).ToLower();
				if (key  == "wsdl") GenerateWsdlDocument (context, req.QueryString ["wsdl"]);
				else if (key == "schema") GenerateSchema (context, req.QueryString ["schema"]);
				else if (key == "code") GenerateCode (context, req.QueryString ["code"]);
				else GenerateDocumentationPage (context);
			}
			else
				GenerateDocumentationPage (context);
		}

		void GenerateDocumentationPage (HttpContext context)
		{
			context.Items["wsdls"] = GetDescriptions ();
			context.Items["schemas"] = GetSchemas ();
			context.Server.Transfer (WSConfig.Instance.WsdlHelpPage, true);
		}

		void GenerateWsdlDocument (HttpContext context, string wsdlId)
		{
			int di = 0;
			if (wsdlId != null && wsdlId != "") di = int.Parse (wsdlId);
			
			context.Response.ContentType = "text/xml; charset=utf-8";
			Stream outStream = context.Response.OutputStream;
			GetDescriptions() [di].Write (outStream);
			outStream.Close ();
		}
		
		void GenerateSchema (HttpContext context, string schemaId)
		{
			int di = 0;
			if (schemaId != null && schemaId != "") di = int.Parse (schemaId);
			
			context.Response.ContentType = "text/xml; charset=utf-8";
			Stream outStream = context.Response.OutputStream;
			GetSchemas() [di].Write (outStream);
			outStream.Close ();
		}
		
		void GenerateCode (HttpContext context, string langId)
		{
			context.Response.ContentType = "text/plain; charset=utf-8";
			CodeNamespace codeNamespace = new CodeNamespace();
			CodeCompileUnit codeUnit = new CodeCompileUnit();
			
			codeUnit.Namespaces.Add (codeNamespace);

			ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
			
			foreach (ServiceDescription sd in GetDescriptions ())
				importer.AddServiceDescription(sd, null, null);

			foreach (XmlSchema sc in GetSchemas())
				importer.Schemas.Add (sc);

			importer.Import(codeNamespace, codeUnit);
			
			if (langId == null || langId == "") langId = "cs";
			CodeDomProvider provider = GetProvider (langId);
			ICodeGenerator generator = provider.CreateGenerator();
			CodeGeneratorOptions options = new CodeGeneratorOptions();
			
			StreamWriter writer = new StreamWriter(context.Response.OutputStream);
			generator.GenerateCodeFromCompileUnit(codeUnit, writer, options);
			writer.Close ();
		}
		
		private CodeDomProvider GetProvider(string langId)
		{
			// FIXME these should be loaded dynamically using reflection
			CodeDomProvider provider;
			
			switch (langId.ToUpper())
			{
			    case "CS":
				    provider = new CSharpCodeProvider();
				    break;
			    
			    default:
				    throw new Exception("Unknown language: " + langId);
			}

			return provider;
		}
		
		ServiceDescriptionCollection GetDescriptions ()
		{
			if (_descriptions == null)
			{
				ServiceDescriptionReflector reflector = new ServiceDescriptionReflector ();
				reflector.Reflect (_type,_url);
				_schemas = reflector.Schemas;
				_descriptions = reflector.ServiceDescriptions;
			}
			return _descriptions;
		}
		
		XmlSchemas GetSchemas()
		{
			GetDescriptions();
			return _schemas;
		}
	}
}
