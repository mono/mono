//
// This exercises the various ways in which the new operator works
// with value types.
//

using System;

struct S {
	int v;
}

class X {
	static bool receive, create, create_and_box;
	
	static void receiver (S x)
	{
		receive = true;
	}

	static object BoxS ()
	{
		create_and_box = true;
		return new S ();
	}

	static S Plain ()
	{
		create = true;
		return new S ();
	}
	
	public static int Main ()
	{
		object a = new S ();
		receiver (new S ());
		S s = Plain ();
		object o = BoxS ();
		
		if (a == null)
			return 1;
		if (receive == false)
			return 2;
		if (create == false)
			return 3;
		if (create_and_box == false)
			return 4;

		Console.WriteLine ("Test pass");
		return 0;
	}
}
