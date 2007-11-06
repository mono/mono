// CS0407: A method or delegate `int X.f(int)' return type does not match delegate `object X.Function(int)' return type
// Line: 17

using System;

class X
{
	public delegate object Function(int arg1);

	static void Main ()
	{
		Delegate d = new Function (f);
	}

	static int f (int a)
	{
		return 1;
	}
}
