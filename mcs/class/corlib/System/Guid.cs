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
	byte[] _defghijk;

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
		_defghijk = new byte[8];
		Array.Copy(b, 8, _defghijk, 0, 8);
	}


	public Guid (int a, short b, short c, byte[] d) 
	{
		CheckArray(d, 8);
		_a = (uint) a;
		_b = (ushort) b;
		_c = (ushort) c;
		_defghijk = (byte[]) d.Clone();
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
		_defghijk = new byte[8];
		_defghijk = new byte[] {d, e, f, g, h, i, j, k};
	}

	public static readonly Guid Empty = new Guid(0,0,0,0,0,0,0,0,0,0,0);

	[MonoTODO]
	public int CompareTo (object value ) {
		return 0;
	}

	[MonoTODO]
	public override bool Equals ( object o ) {
		return false;
	}

	[MonoTODO]
	public override int GetHashCode () {
		return 0;
	}

	[MonoTODO]
	public static Guid NewGuid () {
		return Empty;
	}

	[MonoTODO]
	public byte[] ToByteArray () {
		return new byte[16];
	}

	public override string ToString() {
		string res = "";
		
		res = _a.ToString("x8") + "-";
		res += _b.ToString("x4") + "-";
		res += _c.ToString("x4") + "-";
		for (int i=0; i<8; ++i) {
			res += _defghijk[i].ToString("x2");
			if (i == 1) {
				res += '-';
			}
		}
		return res;
	}

	[MonoTODO]
	public string ToString (string format) {
		return ToString ();
	}

	[MonoTODO]
	public string ToString (string format, IFormatProvider provider) {
		return ToString ();
	}

	[MonoTODO]
	public static bool operator == (Guid a, Guid b) {
		return false;
	}

	[MonoTODO]
	public static bool operator != (Guid a, Guid b) {
		return false;
	}

}

}
