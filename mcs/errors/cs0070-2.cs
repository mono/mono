// CS0070: The event `A.evt' can only appear on the left hand side of += or -= when used outside of the type `A'
// Line: 22

using System;

public static class EventExtensions
{
	public static void Raise (this EventHandler h)
	{
	}
}

public class A
{
	public event EventHandler evt;
}

public class B : A
{
	public void Run()
	{
		Action a = () => evt.Raise ();
	}
}