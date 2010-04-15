//
// System.Guid.cs
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Duco Fijma
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// References
// 1.	UUIDs and GUIDs (DRAFT), Section 3.4
//	http://www.ics.uci.edu/~ejw/authoring/uuid-guid/draft-leach-uuids-guids-01.txt 
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

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace System {

	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	[ComVisible (true)]
	public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid> {
#if MONOTOUCH
		static Guid () {
			if (MonoTouchAOTHelper.FalseFlag) {
				var comparer = new System.Collections.Generic.GenericComparer <Guid> ();
				var eqcomparer = new System.Collections.Generic.GenericEqualityComparer <Guid> ();
			}
		}
#endif
		private int _a; //_timeLow;
		private short _b; //_timeMid;
		private short _c; //_timeHighAndVersion;
		private byte _d; //_clockSeqHiAndReserved;
		private byte _e; //_clockSeqLow;
		private byte _f; //_node0;
		private byte _g; //_node1;
		private byte _h; //_node2;
		private byte _i; //_node3;
		private byte _j; //_node4;
		private byte _k; //_node5;

		enum Format {
			N, // 00000000000000000000000000000000
			D, // 00000000-0000-0000-0000-000000000000
			B, // {00000000-0000-0000-0000-000000000000}
			P, // (00000000-0000-0000-0000-000000000000)
			X, // {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
		}

		class GuidParser {

			private string _src;
			private int _length;
			private int _cur;

			public GuidParser (string src)
			{
				_src = src;
				Reset ();
			}

			void Reset ()
			{
				_cur = 0;
				_length = _src.Length;
			}

			bool Eof {
				get { return _cur >= _length; }
			}

			static bool HasHyphen (Format format)
			{
				switch (format) {
				case Format.D:
				case Format.B:
				case Format.P:
					return true;
				default:
					return false;
				}
			}

			bool TryParseNDBP (Format format, out Guid guid)
			{
				ulong a, b, c;
				guid = new Guid ();

				if (format == Format.B && !ParseChar ('{'))
					return false;

				if (format == Format.P && !ParseChar ('('))
					return false;

				if (!ParseHex (8, true, out a))
					return false;

				var has_hyphen = HasHyphen (format);

				if (has_hyphen && !ParseChar ('-'))
					return false;

				if (!ParseHex (4, true, out b))
					return false;

				if (has_hyphen && !ParseChar ('-'))
					return false;

				if (!ParseHex (4, true, out c))
					return false;

				if (has_hyphen && !ParseChar ('-'))
					return false;

				var d = new byte [8];
				for (int i = 0; i < d.Length; i++) {
					ulong dd;
					if (!ParseHex (2, true, out dd))
						return false;

					if (i == 1 && has_hyphen && !ParseChar ('-'))
						return false;

					d [i] = (byte) dd;
				}

				if (format == Format.B && !ParseChar ('}'))
					return false;

				if (format == Format.P && !ParseChar (')'))
					return false;

				guid = new Guid ((int) a, (short) b, (short) c, d);
				return true;
			}

			bool TryParseX (out Guid guid)
			{
				ulong a, b, c;
				guid = new Guid ();

				if (!(ParseChar ('{')
					&& ParseHexPrefix ()
					&& ParseHex (8, false, out a)
					&& ParseChar (',')
					&& ParseHexPrefix ()
					&& ParseHex (4, false, out b)
					&& ParseChar (',')
					&& ParseHexPrefix ()
					&& ParseHex (4, false, out c)
					&& ParseChar (',')
					&& ParseChar ('{'))) {

					return false;
				}

				var d = new byte [8];
				for (int i = 0; i < d.Length; ++i) {
					ulong dd;

					if (!(ParseHexPrefix () && ParseHex (2, false, out dd)))
						return false;

					d [i] = (byte) dd;

					if (i != 7 && !ParseChar (','))
						return false;
				}

				if (!(ParseChar ('}') && ParseChar ('}')))
					return false;

				guid = new Guid ((int) a, (short) b, (short) c, d);
				return true;
			}

			bool ParseHexPrefix ()
			{
				if (!ParseChar ('0'))
					return false;

				return ParseChar ('x');
			}

			bool ParseChar (char c)
			{
				if (!Eof && _src [_cur] == c) {
					_cur++;
					return true;
				}

				return false;
			}

			bool ParseHex (int length, bool strict, out ulong res)
			{
				res = 0;

				for (int i = 0; i < length; i++) {
					if (Eof)
						return !(strict && (i + 1 != length));

					char c = Char.ToLowerInvariant (_src[_cur]);
					if (Char.IsDigit (c)) {
						res = res * 16 + c - '0';
						_cur++;
					} else if (c >= 'a' && c <= 'f') {
						res = res * 16 + c - 'a' + 10;
						_cur++;
					} else {
						if (!strict)
							return true;

						return !(strict && (i + 1 != length));
					}
				}

				return true;
			}

			public bool Parse (Format format, out Guid guid)
			{
				if (format == Format.X)
					return TryParseX (out guid);

				return TryParseNDBP (format, out guid);
			}

			public bool Parse (out Guid guid)
			{
				if (TryParseNDBP (Format.N, out guid))
					return true;

				Reset ();
				if (TryParseNDBP (Format.D, out guid))
					return true;

				Reset ();
				if (TryParseNDBP (Format.B, out guid))
					return true;

				Reset ();
				if (TryParseNDBP (Format.P, out guid))
					return true;

				Reset ();
				return TryParseX (out guid);
			}
		}

		private static void CheckNull (object o)
		{
			if (o == null) {
				throw new ArgumentNullException (Locale.GetText ("Value cannot be null."));
			}
		}

		private static void CheckLength (byte[] o, int l)
		{
			if (o . Length != l) {
				throw new ArgumentException (String.Format (Locale.GetText ("Array should be exactly {0} bytes long."), l));
			}
		}

		private static void CheckArray (byte[] o, int l)
		{
			CheckNull (o);
			CheckLength (o, l);
		}

		public Guid (byte[] b)
		{
			CheckArray (b, 16);
			_a = Mono.Security.BitConverterLE.ToInt32 (b, 0);
			_b = Mono.Security.BitConverterLE.ToInt16 (b, 4);
			_c = Mono.Security.BitConverterLE.ToInt16 (b, 6);
			_d = b [8];
			_e = b [9];
			_f = b [10];
			_g = b [11];
			_h = b [12];
			_i = b [13];
			_j = b [14];
			_k = b [15];
		}

		public Guid (string g)
		{
			CheckNull (g);
			g = g.Trim();
			var parser = new GuidParser (g);
			Guid guid;
			if (!parser.Parse (out guid))
				throw CreateFormatException (g);

			this = guid;
		}

		static Exception CreateFormatException (string s)
		{
			return new FormatException (string.Format ("Invalid Guid format: {0}", s));
		}

		public Guid (int a, short b, short c, byte[] d)
		{
			CheckArray (d, 8);
			_a = (int) a;
			_b = (short) b;
			_c = (short) c;
			_d = d [0];
			_e = d [1];
			_f = d [2];
			_g = d [3];
			_h = d [4];
			_i = d [5];
			_j = d [6];
			_k = d [7];
		}

		public Guid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			_a = a;
			_b = b;
			_c = c;
			_d = d;
			_e = e;
			_f = f;
			_g = g;
			_h = h;
			_i = i;
			_j = j;
			_k = k;
		}

		[CLSCompliant (false)]
		public Guid (uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
			: this((int) a, (short) b, (short) c, d, e, f, g, h, i, j, k)
		{
		}

		public static readonly Guid Empty = new Guid (0,0,0,0,0,0,0,0,0,0,0);

		private static int Compare (int x, int y)
		{
			if (x < y) {
				return -1;
			}
			else {
				return 1;
			}
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is Guid)) {
				throw new ArgumentException ("value", Locale.GetText (
					"Argument of System.Guid.CompareTo should be a Guid."));
			}

			return CompareTo ((Guid)value);
		}

		public override bool Equals (object o)
		{
			if (o is Guid)
				return CompareTo ((Guid)o) == 0;
			return false;
		}

		public int CompareTo (Guid value)
		{
			if (_a != value._a) {
				return Compare (_a, value._a);
			}
			else if (_b != value._b) {
				return Compare (_b, value._b);
			}
			else if (_c != value._c) {
				return Compare (_c, value._c);
			}
			else if (_d != value._d) {
				return Compare (_d, value._d);
			}
			else if (_e != value._e) {
				return Compare (_e, value._e);
			}
			else if (_f != value._f) {
				return Compare (_f, value._f);
			}
			else if (_g != value._g) {
				return Compare (_g, value._g);
			}
			else if (_h != value._h) {
				return Compare (_h, value._h);
			}
			else if (_i != value._i) {
				return Compare (_i, value._i);
			}
			else if (_j != value._j) {
				return Compare (_j, value._j);
			}
			else if (_k != value._k) {
				return Compare (_k, value._k);
			}
			return 0;
		}

		public bool Equals (Guid g)
		{
			return CompareTo (g) == 0;
		}

		public override int GetHashCode ()
		{
			int res;
	
			res = (int) _a; 
			res = res ^ ((int) _b << 16 | _c);
			res = res ^ ((int) _d << 24);
			res = res ^ ((int) _e << 16);
			res = res ^ ((int) _f << 8);
			res = res ^ ((int) _g);
			res = res ^ ((int) _h << 24);
			res = res ^ ((int) _i << 16);
			res = res ^ ((int) _j << 8);
			res = res ^ ((int) _k);

			return res;
		}

		private static char ToHex (int b)
		{
			return (char)((b<0xA)?('0' + b):('a' + b - 0xA));
		}

		private static object _rngAccess = new object ();
		private static RandomNumberGenerator _rng;
		private static RandomNumberGenerator _fastRng;

		// generated as per section 3.4 of the specification
		public static Guid NewGuid ()
		{
			byte[] b = new byte [16];

			// thread-safe access to the prng
			lock (_rngAccess) {
				if (_rng == null)
					_rng = RandomNumberGenerator.Create ();
				_rng.GetBytes (b);
			}

			Guid res = new Guid (b);
			// Mask in Variant 1-0 in Bit[7..6]
			res._d = (byte) ((res._d & 0x3fu) | 0x80u);
			// Mask in Version 4 (random based Guid) in Bits[15..13]
			res._c = (short) ((res._c & 0x0fffu) | 0x4000u);

			return res;
		}

		// used in ModuleBuilder so mcs doesn't need to invoke 
		// CryptoConfig for simple assemblies.
		internal static byte[] FastNewGuidArray ()
		{
			byte[] guid = new byte [16];

			// thread-safe access to the prng
			lock (_rngAccess) {
				// if known, use preferred RNG
				if (_rng != null)
					_fastRng = _rng;
				// else use hardcoded default RNG (bypassing CryptoConfig)
				if (_fastRng == null)
					_fastRng = new RNGCryptoServiceProvider ();
				_fastRng.GetBytes (guid);
			}

			// Mask in Variant 1-0 in Bit[7..6]
			guid [8] = (byte) ((guid [8] & 0x3f) | 0x80);
			// Mask in Version 4 (random based Guid) in Bits[15..13]
			guid [7] = (byte) ((guid [7] & 0x0f) | 0x40);

			return guid;
		}

		public byte[] ToByteArray ()
		{
			byte[] res = new byte[16];
			byte[] tmp;
			int d = 0;
			int s;

			tmp = Mono.Security.BitConverterLE.GetBytes(_a);
			for (s=0; s<4; ++s) {
				res[d++] = tmp[s];
			}

			tmp = Mono.Security.BitConverterLE.GetBytes(_b);
			for (s=0; s<2; ++s) {
				res[d++] = tmp[s];
			}

			tmp = Mono.Security.BitConverterLE.GetBytes(_c);
			for (s=0; s<2; ++s) {
				res[d++] = tmp[s];
			}

			res[8] = _d;
			res[9] = _e;
			res[10] = _f;
			res[11] = _g;
			res[12] = _h;
			res[13] = _i;
			res[14] = _j;
			res[15] = _k;

			return res;
		}

		static void AppendInt (StringBuilder builder, int value) {
			builder.Append (ToHex ((value >> 28) & 0xf));
			builder.Append (ToHex ((value >> 24) & 0xf));
			builder.Append (ToHex ((value >> 20) & 0xf));
			builder.Append (ToHex ((value >> 16) & 0xf));
			builder.Append (ToHex ((value >> 12) & 0xf));
			builder.Append (ToHex ((value >> 8) & 0xf));
			builder.Append (ToHex ((value >> 4) & 0xf));
			builder.Append (ToHex (value & 0xf));
		}

		static void AppendShort (StringBuilder builder, short value) {
			builder.Append (ToHex ((value >> 12) & 0xf));
			builder.Append (ToHex ((value >> 8) & 0xf));
			builder.Append (ToHex ((value >> 4) & 0xf));
			builder.Append (ToHex (value & 0xf));
		}

		static void AppendByte (StringBuilder builder, byte value) {
			builder.Append (ToHex ((value >> 4) & 0xf));
			builder.Append (ToHex (value & 0xf));
		}

		private string BaseToString (bool h, bool p, bool b)
		{
			StringBuilder res = new StringBuilder (40);
			
			if (p) {
				res.Append ('(');
			} else if (b) {
				res.Append ('{');
			}
		
			AppendInt (res, _a);
			if (h) {
				res.Append ('-');
			}
			AppendShort (res, _b);
			if (h) {
				res.Append ('-');
			}
			AppendShort (res, _c);
			if (h) {
				res.Append ('-');
			}
	
			AppendByte (res, _d);
			AppendByte (res, _e);

			if (h) {
				res.Append ('-');
			}

			AppendByte (res, _f);
			AppendByte (res, _g);
			AppendByte (res, _h);
			AppendByte (res, _i);
			AppendByte (res, _j);
			AppendByte (res, _k);

	
			if (p) {
				res.Append (')');
			} else if (b) {
				res.Append ('}');
			}
		
			return res.ToString ();
		}
	
		public override string ToString ()
		{
			return BaseToString (true, false, false);
		}
	
		public string ToString (string format)
		{
			bool h = true;
			bool p = false;
			bool b = false;
	
			if (format != null) {
				string f = format.ToLowerInvariant();
	
				if (f == "b") {
					b = true;
				}
				else if (f == "p") {
					p = true;
				}
				else if (f == "n") {
					h = false;
				}
				else if (f != "d" && f != String.Empty) {
					throw new FormatException (Locale.GetText (
						"Argument to Guid.ToString(string format) should be \"b\", \"B\", \"d\", \"D\", \"n\", \"N\", \"p\" or \"P\""));
				}
			}

			return BaseToString (h, p, b);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			return ToString (format);
		}

		public static bool operator == (Guid a, Guid b)
		{
			return a.Equals(b);
		}

		public static bool operator != (Guid a, Guid b)
		{
			return !( a.Equals (b) );
		}

#if NET_4_0
		public static Guid Parse (string input)
		{
			Guid guid;
			if (!TryParse (input, out guid))
				throw CreateFormatException (input);

			return guid;
		}

		public static Guid ParseExact (string input, string format)
		{
			Guid guid;
			if (!TryParseExact (input, format, out guid))
				throw CreateFormatException (input);

			return guid;
		}

		public static bool TryParse (string input, out Guid result)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			var parser = new GuidParser (input);
			return parser.Parse (out result);
		}

		public static bool TryParseExact (string input, string format, out Guid result)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			if (format == null)
				throw new ArgumentNullException ("format");

			var parser = new GuidParser (input);
			return parser.Parse (ParseFormat (format), out result);
		}

		static Format ParseFormat (string format)
		{
			if (format.Length != 1)
				throw new ArgumentException ("Wrong format");

			switch (format [0]) {
			case 'N':
				return Format.N;
			case 'D':
				return Format.D;
			case 'B':
				return Format.B;
			case 'P':
				return Format.P;
			case 'X':
				return Format.X;
			}

			throw new ArgumentException ("Wrong format");
		}
#endif
	}
}
