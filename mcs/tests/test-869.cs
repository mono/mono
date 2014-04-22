using System;

public class C
{
	public static readonly C Token = new C ();

	public static C operator & (C set, E value)
	{
		return Token;
	}

	public static implicit operator E (C c)
	{
		throw new ApplicationException ();
	}
}

public enum E
{
	Item = 2
}

enum E2
{
	A = 0,
	B,
	C
}

class FooClass
{
	public static int Main ()
	{
		C m = new C ();
		var x = E.Item;
		var res = m & x;
		if (res != C.Token)
			return 1;

		res = m & E.Item;
		if (res != C.Token)
			return 2;

		E2 e2 = E2.C;

		int day1 = e2 - E2.A;
		return 0;	
	}
}