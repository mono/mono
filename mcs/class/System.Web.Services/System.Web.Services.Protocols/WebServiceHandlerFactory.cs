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
using System.Web.SessionState;
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
	
	class SimpleSyncSessionHandler : HttpSimpleWebServiceHandler, IHttpHandler, IRequiresSessionState
	{
		public SimpleSyncSessionHandler (HttpSimpleTypeStubInfo typeInfo,
						 HttpSimpleMethodStubInfo method, string protocolName)
						 : base (typeInfo, method, protocolName)
		{
		}

		public override bool IsReusable {
			get { return base.IsReusable; }
		}

		public override void ProcessRequest (HttpContext context)
		{
			base.ProcessRequest (context);
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
			case WSProtocol.HttpGet:
				string proto = protocol.ToString ();
				TypeStubInfo tinfo = TypeStubManager.GetTypeStub (type, proto);
				HttpSimpleTypeStubInfo typeInfo = (HttpSimpleTypeStubInfo) tinfo;

				string name = context.Request.PathInfo;
				if (name.StartsWith ("/"))
					name = name.Substring (1);

				HttpSimpleMethodStubInfo method = null;
				method = (HttpSimpleMethodStubInfo) typeInfo.GetMethod (name);
				if (method != null && method.MethodInfo.EnableSession)
					handler = new SimpleSyncSessionHandler (typeInfo, method, proto);
				else
					handler = new HttpSimpleWebServiceHandler (typeInfo, method, proto);

				break;
			case WSProtocol.Documentation:
				handler = new SoapDocumentationHandler (type, context);
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
