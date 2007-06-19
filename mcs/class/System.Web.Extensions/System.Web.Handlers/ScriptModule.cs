//
// ScriptModule.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace System.Web.Handlers
{
	public class ScriptModule : IHttpModule
	{
		protected virtual void Init (HttpApplication context) {
			context.PreSendRequestHeaders += new EventHandler (PreSendRequestHeaders);
			context.PostAcquireRequestState += new EventHandler (PostAcquireRequestState);
			context.AuthenticateRequest += new EventHandler (AuthenticateRequest);
		}

		void AuthenticateRequest (object sender, EventArgs e) {
		}

		void PostAcquireRequestState (object sender, EventArgs e) {
		}

		void PreSendRequestHeaders (object sender, EventArgs e) {
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context.Request.Headers ["X-MicrosoftAjax"] == "Delta=true" && context.Response.StatusCode == 302) {
				context.Response.StatusCode = 200;
				context.Response.ClearContent ();
				ScriptManager.WriteCallbackRedirect (context.Response.Output, context.Response.RedirectLocation);
			}
		}

		protected virtual void Dispose () {
		}

		#region IHttpModule Members

		void IHttpModule.Dispose () {
			Dispose ();
		}

		void IHttpModule.Init (HttpApplication context) {
			Init (context);
		}

		#endregion
	}
}

