// Compiler options: -r:test-792-lib.dll

// Compilation test only for missing 2nd level dependecies

class Program
{
	void Test ()
	{
		new X();
		
		var s = new MultipleSameNames ();
		s.AA = "1";
	}
	
	void Test2 (IMemberDelayed md)
	{
		md.Working ();
		var t = typeof (IMemberDelayed);
	}

	static void Main ()
	{
	}
}

