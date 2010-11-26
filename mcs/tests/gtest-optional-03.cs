// Compiler options: -r:gtest-optional-03-lib.dll

using System;

class C
{
	static int Test (int i = 1, string s = "", bool b = false, ushort u = 4)
	{
		return i;
	}
	
	public static int Main ()
	{
		if (Test () != 1)
			return 1;
		
		if (B.TestString () != "mono")
			return 3;
		
		if (B.TestString ("top") != "top")
			return 4;
		
		if (B.TestB () != null)
			return 5;

		if (B.Test<ushort> () != 0)
			return 6;

		if (B.TestDecimal (2) != decimal.MinValue)
			return 7;
		
		if (B.TestDecimal (2, 5) != 5)
			return 8;
		
		if (B.TestEnum () != E.Value)
			return 9;
		
		B b = new B ();
		b [1] = 'z';
		if (b [0] != 'h')
			return 10;
		
		B.TestNew ();
		
		Console.WriteLine ("ok");
		
		return 0;
	}
}
