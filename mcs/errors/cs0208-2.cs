// cs0208.cs: Cannot take the address or size of a variable of a managed type ('cs208.Foo')
// Line: 20
// Compiler options: -unsafe

namespace cs208
{
	public class Foo
	{
	}

	public class Bar
	{
		unsafe static void Main ()
		{			
			Foo f = new Foo ();
			Foo *s = &f;
		}
	}
}
