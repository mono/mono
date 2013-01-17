// CS0165: Use of unassigned local variable `a'
// Line: 11

using System.Linq;

class M
{
	public static void Main ()
	{
		int[] a;
		int m = a.FirstOrDefault<int> ();
	}
}
