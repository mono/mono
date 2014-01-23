//
// This test is used to check that we can actually use implementations
// provided in our parent to interfaces declared afterwards.
//

using System;

public interface A {
 	int Add (int a, int b);
}

public class X {
	public int Add (int a, int b)
	{
		return a + b;
	}
}

class Y : X, A {

	public static int Main ()
	{
		Y y = new Y ();
		
		if (y.Add (1, 1) != 2)
			return 1;

		Console.WriteLine ("parent interface implementation test passes");
		return 0;
	}
	
}
