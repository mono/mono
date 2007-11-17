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
		public int return_kind;
		
		public FakeHttpWorkerRequest2 (int re)
		{
			return_kind = re;
		}
		
		public override string GetUriPath()
		{
			return "GetUriPath";
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

		bool headers_sent;
		public override void SendKnownResponseHeader(int x, string j)
		{
			headers_sent = true;
		}
		
		public override void SendUnknownResponseHeader(string a, string b)
		{
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

			//Console.WriteLine (Environment.StackTrace);
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
		
	[TestFixture]
	public class HttpResponseTest {

		// NOTE : This test cannot be runned on .net with no web context ....
		// ONLY MONO TESTS

		HttpContext Cook (int re, out FakeHttpWorkerRequest2 f)
		{
			f = new FakeHttpWorkerRequest2 (re);
			HttpContext c = new HttpContext (f);

			return c;
		}

#if TARGET_JVM
		[Category ("NotWorking")] // char output stream in gh make this test fail
#endif
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
		[Test] public void Test_Response ()
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
		[Category ("NotDotNet")]  //This test cannot be runned on .net with no web context ....
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
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
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
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
		[ExpectedException (typeof (HttpException))]
		public void Status2 ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.Status = "200";
		}

		[Test]
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
		[ExpectedException (typeof (HttpException))]
		public void Status3 ()
		{
			FakeHttpWorkerRequest2 f;
			HttpContext c = Cook (2, out f);

			HttpResponse resp = c.Response;
			resp.Status = "200\t";
		}

		[Test]
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
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
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
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
		[Category ("NotDotNet")] //This test cannot be runned on .net with no web context ....
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
		[Category ("NotDotNet")]
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
		
		//[Test][ExpectedException (typeof (HttpException))]
	}
}
