//
// System.Guid
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using System.Globalization;

namespace System {
	
public struct Guid  : IFormattable, IComparable  {

	private uint _a;
	private ushort _b;
	private ushort _c;
	private byte _d;
	private byte _e;
	private byte _f;
	private byte _g;
	private byte _h;
	private byte _i;
	private byte _j;
	private byte _k;

	private static void CheckNull (object o) {
		if (o == null) {
			throw new ArgumentNullException (Locale.GetText ("Value cannot be null."));
		}
	}

	private static void CheckLength (byte[] o, int l) {
		if (o . Length != l) {
			throw new ArgumentException (String.Format(Locale.GetText ("Array should be exactly {0} bytes long."), l));
		}
	}

	private static void CheckArray (byte[] o, int l) {
		CheckNull (o);
		CheckLength (o, l);
	}

	public Guid (byte[] b) {
		CheckArray (b, 16);
		_a = System.BitConverter.ToUInt32 (b, 0);
		_b = System.BitConverter.ToUInt16 (b, 4);
		_c = System.BitConverter.ToUInt16 (b, 6);
		_d = b[8];
		_e = b[9];
		_f = b[10];
		_g = b[11];
		_h = b[12];
		_i = b[13];
		_j = b[14];
		_k = b[15];
	}


	public Guid (int a, short b, short c, byte[] d) 
	{
		CheckArray(d, 8);
		_a = (uint) a;
		_b = (ushort) b;
		_c = (ushort) c;
		_d = d[0];
		_e = d[1];
		_f = d[2];
		_g = d[3];
		_h = d[4];
		_i = d[5];
		_j = d[6];
		_k = d[7];
	}

	public Guid (
		int a,
		short b,
		short c,
		byte d,
		byte e,
		byte f,
		byte g,
		byte h,
		byte i,
		byte j,
		byte k)
		: this((uint) a, (ushort) b, (ushort) c, d, e, f, g, h, i, j, k) {}

	[CLSCompliant(false)]
	public Guid (
		uint a,
		ushort b,
		ushort c,
		byte d,
		byte e,
		byte f,
		byte g,
		byte h,
		byte i,
		byte j,
		byte k)
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

	public static readonly Guid Empty = new Guid(0,0,0,0,0,0,0,0,0,0,0);

	private static int Compare (uint x, uint y)
	{
		if (x < y) {
			return -1;
		}
		else {
			return 1;
		}
	}

	public int CompareTo (object value ) {
		if (value == null )
			return 1;

		if (!(value is Guid)) {
			throw new ArgumentException (Locale.GetText (
				"Argument of System.Guid.CompareTo should be a Guid"));
		}

		Guid v = (Guid) value;

		if (_a != v._a ) {
			return Compare(_a, v._a);
		}
		else if (_b != v._b) {
			return Compare(_b, v._b);
		}
		else if (_c != v._c) {
			return Compare(_c, v._c);
		}
		else if (_d != v._d) {
			return Compare(_d, v._d);
		}
		else if (_e != v._e) {
			return Compare(_e, v._e);
		}
		else if (_f != v._f) {
			return Compare(_f, v._f);
		}
		else if (_g != v._g) {
			return Compare(_g, v._g);
		}
		else if (_h != v._h) {
			return Compare(_h, v._h);
		}
		else if (_i != v._i) {
			return Compare(_i, v._i);
		}
		else if (_j != v._j) {
			return Compare(_j, v._j);
		}
		else if (_k != v._k) {
			return Compare(_k, v._k);
		}

		return 0;
	}

	public override bool Equals ( object o ) {
		try {
			return CompareTo(o) == 0;	
		}
		catch ( ArgumentException ) {
			return false;
		}
	}

	public override int GetHashCode () {
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

	[MonoTODO]
	public static Guid NewGuid () {
		return Empty;
	}

	public byte[] ToByteArray () {
		byte[] res = new byte[16];
		byte[] tmp;
		int d = 0;
		int s;

		tmp = BitConverter.GetBytes(_a);
		for (s=0; s<4; ++s) {
			res[d++] = tmp[s];
		}

		tmp = BitConverter.GetBytes(_b);
		for (s=0; s<2; ++s) {
			res[d++] = tmp[s];
		}

		tmp = BitConverter.GetBytes(_c);
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

	private string BaseToString(bool h, bool p, bool b) {
		string res = "";
		
		if (p) {
			res += "(";
		}
		else if (b) {
			res += "{";
		}
	
		res += _a.ToString("x8");
		if (h) {
			res += "-";
		}
		res += _b.ToString("x4");
		if (h) {
			res += "-";
		}
		res += _c.ToString("x4");
		if (h) {
			res += "-";
		}
		res += _d.ToString("x2");
		res += _e.ToString("x2");
		if (h) {
			res += "-";
		}
		res += _f.ToString("x2");
		res += _g.ToString("x2");
		res += _h.ToString("x2");
		res += _i.ToString("x2");
		res += _j.ToString("x2");
		res += _k.ToString("x2");

		if (p) {
			res += ")";
		}
		else if (b) {
			res += "}";
		}
	
		return res;
	}

	public override string ToString () {
		return BaseToString (true, false, false);
	}

	public string ToString (string format) {
		string f;
		bool h = false;
		bool p = false;
		bool b = false;

		if (format != null) {
			f = format.ToLower();

			if (f == "b") {
				h = true;
				b = true;
			}
			else if (f == "p") {
				h = true;
				p = true;
			}
			else if (f == "d") {
				h = true;
			}
			else if (f != "n" && f != "") {
				throw new FormatException ( Locale.GetText ("Argument to Guid.ToString(string format) should be \"b\", \"B\", \"d\", \"D\", \"n\", \"N\", \"p\" or \"P\""));
			}
		}

		return BaseToString (h, p, b);
	}

	public string ToString (string format, IFormatProvider provider) {
		return ToString (format);
	}

	public static bool operator == (Guid a, Guid b) {
		return a.Equals(b);
	}

	public static bool operator != (Guid a, Guid b) {
		return !( a.Equals (b) );
	}

}

}
