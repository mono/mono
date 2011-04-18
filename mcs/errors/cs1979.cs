// CS1977: Query expression with a source or join sequence of type `dynamic' is not allowed
// Line: 11

using System.Linq;

class C
{
	public static void Main ()
	{
		dynamic d = null;
		var r = from x in d select x;
	}
}
