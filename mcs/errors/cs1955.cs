// CS1955: The member `Y.x' cannot be used as method or delegate
// Line: 17

using System;

class Y
{
	public int x { get { return 1; } }
}

class X
{
	static int Main ()
	{
		Y y = new Y ();

		y.x ();
		return 0;
	}
}
