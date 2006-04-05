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

using System.Text;
using System.Web;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class HttpUtilityTest {

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
			byte[] utf8data = Encoding.UTF8.GetBytes (problem);
			Encoding win1251 = Encoding.GetEncoding ("windows-1251");
			byte[] windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			// now it's a real problem
			Assert.AreEqual ("<script>", Encoding.ASCII.GetString (windata), "<script>");

			string encoded = HttpUtility.HtmlEncode (problem);
			Assert.AreEqual ("&#65308;script&#65310;", encoded, "&#65308;script&#65310;");
			
			utf8data = Encoding.UTF8.GetBytes (encoded);
			windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			Assert.AreEqual ("&#65308;script&#65310;", Encoding.ASCII.GetString (windata), "ok");
		}

		[Test]
		public void UrlDecodeToBytes ()
		{
			byte[] bytes = HttpUtility.UrlDecodeToBytes ("%5c");
			Assert.AreEqual (1, bytes.Length, "#1");
			Assert.AreEqual (0x5c, bytes [0], "#2");
			bytes = HttpUtility.UrlDecodeToBytes ("%5");
			Assert.AreEqual (2, bytes.Length, "#3");
			Assert.AreEqual (0x25, bytes [0], "#4");
			Assert.AreEqual (0x25, bytes [0], "#5");
		}


		[Test]
		public void EscapedCharacters ()
		{
			for (int i = 0; i < 256; i++) {
				string str = new string ((char) i, 1);
				string encoded = HttpUtility.HtmlEncode (str);
				if ((i > 159 && i < 256 ) || i == '&' || i == '<' || i == '>' || i == '"') {
					if (encoded [0] != '&' || encoded [encoded.Length - 1] != ';')
						Assert.Fail ("Failed for i = " + i);
				} else if (encoded.Length != 1) {
					Assert.Fail ("Wrong length for i = " + i);
				}
			}
		}

		[Test]
		public void Decode1 ()
		{
			Assert.AreEqual ("\xE9", HttpUtility.HtmlDecode ("&#233;"));
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

		const string notEncoded = "!'()*-._";

		static void UrlEncodeChar (char c, Stream result, bool isUnicode) {
			if (c > 255) {
				//FIXME: what happens when there is an internal error?
				//if (!isUnicode)
				//	throw new ArgumentOutOfRangeException ("c", c, "c must be less than 256");
				int idx;
				int i = (int) c;

				result.WriteByte ((byte)'%');
				result.WriteByte ((byte)'u');
				idx = i >> 12;
				result.WriteByte ((byte)hexChars [idx]);
				idx = (i >> 8) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				idx = (i >> 4) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				idx = i & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
				return;
			}
			
			if (c>' ' && notEncoded.IndexOf (c)!=-1) {
				result.WriteByte ((byte)c);
				return;
			}
			if (c==' ') {
				result.WriteByte ((byte)'+');
				return;
			}
			if (	(c < '0') ||
				(c < 'A' && c > '9') ||
				(c > 'Z' && c < 'a') ||
				(c > 'z')) {
				if (isUnicode && c > 127) {
					result.WriteByte ((byte)'%');
					result.WriteByte ((byte)'u');
					result.WriteByte ((byte)'0');
					result.WriteByte ((byte)'0');
				}
				else
					result.WriteByte ((byte)'%');
				
				int idx = ((int) c) >> 4;
				result.WriteByte ((byte)hexChars [idx]);
				idx = ((int) c) & 0x0F;
				result.WriteByte ((byte)hexChars [idx]);
			}
			else
				result.WriteByte ((byte)c);
		}

		[Test]
		public void UrlEncode ()
		{
			for (char c=char.MinValue; c<char.MaxValue; c++) {
				byte [] bIn;
				bIn = Encoding.UTF8.GetBytes (c.ToString ());
				MemoryStream expected = new MemoryStream ();
				MemoryStream expUnicode = new MemoryStream ();

				//build expected result for UrlEncode
				for (int i = 0; i<bIn.Length; i++)
					UrlEncodeChar ((char)bIn[i], expected, false);
				//build expected result for UrlEncodeUnicode
				UrlEncodeChar (c, expUnicode, true);

				Assert.AreEqual (Encoding.ASCII.GetString(expected.ToArray()), HttpUtility.UrlEncode (c.ToString()),
					"UrlEncode "+c.ToString());
				Assert.AreEqual (Encoding.ASCII.GetString(expUnicode.ToArray()), HttpUtility.UrlEncodeUnicode (c.ToString()),
					"UrlEncodeUnicode "+c.ToString());
			}
		}
	}
}

