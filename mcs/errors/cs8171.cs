// CS8171: Cannot initialize a by-value variable `l' with a reference expression
// Line: 10

class Test
{
	int field;

	void Foo ()
	{
		int l = ref field;
	}
}