using System;

enum E
{
	A
}

class C
{
	public static void Main ()
	{
		E dt = E.A;
		IntPtr ip = (IntPtr)dt;
		ip = (IntPtr)E.A;
	}
}
