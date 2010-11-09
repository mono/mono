// Compiler options: -r:test-645-lib.dll

class C
{
	public static void Main ()
	{
		var n = new A.AN ();
		n.TestReturn ().Test ();
		
		var t2 = new T2 ();
		t2.Test ();
	}
}
