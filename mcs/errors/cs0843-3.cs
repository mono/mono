// CS0843: An automatically implemented property `S.A' must be fully assigned before control leaves the constructor. Consider calling the default struct contructor from a constructor initializer
// Line: 11

using System;

public struct S
{
	public int A { get; set;}
	event EventHandler eh;

	public S (int a)
	{
		this.eh = null;
		A = a;
	}
}
