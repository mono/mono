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

		[Test]
		public void EntityEncoding ()
		{
			var expected = "\u00A0\u00A1\u00A2\u00A3\u00A4\u00A5\u00A6\u00A7\u00A8\u00A9\u00AA\u00AB\u00AC\u00AD\u00AE\u00AF\u00B0\u00B1\u00B2\u00B3\u00B4\u00B5\u00B6\u00B7\u00B8\u00B9\u00BA\u00BB\u00BC\u00BD\u00BE\u00BF\u00C0\u00C1\u00C2\u00C3\u00C4\u00C5\u00C6\u00C7\u00C8\u00C9\u00CA\u00CB\u00CC\u00CD\u00CE\u00CF\u00D0\u00D1\u00D2\u00D3\u00D4\u00D5\u00D6\u00D7\u00D8\u00D9\u00DA\u00DB\u00DC\u00DD\u00DE\u00DF\u00E0\u00E1\u00E2\u00E3\u00E4\u00E5\u00E6\u00E7\u00E8\u00E9\u00EA\u00EB\u00EC\u00ED\u00EE\u00EF\u00F0\u00F1\u00F2\u00F3\u00F4\u00F5\u00F6\u00F7\u00F8\u00F9\u00FA\u00FB\u00FC\u00FD\u00FE\u00FF\u0192\u0391\u0392\u0393\u0394\u0395\u0396\u0397\u0398\u0399\u039A\u039B\u039C\u039D\u039E\u039F\u03A0\u03A1\u03A3\u03A4\u03A5\u03A6\u03A7\u03A8\u03A9\u03B1\u03B2\u03B3\u03B4\u03B5\u03B6\u03B7\u03B8\u03B9\u03BA\u03BB\u03BC\u03BD\u03BE\u03BF\u03C0\u03C1\u03C2\u03C3\u03C4\u03C5\u03C6\u03C7\u03C8\u03C9\u03D1\u03D2\u03D6\u2022\u2026\u2032\u2033\u203E\u2044\u2118\u2111\u211C\u2122\u2135\u2190\u2191\u2192\u2193\u2194\u21B5\u21D0\u21D1\u21D2\u21D3\u21D4\u2200\u2202\u2203\u2205\u2207\u2208\u2209\u220B\u220F\u2211\u2212\u2217\u221A\u221D\u221E\u2220\u2227\u2228\u2229\u222A\u222B\u2234\u223C\u2245\u2248\u2260\u2261\u2264\u2265\u2282\u2283\u2284\u2286\u2287\u2295\u2297\u22A5\u22C5\u2308\u2309\u230A\u230B\u2329\u232A\u25CA\u2660\u2663\u2665\u2666\u0022\u0026\u003C\u003E\u0152\u0153\u0160\u0161\u0178\u02C6\u02DC\u2002\u2003\u2009\u200C\u200D\u200E\u200F\u2013\u2014\u2018\u2019\u201A\u201C\u201D\u201E\u2020\u2021\u2030\u2039\u203A\u20AC";

			var htmlDecoded = WebUtility.HtmlDecode ("&nbsp;&iexcl;&cent;&pound;&curren;&yen;&brvbar;&sect;&uml;&copy;&ordf;&laquo;&not;&shy;&reg;&macr;&deg;&plusmn;&sup2;&sup3;&acute;&micro;&para;&middot;&cedil;&sup1;&ordm;&raquo;&frac14;&frac12;&frac34;&iquest;&Agrave;&Aacute;&Acirc;&Atilde;&Auml;&Aring;&AElig;&Ccedil;&Egrave;&Eacute;&Ecirc;&Euml;&Igrave;&Iacute;&Icirc;&Iuml;&ETH;&Ntilde;&Ograve;&Oacute;&Ocirc;&Otilde;&Ouml;&times;&Oslash;&Ugrave;&Uacute;&Ucirc;&Uuml;&Yacute;&THORN;&szlig;&agrave;&aacute;&acirc;&atilde;&auml;&aring;&aelig;&ccedil;&egrave;&eacute;&ecirc;&euml;&igrave;&iacute;&icirc;&iuml;&eth;&ntilde;&ograve;&oacute;&ocirc;&otilde;&ouml;&divide;&oslash;&ugrave;&uacute;&ucirc;&uuml;&yacute;&thorn;&yuml;&fnof;&Alpha;&Beta;&Gamma;&Delta;&Epsilon;&Zeta;&Eta;&Theta;&Iota;&Kappa;&Lambda;&Mu;&Nu;&Xi;&Omicron;&Pi;&Rho;&Sigma;&Tau;&Upsilon;&Phi;&Chi;&Psi;&Omega;&alpha;&beta;&gamma;&delta;&epsilon;&zeta;&eta;&theta;&iota;&kappa;&lambda;&mu;&nu;&xi;&omicron;&pi;&rho;&sigmaf;&sigma;&tau;&upsilon;&phi;&chi;&psi;&omega;&thetasym;&upsih;&piv;&bull;&hellip;&prime;&Prime;&oline;&frasl;&weierp;&image;&real;&trade;&alefsym;&larr;&uarr;&rarr;&darr;&harr;&crarr;&lArr;&uArr;&rArr;&dArr;&hArr;&forall;&part;&exist;&empty;&nabla;&isin;&notin;&ni;&prod;&sum;&minus;&lowast;&radic;&prop;&infin;&ang;&and;&or;&cap;&cup;&int;&there4;&sim;&cong;&asymp;&ne;&equiv;&le;&ge;&sub;&sup;&nsub;&sube;&supe;&oplus;&otimes;&perp;&sdot;&lceil;&rceil;&lfloor;&rfloor;&lang;&rang;&loz;&spades;&clubs;&hearts;&diams;&quot;&amp;&lt;&gt;&OElig;&oelig;&Scaron;&scaron;&Yuml;&circ;&tilde;&ensp;&emsp;&thinsp;&zwnj;&zwj;&lrm;&rlm;&ndash;&mdash;&lsquo;&rsquo;&sbquo;&ldquo;&rdquo;&bdquo;&dagger;&Dagger;&permil;&lsaquo;&rsaquo;&euro;");
			
			Assert.AreEqual (expected, htmlDecoded);
		}
	}
}
#endif

