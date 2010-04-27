// CS0079: The event `Foo.Event2' can only appear on the left hand side of `+=' or `-=' operator
// Line: 11

using System;

public class Foo {
	EventHandler event2;

	public Foo ()
	{
		Event2 = null;
	}

	public event EventHandler Event2 {
		add { event2 += value; }
		remove {event2 -= value; }
	}
}
