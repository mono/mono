//
// AcceptEncodingModule.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
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

			if (config != null)
				config.SetFilter (response, request.Headers ["Accept-Encoding"]);
		}
	}
}

