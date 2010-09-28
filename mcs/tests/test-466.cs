// Compiler options: -r:test-466-lib.dll

namespace A.X
{
	using A.B;
	
	class Test
	{
		public static void Main ()
		{
			C c = new C ();
			c.Foo ();
		}
	}
}