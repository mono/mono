//
// System.Web.Hosting.VirtualPathProviderTest
// 
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using NUnit.Framework;

namespace MonoTests.System.Web.Hosting {
	class DummyVPP : VirtualPathProvider {
		public override bool FileExists (string virtualPath)
		{
			bool de = base.FileExists (virtualPath);
			return de;
		}

		public override bool DirectoryExists (string virtualDir)
		{
			bool de = base.DirectoryExists (virtualDir);
			return de;
		}

		public override VirtualFile GetFile (string virtualPath)
		{
			VirtualFile vf = base.GetFile (virtualPath);
			return vf;
		}

		public override string GetFileHash (string virtualPath, IEnumerable dependencies)
		{
			return base.GetFileHash (virtualPath, dependencies);
		}

		public override VirtualDirectory GetDirectory (string virtualDir)
		{
			VirtualDirectory vd = base.GetDirectory (virtualDir);
			return vd;
		}

		public override CacheDependency GetCacheDependency (string virtualPath,
						IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			CacheDependency cd = base.GetCacheDependency (virtualPath, virtualPathDependencies, utcStart);
			return cd;
		}
	}

	[TestFixture]
	public class VirtualPathProviderTest {
		// Unhosted tests: not running inside an ASP.NET appdomain.
		// Some tests may yield different results when hosted. I'll add those later.
		[Test]
		public void FileExists1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsFalse (dummy.FileExists ("hola.aspx"));
		}

		[Test]
		public void DirectoryExists1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsFalse (dummy.DirectoryExists ("hola"));
		}

		[Test]
		public void GetFile1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsNull (dummy.GetFile ("index.aspx"));
		}

		[Test]
		public void GetFileHash1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsNull (dummy.GetFileHash ((string)null, null));
		}

		[Test]
		public void GetFileHash2 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsNull (dummy.GetFileHash ("something", null));
		}

		[Test]
		public void GetDirectory1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsNull (dummy.GetDirectory ("some_directory"));
		}

		[Test]
		public void GetCacheDependency1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsNull (dummy.GetCacheDependency ((string)null, null, DateTime.UtcNow));
		}

		[Test]
		public void GetCacheKey1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.IsNull (dummy.GetCacheKey ("index.aspx"));
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void OpenFile1 ()
		{
			VirtualPathProvider.OpenFile ("index.aspx");
		}

		[Test]
		public void CombineVirtualPaths1 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.AreEqual ("/otherroot", dummy.CombineVirtualPaths ("/root", "/otherroot"));
		}

		[Test]
		public void CombineVirtualPaths2 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.AreEqual ("/otherleaf", dummy.CombineVirtualPaths ("/root", "otherleaf"));
		}

		[Test]
		public void CombineVirtualPaths3 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.AreEqual ("/otherleaf/index.aspx", dummy.CombineVirtualPaths ("/root", "otherleaf/index.aspx"));
		}

		[Test]
        [Category ("NotWorking")]
		public void CombineVirtualPaths4 ()
		{
			DummyVPP dummy = new DummyVPP ();
			Assert.AreEqual ("/otherleaf/index.aspx", dummy.CombineVirtualPaths ("/root", "./otherleaf/index.aspx"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CombineVirtualPaths5 ()
		{
			DummyVPP dummy = new DummyVPP ();
			dummy.CombineVirtualPaths ("root", "./otherleaf/index.aspx");
		}
	}
	
}

