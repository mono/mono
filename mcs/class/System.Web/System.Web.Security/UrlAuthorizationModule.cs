//
// System.Web.Security.UrlAuthorizationModule
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.Web.Configuration;
using System.Security.Principal;

namespace System.Web.Security
{
	public sealed class UrlAuthorizationModule : IHttpModule
	{
		public UrlAuthorizationModule ()
		{
		}

		public void Dispose ()
		{
		}

		public void Init (HttpApplication app)
		{
			app.AuthorizeRequest += new EventHandler (OnAuthorizeRequest);
		}

		void OnAuthorizeRequest (object sender, EventArgs args)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context.SkipAuthorization)
				return;

			AuthorizationConfig config = (AuthorizationConfig) context.GetConfig ("system.web/authorization");
			if (config == null)
				return;

			if (!config.IsValidUser (context.User, context.Request.HttpMethod)) {
				HttpException e =  new HttpException (401, "Unauthorized");
				
				context.Response.StatusCode = 401;
				context.Response.Write (e.GetHtmlErrorMessage ());
				app.CompleteRequest ();
			}
		}
	}
}

