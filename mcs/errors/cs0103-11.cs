// CS0103: The name `a1' does not exist in the current context
// Line: 11

using System.Linq;

class C
{
	public static void Main ()
	{
		var e = 
			from a1 in "abcd"
			select a1;
		
		a1 = null;
	}
}
