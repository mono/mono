// CS8149: By-reference returns can only be used in lambda expressions that return by reference
// Line: 12

using System;

class A
{
	int p;
	
	void Test ()
	{
		Action a = () => ref p;
	}
}