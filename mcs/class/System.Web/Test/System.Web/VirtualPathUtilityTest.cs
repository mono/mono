//
// System.Web.VirtualPathUtilityTest.cs - Unit tests for System.Web.VirtualPathUtility
//
// Author:
//	Chris Toshok  <toshok@novell.com>
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2005,2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Web;
using VPU = System.Web.VirtualPathUtility;

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class VirtualPathUtilityTest {
		[Test]
		public void AppendTrailingSlash ()
		{
			Assert.AreEqual ("/hithere/", VPU.AppendTrailingSlash ("/hithere"), "A1");
			Assert.AreEqual ("/hithere/", VPU.AppendTrailingSlash ("/hithere/"), "A2");
			Assert.AreEqual ("/", VPU.AppendTrailingSlash ("/"), "A3");
			Assert.AreEqual ("", VPU.AppendTrailingSlash (""), "A4");
			Assert.AreEqual (null, VPU.AppendTrailingSlash (null), "A5");
		}

		[Test]
		public void Combine ()
		{
			Assert.AreEqual ("/there", VPU.Combine ("/hi", "there"), "A1");
			Assert.AreEqual ("/hi/you", VPU.Combine ("/hi/there", "you"), "A2");
			Assert.AreEqual ("/hi/there/you", VPU.Combine ("/hi/there/", "you"), "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Combine_ArgException1 ()
		{
			Assert.AreEqual ("hi/there/you", VPU.Combine ("hi/there", "you"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Combine_ArgException2 ()
		{
			Assert.AreEqual ("hi/there", VPU.Combine ("hi/there", null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Combine_ArgException3 ()
		{
			Assert.AreEqual ("hi/there", VPU.Combine (null, "there"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		/* stack trace is:
		   at System.Web.VirtualPath.Create(String virtualPath, VirtualPathOptions options)
		   at System.Web.VirtualPathUtility.Combine(String basePath, String relativePath)
		   at MonoTests.System.Web.VirtualPathUtilityTest.Combine()
		*/
		public void Combine_ArgException4 ()
		{
			Assert.AreEqual ("/you", VPU.Combine ("", "you"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		/* stack trace is:
		   at System.Web.VirtualPath.Create(String virtualPath, VirtualPathOptions options)
		   at System.Web.VirtualPathUtility.Combine(String basePath, String relativePath)
		   at MonoTests.System.Web.VirtualPathUtilityTest.Combine()
		*/
		public void Combine_ArgException5 ()
		{
			Assert.AreEqual ("/hi", VPU.Combine ("/hi", ""), "A1");
		}

		[Test]
		public void GetDirectory ()
		{
			Assert.AreEqual ("/hi/", VPU.GetDirectory ("/hi/there"), "A1");
			Assert.AreEqual ("/hi/", VPU.GetDirectory ("/hi/there/"), "A2");
			Assert.AreEqual (null, VPU.GetDirectory ("/"), "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		/* stack trace is:
		   at System.Web.VirtualPath.Create(String virtualPath, VirtualPathOptions options)
		   at System.Web.VirtualPathUtility.GetDirectory(String virtualPath)
		   at MonoTests.System.Web.VirtualPathUtilityTest.GetDirectory()
		 */
		public void GetDirectory_ArgException1 ()
		{
			Assert.AreEqual ("", VPU.GetDirectory (""), "A1");
		}

		[Test]
		public void GetExtension ()
		{
			Assert.AreEqual (".aspx", VPU.GetExtension ("/hi/index.aspx"), "A1");
			Assert.AreEqual (".aspx", VPU.GetExtension ("index.aspx"), "A2");
			Assert.AreEqual ("", VPU.GetExtension ("/hi/index"), "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetExtension_ArgException1 ()
		{
			Assert.AreEqual (null, VPU.GetExtension (null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetExtension_ArgException2 ()
		{
			Assert.AreEqual ("", VPU.GetExtension (""), "A1");
		}

		[Test]
		public void GetFileName ()
		{
			Assert.AreEqual ("index.aspx", VPU.GetFileName ("/hi/index.aspx"), "A1");
			Assert.AreEqual ("hi", VPU.GetFileName ("/hi/"), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFileName_ArgException1 ()
		{
			Assert.AreEqual (null, VPU.GetFileName (null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFileName_ArgException2 ()
		{
			Assert.AreEqual ("", VPU.GetFileName (""), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFileName_ArgException3 ()
		{
			Assert.AreEqual ("index.aspx", VPU.GetFileName ("index.aspx"), "A1");
		}

		[Test]
		public void IsAbsolute ()
		{
			Assert.IsTrue (VPU.IsAbsolute ("/"), "A1");
			Assert.IsTrue (VPU.IsAbsolute ("/hi/there"), "A2");
			Assert.IsFalse (VPU.IsAbsolute ("hi/there"), "A3");
			Assert.IsFalse (VPU.IsAbsolute ("./hi"), "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAbsolute_ArgException1 ()
		{
			Assert.IsFalse (VPU.IsAbsolute (""), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAbsolute_ArgException2 ()
		{
			Assert.IsFalse (VPU.IsAbsolute (null), "A1");
		}

		[Test]
		public void IsAppRelative ()
		{
			Assert.IsTrue (VPU.IsAppRelative ("~/Stuff"), "A1");
			Assert.IsFalse (VPU.IsAppRelative ("./Stuff"), "A2");
			Assert.IsFalse (VPU.IsAppRelative ("/Stuff"), "A3");
			Assert.IsFalse (VPU.IsAppRelative ("/"), "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAppRelative_ArgException1 ()
		{
			Assert.IsFalse (VPU.IsAppRelative (""), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAppRelative_ArgException2 ()
		{
			Assert.IsFalse (VPU.IsAppRelative (null), "A1");
		}

#if false
		[Test]
		/* this test when run on MS generates the following stack trace (NRE):
		   at System.Web.Util.UrlPath.MakeVirtualPathAppAbsolute(String virtualPath, String applicationPath)
		   at System.Web.Util.UrlPath.MakeRelative(String from, String to)
		   at System.Web.VirtualPathUtility.MakeRelative(String fromPath, String toPath)
		   at MonoTests.System.Web.VirtualPathUtilityTest.MakeRelative()
		*/
		public void MakeRelative ()
		{
			Assert.AreEqual ("../bar", VPU.MakeRelative ("~/foo/hi", "~/foo/bar"), "A1");
		}
#endif

		[Test]
		public void RemoveTrailingSlash ()
		{
			Assert.AreEqual ("/hi/there", VPU.RemoveTrailingSlash ("/hi/there/"), "A1");
			Assert.AreEqual ("/hi/there", VPU.RemoveTrailingSlash ("/hi/there"), "A2");
			Assert.AreEqual ("/", VPU.RemoveTrailingSlash ("/"), "A3");
			Assert.AreEqual (null, VPU.RemoveTrailingSlash (""), "A4");
			Assert.AreEqual (null, VPU.RemoveTrailingSlash (null), "A5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Combine1 ()
		{
			VPU.Combine (null, "something");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Combine2 ()
		{
			VPU.Combine ("something", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetDirectory1 ()
		{
			VPU.GetDirectory (null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetDirectory2 ()
		{
			VPU.GetDirectory ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetDirectory3 ()
		{
			VPU.GetDirectory ("hola");
		}

		[Test]
		public void GetDirectory4 ()
		{
			Assert.AreEqual ("/direc/", VPU.GetDirectory ("/direc/somefilenoextension"));
			Assert.AreEqual ("/direc/", VPU.GetDirectory ("/direc/somefile.aspx"));
			Assert.AreEqual ("/direc/", VPU.GetDirectory ("/////direc///somefile.aspx"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetExtension1 ()
		{
			VPU.GetExtension (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetExtension2 ()
		{
			// Amazing.
			VPU.GetExtension ("");
		}

		[Test]
		public void GetExtension3 ()
		{
			Assert.AreEqual ("", VPU.GetExtension ("/direc/somefilenoextension"));
			Assert.AreEqual ("", VPU.GetExtension ("/"));
			Assert.AreEqual (".aspx", VPU.GetDirectory ("/////direc///somefile.aspx"));
		}

		[Test]
		public void GetFileName1 ()
		{
			Assert.AreEqual ("", VPU.GetFileName ("/"));
			Assert.AreEqual ("hola", VPU.GetFileName ("/hola"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFileName2 ()
		{
			VPU.GetFileName (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFileName3 ()
		{
			VPU.GetFileName ("");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void MakeRelative1 ()
		{
			VPU.MakeRelative (null, "");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void MakeRelative2 ()
		{
			VPU.MakeRelative ("", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void MakeRelative3 ()
		{
			VPU.MakeRelative ("/", "i");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void MakeRelative4 ()
		{
			VPU.MakeRelative ("aa", "/i");
		}

		[Test]
		public void MakeRelative5 ()
		{
			Assert.AreEqual ("", VPU.MakeRelative ("", ""));
			Assert.AreEqual ("", VPU.MakeRelative ("/something", ""));
			Assert.AreEqual ("./", VPU.MakeRelative ("/", "/"));
		}

		[Test]
		public void RemoveTrailingSlash2 ()
		{
			Assert.AreEqual (null, VPU.RemoveTrailingSlash (null));
			Assert.AreEqual (null, VPU.RemoveTrailingSlash (""));
			Assert.AreEqual ("/", VPU.RemoveTrailingSlash ("/"));
			Assert.AreEqual ("////", VPU.RemoveTrailingSlash ("/////"));
			Assert.AreEqual ("/pepe", VPU.RemoveTrailingSlash ("/pepe"));
			Assert.AreEqual ("/pepe", VPU.RemoveTrailingSlash ("/pepe/"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAbsolute1 ()
		{
			VPU.ToAbsolute (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAbsolute2 ()
		{
			VPU.ToAbsolute ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute3 ()
		{
			VPU.ToAbsolute ("..");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute4 ()
		{
			VPU.ToAbsolute ("...");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute5 ()
		{
			VPU.ToAbsolute ("../blah");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ToAbsolute6 ()
		{
			VPU.ToAbsolute ("~/");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ToAbsolute7 ()
		{
			Assert.AreEqual ("/", VPU.ToAbsolute ("/"));
		}
	}
}

#endif

