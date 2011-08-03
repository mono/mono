// CS0266: Cannot implicitly convert type `object' to `System.Collections.ArrayList'. An explicit conversion exists (are you missing a cast?)
// Line: 12

using System.Collections;

class X
{
	static Hashtable h = new Hashtable ();

	public static void Main ()
	{
		ArrayList l = h ["hola"] = new ArrayList ();
	}
}
