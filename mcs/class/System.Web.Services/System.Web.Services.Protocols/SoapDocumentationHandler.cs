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
using System.Web.Services.Discovery;
using System.Web.Services.Configuration;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Web.UI;

namespace System.Web.Services.Protocols
{
	internal class SoapDocumentationHandler: WebServiceHandler
	{
		SoapTypeStubInfo _typeStubInfo;
		ServiceDescriptionCollection _descriptions;
		XmlSchemas _schemas;
		string _url;
		IHttpHandler _pageHandler = null;

		public SoapDocumentationHandler (Type type, HttpContext context): base (type)
		{
			_url = context.Request.Url.ToString();
			int i = _url.LastIndexOf ('?');
			if (i != -1) _url = _url.Substring (0,i);
			_typeStubInfo = (SoapTypeStubInfo) TypeStubManager.GetTypeStub (ServiceType, "Soap");
			
			HttpRequest req = context.Request;
			string key = null;
			if (req.QueryString.Count == 1)
				key = req.QueryString.GetKey(0).ToLower();
				
			if (key == "wsdl" || key == "schema" || key == "code" || key == "disco")
				return;
				
			string help = WSConfig.Instance.WsdlHelpPage;
			string path = Path.GetDirectoryName (WSConfig.Instance.ConfigFilePath);
			string file = Path.GetFileName (WSConfig.Instance.ConfigFilePath);
			string appPath = AppDomain.CurrentDomain.GetData (".appPath").ToString ();
			string vpath;
			if (path.StartsWith (appPath)) {
				vpath = path.Substring (appPath.Length);
				vpath = vpath.Replace ("\\", "/");
			} else {
				vpath = "/";
			}

			if (vpath.EndsWith ("/"))
				vpath += help;
			else
				vpath += "/" + help;

			string physPath = Path.Combine (path, help);
			
			if (!File.Exists (physPath))
				throw new InvalidOperationException ("Documentation page '" + physPath + "' not found");

			_pageHandler = PageParser.GetCompiledPageInstance (vpath, physPath, context);
				
		}

		public override bool IsReusable 
		{
			get { return false; }
		}

		public override void ProcessRequest (HttpContext context)
		{
			if (_pageHandler != null)
			{
				context.Items["wsdls"] = GetDescriptions ();
				context.Items["schemas"] = GetSchemas ();
				_pageHandler.ProcessRequest (context);
			}
			else
			{
				HttpRequest req = context.Request;
				string key = req.QueryString.GetKey(0).ToLower();
				if (key  == "wsdl") GenerateWsdlDocument (context, req.QueryString ["wsdl"]);
				else if (key == "schema") GenerateSchema (context, req.QueryString ["schema"]);
				else if (key == "code") GenerateCode (context, req.QueryString ["code"]);
				else if (key == "disco") GenerateDiscoDocument (context);
				else throw new Exception ("This should never happen");
			}
		}

		void GenerateWsdlDocument (HttpContext context, string wsdlId)
		{
			int di = 0;
			if (wsdlId != null && wsdlId != "") di = int.Parse (wsdlId);
			
			context.Response.ContentType = "text/xml; charset=utf-8";
			GetDescriptions() [di].Write (context.Response.OutputStream);
		}
		
		void GenerateDiscoDocument (HttpContext context)
		{
			DiscoveryDocument doc = new DiscoveryDocument ();
			ContractReference cref = new ContractReference ();
			cref.Ref = _url + "?wsdl";
			cref.DocRef = _url;
			doc.References.Add (cref);

			context.Response.ContentType = "text/xml; charset=utf-8";
			doc.Write (context.Response.OutputStream);
		}
		
		void GenerateSchema (HttpContext context, string schemaId)
		{
			int di = 0;
			if (schemaId != null && schemaId != "") di = int.Parse (schemaId);
			
			context.Response.ContentType = "text/xml; charset=utf-8";
			GetSchemas() [di].Write (context.Response.OutputStream);
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
			
			generator.GenerateCodeFromCompileUnit(codeUnit, context.Response.Output, options);
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
		
		internal ServiceDescriptionCollection GetDescriptions ()
		{
			if (_descriptions == null)
			{
				ServiceDescriptionReflector reflector = new ServiceDescriptionReflector ();
				reflector.Reflect (ServiceType,_url);
				_schemas = reflector.Schemas;
				_descriptions = reflector.ServiceDescriptions;
			}
			return _descriptions;
		}
		
		internal XmlSchemas GetSchemas()
		{
			GetDescriptions();
			return _schemas;
		}
	}
}
