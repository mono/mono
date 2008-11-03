// CS1059: The operand of an increment or decrement operator must be a variable, property or indexer
// Line: 11

using System;

class X
{
	static void Main ()
	{
		Decimal v;
		(v = new Decimal ())++;
	}
}
