//
// IdnMapping.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

/*

** related RFCs

	RFC 3490: IDNA
	RFC 3491: Nameprep
	RFC 3492: Punycode
	RFC 3454: STRINGPREP

Prohibited in [Nameprep]: C.1.2, C.2.2, C.3 - C.9 in [STRINGPREP]

	C.1.2 non-ascii spaces (00A0, 1680, 2000-200B, 202F, 205F, 3000)
	C.2.2 non-ascii controls (0080-009F, 06DD, 070F, 180E, 200C, 200D,
	      2028, 2029, 2060-2063, 206A-206F, FEFF, FFF9-FFFC, 1D173-1D17A)
	C.3 private use (E000-F8FF, F0000-FFFFD, 100000-10FFFD)
	C.4 non-characters (FDD0-FDEF, FFFE-FFFF, nFFFE-nFFFF)
	C.5 surrogate code (D800-DFFF)
	C.6 inappropriate for plain text (FFF9-FFFD)
	C.7 inappropriate for canonical representation (2FF0-2FFB)
	C.8 change display properties or are deprecated (0340, 0341,
		200E, 200F, 202A-202E, 206A-206F)
	C.9 tagging characters (E0001, E0020-E007F)

*/

using System;
using System.Text;

namespace System.Globalization
{
	public sealed class IdnMapping
	{
		bool allow_unassigned, use_std3;
		Punycode puny = new Punycode ();

		public IdnMapping ()
		{
		}

		public bool AllowUnassigned {
			get { return allow_unassigned; }
			set { allow_unassigned = value; }
		}

		public bool UseStd3AsciiRules {
			get { return use_std3; }
			set { use_std3 = value; }
		}

		public override bool Equals (object obj)
		{
			IdnMapping other = obj as IdnMapping;
			return other != null &&
			       allow_unassigned == other.allow_unassigned &&
			       use_std3 == other.use_std3;
		}

		public override int GetHashCode ()
		{
			return (allow_unassigned ? 2 : 0) + (use_std3 ? 1 : 0);
		}

		#region GetAscii

		public string GetAscii (string unicode)
		{
			if (unicode == null)
				throw new ArgumentNullException ("unicode");
			return GetAscii (unicode, 0, unicode.Length);
		}

		public string GetAscii (string unicode, int index)
		{
			if (unicode == null)
				throw new ArgumentNullException ("unicode");
			return GetAscii (unicode, index, unicode.Length - index);
		}

		public string GetAscii (string unicode, int index, int count)
		{
			if (unicode == null)
				throw new ArgumentNullException ("unicode");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index must be non-negative value");
			if (count < 0 || index + count > unicode.Length)
				throw new ArgumentOutOfRangeException ("index + count must point inside the argument unicode string");

			return Convert (unicode, index, count, true);
		}

		string Convert (string input, int index, int count, bool toAscii)
		{
			string s = input.Substring (index, count);

			// Actually lowering string is done as part of
			// Nameprep(), but it is much easier to do it in prior.
			for (int i = 0; i < s.Length; i++)
				if (s [i] >= '\x80') {
					s = s.ToLower (CultureInfo.InvariantCulture);
					break;
				}

			// RFC 3490 section 4. and 4.1
			// 1) -> done as AllowUnassigned property
			// 2) split the input
			string [] labels = s.Split ('.', '\u3002', '\uFF0E', '\uFF61');
			int iter = 0;
			for (int i = 0; i < labels.Length; iter += labels [i].Length, i++) {
				// 3) -> done as UseStd3AsciiRules property
				// 4) ToAscii
				if (labels [i].Length == 0 && i + 1 == labels.Length)
					// If the input ends with '.', Split()
					// adds another empty string. In that
					// case, we have to ignore it.
					continue;
				if (toAscii)
					labels [i] = ToAscii (labels [i], iter);
				else
					labels [i] = ToUnicode (labels [i], iter);
			}
			// 5) join them
			return String.Join (".", labels);
		}

		string ToAscii (string s, int offset)
		{
			// 1.
			for (int i = 0; i < s.Length; i++) {
				// I wonder if this check is really RFC-conformant
				if (s [i] < '\x20' || s [i] == '\x7F')
					throw new ArgumentException (String.Format ("Not allowed character was found, at {0}", offset + i));
				if (s [i] >= 0x80) {
					// 2.
					s = NamePrep (s, offset);
					break;
				}
			}

			// 3.
			if (use_std3)
				VerifyStd3AsciiRules (s, offset);

			// 4.
			for (int i = 0; i < s.Length; i++) {
				if (s [i] >= 0x80) {
					// 5. check ACE.
					if (s.StartsWith ("xn--", StringComparison.OrdinalIgnoreCase))
						throw new ArgumentException (String.Format ("The input string must not start with ACE (xn--), at {0}", offset + i));
					// 6. Punycode it.
					s = puny.Encode (s, offset);
					// 7. prepend ACE.
					s = "xn--" + s;
					break;
				}
			}

			// 8.
			VerifyLength (s, offset);

			return s;
		}

