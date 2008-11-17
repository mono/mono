using System;

public class EventClass<T>
{
	public delegate void HookDelegate (T del);
}

public class Test
{
	public static void Main ()
	{
		Console.WriteLine (typeof (EventClass<>.HookDelegate));
	}
}
