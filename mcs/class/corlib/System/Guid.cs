//
// System.Guid.cs
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Duco Fijma
// Copyright (C) 2004 Novell (http://www.novell.com)
//
// References
// 1.	UUIDs and GUIDs (DRAFT), Section 3.4
//	http://www.ics.uci.edu/~ejw/authoring/uuid-guid/draft-leach-uuids-guids-01.txt 
//

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace System
{
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public struct Guid  : IFormattable, IComparable
	{
		private uint _timeLow;
		private ushort _timeMid;
		private ushort _timeHighAndVersion;
		private byte _clockSeqHiAndReserved;
		private byte _clockSeqLow;
		private byte _node0;
		private byte _node1;
		private byte _node2;
		private byte _node3;
		private byte _node4;
		private byte _node5;

		internal class GuidParser
		{
			private string _src;
			private int _length;
			private int _cur;

			public GuidParser (string src)
			{
				_src = src;
				Reset ();
			}

			private void Reset ()
			{
				_cur = 0;
				_length = _src.Length;
			}

			private bool AtEnd ()
			{
				return _cur >= _length;
			}

			private void ThrowFormatException ()
			{
				throw new FormatException (Locale.GetText ("Invalid format for Guid.Guid(string)."));
			}

			private ulong ParseHex(int length, bool strictLength)
			{
				ulong res = 0;
				int i;
				bool end = false;

				for (i=0; (!end) && i<length; ++i) {
					if (AtEnd ()) {
						if (strictLength || i==0) {
							ThrowFormatException ();
						}
						else {
							end = true;
						}
					}
					else {
						char c = Char.ToLowerInvariant (_src[_cur]);
						if (Char.IsDigit (c)) {
							res = res * 16 + c - '0';
							_cur++;
						}
						else if (c >= 'a' && c <= 'f') {
							res = res * 16 + c - 'a' + 10;
							_cur++;
						}
						else {
							if (strictLength || i==0) {
								ThrowFormatException ();
							}
							else {
								end = true;
							}
						}
					}
				}
				return res;
			}

			private bool ParseOptChar (char c)
			{
				if (!AtEnd() && _src[_cur] == c) {
					_cur++;
					return true;
				}
				else {
					return false;
				}
			}

			private void ParseChar (char c)
			{
				bool b = ParseOptChar (c);
				if (!b) {
					ThrowFormatException ();
				}
			}

			private Guid ParseGuid1 ()
			{
				bool openBrace; 
				bool groups = true;
				char endChar = '}';
				int a;
				short b;
				short c;
				byte[] d = new byte[8];
				int i;

				openBrace = ParseOptChar ('{');
				if (!openBrace) {
					openBrace = ParseOptChar ('(');
					if (openBrace) endChar = ')';
				}
				
				a = (int) ParseHex(8, true);
				
				if (openBrace) ParseChar('-');
				else groups = ParseOptChar('-');
				
				b = (short) ParseHex(4, true);
				if (groups) ParseChar('-');
				
				c = (short) ParseHex(4, true);
				if (groups) ParseChar('-');
				
				for (i=0; i<8; ++i) {
					d[i] = (byte) ParseHex(2, true);
					if (i == 1 && groups) {
						ParseChar('-');
					}	
				}
	
				if (openBrace && !ParseOptChar(endChar)) {
					ThrowFormatException ();
				}
		
				return new Guid(a, b, c, d);
			}

			private void ParseHexPrefix ()
			{
				ParseChar ('0');
				ParseChar ('x');
			}

			private Guid ParseGuid2 ()
			{
				int a;
				short b;
				short c;
				byte[] d = new byte [8];
				int i;

				ParseChar ('{');
				ParseHexPrefix ();
				a = (int) ParseHex (8, false);
				ParseChar (',');
				ParseHexPrefix ();
				b = (short) ParseHex (4, false);
				ParseChar (',');
				ParseHexPrefix ();
				c = (short) ParseHex (4, false);
				ParseChar (',');
				ParseChar ('{');
				for (i=0; i<8; ++i) {
					ParseHexPrefix ();
					d[i] = (byte) ParseHex (2, false);
					if (i != 7) {
						ParseChar (',');
					}
				}
				ParseChar ('}');
				ParseChar ('}');

				return new Guid (a,b,c,d);
				
			}

			public Guid Parse ()
			{
				Guid g;

				try {
					g  = ParseGuid1 ();
				}
				catch (FormatException) {
					Reset ();
					g = ParseGuid2 ();
				}
				if (!AtEnd () ) {
					ThrowFormatException ();
				}
				return g;
			}
		}
	
		private static RandomNumberGenerator _rng = RandomNumberGenerator.Create ();

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
			_timeLow = System.BitConverter.ToUInt32 (b, 0);
			_timeMid = System.BitConverter.ToUInt16 (b, 4);
			_timeHighAndVersion = System.BitConverter.ToUInt16 (b, 6);
			_clockSeqHiAndReserved = b[8];
			_clockSeqLow = b[9];
			_node0 = b[10];
			_node1 = b[11];
			_node2 = b[12];
			_node3 = b[13];
			_node4 = b[14];
			_node5 = b[15];
		}

		public Guid (string g)
		{
			CheckNull (g);
	
			GuidParser p = new GuidParser (g);
			Guid guid = p.Parse();
	
			this = guid;
		}

		public Guid (int a, short b, short c, byte[] d)
		{
			CheckArray(d, 8);
			_timeLow = (uint) a;
			_timeMid = (ushort) b;
			_timeHighAndVersion = (ushort) c;
			_clockSeqHiAndReserved = d[0];
			_clockSeqLow = d[1];
			_node0 = d[2];
			_node1 = d[3];
			_node2 = d[4];
			_node3 = d[5];
			_node4 = d[6];
			_node5 = d[7];
		}

		public Guid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
			: this((uint) a, (ushort) b, (ushort) c, d, e, f, g, h, i, j, k)
		{
		}

		[CLSCompliant (false)]
		public Guid (uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			_timeLow = a;
			_timeMid = b;
			_timeHighAndVersion = c;
			_clockSeqHiAndReserved = d;
			_clockSeqLow = e;
			_node0 = f;
			_node1 = g;
			_node2 = h;
			_node3 = i;
			_node4 = j;
			_node5 = k;
		}

		public static readonly Guid Empty = new Guid (0,0,0,0,0,0,0,0,0,0,0);

		private static int Compare (uint x, uint y)
		{
			if (x < y) {
				return -1;
			}
			else {
				return 1;
			}
		}

		public int CompareTo (object value )
		{
			if (value == null)
				return 1;

			if (!(value is Guid)) {
				throw new ArgumentException (Locale.GetText (
					"Argument of System.Guid.CompareTo should be a Guid."));
			}

			Guid v = (Guid) value;

			if (_timeLow != v._timeLow ) {
				return Compare(_timeLow, v._timeLow);
			}
			else if (_timeMid != v._timeMid) {
				return Compare(_timeMid, v._timeMid);
			}
			else if (_timeHighAndVersion != v._timeHighAndVersion) {
				return Compare(_timeHighAndVersion, v._timeHighAndVersion);
			}
			else if (_clockSeqHiAndReserved != v._clockSeqHiAndReserved) {
				return Compare(_clockSeqHiAndReserved, v._clockSeqHiAndReserved);
			}
			else if (_clockSeqLow != v._clockSeqLow) {
				return Compare(_clockSeqLow, v._clockSeqLow);
			}
			else if (_node0 != v._node0) {
				return Compare(_node0, v._node0);
			}
			else if (_node1 != v._node1) {
				return Compare(_node1, v._node1);
			}
			else if (_node2 != v._node2) {
				return Compare(_node2, v._node2);
			}
			else if (_node3 != v._node3) {
				return Compare(_node3, v._node3);
			}
			else if (_node4 != v._node4) {
				return Compare(_node4, v._node4);
			}
			else if (_node5 != v._node5) {
				return Compare(_node5, v._node5);
			}
			return 0;
		}

		public override bool Equals (object o)
		{
			try {
				return CompareTo (o) == 0;	
			}
			catch (ArgumentException) {
				return false;
			}
		}

		public override int GetHashCode ()
		{
			int res;
	
			res = (int) _timeLow; 
			res = res ^ ((int) _timeMid << 16 | _timeHighAndVersion);
			res = res ^ ((int) _clockSeqHiAndReserved << 24);
			res = res ^ ((int) _clockSeqLow << 16);
			res = res ^ ((int) _node0 << 8);
			res = res ^ ((int) _node1);
			res = res ^ ((int) _node2 << 24);
			res = res ^ ((int) _node3 << 16);
			res = res ^ ((int) _node4 << 8);
			res = res ^ ((int) _node5);

			return res;
		}

		private static char ToHex (int b)
		{
			return (char)((b<0xA)?('0' + b):('a' + b - 0xA));
		}

		// generated as per section 3.4 of the specification
		public static Guid NewGuid ()
		{
			byte[] b = new byte [16];

			// access the prng
			lock (_rng) {
				_rng.GetBytes (b);
			}
	
			Guid res = new Guid (b);
			// Mask in Variant 1-0 in Bit[7..6]
			res._clockSeqHiAndReserved = (byte) ((res._clockSeqHiAndReserved & 0x3fu) | 0x80u);
			// Mask in Version 4 (random based Guid) in Bits[15..13]
			res._timeHighAndVersion = (ushort) ((res._timeHighAndVersion & 0x0fffu) | 0x4000u);
			return res;
		}

		public byte[] ToByteArray ()
		{
			byte[] res = new byte[16];
			byte[] tmp;
			int d = 0;
			int s;

			tmp = BitConverter.GetBytes(_timeLow);
			for (s=0; s<4; ++s) {
				res[d++] = tmp[s];
			}

			tmp = BitConverter.GetBytes(_timeMid);
			for (s=0; s<2; ++s) {
				res[d++] = tmp[s];
			}

			tmp = BitConverter.GetBytes(_timeHighAndVersion);
			for (s=0; s<2; ++s) {
				res[d++] = tmp[s];
			}

			res[8] = _clockSeqHiAndReserved;
			res[9] = _clockSeqLow;
			res[10] = _node0;
			res[11] = _node1;
			res[12] = _node2;
			res[13] = _node3;
			res[14] = _node4;
			res[15] = _node5;

			return res;
		}

		private string BaseToString (bool h, bool p, bool b)
		{
			StringBuilder res = new StringBuilder (40);
			
			if (p) {
				res.Append ('(');
			} else if (b) {
				res.Append ('{');
			}
		
			res.Append (_timeLow.ToString ("x8"));
			if (h) {
				res.Append ('-');
			}
			res.Append (_timeMid.ToString ("x4"));
			if (h) {
				res.Append ('-');
			}
			res.Append (_timeHighAndVersion.ToString ("x4"));
			if (h) {
				res.Append ('-');
			}
	
			char[] vals1 = {
				ToHex((_clockSeqHiAndReserved >> 4) & 0xf),
				ToHex(_clockSeqHiAndReserved & 0xf),
				ToHex((_clockSeqLow >> 4) & 0xf),
				ToHex(_clockSeqLow & 0xf)
			};

			res.Append (vals1);

			if (h) {
				res.Append ('-');
			}
		
			char[] vals2 = {
				ToHex((_node0 >> 4) & 0xf),
				ToHex(_node0 & 0xf),
				ToHex((_node1 >> 4) & 0xf),
				ToHex(_node1 & 0xf),
				ToHex((_node2 >> 4) & 0xf),
				ToHex(_node2 & 0xf),
				ToHex((_node3 >> 4) & 0xf),
				ToHex(_node3 & 0xf),
				ToHex((_node4 >> 4) & 0xf),
				ToHex(_node4 & 0xf),
				ToHex((_node5 >> 4) & 0xf),
				ToHex(_node5 & 0xf)
			};
	
			res.Append (vals2);
	
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
				else if (f != "d" && f != "") {
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
	}
}
