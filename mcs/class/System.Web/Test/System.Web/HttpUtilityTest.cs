//
// System.Web.HttpUtilityTest.cs - Unit tests for System.Web.HttpUtility
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.IO;
using System.Collections.Specialized;

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class HttpUtilityTest
	{

		[Test]
		public void HtmlAttributeEncode ()
		{
			Assert.AreEqual (null, HttpUtility.HtmlAttributeEncode (null), "#A1");
			Assert.AreEqual (String.Empty, HttpUtility.HtmlAttributeEncode (String.Empty), "#A2");
			Assert.AreEqual ("&lt;script>", HttpUtility.HtmlAttributeEncode ("<script>"), "#A3");
			Assert.AreEqual ("&quot;a&amp;b&quot;", HttpUtility.HtmlAttributeEncode ("\"a&b\""), "#A4");
			Assert.AreEqual ("&#39;string&#39;", HttpUtility.HtmlAttributeEncode ("'string'"), "#A5");
		}

		[Test]
		public void HtmlAttributeEncode_String_TextWriter ()
		{
			var sw = new StringWriter ();
			Assert.Throws<ArgumentNullException> (() => {
				HttpUtility.HtmlAttributeEncode ("string", null);
			}, "#A1");

			HttpUtility.HtmlAttributeEncode ("<script>", sw);
			Assert.AreEqual ("&lt;script>", sw.ToString (), "#A2");

			sw = new StringWriter ();
			HttpUtility.HtmlAttributeEncode ("\"a&b\"", sw);
			Assert.AreEqual ("&quot;a&amp;b&quot;", sw.ToString (), "#A3");

			sw = new StringWriter ();
			HttpUtility.HtmlAttributeEncode ("'string'", sw);
			Assert.AreEqual ("&#39;string&#39;", sw.ToString (), "#A4");
			sw = new StringWriter ();
			HttpUtility.HtmlAttributeEncode ("\\string\\", sw);
			Assert.AreEqual ("\\string\\", sw.ToString (), "#A5");

			sw = new StringWriter ();
			HttpUtility.HtmlAttributeEncode (String.Empty, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A6");

			sw = new StringWriter ();
			HttpUtility.HtmlAttributeEncode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A7");
		}

		[Test]
		public void HtmlDecode ()
		{
			Assert.AreEqual (null, HttpUtility.HtmlDecode (null), "#A1");
			Assert.AreEqual (String.Empty, HttpUtility.HtmlDecode (String.Empty), "#A2");

			for (int i = 0; i < _coreDecodingData.GetLength(0); i++)
				Assert.AreEqual ((string)_coreDecodingData[i, 1], HttpUtility.HtmlDecode ((string)_coreDecodingData[i, 0]), "#B" + (i).ToString ());
		}

		[Test]
		public void HtmlDecode_String_TextWriter ()
		{
			StringWriter sw;
			Assert.Throws<ArgumentNullException> (() => {
				HttpUtility.HtmlDecode ("string", null);
			}, "#A1");

			sw = new StringWriter ();
			HttpUtility.HtmlDecode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A2");

			sw = new StringWriter ();
			HttpUtility.HtmlDecode (String.Empty, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A3");

			for (int i = 0; i < _coreDecodingData.GetLength(0); i++) {
				sw = new StringWriter();
				HttpUtility.HtmlDecode((string)_coreDecodingData[i, 0], sw);

				string val = (string)_coreDecodingData[i, 1];

				if (val == null) {
					Assert.IsTrue(String.IsNullOrEmpty(sw.ToString()), "#B" + (i).ToString());
				}
				else {
					Assert.AreEqual(val, sw.ToString(), "#B" + (i).ToString());
				}
			}
		}

		[Test]
		public void HtmlEncode_LtGt ()
		{
			Assert.AreEqual ("&lt;script&gt;", HttpUtility.HtmlEncode ("<script>"));
		}

		// Notes:
		// * this is to avoid a regression that would cause Mono to 
		//   fail item #3 of the XSS vulnerabilities listed at:
		//   http://it-project.ru/andir/docs/aspxvuln/aspxvuln.en.xml
		//   we didn't fall the first time so let's ensure we never will
		// * The author notes that Microsoft has decided not to fix 
		//   this issue (hence the NotDotNet category).

		[Test]
		[Category ("NotDotNet")]
		public void HtmlEncode_XSS ()
		{
			string problem = "\xff1cscript\xff1e";  // unicode looks alike <script>
			byte [] utf8data = Encoding.UTF8.GetBytes (problem);
			Encoding win1251 = Encoding.GetEncoding ("windows-1251");
			byte [] windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			// now it's a real problem
			Assert.AreEqual ("<script>", Encoding.ASCII.GetString (windata), "<script>");

			string encoded = HttpUtility.HtmlEncode (problem);
			Assert.AreEqual ("&#65308;script&#65310;", encoded, "&#65308;script&#65310;");

			utf8data = Encoding.UTF8.GetBytes (encoded);
			windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			Assert.AreEqual ("&#65308;script&#65310;", Encoding.ASCII.GetString (windata), "ok");
		}
		[Test]
		public void JavaScriptStringEncode ()
		{
			Assert.AreEqual (String.Empty, HttpUtility.JavaScriptStringEncode (null), "#A1");
			Assert.AreEqual ("\"\"", HttpUtility.JavaScriptStringEncode (null, true), "#A2");
			Assert.AreEqual (String.Empty, HttpUtility.JavaScriptStringEncode (String.Empty), "#A3");
			Assert.AreEqual ("\"\"", HttpUtility.JavaScriptStringEncode (String.Empty, true), "#A4");

			for (char c = char.MinValue; c < char.MaxValue; c++) {
				string exp = JavaScriptStringEncode (c.ToString (), false);
				string expQuoted = JavaScriptStringEncode (c.ToString (), true);
				string act = HttpUtility.JavaScriptStringEncode (c.ToString ());
				string actQuoted = HttpUtility.JavaScriptStringEncode (c.ToString (), true);
				Assert.AreEqual (exp, act, "JavaScriptStringEncode " + c.ToString () + " [" + (int) c + "]");
				Assert.AreEqual (expQuoted, actQuoted, "JavaScriptStringEncode (quoted) " + c.ToString () + " [" + (int) c + "]");
			}
		}

		string JavaScriptStringEncode (string s, bool addDoubleQuotes)
		{
			if (String.IsNullOrEmpty (s))
				return addDoubleQuotes ? "\"\"" : String.Empty;

			int len = s.Length;
			bool needEncode = false;
			char c;
			for (int i = 0; i < len; i++) {
				c = s [i];

				if (c >= 0 && c <= 31 || c == 34 || c == 38 || c == 39 || c == 60 || c == 62 || c == 92 || c == 133|| c == 8232 || c == 8233) {
					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return addDoubleQuotes ? "\"" + s + "\"" : s;

			var sb = new StringBuilder ();
			if (addDoubleQuotes)
				sb.Append ('"');

			for (int i = 0; i < len; i++) {
				c = s [i];
				if (c >= 0 && c <= 7 || c == 11 || c >= 14 && c <= 31 || c == 38 || c == 39 || c == 60 || c == 62 || c == 133|| c == 8232 || c == 8233)
					sb.AppendFormat ("\\u{0:x4}", (int) c);
				else switch ((int) c) {
						case 8:
							sb.Append ("\\b");
							break;

						case 9:
							sb.Append ("\\t");
							break;

						case 10:
							sb.Append ("\\n");
							break;

						case 12:
							sb.Append ("\\f");
							break;

						case 13:
							sb.Append ("\\r");
							break;

						case 34:
							sb.Append ("\\\"");
							break;

						case 92:
							sb.Append ("\\\\");
							break;

						default:
							sb.Append (c);
							break;
					}
			}

			if (addDoubleQuotes)
				sb.Append ('"');

			return sb.ToString ();
		}

		[Test]
		public void HtmlEncode_2 ()
		{
			StringWriter sw;
			Assert.Throws<ArgumentNullException> (() => {
				HttpUtility.HtmlEncode ("string", null);
			}, "#A1");

			sw = new StringWriter ();
			HttpUtility.HtmlEncode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A2");

			sw = new StringWriter ();
			HttpUtility.HtmlEncode (String.Empty, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A3");

			for (int i = 0; i < _coreEncodingData.GetLength(0); i++) {
				sw = new StringWriter();
				HttpUtility.HtmlEncode((string)_coreEncodingData[i, 0], sw);

				string val = (string)_coreEncodingData[i, 1];

				if (val == null) {
					Assert.IsTrue(String.IsNullOrEmpty(sw.ToString()), "#B" + (i).ToString());
				}
				else {
					Assert.AreEqual(val, sw.ToString(), "#B" + (i).ToString());
				}
			}
		}

		[Test]
		public void HtmlEncode_3 ()
		{
			Assert.AreEqual (null, HttpUtility.HtmlEncode (null), "#A1");
			Assert.AreEqual (String.Empty, HttpUtility.HtmlEncode (String.Empty), "#A2");

			for (int i = 0; i < _coreEncodingData.GetLength(0); i++) {
				Assert.AreEqual((string)_coreEncodingData[i, 1], (string)HttpUtility.HtmlEncode(_coreEncodingData[i, 0]), "#B" + (i.ToString()));
			}
		}

		[Test]
		public void HtmlEncode_IHtmlString ()
		{
			string origString = "<script>alert ('Hola');</script>";
			var hs = new HtmlString (origString);
			Assert.AreEqual (origString, HttpUtility.HtmlEncode (hs), "#A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void HtmlEncode ()
		{
			for (char c = char.MinValue; c < char.MaxValue; c++) {
				String exp = HtmlEncode (c.ToString ());
				String act = HttpUtility.HtmlEncode (c.ToString ());
				Assert.AreEqual (exp, act, "HtmlEncode " + c.ToString () + " [" + (int) c + "]");
			}
		}

		string HtmlEncode (string s)
		{
			if (s == null)
				return null;

			bool needEncode = false;
			for (int i = 0; i < s.Length; i++) {
				char c = s [i];
				if (c == '&' || c == '"' || c == '<' || c == '>' || c > 159
 || c == '\''
) {
					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return s;

			StringBuilder output = new StringBuilder ();

			int len = s.Length;
			for (int i = 0; i < len; i++)
				switch (s [i]) {
					case '&':
						output.Append ("&amp;");
						break;
					case '>':
						output.Append ("&gt;");
						break;
					case '<':
						output.Append ("&lt;");
						break;
					case '"':
						output.Append ("&quot;");
						break;
					case '\'':
						output.Append ("&#39;");
						break;
					default:
						// MS starts encoding with &# from 160 and stops at 255.
						// We don't do that. One reason is the 65308/65310 unicode
						// characters that look like '<' and '>'.
						if (s [i] > 159 && s [i] < 256) {
							output.Append ("&#");
							output.Append (((int) s [i]).ToString ());
							output.Append (";");
						} else {
							output.Append (s [i]);
						}
						break;
				}
			return output.ToString ();
		}

		[Test]
		public void UrlDecodeToBytes ()
		{
			byte [] bytes = HttpUtility.UrlDecodeToBytes ("%5c");
			Assert.AreEqual (1, bytes.Length, "#1");
			Assert.AreEqual (0x5c, bytes [0], "#2");
			bytes = HttpUtility.UrlDecodeToBytes ("%5");
			Assert.AreEqual (2, bytes.Length, "#3");
			Assert.AreEqual (0x25, bytes [0], "#4");
			Assert.AreEqual (0x25, bytes [0], "#5");
		}

		[Test]
		public void UrlDecode1 ()
		{
			Assert.AreEqual ("http://127.0.0.1:8080/appDir/page.aspx?foo=bar",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%61r"),
				"UrlDecode1 #1");

			Assert.AreEqual ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%ar",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%%61r"),
				"UrlDecode1 #2");

			Assert.AreEqual ("http://127.0.0.1:8080/app%Dir/page.aspx?foo=b%ar",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/app%Dir/page.aspx?foo=b%%61r"),
				"UrlDecode1 #3");

			Assert.AreEqual ("http://127.0.0.1:8080/app%%Dir/page.aspx?foo=b%%r",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/app%%Dir/page.aspx?foo=b%%r"),
				"UrlDecode1 #4");

			Assert.AreEqual ("http://127.0.0.1:8080/appDir/page.aspx?foo=ba%r",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%61%r"),
				"UrlDecode1 #5");

			Assert.AreEqual ("http://127.0.0.1:8080/appDir/page.aspx?foo=bar",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%u0061r"),
				"UrlDecode1 #6");

			Assert.AreEqual ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%ar",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%%u0061r"),
				"UrlDecode1 #7");

			Assert.AreEqual ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%uu0061r",
				HttpUtility.UrlDecode ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%uu0061r"),
				"UrlDecode1 #8");
		}

		[Test]
		public void UrlDecode2 ()
		{
			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=bar",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%61r"),
				Encoding.UTF8),
				"UrlDecode2 #1");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%ar",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%%61r"),
				Encoding.UTF8),
				"UrlDecode2 #2");

			Assert.AreEqual (
				"http://127.0.0.1:8080/app%Dir/page.aspx?foo=b%ar",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/app%Dir/page.aspx?foo=b%%61r"),
				Encoding.UTF8),
				"UrlDecode2 #3");

			Assert.AreEqual (
				"http://127.0.0.1:8080/app%%Dir/page.aspx?foo=b%%r",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/app%%Dir/page.aspx?foo=b%%r"),
				Encoding.UTF8),
				"UrlDecode2 #4");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=ba%r",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%61%r"),
				Encoding.UTF8),
				"UrlDecode2 #5");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=bar",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%u0061r"),
				Encoding.UTF8),
				"UrlDecode2 #6");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%ar",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%%u0061r"),
				Encoding.UTF8),
				"UrlDecode2 #7");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%uu0061r",
				HttpUtility.UrlDecode (
				Encoding.UTF8.GetBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%uu0061r"),
				Encoding.UTF8),
				"UrlDecode2 #8");
		}

		[Test]
		public void UrlDecodeToBytes2 ()
		{
			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=bar",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%61r")),
				"UrlDecodeToBytes2 #1");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%ar",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%%61r")),
				"UrlDecodeToBytes2 #2");

			Assert.AreEqual (
				"http://127.0.0.1:8080/app%Dir/page.aspx?foo=b%ar",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/app%Dir/page.aspx?foo=b%%61r")),
				"UrlDecodeToBytes2 #3");

			Assert.AreEqual (
				"http://127.0.0.1:8080/app%%Dir/page.aspx?foo=b%%r",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/app%%Dir/page.aspx?foo=b%%r")),
				"UrlDecodeToBytes2 #4");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=ba%r",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%61%r")),
				"UrlDecodeToBytes2 #5");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%u0061r",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%u0061r")),
				"UrlDecodeToBytes2 #6");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%%u0061r",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%%u0061r")),
				"UrlDecodeToBytes2 #7");

			Assert.AreEqual (
				"http://127.0.0.1:8080/appDir/page.aspx?foo=b%uu0061r",
				Encoding.UTF8.GetString (
				HttpUtility.UrlDecodeToBytes ("http://127.0.0.1:8080/appDir/page.aspx?foo=b%uu0061r")),
				"UrlDecodeToBytes2 #8");
		}

		[Test]
		public void EscapedCharacters ()
		{
			for (int i = 0; i < 256; i++) {
				string str = new string ((char) i, 1);
				string encoded = HttpUtility.HtmlEncode (str);
				if ((i > 159 && i < 256) || i == '&' || i == '<' || i == '>' || i == '"'
 || i == '\''
) {
					if (encoded [0] != '&' || encoded [encoded.Length - 1] != ';')
						Assert.Fail ("Failed for i = " + i);
				} else if (encoded.Length != 1) {
					Assert.Fail ("Wrong length for i = " + i);
				}
			}
		}

		[Test (Description = "Bug #507666")]
		public void UrlDecode_Bug507666 ()
		{
			// Get Encoding object.
			var enc_utf8 = Encoding.UTF8;
			var enc_sjis = Encoding.GetEncoding (932);

			// Generate equiv. client request query string with url-encoded shift_jis string.
			var utf8_string = "紅茶"; // it's UTF-8 string
			var utf8_bin = enc_utf8.GetBytes (utf8_string); // convert to UTF-8 byte[]
			var sjis_bin = Encoding.Convert (enc_utf8, enc_sjis, utf8_bin); // convert to Shift_jis byte[]
			var urlenc_string = HttpUtility.UrlEncode (sjis_bin); // equiv. client request query string.

			// Test using UrlDecode only.
			var decoded_by_web = HttpUtility.UrlDecode (urlenc_string, enc_sjis);

			Assert.AreEqual (utf8_string, decoded_by_web, "#A1");
		}

		[Test]
		public void Decode1 ()
		{
			Assert.AreEqual ("\xE9", HttpUtility.HtmlDecode ("&#233;"));
		}

		[Test (Description = "Bug #585992")]
		public void Decode2 ()
		{
			string encodedSource = "&#169; == &#xA9; == &#XA9; and &#915; == &#x393; == &#X393;";
			string utf8Result = "© == © == © and Γ == Γ == Γ";

			Assert.AreEqual (utf8Result, HttpUtility.HtmlDecode (encodedSource), "#A1");
		}

		[Test]
		public void RoundTrip ()
		{
			string x = "<html>& hello+= world!";
			string y = HttpUtility.HtmlEncode (x);
			string z = HttpUtility.HtmlDecode (y);
			Assert.AreEqual (x, z);
		}

		[Test]
		public void LooksLikeEntity ()
		{
			string str = "<%# \"hola\" + \"/somepage.aspx?ItemID=\" + DataBinder.Eval(Container.DataItem,\"Country\")" +
					" + \"&mid=\" + ModuleID + \"&pageindex=\" + Request.Params.Get(\"pageindex\") %>";
			Assert.AreEqual (str, HttpUtility.HtmlDecode (str));
		}

		[Test]
		public void UrlEncodeUnicodeTest ()
		{
			string str = "sch" + (char) 0xf6 + "n";

			Assert.AreEqual ("sch%u00f6n", HttpUtility.UrlEncodeUnicode (str), "#1");
			Assert.AreEqual ("abc", "abc", "#2");
			Assert.AreEqual ("%26", HttpUtility.UrlEncodeUnicode ("&"), "#3");
			Assert.AreEqual ("%7f", HttpUtility.UrlEncodeUnicode ("" + (char) 127), "#4");
			Assert.AreEqual ("%u0080", HttpUtility.UrlEncodeUnicode ("" + (char) 128), "#5");
		}

		[Test]
		public void UrlDecodeNoThrow ()
		{
			string str = "../../&amp;param2=%CURRREV%";
			Assert.AreEqual (str, HttpUtility.UrlDecode (str), "#1");
		}

		static char [] hexChars = "0123456789abcdef".ToCharArray ();

		const string notEncoded = "!()*-._";

		static void UrlPathEncodeChar (char c, Stream result)
		{
			if (c < 33 || c > 126) {
				byte [] bIn = Encoding.UTF8.GetBytes (c.ToString ());
				for (int i = 0; i < bIn.Length; i++) {
					result.WriteByte ((byte) '%');
					int idx = ((int) bIn [i]) >> 4;
					result.WriteByte ((byte) hexChars [idx]);
					idx = ((int) bIn [i]) & 0x0F;
					result.WriteByte ((byte) hexChars [idx]);
				}
			} else if (c == ' ') {
				result.WriteByte ((byte) '%');
				result.WriteByte ((byte) '2');
				result.WriteByte ((byte) '0');
			} else
				result.WriteByte ((byte) c);
		}

		static void UrlEncodeChar (char c, Stream result, bool isUnicode)
		{
			if (c > 255) {
				//FIXME: what happens when there is an internal error?
				//if (!isUnicode)
				//	throw new ArgumentOutOfRangeException ("c", c, "c must be less than 256");
				int idx;
				int i = (int) c;

				result.WriteByte ((byte) '%');
				result.WriteByte ((byte) 'u');
				idx = i >> 12;
				result.WriteByte ((byte) hexChars [idx]);
				idx = (i >> 8) & 0x0F;
				result.WriteByte ((byte) hexChars [idx]);
				idx = (i >> 4) & 0x0F;
				result.WriteByte ((byte) hexChars [idx]);
				idx = i & 0x0F;
				result.WriteByte ((byte) hexChars [idx]);
				return;
			}

			if (c > ' ' && notEncoded.IndexOf (c) != -1) {
				result.WriteByte ((byte) c);
				return;
			}
			if (c == ' ') {
				result.WriteByte ((byte) '+');
				return;
			}
			if ((c < '0') ||
				(c < 'A' && c > '9') ||
				(c > 'Z' && c < 'a') ||
				(c > 'z')) {
				if (isUnicode && c > 127) {
					result.WriteByte ((byte) '%');
					result.WriteByte ((byte) 'u');
					result.WriteByte ((byte) '0');
					result.WriteByte ((byte) '0');
				} else
					result.WriteByte ((byte) '%');

				int idx = ((int) c) >> 4;
				result.WriteByte ((byte) hexChars [idx]);
				idx = ((int) c) & 0x0F;
				result.WriteByte ((byte) hexChars [idx]);
			} else
				result.WriteByte ((byte) c);
		}

		[Test]
		public void UrlEncode ()
		{
			for (char c = char.MinValue; c < char.MaxValue; c++) {
				byte [] bIn;
				bIn = Encoding.UTF8.GetBytes (c.ToString ());
				MemoryStream expected = new MemoryStream ();
				MemoryStream expUnicode = new MemoryStream ();

				//build expected result for UrlEncode
				for (int i = 0; i < bIn.Length; i++)
					UrlEncodeChar ((char) bIn [i], expected, false);
				//build expected result for UrlEncodeUnicode
				UrlEncodeChar (c, expUnicode, true);

				Assert.AreEqual (Encoding.ASCII.GetString (expected.ToArray ()), HttpUtility.UrlEncode (c.ToString ()),
					"UrlEncode " + c.ToString ());
				Assert.AreEqual (Encoding.ASCII.GetString (expUnicode.ToArray ()), HttpUtility.UrlEncodeUnicode (c.ToString ()),
					"UrlEncodeUnicode " + c.ToString ());
			}
		}

		[Test]
		public void UrlPathEncode ()
		{
			Assert.AreEqual (null, HttpUtility.UrlPathEncode (null), "#A1-1");
			Assert.AreEqual (String.Empty, HttpUtility.UrlPathEncode (String.Empty), "#A1-2");

			for (char c = char.MinValue; c < char.MaxValue; c++) {
				MemoryStream expected = new MemoryStream ();
				UrlPathEncodeChar (c, expected);

				String exp = Encoding.ASCII.GetString (expected.ToArray ());
				String act = HttpUtility.UrlPathEncode (c.ToString ());
				Assert.AreEqual (exp, act, "UrlPathEncode " + c.ToString ());
			}
		}
		[Test]
		public void UrlPathEncode2 ()
		{
			string s = "default.xxx?sdsd=sds";
			string s2 = HttpUtility.UrlPathEncode (s);
			Assert.AreEqual (s, s2, "UrlPathEncode " + s);
		}


		[Test]
		public void ParseQueryString ()
		{
			ParseQueryString_Helper (HttpUtility.ParseQueryString ("name=value"), "#1",
				new string [] { "name" }, new string [] [] { new string [] { "value" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("name=value&foo=bar"), "#2",
				new string [] { "name", "foo" }, new string [] [] { new string [] { "value" }, new string [] { "bar" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("name=value&name=bar"), "#3",
				new string [] { "name" }, new string [] [] { new string [] { "value", "bar" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("value"), "#4",
				new string [] { null }, new string [] [] { new string [] { "value" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("name=value&bar"), "#5",
				new string [] { "name", null }, new string [] [] { new string [] { "value" }, new string [] { "bar" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("bar&name=value"), "#6",
				new string [] { null, "name" }, new string [] [] { new string [] { "bar" }, new string [] { "value" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("value&bar"), "#7",
				new string [] { null }, new string [] [] { new string [] { "value", "bar" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString (""), "#8",
				new string [0], new string [0] []);

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("="), "#9",
				new string [] { "" }, new string [] [] { new string [] { "" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("&"), "#10",
				new string [] { null }, new string [] [] { new string [] { "", "" } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("?value"), "#11",
				new string [] { null }, new string [] [] { new string [] { "value" } });

			try {
				HttpUtility.ParseQueryString (null);
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#12");
			}

			try {
				HttpUtility.ParseQueryString ("", null);
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#13");
			}

			string str = new string (new char [] { '\u304a', '\u75b2', '\u308c', '\u69d8', '\u3067', '\u3059' });
			string utf8url = HttpUtility.UrlEncode (str, Encoding.UTF8);
			ParseQueryString_Helper (HttpUtility.ParseQueryString (utf8url + "=" + utf8url), "#14",
				new string [] { str }, new string [] [] { new string [] { str } });

			ParseQueryString_Helper (HttpUtility.ParseQueryString ("name=value=test"), "#15",
				new string [] { "name" }, new string [] [] { new string [] { "value=test" } });
		}
		static void ParseQueryString_Helper (NameValueCollection nvc, string msg, string [] keys, string [] [] values)
		{
			Assert.AreEqual (keys.Length, nvc.Count, msg + "[Count]");
			for (int i = 0; i < keys.Length; i++) {
				Assert.AreEqual (keys [i], nvc.GetKey (i), msg + "[Key]");
				string [] tmp = nvc.GetValues (i);
				Assert.AreEqual (values [i].Length, tmp.Length, msg + "[ValueCount]");
				for (int q = 0; q < values [i].Length; q++)
					Assert.AreEqual (values [i] [q], tmp [q], msg + "[Value]");
			}
		}

		[Test]
		public void ToStringEncoding ()
		{
			var queryStringNameValues = HttpUtility.ParseQueryString(string.Empty);
			queryStringNameValues.Add("ReturnUrl", @"http://localhost/login/authenticate?ReturnUrl=http://localhost/secured_area&__provider__=google");

			var expected = "ReturnUrl=http%3a%2f%2flocalhost%2flogin%2fauthenticate%3fReturnUrl%3dhttp%3a%2f%2flocalhost%2fsecured_area%26__provider__%3dgoogle";
			Assert.AreEqual (expected, queryStringNameValues.ToString());
		}

		// Took the expected encoding and decoding values from corefx tests.  
		// We may just want to hook into that eventually...
		object[,] _coreEncodingData = {
			// Single quotes need to be encoded as &#39; rather than &apos; since &#39; is valid both for
            // HTML and XHTML, but &apos; is valid only for XHTML.
            // For more info: http://fishbowl.pastiche.org/2003/07/01/the_curse_of_apos/
            { "'", "&#39;" },

            { "Hello! '\"<&>\u2665\u00E7 World", "Hello! &#39;&quot;&lt;&amp;&gt;&#9829;&#231; World" },
            { "<>\"\\&", "&lt;&gt;&quot;\\&amp;" },
            { "\u00A0", "&#160;" },
            { "\u00FF", "&#255;" },
            { "\u0100", "&#256;" },
            { "\u0021\u0023\u003D\u003F", "!#=?" },

            // Surrogate pairs - default strict settings
            { char.ConvertFromUtf32(144308), "&#144308;" },
            { "\uD800\uDC00", "&#65536;" },
            { "a\uD800\uDC00b", "a&#65536;b" },

            // High BMP non-chars
            { "\uFFFD", "&#65533;" },
            { "\uFFFE", "&#65534;" },
            { "\uFFFF", "&#65535;" },

            // Lone high surrogate
            { "\uD800", "\uFFFD" },
            { "\uD800a", "\uFFFDa" },

            // Lone low surrogate
            { "\uDC00", "\uFFFD" },
            { "\uDC00a", "\uFFFDa" },

            // Invalid surrogate pair
            { "\uD800\uD800", "\uFFFD\uFFFD" }, // High, high
            { "\uDC00\uD800", "\uFFFD\uFFFD" }, // Low, high
            { "\uDC00\uDC00", "\uFFFD\uFFFD" }, // Low, low

            // Basic
            { "Hello, world!", "Hello, world!" },
            { "    ", "    " },

            // Empty string
            { "", "" },
            { null, null }
		};

		object [,] _coreDecodingData = {
			{ "Hello! &apos;&quot;&lt;&amp;&gt;\u2665&hearts;\u00E7&#xe7;&#231;", "Hello! '\"<&>\u2665\u2665\u00E7\u00E7\u00E7" },
			{ "&#xD7FF;&#xd7ff;", "\uD7FF\uD7FF" },
			{ "&#xE000;&#xe000;", "\uE000\uE000" },
			{ "&#97;&#98;&#99;", "abc" },

			// Surrogate pairs
			{ "&#65536;", "\uD800\uDC00" },
			{ "a&#65536;b", "a\uD800\uDC00b" },
			{ "&#144308;", char.ConvertFromUtf32(144308) },

			// Invalid encoding
			{ "&", "&" },
			{ "&#", "&#" },
			{ "&#x", "&#x" },
			{ "&abc", "&abc" },
			{ "&abc;", "&abc;" },
			{ "&#65536", "&#65536" },
			{ "&#xD7FF", "&#xD7FF" },
			{ "&#xG123;", "&#xG123;" },
			{ "&#xD800;", "&#xD800;" },
			{ "&#xDFFF;", "&#xDFFF;" },
			{ "&#1114112;", "&#1114112;" },
			{ "&#x110000;", "&#x110000;" },
			{ "&#4294967296;", "&#4294967296;" },
			{ "&#x100000000;", "&#x100000000;" },

			// High BMP non-chars
			{ "\uFFFD", "\uFFFD" },
			{ "\uFFFE", "\uFFFE" },
			{ "\uFFFF", "\uFFFF" },

			// Basic
			{ "Hello, world!", "Hello, world!" },
			{ "Hello, world! \"<>\u2665\u00E7", "Hello, world! \"<>\u2665\u00E7" },
			{ "    ", "    " },

			// Empty
			{ "", "" },
			{ null, null }
		};
		// Needs decoding
           
		
	}
}

