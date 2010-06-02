// CS0103: The name `a' does not exist in the current context
// Line: 11

using System.Linq;

class C
{
	public static void Main ()
	{
		var e = from a in "abcd"
			join b in "defg" on "a" equals a
			select a;
	}
}
