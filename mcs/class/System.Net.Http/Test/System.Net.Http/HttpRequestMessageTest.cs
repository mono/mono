//
// HttpRequestMessageTest.cs
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
using System.IO;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpRequestMessageTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new HttpRequestMessage (null, "");
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Ctor_Default ()
		{
			var m = new HttpRequestMessage ();
			Assert.IsNull (m.Content, "#1");
			Assert.IsNotNull (m.Headers, "#2");
			Assert.AreEqual (HttpMethod.Get, m.Method, "#3");
			Assert.IsNotNull (m.Properties, "#4");
			Assert.IsNull (m.RequestUri, "#5");
			Assert.AreEqual (new Version (1, 1), m.Version, "#6");

			Assert.AreEqual ("Method: GET, RequestUri: '<null>', Version: 1.1, Content: <null>, Headers:\r\n{\r\n}", m.ToString (), "#7");
		}

		[Test]
		public void Ctor_Uri ()
		{
			var c = new HttpRequestMessage (HttpMethod.Get, new Uri ("http://xamarin.com"));
			Assert.AreEqual ("http://xamarin.com/", c.RequestUri.AbsoluteUri, "#1");

			c = new HttpRequestMessage (HttpMethod.Get, new Uri ("https://xamarin.com"));
			Assert.AreEqual ("https://xamarin.com/", c.RequestUri.AbsoluteUri, "#2");

			c = new HttpRequestMessage (HttpMethod.Get, new Uri ("HTTP://xamarin.com:8080"));
			Assert.AreEqual ("http://xamarin.com:8080/", c.RequestUri.AbsoluteUri, "#3");

			var a = Uri.UriSchemeHttps;
			var b = new Uri ("http://xamarin.com");

			try {
				new HttpRequestMessage (HttpMethod.Get, new Uri ("ftp://xamarin.com"));
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Ctor_RelativeUri ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var uri = new Uri ("Computer", UriKind.Relative);
			var req = new HttpRequestMessage (HttpMethod.Get, uri);
			// HttpRequestMessage does not rewrite it here.
			Assert.AreEqual (req.RequestUri, uri);
		}

		[Test]
		public void Ctor_RelativeOrAbsoluteUri ()
		{
			var uri = new Uri ("/", UriKind.RelativeOrAbsolute);
			new HttpRequestMessage (HttpMethod.Get, uri);

			uri = new Uri ("file://", UriKind.RelativeOrAbsolute);
			try {
				new HttpRequestMessage (HttpMethod.Get, uri);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Ctor_RelativeUriString ()
		{
			var client = new HttpClient ();
			client.BaseAddress = new Uri ("http://en.wikipedia.org/wiki/");
			var req = new HttpRequestMessage (HttpMethod.Get, "Computer");
			// HttpRequestMessage does not rewrite it here.
			Assert.IsFalse (req.RequestUri.IsAbsoluteUri);
		}

		[Test]
		public void Ctor_RelativeOrAbsoluteUriString ()
		{
			new HttpRequestMessage (HttpMethod.Get, "/");

			try {
				new HttpRequestMessage (HttpMethod.Get, "file://");
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Headers ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;

			headers.Accept.Add (new MediaTypeWithQualityHeaderValue ("audio/x"));
			headers.AcceptCharset.Add (new StringWithQualityHeaderValue ("str-v", 0.002));
			headers.AcceptEncoding.Add (new StringWithQualityHeaderValue ("str-enc", 0.44));
			headers.AcceptLanguage.Add (new StringWithQualityHeaderValue ("str-lang", 0.41));
			headers.Authorization = new AuthenticationHeaderValue ("sh-aut", "par");
			headers.CacheControl = new CacheControlHeaderValue () {
				MaxAge = TimeSpan.MaxValue
			};
			headers.Connection.Add ("test-value");
			headers.ConnectionClose = true;
			headers.Date = new DateTimeOffset (DateTime.Today);
			headers.Expect.Add (new NameValueWithParametersHeaderValue ("en", "ev"));
			headers.ExpectContinue = true;
			headers.From = "webmaster@w3.org";
			headers.Host = "host";
			headers.IfMatch.Add (new EntityTagHeaderValue ("\"tag\"", true));
			headers.IfModifiedSince = new DateTimeOffset (DateTime.Today);
			headers.IfNoneMatch.Add (new EntityTagHeaderValue ("\"tag2\"", true));
			headers.IfRange = new RangeConditionHeaderValue (new DateTimeOffset (DateTime.Today));
			headers.IfUnmodifiedSince = new DateTimeOffset? (DateTimeOffset.Now);
			headers.MaxForwards = 0x15b3;
			headers.Pragma.Add (new NameValueHeaderValue ("name", "value"));
			headers.ProxyAuthorization = new AuthenticationHeaderValue ("s", "p");
			headers.Range = new RangeHeaderValue (5L, 30L);
			headers.Referrer = new Uri ("http://xamarin.com");
			headers.TE.Add (new TransferCodingWithQualityHeaderValue ("TE", 0.3));
			headers.Trailer.Add ("value");
			headers.TransferEncoding.Add (new TransferCodingHeaderValue ("tchv"));
			headers.TransferEncodingChunked = true;
			headers.Upgrade.Add (new ProductHeaderValue ("prod", "ver"));
			headers.UserAgent.Add (new ProductInfoHeaderValue ("(comment)"));
			headers.Via.Add (new ViaHeaderValue ("protocol", "rec-by"));
			headers.Warning.Add (new WarningHeaderValue (5, "agent", "\"txt\""));

			try {
				headers.Add ("authorization", "");
				Assert.Fail ("Authorization");
			} catch (FormatException) {
			}

			try {
				headers.Add ("connection", "extra ß ");
				Assert.Fail ("Date");
			} catch (FormatException) {
			}

			try {
				headers.Add ("date", "");
				Assert.Fail ("Date");
			} catch (FormatException) {
			}

			try {
				headers.Add ("from", "a@w3.org");
				Assert.Fail ("From");
			} catch (FormatException) {
			}

			try {
				headers.Add ("hOst", "host");
				Assert.Fail ("Host");
			} catch (FormatException) {
			}

			try {
				headers.Add ("if-modified-since", "");
				Assert.Fail ("if-modified-since");
			} catch (FormatException) {
			}

			try {
				headers.Add ("if-range", "");
				Assert.Fail ("if-range");
			} catch (FormatException) {
			}

			try {
				headers.Add ("if-unmodified-since", "");
				Assert.Fail ("if-unmodified-since");
			} catch (FormatException) {
			}

			try {
				headers.Add ("max-forwards", "");
				Assert.Fail ("max-forwards");
			} catch (FormatException) {
			}

			try {
				headers.Add ("proxy-authorization", "");
				Assert.Fail ("proxy-authorization");
			} catch (FormatException) {
			}

			try {
				headers.Add ("range", "");
			} catch (FormatException) {
			}

			try {
				headers.Add ("referer", "");
				Assert.Fail ("referer");
			} catch (FormatException) {
			}

			try {
				headers.Add ("pragma", "nocache,RequestID=1,g=");
 				Assert.Fail ("pragma");
			} catch (FormatException) {				
			}

			headers.Add ("accept", "audio/y");
			headers.Add ("accept-charset", "achs");
			headers.Add ("accept-encoding", "aenc");
			headers.Add ("accept-language", "alan");
			headers.Add ("expect", "exp");
			headers.Add ("if-match", "\"v\"");
			headers.Add ("if-none-match", "\"v2\"");
			headers.Add ("TE", "0.8");
			headers.Add ("trailer", "value2");
			headers.Add ("transfer-encoding", "ttt");
			headers.Add ("upgrade", "uuu");
			headers.Add ("user-agent", "uaua");
			headers.Add ("via", "prot v");
			headers.Add ("warning", "4 ww \"t\"");
			headers.Add ("pragma", "nocache,R=1,g");

			Assert.IsTrue (headers.Accept.SequenceEqual (
				new[] {
					new MediaTypeWithQualityHeaderValue ("audio/x"),
					new MediaTypeWithQualityHeaderValue ("audio/y")
				}
			));

			Assert.IsTrue (headers.AcceptCharset.SequenceEqual (
				new[] {
					new StringWithQualityHeaderValue ("str-v", 0.002),
					new StringWithQualityHeaderValue ("achs")
				}
			));

			Assert.IsTrue (headers.AcceptEncoding.SequenceEqual (
				new[] {
					new StringWithQualityHeaderValue ("str-enc", 0.44),
					new StringWithQualityHeaderValue ("aenc")
				}
			));

			Assert.IsTrue (headers.AcceptLanguage.SequenceEqual (
				new[] {
					new StringWithQualityHeaderValue ("str-lang", 0.41),
					new StringWithQualityHeaderValue ("alan")
				}
			));

			Assert.AreEqual (new AuthenticationHeaderValue ("sh-aut", "par"), headers.Authorization);

			var cch = new CacheControlHeaderValue () {
					MaxAge = TimeSpan.MaxValue,
				};

			Assert.AreEqual (cch, headers.CacheControl);

			Assert.IsTrue (headers.Connection.SequenceEqual (
				new string[] { "test-value", "close" }));

			Assert.AreEqual (headers.Date, new DateTimeOffset (DateTime.Today));

			Assert.IsTrue (headers.Expect.SequenceEqual (
				new [] {
					new NameValueWithParametersHeaderValue ("en", "ev"),
					new NameValueWithParametersHeaderValue ("100-continue"),
					new NameValueWithParametersHeaderValue ("exp")
				}));

			Assert.AreEqual (headers.From, "webmaster@w3.org");

			Assert.IsTrue (headers.IfMatch.SequenceEqual (
				new EntityTagHeaderValue[] {
					new EntityTagHeaderValue ("\"tag\"", true),
					new EntityTagHeaderValue ("\"v\"", false)
				}
			));

			Assert.AreEqual (headers.IfModifiedSince, new DateTimeOffset (DateTime.Today));
			Assert.IsTrue (headers.IfNoneMatch.SequenceEqual (new EntityTagHeaderValue[] { new EntityTagHeaderValue ("\"tag2\"", true), new EntityTagHeaderValue ("\"v2\"", false) }));
			Assert.AreEqual (new DateTimeOffset (DateTime.Today), headers.IfRange.Date);
			Assert.AreEqual (headers.MaxForwards, 0x15b3);
			Assert.AreEqual ("p", headers.ProxyAuthorization.Parameter);
			Assert.AreEqual ("s", headers.ProxyAuthorization.Scheme);
			Assert.AreEqual (5, headers.Range.Ranges.First ().From);
			Assert.AreEqual (30, headers.Range.Ranges.First ().To);
			Assert.AreEqual ("bytes", headers.Range.Unit);
			Assert.AreEqual (headers.Referrer, new Uri ("http://xamarin.com"));
			Assert.IsTrue (headers.TE.SequenceEqual (new TransferCodingWithQualityHeaderValue[] { new TransferCodingWithQualityHeaderValue ("TE", 0.3), new TransferCodingWithQualityHeaderValue ("0.8") }), "29");
			Assert.IsTrue (headers.Trailer.SequenceEqual (
				new string[] {
					"value", "value2"
				}), "30");

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
					new ProductHeaderValue ("uuu")
				}
			));

			Assert.IsTrue (headers.UserAgent.SequenceEqual (
				new[] {
					new ProductInfoHeaderValue ("(comment)"),
					new ProductInfoHeaderValue ("uaua", null)
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

			Assert.IsTrue (headers.Pragma.SequenceEqual (
				new[] {
					new NameValueHeaderValue ("name", "value"),
					new NameValueHeaderValue ("nocache", null),
					new NameValueHeaderValue ("R", "1"),
					new NameValueHeaderValue ("g", null)
				}
			));			
		}

		[Test]
		public void Headers_MultiValues ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;

			headers.Add ("Accept", "application/vnd.citrix.requesttokenresponse+xml, application/vnd.citrix.requesttokenchoices+xml");
			headers.Add ("Accept-Charset", "aa ;Q=0,bb;Q=1");
			headers.Add ("Expect", "x=1; v, y=5");
			headers.Add ("If-Match", "\"a\",*, \"b\",*");
			headers.Add ("user-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36");

			Assert.AreEqual (2, headers.Accept.Count, "#1a");
			Assert.IsTrue (headers.Accept.SequenceEqual (
				new[] {
					new MediaTypeWithQualityHeaderValue ("application/vnd.citrix.requesttokenresponse+xml"),
					new MediaTypeWithQualityHeaderValue ("application/vnd.citrix.requesttokenchoices+xml"),
				}
			), "#1b");

			Assert.AreEqual (2, headers.AcceptCharset.Count, "#2a");
			Assert.IsTrue (headers.AcceptCharset.SequenceEqual (
				new[] {
					new StringWithQualityHeaderValue ("aa", 0),
					new StringWithQualityHeaderValue ("bb", 1),
				}
			), "#2b");

			Assert.AreEqual (2, headers.Expect.Count, "#3a");
			var expect_expected = new[] {
					new NameValueWithParametersHeaderValue ("x", "1") {
					},
					new NameValueWithParametersHeaderValue ("y", "5"),
				};
			expect_expected [0].Parameters.Add (new NameValueHeaderValue ("v"));
			Assert.IsTrue (headers.Expect.SequenceEqual (
				expect_expected
			), "#3b");

			Assert.AreEqual (4, headers.IfMatch.Count, "#4a");
			Assert.IsTrue (headers.IfMatch.SequenceEqual (
				new[] {
					new EntityTagHeaderValue ("\"a\""),
					EntityTagHeaderValue.Any,
					new EntityTagHeaderValue ("\"b\""),
					EntityTagHeaderValue.Any
				}
			), "#4b");

			Assert.AreEqual (6, headers.UserAgent.Count, "#10a");

			Assert.IsTrue (headers.UserAgent.SequenceEqual (
				new[] {
					new ProductInfoHeaderValue ("Mozilla", "5.0"),
					new ProductInfoHeaderValue ("(Macintosh; Intel Mac OS X 10_8_4)"),
					new ProductInfoHeaderValue ("AppleWebKit", "537.36"),
					new ProductInfoHeaderValue ("(KHTML, like Gecko)"),
					new ProductInfoHeaderValue ("Chrome", "29.0.1547.62"),
					new ProductInfoHeaderValue ("Safari", "537.36")
				}
			), "#10b");
		}

		[Test]
		public void Headers_UserAgentExtra ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			var headers = message.Headers;

			headers.Add ("User-Agent", "MonoDevelop (Unix 3.13.0; amd64; en-US; Octokit 0.3.4)");

			var se = headers.UserAgent.SequenceEqual (
				new[] {
						new ProductInfoHeaderValue ("MonoDevelop", null),
						new ProductInfoHeaderValue ("(Unix 3.13.0; amd64; en-US; Octokit 0.3.4)")
				});
		}

		[Test]
		public void Header_BaseImplementation ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;

			headers.Add ("a", "a-value");
			headers.Add ("b", new List<string> { "v1", "v2" });
			headers.Add ("c", null as string);
			headers.Add ("d", new string[0]);

			Assert.IsTrue (headers.TryAddWithoutValidation ("accept", "audio"), "#0");

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

			headers.Accept.Add (new MediaTypeWithQualityHeaderValue ("audio/x"));
			Assert.IsTrue (headers.TryAddWithoutValidation ("accept", "audio"), "#55");

			values = headers.GetValues ("accept").ToList ();
			Assert.AreEqual (2, values.Count, "#6");
			Assert.AreEqual ("audio/x", values[0], "#6a");
			Assert.AreEqual ("audio", values[1], "#6b");
			Assert.AreEqual (1, headers.Accept.Count, "#6c");

			headers.Clear ();

			Assert.IsTrue (headers.TryAddWithoutValidation ("from", new[] { "a@a.com", "ssss@oo.com" }), "#70");
			values = headers.GetValues ("from").ToList ();

			Assert.AreEqual (2, values.Count, "#7");
			Assert.AreEqual ("a@a.com", values[0], "#7a");
			Assert.AreEqual ("ssss@oo.com", values[1], "#7b");
			Assert.AreEqual ("a@a.com", headers.From, "#7c");

			headers.Clear ();

			Assert.IsTrue (headers.TryAddWithoutValidation ("Date", "wrong date"), "#8-0");
			var value = headers.Date;
			Assert.IsNull (headers.Date, "#8");
		}

		[Test]
		public void Headers_Invalid ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;

			try {
				headers.Add ("Allow", "");
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
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
				headers.Add ("accept", "audio");
				Assert.Fail ("#2c");
			} catch (FormatException) {
			}

			Assert.IsFalse (headers.TryAddWithoutValidation ("Allow", ""), "#3"); ;

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
				headers.Add ("from", new[] { "a@a.com", "ssss@oo.com" });
				Assert.Fail ("#7a");
			} catch (FormatException) {
			}

			Assert.IsTrue (headers.TryAddWithoutValidation ("from", "a@a.com"), "#7-0");
			try {
				headers.Add ("from", "valid@w3.org");
				Assert.Fail ("#7b");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Headers_Response ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;

			headers.Add ("Age", "vv");
			Assert.AreEqual ("vv", headers.GetValues ("Age").First (), "#1");

			headers.Clear ();
			headers.TryAddWithoutValidation ("Age", "vv");
			Assert.AreEqual ("vv", headers.GetValues ("Age").First (), "#2");

			// .NET encloses the "Age: vv" with two whitespaces.
			var normalized = Regex.Replace (message.ToString (), @"\s", "");
			Assert.AreEqual ("Method:GET,RequestUri:'<null>',Version:1.1,Content:<null>,Headers:{Age:vv}", normalized, "#3");
		}

		[Test]
		public void Headers_ExpectContinue ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;
			Assert.IsNull (headers.ExpectContinue, "#1");

			headers.ExpectContinue = false;
			Assert.IsFalse (headers.ExpectContinue.Value, "#2");

			headers.Clear ();

			headers.ExpectContinue = true;
			headers.ExpectContinue = true;
			headers.ExpectContinue = true;
			headers.ExpectContinue = true;
			Assert.IsTrue (headers.ExpectContinue.Value, "#3");
			Assert.AreEqual (1, headers.GetValues ("expect").ToList ().Count, "#4");

			headers.Clear ();
			headers.Expect.Add (new NameValueWithParametersHeaderValue ("100-conTinuE"));
			Assert.IsTrue (headers.ExpectContinue.Value, "#5");
		}

		[Test]
		public void Headers_ConnectionClose ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;
			Assert.IsNull (headers.ConnectionClose, "#1");

			headers.ConnectionClose = false;
			Assert.IsFalse (headers.ConnectionClose.Value, "#2");

			headers.Clear ();

			headers.ConnectionClose = true;
			Assert.IsTrue (headers.ConnectionClose.Value, "#3");

			headers.Clear ();
			headers.Connection.Add ("Close");
			Assert.IsTrue (headers.ConnectionClose.Value, "#4");
		}

		[Test]
		public void Headers_From_Invalid ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;
			headers.From = null;
			headers.From = "";
			try {
				headers.From = " ";
				Assert.Fail ("#1");
			} catch (FormatException) {
			}
			try {
				headers.From = "test";
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Headers_TransferEncodingChunked ()
		{
			HttpRequestMessage message = new HttpRequestMessage ();
			HttpRequestHeaders headers = message.Headers;
			Assert.IsNull (headers.TransferEncodingChunked, "#1");

			headers.TransferEncodingChunked = false;
			Assert.IsFalse (headers.TransferEncodingChunked.Value, "#2");

			headers.Clear ();

			headers.TransferEncodingChunked = true;
			Assert.IsTrue (headers.TransferEncodingChunked.Value, "#3");
			Assert.AreEqual (1, headers.TransferEncoding.Count, "#3b");
		}

		[Test]
		public void Properties ()
		{
			var c = new HttpRequestMessage ();
			c.Content = null;
			c.Method = HttpMethod.Post;
			c.Properties.Add ("a", "test");
			c.RequestUri = null;
			c.Version = HttpVersion.Version10;

			Assert.IsNull (c.Content, "#1");
			Assert.AreEqual (HttpMethod.Post, c.Method, "#2");
			Assert.AreEqual ("test", c.Properties["a"], "#3");
			Assert.IsNull (c.RequestUri, "#4");
			Assert.AreEqual (HttpVersion.Version10, c.Version, "#5");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var c = new HttpRequestMessage ();
			try {
				c.Method = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				c.RequestUri = new Uri ("ftp://xamarin.com");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				c.Version = null;
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {
			}
		}
	}
}
