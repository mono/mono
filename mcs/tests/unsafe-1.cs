//
// Tests unsafe operators.  address-of, dereference, member access
//
using System;

unsafe struct Y {
	public int a;
	public int s;
}

unsafe class X {
	static int TestDereference ()
	{
		Y y;
		Y *z; 
		Y a;

		z = &y;
		y.a = 1;
		y.s = 2;

		a.a = z->a;
		a.s = z->s;

		if (a.a != y.a)
			return 1;
		if (a.s != y.s)
			return 2;

		return 0;
	}

	static int TestPtrAdd ()
	{
		int [] a = new int [10];
		int i;
		
		for (i = 0; i < 10; i++)
			a [i] = i;

		i = 0;
		fixed (int *b = &a [0]){ 
			int *p = b;

			for (i = 0; i < 10; i++){
				if (*p != a [i])
					return 10+i;
				p++;
			}
		}
		return 0;
	}
	
	static int Main ()
	{
		int v;

		if ((v = TestDereference ()) != 0)
			return v;

		if ((v = TestPtrAdd ()) != 0)
			return v;

		Console.WriteLine ("Ok");
		return 0;
	}
}
