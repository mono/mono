using System;
class X {
	bool sbyte_selected;
	bool int_selected;

	void test (sbyte s)
	{
		sbyte_selected = true;
	}

	void test (int i)
	{
		int_selected = true;
	}

	static int Main ()
	{
		X y = new X ();
		sbyte s = 10;

		y.test (s);
		if (y.sbyte_selected){
			Console.WriteLine ("OK: sbyte selected for sbyte argument");
		} else {
			Console.WriteLine ("FAILED: sbyte not selected for sbyte argument");
			return 1;
		}
		return 0;
	}
}		
