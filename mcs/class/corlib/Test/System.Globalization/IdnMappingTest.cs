//
// IdnMappingTest.cs
//
// Author:
// 	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
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


using NUnit.Framework;
using System.IO;
using System;
using System.Globalization;
using System.Threading;
using System.Text;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	public class IdnMappingTest
	{
		void GetAscii (IdnMapping m, string source, string expected, object label)
		{
			Assert.AreEqual (expected, m.GetAscii (source), label != null ? label.ToString () : expected);
		}

		void GetAsciiInvalid (IdnMapping m, string s, object label)
		{
			try {
				m.GetAscii (s);
				Assert.Fail (label != null ? label.ToString () + ":" + s : s);
			} catch (ArgumentException) {
			}
		}

		void GetUnicode (IdnMapping m, string source, string expected, object label)
		{
			Assert.AreEqual (expected, m.GetUnicode (source), label != null ? label.ToString () : expected);
		}

		void GetUnicodeInvalid (IdnMapping m, string s, object label)
		{
			try {
				m.GetUnicode (s);
				Assert.Fail (label != null ? label.ToString () + ":" + s : s);
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void GetAsciiInvalid ()
		{
			// hmm... according to RFC 3490, there are couple of
			// invalid characters in RFC 3491 [Nameprop] ...
			// comment out them for now.

			IdnMapping m = new IdnMapping ();
			GetAsciiInvalid (m, "\x0", 1);
			GetAsciiInvalid (m, "\xD\xA", 2);
			GetAsciiInvalid (m, "\x1F", 3);
			GetAsciiInvalid (m, "\x7F", 4);
			GetAsciiInvalid (m, "\x80", 5);
			// GetAsciiInvalid (m, "\xA0", 6);
			// GetAsciiInvalid (m, "\u1680", 7);
			GetAsciiInvalid (m, "\u200E", 8);
			// GetAsciiInvalid (m, "\u0341", 9);
			GetAsciiInvalid (m, "\uE000", 10);
			GetAsciiInvalid (m, "\uFFFF", 11);
			GetAsciiInvalid (m, "\u2028", 12);
			GetAsciiInvalid (m, "\uD800", 13);
			// GetAsciiInvalid (m, "\u3000", 14);
		}

		[Test]
		public void GetAscii ()
		{
			IdnMapping m = new IdnMapping ();
			GetAscii (m, "www.example.com", "www.example.com", 1);
			GetAscii (m, "a\uFF61b", "a.b", 2);

			// umm... does it reject anything larger than U+FFFF ??
			// GetAscii (m, "\uD800\uDC00", "", 3);

			// hmm, according to RFC 3490, ToASCII never changes
			// the input whose codepoints are all within ASCII.
			GetAscii (m, "A\uFF61B", "a.b", 4);
			GetAscii (m, "A.B", "A.B", 5); // no lowercasing
		}

		[Test]
		public void Dots ()
		{
			IdnMapping m = new IdnMapping ();
			GetAsciiInvalid (m, ".", 1);
			GetAsciiInvalid (m, "\uFF61", 2);
			GetAscii (m, "Am running now.", "Am running now.", 3);
			GetAsciiInvalid (m, ".bashrc", 4);
		}

		[Test]
		public void UnassignedCharacter ()
		{
			IdnMapping m = new IdnMapping ();
			//GetAsciiInvalid (m, "\u18b0.com", 1);
			m.AllowUnassigned = true;
			GetAscii (m, "\u18b0.com", "xn--6bf.com", 2);
		}

		[Test]
		public void UseStd3AsciiRules ()
		{
			IdnMapping m = new IdnMapping ();
			GetAscii (m, "-_-.com", "-_-.com", 1);
			m.UseStd3AsciiRules = true;
			GetAsciiInvalid (m, "a b.com", 1.5);
			GetAsciiInvalid (m, "_.com", 2);
			GetAsciiInvalid (m, "-abc.com", 3);
			GetAscii (m, "abc-def.com", "abc-def.com", 4); // hyphen in the middle is okay
			GetAsciiInvalid (m, "abc-.def.com", 5); // hyphen is *not* in the middle.
		}

		[Test]
		public void AcePrefix ()
		{
			IdnMapping m = new IdnMapping ();
			GetAscii (m, "bl--.com", "bl--.com", 1); // only ascii
			GetAscii (m, "bl--\xC0.com", "xn--bl---3na.com", 2);
			GetAscii (m, "xn--.com", "xn--.com", 3); // only ascii
			GetAsciiInvalid (m, "xn--\xC0.com", 4);
			GetAsciiInvalid (m, "xN--\xC0.com", 5);
			GetAsciiInvalid (m, "Xn--\xC0.com", 6);
			GetAsciiInvalid (m, "XN--\xC0.com", 7);
			GetAscii (m, "xN\xC0.com", "xn--xn-kia.com", 8);
			GetAscii (m, "bl--\xC0.wl--\xC0.com", "xn--bl---3na.xn--wl---3na.com", 9);
		}

		[Test]
		public void GetAsciiRFC3492Examples ()
		{
			IdnMapping m = new IdnMapping ();
			// 3<nen>B<gumi><kin><pachi><sen><sei>
			GetAscii (m, "3\u5E74B\u7D44\u91D1\u516B\u5148\u751F", "xn--3b-ww4c5e180e575a65lsy2b", "(L)");
			// Maji<de>Koi<suru>5<byoumae>
			GetAscii (m, "Maji\u3067Koi\u3059\u308B5\u79D2\u524D", "xn--majikoi5-783gue6qz075azm5e", "(P)");
		}

		[Test]
		public void GetUnicode ()
		{
			IdnMapping m = new IdnMapping ();
			GetAscii (m, "www.example.com", "www.example.com", 1);
			GetAscii (m, "a\uFF61b", "a.b", 2);

			// umm... does it reject anything larger than U+FFFF ??
			// GetAscii (m, "\uD800\uDC00", "", 3);

			// hmm, according to RFC 3490, ToASCII never changes
			// the input whose codepoints are all within ASCII.
			GetAscii (m, "A\uFF61B", "a.b", 4);
			GetAscii (m, "A.B", "A.B", 5); // no lowercasing
		}

		[Test]
		public void GetUnicodeRFC3492Examples ()
		{
			// uppercases in the ASCII strings are ignored.
			IdnMapping m = new IdnMapping ();
			// 3<nen>B<gumi><kin><pachi><sen><sei>
			GetUnicode (m, "xn--3B-ww4c5e180e575a65lsy2b", "3\u5E74b\u7D44\u91D1\u516B\u5148\u751F", "(L)");
			// Maji<de>Koi<suru>5<byoumae>
			GetUnicode (m, "xn--MajiKoi5-783gue6qz075azm5e", "maji\u3067koi\u3059\u308B5\u79D2\u524D", "(P)");
		}
	}
}

