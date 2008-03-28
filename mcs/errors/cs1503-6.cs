// CS1503: Argument 1: Cannot convert type `object' to `int'
// Line: 16

using System;

class T
{
	public void M1 (int i, params object[] args) {}
}

class MainClass
{
	static void Main ()
	{
		T t = new T ();
		t.M1 (new object ());
	}
}
