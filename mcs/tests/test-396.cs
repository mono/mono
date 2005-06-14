// Compiler options: -r:test-396-lib.dll

public class MainClass
{
	public static int Main ()
	{
		A a = new A ();
		B b = new B ();
		bool r = (a == b);

                return 0;
	}
}
