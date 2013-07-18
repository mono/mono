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

		return 0;	
	}
}