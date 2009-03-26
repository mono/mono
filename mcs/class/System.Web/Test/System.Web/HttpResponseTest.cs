//
// System.Web.HttpResponseTest.cs - Unit tests for System.Web.HttpResponse
//
// Author:
//	Miguel de Icaza  <miguel@ximian.com>
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

using System.Text;
using System.Web;
using System;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Web {

	public class FakeHttpWorkerRequest2 : HttpWorkerRequest {
		public Hashtable KnownResponseHeaders;
		public Hashtable UnknownResponseHeaders;
		public int return_kind;
		
		public FakeHttpWorkerRequest2 (int re)
		{
			KnownResponseHeaders = CollectionsUtil.CreateCaseInsensitiveHashtable();
			UnknownResponseHeaders = CollectionsUtil.CreateCaseInsensitiveHashtable();
			return_kind = re;
		}
		
		public override string GetUriPath()
		{
			return "/fake";
		}
		
		public override string GetQueryString()
		{
			return "GetQueryString";
		}
		
		public override string GetRawUrl()
		{
			return "GetRawUrl";
		}
		
		public override string GetHttpVerbName()
		{
			return "GET";
		}
		
		public override string GetHttpVersion()
		{
			if (return_kind == 1)
				return "HTTP/1.0";
			else
				return "HTTP/1.1";
		}
		
		public override string GetRemoteAddress()
		{
			return "__GetRemoteAddress";
			}
	
		public override int GetRemotePort()
		{
			return 1010;
		}
		
		public override string GetLocalAddress()
		{
			return "GetLocalAddress";
		}
		
		public override string GetAppPath ()
		{
			return "AppPath";
		}
		
		public override int GetLocalPort()
		{
			return 2020;
		}

		public bool status_sent;
		public int status_code;
		public string status_string;
		
		public override void SendStatus(int s, string x)
		{
			status_sent = true;
			status_code = s;
			status_string = x;
		}

		void AddHeader (Hashtable table, string header_name, object header)
		{
			object o = table [header_name];
			if (o == null)
				table.Add (header_name, header);
			else {
				ArrayList al = o as ArrayList;
				if (al == null) {
					al = new ArrayList ();
					al.Add (o);
					table [header_name] = al;
				} else
					al = o as ArrayList;
				
				al.Add (header);
			}
		}
		
		bool headers_sent;
		public override void SendKnownResponseHeader(int x, string j)
		{
			string header_name = HttpWorkerRequest.GetKnownRequestHeaderName (x);
			AddHeader (KnownResponseHeaders, header_name, new KnownResponseHeader (x, j));
			headers_sent = true;
		}
		
		public override void SendUnknownResponseHeader(string a, string b)
		{
			AddHeader (UnknownResponseHeaders, a, new UnknownResponseHeader (a, b));
			headers_sent = true;
		}

		bool data_sent;
		public byte [] data;
		public int data_len;
		public int total = 0;
		
		public override void SendResponseFromMemory(byte[] arr, int x)
		{
			data_sent = true;
			data = new byte [x];
			for (int i = 0; i < x; i++)
				data [i] = arr [i];
			data_len = x;
			total += data_len;
		}
		
		public override void SendResponseFromFile(string a, long b , long c)
		{
			data_sent = true;
		}
		
		public override void SendResponseFromFile (IntPtr a, long b, long c)
		{
			data_sent = true;
		}
		
		public override void FlushResponse(bool x)
		{
		}
		
		public override void EndOfRequest() {
		}
		
		public override string GetKnownRequestHeader (int index)
		{
			return null;
		}

		public bool OutputProduced {
			get {
				return headers_sent || data_sent;
			}
		}
	}

	class KnownResponseHeader
	{
		private int index;
		private string value;

		public KnownResponseHeader (int index, string value)
		{
			this.index = index;
			this.value = value;
		}

		public int Index {
			get { return index; }
		}

		public string Value {
			get { return value; }
		}
	}

	class UnknownResponseHeader
	{
		private string name;
		private string value;

		public UnknownResponseHeader (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public string Name {
			get { return name; }
		}

		public string Value {
			get { return value; }
		}
	}

	[TestFixture]
	public class HttpResponseTest {
		HttpContext Cook (int re, out FakeHttpWorkerRequest2 f)
		{
			f = new FakeHttpWorkerRequest2 (re);
			HttpContext c = new HttpContext (f);

			return c;
		}

		[SetUp]
		public void SetUp ()
		{
#if NET_2_0
			AppDomain.CurrentDomain.SetData (".appPath", AppDomain.CurrentDomain.BaseDirectory);
#endif
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // char output stream in gh make this test fail
#endif
		public void Test_Response ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (1, out f);

			c.Response.Write ("a");
			Assert.AreEqual (false, f.OutputProduced, "T1");
			c.Response.Flush ();
			Assert.AreEqual (1, f.data_len, "T2");
			c.Response.Write ("Hola");
			Assert.AreEqual (1, f.data_len, "T3");
			c.Response.Flush ();
			Assert.AreEqual (4, f.data_len, "T4");
			Assert.AreEqual ((byte) 'H', f.data [0], "T5");
			Assert.AreEqual ((byte) 'o', f.data [1], "T6");
			Assert.AreEqual ((byte) 'l', f.data [2], "T7");
			Assert.AreEqual ((byte) 'a', f.data [3], "T8");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // char output stream in gh make this test fail
#endif
		public void TestResponse_Chunked ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			c.Response.Write ("a");
			Assert.AreEqual (false, f.OutputProduced, "CT1");
			c.Response.Flush ();
			Assert.AreEqual (6, f.total, "CT2");
			c.Response.Write ("Hola");
			Assert.AreEqual (6, f.total, "CT3");
			c.Response.Flush ();
			
		}

		[Test]
		public void Status1 ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.Status = "200 Lalala";
			Assert.AreEqual (200, resp.StatusCode, "ST1");
			Assert.AreEqual ("Lalala", resp.StatusDescription, "ST2");

			resp.Status = "10000 La la la";
			Assert.AreEqual (10000, resp.StatusCode, "ST3");
			Assert.AreEqual ("La la la", resp.StatusDescription, "ST4");

			resp.Status = "-1 La la la";
			Assert.AreEqual (-1, resp.StatusCode, "ST5");
			Assert.AreEqual ("La la la", resp.StatusDescription, "ST6");

			resp.Status = "-200 La la la";
			Assert.AreEqual (-200, resp.StatusCode, "ST7");
			Assert.AreEqual ("La la la", resp.StatusDescription, "ST8");

			resp.Status = "200 ";
			Assert.AreEqual (200, resp.StatusCode, "ST7");
			Assert.AreEqual ("", resp.StatusDescription, "ST8");
		}

		[Test]
		public void Status2 ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			try {
				resp.Status = "200";
				Assert.Fail ("#1");
			} catch (HttpException) {
			}
		}

		[Test]
		public void Status3 ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			try {
				resp.Status = "200\t";
				Assert.Fail ("#1");
			} catch (HttpException) {
			}
		}

		[Test]
		public void Status4 ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;

			Assert.AreEqual (200, resp.StatusCode, "STT1");
			Assert.AreEqual (HttpWorkerRequest.GetStatusDescription (200), resp.StatusDescription, "STT2");

			resp.StatusCode = 400;
			Assert.AreEqual (400, resp.StatusCode, "STT3");
			Assert.AreEqual (HttpWorkerRequest.GetStatusDescription (400), resp.StatusDescription, "STT4");

			resp.StatusDescription = "Something else";
			Assert.AreEqual (400, resp.StatusCode, "STT5");
			Assert.AreEqual ("Something else", resp.StatusDescription, "STT6");

			resp.StatusDescription = null;
			Assert.AreEqual (400, resp.StatusCode, "STT7");
			Assert.AreEqual (HttpWorkerRequest.GetStatusDescription (400), resp.StatusDescription, "STT8");
		}

		//
		// TODO: Add test for BinaryWrite and the various writes to check for Chunked Mode
		//`

		[Test]
		public void SetCacheability ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (1, out f);

			//
			// Basically the values from CacheControl are useless once Response.Cache is used
			//
			c.Response.Cache.SetCacheability (HttpCacheability.ServerAndNoCache);
			Assert.AreEqual ("private", c.Response.CacheControl, "C1");
			
			c.Response.Cache.SetCacheability (HttpCacheability.ServerAndPrivate);
			Assert.AreEqual ("private", c.Response.CacheControl, "C2");
			
			c.Response.Cache.SetCacheability (HttpCacheability.NoCache);
			Assert.AreEqual ("private", c.Response.CacheControl, "C3");
			
			c.Response.Cache.SetCacheability (HttpCacheability.Private);
			Assert.AreEqual ("private", c.Response.CacheControl, "C4");
			
			c.Response.Cache.SetCacheability (HttpCacheability.Server);
			Assert.AreEqual ("private", c.Response.CacheControl, "C5");
			
			c.Response.Cache.SetCacheability (HttpCacheability.Public);
			Assert.AreEqual ("private", c.Response.CacheControl, "C6");
		}

		//
		// Test the values allowed;  .NET only documents private and public, but
		// "no-cache" from the spec is also allowed
		//
		[Test]
		public void CacheControl ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (1, out f);

			// Default value.
			Assert.AreEqual ("private", c.Response.CacheControl, "D1");
					 
			c.Response.CacheControl = "private";
			Assert.AreEqual ("private", c.Response.CacheControl, "D2");

			c.Response.CacheControl = "public";
			Assert.AreEqual ("public", c.Response.CacheControl, "D3");
			
			c.Response.CacheControl = "no-cache";
			Assert.AreEqual ("no-cache", c.Response.CacheControl, "D4");

			c.Response.CacheControl = null;
			Assert.AreEqual ("private", c.Response.CacheControl, "D5");

			c.Response.CacheControl = "";
			Assert.AreEqual ("private", c.Response.CacheControl, "D6");
		}

		//
		// Just checks if the AddFileDepend* methods accept values, added after bug #342511
		[Test]
		public void AddFileDependencies ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (1, out f);

			ArrayList a = new ArrayList (1);
			a.Add ("somefile.txt");
			c.Response.AddFileDependencies (a);

