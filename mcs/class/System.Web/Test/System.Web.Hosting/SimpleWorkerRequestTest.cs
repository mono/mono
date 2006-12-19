//
// Tests for System.Web.Hosting.SimpleWorkerRequest.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

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
using System.Globalization;
using System.Web;
using System.Web.Hosting;

namespace MonoTests.System.Web.Hosting {

	[TestFixture]
	public class SimpleWorkerRequestTest {

		string cwd, bindir;
		string assembly;
		
		[SetUp] public void startup ()
		{
			cwd = Environment.CurrentDirectory;
			bindir = Path.Combine (cwd, "bin");

			Uri u = new Uri (typeof (SimpleWorkerRequestTest).Assembly.CodeBase);
			assembly = u.LocalPath;

			if (!Directory.Exists (bindir))
				Directory.CreateDirectory (bindir);
			
			File.Copy (assembly, Path.Combine (bindir, Path.GetFileName (assembly)), true);
		}
			
		//
		// This tests the constructor when the user code creates an HttpContext
		//
		[Test] public void ConstructorTests ()
		{
			StringWriter sw = new StringWriter ();
			
			SimpleWorkerRequest swr;

			swr = new SimpleWorkerRequest ("/appVirtualDir", cwd, "pageVirtualPath", "querystring", sw);
			Assert.AreEqual ("/appVirtualDir", swr.GetAppPath (), "S1");
			Assert.AreEqual ("/appVirtualDir/pageVirtualPath", swr.GetFilePath (), "S2");
			Assert.AreEqual ("GET", swr.GetHttpVerbName (), "S3");
			Assert.AreEqual ("HTTP/1.0", swr.GetHttpVersion (), "S4");
			Assert.AreEqual ("127.0.0.1", swr.GetLocalAddress (), "S5");
			Assert.AreEqual (80, swr.GetLocalPort (), "S6");
			Assert.AreEqual ("querystring", swr.GetQueryString (), "S7");
			Assert.AreEqual ("127.0.0.1", swr.GetRemoteAddress (), "S8");
			Assert.AreEqual (0, swr.GetRemotePort (), "S9");
			Assert.AreEqual ("/appVirtualDir/pageVirtualPath?querystring", swr.GetRawUrl (), "S10");
			Assert.AreEqual ("/appVirtualDir/pageVirtualPath", swr.GetUriPath (), "S11");
			Assert.AreEqual ("0", swr.GetUserToken ().ToString (), "S12");
			Assert.AreEqual (null, swr.MapPath ("x"), "S13");
			Assert.AreEqual (null, swr.MachineConfigPath, "S14");
			Assert.AreEqual (null, swr.MachineInstallDirectory, "S15");
			Assert.AreEqual (Path.Combine (cwd, "pageVirtualPath"), swr.GetFilePathTranslated (), "S16");
			Assert.AreEqual ("", swr.GetServerVariable ("AUTH_TYPE"), "S18");
			Assert.AreEqual ("", swr.GetServerVariable ("AUTH_USER"), "S19");
			Assert.AreEqual ("", swr.GetServerVariable ("REMOTE_USER"), "S20");
			Assert.AreEqual ("", swr.GetServerVariable ("SERVER_SOFTWARE"), "S21");
			Assert.AreEqual ("/appVirtualDir/pageVirtualPath", swr.GetUriPath (), "S22");

			//
			// MapPath
			//
			Assert.AreEqual (null, swr.MapPath ("file.aspx"), "MP1");
			Assert.AreEqual (null, swr.MapPath ("/appVirtualDir/pageVirtualPath"), "MP2");
			Assert.AreEqual (null, swr.MapPath ("appVirtualDir/pageVirtualPath"), "MP3");
			Assert.AreEqual (null, swr.MapPath ("/appVirtualDir/pageVirtualPath/page.aspx"), "MP4");
			Assert.AreEqual (null, swr.MapPath ("/appVirtualDir/pageVirtualPath/Subdir"), "MP5");

			swr = new SimpleWorkerRequest ("/appDir", cwd, "/Something/page.aspx", "querystring", sw);

			//Assert.AreEqual ("c:\\tmp\\page.aspx", swr.GetFilePathTranslated (), "S17");

			//
			// GetUriPath tests, veredict: MS implementation is a bit fragile on this interface
			//
			swr = new SimpleWorkerRequest ("/appDir", cwd, "/page.aspx", null, sw);
			Assert.AreEqual ("/appDir//page.aspx", swr.GetUriPath (), "S23");

			swr = new SimpleWorkerRequest ("/appDir/", cwd, "/page.aspx", null, sw);
			Assert.AreEqual ("/appDir///page.aspx", swr.GetUriPath (), "S24");

			swr = new SimpleWorkerRequest ("/appDir/", cwd, "page.aspx", null, sw);
			Assert.AreEqual ("/appDir//page.aspx", swr.GetUriPath (), "S25");

			swr = new SimpleWorkerRequest ("/appDir", cwd, "page.aspx", null, sw);
			Assert.AreEqual ("/appDir/page.aspx", swr.GetUriPath (), "S26");
			
		}

