// CS1979: Query expressions with a source or join sequence of type `dynamic' are not allowed
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
