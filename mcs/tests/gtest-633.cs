using System;

struct BB
{}

public class X
{
	public static void Main ()
	{
		// Reduced expression statements
		
		new BB? ();
		new float();

		Action a = () => new float();
		a ();
	}
}