		[Test]
		public void GetPathInfo ()
		{
			StringWriter sw = new StringWriter ();
			SimpleWorkerRequest swr = new SimpleWorkerRequest ("appVirtualDir", cwd, "/pageVirtualPath", "querystring", sw);
			Assert.AreEqual ("/pageVirtualPath", swr.GetPathInfo (), "GetPathInfo-1");

			swr = new SimpleWorkerRequest ("appVirtualDir", cwd, "/", "querystring", sw);
			Assert.AreEqual ("/", swr.GetPathInfo (), "GetPathInfo-2");

			swr = new SimpleWorkerRequest ("appVirtualDir", cwd, "pageVirtualPath", "querystring", sw);
			Assert.AreEqual (String.Empty, swr.GetPathInfo (), "GetPathInfo-3");
		}

		[Test]
		public void GetUriPath ()
		{
			StringWriter sw = new StringWriter ();
			SimpleWorkerRequest swr = new SimpleWorkerRequest ("/", cwd, String.Empty, String.Empty, sw);
			Assert.AreEqual ("/", swr.GetUriPath (), "GetUriPath");

			swr = new SimpleWorkerRequest (String.Empty, cwd, String.Empty, String.Empty, sw);
			Assert.AreEqual ("/", swr.GetUriPath (), "GetUriPath-2");

			swr = new SimpleWorkerRequest (String.Empty, cwd, "/", String.Empty, sw);
			Assert.AreEqual ("//", swr.GetUriPath (), "GetUriPath-3");

			swr = new SimpleWorkerRequest ("/", cwd, "/", String.Empty, sw);
			Assert.AreEqual ("//", swr.GetUriPath (), "GetUriPath-4");

			swr = new SimpleWorkerRequest ("virtual", cwd, "/", String.Empty, sw);
			Assert.AreEqual ("virtual//", swr.GetUriPath (), "GetUriPath-5");

			swr = new SimpleWorkerRequest ("/virtual", cwd, "/", String.Empty, sw);
			Assert.AreEqual ("/virtual//", swr.GetUriPath (), "GetUriPath-6");

			swr = new SimpleWorkerRequest ("/virtual/", cwd, "/", String.Empty, sw);
			Assert.AreEqual ("/virtual///", swr.GetUriPath (), "GetUriPath-7");

			swr = new SimpleWorkerRequest ("virtual", cwd, "page", String.Empty, sw);
			Assert.AreEqual ("virtual/page", swr.GetUriPath (), "GetUriPath-8");

			swr = new SimpleWorkerRequest ("/virtual", cwd, "page", String.Empty, sw);
			Assert.AreEqual ("/virtual/page", swr.GetUriPath (), "GetUriPath-9");

			swr = new SimpleWorkerRequest ("/virtual/", cwd, "page", String.Empty, sw);
			Assert.AreEqual ("/virtual//page", swr.GetUriPath (), "GetUriPath-a");
		}

