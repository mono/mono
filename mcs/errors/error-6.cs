using System.Collections;

class X
{
	static Hashtable h = new Hashtable ();

	public static void Main ()
	{
		// CS0029
		ArrayList l = h ["hola"] = new ArrayList ();
	}
}
