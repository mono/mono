// CS0154: The property or indexer `Test.this[bool]' cannot be used in this context because it lacks the `get' accessor
// Line: 13

class Test
{
	public int this[bool b] { set {} }
}

class C
{
	public static void Main ()
	{
		int i = new Test()[false];
	}
}
