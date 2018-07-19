//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Util;

using NUnit.Framework;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.Util
{
	class HttpEncoderPoker : HttpEncoder
	{
		public void CallHtmlAttributeEncode (string value, TextWriter output)
		{
			HtmlAttributeEncode (value, output);
		}

		public void CallHtmlDecode (string value, TextWriter output)
		{
			HtmlDecode (value, output);
		}

		public void CallHtmlEncode (string value, TextWriter output)
		{
			HtmlEncode (value, output);
		}

		public byte [] CallUrlEncode (byte [] bytes, int offset, int count)
		{
			return UrlEncode (bytes, offset, count);
		}

		public string CallUrlPathEncode (string value)
		{
			return UrlPathEncode (value);
		}

		public void CallHeaderNameValueEncode (string headerName, string headerValue, out string encodedHeaderName, out string encodedHeaderValue)
		{
			HeaderNameValueEncode (headerName, headerValue, out encodedHeaderName, out encodedHeaderValue);
		}
	}

	[TestFixture]
	public class HttpEncoderTest
	{
		const string notEncoded = "!()*-._";
		static char [] hexChars = "0123456789abcdef".ToCharArray ();

		[Test]
		public void HtmlAttributeEncode ()
		{
			var encoder = new HttpEncoderPoker ();
			var sw = new StringWriter ();

			Assert.Throws<ArgumentNullException> (() => {
				encoder.CallHtmlAttributeEncode ("string", null);
			}, "#A1");

			encoder.CallHtmlAttributeEncode ("<script>", sw);
			Assert.AreEqual ("&lt;script>", sw.ToString (), "#A2");

			sw = new StringWriter ();
			encoder.CallHtmlAttributeEncode ("\"a&b\"", sw);
			Assert.AreEqual ("&quot;a&amp;b&quot;", sw.ToString (), "#A3");

			sw = new StringWriter ();
			encoder.CallHtmlAttributeEncode ("'string'", sw);
			Assert.AreEqual ("&#39;string&#39;", sw.ToString (), "#A4");

			sw = new StringWriter ();
			encoder.CallHtmlAttributeEncode ("\\string\\", sw);
			Assert.AreEqual ("\\string\\", sw.ToString (), "#A5");

			sw = new StringWriter ();
			encoder.CallHtmlAttributeEncode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A6");

			sw = new StringWriter ();
			encoder.CallHtmlAttributeEncode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A7");
		}

		[Test]
		public void HtmlDecode ()
		{
			var encoder = new HttpEncoderPoker ();
			StringWriter sw;

			Assert.Throws<ArgumentNullException> (() => {
				encoder.CallHtmlDecode ("string", null);
			}, "#A1");

			sw = new StringWriter ();
			encoder.CallHtmlDecode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A2");

			sw = new StringWriter ();
			encoder.CallHtmlDecode (String.Empty, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A3");

			for (int i = 0; i < decoding_pairs.Length; i += 2) {
				sw = new StringWriter ();
				encoder.CallHtmlDecode (decoding_pairs [i], sw);
				Assert.AreEqual (decoding_pairs [i + 1], sw.ToString (), "#B" + (i / 2).ToString ());
			}
		}

		[Test]
		public void HtmlDecode_Specials ()
		{
			var encoder = new HttpEncoderPoker ();
			var sw = new StringWriter ();

			encoder.CallHtmlDecode ("&hearts;&#6iQj", sw);
			Assert.AreEqual ("♥&#6iQj", sw.ToString (), "#A1");
		}

		[Test]
		public void HtmlEncode ()
		{
			var encoder = new HttpEncoderPoker ();
			StringWriter sw;

			Assert.Throws<ArgumentNullException> (() => {
				encoder.CallHtmlEncode ("string", null);
			}, "#A1");

			sw = new StringWriter ();
			encoder.CallHtmlEncode (null, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A2");

			sw = new StringWriter ();
			encoder.CallHtmlEncode (String.Empty, sw);
			Assert.AreEqual (String.Empty, sw.ToString (), "#A3");

			for (int i = 0; i < encoding_pairs.Length; i += 2) {
				sw = new StringWriter ();
				encoder.CallHtmlEncode (encoding_pairs [i], sw);
				Assert.AreEqual (encoding_pairs [i + 1], sw.ToString (), "#B" + (i / 2).ToString ());
			}
		}

		[Test]
		public void UrlEncode ()
		{
			var encoder = new HttpEncoderPoker ();
			byte [] bytes = new byte [10];

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				encoder.CallUrlEncode (bytes, -1, 1);
			}, "#A1-1");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				encoder.CallUrlEncode (bytes, 11, 1);
			}, "#A1-2");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				encoder.CallUrlEncode (bytes, 0, -1);
			}, "#A1-3");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				encoder.CallUrlEncode (bytes, 01, 11);
			}, "#A1-4");

			Assert.Throws<ArgumentNullException> (() => {
				encoder.CallUrlEncode (null, 0, 1);
			}, "#A1-5");

			for (char c = char.MinValue; c < char.MaxValue; c++) {
				byte [] bIn;
				bIn = Encoding.UTF8.GetBytes (c.ToString ());
				MemoryStream expected = new MemoryStream ();
				MemoryStream expUnicode = new MemoryStream ();

				//build expected result for UrlEncode
				for (int i = 0; i < bIn.Length; i++)
					UrlEncodeChar ((char) bIn [i], expected, false);

				byte [] expectedBytes = expected.ToArray ();
				byte [] encodedBytes = encoder.CallUrlEncode (bIn, 0, bIn.Length);
				Assert.IsNotNull (encodedBytes, "#B1-1");
				Assert.AreEqual (expectedBytes.Length, encodedBytes.Length, "#B1-2");

				for (int i = 0; i < expectedBytes.Length; i++)
					Assert.AreEqual (expectedBytes [i], encodedBytes [i], String.Format ("[Char: {0}] [Pos: {1}]", c, i));
			}

			byte [] encoded = encoder.CallUrlEncode (new byte [0], 0, 0);
			Assert.IsNotNull (encoded, "#C1-1");
			Assert.AreEqual (0, encoded.Length, "#C1-2");
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
		public void UrlPathEncode ()
		{
			var encoder = new HttpEncoderPoker ();

			Assert.AreEqual (null, encoder.CallUrlPathEncode (null), "#A1-1");
			Assert.AreEqual (String.Empty, encoder.CallUrlPathEncode (String.Empty), "#A1-2");

			for (char c = char.MinValue; c < char.MaxValue; c++) {
				MemoryStream expected = new MemoryStream ();
				UrlPathEncodeChar (c, expected);

				string exp = Encoding.ASCII.GetString (expected.ToArray ());
				string act = encoder.CallUrlPathEncode (c.ToString ());
				Assert.AreEqual (exp, act, "UrlPathEncode " + c.ToString ());
			}
		}

		[Test]
		public void UrlPathEncode2 ()
		{
			var encoder = new HttpEncoderPoker ();

			string s = "default.xxx?sdsd=sds";
			string s2 = encoder.CallUrlPathEncode (s);
			Assert.AreEqual (s, s2, "UrlPathEncode " + s);
		}

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

		[Test]
		public void HeaderNameValueEncode ()
		{
			var encoder = new HttpEncoderPoker ();
			string encodedName;
			string encodedValue;
			
			encoder.CallHeaderNameValueEncode (null, null, out encodedName, out encodedValue);
			Assert.AreEqual (null, encodedName, "#A1-1");
			Assert.AreEqual (null, encodedValue, "#A1-2");

			encoder.CallHeaderNameValueEncode (String.Empty, String.Empty, out encodedName, out encodedValue);
			Assert.AreEqual (String.Empty, encodedName, "#A2-1");
			Assert.AreEqual (String.Empty, encodedValue, "#A2-2");

			char ch;
			for (int i = Char.MinValue; i <= Char.MaxValue; i++) {
				ch = (char) i;
				encoder.CallHeaderNameValueEncode (ch.ToString (), null, out encodedName, out encodedValue);

				if (headerNameEncodedChars.ContainsKey (ch))
					Assert.AreEqual (headerNameEncodedChars [ch], encodedName, "#B1-" + i.ToString ());
				else
					Assert.AreEqual (ch.ToString (), encodedName, "#B1-" + i.ToString ());

				encoder.CallHeaderNameValueEncode (null, ch.ToString (), out encodedName, out encodedValue);
				if (headerValueEncodedChars.ContainsKey (ch))
					Assert.AreEqual (headerValueEncodedChars [ch], encodedValue, "#C1-" + i.ToString ());
				else
					Assert.AreEqual (ch.ToString (), encodedValue, "#C1-" + i.ToString ());
			}
		}

		Dictionary<char, string> headerNameEncodedChars = new Dictionary<char, string> {
			{'\u0000', "%00"}, {'\u0001', "%01"}, {'\u0002', "%02"}, {'\u0003', "%03"}, {'\u0004', "%04"}, 
			{'\u0005', "%05"}, {'\u0006', "%06"}, {'\u0007', "%07"}, {'\u0008', "%08"}, {'\u000A', "%0a"}, 
			{'\u000B', "%0b"}, {'\u000C', "%0c"}, {'\u000D', "%0d"}, {'\u000E', "%0e"}, {'\u000F', "%0f"}, 
			{'\u0010', "%10"}, {'\u0011', "%11"}, {'\u0012', "%12"}, {'\u0013', "%13"}, {'\u0014', "%14"}, 
			{'\u0015', "%15"}, {'\u0016', "%16"}, {'\u0017', "%17"}, {'\u0018', "%18"}, {'\u0019', "%19"}, 
			{'\u001A', "%1a"}, {'\u001B', "%1b"}, {'\u001C', "%1c"}, {'\u001D', "%1d"}, {'\u001E', "%1e"}, 
			{'\u001F', "%1f"}, {'', "%7f"}, 
		};

		Dictionary<char, string> headerValueEncodedChars = new Dictionary<char, string> {
			{'\u0000', "%00"}, {'\u0001', "%01"}, {'\u0002', "%02"}, {'\u0003', "%03"}, {'\u0004', "%04"}, 
			{'\u0005', "%05"}, {'\u0006', "%06"}, {'\u0007', "%07"}, {'\u0008', "%08"}, {'\u000A', "%0a"}, 
			{'\u000B', "%0b"}, {'\u000C', "%0c"}, {'\u000D', "%0d"}, {'\u000E', "%0e"}, {'\u000F', "%0f"}, 
			{'\u0010', "%10"}, {'\u0011', "%11"}, {'\u0012', "%12"}, {'\u0013', "%13"}, {'\u0014', "%14"}, 
			{'\u0015', "%15"}, {'\u0016', "%16"}, {'\u0017', "%17"}, {'\u0018', "%18"}, {'\u0019', "%19"}, 
			{'\u001A', "%1a"}, {'\u001B', "%1b"}, {'\u001C', "%1c"}, {'\u001D', "%1d"}, {'\u001E', "%1e"}, 
			{'\u001F', "%1f"}, {'', "%7f"}, 
		};

		#region Long arrays of strings
		string [] decoding_pairs = {
	@"&aacute;&Aacute;&acirc;&Acirc;&acute;&aelig;&AElig;&agrave;&Agrave;&alefsym;&alpha;&Alpha;&amp;&and;&ang;&aring;&Aring;&asymp;&atilde;&Atilde;&auml;&Auml;&bdquo;&beta;&Beta;&brvbar;&bull;&cap;&ccedil;&Ccedil;&cedil;&cent;&chi;&Chi;&circ;&clubs;&cong;&copy;&crarr;&cup;&curren;&dagger;&Dagger;&darr;&dArr;&deg;&delta;&Delta;&diams;&divide;&eacute;&Eacute;&ecirc;&Ecirc;&egrave;&Egrave;&empty;&emsp;&ensp;&epsilon;&Epsilon;&equiv;&eta;&Eta;&eth;&ETH;&euml;&Euml;&euro;&exist;&fnof;&forall;&frac12;&frac14;&frac34;&frasl;&gamma;&Gamma;&ge;&gt;&harr;&hArr;&hearts;&hellip;&iacute;&Iacute;&icirc;&Icirc;&iexcl;&igrave;&Igrave;&image;&infin;&int;&iota;&Iota;&iquest;&isin;&iuml;&Iuml;&kappa;&Kappa;&lambda;&Lambda;&lang;&laquo;&larr;&lArr;&lceil;&ldquo;&le;&lfloor;&lowast;&loz;&lrm;&lsaquo;&lsquo;&lt;&macr;&mdash;&micro;&middot;&minus;&mu;&Mu;&nabla;&nbsp;&ndash;&ne;&ni;&not;&notin;&nsub;&ntilde;&Ntilde;&nu;&Nu;&oacute;&Oacute;&ocirc;&Ocirc;&oelig;&OElig;&ograve;&Ograve;&oline;&omega;&Omega;&omicron;&Omicron;&oplus;&or;&ordf;&ordm;&oslash;&Oslash;&otilde;&Otilde;&otimes;&ouml;&Ouml;&para;&part;&permil;&perp;&phi;&Phi;&pi;&Pi;&piv;&plusmn;&pound;&prime;&Prime;&prod;&prop;&psi;&Psi;&quot;&radic;&rang;&raquo;&rarr;&rArr;&rceil;&rdquo;&real;&reg;&rfloor;&rho;&Rho;&rlm;&rsaquo;&rsquo;&sbquo;&scaron;&Scaron;&sdot;&sect;&shy;&sigma;&Sigma;&sigmaf;&sim;&spades;&sub;&sube;&sum;&sup;&sup1;&sup2;&sup3;&supe;&szlig;&tau;&Tau;&there4;&theta;&Theta;&thetasym;&thinsp;&thorn;&THORN;&tilde;&times;&trade;&uacute;&Uacute;&uarr;&uArr;&ucirc;&Ucirc;&ugrave;&Ugrave;&uml;&upsih;&upsilon;&Upsilon;&uuml;&Uuml;&weierp;&xi;&Xi;&yacute;&Yacute;&yen;&yuml;&Yuml;&zeta;&Zeta;&zwj;&zwnj;",
	@"áÁâÂ´æÆàÀℵαΑ&∧∠åÅ≈ãÃäÄ„βΒ¦•∩çÇ¸¢χΧˆ♣≅©↵∪¤†‡↓⇓°δΔ♦÷éÉêÊèÈ∅  εΕ≡ηΗðÐëË€∃ƒ∀½¼¾⁄γΓ≥>↔⇔♥…íÍîÎ¡ìÌℑ∞∫ιΙ¿∈ïÏκΚλΛ〈«←⇐⌈“≤⌊∗◊‎‹‘<¯—µ·−μΜ∇ –≠∋¬∉⊄ñÑνΝóÓôÔœŒòÒ‾ωΩοΟ⊕∨ªºøØõÕ⊗öÖ¶∂‰⊥φΦπΠϖ±£′″∏∝ψΨ""√〉»→⇒⌉”ℜ®⌋ρΡ‏›’‚šŠ⋅§­σΣς∼♠⊂⊆∑⊃¹²³⊇ßτΤ∴θΘϑ þÞ˜×™úÚ↑⇑ûÛùÙ¨ϒυΥüÜ℘ξΞýÝ¥ÿŸζΖ‍‌",
	@"&aacute;?dCO+6Mk'2R&Aacute;T148quH^^=972 &acirc;#&Acirc;js""{1LZz)U&acute;u@Rv-05n L&aelig;3x}&AElig;!&agrave;,=*-J*&Agrave;=P|B&alefsym;Y<g?cg>jB)&alpha;&Alpha;9#4V`)|&J/n&amp;JVK56X\2q*F&and;Js&ang;6k6&aring;""&Aring;?rGt&asymp;\F <9IM{s-&atilde;(ShK&Atilde;w/[%,ksf93'k&auml;+b$@Q{5&Auml;Uo&bdquo;aN~'ycb>VKGcjo&beta;oR8""%B`L&Beta;I7g""k5]A>^B&brvbar;lllUPg5#b&bull;8Pw,bwSiY ""5]a&cap;_R@m&D+Lz""dKLT&ccedil;KH&I}6)_Q&Ccedil;mS%BZV/*Xo&cedil;s5[&cent;-$|)|L&5~&chi;Y/3cdUrn&Chi;8&circ;&)@KU@scEW2I&clubs;p2,US7f>&m!F&cong;Fr9A%,Ci'y[]F+&copy;PY&crarr;FeCrQI<:pPP~;>&cup;&curren;y J#R&%%i&dagger;Ow,&Dagger;T&darr;KpY`WSAo$i:r&dArr;']=&deg;k12&UI@_&delta;(9xD&Delta;dz&diams;RJdB""F^Y}g&divide;2kbZ2>@yBfu&eacute;9!9J(v&Eacute;\TwTS2X5i&ecirc;SLWaTMQE]e&&Ecirc;jW{\#JAh{Ua=&egrave;5&Egrave;6/GY&empty;U&emsp;n:&ensp;dcSf&epsilon;&Epsilon;1Yoi?X&equiv;.-s!n|i9U?3:6&eta;+|6&Eta;ha?>fm!v,&eth;c;Ky]88&ETH;4T@qO#.&euml;@Kl3%&Euml;X-VvUoE& &euro;o9T:r8\||^ha;&exist;1;/BMT*xJ(a>B&fnof;bH'-TH!6NrP&forall;n&frac12;5Fqvq_e9_""XJ&frac14;vmLXTtu:TVZ,&frac34;syl;qEe:b$5j&frasl;b Hg%T&gamma;[&Gamma;H&ge;&gt;{1wT&harr;o6i~jjKC02&hArr;Q4i6m(2tpl&hearts;&#6iQj!&hellip;4le""4} Lv5{Cs&iacute;D*u]j&Iacute;s}#br=&icirc;fh&Icirc;&iexcl;_B:|&igrave;k2U7lZ;_sI\c]&Igrave;s&image; T!5h"".um9ctz&infin; YDL&int;b(S^&iota;bCm&Iota;_L(\-F&iquest;m9g.h$^HSv&isin;cWH#>&iuml;m0&Iuml;KtgRE3c5@0&&kappa;T[2?\>T^H**&Kappa;=^6 [&lambda;um&Lambda;[3wQ5gT?H(Bo\/&lang;6car8P@AjF4e|b&laquo;397jxG:m&larr;U~?~""f&lArr;`O9iwJ#&lceil;L:q-* !V&ldquo;os)Wq6S{t&le;=80A&lfloor;#tS6&lowast;x`g6a>]U-b&loz;SHb/-]&lrm;m9dm""/d<;xR)4&lsaquo;jrb/,q&lsquo;RW}n2shoM11D|&lt;{}*]WPE#d#&macr;&mdash;yhT   k&micro;&middot;`f~o&minus;{Kmf&mu;d7fmt&Mu;PT@OOrzj&nabla;y ;M01XyI:&nbsp;+l<&ndash;x5|a>62y&ne;GNKJQjmj3&ni;Az&not;?V&notin;,<&nsub;R]Lc&ntilde;kV:&Ntilde;9LLf&Z%`d-H^L&nu;v_yXht&Nu;R1yuF!&oacute;j3]zOwQf_YtT9t&Oacute;}s]&1T&ocirc;&Ocirc;2lEN&oelig;:Rp^X+tPNL.&OElig;x0 ?c3ZP&ograve;3&Ograve;&oline;@nE&omega;uK-*HjL-h5z&Omega;~x&omicron;FNQ8D#{&Omicron;Yj|]'LX&oplus;ie-Y&or;&ordf;$*.c&ordm;VM7KQ.b]hmV &oslash;x{R>J-D_0v&Oslash;Hp&otilde;L'IG&Otilde;`&otimes;E &ouml;>KNCm&Ouml;O2dH_&jd^ >2&para;U%""_n&part;U>F&permil;?TSz0~~&perp;!p@G~bH^E&phi;dg)A&Phi; J<<j_,7Q)dEs,&pi;Z&Pi;_B<@%.&?70&piv;9Y^C|VRPrb4}&plusmn;Yn=9=SQ;`}(e%&pound;y;6|RN;|w&prime;AH=XXf&Prime;&prod;DGf6ol&prop;&psi;]UXZU\vzW4&Psi;e`NY[vrvs&quot;xay&radic;[@\scKIznodD<s&rang;PB C)<itm+&raquo;{t-L&rarr;s^^x<:&sh3&rArr;p^s6Y~3Csw=&rceil;_pKnhDNTmA*p&rdquo;]yG6;,ZuPx&real;xsd&reg;`hXlUn~(pK=N:^&rfloor;OS""P{%j-Wjbx.w&rho;ts^&Rho;r$h<:u^&rlm;Vj}\?7SIauBh&rsaquo;u[ !rto/[UHog&rsquo;xe6gY<24BY.&sbquo;`ZNR}&scaron;uY{Gg;F&Scaron;&sdot;az4TlWKYbJ.h&sect;c`9FrP&shy;5_)&sigma;wx.nP}z@&Sigma;NP9-$@j5&sigmaf;&sim;'ogIt:.@Gul&spades;""p\\rH[)&sub;Om/|3G+BQe&sube;5s!f/O9SA\RJkv&sum;GOFMAXu&sup;W&sup1;&sup2;L`r""}u/n&sup3;.ouLC&supe;(f&szlig;{&tau;B%e [&Tau;$DD>kIdV#X`?^\&there4;|S?W&theta;x)2P.![^5&Theta;zqF""pj&thetasym;#BE1u?&thinsp;GGG>(EQE&thorn;!""y1r/&THORN;m&@[\mw[kNR&tilde;|1G#i[(&times;X<UotTID uY&trade;sWW+TbxY&uacute;kQXr!H6&Uacute;~0TiH1POP&uarr;(CRZttz\EY<&uArr;&bN7ki|&ucirc;r,3j!e$kJE&Z$z&Ucirc;5{0[bvD""[<P)&ugrave;;1EeRSrz/gY/&Ugrave;/1 S`I*q8:Z-&uml;%N)W&upsih;O[2P9 ?&upsilon;O&Upsilon;t&uuml;&Uuml;VLq&weierp;2""(Z'~~""uiX&xi;NCq&Xi;9)S]^v 3&yacute;x""|2&$`G&Yacute;<&Nr&yen;[3NB5f&yuml; c""MzMw3(;""s&Yuml;&zeta;{!&Zeta;oevp1'j(E`vJ&zwj;Si&zwnj;gw>yc*U",
	@"á?dCO+6Mk'2RÁT148quH^^=972 â#Âjs""{1LZz)U´u@Rv-05n Læ3x}Æ!à,=*-J*À=P|BℵY<g?cg>jB)αΑ9#4V`)|&J/n&JVK56X\2q*F∧Js∠6k6å""Å?rGt≈\F <9IM{s-ã(ShKÃw/[%,ksf93'kä+b$@Q{5ÄUo„aN~'ycb>VKGcjoβoR8""%B`LΒI7g""k5]A>^B¦lllUPg5#b•8Pw,bwSiY ""5]a∩_R@m&D+Lz""dKLTçKH&I}6)_QÇmS%BZV/*Xo¸s5[¢-$|)|L&5~χY/3cdUrnΧ8ˆ&)@KU@scEW2I♣p2,US7f>&m!F≅Fr9A%,Ci'y[]F+©PY↵FeCrQI<:pPP~;>∪¤y J#R&%%i†Ow,‡T↓KpY`WSAo$i:r⇓']=°k12&UI@_δ(9xDΔdz♦RJdB""F^Y}g÷2kbZ2>@yBfué9!9J(vÉ\TwTS2X5iêSLWaTMQE]e&ÊjW{\#JAh{Ua=è5È6/GY∅U n: dcSfεΕ1Yoi?X≡.-s!n|i9U?3:6η+|6Ηha?>fm!v,ðc;Ky]88Ð4T@qO#.ë@Kl3%ËX-VvUoE& €o9T:r8\||^ha;∃1;/BMT*xJ(a>BƒbH'-TH!6NrP∀n½5Fqvq_e9_""XJ¼vmLXTtu:TVZ,¾syl;qEe:b$5j⁄b Hg%Tγ[ΓH≥>{1wT↔o6i~jjKC02⇔Q4i6m(2tpl♥&#6iQj!…4le""4} Lv5{CsíD*u]jÍs}#br=îfhÎ¡_B:|ìk2U7lZ;_sI\c]Ìsℑ T!5h"".um9ctz∞ YDL∫b(S^ιbCmΙ_L(\-F¿m9g.h$^HSv∈cWH#>ïm0ÏKtgRE3c5@0&κT[2?\>T^H**Κ=^6 [λumΛ[3wQ5gT?H(Bo\/〈6car8P@AjF4e|b«397jxG:m←U~?~""f⇐`O9iwJ#⌈L:q-* !V“os)Wq6S{t≤=80A⌊#tS6∗x`g6a>]U-b◊SHb/-]‎m9dm""/d<;xR)4‹jrb/,q‘RW}n2shoM11D|<{}*]WPE#d#¯—yhT   kµ·`f~o−{Kmfμd7fmtΜPT@OOrzj∇y ;M01XyI: +l<–x5|a>62y≠GNKJQjmj3∋Az¬?V∉,<⊄R]LcñkV:Ñ9LLf&Z%`d-H^Lνv_yXhtΝR1yuF!ój3]zOwQf_YtT9tÓ}s]&1TôÔ2lENœ:Rp^X+tPNL.Œx0 ?c3ZPò3Ò‾@nEωuK-*HjL-h5zΩ~xοFNQ8D#{ΟYj|]'LX⊕ie-Y∨ª$*.cºVM7KQ.b]hmV øx{R>J-D_0vØHpõL'IGÕ`⊗E ö>KNCmÖO2dH_&jd^ >2¶U%""_n∂U>F‰?TSz0~~⊥!p@G~bH^Eφdg)AΦ J<<j_,7Q)dEs,πZΠ_B<@%.&?70ϖ9Y^C|VRPrb4}±Yn=9=SQ;`}(e%£y;6|RN;|w′AH=XXf″∏DGf6ol∝ψ]UXZU\vzW4Ψe`NY[vrvs""xay√[@\scKIznodD<s〉PB C)<itm+»{t-L→s^^x<:&sh3⇒p^s6Y~3Csw=⌉_pKnhDNTmA*p”]yG6;,ZuPxℜxsd®`hXlUn~(pK=N:^⌋OS""P{%j-Wjbx.wρts^Ρr$h<:u^‏Vj}\?7SIauBh›u[ !rto/[UHog’xe6gY<24BY.‚`ZNR}šuY{Gg;FŠ⋅az4TlWKYbJ.h§c`9FrP­5_)σwx.nP}z@ΣNP9-$@j5ς∼'ogIt:.@Gul♠""p\\rH[)⊂Om/|3G+BQe⊆5s!f/O9SA\RJkv∑GOFMAXu⊃W¹²L`r""}u/n³.ouLC⊇(fß{τB%e [Τ$DD>kIdV#X`?^\∴|S?Wθx)2P.![^5ΘzqF""pjϑ#BE1u? GGG>(EQEþ!""y1r/Þm&@[\mw[kNR˜|1G#i[(×X<UotTID uY™sWW+TbxYúkQXr!H6Ú~0TiH1POP↑(CRZttz\EY<⇑&bN7ki|ûr,3j!e$kJE&Z$zÛ5{0[bvD""[<P)ù;1EeRSrz/gY/Ù/1 S`I*q8:Z-¨%N)WϒO[2P9 ?υOΥtüÜVLq℘2""(Z'~~""uiXξNCqΞ9)S]^v 3ýx""|2&$`GÝ<&Nr¥[3NB5fÿ c""MzMw3(;""sŸζ{!Ζoevp1'j(E`vJ‍Si‌gw>yc*U",
	@"&aacute&Aacute&acirc&Acirc&acute&aelig&AElig&agrave&Agrave&alefsym&alpha&Alpha&amp&and&ang&aring&Aring&asymp&atilde&Atilde&auml&Auml&bdquo&beta&Beta&brvbar&bull&cap&ccedil&Ccedil&cedil&cent&chi&Chi&circ&clubs&cong&copy&crarr&cup&curren&dagger&Dagger&darr&dArr&deg&delta&Delta&diams&divide&eacute&Eacute&ecirc&Ecirc&egrave&Egrave&empty&emsp&ensp&epsilon&Epsilon&equiv&eta&Eta&eth&ETH&euml&Euml&euro&exist&fnof&forall&frac12&frac14&frac34&frasl&gamma&Gamma&ge&gt&harr&hArr&hearts&hellip&iacute&Iacute&icirc&Icirc&iexcl&igrave&Igrave&image&infin&int&iota&Iota&iquest&isin&iuml&Iuml&kappa&Kappa&lambda&Lambda&lang&laquo&larr&lArr&lceil&ldquo&le&lfloor&lowast&loz&lrm&lsaquo&lsquo&lt&macr&mdash&micro&middot&minus&mu&Mu&nabla&nbsp&ndash&ne&ni&not&notin&nsub&ntilde&Ntilde&nu&Nu&oacute&Oacute&ocirc&Ocirc&oelig&OElig&ograve&Ograve&oline&omega&Omega&omicron&Omicron&oplus&or&ordf&ordm&oslash&Oslash&otilde&Otilde&otimes&ouml&Ouml&para&part&permil&perp&phi&Phi&pi&Pi&piv&plusmn&pound&prime&Prime&prod&prop&psi&Psi&quot&radic&rang&raquo&rarr&rArr&rceil&rdquo&real&reg&rfloor&rho&Rho&rlm&rsaquo&rsquo&sbquo&scaron&Scaron&sdot&sect&shy&sigma&Sigma&sigmaf&sim&spades&sub&sube&sum&sup&sup1&sup2&sup3&supe&szlig&tau&Tau&there4&theta&Theta&thetasym&thinsp&thorn&THORN&tilde&times&trade&uacute&Uacute&uarr&uArr&ucirc&Ucirc&ugrave&Ugrave&uml&upsih&upsilon&Upsilon&uuml&Uuml&weierp&xi&Xi&yacute&Yacute&yen&yuml&Yuml&zeta&Zeta&zwj&zwnj",
	@"&aacute&Aacute&acirc&Acirc&acute&aelig&AElig&agrave&Agrave&alefsym&alpha&Alpha&amp&and&ang&aring&Aring&asymp&atilde&Atilde&auml&Auml&bdquo&beta&Beta&brvbar&bull&cap&ccedil&Ccedil&cedil&cent&chi&Chi&circ&clubs&cong&copy&crarr&cup&curren&dagger&Dagger&darr&dArr&deg&delta&Delta&diams&divide&eacute&Eacute&ecirc&Ecirc&egrave&Egrave&empty&emsp&ensp&epsilon&Epsilon&equiv&eta&Eta&eth&ETH&euml&Euml&euro&exist&fnof&forall&frac12&frac14&frac34&frasl&gamma&Gamma&ge&gt&harr&hArr&hearts&hellip&iacute&Iacute&icirc&Icirc&iexcl&igrave&Igrave&image&infin&int&iota&Iota&iquest&isin&iuml&Iuml&kappa&Kappa&lambda&Lambda&lang&laquo&larr&lArr&lceil&ldquo&le&lfloor&lowast&loz&lrm&lsaquo&lsquo&lt&macr&mdash&micro&middot&minus&mu&Mu&nabla&nbsp&ndash&ne&ni&not&notin&nsub&ntilde&Ntilde&nu&Nu&oacute&Oacute&ocirc&Ocirc&oelig&OElig&ograve&Ograve&oline&omega&Omega&omicron&Omicron&oplus&or&ordf&ordm&oslash&Oslash&otilde&Otilde&otimes&ouml&Ouml&para&part&permil&perp&phi&Phi&pi&Pi&piv&plusmn&pound&prime&Prime&prod&prop&psi&Psi&quot&radic&rang&raquo&rarr&rArr&rceil&rdquo&real&reg&rfloor&rho&Rho&rlm&rsaquo&rsquo&sbquo&scaron&Scaron&sdot&sect&shy&sigma&Sigma&sigmaf&sim&spades&sub&sube&sum&sup&sup1&sup2&sup3&supe&szlig&tau&Tau&there4&theta&Theta&thetasym&thinsp&thorn&THORN&tilde&times&trade&uacute&Uacute&uarr&uArr&ucirc&Ucirc&ugrave&Ugrave&uml&upsih&upsilon&Upsilon&uuml&Uuml&weierp&xi&Xi&yacute&Yacute&yen&yuml&Yuml&zeta&Zeta&zwj&zwnj",
	@"&#160;&#161;&#162;&#163;&#164;&#165;&#166;&#167;&#168;&#169;&#170;&#171;&#172;&#173;&#174;&#175;&#176;&#177;&#178;&#179;&#180;&#181;&#182;&#183;&#184;&#185;&#186;&#187;&#188;&#189;&#190;&#191;&#192;&#193;&#194;&#195;&#196;&#197;&#198;&#199;&#200;&#201;&#202;&#203;&#204;&#205;&#206;&#207;&#208;&#209;&#210;&#211;&#212;&#213;&#214;&#215;&#216;&#217;&#218;&#219;&#220;&#221;&#222;&#223;&#224;&#225;&#226;&#227;&#228;&#229;&#230;&#231;&#232;&#233;&#234;&#235;&#236;&#237;&#238;&#239;&#240;&#241;&#242;&#243;&#244;&#245;&#246;&#247;&#248;&#249;&#250;&#251;&#252;&#253;&#254;&#255;",
	@" ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ",
	@"&#000;&#001;&#002;&#003;&#004;&#005;&#006;&#007;&#008;&#009;&#010;&#011;&#012;&#013;&#014;&#015;&#016;&#017;&#018;&#019;&#020;&#021;&#022;&#023;&#024;&#025;&#026;&#027;&#028;&#029;&#030;&#031;&#032;",
	"&#000;\x1\x2\x3\x4\x5\x6\x7\x8\x9\xa\xb\xc\xd\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f ",
	@"&#x00;&#x01;&#x02;&#x03;&#x04;&#x05;&#x06;&#x07;&#x08;&#x09;&#x0A;&#x0B;&#x0C;&#x0D;&#x0E;&#x0F;&#x10;&#x11;&#x12;&#x13;&#x14;&#x15;&#x16;&#x17;&#x18;&#x19;&#x1A;&#x1B;&#x1C;&#x1D;&#x1E;&#x1F;&#x20;",
	"&#x00;\x1\x2\x3\x4\x5\x6\x7\x8\x9\xa\xb\xc\xd\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f ",
	@"&#xA0;&#xA1;&#xA2;&#xA3;&#xA4;&#xA5;&#xA6;&#xA7;&#xA8;&#xA9;&#xAA;&#xAB;&#xAC;&#xAD;&#xAE;&#xAF;&#xB0;&#xB1;&#xB2;&#xB3;&#xB4;&#xB5;&#xB6;&#xB7;&#xB8;&#xB9;&#xBA;&#xBB;&#xBC;&#xBD;&#xBE;&#xBF;&#xC0;&#xC1;&#xC2;&#xC3;&#xC4;&#xC5;&#xC6;&#xC7;&#xC8;&#xC9;&#xCA;&#xCB;&#xCC;&#xCD;&#xCE;&#xCF;&#xD0;&#xD1;&#xD2;&#xD3;&#xD4;&#xD5;&#xD6;&#xD7;&#xD8;&#xD9;&#xDA;&#xDB;&#xDC;&#xDD;&#xDE;&#xDF;&#xE0;&#xE1;&#xE2;&#xE3;&#xE4;&#xE5;&#xE6;&#xE7;&#xE8;&#xE9;&#xEA;&#xEB;&#xEC;&#xED;&#xEE;&#xEF;&#xF0;&#xF1;&#xF2;&#xF3;&#xF4;&#xF5;&#xF6;&#xF7;&#xF8;&#xF9;&#xFA;&#xFB;&#xFC;&#xFD;&#xFE;&#xFF;",
	" ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ",
};
		string [] encoding_pairs = {
	@"áÁâÂ´æÆàÀℵαΑ&∧∠åÅ≈ãÃäÄ„βΒ¦•∩çÇ¸¢χΧˆ♣≅©↵∪¤†‡↓⇓°δΔ♦÷éÉêÊèÈ∅  εΕ≡ηΗðÐëË€∃ƒ∀½¼¾⁄γΓ≥>↔⇔♥…íÍîÎ¡ìÌℑ∞∫ιΙ¿∈ïÏκΚλΛ〈«←⇐⌈“≤⌊∗◊‎‹‘<¯—µ·−μΜ∇ –≠∋¬∉⊄ñÑνΝóÓôÔœŒòÒ‾ωΩοΟ⊕∨ªºøØõÕ⊗öÖ¶∂‰⊥φΦπΠϖ±£′″∏∝ψΨ""√〉»→⇒⌉”ℜ®⌋ρΡ‏›’‚šŠ⋅§­σΣς∼♠⊂⊆∑⊃¹²³⊇ßτΤ∴θΘϑ þÞ˜×™úÚ↑⇑ûÛùÙ¨ϒυΥüÜ℘ξΞýÝ¥ÿŸζΖ‍‌",
	@"&#225;&#193;&#226;&#194;&#180;&#230;&#198;&#224;&#192;ℵαΑ&amp;∧∠&#229;&#197;≈&#227;&#195;&#228;&#196;„βΒ&#166;•∩&#231;&#199;&#184;&#162;χΧˆ♣≅&#169;↵∪&#164;†‡↓⇓&#176;δΔ♦&#247;&#233;&#201;&#234;&#202;&#232;&#200;∅  εΕ≡ηΗ&#240;&#208;&#235;&#203;€∃ƒ∀&#189;&#188;&#190;⁄γΓ≥&gt;↔⇔♥…&#237;&#205;&#238;&#206;&#161;&#236;&#204;ℑ∞∫ιΙ&#191;∈&#239;&#207;κΚλΛ〈&#171;←⇐⌈“≤⌊∗◊‎‹‘&lt;&#175;—&#181;&#183;−μΜ∇&#160;–≠∋&#172;∉⊄&#241;&#209;νΝ&#243;&#211;&#244;&#212;œŒ&#242;&#210;‾ωΩοΟ⊕∨&#170;&#186;&#248;&#216;&#245;&#213;⊗&#246;&#214;&#182;∂‰⊥φΦπΠϖ&#177;&#163;′″∏∝ψΨ&quot;√〉&#187;→⇒⌉”ℜ&#174;⌋ρΡ‏›’‚šŠ⋅&#167;&#173;σΣς∼♠⊂⊆∑⊃&#185;&#178;&#179;⊇&#223;τΤ∴θΘϑ &#254;&#222;˜&#215;™&#250;&#218;↑⇑&#251;&#219;&#249;&#217;&#168;ϒυΥ&#252;&#220;℘ξΞ&#253;&#221;&#165;&#255;ŸζΖ‍‌",
	@"á9cP!qdO#hU@mg1ÁK%0<}*âÂ5[Y;lfMQ$4`´uim7E`%_1zVDkæ[cM{Æt9y:E8Hb;;$;Y'àUa6wÀ<$@W9$4NL*h#'ℵk\zαG}{}hC-Α|=QhyLT%`&wB!@#x51R 4C∧]Z3n∠y>:{JZ'v|c0;N""åzcWM'z""gÅo-JX!r.e≈Z+BT{wF8+ãQ 6P1o?x""ef}vUÃ+</Nt)TI]sä0Eg_'mn&6WY[8Äay+ u[3kqoZ„i6rβUX\:_y1A^x.p>+Β`uf3/HI¦7bCRv%o$X3:•∩ç|(fgiA|MBLf=y@Ç¸¢R,qDW;F9<mχU]$)Q`w^KF^(hΧ?ukX+O!UOftˆZE♣@MLR(vcH]k8≅CU;r#(©7DZ`1>r~.↵4B&R∪+x2T`q[M-lq'¤~3rp%~-Gd†;35wU+II1tQJ‡`NGh[↓Lr>74~yHB=&EI⇓,u@Jx°δcC`2,Δo2B]6PP8♦|{!wZa&,*N'$6÷-{nVSgO]%(Ié6Éêosx-2xDI!Ê_]7Ub%èYG4`Gx{ÈH>vwMPJ∅ :Z-u#ph l,s*8(AεΕOnj|Gy|]iYLPR≡5Wi:(vZUUK.YlηDΗ6TðT!Z:Nq_0797;!Ð4]QNë9+>x9>nm-s8YËwZ}vY€:HHf∃;=0,?ƒIr`I:i5'∀z_$Q<½_sCF;=$43DpDz]¼.aMTIEwx\ogn7A¾CuJD[Hke#⁄E]M%γE:IEk}Γ{qXfzeUS≥kqW yxV>↔AzJ:$fJ⇔3IMDqU\myWjsL♥…Okíjt$NKbGrÍ""+alp<îRÎ%¡yìz2 AÌ-%;jyMK{Umdℑi|}+Za8jyWDS#I∞]NyqN*v:m-∫03Aιf9m.:+z0@OfVoΙ_gfPilLZ¿6qqb0|BQ$H%p+d∈.Wa=YBfS'd-EOïISG+=W;GHÏ3||b-icT""qAκ*/ΚλN>j}""WrqΛt]dm-Xe/v〈\«$F< X←]=8H8⇐c⌈|“JgZ)+(7,}≤s8[""3%C4JvN⌊H55TAKEZ*%Z)d.∗R9z//!q◊D`643eO‎&-L>DsUej‹C[n]Q<%UoyO‘?zUgpr+62sY<T{7n*^¯4CH]6^e/x/—uT->mQh\""µZSTN!F(U%5·17:Cu<−)*c2μTΜ%:6-e&L[ Xos/4∇]Xr 1c=qyv4HSw~HL~–{+qG?/}≠6`S"",+pL∋>¬B9∉G;6P]xc 0Bs⊄7,j0Sj2/&ñFsÑ=νKs*?[54bV1ΝQ%p6P0.Lrc`yóA/*`6sBH?67Ó&ôÔI""Hœ~e9Œ>oò5eZI}iy?}KÒS‾anD1nXωIΩu""ο:Mz$(""joU^[mΟ7M1f$j>N|Q/@(⊕de6(∨WXb<~;tI?bt#ªU:º+wb(*cA=øjb c%*?Uj6<T02Ø/A}j'MõjlfYlR~er7D@3WÕe:XTLF?|""yd7x⊗eV6Mmw2{K<lö%B%/o~r9Öc1Q TJnd^¶;∂|‰_.⊥E_bim;gvA{wqφeΦ^-!Dcπ8LB6k4PΠ(5D |Y3ϖptuh)3Mv±TAvFo+;JE,2?£""'6F9fRp′,0″<∏N∝C%}JC7qY(7))UWψ 7=rmQaΨeD!G5e>S~kO""'4""/i4\>!]H;T^0o√8_G`*8&An\rhc)〉&UEk»-(YtC→(zerUTMTe,'@{⇒mlzVhU<S,5}9DM⌉/%R=10*[{'=:”C0ℜ4HoT?-#+l[SnPs®0 bV⌋TρΡjb1}OJ:,0z6‏oTxP""""FOT[;›'’-:Ll)I0^$p.‚S_šNBr9)K[Š1⋅$-S4/G&u§= _CqlY1O'­qNf|&σGp}ΣP3:8ς∼[ItI♠8⊂BQn~!KO:+~ma⊆FV.u 4wD∑lE+kQ|gZ];Y⊃DK69EEM$D¹KVO²%:~Iq?IUcHr4y³QP@R't!⊇vßYnI@FXxT<τvL[4H95mfΤF0JzQsrxNZry∴Bn#t(θ*OΘw=Z%ϑ+*l^3C)5HCNmR  %`g|*8DECþ_[Þ'8,?˜}gnaz_U×-F^™9ZDO86ú]y\ecHQSÚk-07/AT|0Ce↑F⇑*}e|r$6ln!V`ûA!*8H,mÛ~6G6w&GùsPL6ÙQ¨}J^NO}=._Mnϒ{&υ=ΥWD+f>fy|nNyP*Jüo8,lh\ÜN`'g℘(sJ8h3P]cF ξcdQ_OC]U#ΞBby=Sý9tI_Ý}p(D51=X¥cH8L)$*]~=IÿdbŸf>J^1Dnζ@(drH;91?{6`xJΖ4N4[u+5‍9.W\v‌]GGtKvCC0`A",
	@"&#225;9cP!qdO#hU@mg1&#193;K%0&lt;}*&#226;&#194;5[Y;lfMQ$4`&#180;uim7E`%_1zVDk&#230;[cM{&#198;t9y:E8Hb;;$;Y&#39;&#224;Ua6w&#192;&lt;$@W9$4NL*h#&#39;ℵk\zαG}{}hC-Α|=QhyLT%`&amp;wB!@#x51R 4C∧]Z3n∠y&gt;:{JZ&#39;v|c0;N&quot;&#229;zcWM&#39;z&quot;g&#197;o-JX!r.e≈Z+BT{wF8+&#227;Q 6P1o?x&quot;ef}vU&#195;+&lt;/Nt)TI]s&#228;0Eg_&#39;mn&amp;6WY[8&#196;ay+ u[3kqoZ„i6rβUX\:_y1A^x.p&gt;+Β`uf3/HI&#166;7bCRv%o$X3:•∩&#231;|(fgiA|MBLf=y@&#199;&#184;&#162;R,qDW;F9&lt;mχU]$)Q`w^KF^(hΧ?ukX+O!UOftˆZE♣@MLR(vcH]k8≅CU;r#(&#169;7DZ`1&gt;r~.↵4B&amp;R∪+x2T`q[M-lq&#39;&#164;~3rp%~-Gd†;35wU+II1tQJ‡`NGh[↓Lr&gt;74~yHB=&amp;EI⇓,u@Jx&#176;δcC`2,Δo2B]6PP8♦|{!wZa&amp;,*N&#39;$6&#247;-{nVSgO]%(I&#233;6&#201;&#234;osx-2xDI!&#202;_]7Ub%&#232;YG4`Gx{&#200;H&gt;vwMPJ∅ :Z-u#ph l,s*8(AεΕOnj|Gy|]iYLPR≡5Wi:(vZUUK.YlηDΗ6T&#240;T!Z:Nq_0797;!&#208;4]QN&#235;9+&gt;x9&gt;nm-s8Y&#203;wZ}vY€:HHf∃;=0,?ƒIr`I:i5&#39;∀z_$Q&lt;&#189;_sCF;=$43DpDz]&#188;.aMTIEwx\ogn7A&#190;CuJD[Hke#⁄E]M%γE:IEk}Γ{qXfzeUS≥kqW yxV&gt;↔AzJ:$fJ⇔3IMDqU\myWjsL♥…Ok&#237;jt$NKbGr&#205;&quot;+alp&lt;&#238;R&#206;%&#161;y&#236;z2 A&#204;-%;jyMK{Umdℑi|}+Za8jyWDS#I∞]NyqN*v:m-∫03Aιf9m.:+z0@OfVoΙ_gfPilLZ&#191;6qqb0|BQ$H%p+d∈.Wa=YBfS&#39;d-EO&#239;ISG+=W;GH&#207;3||b-icT&quot;qAκ*/ΚλN&gt;j}&quot;WrqΛt]dm-Xe/v〈\&#171;$F&lt; X←]=8H8⇐c⌈|“JgZ)+(7,}≤s8[&quot;3%C4JvN⌊H55TAKEZ*%Z)d.∗R9z//!q◊D`643eO‎&amp;-L&gt;DsUej‹C[n]Q&lt;%UoyO‘?zUgpr+62sY&lt;T{7n*^&#175;4CH]6^e/x/—uT-&gt;mQh\&quot;&#181;ZSTN!F(U%5&#183;17:Cu&lt;−)*c2μTΜ%:6-e&amp;L[ Xos/4∇]Xr&#160;1c=qyv4HSw~HL~–{+qG?/}≠6`S&quot;,+pL∋&gt;&#172;B9∉G;6P]xc 0Bs⊄7,j0Sj2/&amp;&#241;Fs&#209;=νKs*?[54bV1ΝQ%p6P0.Lrc`y&#243;A/*`6sBH?67&#211;&amp;&#244;&#212;I&quot;Hœ~e9Œ&gt;o&#242;5eZI}iy?}K&#210;S‾anD1nXωIΩu&quot;ο:Mz$(&quot;joU^[mΟ7M1f$j&gt;N|Q/@(⊕de6(∨WXb&lt;~;tI?bt#&#170;U:&#186;+wb(*cA=&#248;jb c%*?Uj6&lt;T02&#216;/A}j&#39;M&#245;jlfYlR~er7D@3W&#213;e:XTLF?|&quot;yd7x⊗eV6Mmw2{K&lt;l&#246;%B%/o~r9&#214;c1Q TJnd^&#182;;∂|‰_.⊥E_bim;gvA{wqφeΦ^-!Dcπ8LB6k4PΠ(5D |Y3ϖptuh)3Mv&#177;TAvFo+;JE,2?&#163;&quot;&#39;6F9fRp′,0″&lt;∏N∝C%}JC7qY(7))UWψ 7=rmQaΨeD!G5e&gt;S~kO&quot;&#39;4&quot;/i4\&gt;!]H;T^0o√8_G`*8&amp;An\rhc)〉&amp;UEk&#187;-(YtC→(zerUTMTe,&#39;@{⇒mlzVhU&lt;S,5}9DM⌉/%R=10*[{&#39;=:”C0ℜ4HoT?-#+l[SnPs&#174;0 bV⌋TρΡjb1}OJ:,0z6‏oTxP&quot;&quot;FOT[;›&#39;’-:Ll)I0^$p.‚S_šNBr9)K[Š1⋅$-S4/G&amp;u&#167;= _CqlY1O&#39;&#173;qNf|&amp;σGp}ΣP3:8ς∼[ItI♠8⊂BQn~!KO:+~ma⊆FV.u 4wD∑lE+kQ|gZ];Y⊃DK69EEM$D&#185;KVO&#178;%:~Iq?IUcHr4y&#179;QP@R&#39;t!⊇v&#223;YnI@FXxT&lt;τvL[4H95mfΤF0JzQsrxNZry∴Bn#t(θ*OΘw=Z%ϑ+*l^3C)5HCNmR  %`g|*8DEC&#254;_[&#222;&#39;8,?˜}gnaz_U&#215;-F^™9ZDO86&#250;]y\ecHQS&#218;k-07/AT|0Ce↑F⇑*}e|r$6ln!V`&#251;A!*8H,m&#219;~6G6w&amp;G&#249;sPL6&#217;Q&#168;}J^NO}=._Mnϒ{&amp;υ=ΥWD+f&gt;fy|nNyP*J&#252;o8,lh\&#220;N`&#39;g℘(sJ8h3P]cF ξcdQ_OC]U#ΞBby=S&#253;9tI_&#221;}p(D51=X&#165;cH8L)$*]~=I&#255;dbŸf&gt;J^1Dnζ@(drH;91?{6`xJΖ4N4[u+5‍9.W\v‌]GGtKvCC0`A",
	@"&aacute&Aacute&acirc&Acirc&acute&aelig&AElig&agrave&Agrave&alefsym&alpha&Alpha&amp&and&ang&aring&Aring&asymp&atilde&Atilde&auml&Auml&bdquo&beta&Beta&brvbar&bull&cap&ccedil&Ccedil&cedil&cent&chi&Chi&circ&clubs&cong&copy&crarr&cup&curren&dagger&Dagger&darr&dArr&deg&delta&Delta&diams&divide&eacute&Eacute&ecirc&Ecirc&egrave&Egrave&empty&emsp&ensp&epsilon&Epsilon&equiv&eta&Eta&eth&ETH&euml&Euml&euro&exist&fnof&forall&frac12&frac14&frac34&frasl&gamma&Gamma&ge&gt&harr&hArr&hearts&hellip&iacute&Iacute&icirc&Icirc&iexcl&igrave&Igrave&image&infin&int&iota&Iota&iquest&isin&iuml&Iuml&kappa&Kappa&lambda&Lambda&lang&laquo&larr&lArr&lceil&ldquo&le&lfloor&lowast&loz&lrm&lsaquo&lsquo&lt&macr&mdash&micro&middot&minus&mu&Mu&nabla&nbsp&ndash&ne&ni&not&notin&nsub&ntilde&Ntilde&nu&Nu&oacute&Oacute&ocirc&Ocirc&oelig&OElig&ograve&Ograve&oline&omega&Omega&omicron&Omicron&oplus&or&ordf&ordm&oslash&Oslash&otilde&Otilde&otimes&ouml&Ouml&para&part&permil&perp&phi&Phi&pi&Pi&piv&plusmn&pound&prime&Prime&prod&prop&psi&Psi&quot&radic&rang&raquo&rarr&rArr&rceil&rdquo&real&reg&rfloor&rho&Rho&rlm&rsaquo&rsquo&sbquo&scaron&Scaron&sdot&sect&shy&sigma&Sigma&sigmaf&sim&spades&sub&sube&sum&sup&sup1&sup2&sup3&supe&szlig&tau&Tau&there4&theta&Theta&thetasym&thinsp&thorn&THORN&tilde&times&trade&uacute&Uacute&uarr&uArr&ucirc&Ucirc&ugrave&Ugrave&uml&upsih&upsilon&Upsilon&uuml&Uuml&weierp&xi&Xi&yacute&Yacute&yen&yuml&Yuml&zeta&Zeta&zwj&zwnj",
	@"&amp;aacute&amp;Aacute&amp;acirc&amp;Acirc&amp;acute&amp;aelig&amp;AElig&amp;agrave&amp;Agrave&amp;alefsym&amp;alpha&amp;Alpha&amp;amp&amp;and&amp;ang&amp;aring&amp;Aring&amp;asymp&amp;atilde&amp;Atilde&amp;auml&amp;Auml&amp;bdquo&amp;beta&amp;Beta&amp;brvbar&amp;bull&amp;cap&amp;ccedil&amp;Ccedil&amp;cedil&amp;cent&amp;chi&amp;Chi&amp;circ&amp;clubs&amp;cong&amp;copy&amp;crarr&amp;cup&amp;curren&amp;dagger&amp;Dagger&amp;darr&amp;dArr&amp;deg&amp;delta&amp;Delta&amp;diams&amp;divide&amp;eacute&amp;Eacute&amp;ecirc&amp;Ecirc&amp;egrave&amp;Egrave&amp;empty&amp;emsp&amp;ensp&amp;epsilon&amp;Epsilon&amp;equiv&amp;eta&amp;Eta&amp;eth&amp;ETH&amp;euml&amp;Euml&amp;euro&amp;exist&amp;fnof&amp;forall&amp;frac12&amp;frac14&amp;frac34&amp;frasl&amp;gamma&amp;Gamma&amp;ge&amp;gt&amp;harr&amp;hArr&amp;hearts&amp;hellip&amp;iacute&amp;Iacute&amp;icirc&amp;Icirc&amp;iexcl&amp;igrave&amp;Igrave&amp;image&amp;infin&amp;int&amp;iota&amp;Iota&amp;iquest&amp;isin&amp;iuml&amp;Iuml&amp;kappa&amp;Kappa&amp;lambda&amp;Lambda&amp;lang&amp;laquo&amp;larr&amp;lArr&amp;lceil&amp;ldquo&amp;le&amp;lfloor&amp;lowast&amp;loz&amp;lrm&amp;lsaquo&amp;lsquo&amp;lt&amp;macr&amp;mdash&amp;micro&amp;middot&amp;minus&amp;mu&amp;Mu&amp;nabla&amp;nbsp&amp;ndash&amp;ne&amp;ni&amp;not&amp;notin&amp;nsub&amp;ntilde&amp;Ntilde&amp;nu&amp;Nu&amp;oacute&amp;Oacute&amp;ocirc&amp;Ocirc&amp;oelig&amp;OElig&amp;ograve&amp;Ograve&amp;oline&amp;omega&amp;Omega&amp;omicron&amp;Omicron&amp;oplus&amp;or&amp;ordf&amp;ordm&amp;oslash&amp;Oslash&amp;otilde&amp;Otilde&amp;otimes&amp;ouml&amp;Ouml&amp;para&amp;part&amp;permil&amp;perp&amp;phi&amp;Phi&amp;pi&amp;Pi&amp;piv&amp;plusmn&amp;pound&amp;prime&amp;Prime&amp;prod&amp;prop&amp;psi&amp;Psi&amp;quot&amp;radic&amp;rang&amp;raquo&amp;rarr&amp;rArr&amp;rceil&amp;rdquo&amp;real&amp;reg&amp;rfloor&amp;rho&amp;Rho&amp;rlm&amp;rsaquo&amp;rsquo&amp;sbquo&amp;scaron&amp;Scaron&amp;sdot&amp;sect&amp;shy&amp;sigma&amp;Sigma&amp;sigmaf&amp;sim&amp;spades&amp;sub&amp;sube&amp;sum&amp;sup&amp;sup1&amp;sup2&amp;sup3&amp;supe&amp;szlig&amp;tau&amp;Tau&amp;there4&amp;theta&amp;Theta&amp;thetasym&amp;thinsp&amp;thorn&amp;THORN&amp;tilde&amp;times&amp;trade&amp;uacute&amp;Uacute&amp;uarr&amp;uArr&amp;ucirc&amp;Ucirc&amp;ugrave&amp;Ugrave&amp;uml&amp;upsih&amp;upsilon&amp;Upsilon&amp;uuml&amp;Uuml&amp;weierp&amp;xi&amp;Xi&amp;yacute&amp;Yacute&amp;yen&amp;yuml&amp;Yuml&amp;zeta&amp;Zeta&amp;zwj&amp;zwnj",
	@" ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ",
	@"&#160;&#161;&#162;&#163;&#164;&#165;&#166;&#167;&#168;&#169;&#170;&#171;&#172;&#173;&#174;&#175;&#176;&#177;&#178;&#179;&#180;&#181;&#182;&#183;&#184;&#185;&#186;&#187;&#188;&#189;&#190;&#191;&#192;&#193;&#194;&#195;&#196;&#197;&#198;&#199;&#200;&#201;&#202;&#203;&#204;&#205;&#206;&#207;&#208;&#209;&#210;&#211;&#212;&#213;&#214;&#215;&#216;&#217;&#218;&#219;&#220;&#221;&#222;&#223;&#224;&#225;&#226;&#227;&#228;&#229;&#230;&#231;&#232;&#233;&#234;&#235;&#236;&#237;&#238;&#239;&#240;&#241;&#242;&#243;&#244;&#245;&#246;&#247;&#248;&#249;&#250;&#251;&#252;&#253;&#254;&#255;",
	"&#000;\x1\x2\x3\x4\x5\x6\x7\x8\x9\xa\xb\xc\xd\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f ",
	"&amp;#000;\x1\x2\x3\x4\x5\x6\x7\x8\x9\xa\xb\xc\xd\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f ",
	"&#x00;\x1\x2\x3\x4\x5\x6\x7\x8\x9\xa\xb\xc\xd\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f ",
	"&amp;#x00;\x1\x2\x3\x4\x5\x6\x7\x8\x9\xa\xb\xc\xd\xe\xf\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f ",
	@" ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ",
	@"&#160;&#161;&#162;&#163;&#164;&#165;&#166;&#167;&#168;&#169;&#170;&#171;&#172;&#173;&#174;&#175;&#176;&#177;&#178;&#179;&#180;&#181;&#182;&#183;&#184;&#185;&#186;&#187;&#188;&#189;&#190;&#191;&#192;&#193;&#194;&#195;&#196;&#197;&#198;&#199;&#200;&#201;&#202;&#203;&#204;&#205;&#206;&#207;&#208;&#209;&#210;&#211;&#212;&#213;&#214;&#215;&#216;&#217;&#218;&#219;&#220;&#221;&#222;&#223;&#224;&#225;&#226;&#227;&#228;&#229;&#230;&#231;&#232;&#233;&#234;&#235;&#236;&#237;&#238;&#239;&#240;&#241;&#242;&#243;&#244;&#245;&#246;&#247;&#248;&#249;&#250;&#251;&#252;&#253;&#254;&#255;",
};
		#endregion
	}
}
