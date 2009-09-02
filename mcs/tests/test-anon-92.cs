using System;
using System.Collections;

delegate void D ();

class MainClass
{
	static void Test (IEnumerable a)
	{
		D d = delegate () {
			foreach (object o in a) {
				if (o == null)
					return;
			}
		};
		
		d ();
	}
	
    public static int Main ()
    {
		Test (new string [] { "l", null});
		return 0;
    }
}
