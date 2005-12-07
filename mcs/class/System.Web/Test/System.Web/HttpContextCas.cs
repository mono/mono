//
// HttpContextCas.cs - CAS unit tests for System.Web.HttpContext
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.Handlers;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpContextCas : AspNetHostingMinimal {

		private HttpContext context;
		private HttpRequest request;
		private HttpResponse response;
		private StringWriter sw;
		private IHttpHandler handler;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// running at fulltrust
			context = new HttpContext (null);
			request = new HttpRequest (String.Empty, "http://localhost/", String.Empty);
			sw = new StringWriter ();
			response = new HttpResponse (sw);
			handler = new TraceHandler ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructors_Deny_Unrestricted ()
		{
			Assert.IsNotNull (new HttpContext (null), "ctor(HttpWorkerRequest)");
			Assert.IsNotNull (new HttpContext (request, response), "ctor(HttpRequest,HttpResponse)");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			// AllErrors value depends on the execution order
			Exception[] exceptions = context.AllErrors;
			Assert.IsNull (context.ApplicationInstance, "ApplicationInstance");
			context.ApplicationInstance = new HttpApplication ();
			Assert.IsNotNull (context.Cache, "Cache");
			Assert.IsNull (context.Error, "Error");
			Assert.IsNull (context.Handler, "Handler");
			context.Handler = handler;
			// FIXME: Mono returns false, MS returns true
			bool b = context.IsCustomErrorEnabled;
			Assert.IsFalse (context.IsDebuggingEnabled, "IsDebuggingEnabled");
			Assert.IsNotNull (context.Items, "Items");
			Assert.IsNotNull (context.Request, "Request");
			Assert.IsNotNull (context.Response, "Response");
			Assert.IsNotNull (context.Server, "Server");
			Assert.IsNull (context.Session, "Session");
			// note: only SkipAuthorization setter is protected
			Assert.IsFalse (context.SkipAuthorization, "SkipAuthorization");
			Assert.IsTrue (context.Timestamp < DateTime.MaxValue, "Timestamp");
			Assert.IsNotNull (context.Trace, "Trace");
			// note: only User setter is protected
			Assert.IsNull (context.User, "User");
#if NET_2_0
			Assert.IsNotNull (context.Application, "Application");
			Assert.IsNotNull (context.CurrentHandler, "CurrentHandler");
			Assert.IsNull (context.PreviousHandler, "PreviousHandler");
//			Assert.IsNull (context.Profile, "Profile");
#endif
			// static properties
			Assert.IsNull (HttpContext.Current, "Current");
		}

#if ONLY_1_1
		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		[Ignore ("occurs only in certain conditions - removed in 2.0")]
		public void Application_Deny_UnmanagedCode ()
		{
			// The SecurityException doesn't always occurs (e.g. CAS unit tests
			// works for HttpContextCas alone but will fail if the whole suit
			// is executed). This is because the value is cached and may be 
			// created differently (without the check). This is _probably_ why
			// this check has been removed in 2.0.
			Assert.IsNotNull (context.Application, "Application");
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void Application_PermitOnly_UnmanagedCode ()
		{
			Assert.IsNotNull (context.Application, "Application");
		}
#endif

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SkipAuthorization_Set_Deny ()
		{
			context.SkipAuthorization = true;
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void SkipAuthorization_Set_PermitOnly ()
		{
			context.SkipAuthorization = true;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlPrincipal = true)]
		[ExpectedException (typeof (SecurityException))]
		public void User_Set_Deny ()
		{
			context.User = new GenericPrincipal (new GenericIdentity ("me"), null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, ControlPrincipal = true)]
		public void User_Set_PermitOnly ()
		{
			context.User = new GenericPrincipal (new GenericIdentity ("me"), null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			context.AddError (new Exception ());
			context.ClearError ();
			try {
				context.GetConfig (String.Empty);
			}
			catch (NullReferenceException) {
			}

			try {
				context.RewritePath (String.Empty);
			}
			catch (NullReferenceException) {
				// ms
			}
			catch (ArgumentNullException) {
				// mono
			}

			try {
				context.RewritePath (String.Empty, String.Empty, String.Empty);
			}
			catch (NullReferenceException) {
				// ms
			}
			catch (ArgumentNullException) {
				// mono
			}
#if NET_2_0
			context.GetSection (String.Empty);

			try {
				context.RewritePath (String.Empty, true);
			}
			catch (NullReferenceException) {
				// ms
			}
			catch (NotImplementedException) {
				// mono
			}
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StaticMethods_Deny_Unrestricted ()
		{
			HttpContext.GetAppConfig (String.Empty);
#if NET_2_0
			HttpContext.GetGlobalResourceObject (String.Empty, String.Empty);
			HttpContext.GetGlobalResourceObject (String.Empty, String.Empty, CultureInfo.InvariantCulture);
			try {
				HttpContext.GetLocalResourceObject ("/", String.Empty);
			}
			catch (NotImplementedException) {
				// mono
			}
			catch (NullReferenceException) {
			}
			try {
				HttpContext.GetLocalResourceObject ("/", String.Empty, CultureInfo.InvariantCulture);
			}
			catch (NotImplementedException) {
				// mono
			}
			catch (NullReferenceException) {
			}
#endif
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (HttpWorkerRequest) });
			Assert.IsNotNull (ci, ".ctor(HttpWorkerRequest)");
			return ci.Invoke (new object[1] { null });
		}

		public override Type Type {
			get { return typeof (HttpContext); }
		}
	}
}