		void VerifyLength (string s, int offset)
		{
			if (s.Length == 0)
				throw new ArgumentException (String.Format ("A label in the input string resulted in an invalid zero-length string, at {0}", offset));
			if (s.Length > 63)
				throw new ArgumentException (String.Format ("A label in the input string exceeded the length in ASCII representation, at {0}", offset));
		}

		string NamePrep (string s, int offset)
		{
			s = s.Normalize (NormalizationForm.FormKC);
			VerifyProhibitedCharacters (s, offset);
			// FIXME: check BIDI

			if (!allow_unassigned) {
				for (int i = 0; i < s.Length; i++)
					if (Char.GetUnicodeCategory (s, i) == UnicodeCategory.OtherNotAssigned)
						throw new ArgumentException (String.Format ("Use of unassigned Unicode characer is prohibited in this IdnMapping, at {0}", offset + i));
			}
			return s;
		}

		void VerifyProhibitedCharacters (string s, int offset)
		{
			for (int i = 0; i < s.Length; i++) {
				switch (Char.GetUnicodeCategory (s, i)) {
				case UnicodeCategory.SpaceSeparator:
					if (s [i] < '\x80')
						continue; // valid
					break;
				case UnicodeCategory.Control:
					if (s [i] != '\x0' && s [i] < '\x80')
						continue; // valid
					break;
				case UnicodeCategory.PrivateUse:
				case UnicodeCategory.Surrogate:
					break;
				default:
					char c = s [i];
					if (// C.4
					    '\uFDDF' <= c && c <= '\uFDEF' ||
					    ((int) c & 0xFFFF) == 0xFFFE ||
					    // C.6
					    '\uFFF9' <= c && c <= '\uFFFD' ||
					    // C.7
					    '\u2FF0' <= c && c <= '\u2FFB' ||
					    // C.8
					    '\u202A' <= c && c <= '\u202E' ||
					    '\u206A' <= c && c <= '\u206F')
						break;
					switch (c) {
					// C.8
					case '\u0340':
					case '\u0341':
					case '\u200E':
					case '\u200F':
					// C.2.2
					case '\u2028':
					case '\u2029':
						break;
					default:
						continue;
					}
					break;
				}
				throw new ArgumentException (String.Format ("Not allowed character was in the input string, at {0}", offset + i));
			}
		}

		void VerifyStd3AsciiRules (string s, int offset)
		{
			if (s.Length > 0 && s [0] == '-')
				throw new ArgumentException (String.Format ("'-' is not allowed at head of a sequence in STD3 mode, found at {0}", offset));
			if (s.Length > 0 && s [s.Length - 1] == '-')
				throw new ArgumentException (String.Format ("'-' is not allowed at tail of a sequence in STD3 mode, found at {0}", offset + s.Length - 1));

			for (int i = 0; i < s.Length; i++) {
				char c = s [i];
				if (c == '-')
					continue;
				if (c <= '\x2F' || '\x3A' <= c && c <= '\x40' || '\x5B' <= c && c <= '\x60' || '\x7B' <= c && c <= '\x7F')
					throw new ArgumentException (String.Format ("Not allowed character in STD3 mode, found at {0}", offset + i));
			}
		}

		#endregion

		public string GetUnicode (string ascii)
		{
			if (ascii == null)
				throw new ArgumentNullException ("ascii");
			return GetUnicode (ascii, 0, ascii.Length);
		}

		public string GetUnicode (string ascii, int index)
		{
			if (ascii == null)
				throw new ArgumentNullException ("ascii");
			return GetUnicode (ascii, index, ascii.Length - index);
		}

		public string GetUnicode (string ascii, int index, int count)
		{
			if (ascii == null)
				throw new ArgumentNullException ("ascii");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index must be non-negative value");
			if (count < 0 || index + count > ascii.Length)
				throw new ArgumentOutOfRangeException ("index + count must point inside the argument ascii string");

			return Convert (ascii, index, count, false);
		}

