// CS0201: Only assignment, call, increment, decrement, and new object expressions can be used as a statement
// Line: 10

using System.Linq;

class C
{
	public static void Main ()
	{
		from s in "string" select s;
	}
}