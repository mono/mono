using System;

public delegate int DelType ();

struct S
{
	public event DelType MyEvent;
	public static event DelType MyEventStatic;
	
	public int RunInstance ()
	{
		return MyEvent ();
	}
	
	public int RunStatic ()
	{
		return MyEventStatic ();
	}
}

public class Test
{
	public static int Main ()
	{
		S.MyEventStatic += delegate () { return 22; };
		S s = new S ();
		s.MyEvent += delegate () { return 6; };
		if (s.RunInstance () != 6)
			return 1;
		
		if (s.RunStatic () != 22)
			return 2;
		
		return 0;
	}
}
