//
// Tests the syntax for delegates and events
//
using System;

delegate void ClickEvent ();

class Button {
	public event ClickEvent Clicked;

	public void DoClick ()
	{
		Clicked ();
	}
	
}

class X {
	static bool called = false;
	
	public static int Main ()
	{
		Button b = new Button ();
				       
		b.Clicked += delegate {
			Console.WriteLine ("This worked!");
			called = true;
		};

		b.DoClick ();
		
		if (called)
			return 0;
		else
			return 1;
	}
	
}
