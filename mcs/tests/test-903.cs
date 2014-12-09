using System;

struct S
{
}

class C
{
	public static int ConversionCalled;

	public static implicit operator S (C c)
	{
		++ConversionCalled;
		return new S ();
	}
}

class Program
{
	static C field;

	static int Main ()
	{
		C c = new C ();
		var x = c ?? new S ();

		if (C.ConversionCalled != 1)
			return 1;

		c = null;
		x = c ?? new S ();
		if (C.ConversionCalled != 1)
			return 2;

		x = field ?? new S ();
		if (C.ConversionCalled != 1)
			return 3;

		return 0;
	}
}