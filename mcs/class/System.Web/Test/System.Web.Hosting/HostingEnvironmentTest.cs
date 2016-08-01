//
// System.Web.Hosting.HostingEnvironmentTest 
// 
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Web.Hosting;
using NUnit.Framework;
using System.Web;
using System.Web.UI;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.Hosting {
	public class MyRegisteredObject : IRegisteredObject {
		public void Stop(bool immediate) {}
	}

	[TestFixture]
	public class HostingEnvironmentTest {
		[Test]
		public void StaticDefaultValues ()
		{
			StaticDefaultValues (string.Empty);
		}

		private void StaticDefaultValues (string errorPrefix)
		{
			Assert.IsNull (HostingEnvironment.InitializationException, errorPrefix + "InitializationException");
			Assert.IsFalse (HostingEnvironment.IsHosted, errorPrefix + "IsHosted");
			Assert.IsNull (HostingEnvironment.ApplicationID, errorPrefix + "ApplicationID");
			Assert.IsNull (HostingEnvironment.ApplicationPhysicalPath, errorPrefix + "ApplicationPhysicalPath");
			Assert.IsNull (HostingEnvironment.ApplicationVirtualPath, errorPrefix + "ApplicationVirtualPath");
			Assert.IsNull (HostingEnvironment.SiteName, errorPrefix + "SiteName");
			Assert.IsNotNull (HostingEnvironment.Cache, errorPrefix + "Cache");
			Assert.AreEqual (ApplicationShutdownReason.None, HostingEnvironment.ShutdownReason, errorPrefix + "None");
			Assert.IsNull (HostingEnvironment.VirtualPathProvider, errorPrefix + "VirtualPathProvider");
		}

		[Test]
		[Category ("NunitWeb")]
		public void HostedDefaultValues () 
		{
			StaticDefaultValues ("Before:");

			WebTest t = new WebTest (PageInvoker.CreateOnLoad (HostedDefaultValues_OnLoad));
			t.Run ();
			Assert.AreEqual (global::System.Net.HttpStatusCode.OK, t.Response.StatusCode, "HttpStatusCode");

			StaticDefaultValues ("After:");
		}

		public static void HostedDefaultValues_OnLoad(Page p) 
		{
			Assert.IsNull (HostingEnvironment.InitializationException, "During:InitializationException");
			Assert.IsTrue (HostingEnvironment.IsHosted, "During:IsHosted");
			Assert.IsNotNull (HostingEnvironment.ApplicationID, "During:ApplicationID:Null");
			AssertHelper.IsNotEmpty (HostingEnvironment.ApplicationID, "During:ApplicationID:Empty");
			Assert.IsNotNull (HostingEnvironment.ApplicationPhysicalPath, "During:ApplicationPhysicalPath:Null");
			AssertHelper.IsNotEmpty (HostingEnvironment.ApplicationPhysicalPath, "During:ApplicationPhysicalPath:Empty");
			Assert.IsNotNull (HostingEnvironment.ApplicationVirtualPath, "During:ApplicationVirtualPath:Null");
			AssertHelper.IsNotEmpty (HostingEnvironment.ApplicationVirtualPath, "During:ApplicationVirtualPath:Empty");
			Assert.IsNotNull (HostingEnvironment.SiteName, "During:SiteName:Null");
			AssertHelper.IsNotEmpty (HostingEnvironment.SiteName, "During:SiteName:Empty");
			Assert.IsNotNull (HostingEnvironment.Cache, "During:Cache");
			Assert.AreEqual (ApplicationShutdownReason.None, HostingEnvironment.ShutdownReason, "During:ShutdownReason");
			Assert.IsNotNull (HostingEnvironment.VirtualPathProvider, "During:VirtualPathProvider");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MapPath1 ()
		{
			HostingEnvironment.MapPath (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MapPath2 ()
		{
			HostingEnvironment.MapPath ("");
		}

		[Test]
		public void MapPath3 ()
		{
			Assert.IsNull (HostingEnvironment.MapPath ("hola"));
		}

		[Test]
		public void RegisterAndUnregisterObject ()
		{
			var registered = new MyRegisteredObject ();

			HostingEnvironment.RegisterObject (registered);
			HostingEnvironment.UnregisterObject (registered);
		}
	}
}

