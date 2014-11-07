// CS0171: Field `S1.s2' must be fully assigned before control leaves the constructor
// Line: 11

using System;

struct S1
{
	S2 s2;

	public S1 (int arg)
	{
	}
}

struct S2
{
	int field;
}
