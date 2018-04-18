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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.Caching;

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
			return "/GetRawUrl";
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
			if (data != null) {
				byte [] tmp = new byte [data.Length + x];
				Array.Copy (data, tmp, data.Length);
				Array.Copy (arr, 0, tmp, data.Length, x);
				data = tmp;
				data_len = data.Length;
			} else {
				data = new byte [x];
				for (int i = 0; i < x; i++)
					data [i] = arr [i];
				data_len = x;
			}
			total += x;
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
		public static HttpContext Cook (int re, out FakeHttpWorkerRequest2 f)
		{
			f = new FakeHttpWorkerRequest2 (re);
			HttpContext c = new HttpContext (f);

			return c;
		}

		[SetUp]
		public void SetUp ()
		{
			AppDomain.CurrentDomain.SetData (".appPath", AppDomain.CurrentDomain.BaseDirectory);
		}

		[Test]
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
			Assert.AreEqual (5, f.data_len, "T4");
			Assert.AreEqual ((byte) 'a', f.data [0], "T5");
			Assert.AreEqual ((byte) 'H', f.data [1], "T6");
			Assert.AreEqual ((byte) 'o', f.data [2], "T7");
			Assert.AreEqual ((byte) 'l', f.data [3], "T8");
			Assert.AreEqual ((byte) 'a', f.data [4], "T9");
		}

		[Test]
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

			string[] sa = new string [1] {"somefile.txt"};
			c = Cook (1, out f);
			c.Response.AddFileDependencies (sa);

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

		[Test] // pull #866
		public void WriteHeadersNoCharset ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.ContentType = "text/plain";

			Assert.AreEqual ("text/plain", resp.ContentType, "#A1");

			resp.Flush ();

			KnownResponseHeader known;

			AssertHelper.LessOrEqual (1, f.KnownResponseHeaders.Count, "#B1");

			known = (KnownResponseHeader)f.KnownResponseHeaders ["Content-Type"];
			Assert.AreEqual (HttpWorkerRequest.HeaderContentType, known.Index, "#B2");
			Assert.AreEqual ("text/plain", known.Value, "#B3");
		}

		[Test] // pull #866
		public void WriteHeadersHasCharset ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.ContentType = "text/plain";
			resp.Charset = "big5";

			Assert.AreEqual ("text/plain", resp.ContentType, "#A1");
			Assert.AreEqual ("big5", resp.Charset, "#A2");

			resp.Flush ();

			KnownResponseHeader known;

			AssertHelper.LessOrEqual (1, f.KnownResponseHeaders.Count, "#B1");

			known = (KnownResponseHeader)f.KnownResponseHeaders ["Content-Type"];
			Assert.AreEqual (HttpWorkerRequest.HeaderContentType, known.Index, "#B2");
			Assert.AreEqual ("text/plain; charset=big5", known.Value, "#B3");
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

			Assert.AreEqual (3, f.KnownResponseHeaders.Count, "#B1");
			
			known = (KnownResponseHeader) f.KnownResponseHeaders ["Transfer-Encoding"];
			Assert.AreEqual (HttpWorkerRequest.HeaderTransferEncoding, known.Index, "#B2");
			Assert.AreEqual ("chunked", known.Value, "#B3");
			
			known = (KnownResponseHeader) f.KnownResponseHeaders ["Cache-Control"];
			Assert.AreEqual (HttpWorkerRequest.HeaderCacheControl, known.Index, "#B4");
			Assert.AreEqual ("private", known.Value, "#B5");
			
			known = (KnownResponseHeader) f.KnownResponseHeaders ["Content-Type"];
			Assert.AreEqual (HttpWorkerRequest.HeaderContentType, known.Index, "#B6");
			Assert.AreEqual ("text/html", known.Value, "#B7");

			Assert.AreEqual (1, f.UnknownResponseHeaders.Count, "#C1");
			UnknownResponseHeader unknown = (UnknownResponseHeader) f.UnknownResponseHeaders ["X-AspNet-Version"];
			Assert.AreEqual ("X-AspNet-Version", unknown.Name, "#C2");
			Assert.AreEqual (Environment.Version.ToString (3), unknown.Value, "#C3");
		}

		[Test]
		public void Constructor ()
		{
			var resp = new HttpResponse (null);
			Assert.IsNull (resp.Output, "#A1");
		}
		[Test]
		public void RedirectPermanent ()
		{
			FakeHttpWorkerRequest2 request;
			HttpContext context = Cook (1, out request);
			Assert.Throws<ArgumentNullException> (() => {
				context.Response.RedirectPermanent (null);
			}, "#A1");

			Assert.Throws<ArgumentException> (() => {
				context.Response.RedirectPermanent ("http://invalid\nurl.com");
			}, "#A2");

			Assert.Throws<ArgumentNullException> (() => {
				context.Response.RedirectPermanent (null, true);
			}, "#A3");

			Assert.Throws<ArgumentException> (() => {
				context.Response.RedirectPermanent ("http://invalid\nurl.com", true);
			}, "#A4");
		}

		[Test]
		public void RedirectToRoute ()
		{
			var resp = new HttpResponse (new StringWriter ());
			// Ho, ho, ho!
			Assert.Throws<NullReferenceException> (() => {
				resp.RedirectToRoute ("SomeRoute");
			}, "#A1");

			FakeHttpWorkerRequest2 request;
			HttpContext context = Cook (1, out request);

			// From RouteCollection.GetVirtualPath
			Assert.Throws<ArgumentException> (() => {
				context.Response.RedirectToRoute ("SomeRoute");
			}, "#A2");

			Assert.Throws<InvalidOperationException> (() => {
				context.Response.RedirectToRoute (new { productId = "1", category = "widgets" });
			}, "#A3");
		}

		[Test]
		public void RemoveOutputCacheItem ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				HttpResponse.RemoveOutputCacheItem (null, "MyProvider");
			}, "#A1");

			Assert.Throws<ArgumentException> (() => {
				HttpResponse.RemoveOutputCacheItem ("badPath", null);
			}, "#A2");

			Assert.IsNull (OutputCache.Providers, "#A3");
			HttpResponse.RemoveOutputCacheItem ("/Path", null);

			Assert.Throws<ProviderException> (() => {
				HttpResponse.RemoveOutputCacheItem ("/Path", String.Empty);
			}, "#A3");

			Assert.Throws<ProviderException> (() => {
				HttpResponse.RemoveOutputCacheItem ("/Path", "MyProvider");
			}, "#A4");
		}

		[Test]
		public void OutputSetter ()
		{
			FakeHttpWorkerRequest2 request;
			HttpContext context = Cook (1, out request);

			Assert.IsNotNull (context.Response.Output, "#A1");
			context.Response.Output = null;
			Assert.IsNull (context.Response.Output, "#A2");

			// Classy...
			Assert.Throws<NullReferenceException> (() => {
				context.Response.Write ('t');
			}, "#A3-1");

			Assert.Throws<NullReferenceException> (() => {
				context.Response.Write ((object) 5);
			}, "#A3-2");

			Assert.Throws<NullReferenceException> (() => {
				context.Response.Write ("string");
			}, "#A3-3");

			Assert.Throws<NullReferenceException> (() => {
				context.Response.Write (new char [] { '1' }, 0, 1);
			}, "#A3-4");

			Assert.Throws<NullReferenceException> (() => {
				context.Response.Write ((object) null);
			}, "#A3-5");
		}
	}

	[TestFixture]
	public class HttpResponseOutputStreamTest
	{
		FakeHttpWorkerRequest2 worker;
		HttpContext context;
		HttpResponse response;
		Stream out_stream;

		[SetUp]
		public void Setup ()
		{
			context = Cook (2, out worker);
			response = context.Response;
			out_stream = response.OutputStream;
		}

		[TearDown]
		public void TearDown ()
		{
			if (response != null)
				response.Close ();
		}

		[Test]
		public void CanRead ()
		{
			Assert.IsFalse (out_stream.CanRead, "#1");
			out_stream.Close ();
			Assert.IsFalse (out_stream.CanRead, "#2");
		}

		[Test]
		public void CanSeek ()
		{
			Assert.IsFalse (out_stream.CanSeek, "#1");
			out_stream.Close ();
			Assert.IsFalse (out_stream.CanSeek, "#2");
		}

		[Test]
		public void CanWrite ()
		{
			Assert.IsTrue (out_stream.CanWrite, "#1");
			out_stream.Close ();
			Assert.IsTrue (out_stream.CanWrite, "#2");
		}

		[Test]
		public void Flush ()
		{
			byte [] buffer = Encoding.UTF8.GetBytes ("mono");
			out_stream.Write (buffer, 0, buffer.Length);
			out_stream.Flush ();
			Assert.AreEqual (0, worker.data_len);
		}

		[Test]
		public void Length ()
		{
			try {
				long len = out_stream.Length;
				Assert.Fail ("#1:" + len);
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Position ()
		{
			try {
				long pos = out_stream.Position;
				Assert.Fail ("#A1:" + pos);
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			try {
				out_stream.Position = 0;
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void Read ()
		{
			byte [] buffer = new byte [5];

			try {
				out_stream.Read (buffer, 0, buffer.Length);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Seek ()
		{
			try {
				out_stream.Seek (5, SeekOrigin.Begin);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void SetLength ()
		{
			try {
				out_stream.SetLength (5L);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void Write ()
		{
			byte [] buffer;

			buffer = Encoding.UTF8.GetBytes ("mono");
			out_stream.Write (buffer, 0, buffer.Length);
			buffer = Encoding.UTF8.GetBytes ("just rocks!!");
			out_stream.Write (buffer, 5, 6);
			out_stream.Write (buffer, 0, 4);
			Assert.IsFalse (worker.OutputProduced, "#1");
			response.Flush ();
			Assert.IsTrue (worker.OutputProduced, "#2");

			string output = Encoding.UTF8.GetString (worker.data);
			Assert.AreEqual ("e\r\nmonorocks!just\r\n", output);
		}

		[Test]
		public void Write_Buffer_Null ()
		{
			try {
				out_stream.Write ((byte []) null, 0, 0);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Write_Count_Negative ()
		{
			byte [] buffer = new byte [] { 0x0a, 0x1f, 0x2d };

			// offset < 0
			try {
				out_stream.Write (buffer, 1, -1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("count", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Write_Count_Overflow ()
		{
			byte [] buffer;

			buffer = Encoding.UTF8.GetBytes ("Mono");
			out_stream.Write (buffer, 0, buffer.Length + 5);
			buffer = Encoding.UTF8.GetBytes ("Just Rocks!!");
			out_stream.Write (buffer, 5, buffer.Length - 2);
			response.Flush ();

			string output = Encoding.UTF8.GetString (worker.data);
			Assert.AreEqual ("b\r\nMonoRocks!!\r\n", output);
		}

		[Test]
		public void Write_Offset_Negative ()
		{
			byte [] buffer = new byte [] { 0x0a, 0x1f, 0x2d };

			// offset < 0
			try {
				out_stream.Write (buffer, -1, 0);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("offset", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Write_Offset_Overflow ()
		{
			byte [] buffer = new byte [] { 0x0a, 0x1f, 0x2d };

			// offset == buffer length
			try {
				out_stream.Write (buffer, buffer.Length, 0);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("offset", ex.ParamName, "#A5");
			}

			// offset > buffer length
			try {
				out_stream.Write (buffer, buffer.Length + 1, 0);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("offset", ex.ParamName, "#B5");
			}

			response.Flush ();
			Assert.AreEqual (0, worker.data_len);
		}

		[Test]
		public void Write_Stream_Closed ()
		{
			byte [] buffer = Encoding.UTF8.GetBytes ("mono");
			out_stream.Close ();
			out_stream.Write (buffer, 0, buffer.Length);
			response.Flush ();

			string output = Encoding.UTF8.GetString (worker.data);
			Assert.AreEqual ("4\r\nmono\r\n", output);
		}

		HttpContext Cook (int re, out FakeHttpWorkerRequest2 f)
		{
			f = new FakeHttpWorkerRequest2 (re);
			return new HttpContext (f);
		}
	}
}