#if NET_2_0
			string[] sa = new string [1] {"somefile.txt"};
			c = Cook (1, out f);
			c.Response.AddFileDependencies (sa);
#endif

			c = Cook (1, out f);
			c.Response.AddFileDependency ("somefile.txt");
		}

		[Test] // bug #488702
		public void WriteHeaders ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.CacheControl = "public";
			resp.Cache.SetCacheability (HttpCacheability.NoCache);
			resp.ContentType = "text/xml";
			resp.AppendHeader ("Content-Disposition", "inline");
			resp.AppendHeader ("Content-Type", "application/ms-word");
			resp.AppendHeader ("Content-Length", "40");
			resp.AppendHeader ("Transfer-Encoding", "compress");
			resp.AppendHeader ("My-Custom-Header", "never");
			resp.AppendHeader ("My-Custom-Header", "always");

			Assert.AreEqual ("public", resp.CacheControl, "#A1");
			Assert.AreEqual ("application/ms-word", resp.ContentType, "#A2");
			Assert.AreEqual (0, f.KnownResponseHeaders.Count, "#A3");
			Assert.AreEqual (0, f.UnknownResponseHeaders.Count, "#A4");

			resp.Flush ();

			KnownResponseHeader known;

			Assert.AreEqual (6, f.KnownResponseHeaders.Count, "#B1");
			
			known = (KnownResponseHeader)f.KnownResponseHeaders ["Content-Length"];
			Assert.AreEqual (HttpWorkerRequest.HeaderContentLength, known.Index, "#B2");
			Assert.AreEqual ("40", known.Value, "#B3");
			
			known = (KnownResponseHeader)f.KnownResponseHeaders["Transfer-Encoding"];
			Assert.AreEqual (HttpWorkerRequest.HeaderTransferEncoding, known.Index, "#B4");
			Assert.AreEqual ("compress", known.Value, "#B5");
			
			known = (KnownResponseHeader)f.KnownResponseHeaders["Cache-Control"];
			Assert.AreEqual (HttpWorkerRequest.HeaderCacheControl, known.Index, "#B6");
			Assert.AreEqual ("no-cache", known.Value, "#B7");
			
			known = (KnownResponseHeader)f.KnownResponseHeaders["Pragma"];
			Assert.AreEqual (HttpWorkerRequest.HeaderPragma, known.Index, "#B8");
			Assert.AreEqual ("no-cache", known.Value, "#B9");
			
			known = (KnownResponseHeader)f.KnownResponseHeaders["Expires"];
			Assert.AreEqual (HttpWorkerRequest.HeaderExpires, known.Index, "#B10");
			Assert.AreEqual ("-1", known.Value, "#B11");
			
			known = (KnownResponseHeader)f.KnownResponseHeaders["Content-Type"];
			Assert.AreEqual (HttpWorkerRequest.HeaderContentType, known.Index, "#B12");
			Assert.AreEqual ("application/ms-word", known.Value, "#B13");

			UnknownResponseHeader unknown;

			Assert.AreEqual (3, f.UnknownResponseHeaders.Count, "#C1");
			
			unknown = (UnknownResponseHeader) f.UnknownResponseHeaders ["X-AspNet-Version"];
			Assert.AreEqual ("X-AspNet-Version", unknown.Name, "#C2");
			Assert.AreEqual (Environment.Version.ToString (3), unknown.Value, "#C3");
			
			unknown = (UnknownResponseHeader) f.UnknownResponseHeaders ["Content-Disposition"];
			Assert.AreEqual ("Content-Disposition", unknown.Name, "#C4");
			Assert.AreEqual ("inline", unknown.Value, "#C5");

			ArrayList al = f.UnknownResponseHeaders ["My-Custom-Header"] as ArrayList;
			Assert.AreEqual (2, al.Count, "#C6");
			
			unknown = (UnknownResponseHeader) al [0];
			Assert.AreEqual ("My-Custom-Header", unknown.Name, "#C7");
			Assert.AreEqual ("never", unknown.Value, "#C8");
			
			unknown = (UnknownResponseHeader) al [1];
			Assert.AreEqual ("My-Custom-Header", unknown.Name, "#C9");
			Assert.AreEqual ("always", unknown.Value, "#C10");
		}

		[Test] // bug #485557
		[Category ("NotWorking")] // bug #488702
		public void ClearHeaders ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.CacheControl = "public";
			resp.Cache.SetCacheability (HttpCacheability.NoCache);
			resp.ContentType = "text/xml";
			resp.AppendHeader ("Content-Disposition", "inline");
			resp.AppendHeader ("Content-Type", "application/ms-word");
			resp.AppendHeader ("Content-Length", "40");
			resp.AppendHeader ("Transfer-Encoding", "compress");
			resp.AppendHeader ("My-Custom-Header", "never");
			resp.AppendHeader ("My-Custom-Header", "always");
			resp.ClearHeaders ();

			Assert.AreEqual ("private", resp.CacheControl, "#A1");
			Assert.AreEqual ("text/html", resp.ContentType, "#A2");
			Assert.AreEqual (0, f.KnownResponseHeaders.Count, "#A3");
			Assert.AreEqual (0, f.UnknownResponseHeaders.Count, "#A4");

			resp.Flush ();

			KnownResponseHeader known;

			Assert.AreEqual (2, f.KnownResponseHeaders.Count, "#B1");
			// Assert.IsTrue (f != null, "f is null");
			// Assert.IsTrue (f.KnownResponseHeaders != null, "f.KnownResponseHeaders is null");
			// Assert.IsTrue (f.KnownResponseHeaders ["Transfer-Encoding"] != null, "No Transfer-Encoding");
			
			// known = (KnownResponseHeader) f.KnownResponseHeaders ["Transfer-Encoding"];
			// Assert.AreEqual (HttpWorkerRequest.HeaderTransferEncoding, known.Index, "#B2");
			// Assert.AreEqual ("chunked", known.Value, "#B3");
			
			known = (KnownResponseHeader) f.KnownResponseHeaders ["Cache-Control"];
			Assert.AreEqual (HttpWorkerRequest.HeaderCacheControl, known.Index, "#B4");
			Assert.AreEqual ("private", known.Value, "#B5");
			
			known = (KnownResponseHeader) f.KnownResponseHeaders ["Content-Type"];
			Assert.AreEqual (HttpWorkerRequest.HeaderContentType, known.Index, "#B6");
			Assert.AreEqual ("text/html", known.Value, "#B7");

#if NET_2_0
			Assert.AreEqual (1, f.UnknownResponseHeaders.Count, "#C1");
			UnknownResponseHeader unknown = (UnknownResponseHeader) f.UnknownResponseHeaders ["X-AspNet-Version"];
			Assert.AreEqual ("X-AspNet-Version", unknown.Name, "#C2");
			Assert.AreEqual (Environment.Version.ToString (3), unknown.Value, "#C3");
#else
			Assert.AreEqual (0, f.UnknownResponseHeaders.Count, "#C1");
#endif
		}
	}
}
