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
using Mono.Http.Configuration;

namespace Mono.Http.Modules
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

			if (config == null)
				config = (AcceptEncodingConfig) app.Context.GetConfig (configSection);

			config.SetFilter (response, request.Headers ["Accept-Encoding"]);
		}
	}
}

