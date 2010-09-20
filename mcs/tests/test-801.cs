using System;

class C
{
	public enum E
	{
		V_0 = 10,
		V_1	= 50,
		V_2 = 80
	}
	
	public static implicit operator E (C x)
	{
		return E.V_2;
	}

	public static implicit operator int (C x)
	{
		return 1;
	}

	public static int Main ()
	{
		var v = new C ();
		int i = E.V_1 - v;
		if (i != -30)
			return 1;
		
		i = v - E.V_1;
		if (i != 30)
			return 10;
		
		E e = E.V_1 + v;
		if (e != (E) 51)
			return 2;
		
		e = v + E.V_0;
		if (e != (E) 11)
			return 3;
		
		bool b = E.V_2 > v;
		if (b)
			return 4;
		
		int iv = 900;
		e = iv - E.V_1;
		if (e != (E)850)
			return 5;
		
		i = v - E.V_1;
		if (i != (int) 30)
			return 6;

		return 0;
	}
}
