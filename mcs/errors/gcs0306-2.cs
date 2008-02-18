// CS0306: The type `int*' may not be used as a type argument
// Line: 11
// Compiler options: -unsafe

using System.Linq;

public class C
{
	public static unsafe void Main ()
	{
		var e = from int* a in "aaa"
				select a;
	}
}
