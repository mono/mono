//
// Abstract Authentication implementation
//
// Authors:
//	Greg Reinacker (gregr@rassoc.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2002-2003 Greg Reinacker, Reinacker & Associates, Inc. All rights reserved.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Based on "DigestAuthenticationModule.cs". Original source code available at
// http://www.rassoc.com/gregr/weblog/stories/2002/07/09/webServicesSecurityHttpDigestAuthenticationWithoutActiveDirectory.html
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
using System.Web;

namespace Mono.Http.Modules
{
	abstract public class AuthenticationModule : IHttpModule
	{
		string authMethod;

		public AuthenticationModule (string authenticationMethod) 
		{
			authMethod = authenticationMethod;
		}

		public string AuthenticationMethod { 
			get { return authMethod; }
		}

		#region IHttpModule Members

		public virtual void Init (HttpApplication context) 
		{
			context.AuthenticateRequest += new EventHandler (this.OnAuthenticateRequest);
			context.EndRequest += new EventHandler (this.OnEndRequest);
		}

		public virtual void Dispose () {}

		#endregion

		#region Event Handlers

		public virtual void OnAuthenticateRequest (object source, EventArgs eventArgs) 
		{
			if (!AuthenticationRequired)
				return;

			HttpApplication app = (HttpApplication) source;
			string authdata = Authorization (app, AuthenticationMethod);
			if ((authdata == null) || (!AcceptCredentials (app, authdata))) {
				DenyAccess (app);
				return;
			}
		}

		abstract public void OnEndRequest (object source, EventArgs eventArgs);

		#endregion

		abstract protected bool AcceptCredentials (HttpApplication app, string authentication);

		protected bool AuthenticationRequired {
			get { return (AuthenticationMethod == ConfigurationSettings.AppSettings ["Authentication"]); }
		}

		protected void DenyAccess (HttpApplication app) 
		{
			app.Response.StatusCode = 401;
			app.Response.StatusDescription = "Access Denied";
			// Write to response stream as well, to give user visual 
			// indication of error during development
			app.Response.Write ("401 Access Denied");
			app.CompleteRequest ();
		}

		protected string Authorization (HttpApplication app, string authenticationMethod) 
		{
			string autz = app.Request.Headers ["Authorization"];
			if ((autz == null) || (autz.Length == 0)) {
				// No credentials; anonymous request
				return null;
			}
			
			if (autz.ToUpper ().StartsWith (authenticationMethod.ToUpper ())) {
				return autz.Substring (authenticationMethod.Length + 1);
			}

			return null;
		}
	}
}

