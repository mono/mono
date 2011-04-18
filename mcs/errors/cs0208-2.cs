// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `CS208.Foo'
// Line: 20
// Compiler options: -unsafe

namespace CS208
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
