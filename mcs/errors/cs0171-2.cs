// CS0171: Field `S.ev' must be fully assigned before control leaves the constructor
// Line: 12

using System;

struct S
{
	event EventHandler ev;
	
	public S (int i)
	{
	}
}
