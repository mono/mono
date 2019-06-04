//
// HttpResponseMessageTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Linq;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpResponseMessageTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new HttpResponseMessage ((HttpStatusCode) (-1));
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Ctor_Default ()
		{
			var m = new HttpResponseMessage ();
			Assert.IsNull (m.Content, "#1");
			Assert.IsNotNull (m.Headers, "#2");
			Assert.IsTrue (m.IsSuccessStatusCode, "#3");
			Assert.AreEqual ("OK", m.ReasonPhrase, "#4");
			Assert.IsNull (m.RequestMessage, "#5");
			Assert.AreEqual (HttpStatusCode.OK, m.StatusCode, "#6");
			Assert.AreEqual (new Version (1, 1), m.Version, "#7");
			Assert.IsNull (m.Headers.CacheControl, "#8");

			Assert.AreEqual ("StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: <null>, Headers:\r\n{\r\n}", m.ToString (), "#9");
		}

		[Test]
		public void Headers ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;

			headers.AcceptRanges.Add ("ac-v");
			headers.Age = TimeSpan.MaxValue;
			headers.CacheControl = new CacheControlHeaderValue () {
				MaxStale = true
			};
			headers.Connection.Add ("test-value");
			headers.ConnectionClose = true;
			headers.Date = new DateTimeOffset (DateTime.Today);
			headers.ETag = new EntityTagHeaderValue ("\"tag\"", true);
			headers.Location = new Uri ("http://xamarin.com");
			headers.Pragma.Add (new NameValueHeaderValue ("name", "value"));
			headers.ProxyAuthenticate.Add (new AuthenticationHeaderValue ("proxy", "par"));
			headers.RetryAfter = new RetryConditionHeaderValue (TimeSpan.MinValue);
			headers.Server.Add (new ProductInfoHeaderValue ("(comment)"));
			headers.Trailer.Add ("trailer-vvv");
			headers.TransferEncoding.Add (new TransferCodingHeaderValue ("tchv"));
			headers.TransferEncodingChunked = true;
			headers.Upgrade.Add (new ProductHeaderValue ("prod", "ver"));
			headers.Vary.Add ("vary");
			headers.Via.Add (new ViaHeaderValue ("protocol", "rec-by"));
			headers.Warning.Add (new WarningHeaderValue (5, "agent", "\"txt\""));
			headers.WwwAuthenticate.Add (new AuthenticationHeaderValue ("www", "par"));

			try {
				headers.Add ("age", "");
				Assert.Fail ("age");
			} catch (FormatException) {
			}

			try {
				headers.Add ("date", "");
				Assert.Fail ("date");
			} catch (FormatException) {
			}

			try {
				headers.Add ("etag", "");
				Assert.Fail ("etag");
			} catch (FormatException) {
			}

			try {
				headers.Add ("location", "extra");
				Assert.Fail ("location");
			} catch (FormatException) {
			}

			try {
				headers.Add ("retry-after", "extra");
				Assert.Fail ("retry-after");
			} catch (FormatException) {
			}

			headers.Add ("accept-ranges", "achs");
