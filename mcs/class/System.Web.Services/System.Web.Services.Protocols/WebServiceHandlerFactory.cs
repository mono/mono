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

using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.UI;

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

			if (true /*WSConfig.IsSupported (protocol)*/) {
				switch (protocol) {
				case WSProtocol.HttpSoap:
					handler = new HttpSoapWebServiceHandler (type);
					break;
				case WSProtocol.HttpPost:
					//handler = new ();
					break;
				case WSProtocol.HttpGet:
					//handler = new ();
					break;
				case WSProtocol.Documentation:
					//handler = new ();
					break;
				}
				
			} else {
				handler = new DummyHttpHandler ();
			}

			return handler;
		}

		static WSProtocol GuessProtocol (HttpContext context, string verb)
		{
			return WSProtocol.HttpSoap;
		}

		public void ReleaseHandler (IHttpHandler handler)
		{
		}

		#endregion // Methods
	}
}
