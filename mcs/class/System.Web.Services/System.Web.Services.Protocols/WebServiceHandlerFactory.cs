// 
// System.Web.Services.Protocols.WebServiceHandlerFactory.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
//	Dave Bettin (dave@opendotnet.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.IO;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.UI;
using System.Collections.Specialized;

namespace System.Web.Services.Protocols
{
	class DummyHttpHandler : IHttpHandler
	{
		bool IHttpHandler.IsReusable {
			get { return false; }
		}

		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			// Do nothing
		}
	}
	
	public class WebServiceHandlerFactory : IHttpHandlerFactory
	{

		#region Constructors

		public WebServiceHandlerFactory () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public IHttpHandler GetHandler (HttpContext context, string verb, string url, string filePath)
		{
			Type type = WebServiceParser.GetCompiledType (filePath, context);

			WSProtocol protocol = GuessProtocol (context, verb);
			IHttpHandler handler = null;

			if (!WSConfig.IsSupported (protocol))
				return new DummyHttpHandler ();

			switch (protocol) {
			case WSProtocol.HttpSoap:
				handler = new HttpSoapWebServiceHandler (type);
				break;
			case WSProtocol.HttpPost:
				handler = new HttpPostWebServiceHandler (type);
				break;
			case WSProtocol.HttpGet:
				handler = new HttpGetWebServiceHandler (type);
				break;
			case WSProtocol.Documentation:
				HttpRequest req = context.Request;
				string key = null;
				if (req.QueryString.Count == 1)
					key = req.QueryString.GetKey(0).ToLower();

				SoapDocumentationHandler soapHandler;
				soapHandler = new SoapDocumentationHandler (type, context);
				handler = soapHandler;
				
				if (key != null && (key == "wsdl" || key == "schema" || key == "code"))
					return handler;

				context.Items["wsdls"] = soapHandler.GetDescriptions ();
				context.Items["schemas"] = soapHandler.GetSchemas ();

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
				handler = PageParser.GetCompiledPageInstance (vpath, physPath, context);
				break;
			}

			return handler;
		}

		static WSProtocol GuessProtocol (HttpContext context, string verb)
		{
			if (context.Request.PathInfo == null || context.Request.PathInfo == "")
			{
				if (context.Request.RequestType == "GET")
					return WSProtocol.Documentation;
				else
					return WSProtocol.HttpSoap;
			}
			else
			{
				if (context.Request.RequestType == "GET")
					return WSProtocol.HttpGet;
				else
					return WSProtocol.HttpPost;
			}
		}

		public void ReleaseHandler (IHttpHandler handler)
		{
		}

		#endregion // Methods
	}
}
