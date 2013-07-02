using System;

interface IMyInterface<T>
{
	event EventHandler MyEvent;
}

public class MyClass: IMyInterface<string>, IMyInterface<int>
{
	event EventHandler IMyInterface<string>.MyEvent
	{
	add {}
	remove {}
	}

	event EventHandler IMyInterface<int>.MyEvent
	{
	add {}
	remove {}
	}
	
}

class X
{
	public static void Main ()
	{ }
}
