//
// System.Web.HttpRequestTest.cs - Unit tests for System.Web.HttpRequest
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Miguel de Icaza    <miguel@novell.com>
//	Gonzalo Paniagua Javier <gonzalo@novell.com>
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
using System;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Diagnostics;
using MonoTests.SystemWeb.Framework;
using System.IO;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Web {

	[TestFixture]
	public class HttpRequestTest {

		[Test]
		[ExpectedException (typeof (HttpRequestValidationException))]
		public void ValidateInput_XSS ()
		{
			string problem = "http://server.com/attack2.aspx?test=<script>alert('vulnerability')</script>";
			string decoded = HttpUtility.UrlDecode (problem);
			int n = decoded.IndexOf ('?');
			HttpRequest request = new HttpRequest (null, decoded.Substring (0,n), decoded.Substring (n+1));
			request.ValidateInput ();
			// the next statement throws
			Assert.AreEqual ("<script>alert('vulnerability')</script>", request.QueryString ["test"], "QueryString");
		}

		// Notes:
		// * this is to avoid a regression that would cause Mono to 
		//   fail again on item #2 of the XSS vulnerabilities listed at:
		//   http://it-project.ru/andir/docs/aspxvuln/aspxvuln.en.xml
		// * The author notes that Microsoft has decided not to fix 
		//   this issue (hence the NotDotNet category).

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (HttpRequestValidationException))]
		public void ValidateInput_XSS_Unicode ()
		{
			string problem = "http://server.com/attack2.aspx?test=%uff1cscript%uff1ealert('vulnerability')%uff1c/script%uff1e";
			string decoded = HttpUtility.UrlDecode (problem);
			int n = decoded.IndexOf ('?');
			HttpRequest request = new HttpRequest (null, decoded.Substring (0,n), decoded.Substring (n+1));
			request.ValidateInput ();
			// the next statement throws
			Assert.AreEqual ("\xff1cscript\xff1ealert('vulnerability')\xff1c/script\xff1e", request.QueryString ["test"], "QueryString");
		}

		// This has affected ASP.NET 1.1 but it seems fixed now
		// http://secunia.com/advisories/9716/
		// http://weblogs.asp.net/kaevans/archive/2003/11/12/37169.aspx
		[Test]
		[ExpectedException (typeof (HttpRequestValidationException))]
		public void ValidateInput_XSS_Null ()
		{
			string problem = "http://secunia.com/?test=<%00SCRIPT>alert(document.cookie)</SCRIPT>";
			string decoded = HttpUtility.UrlDecode (problem);
			int n = decoded.IndexOf ('?');
			HttpRequest request = new HttpRequest (null, decoded.Substring (0,n), decoded.Substring (n+1));
			request.ValidateInput ();
			// the next statement throws
			Assert.AreEqual ("<SCRIPT>alert(document.cookie)</SCRIPT>", request.QueryString ["test"], "QueryString");
		}
		//
		// Tests the properties from the simple constructor.
		[Test]
		public void Test_PropertiesSimpleConstructor ()
		{
			string url = "http://www.gnome.org/";
			string qs = "key=value&key2=value%32second";
			
			HttpRequest r = new HttpRequest ("file", url, qs);

			Assert.AreEqual ("/?" + qs, r.RawUrl, "U1");
			Assert.AreEqual (url, r.Url.ToString (), "U2");

			r = new HttpRequest ("file", "http://www.gnome.org", qs);
			Assert.AreEqual (url, r.Url.ToString (), "U3");

			qs = "a&b=1&c=d&e&b=2&d=";
			r = new HttpRequest ("file", url, qs);
			NameValueCollection nvc = r.QueryString;

			Assert.AreEqual ("a,e", nvc [null], "U4");
			Assert.AreEqual ("1,2", nvc ["b"], "U5");
			Assert.AreEqual ("d", nvc ["c"], "U5");
			Assert.AreEqual ("", nvc ["d"], "U6");
			Assert.AreEqual (4, nvc.Count, "U6");

			Assert.AreEqual (null, r.ApplicationPath, "U7");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Test_AccessToVars ()
		{
			string url = "http://www.gnome.org/";
			string qs = "key=value&key2=value%32second";
			
			HttpRequest r = new HttpRequest ("file", url, qs);
			string s = r.PhysicalApplicationPath;
		}
	
		[Test]
		public void Test_QueryStringDecoding()
		{
			// Note: The string to encode must exist in default encoding language 
			// \r - will exist in all encodings
			string url = "http://www.gnome.org/";
			string qs = "umlaut=" + HttpUtility.UrlEncode("\r", Encoding.Default);

			HttpRequest r = new HttpRequest ("file", url, qs);
			Assert.AreEqual ("\r", r.QueryString["umlaut"]);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Test_PhysicalApplicationPath ()
		{
			WebTest t = new WebTest (new HandlerInvoker (new HandlerDelegate (
				PhysicalApplicationPathDelegate)));
			t.Run ();
		}

		static public void PhysicalApplicationPathDelegate ()
		{
			HttpRequest r = HttpContext.Current.Request;
			string pap = r.PhysicalApplicationPath;
			Assert.IsTrue (pap.EndsWith (Path.DirectorySeparatorChar.ToString()), "#1");
			Assert.AreEqual (Path.GetFullPath (pap), pap, "#2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Test_MapPath ()
		{
			WebTest t = new WebTest (new HandlerInvoker (new HandlerDelegate (
				MapPathDelegate)));
			t.Run ();
		}

		static public void MapPathDelegate ()
		{
			HttpRequest r = HttpContext.Current.Request;
			string appBase = r.PhysicalApplicationPath.TrimEnd (Path.DirectorySeparatorChar);
			Assert.AreEqual (appBase, r.MapPath ("~"), "test1");
			Assert.AreEqual (appBase, r.MapPath (null), "test1a");
			Assert.AreEqual (appBase, r.MapPath (""), "test1b");
			Assert.AreEqual (appBase, r.MapPath (" "), "test1c");
			Assert.AreEqual (appBase + Path.DirectorySeparatorChar, r.MapPath ("~/"), "test1");
			Assert.AreEqual (appBase, r.MapPath ("/NunitWeb"), "test1.1");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("Web.config"), "test2");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("~/Web.config"), "test3");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("/NunitWeb/Web.config"), "test4");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("Web.config", null, false), "test5");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("Web.config", "", false), "test6");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("Web.config", "/NunitWeb", false), "test7");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("/NunitWeb/Web.config", "/NunitWeb", false), "test8");
			
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("Web.config", "/NunitWeb/", false), "test8");
			Assert.AreEqual (Path.Combine (appBase, "Web.config"),
				r.MapPath ("Web.config", "~", false), "test10");
			Assert.AreEqual (Path.Combine (appBase, "DIR" + Path.DirectorySeparatorChar + "Web.config"),
				r.MapPath ("Web.config", "~/DIR", false), "test11");

			Assert.Throws<InvalidOperationException> (() => {
				// Throws because the test's virtual dir is /NunitWeb and / is above it
				r.MapPath ("/test.txt");
			}, "test12");

			Assert.Throws<InvalidOperationException> (() => {
				// Throws because the test's virtual dir is /NunitWeb and /NunitWeb1 does not match it
				r.MapPath ("/NunitWeb1/test.txt");
			}, "test13");

			Assert.Throws<ArgumentException> (() => {
				r.MapPath ("/test.txt", "/", false);
			}, "test14");

			Assert.Throws<ArgumentException> (() => {
				r.MapPath ("/test.txt", "/NunitWeb", false);
			}, "test15");

			Assert.AreEqual (Path.Combine (appBase, "test.txt"), r.MapPath ("/NunitWeb/test.txt", "/NunitWeb", true), "test16");
		}
	
		[Test]
		[Category ("NunitWeb")]
		[ExpectedException (typeof (ArgumentException))]
		public void Test_MapPath_InvalidBase_1 ()
		{
			WebTest t = new WebTest (new HandlerInvoker (new HandlerDelegate (
				MapPathDelegate_InvalidBase_1)));
			t.Run ();
		}

		static public void MapPathDelegate_InvalidBase_1 ()
		{
			HttpContext.Current.Request.MapPath ("Web.config", "something", true);
		}

		[Test]
		[Category ("NunitWeb")]
		[ExpectedException (typeof (ArgumentException))]
		public void Test_MapPath_InvalidBase_2 ()
		{
			WebTest t = new WebTest (new HandlerInvoker (new HandlerDelegate (
				MapPathDelegate_InvalidBase_2)));
			t.Run ();
		}

		static public void MapPathDelegate_InvalidBase_2 ()
		{
			HttpContext.Current.Request.MapPath ("Web.config", "something", false);
		}
	
		[Test]
		[ExpectedException (typeof (PlatformNotSupportedException))]
		public void ReadOnlyHeadersAdd ()
		{
			var r = new HttpRequest ("file", "http://www.gnome.org", "key=value&key2=value%32second");
			r.Headers.Add ("a","a");
		}

		[Test]
		[ExpectedException (typeof (PlatformNotSupportedException))]
		public void ReadOnlyHeadersSet ()
		{
			var r = new HttpRequest ("file", "http://www.gnome.org", "key=value&key2=value%32second");
			r.Headers.Set ("a","a");
		}

		[Test]
		[ExpectedException (typeof (PlatformNotSupportedException))]
		public void ReadOnlyHeadersRemove ()
		{
			var r = new HttpRequest ("file", "http://www.gnome.org", "key=value&key2=value%32second");
			r.Headers.Remove ("a");
		}
	}
	
	[TestFixture]
	public class Test_HttpFakeRequest {
		class FakeHttpWorkerRequest : HttpWorkerRequest {
			public int return_kind;

			[Conditional ("REQUEST_TEST_VERY_VERBOSE")]
			void WhereAmI ()
			{
				Console.WriteLine (Environment.StackTrace);
			}
			

			public FakeHttpWorkerRequest (int re)
			{
				return_kind = re;
			}
			
			public override string GetFilePath()
			{
				WhereAmI ();
				return "/tmp/uri.aspx";
			}

			public override string GetUriPath()
			{
				WhereAmI ();
				return "/uri.aspx";
			}
	
			public override string GetQueryString()
			{
				WhereAmI ();

				switch (return_kind) {
				case 20:
					return null;
				case 16:
					return "key1=value1&key2=value2";
				case 25: // HEAD
				case 30: // POST
					return "mapa.x=10&mapa.y=20";
				case 26: // HEAD
				case 31: // POST
					return "mapa.x=10&mapa=20";
				case 27: // GET
					return "mapa.x=10&mapa.y=20";
				case 28: // GET
					return "mapa.x=10";
				case 29: // GET
					return "mapa=10";
				case 32: // GET
					return "mapa.x=pi&mapa.y=20";
				case 50:
					return "PlainString";
				case 51:
					return "Plain&Arg=1";
				case 52:
					return "Empty=";
				default:
					return "GetQueryString";
				}
			}
	
			public override string GetRawUrl()
			{
				WhereAmI ();
				return "/bb.aspx";
			}
	
			public override string GetHttpVerbName()
			{
				WhereAmI ();
				if (return_kind == 25 || return_kind == 26)
					return "HEAD";
				if (return_kind == 30 || return_kind == 31)
					return "POST";
				return "GET";
			}
	
			public override string GetHttpVersion()
			{
				WhereAmI ();
				return "HTTP/1.1";
			}
	
			public override byte [] GetPreloadedEntityBody ()
			{
				if (return_kind != 30 && return_kind != 31)
					return base.GetPreloadedEntityBody ();

				return Encoding.UTF8.GetBytes (GetQueryString ());
			}

			public override bool IsEntireEntityBodyIsPreloaded ()
			{
				if (return_kind != 30 && return_kind != 31)
					return base.IsEntireEntityBodyIsPreloaded ();

				return true;
			}

			public override int GetRemotePort()
			{
				return 1010;
			}
	
			public override string GetLocalAddress()
			{
				return "localhost";
			}

			public override string GetAppPath ()
			{
				return "AppPath";
			}

			public override string GetRemoteName ()
			{
				return "RemoteName";
			}

			public override string GetRemoteAddress ()
			{
				return "RemoteAddress";
			}

			public override string GetServerName ()
			{
				return "localhost";
			}
			
			public override int GetLocalPort()
			{
				return 2020;
			}
	
			public override void SendStatus(int s, string x)
			{
			}
	
			public override void SendKnownResponseHeader(int x, string j)
			{
			}
	
			public override void SendUnknownResponseHeader(string a, string b)
			{
			}
		
			public override void SendResponseFromMemory(byte[] arr, int x)
			{
			}
	
			public override void SendResponseFromFile(string a, long b , long c)
			{
				WhereAmI ();
			}
	
			public override void SendResponseFromFile (IntPtr a, long b, long c)
			{
			}
	
			public override void FlushResponse(bool x)
			{
			}
	
			public override void EndOfRequest() {
			}

			public override string GetKnownRequestHeader (int index)
			{
				switch (index){
				case HttpWorkerRequest.HeaderContentType: 
					switch (return_kind){
					case 1: return "text/plain";
					case 2: return "text/plain; charset=latin1";
					case 3: return "text/plain; charset=iso-8859-1";
					case 4: return "text/plain; charset=\"iso-8859-1\"";	
					case 5: return "text/plain; charset=\"iso-8859-1\" ; other";
					case 30:
					case 31:
						return "application/x-www-form-urlencoded";
					}
					break;
					
				case HttpWorkerRequest.HeaderContentLength:
					switch (return_kind){
					case 0:  return "1024";
					case 1:  return "-1024";
					case 30:
					case 31:
						return GetQueryString ().Length.ToString ();
					case -1: return "Blah";
					case -2: return "";
					}
						break;

				case HttpWorkerRequest.HeaderCookie:
					switch (return_kind){
					case 10: return "Key=Value";
					case 11: return "Key=<value>";
					case 12: return "Key=>";
					case 13: return "Key=\xff1c";
					case 14: return "Key=\xff1e";
					}
					break;
				case HttpWorkerRequest.HeaderReferer:
					switch (return_kind){
					case 1: return null;
					case 2: return "http://www.mono-project.com/test.aspx";
					case 15: return "http://www.mono-project.com";
					case 33: return "x";
					}
					break;
				case HttpWorkerRequest.HeaderUserAgent:
					switch (return_kind){
					case 15: return "Mozilla/5.0 (X11; U; Linux i686; rv:1.7.3) Gecko/20040913 Firefox/0.10";
					}
					break;
				case HttpWorkerRequest.HeaderAccept:
					switch (return_kind){
					case 21: return "text/xml,application/xml, application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
					}
					break;
				case HttpWorkerRequest.HeaderAcceptLanguage:
					switch (return_kind){
					case 21: return "en-us, en;q=0.5";
					}
					break;
				}
				return "";
			}

			public override string [][] GetUnknownRequestHeaders ()
			{
				if (return_kind == 0)
					return new string [0][];

				if (return_kind == 3){
					string [][] x = new string [4][];
					x [0] = new string [] { "k1", "v1" };
					x [1] = new string [] { "k2", "v2" };
					x [2] = new string [] { "k3", "v3" };
					x [3] = new string [] { "k4", "v4" };

					return x;
				}

				if (return_kind == 4){
					//
					// This tests the bad values and the extra row with an error
					//
					string [][] x = new string [3][];
					x [0] = new string [] { "k1", "" };
					x [1] = new string [] { "k2", null };
					x [2] = new string [] { "k3", "   " };

					return x;
				}

				if (return_kind == 2){
					string [][] x = new string [2][];
					x [0] = new string [] { "k1", "" };

					// Returns an empty row.
					return x;
				}

				return null;
			}
		}

		HttpContext Cook (int re)
		{
			FakeHttpWorkerRequest f = new FakeHttpWorkerRequest (re);
			HttpContext c = new HttpContext (f);

			return c;
		}
		
		[Test] public void Test_BrokenContentLength ()
		{
			HttpContext c = Cook (-1);
			Assert.AreEqual (0, c.Request.ContentLength, "C1");

			c = Cook (-2);
			Assert.AreEqual (0, c.Request.ContentLength, "C2");

		}

		[Test]
		[Ignore ("tests undocumented behavior that depends on implementation")] 
		[ExpectedException(typeof(NullReferenceException))]
		public void Test_EmptyUnknownRow ()
		{
			HttpContext c = Cook (2);
			NameValueCollection x = c.Request.Headers;
		}
		
		[Test] 
	[Category ("NotWorking")]
	public void Test_RequestFields ()
		{
			HttpContext c = Cook (1);

			Assert.IsNull (c.Request.ApplicationPath, "A1");
			Assert.AreEqual ("text/plain", c.Request.ContentType, "A2");

			c = Cook (0);
			Assert.AreEqual (1024, c.Request.ContentLength, "A3");
			
			c = Cook (3);
			Assert.AreEqual ("iso-8859-1", c.Request.ContentEncoding.WebName, "A4");
			NameValueCollection x = c.Request.Headers;
			
			Assert.AreEqual ("v1", x ["k1"], "K1");
			Assert.AreEqual ("v2", x ["k2"], "K2");
			Assert.AreEqual ("v3", x ["k3"], "K3");
			Assert.AreEqual ("v4", x ["k4"], "K4");
			Assert.AreEqual ("text/plain; charset=iso-8859-1", x ["Content-Type"], "K4");
			Assert.AreEqual (5, x.Count, "K5");

			c = Cook (2);
			Assert.AreEqual ("iso-8859-1", c.Request.ContentEncoding.WebName, "A5");
			Assert.AreEqual ("text/plain; charset=latin1", c.Request.ContentType, "A5-1");

			c = Cook (4);
			Assert.AreEqual ("iso-8859-1", c.Request.ContentEncoding.WebName, "A6");
			x = c.Request.Headers;
			Assert.AreEqual ("", x ["k1"], "K6");
			Assert.AreEqual (null, x ["k2"], "K7");
			Assert.AreEqual ("   ", x ["k3"], "K8");
			Assert.AreEqual (4, x.Count, "K9");

			c = Cook (5);
			Assert.AreEqual ("iso-8859-1", c.Request.ContentEncoding.WebName, "A7");

			Assert.AreEqual ("RemoteName", c.Request.UserHostName, "A8");
			Assert.AreEqual ("RemoteAddress", c.Request.UserHostAddress, "A9");

			// Difference between Url property and RawUrl one: one is resolved, the other is not
			Assert.AreEqual ("/bb.aspx", c.Request.RawUrl, "A10");
			Assert.AreEqual ("http://localhost:2020/uri.aspx?GetQueryString", c.Request.Url.ToString (), "A11");
		}
		
		[Test] public void Test_Cookies ()
		{
			HttpContext c;

			c = Cook (10);
			c.Request.ValidateInput ();
			Assert.AreEqual ("Value", c.Request.Cookies ["Key"].Value, "cookie1");
		}

		[Test]
		[ExpectedException(typeof (HttpRequestValidationException))]
		public void Test_DangerousCookie ()
		{
			HttpContext c;

			c = Cook (11);
			c.Request.ValidateInput ();
			object a = c.Request.Cookies;
		}
		
		[Test]
		public void Test_DangerousCookie2 ()
		{
			HttpContext c;

			c = Cook (12);
			c.Request.ValidateInput ();
			object a = c.Request.Cookies;
		}
		
		[Test]
		public void Test_DangerousCookie3 ()
		{
			HttpContext c;

			c = Cook (13);
			c.Request.ValidateInput ();
			object a = c.Request.Cookies;
		}
		
		[Test]
		public void Test_DangerousCookie4 ()
		{
			HttpContext c;

			c = Cook (14);
			c.Request.ValidateInput ();
			object a = c.Request.Cookies;
		}

		[Test]
		public void Test_MiscHeaders ()
		{
			HttpContext c = Cook (15);

			// The uri ToString contains the trailing slash.
			Assert.AreEqual ("http://www.mono-project.com/", c.Request.UrlReferrer.ToString (), "ref1");
			
			Assert.AreEqual ("Mozilla/5.0 (X11; U; Linux i686; rv:1.7.3) Gecko/20040913 Firefox/0.10", c.Request.UserAgent, "ref2");

			// All the AcceptTypes and UserLanguages tests below here pass under MS
			c = Cook (20);
			string [] at = c.Request.AcceptTypes;
			string [] ul = c.Request.UserLanguages;
			Assert.IsNull (at, "AT1");
			Assert.IsNull (ul, "UL1");
			c = Cook (21);
			at = c.Request.AcceptTypes;
			Assert.IsNotNull (at, "AT2");
			string [] expected = { "text/xml", "application/xml", "application/xhtml+xml", "text/html;q=0.9",
						"text/plain;q=0.8", "image/png", "*/*;q=0.5" };

			Assert.AreEqual (expected.Length, at.Length, "AT3");
			for (int i = expected.Length - 1; i >= 0; i--)
				Assert.AreEqual (expected [i], at [i], "AT" + (3 + i));

			ul = c.Request.UserLanguages;
			Assert.IsNotNull (ul, "UL2");
			expected = new string [] { "en-us", "en;q=0.5" };

			Assert.AreEqual (expected.Length, ul.Length, "UL3");
			for (int i = expected.Length - 1; i >= 0; i--)
				Assert.AreEqual (expected [i], ul [i], "UL" + (3 + i));

		}

		[Test]
		public void Empty_WorkerRequest_QueryString ()
		{
			HttpContext c = Cook (20);

			//
			// Checks that the following line does not throw an exception if
			// the querystring returned by the HttpWorkerRequest is null.
			//
			NameValueCollection nvc = c.Request.QueryString;
		}

		[Test]
		public void Test_QueryString_ToString ()
		{
			HttpContext c = Cook (50);

			Assert.AreEqual (c.Request.QueryString.ToString (), "PlainString", "QTS#1");

			c = Cook (51);
			Assert.AreEqual (c.Request.QueryString.ToString (), "Plain&Arg=1", "QTS#2");
		}

		[Test]
		public void QueryString_NullTest ()
		{
			HttpRequest req = new HttpRequest ("file.aspx", "http://localhost/file.aspx", null);
			
			Assert.AreEqual (req.QueryString.ToString (), "", "QSNT#1");
		}
		
		[Test]
		public void Leading_qm_in_QueryString ()
		{
			HttpContext c = Cook (16);
			NameValueCollection nvc = c.Request.QueryString;
			foreach (string id in nvc.AllKeys) {
				if (id.StartsWith ("?"))
					Assert.Fail (id);
			}
		}

		[Test]
		public void TestPath ()
		{
			HttpContext c = Cook (16);

			// This used to crash, ifolder exposed this
			string x = c.Request.Path;
		}
		
		[Test]
		public void MapImageCoordinatesHEAD ()
		{
			HttpContext c = Cook (25);
			int [] coords = c.Request.MapImageCoordinates ("mapa");
			Assert.IsNotNull (coords, "A1");
			Assert.AreEqual (10, coords [0], "X");
			Assert.AreEqual (20, coords [1], "Y");

			c = Cook (26);
			coords = c.Request.MapImageCoordinates ("mapa");
			Assert.AreEqual (null, coords, "coords");
		}

		[Test]
		public void MapImageCoordinatesGET ()
		{
			HttpContext c = Cook (27);
			int [] coords = c.Request.MapImageCoordinates ("mapa");
			Assert.AreEqual (10, coords [0], "X");
			Assert.AreEqual (20, coords [1], "Y");

			coords = c.Request.MapImageCoordinates ("m");
			Assert.AreEqual (null, coords, "coords1");

			c = Cook (28);
			coords = c.Request.MapImageCoordinates ("mapa");
			Assert.AreEqual (null, coords, "coords2");
			c = Cook (29);
			coords = c.Request.MapImageCoordinates ("mapa");
			Assert.AreEqual (null, coords, "coords3");
			c = Cook (32);
			coords = c.Request.MapImageCoordinates ("mapa");
			Assert.AreEqual (null, coords, "coords4");
		}

		[Test]
		public void MapImageCoordinatesPOST ()
		{
			HttpContext c = Cook (30);
			int [] coords = c.Request.MapImageCoordinates ("mapa");
			Assert.IsNotNull (coords, "A1");
			Assert.AreEqual (10, coords [0], "X");
			Assert.AreEqual (20, coords [1], "Y");

			c = Cook (31);
			coords = c.Request.MapImageCoordinates ("mapa");
			Assert.AreEqual (null, coords, "coords2");
		}

		[Test]
		public void TestReferer ()
		{
			HttpContext c = Cook (1);

			Assert.AreEqual (null, c.Request.UrlReferrer, "REF1");

			c = Cook (2);
			Assert.AreEqual ("http://www.mono-project.com/test.aspx", c.Request.UrlReferrer.ToString (), "REF1");

			c = Cook (33);
			Assert.AreEqual (null, c.Request.UrlReferrer, "REF1");			
		}
		

		[Test]
		public void NegativeContentLength ()
		{
			HttpContext c = Cook (1);
			HttpRequest req = c.Request;
			Assert.AreEqual (0, req.ContentLength, "#01");
		}

		[Test]
		public void EmptyQueryValueParams ()
		{
			HttpContext c = Cook (52);
			Assert.AreEqual ("", c.Request.Params["Empty"]);
		}
	}

	// This class is defined here to make it easy to create fake
	// HttpWorkerRequest-derived classes by only overriding the methods
	// necessary for testing.
	class BaseFakeHttpWorkerRequest : HttpWorkerRequest
	{
		public override void EndOfRequest()
		{
		}

		public override void FlushResponse(bool finalFlush)
		{
		}

		public override string GetHttpVerbName()
		{
			return "GET";
		}

		public override string GetHttpVersion()
		{
			return "HTTP/1.1";
		}

		public override string GetLocalAddress()
		{
			return "localhost";
		}

		public override int GetLocalPort()
		{
			return 8080;
		}

		public override string GetQueryString()
		{
			return String.Empty;
		}

		public override string GetRawUrl()
		{
			string rawUrl = GetUriPath();
			string queryString = GetQueryString();
			if (queryString != null && queryString.Length > 0)
			{
				rawUrl += "?" + queryString;
			}
			return rawUrl;
		}

		public override string GetRemoteAddress()
		{
			return "remotehost";
		}

		public override int GetRemotePort()
		{
			return 8080;
		}

		public override string GetUriPath()
		{
			return "default.aspx";
		}

		public override void SendKnownResponseHeader(int index, string value)
		{
		}

		public override void SendResponseFromFile(IntPtr handle, long offset, long length)
		{
		}

		public override void SendResponseFromFile(string filename, long offset, long length)
		{
		}

		public override void SendResponseFromMemory(byte[] data, int length)
		{
		}

		public override void SendStatus(int statusCode, string statusDescription)
		{
		}

		public override void SendUnknownResponseHeader(string name, string value)
		{
		}
	}

	// This test ensures accessing the Form property does not throw an
	// exception when the length of data in the request exceeds the length
	// as reported by the Content-Length header. This bug was discovered
	// with an AJAX application using XMLHttpRequest to POST back to the
	// server. The Content-Length header was two bytes less than the length
	// of the buffer returned from GetPreloadedEntityBody. This was causing
	// an exception to be thrown by Mono because it was trying to allocate
	// a buffer that was -2 bytes in length.
	[TestFixture]
	public class Test_UrlEncodedBodyWithExtraCRLF
	{
		class FakeHttpWorkerRequest : BaseFakeHttpWorkerRequest
		{
			// This string is 9 bytes in length. That's 2 more than
			// the Content-Length header says it should be.
			string data = "foo=bar\r\n";

			public override string GetKnownRequestHeader(int index)
			{
				switch (index)
				{
					case HttpWorkerRequest.HeaderContentLength:
						return (data.Length - 2).ToString();
					case HttpWorkerRequest.HeaderContentType:
						return "application/x-www-form-urlencoded";
				}
				return String.Empty;
			}

			public override byte[] GetPreloadedEntityBody()
			{
				return Encoding.ASCII.GetBytes(data);
			}
		}

		HttpContext context = null;

		[SetUp]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void SetUp()
		{
			HttpWorkerRequest workerRequest = new FakeHttpWorkerRequest();
			context = new HttpContext(workerRequest);
		}

		[Test]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void ContentLength()
		{
			Assert.AreEqual(7, context.Request.ContentLength);
		}

		[Test]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void Form_Count()
		{
			Assert.AreEqual(1, context.Request.Form.Count);
		}

		[Test]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void Form_Item()
		{
			// I would have expected the extra two characters to be stripped
			// but Microsoft's CLR keeps them so Mono should, too.
			//Assert.AreEqual("bar\r\n", context.Request.Form["foo"]);
			Assert.AreEqual("bar", context.Request.Form["foo"]);
		}
	}

	// This test ensures the HttpRequet object's Form property gets
	// properly constructed and populated when the Content-Type header
	// includes a charset parameter and that the charset parameter is
	// respected.
	[TestFixture]
	public class Test_UrlEncodedBodyWithUtf8CharsetParameter
	{
		class FakeHttpWorkerRequest : BaseFakeHttpWorkerRequest
		{
			// The two funny-looking characters are really a single
			// accented "a" character encoded in UTF-8.
			string data = "foo=b%C3%A1r";

			public override string GetKnownRequestHeader(int index)
			{
				switch (index)
				{
					case HttpWorkerRequest.HeaderContentLength:
						return data.Length.ToString();
					case HttpWorkerRequest.HeaderContentType:
						return "application/x-www-form-urlencoded; charset=utf-8";
				}
				return String.Empty;
			}

			public override byte[] GetPreloadedEntityBody()
			{
				return Encoding.ASCII.GetBytes(data);
			}
		}

		HttpContext context = null;

		[SetUp]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void SetUp()
		{
			HttpWorkerRequest workerRequest = new FakeHttpWorkerRequest();
			context = new HttpContext(workerRequest);
		}

		[Test]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void Form_Count()
		{
			Assert.AreEqual(1, context.Request.Form.Count);
		}

		[Test]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void Form_Item()
		{
			Assert.AreEqual("b\xE1r", context.Request.Form["foo"]);
		}

	}

	// This test ensures the HttpRequest object's InputStream property
	// gets properly constructed and populated when the request is not
	// preloaded.
	[TestFixture]
	public class Test_NonPreloadedRequest
	{
		private const string expected = "Hello, World!\n";

		class FakeHttpWorkerRequest : BaseFakeHttpWorkerRequest
		{
			private readonly Stream body = new MemoryStream(Encoding.UTF8.GetBytes(expected));

			public override string GetHttpVerbName()
			{
				return "POST";
			}

			public override int ReadEntityBody(byte[] buffer, int size)
			{
				return body.Read(buffer, 0, size);
			}
		}

		HttpContext context = null;

		[SetUp]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void SetUp()
		{
			HttpWorkerRequest workerRequest = new FakeHttpWorkerRequest();
			context = new HttpContext(workerRequest);
		}

		[Test]
		[Category ("NotDotNet")] // Cannot be runned on .net with no web context
		public void InputStream_Contents()
		{
			Assert.AreEqual(expected, new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd());
		}

	}
}

