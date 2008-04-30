// CS0832: An expression tree cannot contain an assignment operator
// Line: 19

using System;
using System.Linq.Expressions;

public delegate void EventHandler (int i, int j);

public class Button
{
	public event EventHandler Click;
}

public class Blah
{
	public static void Main ()
	{
		Button b = new Button ();
		Expression<Action> e = () => b.Click += new EventHandler (Button1_Click);
	}

	public static void Button1_Click (int i, int j)
	{
	}
}
