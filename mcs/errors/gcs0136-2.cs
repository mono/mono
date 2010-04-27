// CS0136: A local variable named `v' cannot be declared in this scope because it would give a different meaning to `v', which is already used in a `parent or current' scope to denote something else
// Line: 13

using System.Linq;

public class Test
{
	public static void Main ()
	{
		var l = from v in "abcd" select (v => v);
	}
}
