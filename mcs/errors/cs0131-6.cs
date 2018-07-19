// CS0131: The left-hand side of an assignment must be a variable, a property or an indexer
// Line: 8

class X
{
	void Test ()
	{
		Foo () = 1;
	}

	static int Foo ()
	{
		return 1;
	}
}