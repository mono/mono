// CS1059: The operand of an increment or decrement operator must be a variable, property or indexer
// Line: 9

static class C
{
	static void Foo()
	{
		const int uu = 1;
		uu++;
	}
}