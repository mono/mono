/*
 * Test explicit numeric conversions.
 */

using System;

class X {

	void asbyte (byte a, ushort b, uint c, ulong d, char e)
	{
	}

	void bsbyte ()
	{
		sbyte s = 0;

		asbyte ((byte) s, (ushort) s, (uint) s, (ulong) s, (char) s);
		asbyte (checked ((byte) s), checked ((ushort) s), checked ((uint) s), checked ((ulong) s), checked ((char) s));
	}

	void abyte (sbyte a, char b)
	{
	}

	void bbyte ()
	{
		byte b = 0;

		abyte ((sbyte) b, (char) b);
		abyte (checked ((sbyte) b), checked ((char) b));
	}

	void ashort (sbyte a, byte b, ushort c, uint d, ulong e, char f)
	{
	}

	void bshort ()
	{
		short a = 1;

		ashort ((sbyte) a, (byte) a, (ushort) a, (uint) a, (ulong) a, (char) a);
		ashort (checked ((sbyte) a), checked ((byte) a), checked ((ushort) a), checked ((uint) a), checked ((ulong) a), checked ((char) a));
	}

	void aushort (sbyte a, byte b, short c, char d)
	{
	}

	void bushort ()
	{
		ushort a = 1;
		aushort ((sbyte) a, (byte) a, (short) a, (char) a);
		aushort (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((char) a));
	}

	void aint (sbyte a, byte b, short c, ushort d, uint e, ulong f, char g)
	{
	}

	void bint ()
	{
		int a = 1;

		aint ((sbyte) a, (byte) a, (short) a, (ushort) a, (uint) a, (ulong) a, (char) a);
		aint (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((ushort) a), checked ((uint) a), checked ((ulong) a), checked ((char) a));
	}

	void auint (sbyte a, byte b, short c, ushort d, int e, char f)
	{
	}

	void buint ()
	{
		uint a = 1;

		auint ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (char) a);
		auint (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((ushort) a), checked ((int) a), checked ((char) a));
	}

	void along (sbyte a, byte b, short c, ushort d, int e, uint f, ulong g, char h)
	{
	}

	void blong ()
	{
		long a = 1;

		along ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (ulong) a, (char) a);
		along (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((ushort) a), checked ((int) a), checked ((uint) a), checked ((ulong) a), checked ((char) a));
	}

	void aulong (sbyte a, byte b, short c, ushort d, int e, uint f, long g, char h)
	{
	}

	void bulong ()
	{
		ulong a = 1;

		aulong ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (long) a, (char) a);
		aulong (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((ushort) a), checked ((int) a), checked ((uint) a), checked ((long) a), checked ((char) a));
	}

	void achar (sbyte a, byte b, short c)
	{

	}

	void bchar ()
	{
		char a = (char) 1;

		achar ((sbyte) a, (byte) a, (short) a);
		achar (checked ((sbyte) a), checked ((byte) a), checked ((short) a));
	}

	void afloat (sbyte a, byte b, short c, ushort d, int e, uint f, long ll, ulong g, char h, decimal dd)
	{
	}

	void bfloat ()
	{
		float a = 1;

		afloat ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (long) a,
			(ulong) a, (char) a, (decimal) a);
		afloat (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((ushort) a), checked ((int) a), checked ((uint) a), checked ((long) a),
checked (			(ulong) a), checked ((char) a), checked ((decimal) a));
	}

	void adouble (sbyte a, byte b, short c, ushort d, int e, uint f, long ll, ulong g, char h,
		      float ff, decimal dd)
	{
	}
	
	void bdouble ()
	{
		double a = 1;

		adouble ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (long) a,
			(ulong) a, (char) a, (float) a, (decimal) a);
		adouble (checked ((sbyte) a), checked ((byte) a), checked ((short) a), checked ((ushort) a), checked ((int) a), checked ((uint) a), checked ((long) a),
checked (			(ulong) a), checked ((char) a), checked ((float) a), (decimal) a);
	}
	
	void TestDecimal (decimal d)
	{
		double dec = (double)d;
		decimal dec2 = (decimal)dec;
	}
	
	public static void Main ()
	{

	}
}

enum E:byte {
	Min = 9
}

class Test2 {
	void ExtraTst ()
	{
		E error = E.Min - 9;
		string s = (string)null;
		const decimal d = -10.1m;
		const long l = (long)d;
		char ch = (char)E.Min;
		bool b = (DBNull) null == null;
	}
}