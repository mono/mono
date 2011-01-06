// CS0136: A local variable named `v' cannot be declared in this scope because it would give a different meaning to `v', which is already used in a `child' scope to denote something else
// Line: 11

using System.Linq;

class C
{
	public static void Main ()
	{
		var l = from v in "abcd" select v;
		int v;
	}
}
