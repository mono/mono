// CS0170: Use of possibly unassigned field `a'
// Line: 17

using System;

public struct S
{
	public Action a;
}


public class Test
{
	static void Main ()
	{
		S s;
		s.a += delegate { };
	}
}
