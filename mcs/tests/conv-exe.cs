// Compiler options: -r:conv-dll.dll

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
