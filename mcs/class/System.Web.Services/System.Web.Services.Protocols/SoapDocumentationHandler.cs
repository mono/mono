//
// System.Web.Services.Protocols.SoapDocumentationHandler.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
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
using System.Web;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Web.Compilation;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Configuration;
using System.Configuration;
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
			int i = _url.IndexOf ('?');
			if (i != -1) _url = _url.Substring (0,i);
			_typeStubInfo = (SoapTypeStubInfo) TypeStubManager.GetTypeStub (ServiceType, "Soap");
			
			HttpRequest req = context.Request;
			string key = null;
			if (req.QueryString.Count == 1) {
				key = req.QueryString.GetKey (0);
				if (key == null)
					key = req.QueryString [0];

				if (key != null)
					key = key.ToLower (CultureInfo.InvariantCulture);
			}
				
			if (key == "wsdl" || key == "schema" || key == "code" || key == "disco")
				return;
				
#if NET_2_0
			string help = WebServicesSection.Current.WsdlHelpGenerator.Href;
			string path = Path.GetDirectoryName (ConfigurationManager.OpenMachineConfiguration().FilePath);
#else
			string help = WSConfig.Instance.WsdlHelpPage;
			string path = Path.GetDirectoryName (WSConfig.Instance.ConfigFilePath);
#endif
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
			
#if !TARGET_JVM
			if (!File.Exists (physPath))
				throw new InvalidOperationException ("Documentation page '" + physPath + "' not found");
#endif

			_pageHandler = PageParser.GetCompiledPageInstance (vpath, physPath, context);
		}

		internal IHttpHandler PageHandler {
			get { return _pageHandler; }
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
				string key = req.QueryString.GetKey (0);
				if (key == null)
					key = req.QueryString [0];

				if (key != null)
					key = key.ToLower (CultureInfo.InvariantCulture);

				if (key  == "wsdl") GenerateWsdlDocument (context, req.QueryString ["wsdl"]);
				else if (key == "schema") GenerateSchema (context, req.QueryString ["schema"]);
#if !TARGET_JVM //code generation is not supported
				else if (key == "code") GenerateCode (context, req.QueryString ["code"]);
#else
				else if (key == "code") throw new Exception("Code generation is not supported.");
#endif
				else if (key == "disco") GenerateDiscoDocument (context);
				else throw new Exception ("This should never happen");
			}
		}

		void GenerateWsdlDocument (HttpContext context, string wsdlId)
		{
			int di = 0;
			if (wsdlId != null && wsdlId != "") di = int.Parse (wsdlId);
			
			context.Response.ContentType = "text/xml; charset=utf-8";
			XmlTextWriter xtw = new XmlTextWriter (context.Response.OutputStream, new UTF8Encoding (false));
			xtw.Formatting = Formatting.Indented;
			GetDescriptions() [di].Write (xtw);
		}
		
		void GenerateDiscoDocument (HttpContext context)
		{
			ServiceDescriptionCollection descs = GetDescriptions ();
			
			DiscoveryDocument doc = new DiscoveryDocument ();
			ContractReference cref = new ContractReference ();
			cref.Ref = _url + "?wsdl";
			cref.DocRef = _url;
			doc.References.Add (cref);
			
			foreach (ServiceDescription desc in descs)
				foreach (Service ser in desc.Services)
					foreach (Port port in ser.Ports)
					{
						SoapAddressBinding sab = port.Extensions.Find (typeof(SoapAddressBinding)) as SoapAddressBinding;
						if (sab != null)
						{
							System.Web.Services.Discovery.SoapBinding dsb = new System.Web.Services.Discovery.SoapBinding ();
							dsb.Address = sab.Location;
							dsb.Binding = port.Binding;
							doc.AdditionalInfo.Add (dsb);
						}
					}

			context.Response.ContentType = "text/xml; charset=utf-8";
			XmlTextWriter xtw = new XmlTextWriter (context.Response.OutputStream, new UTF8Encoding (false));
			xtw.Formatting = Formatting.Indented;
			doc.Write (xtw);
		}
		
		void GenerateSchema (HttpContext context, string schemaId)
		{
			int di = -1;
			if (schemaId != null && schemaId != "") {
				try {
					di = int.Parse (schemaId);
				} catch {
					XmlSchemas xss = GetSchemas ();
					for (int i = 0; i < xss.Count; i++) {
						if (xss [i].Id == schemaId) {
							di = i;
							break;
						}
					}
				}
				if (di < 0)
					throw new InvalidOperationException (String.Format ("HTTP parameter 'schema' needs to specify an Id of a schema in the schemas. {0} points to nowhere.", schemaId));
			}
			context.Response.ContentType = "text/xml; charset=utf-8";
			XmlTextWriter xtw = new XmlTextWriter (context.Response.OutputStream, new UTF8Encoding (false));
			xtw.Formatting = Formatting.Indented;
			GetSchemas() [di].Write (xtw);
		}

#if !TARGET_JVM		
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
#endif
		
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
