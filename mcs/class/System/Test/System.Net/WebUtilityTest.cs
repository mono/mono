//
// System.Net.WebUtilityTest.cs
//
// copied and edited from System.Web.HttpUtilityTest.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Mike Kestner <mkestner@novell.com>
//
// Copyright (C) 2005, 2010 Novell, Inc (http://www.novell.com)
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

#if NET_4_0 && !MOBILE

using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Specialized;

using NUnit.Framework;

namespace MonoTests.System.Net {

	[TestFixture]
	public class WebUtilityTest {

		[Test]
		public void HtmlEncode_LtGt ()
		{
			Assert.AreEqual ("&lt;script&gt;", WebUtility.HtmlEncode ("<script>"));
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
#if TARGET_JVM
		[Ignore ("TD #6954")]
#endif
		public void HtmlEncode_XSS ()
		{
			string problem = "\xff1cscript\xff1e";  // unicode looks alike <script>
			byte[] utf8data = Encoding.UTF8.GetBytes (problem);
			Encoding win1251 = Encoding.GetEncoding ("windows-1251");
			byte[] windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			// now it's a real problem
			Assert.AreEqual ("<script>", Encoding.ASCII.GetString (windata), "<script>");

			string encoded = WebUtility.HtmlEncode (problem);
			Assert.AreEqual ("&#65308;script&#65310;", encoded, "&#65308;script&#65310;");
			
			utf8data = Encoding.UTF8.GetBytes (encoded);
			windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			Assert.AreEqual ("&#65308;script&#65310;", Encoding.ASCII.GetString (windata), "ok");
		}

		[Test]
#if !TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void HtmlEncode () {
			for (char c = char.MinValue; c < char.MaxValue; c++) {
				String exp = HtmlEncode (c.ToString ());
				String act = WebUtility.HtmlEncode (c.ToString ());
				Assert.AreEqual (exp, act, "HtmlEncode " + c.ToString () + " [" + (int) c + "]");
			}
		}
		
		string HtmlEncode (string s) {
			if (s == null)
				return null;

			bool needEncode = false;
			for (int i = 0; i < s.Length; i++) {
				char c = s [i];
				if (c == '&' || c == '"' || c == '<' || c == '>' || c > 159) {
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
				default:
					// MS starts encoding with &# from 160 and stops at 255.
					// We don't do that. One reason is the 65308/65310 unicode
					// characters that look like '<' and '>'.
					if (s [i] > 159 && s [i] < 256) {
						output.Append ("&#");
						output.Append (((int) s [i]).ToString ());
						output.Append (";");
					}
					else {
						output.Append (s [i]);
					}
					break;
				}
			return output.ToString ();
		}

		
		[Test]
		public void EscapedCharacters ()
		{
			for (int i = 0; i < 256; i++) {
				string str = new string ((char) i, 1);
				string encoded = WebUtility.HtmlEncode (str);
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
			Assert.AreEqual ("\xE9", WebUtility.HtmlDecode ("&#233;"));
		}

		[Test]
		public void RoundTrip ()
		{
			string x = "<html>& hello+= world!";
                        string y = WebUtility.HtmlEncode (x);
                        string z = WebUtility.HtmlDecode (y);
			Assert.AreEqual (x, z);
		}

		[Test]
		public void LooksLikeEntity ()
		{
			string str = "<%# \"hola\" + \"/somepage.aspx?ItemID=\" + DataBinder.Eval(Container.DataItem,\"Country\")" +
					" + \"&mid=\" + ModuleID + \"&pageindex=\" + Request.Params.Get(\"pageindex\") %>";
			Assert.AreEqual (str, WebUtility.HtmlDecode (str));
		}
	}
}
#endif

