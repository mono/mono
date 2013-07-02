using T1 = N1.C1;

namespace N2
{
	class Test : T1
	{
		public static int Main()
		{
			// Compilation-only test.
			Foo ();
			return 0;
		}
	}
}

namespace N1
{
	public class C1
	{        
		public static void Foo ()
		{ }
	}
}
