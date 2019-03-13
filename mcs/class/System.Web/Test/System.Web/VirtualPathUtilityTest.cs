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


using System;
using System.Web;
using VPU = System.Web.VirtualPathUtility;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Web.UI;

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class VirtualPathUtilityTest {

		const string NunitWebAppName = "NunitWeb";

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
			
			Assert.AreEqual ("/there/", VPU.Combine ("/hi", "there/"), "A1");
			Assert.AreEqual ("/hi/you/", VPU.Combine ("/hi/there", "you/"), "A2");
			Assert.AreEqual ("/hi/there/you/", VPU.Combine ("/hi/there/", "you/"), "A3");

			Assert.AreEqual ("/there", VPU.Combine ("/hi", "/there"), "A1");
			Assert.AreEqual ("/you", VPU.Combine ("/hi/there", "/you"), "A2");
			Assert.AreEqual ("/you", VPU.Combine ("/hi/there/", "/you"), "A3");
		}

		[Test]
		public void Combine3 ()
		{
			Assert.AreEqual ("/", VPU.Combine ("/hi/", ".."), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi/there", ".."), "A2");
			Assert.AreEqual ("/hi", VPU.Combine ("/hi/there/", ".."), "A3");

			Assert.AreEqual ("/", VPU.Combine ("/hi/", "../"), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi/there", "../"), "A2");
			Assert.AreEqual ("/hi/", VPU.Combine ("/hi/there/", "../"), "A3");
			
			Assert.AreEqual ("/", VPU.Combine ("/", "."), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi", "."), "A2");
			Assert.AreEqual ("/hi", VPU.Combine ("/hi/", "."), "A3");

			Assert.AreEqual ("/", VPU.Combine ("/", "./"), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi", "./"), "A2");
			Assert.AreEqual ("/hi/", VPU.Combine ("/hi/", "./"), "A3");

			Assert.AreEqual ("/", VPU.Combine ("/hi", "there/../"), "A1");
			Assert.AreEqual ("/hi", VPU.Combine ("/hi/there", "you/.."), "A2");

			Assert.AreEqual ("/there/", VPU.Combine ("/hi", "there/./"), "A1");
			Assert.AreEqual ("/hi/you", VPU.Combine ("/hi/there", "you/."), "A2");
			
			Assert.AreEqual ("/blah2/", VPU.Combine ("/ROOT", "/blah1/../blah2/"));
			Assert.AreEqual ("/blah1/blah2/", VPU.Combine ("/ROOT", "/blah1/./blah2/"));

			Assert.AreEqual ("/blah1", VPU.Combine ("/ROOT", "/blah1/blah2/.."));
			Assert.AreEqual ("/", VPU.Combine ("/ROOT", "/blah1/.."));
			Assert.AreEqual ("/blah1/", VPU.Combine ("/ROOT", "/blah1/blah2/../"));
			Assert.AreEqual ("/", VPU.Combine ("/ROOT", "/blah1/../"));

			Assert.AreEqual ("/blah1", VPU.Combine ("/ROOT", "/blah1/."));
			Assert.AreEqual ("/", VPU.Combine ("/ROOT", "/."));
			Assert.AreEqual ("/blah1/", VPU.Combine ("/ROOT", "/blah1/./"));
			Assert.AreEqual ("/", VPU.Combine ("/ROOT", "/./"));

			Assert.AreEqual ("/", VPU.Combine ("///hi/", ".."), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi/there/me/..", ".."), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi/there/../", ".."), "A1");
			Assert.AreEqual ("/hi/me", VPU.Combine ("/hi/there/../", "me"), "A1");
			Assert.AreEqual ("/", VPU.Combine ("/hi/there/../you", ".."), "A1");
			Assert.AreEqual ("/hi/me", VPU.Combine ("/hi/there/../you", "me"), "A1");
			Assert.AreEqual ("/hi/you/me", VPU.Combine ("/hi/there/../you/", "me"), "A1");
		}

		[Test]
		public void Combine4 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (Combine4_Load)).Run ();
		}

		public static void Combine4_Load (Page p)
		{
			Assert.AreEqual ("~", VPU.Combine ("/ROOT", "~"), "/ROOT, ~");
			Assert.AreEqual ("~/blah1", VPU.Combine ("/ROOT", "~/blah1"), "/ROOT, ~/blah1");
			Assert.AreEqual ("~/blah1/", VPU.Combine ("/ROOT", "~/blah1/"));

			Assert.AreEqual ("~/blah2/", VPU.Combine ("/ROOT", "~/blah1/../blah2/"));
			Assert.AreEqual ("~/blah1/blah2/", VPU.Combine ("/ROOT", "~/blah1/./blah2/"));

			Assert.AreEqual ("~/blah1", VPU.Combine ("/ROOT", "~/blah1/blah2/.."));
			Assert.AreEqual ("~", VPU.Combine ("/ROOT", "~/blah1/.."));
			Assert.AreEqual ("~/blah1/", VPU.Combine ("/ROOT", "~/blah1/blah2/../"));
			Assert.AreEqual ("~/", VPU.Combine ("/ROOT", "~/blah1/../"));

			Assert.AreEqual ("~/blah1", VPU.Combine ("/ROOT", "~/blah1/."));
			Assert.AreEqual ("~", VPU.Combine ("/ROOT", "~/."));
			Assert.AreEqual ("~/blah1/", VPU.Combine ("/ROOT", "~/blah1/./"));
			Assert.AreEqual ("~/", VPU.Combine ("/ROOT", "~/./"));

			Assert.AreEqual ("/", VPU.Combine ("~/ROOT", "~/.."), "~/ROOT, ~/..");
			Assert.AreEqual ("/", VPU.Combine ("~/ROOT", ".."));
			Assert.AreEqual ("~", VPU.Combine ("~/ROOT/", ".."));
			Assert.AreEqual ("~/", VPU.Combine ("~/ROOT/", "../"));
			Assert.AreEqual ("~/folder", VPU.Combine ("~/ROOT", "folder"));
			Assert.AreEqual ("~/ROOT/folder", VPU.Combine ("~/ROOT/", "folder"));
			Assert.AreEqual ("~/ROOT/folder/", VPU.Combine ("~/ROOT/", "folder/"));

			Assert.AreEqual ("/", VPU.Combine ("~", ".."));
			Assert.AreEqual ("~/me", VPU.Combine ("~", "me"));
			Assert.AreEqual ("/me", VPU.Combine ("~", "../me"));
			Assert.AreEqual ("~/me", VPU.Combine ("~", "./me"));
			
			Assert.AreEqual ("/me", VPU.Combine ("~/..", "me"));

			Assert.AreEqual ("/", VPU.Combine ("~/hi/there/..", ".."), "A1");
			Assert.AreEqual ("~", VPU.Combine ("~/hi/there/../", ".."), "A1");
			Assert.AreEqual ("/", VPU.Combine ("~/hi/there/../", "../.."), "A1");
			Assert.AreEqual ("~/hi/me", VPU.Combine ("~/hi/there/../", "me"), "A1");
			Assert.AreEqual ("~", VPU.Combine ("~/hi/there/../you", ".."), "A1");
			Assert.AreEqual ("~/hi/me", VPU.Combine ("~/hi/there/../you", "me"), "A1");
			Assert.AreEqual ("~/hi/you/me", VPU.Combine ("~/hi/there/../you/", "me"), "A1");
			
			Assert.AreEqual (HttpRuntime.AppDomainAppVirtualPath, VPU.Combine ("/ROOT", HttpRuntime.AppDomainAppVirtualPath));
			Assert.AreEqual (HttpRuntime.AppDomainAppVirtualPath, VPU.Combine ("~/ROOT", HttpRuntime.AppDomainAppVirtualPath));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		// The relative virtual path 'hi/there' is not allowed here.
		public void Combine_ArgException1 ()
		{
			Assert.AreEqual ("hi/there/you", VPU.Combine ("hi/there", "you"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		// The relative virtual path 'hi/there' is not allowed here.
		public void Combine_ArgException2 ()
		{
			Assert.AreEqual ("hi/there", VPU.Combine ("hi/there", null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Combine_ArgException2_1 ()
		{
			Assert.AreEqual ("hi/there", VPU.Combine ("/hi/there", null), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		// The relative virtual path 'hi/there' is not allowed here.
		public void Combine_ArgException2_2 ()
		{
			Assert.AreEqual ("hi/there", VPU.Combine ("hi/there", "/dir"), "A1");
		}
		
		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Combine_ArgException2_3 ()
		{
			Assert.AreEqual ("hi/there", VPU.Combine ("/../hi", null), "A1");
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
		[ExpectedException (typeof (HttpException))]
		public void Combine_ArgException6 ()
		{
			VPU.Combine ("/ROOT", "..");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Combine_ArgException7 ()
		{
			VPU.Combine ("/ROOT", "/..");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Combine_ArgException8 () {
			VPU.Combine ("/ROOT", "./..");
		}

		[Test]
		public void GetExtension ()
		{
			Assert.AreEqual (".aspx", VPU.GetExtension ("/hi/index.aspx"), "A1");
			Assert.AreEqual ("", VPU.GetExtension ("/hi/index.aspx/"), "A1");
			Assert.AreEqual (".aspx", VPU.GetExtension ("index.aspx"), "A2");
			Assert.AreEqual ("", VPU.GetExtension ("/hi/index"), "A3");

			Assert.AreEqual (".aspx", VPU.GetExtension ("/hi/./index.aspx"), "A1");
			Assert.AreEqual ("", VPU.GetExtension ("hi/index"), "A2");

			Assert.AreEqual ("", VPU.GetExtension ("/hi/index.aspx/file"), "A1");
			Assert.AreEqual ("", VPU.GetExtension ("/hi/index.aspx\\file"), "A1");
			Assert.AreEqual ("", VPU.GetExtension ("/hi/index.aspx/../file"), "A1");
			Assert.AreEqual (".htm", VPU.GetExtension ("/hi/index.aspx/../file.htm"), "A1");

			Assert.AreEqual ("", VPU.GetExtension (".."), "A2");
			Assert.AreEqual ("", VPU.GetExtension ("../.."), "A2");
			Assert.AreEqual (".aspx", VPU.GetExtension ("../../file.aspx"), "A2");
		}

		[Test]
		public void GetFileName ()
		{
			Assert.AreEqual ("index.aspx", VPU.GetFileName ("/hi/index.aspx"), "A1");
			Assert.AreEqual ("index.aspx", VPU.GetFileName ("/hi/index.aspx/"), "A1");
			Assert.AreEqual ("hi", VPU.GetFileName ("/hi/"), "A2");
		}

		[Test]
		public void IsAbsolute ()
		{
			Assert.IsTrue (VPU.IsAbsolute ("/"), "A1");
			Assert.IsTrue (VPU.IsAbsolute ("/hi/there"), "A2");
			Assert.IsFalse (VPU.IsAbsolute ("hi/there"), "A3");
			Assert.IsFalse (VPU.IsAbsolute ("./hi"), "A4");

			Assert.IsTrue (VPU.IsAbsolute ("\\"), "A1");
			Assert.IsTrue (VPU.IsAbsolute ("\\hi\\there"), "A2");
			Assert.IsFalse (VPU.IsAbsolute ("hi\\there"), "A3");
			Assert.IsFalse (VPU.IsAbsolute (".\\hi"), "A4");
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
			Assert.IsTrue (VPU.IsAppRelative ("~"), "A0");
			Assert.IsTrue (VPU.IsAppRelative ("~/Stuff"), "A1");
			Assert.IsFalse (VPU.IsAppRelative ("./Stuff"), "A2");
			Assert.IsFalse (VPU.IsAppRelative ("/Stuff"), "A3");
			Assert.IsFalse (VPU.IsAppRelative ("/"), "A4");
			Assert.IsFalse (VPU.IsAppRelative ("~Stuff"), "A5");
			
			Assert.IsTrue (VPU.IsAppRelative ("~"), "A0");
			Assert.IsTrue (VPU.IsAppRelative ("~\\Stuff"), "A1");
			Assert.IsFalse (VPU.IsAppRelative (".\\Stuff"), "A2");
			Assert.IsFalse (VPU.IsAppRelative ("\\Stuff"), "A3");
			Assert.IsFalse (VPU.IsAppRelative ("\\"), "A4");
			Assert.IsFalse (VPU.IsAppRelative ("~Stuff"), "A5");
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
		[ExpectedException (typeof (ArgumentException))]
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetDirectory2 ()
		{
			VPU.GetDirectory ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
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
			Assert.AreEqual ("/direc/", VPU.GetDirectory ("\\\\direc\\///somefile.aspx"));
			Assert.AreEqual ("/direc/", VPU.GetDirectory ("/direc/somefilenoextension/"));
			Assert.AreEqual (null, VPU.GetDirectory ("/"), "A3");

			Assert.AreEqual (null, VPU.GetDirectory ("/dir1/.."), "/dir1/..");
			Assert.AreEqual (null, VPU.GetDirectory ("/dir1/../"), "/dir1/../");
			Assert.AreEqual ("/", VPU.GetDirectory ("/dir1/../dir2"), "/dir1/../dir2");
			Assert.AreEqual ("/", VPU.GetDirectory ("/dir1/../dir2/"), "/dir1/../dir2/");
			Assert.AreEqual ("/dir2/", VPU.GetDirectory ("/dir1/../dir2/somefile.aspx"));
			
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/direc/somefilenoextension"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/direc/somefile.aspx"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/////direc///somefile.aspx"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~\\\\direc\\///somefile.aspx"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/direc/somefilenoextension/"));
		}

		[Test]
		public void GetDirectory5 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (GetDirectory5_Load)).Run ();
		}

		public static void GetDirectory5_Load (Page p)
		{
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/direc/somefilenoextension"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/direc/somefile.aspx"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/////direc///somefile.aspx"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~\\\\direc\\///somefile.aspx"));
			Assert.AreEqual ("~/direc/", VPU.GetDirectory ("~/direc/somefilenoextension/"));
			Assert.AreEqual ("/", VPU.GetDirectory ("~/"), "~/");
			Assert.AreEqual ("/", VPU.GetDirectory ("~"), "~");

			Assert.AreEqual ("/", VPU.GetDirectory ("~/dir1/.."), "/dir1/..");
			Assert.AreEqual ("/", VPU.GetDirectory ("~/dir1/../"), "/dir1/../");
			Assert.AreEqual ("~/", VPU.GetDirectory ("~/dir1/../dir2"), "/dir1/../dir2");
			Assert.AreEqual ("~/", VPU.GetDirectory ("~/dir1/../dir2/"), "/dir1/../dir2/");
			Assert.AreEqual ("~/dir2/", VPU.GetDirectory ("~/dir1/../dir2/somefile.aspx"));

			Assert.AreEqual ("/dir1/", VPU.GetDirectory ("~/../dir1/dir2"), "~/../dir1");
			Assert.AreEqual ("/dir1/", VPU.GetDirectory ("~/../dir1/dir2/"), "~/../dir1/");
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
			Assert.AreEqual ("/direc/", VPU.GetDirectory ("/////direc///somefile.aspx"));
		}

		[Test]
		public void GetFileName1 ()
		{
			Assert.AreEqual ("", VPU.GetFileName ("/"));
			Assert.AreEqual ("hola", VPU.GetFileName ("/hola"));
			Assert.AreEqual ("hola", VPU.GetFileName ("/hola/"));
			Assert.AreEqual ("hi", VPU.GetFileName ("/hi/there/.."));
			Assert.AreEqual ("there", VPU.GetFileName ("/hi/there/."));
		}

		[Test]
		public void GetFileName4 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (GetFileName4_Load)).Run ();
		}

		public static void GetFileName4_Load (Page p)
		{
			Assert.AreEqual (NunitWebAppName, VPU.GetFileName ("~/"));
			Assert.AreEqual ("hola", VPU.GetFileName ("~/hola"));
			Assert.AreEqual ("hola", VPU.GetFileName ("~/hola/"));
			Assert.AreEqual ("hi", VPU.GetFileName ("~/hi/there/.."));
			Assert.AreEqual ("there", VPU.GetFileName ("~/hi/there/."));

			Assert.AreEqual (NunitWebAppName, VPU.GetFileName ("~"));
			Assert.AreEqual ("", VPU.GetFileName ("~/.."));
			Assert.AreEqual ("", VPU.GetFileName ("~/../"));
			Assert.AreEqual ("hi", VPU.GetFileName ("~/../hi"));
			Assert.AreEqual ("hi", VPU.GetFileName ("~/../hi/"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFileName5 ()
		{
			VPU.GetFileName ("hi");
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

		// LAMESPEC: MSDN: If the fromPath and toPath parameters are not rooted; that is, 
		// they do not equal the root operator (the tilde [~]), do not start with a tilde (~), 
		// such as a tilde and a slash mark (~/) or a tilde and a double backslash (~//), 
		// or do not start with a slash mark (/), an ArgumentException exception is thrown.
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
			Assert.AreEqual ("./", VPU.MakeRelative ("/something", ""));
		}

        [Test]
        public void MakeRelative6()
        {
			Assert.AreEqual ("./", VPU.MakeRelative ("/", "/"));
			Assert.AreEqual ("directory1", VPU.MakeRelative ("/directory1", "/directory1"));
			Assert.AreEqual ("directory2", VPU.MakeRelative ("/directory1", "/directory2"));
			Assert.AreEqual ("directory1", VPU.MakeRelative ("/", "/directory1"));
			Assert.AreEqual ("./", VPU.MakeRelative ("/directory1", "/"));
			Assert.AreEqual ("./", VPU.MakeRelative ("/directory1/", "/directory1/"));
			Assert.AreEqual ("directory1/file1.aspx", VPU.MakeRelative ("/directory1", "/directory1/file1.aspx"));
			Assert.AreEqual ("file1.aspx", VPU.MakeRelative ("/directory1/file1.aspx", "/directory1/file1.aspx"));
			Assert.AreEqual ("file1.aspx", VPU.MakeRelative ("/directory1/", "/directory1/file1.aspx"));
			Assert.AreEqual ("../directory2/file2.aspx", VPU.MakeRelative ("/directory1/file1.aspx", "/directory2/file2.aspx"));
		}

		[Test]
		public void MakeRelative6_a ()
		{
			Assert.AreEqual ("directory1", VPU.MakeRelative ("/directory1/../", "/directory1"));
			Assert.AreEqual ("./", VPU.MakeRelative ("/directory1", "/directory1/../"));
			Assert.AreEqual ("./", VPU.MakeRelative ("/", "/directory1/../"));
			Assert.AreEqual ("directory1", VPU.MakeRelative ("/directory1", "/directory2/../directory1"));
		}

		[Test]
		[Category ("NunitWeb")]
		public void MakeRelative7 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (MakeRelative7_Load)).Run (); 
		}

		public static void MakeRelative7_Load (Page p)
		{
			Assert.AreEqual ("./", VPU.MakeRelative ("~", "~"), "~, ~");
			Assert.AreEqual ("./", VPU.MakeRelative ("~/", "~/"));
			Assert.AreEqual ("./", VPU.MakeRelative ("~//", "~//"));
			Assert.AreEqual ("/", VPU.MakeRelative ("~", "~//"), "~, ~//");
			Assert.AreEqual ("directory1", VPU.MakeRelative ("~/directory1", "~/directory1"));
			Assert.AreEqual ("directory2", VPU.MakeRelative ("~/directory1", "~/directory2"));
			Assert.AreEqual ("directory1", VPU.MakeRelative ("~/", "~/directory1"));
			Assert.AreEqual ("./", VPU.MakeRelative ("~/directory1", "~/"));
			Assert.AreEqual ("./", VPU.MakeRelative ("~/directory1/", "~/directory1/"));
			Assert.AreEqual ("directory1/file1.aspx", VPU.MakeRelative ("~/directory1", "~/directory1/file1.aspx"));
			Assert.AreEqual ("file1.aspx", VPU.MakeRelative ("~/directory1/", "~/directory1/file1.aspx"));
			Assert.AreEqual ("../directory2/file2.aspx", VPU.MakeRelative ("~/directory1/file1.aspx", "~/directory2/file2.aspx"));

			Assert.AreEqual ("directory1", VPU.MakeRelative ("~/directory1/../", "~/directory1"));
			Assert.AreEqual ("./", VPU.MakeRelative ("~/directory1", "~/directory1/../"));
			Assert.AreEqual ("./", VPU.MakeRelative ("~/", "~/directory1/../"));
			Assert.AreEqual ("directory1", VPU.MakeRelative ("~/directory1", "~/directory2/../directory1"));


			Assert.AreEqual ("../", VPU.MakeRelative ("~", "/"));
			Assert.AreEqual ("NunitWeb/", VPU.MakeRelative ("/", "~"));
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
        [Category("NunitWeb")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAbsolute1 ()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute1_Load)).Run();
		}

        public static void ToAbsolute1_Load(Page p)
        {
            VPU.ToAbsolute(null);
        }

		[Test]
        [Category("NunitWeb")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAbsolute2 ()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute2_Load)).Run();
		}

        public static void ToAbsolute2_Load(Page p) 
        {
            VPU.ToAbsolute("");
        }

		[Test]
        [Category("NunitWeb")]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute3 ()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute3_Load)).Run();
		}

        public static void ToAbsolute3_Load(Page p)
        {
            VPU.ToAbsolute("..");
        }

		[Test]
		[Category ("NunitWeb")]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute4 ()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute4_Load)).Run();
		}

        public static void ToAbsolute4_Load(Page p)
        {
            VPU.ToAbsolute("...");
        }

		[Test]
		[Category ("NunitWeb")]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute5 ()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute5_Load)).Run();
		}

        public static void ToAbsolute5_Load(Page p)
        {
            VPU.ToAbsolute("../blah");
        }

		[Test]
        [Category("NunitWeb")]
        public void ToAbsolute6()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute6_Load)).Run();
		}
        public static void ToAbsolute6_Load(Page p)
        {
			Assert.AreEqual ("/" + NunitWebAppName + "/", VPU.ToAbsolute ("~/"));
			Assert.AreEqual ("/" + NunitWebAppName, VPU.ToAbsolute ("~"));
		}

		[Test]
		[Category("NunitWeb")]
		public void ToAbsolute7 ()
		{
            new WebTest(PageInvoker.CreateOnLoad(ToAbsolute7_Load)).Run();
		}
        public static void ToAbsolute7_Load(Page p)
        {
			Assert.AreEqual ("/", VPU.ToAbsolute ("/"));
			Assert.AreEqual ("/", VPU.ToAbsolute ("//"));
        }
		[Test]
		public void ToAbsolute8 ()
		{
			Assert.AreEqual ("/", VPU.ToAbsolute ("/", "/ROOT"));
			Assert.AreEqual ("/blah/blah/", VPU.ToAbsolute ("/blah//blah//", "/ROOT"));
			Assert.AreEqual ("/blah/blah/", VPU.ToAbsolute ("/blah\\blah/", "/ROOT"));
		}

		[Test]
		public void ToAbsolute8_a ()
		{
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1/../blah2/", "/ROOT"));
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1//../blah2/", "/ROOT"));
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1/\\../blah2/", "/ROOT"));
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1\\\\../blah2/", "/ROOT"));
			Assert.AreEqual ("/blah1/blah2/", VPU.ToAbsolute ("/blah1/./blah2/", "/ROOT"));

			Assert.AreEqual ("/blah1", VPU.ToAbsolute ("/blah1/blah2/..", "/ROOT"));
			Assert.AreEqual ("/", VPU.ToAbsolute ("/blah1/..", "/ROOT"));
			Assert.AreEqual ("/blah1/", VPU.ToAbsolute ("/blah1/blah2/../", "/ROOT"));
			Assert.AreEqual ("/", VPU.ToAbsolute ("/blah1/../", "/ROOT"));

			Assert.AreEqual ("/blah1", VPU.ToAbsolute ("/blah1/.", "/ROOT"));
			Assert.AreEqual ("/", VPU.ToAbsolute ("/.", "/ROOT"));
			Assert.AreEqual ("/blah1/", VPU.ToAbsolute ("/blah1/./", "/ROOT"));
			Assert.AreEqual ("/", VPU.ToAbsolute ("/./", "/ROOT"));
		}
		
		[Test]
		public void ToAbsolute8_b ()
		{
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1/../blah2/"));
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1//../blah2/"));
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1/\\../blah2/"));
			Assert.AreEqual ("/blah2/", VPU.ToAbsolute ("/blah1\\\\../blah2/"));
			Assert.AreEqual ("/blah1/blah2/", VPU.ToAbsolute ("/blah1/./blah2/"));
		}

		[Test]
		public void ToAbsolute9 ()
		{
			Assert.AreEqual ("/ROOT/", VPU.ToAbsolute ("~", "/ROOT"));
			Assert.AreEqual ("/ROOT/", VPU.ToAbsolute ("~/", "/ROOT"));
			Assert.AreEqual ("/ROOT/blah", VPU.ToAbsolute ("~/blah", "/ROOT/"));
		}

		[Test]
		public void ToAppRelative ()
		{
			Assert.AreEqual ("~/hi", VPU.ToAppRelative ("~/hi", null));
			Assert.AreEqual ("~/hi", VPU.ToAppRelative ("~/hi", ""));
			Assert.AreEqual ("~/hi", VPU.ToAppRelative ("~/hi", "/.."));
			Assert.AreEqual ("~/hi", VPU.ToAppRelative ("~/hi", "me"));

			Assert.AreEqual ("~", VPU.ToAppRelative ("~", "/ROOT"));
			Assert.AreEqual ("~/", VPU.ToAppRelative ("~/", "/ROOT"));
			Assert.AreEqual ("~/blah", VPU.ToAppRelative ("~/blah", "/ROOT/"));
			Assert.AreEqual ("~/blah2/", VPU.ToAppRelative ("~/blah1/../blah2/", "/ROOT/"));

			Assert.AreEqual ("~/", VPU.ToAppRelative ("/ROOT", "/ROOT"));
			Assert.AreEqual ("~/", VPU.ToAppRelative ("/ROOT/", "/ROOT"));
			Assert.AreEqual ("~/blah/blah/", VPU.ToAppRelative ("/ROOT/blah//blah//", "/ROOT"));
			Assert.AreEqual ("~/blah/blah/", VPU.ToAppRelative ("/ROOT/blah\\blah/", "/ROOT"));

			Assert.AreEqual ("~/blah2/", VPU.ToAppRelative ("/ROOT/blah1/../blah2/", "/ROOT"));
			Assert.AreEqual ("~/blah1/blah2/", VPU.ToAppRelative ("/ROOT/blah1/./blah2/", "/ROOT"));

			Assert.AreEqual ("~/blah1", VPU.ToAppRelative ("/ROOT/blah1/blah2/..", "/ROOT"));
			Assert.AreEqual ("~/", VPU.ToAppRelative ("/ROOT/blah1/..", "/ROOT"));
			Assert.AreEqual ("~/blah1/", VPU.ToAppRelative ("/ROOT/blah1/blah2/../", "/ROOT"));
			Assert.AreEqual ("~/", VPU.ToAppRelative ("/ROOT/blah1/../", "/ROOT"));

			Assert.AreEqual ("~/blah1", VPU.ToAppRelative ("/ROOT/blah1/.", "/ROOT"));
			Assert.AreEqual ("~/", VPU.ToAppRelative ("/ROOT/.", "/ROOT"));
			Assert.AreEqual ("~/blah1/", VPU.ToAppRelative ("/ROOT/blah1/./", "/ROOT"));
			Assert.AreEqual ("~/", VPU.ToAppRelative ("/ROOT/./", "/ROOT"));

			Assert.AreEqual ("~/ROOT", VPU.ToAppRelative ("/ROOT", "/"));
			Assert.AreEqual ("~/ROOT", VPU.ToAppRelative ("/ROOT", "/hi/.."));
			Assert.AreEqual ("~/ROOT", VPU.ToAppRelative ("/ROOT/hi/..", "/"));
		}

		[Test]
		public void ToAppRelative2 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (ToAppRelative2_Load)).Run ();
		}
		
		public static void ToAppRelative2_Load (Page p)
		{
			Assert.AreEqual ("~/hi", VPU.ToAppRelative ("~/../NunitWeb/hi", "/NunitWeb"));
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ToAppRelative_Exc1 ()
		{
			VPU.ToAppRelative ("/ROOT/hi", "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAppRelative_Exc2 ()
		{
			VPU.ToAppRelative ("/ROOT/hi", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAppRelative_Exc3 ()
		{
			VPU.ToAppRelative ("/ROOT/hi", "hi");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ToAppRelative_Exc4 () {
			VPU.ToAppRelative ("/ROOT/hi", "/../hi");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAppRelative_Exc5 ()
		{
			VPU.ToAppRelative (null, "/ROOT");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToAppRelative_Exc6 ()
		{
			VPU.ToAppRelative ("", "/ROOT");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		//The relative virtual path 'hi' is not allowed here.
		public void ToAppRelative_Exc7 ()
		{
			VPU.ToAppRelative ("hi", "/ROOT");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		//The relative virtual path 'hi' is not allowed here.
		public void ToAppRelative_Exc7_a ()
		{
			VPU.ToAppRelative ("hi", null);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void ToAppRelative_Exc8 ()
		{
			VPU.ToAppRelative ("/../ROOT/hi", "/ROOT");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute10 ()
		{
			VPU.ToAbsolute ("../blah", "/ROOT");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute11 ()
		{
			VPU.ToAbsolute ("blah", "/ROOT");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToAbsolute12 ()
		{
			VPU.ToAbsolute ("~/blah", "ROOT");
		}

		[Test]
		public void ToAbsolute13 ()
		{
			Assert.AreEqual ("/blah", VPU.ToAbsolute ("/blah", "ROOT"));
		}
	}
}





