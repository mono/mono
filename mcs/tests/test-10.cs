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
	}

	void abyte (sbyte a, char b)
	{
	}

	void bbyte ()
	{
		byte b = 0;

		abyte ((sbyte) b, (char) b);
	}

	void ashort (sbyte a, byte b, ushort c, uint d, ulong e, char f)
	{
	}

	void bshort ()
	{
		short a = 1;

		ashort ((sbyte) a, (byte) a, (ushort) a, (uint) a, (ulong) a, (char) a);
	}

	void aushort (sbyte a, byte b, short c, char d)
	{
	}

	void bushort ()
	{
		ushort a = 1;
		aushort ((sbyte) a, (byte) a, (short) a, (char) a);
	}

	void aint (sbyte a, byte b, short c, ushort d, uint e, ulong f, char g)
	{
	}

	void bint ()
	{
		int a = 1;

		aint ((sbyte) a, (byte) a, (short) a, (ushort) a, (uint) a, (ulong) a, (char) a);
	}

	void auint (sbyte a, byte b, short c, ushort d, int e, char f)
	{
	}

	void buint ()
	{
		uint a = 1;

		auint ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (char) a);
	}

	void along (sbyte a, byte b, short c, ushort d, int e, uint f, ulong g, char h)
	{
	}

	void blong ()
	{
		long a = 1;

		along ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (ulong) a, (char) a);
	}

	void aulong (sbyte a, byte b, short c, ushort d, int e, uint f, long g, char h)
	{
	}

	void bulong ()
	{
		ulong a = 1;

		aulong ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (long) a, (char) a);
	}

	void achar (sbyte a, byte b, short c)
	{

	}

	void bchar ()
	{
		char a = (char) 1;

		achar ((sbyte) a, (byte) a, (short) a);
	}

	void afloat (sbyte a, byte b, short c, ushort d, int e, uint f, long ll, ulong g, char h, decimal dd)
	{
	}

	void bfloat ()
	{
		float a = 1;

		afloat ((sbyte) a, (byte) a, (short) a, (ushort) a, (int) a, (uint) a, (long) a,
			(ulong) a, (char) a, (decimal) a);
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
	}

	static void Main ()
	{

	}
}
