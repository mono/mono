// CS0165: Use of unassigned local variable `eh'
// Line: 12

using System;

public class E
{
	public static void Main ()
	{
		EventHandler eh;
		eh = delegate {
			Console.WriteLine (eh);
		};
	}
}