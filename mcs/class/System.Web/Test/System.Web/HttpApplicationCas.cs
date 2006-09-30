//
// HttpApplicationCas.cs - CAS unit tests for System.Web.HttpApplication
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpApplicationCas : AspNetHostingMinimal {

		private void Handler (object sender, EventArgs e)
		{
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			// FIXME
			if (app.Application == null) {
				// ms 1.x/2.0
				Assert.IsNull (app.Application, "Application");
			} else {
				// mono
				Assert.IsNotNull (app.Application, "Application");
			}
			Assert.IsNull (app.Context, "Context");
			Assert.IsNotNull (app.Server, "Server");
			Assert.IsNull (app.Site, "Site");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Events_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			app.Disposed += new EventHandler (Handler);
			app.Error += new EventHandler (Handler);
			app.PreSendRequestContent += new EventHandler (Handler);
			app.PreSendRequestHeaders += new EventHandler (Handler);
			app.AcquireRequestState += new EventHandler (Handler);
			app.AuthenticateRequest += new EventHandler (Handler);
			app.AuthorizeRequest += new EventHandler (Handler);
			app.BeginRequest += new EventHandler (Handler);
			app.EndRequest += new EventHandler (Handler);
			app.PostRequestHandlerExecute += new EventHandler (Handler);
			app.PreRequestHandlerExecute += new EventHandler (Handler);
			app.ReleaseRequestState += new EventHandler (Handler);
			app.ResolveRequestCache += new EventHandler (Handler);
			app.UpdateRequestCache += new EventHandler (Handler);

			app.AddOnAcquireRequestStateAsync (null, null);
			app.AddOnAuthenticateRequestAsync (null, null);
			app.AddOnAuthorizeRequestAsync (null, null);
			app.AddOnBeginRequestAsync (null, null);
			app.AddOnEndRequestAsync (null, null);
			app.AddOnPostRequestHandlerExecuteAsync (null, null);
			app.AddOnPreRequestHandlerExecuteAsync (null, null);
			app.AddOnReleaseRequestStateAsync (null, null);
			app.AddOnResolveRequestCacheAsync (null, null);
			app.AddOnUpdateRequestCacheAsync (null, null);

			app.Disposed -= new EventHandler (Handler);
			app.Error -= new EventHandler (Handler);
			app.PreSendRequestContent -= new EventHandler (Handler);
			app.PreSendRequestHeaders -= new EventHandler (Handler);
			app.AcquireRequestState -= new EventHandler (Handler);
			app.AuthenticateRequest -= new EventHandler (Handler);
			app.AuthorizeRequest -= new EventHandler (Handler);
			app.BeginRequest -= new EventHandler (Handler);
			app.EndRequest -= new EventHandler (Handler);
			app.PostRequestHandlerExecute -= new EventHandler (Handler);
			app.PreRequestHandlerExecute -= new EventHandler (Handler);
			app.ReleaseRequestState -= new EventHandler (Handler);
			app.ResolveRequestCache -= new EventHandler (Handler);
			app.UpdateRequestCache -= new EventHandler (Handler);
#if NET_2_0
			app.PostAuthenticateRequest += new EventHandler (Handler);
			app.PostAuthorizeRequest += new EventHandler (Handler);
			app.PostResolveRequestCache += new EventHandler (Handler);
			app.PostMapRequestHandler += new EventHandler (Handler);
			app.PostAcquireRequestState += new EventHandler (Handler);
			app.PostReleaseRequestState += new EventHandler (Handler);
			app.PostUpdateRequestCache += new EventHandler (Handler);

			app.AddOnPostAuthenticateRequestAsync (null, null);
			app.AddOnPostAuthenticateRequestAsync (null, null, null);
			app.AddOnPostAuthorizeRequestAsync (null, null);
			app.AddOnPostAuthorizeRequestAsync (null, null, null);
			app.AddOnPostResolveRequestCacheAsync (null, null);
			app.AddOnPostResolveRequestCacheAsync (null, null, null);
			app.AddOnPostMapRequestHandlerAsync (null, null);
			app.AddOnPostMapRequestHandlerAsync (null, null, null);
			app.AddOnPostAcquireRequestStateAsync (null, null);
			app.AddOnPostAcquireRequestStateAsync (null, null, null);
			app.AddOnPostReleaseRequestStateAsync (null, null);
			app.AddOnPostReleaseRequestStateAsync (null, null, null);
			app.AddOnPostUpdateRequestCacheAsync (null, null);
			app.AddOnPostUpdateRequestCacheAsync (null, null, null);

			app.AddOnAcquireRequestStateAsync (null, null, null);
			app.AddOnAuthenticateRequestAsync (null, null, null);
			app.AddOnAuthorizeRequestAsync (null, null, null);
			app.AddOnBeginRequestAsync (null, null, null);
			app.AddOnEndRequestAsync (null, null, null);
			app.AddOnPostRequestHandlerExecuteAsync (null, null, null);
			app.AddOnPreRequestHandlerExecuteAsync (null, null, null);
			app.AddOnReleaseRequestStateAsync (null, null, null);
			app.AddOnResolveRequestCacheAsync (null, null, null);
			app.AddOnUpdateRequestCacheAsync (null, null, null);

			app.PostAuthenticateRequest -= new EventHandler (Handler);
			app.PostAuthorizeRequest -= new EventHandler (Handler);
			app.PostResolveRequestCache -= new EventHandler (Handler);
			app.PostMapRequestHandler -= new EventHandler (Handler);
			app.PostAcquireRequestState -= new EventHandler (Handler);
			app.PostReleaseRequestState -= new EventHandler (Handler);
			app.PostUpdateRequestCache -= new EventHandler (Handler);
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			app.CompleteRequest ();
			app.GetVaryByCustomString (null, String.Empty);
			app.Dispose ();
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.High)]
		[ExpectedException (typeof (SecurityException))]
		public void Modules_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			Assert.IsNotNull (app.Modules, "Modules");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.High)]
		public void Modules_PermitOnly_High ()
		{
			HttpApplication app = new HttpApplication ();
			Assert.IsNotNull (app.Modules, "Modules");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Request_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			try {
				Assert.IsNotNull (app.Request, "Request");
			}
			catch (HttpException) {
				// mono, ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.x
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Response_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			try {
				Assert.IsNotNull (app.Response, "Response");
			}
			catch (HttpException) {
				// mono, ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.x
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Session_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			try {
				Assert.IsNotNull (app.Session, "Session");
			}
			catch (HttpException) {
				// mono, ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.x
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void User_Deny_Unrestricted ()
		{
			HttpApplication app = new HttpApplication ();
			try {
				Assert.IsNull (app.User);
			}
			catch (HttpException) {
				// mono, ms 2.0
			}
			catch (TypeInitializationException) {
				// ms 1.x
			}
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (HttpApplication); }
		}
	}
}
