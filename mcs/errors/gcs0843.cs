// CS0843: An automatically implemented property `S.Short' must be fully assigned before control leaves the constructor. Consider calling default contructor
// Line: 8

using System;

struct S
{
	public S (int value)
	{
	}
	
	public short Short { get; set; }
}
