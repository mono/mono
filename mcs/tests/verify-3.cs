using System;

public delegate void MyEventHandler (int a);

public class X
{
	public static event MyEventHandler TestEvent;

	public static void Main ()
	{
	}
}
