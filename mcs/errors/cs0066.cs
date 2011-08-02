// CS0066: `Button.Click': event must be of a delegate type
// Line : 10

using System;

public delegate void EventHandler (object sender, EventArgs e);

public class Button {

	public event Blah Click;

	public void Reset ()
	{
		Click = null;
	}
}

public class Blah {

	public static void Main ()
	{
		Blah b = new Blah ();
	}
	
}
