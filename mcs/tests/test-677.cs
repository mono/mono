using System;
	
class InvokeWindow
{
	public event D E;
		
	public void Run ()
	{
		E ();
	}
}
	
delegate void D ();

public class Test
{
	public static int Main ()
	{
		InvokeWindow win = new InvokeWindow ();
		win.E += new D (OnDeleteEvent);
		win.Run ();
		return 0;
	}

	static void OnDeleteEvent ()
	{
	}
	
	void OnDeleteEvent (int i)
	{
	}
}