// TODO:			headers.Add ("cache-control", "cache-value");
			headers.Add ("connection", "ccc");
			headers.Add ("pragma", "p");
			headers.Add ("proxy-authenticate", "ttt");
			headers.Add ("server", "server");
			headers.Add ("trailer", "tt-r");
			headers.Add ("transfer-encoding", "ttt");
			headers.Add ("upgrade", "uuu");
			headers.Add ("upgrade", "vvvvaa");
			headers.Add ("vary", "vvaar");
			headers.Add ("via", "prot v");
			headers.Add ("warning", "4 ww \"t\"");
			headers.Add ("www-Authenticate", "ww");

			Assert.IsTrue (headers.AcceptRanges.SequenceEqual (
				new[] {
					"ac-v",
					"achs"
				}
			));

			Assert.AreEqual (TimeSpan.MaxValue, headers.Age);

			Assert.AreEqual (
				new CacheControlHeaderValue () {
						MaxStale = true,
// TODO						Extensions = { new NameValueHeaderValue ("cache-value") }
					}, headers.CacheControl);

			Assert.IsTrue (headers.Connection.SequenceEqual (
				new string[] { "test-value", "close", "ccc" }));

			Assert.AreEqual (new DateTimeOffset (DateTime.Today), headers.Date);

			Assert.AreEqual (new EntityTagHeaderValue ("\"tag\"", true), headers.ETag);

			Assert.AreEqual (new Uri ("http://xamarin.com"), headers.Location);

			Assert.IsTrue (headers.Pragma.SequenceEqual (
				new [] {
					new NameValueHeaderValue ("name", "value"),
					new NameValueHeaderValue ("p"),
				}));

			Assert.IsTrue (headers.ProxyAuthenticate.SequenceEqual (
				new [] {
					new AuthenticationHeaderValue ("proxy", "par"),
					new AuthenticationHeaderValue ("ttt")
				}
			));

			Assert.AreEqual (new RetryConditionHeaderValue (TimeSpan.MinValue), headers.RetryAfter);

			Assert.IsTrue (headers.Server.SequenceEqual (
				new [] {
					new ProductInfoHeaderValue ("(comment)"),
					new ProductInfoHeaderValue (new ProductHeaderValue ("server"))
				}
			));

			Assert.IsTrue (headers.Trailer.SequenceEqual (
				new [] {
					"trailer-vvv",
					"tt-r"
				}));

			Assert.IsTrue (headers.TransferEncoding.SequenceEqual (
				new[] {
					new TransferCodingHeaderValue ("tchv"),
					new TransferCodingHeaderValue ("chunked"),
					new TransferCodingHeaderValue ("ttt")
				}
			));

			Assert.IsTrue (headers.Upgrade.SequenceEqual (
				new[] {
					new ProductHeaderValue ("prod", "ver"),
					new ProductHeaderValue ("uuu"),
					new ProductHeaderValue ("vvvvaa")
				}
			));

			Assert.IsTrue (headers.Vary.SequenceEqual (
				new[] {
					"vary",
					"vvaar"
				}
			));

			Assert.IsTrue (headers.Via.SequenceEqual (
				new[] {
					new ViaHeaderValue ("protocol", "rec-by"),
					new ViaHeaderValue ("prot", "v")
				}
			));

			Assert.IsTrue (headers.Warning.SequenceEqual (
				new[] {
					new WarningHeaderValue (5, "agent", "\"txt\""),
					new WarningHeaderValue (4, "ww", "\"t\"")
				}
			));

			Assert.IsTrue (headers.WwwAuthenticate.SequenceEqual (
				new[] {
					new AuthenticationHeaderValue ("www", "par"),
					new AuthenticationHeaderValue ("ww")
				}
			));
		}

		[Test]
		public void EnsureSuccessStatusCode ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			Assert.AreSame (message, message.EnsureSuccessStatusCode (), "#1");

			message = new HttpResponseMessage (HttpStatusCode.BadRequest);
			message.ReasonPhrase = "test reason";
			try {
				message.EnsureSuccessStatusCode ();
				Assert.Fail ("#2");
			} catch (HttpRequestException e) {
				Assert.IsTrue (e.Message.Contains ("400 (test reason)"), "#3");
			}
		}

		[Test]
		public void Headers_GetEnumerator ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;

			headers.Add ("a", new[] { "v1", "v2" });
			headers.Add ("cache-control", "audio");
			headers.Age = new TimeSpan (4444, 2, 3, 4, 5);

			int i = 0;
			List<string> values;
			foreach (var entry in headers) {
				switch (i) {
				case 0:
					Assert.AreEqual ("a", entry.Key);
					values = entry.Value.ToList ();
					Assert.AreEqual (2, values.Count);
					Assert.AreEqual ("v1", values[0]);
					break;
				case 1:
					if (HttpClientTestHelpers.UsingSocketsHandler)
						Assert.AreEqual ("Cache-Control", entry.Key);
					else
						Assert.AreEqual ("cache-control", entry.Key);
					values = entry.Value.ToList ();
					Assert.AreEqual (1, values.Count);
					Assert.AreEqual ("audio", values[0]);
					break;
				case 2:
					Assert.AreEqual ("Age", entry.Key);
					values = entry.Value.ToList ();
					Assert.AreEqual (1, values.Count);
					Assert.AreEqual ("383968984", values[0]);
					break;
				}

				++i;
			}

			Assert.AreEqual (3, i, "#10");
		}

		[Test]
		public void Headers_MultiValues ()
		{
			var message = new HttpResponseMessage ();
			var headers = message.Headers;

			headers.Add ("Proxy-Authenticate", "x, y, z,i");
			headers.Add ("Upgrade", "HTTP/2.0, SHTTP/1.3, IRC, RTA/x11");
			headers.Add ("Via", "1.0 fred, 1.1 nowhere.com (Apache/1.1)");
			headers.Add ("Warning", "199 Miscellaneous \"w\", 200 a \"b\"");

			Assert.AreEqual (4, headers.ProxyAuthenticate.Count, "#1a");
			Assert.IsTrue (headers.ProxyAuthenticate.SequenceEqual (
				new[] {
					new AuthenticationHeaderValue ("x"),

					new AuthenticationHeaderValue ("y"),
					new AuthenticationHeaderValue ("z"),
					new AuthenticationHeaderValue ("i")
				}
			), "#1b");

			
			Assert.AreEqual (4, headers.Upgrade.Count, "#2a");
			Assert.IsTrue (headers.Upgrade.SequenceEqual (
				new[] {
					new ProductHeaderValue ("HTTP", "2.0"),
					new ProductHeaderValue ("SHTTP", "1.3"),
					new ProductHeaderValue ("IRC"),
					new ProductHeaderValue ("RTA", "x11")
				}
			), "#2b");

			Assert.AreEqual (2, headers.Via.Count, "#3a");
			Assert.IsTrue (headers.Via.SequenceEqual (
				new[] {
					new ViaHeaderValue ("1.0", "fred"),
					new ViaHeaderValue ("1.1", "nowhere.com", null, "(Apache/1.1)")
				}
			), "#2b");

			Assert.AreEqual (2, headers.Warning.Count, "#4a");
			Assert.IsTrue (headers.Warning.SequenceEqual (
				new[] {
					new WarningHeaderValue (199, "Miscellaneous", "\"w\""),
					new WarningHeaderValue (200, "a", "\"b\"")
				}
			), "#4b");
		}

		[Test]
		public void Header_BaseImplementation ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;

			headers.Add ("a", "a-value");
			headers.Add ("b", new List<string> { "v1", "v2" });
			headers.Add ("c", null as string);
			headers.Add ("d", new string[0]);

			headers.TryAddWithoutValidation ("cache-control", "audio");

			Assert.IsFalse (headers.Contains ("nn"), "#1a");
			Assert.IsTrue (headers.Contains ("b"), "#1b");

			var values = headers.GetValues ("b").ToList ();
			Assert.AreEqual ("v1", values[0], "#2a");
			Assert.AreEqual ("v2", values[1], "#2b");

			Assert.IsFalse (headers.Remove ("p"), "#3a");
			Assert.IsTrue (headers.Remove ("b"), "#3b");
			Assert.IsFalse (headers.Contains ("b"), "#3b-c");

			IEnumerable<string> values2;
			Assert.IsTrue (headers.TryGetValues ("c", out values2));
			values = values2.ToList ();
			Assert.AreEqual ("", values[0], "#4a");

			int counter = 0;
			foreach (var i in headers) {
				++counter;
			}

			Assert.AreEqual (3, counter, "#5");

			headers.Clear ();
		}

		[Test]
		public void Headers_Invalid ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;

			try {
				headers.Add ("age", "");
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				headers.Add (null, "");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				headers.Add ("mm", null as IEnumerable<string>);
				Assert.Fail ("#2b");
			} catch (ArgumentNullException) {
			}

			try {
				headers.Add ("Allow", "audio");
				Assert.Fail ("#2c");
			} catch (InvalidOperationException) {
			}

			Assert.IsFalse (headers.TryAddWithoutValidation ("Allow", ""), "#3");

			Assert.IsFalse (headers.TryAddWithoutValidation (null, ""), "#4");

			try {
				headers.Contains (null);
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}

			try {
				headers.GetValues (null);
				Assert.Fail ("#6a");
			} catch (ArgumentException) {
			}

			try {
				headers.GetValues ("bbbb");
				Assert.Fail ("#6b");
			} catch (InvalidOperationException) {
			}

			try {
				headers.Add ("location", new[] { "example.com", "example.org" });
				Assert.Fail ("#7a");
			} catch (FormatException) {
			}

			headers.TryAddWithoutValidation ("location", "a@a.com");
			try {
				headers.Add ("location", "w3.org");
				Assert.Fail ("#7b");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Headers_Request ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;

			headers.Add ("accept", "audio");
			Assert.AreEqual ("audio", headers.GetValues ("Accept").First (), "#1");

			headers.Clear ();
			Assert.IsTrue (headers.TryAddWithoutValidation ("accept", "audio"), "#2a");
			Assert.AreEqual ("audio", headers.GetValues ("Accept").First (), "#2");
		}

		[Test]
		public void Headers_ConnectionClose ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;
			Assert.IsNull (headers.ConnectionClose, "#1");

			headers.ConnectionClose = false;
			Assert.IsFalse (headers.ConnectionClose.Value, "#2");

			headers.Clear ();

			headers.ConnectionClose = true;
			Assert.IsTrue (headers.ConnectionClose.Value, "#3");

			headers.Clear ();
			headers.Connection.Add ("Close");
			Assert.IsTrue (headers.ConnectionClose.Value, "#4");

			// .NET encloses the "Connection: Close" with two whitespaces.
			var normalized = Regex.Replace (message.ToString (), @"\s", "");
			Assert.AreEqual ("StatusCode:200,ReasonPhrase:'OK',Version:1.1,Content:<null>,Headers:{Connection:Close}", normalized, "#5");
		}

		[Test]
		public void Headers_Location ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;
			headers.TryAddWithoutValidation ("location", "http://w3.org");
			Assert.AreEqual (new Uri ("http://w3.org"), headers.Location);
		}

		[Test]
		public void Headers_TransferEncoding ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;
			headers.TryAddWithoutValidation ("Transfer-Encoding", "mmm");
			headers.TryAddWithoutValidation ("Transfer-Encoding", "▀");
			headers.TryAddWithoutValidation ("Transfer-Encoding", "zz");

			var a = headers.TransferEncoding;
			Assert.AreEqual (2, a.Count, "#1");

			// Assert.AreEqual ("mmm, zz, ▀", a.ToString (), "#2");
		}

		[Test]
		public void Headers_TransferEncodingChunked ()
		{
			HttpResponseMessage message = new HttpResponseMessage ();
			HttpResponseHeaders headers = message.Headers;
			Assert.IsNull (headers.TransferEncodingChunked, "#1");

			headers.TransferEncodingChunked = false;
			Assert.IsFalse (headers.TransferEncodingChunked.Value, "#2");

			headers.Clear ();

			headers.TransferEncodingChunked = true;
			Assert.IsTrue (headers.TransferEncodingChunked.Value, "#3");
			Assert.AreEqual (1, headers.TransferEncoding.Count, "#3b");

			headers.Clear ();
			Assert.IsTrue (headers.TryAddWithoutValidation ("Transfer-Encoding", "value"), "#4-0");
//			Assert.AreEqual (false, headers.TransferEncodingChunked, "#4");

			headers.Clear ();
			Assert.IsTrue (headers.TryAddWithoutValidation ("Transfer-Encoding", "chunked"), "#5-0");
			Assert.AreEqual (true, headers.TransferEncodingChunked, "#5");

			message = new HttpResponseMessage ();
			headers = message.Headers;
			Assert.IsTrue (headers.TryAddWithoutValidation ("Transfer-Encoding", "ß"), "#6-0");
			Assert.IsNull (headers.TransferEncodingChunked, "#6");
		}

		[Test]
		public void Properties ()
		{
			var c = new HttpResponseMessage ();
			c.Content = null;
			c.ReasonPhrase = "rphr";
			c.RequestMessage = new HttpRequestMessage ();
			c.RequestMessage.Properties.Add ("a", "test");
			c.StatusCode = HttpStatusCode.UnsupportedMediaType;
			c.Version = HttpVersion.Version10;

			Assert.IsNull (c.Content, "#1");
			Assert.AreEqual ("rphr", c.ReasonPhrase, "#2");
			Assert.AreEqual ("test", c.RequestMessage.Properties["a"], "#3");
			Assert.AreEqual (HttpStatusCode.UnsupportedMediaType, c.StatusCode, "#4");
			Assert.AreEqual (HttpVersion.Version10, c.Version, "#5");
			Assert.IsFalse (c.IsSuccessStatusCode, "#6");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var c = new HttpResponseMessage ();

			try {
				c.StatusCode = (HttpStatusCode) (-1);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				c.Version = null;
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}
		}
	}
}
