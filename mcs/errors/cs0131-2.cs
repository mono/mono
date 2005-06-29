// cs0131-2.cs: The left-hand side of an assignment or mutating operation must be a variable, property or indexer
// Line: 10
using System;

class X
{
	static void Main ()
	{
		int a = 7;
		+a = 9;
	}
}
