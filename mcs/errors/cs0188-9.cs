// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 11

using System;

public struct S
{
	public int A { get; private set;}
	event EventHandler eh;

	public S (int a)
	{
		this.eh = null;
		A = a;
	}
}
