using System;

class App
{
	public static void Main ()
	{
		EventHandler h = new EventHandler (Test) ?? Test;
	}

	public static void Test (object s, EventArgs a)
	{
	}
}
