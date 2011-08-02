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
using System.Web.Script.Services;

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
			// The AuthenticateRequest event is raised after the identity of the current user has been 
			// established. The handler for this event sets the SkipAuthorization property of the HttpContext 
			// for the current request. This property is checked in the authorization module to see 
			// if it has to omit authorization checking for the requested url. Usually an HttpModule 
			// use this property to allow anonymous access to some resources (for example, 
			// the Login Page if we’re using forms authentication). In our scenario, 
			// the ScriptModule sets the SkipAuthorization to true if the requested url is 
			// scriptresource.axd or if the authorization module is enabled and the request is a rest 
			// request to the authorization web service.
		}

		void PostAcquireRequestState (object sender, EventArgs e) {
			// The PostAcquireRequestState event is raised after the session data has been obtained. 
			// If the request is for a class that implements System.Web.UI.Page and it is a rest 
			// method call, the WebServiceData class (that was explained in a previous post) is used 
			// to call the requested method from the Page. After the method has been called, 
			// the CompleteRequest method is called, bypassing all pipeline events and executing 
			// the EndRequest method. This allows MS AJAX to be able to call a method on a page 
			// instead of having to create a web service to call a method.
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context == null)
				return;
			
			HttpRequest request = context.Request;
			string contentType = request.ContentType;
			IHttpHandler currentHandler = context.CurrentHandler;
			if (currentHandler == null)
				return;
#if TARGET_J2EE
			if (!(currentHandler is Page) && currentHandler is IServiceProvider) {
				pageType = (Type) ((IServiceProvider) currentHandler).GetService (typeof (Type));
				if (pageType == null)
					return;
			}
#endif
			Type pageType = currentHandler.GetType ();
			if (typeof (Page).IsAssignableFrom (pageType) && !String.IsNullOrEmpty (contentType) && contentType.StartsWith ("application/json", StringComparison.OrdinalIgnoreCase)) {
				IHttpHandler h = RestHandler.GetHandler (context, pageType, request.FilePath);
				h.ProcessRequest (context);
				app.CompleteRequest ();
			}
		}

		void PreSendRequestHeaders (object sender, EventArgs e)
		{
			HttpApplication app = (HttpApplication) sender;
			HttpContext context = app.Context;
			if (context.Request.Headers ["X-MicrosoftAjax"] == "Delta=true") {
				Page p = context.CurrentHandler as Page;
#if TARGET_J2EE
				if (p == null && context.CurrentHandler is IServiceProvider)
					p = (Page) ((IServiceProvider) context.CurrentHandler).GetService (typeof (Page));
#endif
				ScriptManager sm = ScriptManager.GetCurrentInternal (p);
				if (context.Response.StatusCode == 302) {
					context.Response.StatusCode = 200;
					context.Response.ClearContent ();
					if (context.Error == null || (sm != null && sm.AllowCustomErrorsRedirect))
						ScriptManager.WriteCallbackRedirect (context.Response.Output, context.Response.RedirectLocation);
					else
						ScriptManager.WriteCallbackException (sm, context.Response.Output, context.Error, false);
				} else if (context.Error != null) {
					context.Response.StatusCode = 200;
					context.Response.ClearContent ();
					ScriptManager.WriteCallbackException (sm, context.Response.Output, context.Error, true);
				}
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

