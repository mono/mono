// Compiler options: -r:dtest-friend-01-lib.dll

class C
{
	public static void Main ()
	{
		dynamic d = new A();
		d.Test ();
	}
}
