using System;

struct RVA {
	public uint value;

	public RVA (uint val)
	{
		value = val;
	}

	public static implicit operator RVA (uint val)
	{
		return new RVA (val);
	}

	public static implicit operator uint (RVA rva)
	{
		return rva.value;
	}
}

class X
{
	public static int Main ()
	{
		RVA a = 10;
		RVA b = 20;

		if (a > b)
			return 1;

		if (a + b != 30)
			return 2;

		return 0;
	}
}