		string ToUnicode (string s, int offset)
		{
			// 1.
			for (int i = 0; i < s.Length; i++) {
				if (s [i] >= 0x80) {
					// 2.
					s = NamePrep (s, offset);
					break;
				}
			}

			// 3.
			if (!s.StartsWith ("xn--", StringComparison.OrdinalIgnoreCase))
				return s; // failure = return the input string as is.
			// Actually lowering string is done as part of
			// Nameprep(), but it is much easier to do it in prior.
			s = s.ToLower (CultureInfo.InvariantCulture);

			string at3 = s;

			// 4.
			s = s.Substring (4);

			// 5.
			s = puny.Decode (s, offset);
			string at5 = s;

			// 6.
			s = ToAscii (s, offset);

			// 7.
			if (String.Compare (at3, s, StringComparison.OrdinalIgnoreCase) != 0)
				throw new ArgumentException (String.Format ("ToUnicode() failed at verifying the result, at label part from {0}", offset));

			// 8.
			return at5;
		}
	}

	class Bootstring
	{
		readonly char delimiter;
		readonly int base_num, tmin, tmax, skew, damp, initial_bias, initial_n;
		
		public Bootstring (char delimiter,
				 int baseNum, int tmin, int tmax,
				 int skew, int damp,
				 int initialBias, int initialN)
		{
			this.delimiter = delimiter;
			base_num = baseNum;
			this.tmin = tmin;
			this.tmax = tmax;
			this.skew = skew;
			this.damp = damp;
			initial_bias = initialBias;
			initial_n = initialN;
		}

		public string Encode (string s, int offset)
		{
			int n = initial_n;
			int delta = 0;
			int bias = initial_bias;
			int b = 0, h = 0;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < s.Length; i++)
				if (s [i] < '\x80')
					sb.Append (s [i]);
			b = h = sb.Length;
			if (b > 0)
				sb.Append (delimiter);

			while (h < s.Length) {
				int m = int.MaxValue;
				for (int i = 0; i < s.Length; i++)
					if (s [i] >= n && s [i] < m)
						m = s [i];
				checked { delta += (m - n) * (h + 1); }
				n = m;
				for (int i = 0; i < s.Length; i++) {
					char c = s [i];
					if (c < n || c < '\x80')
						checked { delta++; }
					if (c == n) {
						int q = delta;
						for (int k = base_num; ;k += base_num) {
							int t =
								k <= bias + tmin ? tmin :
								k >= bias + tmax ? tmax :
								k - bias;
							if (q < t)
								break;
							sb.Append (EncodeDigit (t + (q - t) % (base_num - t)));
							q = (q - t) / (base_num - t);
						}
						sb.Append (EncodeDigit (q));
						bias = Adapt (delta, h + 1, h == b);
						delta = 0;
						h++;
					}
				}
				delta++;
				n++;
			}

			return sb.ToString ();
		}

		// 41..5A (A-Z) = 0-25
		// 61..7A (a-z) = 0-25
		// 30..39 (0-9) = 26-35
		char EncodeDigit (int d)
		{
			return (char) (d < 26 ? d + 'a' : d - 26 + '0');
		}

		int DecodeDigit (char c)
		{
			return  c - '0' < 10 ? c - 22 :
				c - 'A' < 26 ? c - 'A' :
				c - 'a' < 26 ? c - 'a' : base_num;
		}

		int Adapt (int delta, int numPoints, bool firstTime)
		{
			if (firstTime)
				delta = delta / damp;
			else
				delta = delta / 2;
			delta = delta + (delta / numPoints);
			int k = 0;
			while (delta > ((base_num - tmin) * tmax) / 2) {
				delta = delta / (base_num - tmin);
				k += base_num;
			}
			return k + (((base_num - tmin + 1) * delta) / (delta + skew));
		}

		public string Decode (string s, int offset)
		{
			int n = initial_n;
			int i = 0;
			int bias = initial_bias;
			int b = 0;
			StringBuilder sb = new StringBuilder ();

			for (int j = 0; j < s.Length; j++) {
				if (s [j] == delimiter)
					b = j;
			}
			if (b < 0)
				return s;
			sb.Append (s, 0, b);

			for (int z = b > 0 ? b + 1 : 0; z < s.Length; ) {
				int old_i = i;
				int w = 1;
				for (int k = base_num; ; k += base_num) {
					int digit = DecodeDigit (s [z++]);
					i = i + digit * w;
					int t = k <= bias + tmin ? tmin :
						k >= bias + tmax ? tmax :
						k - bias;
					if (digit < t)
						break;
					w = w * (base_num - t);
				}
				bias = Adapt (i - old_i, sb.Length + 1, old_i == 0);
				n = n + i / (sb.Length + 1);
				i = i % (sb.Length + 1);
				if (n < '\x80')
					throw new ArgumentException (String.Format ("Invalid Bootstring decode result, at {0}", offset + z));
				sb.Insert (i, (char) n);
				i++;
			}

			return sb.ToString ();
		}
	}

	class Punycode : Bootstring
	{
		public Punycode ()
			: base ('-', 36, 1, 26, 38, 700, 72, 0x80)
		{
		}
	}
}
#endif