		public class Host : MarshalByRefObject {
			string cwd = Environment.CurrentDirectory;

			
			public void Demo ()
			{
				StringWriter sw = new StringWriter ();
				SimpleWorkerRequest swr = new SimpleWorkerRequest("file.aspx", "querystring", sw);

				Assert.AreEqual ("/appVirtualDir", swr.GetAppPath (), "T1");
				Assert.AreEqual (cwd + Path.DirectorySeparatorChar, swr.GetAppPathTranslated (), "TRANS1");
				Assert.AreEqual ("/appVirtualDir/file.aspx", swr.GetFilePath (), "T2");
				Assert.AreEqual ("GET", swr.GetHttpVerbName (), "T3");
				Assert.AreEqual ("HTTP/1.0", swr.GetHttpVersion (), "T4");
				Assert.AreEqual ("127.0.0.1", swr.GetLocalAddress (), "T5");
				Assert.AreEqual (80, swr.GetLocalPort (), "T6");
				Assert.AreEqual ("querystring", swr.GetQueryString (), "T7");
				Assert.AreEqual ("127.0.0.1", swr.GetRemoteAddress (), "T8");
				Assert.AreEqual (0, swr.GetRemotePort (), "T9");
				Assert.AreEqual ("/appVirtualDir/file.aspx?querystring", swr.GetRawUrl (), "T10");
				Assert.AreEqual ("/appVirtualDir/file.aspx", swr.GetUriPath (), "T11");
				Assert.AreEqual ("0", swr.GetUserToken ().ToString (), "T12");
				Assert.AreEqual ("", swr.GetPathInfo (), "TRANS2");

				//
				// On windows:
				// \windows\microsoft.net\framework\v1.1.4322\Config\machine.config
				//
				Assert.AreEqual (true, swr.MachineConfigPath != null, "T14");
				//
				// On windows:
				// \windows\microsoft.net\framework\v1.1.4322
				//
				Assert.AreEqual (true, swr.MachineInstallDirectory != null, "T15");

				// Disabled T16. It throws a nullref on MS
				//      Assert.AreEqual (Path.Combine (cwd, "file.aspx"), swr.GetFilePathTranslated (), "T16");
				//
				Assert.AreEqual ("", swr.GetServerVariable ("AUTH_TYPE"), "T18");
				Assert.AreEqual ("", swr.GetServerVariable ("AUTH_USER"), "T19");
				Assert.AreEqual ("", swr.GetServerVariable ("REMOTE_USER"), "T20");
				Assert.AreEqual ("", swr.GetServerVariable ("TERVER_SOFTWARE"), "T21");
				Assert.AreEqual ("/appVirtualDir/file.aspx", swr.GetUriPath (), "T22");

				//
				// MapPath
				//
				Assert.AreEqual (Path.Combine (cwd, "file.aspx"), swr.MapPath ("/appVirtualDir/file.aspx"), "TP2");
				Assert.AreEqual (Path.Combine (cwd, "file.aspx"), swr.MapPath ("/appVirtualDir/file.aspx"), "TP4");
				Assert.AreEqual (Path.Combine (cwd, Path.Combine ("Subdir", "file.aspx")), swr.MapPath ("/appVirtualDir/Subdir/file.aspx"), "TP5");

			}

			public void Exception1 ()
			{
				StringWriter sw = new StringWriter ();
				SimpleWorkerRequest swr = new SimpleWorkerRequest("file.aspx", "querystring", sw);

				swr.MapPath ("x");
			}

			public void Exception2 ()
			{
				StringWriter sw = new StringWriter ();
				SimpleWorkerRequest swr = new SimpleWorkerRequest("file.aspx", "querystring", sw);

				swr.MapPath ("file.aspx");
			}

			public void Exception3 ()
			{
				StringWriter sw = new StringWriter ();
				SimpleWorkerRequest swr = new SimpleWorkerRequest("file.aspx", "querystring", sw);

				swr.MapPath ("appVirtualDir/file.aspx");
			}

			public void MapPathOnEmptyVirtualDir ()
			{
				StringWriter sw = new StringWriter ();
				SimpleWorkerRequest swr = new SimpleWorkerRequest("file.aspx", "querystring", sw);

				swr.MapPath ("/");
			}
		}

		Host MakeHost (string virtualDir)
		{
			return (Host) ApplicationHost.CreateApplicationHost (typeof (Host), virtualDir, cwd);
		}

		Host MakeHost ()
		{
			return MakeHost ("/appVirtualDir");
		}
		
		[Test] public void AppDomainTests ()
		{
			Host h = MakeHost ();

			h.Demo ();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AppDomain_MapPath1 ()
		{
			Host h = MakeHost ();
			
			h.Exception1 ();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AppDomain_MapPath2 ()
		{
			Host h = MakeHost ();
			
			h.Exception2 ();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AppDomain_MapPath3 ()
		{
			Host h = MakeHost ();
			
			h.Exception3 ();
		}

		[Test]
		public void AppDomain_MapPath4 ()
		{
			Host h = MakeHost ("/");
			
			h.MapPathOnEmptyVirtualDir ();
		}
		
		//
		// This tests the constructor when the target application domain is created with
		// CreateApplicationHost
		//
		[Test]
		public void ConstructorTest_CreateApplicationHost ()
		{
			// Does not work without a NRE, need to call CreateApplicationHost.
			// = new SimpleWorkerRequest ("pageVirtualPath", "querystring", sw);
			// Assert.AreEqual ("querystring", swr.GetQueryString ());
		}

		[Test]
		public void PathInfos ()
		{
			SimpleWorkerRequest wr = new SimpleWorkerRequest ("/appDir", "", "page.aspx/pathinfo", "", null);
			HttpContext c = new HttpContext (wr);
			Assert.AreEqual ("http://127.0.0.1/appDir/page.aspx/pathinfo", c.Request.Url.AbsoluteUri);
			Assert.AreEqual ("/appDir/page.aspx", c.Request.FilePath);
			Assert.AreEqual ("/appDir/page.aspx/pathinfo", c.Request.Path);
			Assert.AreEqual ("/pathinfo", c.Request.PathInfo);
		}
	}
}

