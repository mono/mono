//
// AcceptEncodingModule.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace Mono.Http
{
	public class AcceptEncodingModule : IHttpModule
	{
		static readonly string configSection = "mono.aspnet/acceptEncoding";
		AcceptEncodingConfig config;

		public void Init (HttpApplication app)
		{
			app.BeginRequest += new EventHandler (CheckIfAddFilter);
		}

		public void Dispose ()
		{
		}

		void CheckIfAddFilter (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpRequest request = app.Request;
			HttpResponse response = app.Response;

			//FIXME: fix this when config is cached
			if (config == null)
				//config = (AcceptEncodingConfig) app.Context.GetConfig (configSection);
				config = (AcceptEncodingConfig) ConfigurationSettings.GetConfig (configSection);

			config.SetFilter (response, request.Headers ["Accept-Encoding"]);
		}
	}
}

