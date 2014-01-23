using System;

class Program
{
	public static void Main ()
	{
		Action action = () => Console.WriteLine (1);
		action += null;
		action = null + action;
		action = action + null;
		action ();
	}
}
