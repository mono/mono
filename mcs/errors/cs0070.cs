// cs0070.cs :  The event 'Click' can only appear on the left-side of a += or -= (except when used from within the type 'Button')
// Line : 20

using System;

public delegate void EventHandler (int i, int j);

public class Button {

	public event EventHandler Click;

}

public class Blah {

	Button Button1 = new Button ();

	public void Connect ()
	{
		Button1.Click = new EventHandler (Button1_Click);
	}

	public void Button1_Click (int i, int j)
	{
	}
	
	public static void Main ()
	{
	}
}
