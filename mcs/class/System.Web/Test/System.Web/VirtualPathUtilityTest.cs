//
// System.Web.VirtualPathUtilityTest.cs - Unit tests for System.Web.VirtualPathUtility
//
// Author:
//	Chris Toshok  <toshok@novell.com>
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

#if NET_2_0

using System;
using System.Web;

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class VirtualPathUtilityTest {
		[Test]
		public void AppendTrailingSlash ()
		{
			Assert.AreEqual ("/hithere/", VirtualPathUtility.AppendTrailingSlash ("/hithere"), "A1");
			Assert.AreEqual ("/hithere/", VirtualPathUtility.AppendTrailingSlash ("/hithere/"), "A2");
			Assert.AreEqual ("/", VirtualPathUtility.AppendTrailingSlash ("/"), "A3");
			Assert.AreEqual ("", VirtualPathUtility.AppendTrailingSlash (""), "A4");
			Assert.AreEqual (null, VirtualPathUtility.AppendTrailingSlash (null), "A5");
		}

		[Test]
		public void Combine ()
		{
			Assert.AreEqual ("/there", VirtualPathUtility.Combine ("/hi", "there"), "A1");
			Assert.AreEqual ("/hi/you", VirtualPathUtility.Combine ("/hi/there", "you"), "A2");
			Assert.AreEqual ("/hi/there/you", VirtualPathUtility.Combine ("/hi/there/", "you"), "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Combine_ArgException1 ()
		{
			Assert.AreEqual ("hi/there/you", VirtualPathUtility.Combine ("hi/there", "you"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Combine_ArgException2 ()
		{
			Assert.AreEqual ("hi/there", VirtualPathUtility.Combine ("hi/there", null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Combine_ArgException3 ()
		{
			Assert.AreEqual ("hi/there", VirtualPathUtility.Combine (null, "there"), "A1");
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
			Assert.AreEqual ("/you", VirtualPathUtility.Combine ("", "you"), "A1");
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
			Assert.AreEqual ("/hi", VirtualPathUtility.Combine ("/hi", ""), "A1");
		}

		[Test]
		public void GetDirectory ()
		{
			Assert.AreEqual ("/hi/", VirtualPathUtility.GetDirectory ("/hi/there"), "A1");
			Assert.AreEqual ("/hi/", VirtualPathUtility.GetDirectory ("/hi/there/"), "A2");
			Assert.AreEqual (null, VirtualPathUtility.GetDirectory ("/"), "A3");
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
			Assert.AreEqual ("", VirtualPathUtility.GetDirectory (""), "A1");
		}

		[Test]
		public void GetExtension ()
		{
			Assert.AreEqual (".aspx", VirtualPathUtility.GetExtension ("/hi/index.aspx"), "A1");
			Assert.AreEqual (".aspx", VirtualPathUtility.GetExtension ("index.aspx"), "A2");
			Assert.AreEqual ("", VirtualPathUtility.GetExtension ("/hi/index"), "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetExtension_ArgException1 ()
		{
			Assert.AreEqual (null, VirtualPathUtility.GetExtension (null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetExtension_ArgException2 ()
		{
			Assert.AreEqual ("", VirtualPathUtility.GetExtension (""), "A1");
		}

		[Test]
		public void GetFileName ()
		{
			Assert.AreEqual ("index.aspx", VirtualPathUtility.GetFileName ("/hi/index.aspx"), "A1");
			Assert.AreEqual ("hi", VirtualPathUtility.GetFileName ("/hi/"), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFileName_ArgException1 ()
		{
			Assert.AreEqual (null, VirtualPathUtility.GetFileName (null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFileName_ArgException2 ()
		{
			Assert.AreEqual ("", VirtualPathUtility.GetFileName (""), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFileName_ArgException3 ()
		{
			Assert.AreEqual ("index.aspx", VirtualPathUtility.GetFileName ("index.aspx"), "A1");
		}

		[Test]
		public void IsAbsolute ()
		{
			Assert.IsTrue (VirtualPathUtility.IsAbsolute ("/"), "A1");
			Assert.IsTrue (VirtualPathUtility.IsAbsolute ("/hi/there"), "A2");
			Assert.IsFalse (VirtualPathUtility.IsAbsolute ("hi/there"), "A3");
			Assert.IsFalse (VirtualPathUtility.IsAbsolute ("./hi"), "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAbsolute_ArgException1 ()
		{
			Assert.IsFalse (VirtualPathUtility.IsAbsolute (""), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAbsolute_ArgException2 ()
		{
			Assert.IsFalse (VirtualPathUtility.IsAbsolute (null), "A1");
		}

		[Test]
		public void IsAppRelative ()
		{
			Assert.IsTrue (VirtualPathUtility.IsAppRelative ("~/Stuff"), "A1");
			Assert.IsFalse (VirtualPathUtility.IsAppRelative ("./Stuff"), "A2");
			Assert.IsFalse (VirtualPathUtility.IsAppRelative ("/Stuff"), "A3");
			Assert.IsFalse (VirtualPathUtility.IsAppRelative ("/"), "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAppRelative_ArgException1 ()
		{
			Assert.IsFalse (VirtualPathUtility.IsAppRelative (""), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IsAppRelative_ArgException2 ()
		{
			Assert.IsFalse (VirtualPathUtility.IsAppRelative (null), "A1");
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
			Assert.AreEqual ("../bar", VirtualPathUtility.MakeRelative ("~/foo/hi", "~/foo/bar"), "A1");
		}
#endif

		[Test]
		public void RemoveTrailingSlash ()
		{
			Assert.AreEqual ("/hi/there", VirtualPathUtility.RemoveTrailingSlash ("/hi/there/"), "A1");
			Assert.AreEqual ("/hi/there", VirtualPathUtility.RemoveTrailingSlash ("/hi/there"), "A2");
			Assert.AreEqual ("/", VirtualPathUtility.RemoveTrailingSlash ("/"), "A3");
			Assert.AreEqual (null, VirtualPathUtility.RemoveTrailingSlash (""), "A4");
			Assert.AreEqual (null, VirtualPathUtility.RemoveTrailingSlash (null), "A5");
		}
	}
}

#endif
