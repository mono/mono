//
// HttpServerUtilityCas.cs - CAS unit tests for System.Web.HttpServerUtility
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace MonoCasTests.System.Web {

	[TestFixture]
	[Category ("CAS")]
	public class HttpServerUtilityCas : AspNetHostingMinimal {

		private const string url = "http://www.mono-project.com/";

		private StringWriter sw;
		private HttpContext context;
		private HttpServerUtility hsu;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			sw = new StringWriter ();
			context = new HttpContext (null);
			hsu = context.Server;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			try {
				Assert.IsTrue (hsu.ScriptTimeout > 0, "ScriptTimeout");
			}
			catch (NullReferenceException) {
				// ms 1.x, mono
			}
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void ScriptTimeout_Deny_Unrestricted ()
		{
			hsu.ScriptTimeout = 1;
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void ScriptTimeout_PermitOnly_Unrestricted ()
		{
			hsu.ScriptTimeout = 1;
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			hsu.ClearError ();

			Assert.IsNull (hsu.GetLastError (), "GetLastError");

			Assert.IsNotNull (hsu.HtmlDecode (String.Empty), "HtmlDecode(string)");
			hsu.HtmlDecode (String.Empty, sw);

			Assert.IsNotNull (hsu.HtmlEncode (String.Empty), "HtmlEncode(string)");
			hsu.HtmlEncode (String.Empty, sw);

			try {
				Assert.IsNull (hsu.MapPath (String.Empty), "MapPath(string)");
			}
			catch (NullReferenceException) {
				// ms 1.x
			}

			try {
				hsu.Transfer ("/");
			}
			catch (NullReferenceException) {
				// ms
			}
			try {
				hsu.Transfer ("/", true);
			}
			catch (NullReferenceException) {
				// ms
			}
			try {
				hsu.Transfer ("/", false);
			}
			catch (NullReferenceException) {
				// ms
			}
#if NET_2_0
			try {
				hsu.Transfer ((IHttpHandler)null, true);
			}
			catch (NullReferenceException) {
				// ms
			}
			try {
				hsu.Transfer ((IHttpHandler)null, false);
			}
			catch (NullReferenceException) {
				// ms
			}
#endif
			try {
				Assert.IsNotNull (hsu.UrlDecode (url), "UrlDecode(string)");
			}
			catch (NullReferenceException) {
				// ms
			}
			try {
				hsu.UrlDecode ("http://www.mono-project.com/", sw);
			}
			catch (NullReferenceException) {
				// ms
			}

			Assert.IsNotNull (hsu.UrlEncode (String.Empty), "UrlEncode(string)");
			hsu.UrlEncode (String.Empty, sw);

			Assert.IsNotNull (hsu.UrlPathEncode (String.Empty), "UrlPathEncode(string)");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateObject_String_Deny_UnmanagedCode ()
		{
			hsu.CreateObject (String.Empty);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[ExpectedException (typeof (HttpException))] // String.Empty isn't valid
		public void CreateObject_String_PermitOnly_UnmanagedCode ()
		{
			hsu.CreateObject (String.Empty);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateObject_Type_Deny_UnmanagedCode ()
		{
			hsu.CreateObject (String.Empty);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void CreateObject_Type_PermitOnly_UnmanagedCode ()
		{
			try {
				hsu.CreateObject (typeof (string));
			}
			catch (MissingMethodException) {
				// ms
			}
			catch (HttpException) {
				// mono
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateObjectFromClsid_String_Deny_UnmanagedCode ()
		{
			hsu.CreateObjectFromClsid (String.Empty);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void CreateObjectFromClsid_PermitOnly_UnmanagedCode ()
		{
			try {
				hsu.CreateObjectFromClsid (String.Empty);
			}
			catch (FormatException) {
				// ms (not a valid guid)
			}
			catch (HttpException) {
				// mono
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void Execute_String_Deny_Unrestricted ()
		{
			hsu.Execute (String.Empty);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void Execute_StringTextWriter_Deny_Unrestricted ()
		{
			hsu.Execute (String.Empty, sw);
		}

#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void Execute_StringTextWriterTrue_Deny_Unrestricted ()
		{
			hsu.Execute (String.Empty, sw, true);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (NullReferenceException))]
		public void Execute_StringTextWriterFalse_Deny_Unrestricted ()
		{
			hsu.Execute (String.Empty, sw, false);
		}
#endif

		[Test]
		[AspNetHostingPermission (SecurityAction.Deny, Level = AspNetHostingPermissionLevel.Medium)]
		[ExpectedException (typeof (SecurityException))]
		public void MachineName_Deny_Medium ()
		{
			Assert.IsNotNull (hsu.MachineName, "MachineName");
		}

		[Test]
		[AspNetHostingPermission (SecurityAction.PermitOnly, Level = AspNetHostingPermissionLevel.Medium)]
		public void MachineName_PermitOnly_Medium ()
		{
			Assert.IsNotNull (hsu.MachineName, "MachineName");
		}

		// LinkDemand

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// there are no public ctor so we're taking a method that we know isn't protected
			// (by a Demand) and call it thru reflection so any linkdemand (on the class) will
			// be promoted to a Demand
			MethodInfo mi = this.Type.GetMethod ("HtmlDecode", new Type[1] { typeof (string) } );
			return mi.Invoke (hsu, new object[1] { String.Empty });
		}

		public override Type Type {
			get { return typeof (HttpServerUtility); }
		}
	}
}
