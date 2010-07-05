// CS0154: The property or indexer `A.this[int]' cannot be used in this context because it lacks a `get' accessor
// Line: 18

public class A
{
	public int this[int i] { set { } }
}

public class B : A
{
	public int this[string i] { get { return 9; } }
}

class Test
{
	public static void Main ()
	{
		int i = new B ()[1];
	}
}

