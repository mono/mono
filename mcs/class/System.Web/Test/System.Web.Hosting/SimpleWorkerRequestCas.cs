//
// SimpleWorkerRequestCas.cs 
//	- CAS unit tests for System.Web.Hosting.SimpleWorkerRequest
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
using System.Web.Hosting;

namespace MonoCasTests.System.Web.Hosting {

	[TestFixture]
	[Category ("CAS")]
	public class SimpleWorkerRequestCas : AspNetHostingMinimal {

		private StringWriter sw;
		private string cwd;
		private SimpleWorkerRequest swr;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// we're at full trust here
			sw = new StringWriter ();
			cwd = Environment.CurrentDirectory;
			swr = new SimpleWorkerRequest ("/", cwd, String.Empty, String.Empty, sw);
		}


		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor3_Deny_UnmanagedCode ()
		{
			new SimpleWorkerRequest (null, null, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[Ignore ("I don't have a 'real' working case, inside NUnit, for this .ctor")]
		public void Constructor3_PermitOnly_UnmanagedCode ()
		{
			try {
				new SimpleWorkerRequest ("/", String.Empty, sw);
			}
			catch (NullReferenceException) {
				// we always seems to get a NRE from MS here (both 1.x and 2.0)
			}
			// note: on Mono a FileIOPermission is triggered later
			// in a call to HttpRuntime
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Constructor5_Deny_UnmanagedCode ()
		{
			new SimpleWorkerRequest (null, null, null, null, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void Constructor5_PermitOnly_UnmanagedCode ()
		{
			new SimpleWorkerRequest (null, cwd, "/", String.Empty, sw);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Properties_Deny_Unrestricted ()
		{
			Assert.IsNull (swr.MachineConfigPath, "MachineConfigPath");
			Assert.IsNull (swr.MachineInstallDirectory, "MachineInstallDirectory");
			Assert.AreEqual ("/", swr.GetAppPath (), "GetAppPath");
			Assert.AreEqual ("/", swr.GetFilePath (), "GetFilePath");
			Assert.AreEqual ("GET", swr.GetHttpVerbName (), "GetHttpVerbName");
			Assert.AreEqual ("HTTP/1.0", swr.GetHttpVersion (), "GetHttpVersion");
			Assert.AreEqual ("127.0.0.1", swr.GetLocalAddress (), "GetLocalAddress");
			Assert.AreEqual (80, swr.GetLocalPort (), "GetLocalPort");
			Assert.AreEqual (String.Empty, swr.GetPathInfo (), "GetPathInfo");
			Assert.AreEqual (String.Empty, swr.GetQueryString (), "GetQueryString");
			Assert.AreEqual ("/", swr.GetRawUrl (), "GetRawUrl");
			Assert.AreEqual ("127.0.0.1", swr.GetRemoteAddress (), "GetRemoteAddress");
			Assert.AreEqual (0, swr.GetRemotePort (), "GetRemotePort");
			Assert.AreEqual (String.Empty, swr.GetServerVariable ("mono"), "GetServerVariable");
			Assert.IsNotNull (swr.GetUriPath (), "GetUriPath");
			Assert.AreEqual (IntPtr.Zero, swr.GetUserToken (), "GetUserToken");
			Assert.IsNull (swr.MapPath ("/"), "MapPath");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Methods_Deny_Unrestricted ()
		{
			swr.EndOfRequest ();
			swr.FlushResponse (true);
			swr.SendKnownResponseHeader (0, String.Empty);
			swr.SendResponseFromFile (IntPtr.Zero, 0, 0);
			swr.SendResponseFromFile (String.Empty, 0, 0);
			swr.SendResponseFromMemory (new byte[0], 0);
			swr.SendStatus (0, "hello?");
			swr.SendUnknownResponseHeader ("mono", "monkey");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetAppPathTranslated_Deny_FileIOPermission ()
		{
			// path discovery
			swr.GetAppPathTranslated ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetAppPathTranslated_PermitOnly_FileIOPermission ()
		{
			Assert.IsNotNull (swr.GetAppPathTranslated (), "GetAppPathTranslated");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetFilePathTranslated_Deny_FileIOPermission ()
		{
			// path discovery
			swr.GetFilePathTranslated ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void GetFilePathTranslated_PermitOnly_FileIOPermission ()
		{
			Assert.IsNotNull (swr.GetFilePathTranslated (), "GetFilePathTranslated");
		}

		// LinkDemand

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			ConstructorInfo ci = this.Type.GetConstructor (new Type[5] { typeof (string), typeof (string), typeof (string), typeof (string), typeof (TextWriter) });
			Assert.IsNotNull (ci, ".ctor(string,string,TextWriter)");
			return ci.Invoke (new object[5] { null, cwd, "/", String.Empty, sw });
		}

		public override Type Type {
			get { return typeof (SimpleWorkerRequest); }
		}
	}
}
