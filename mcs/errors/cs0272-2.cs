// CS0272: The property or indexer `C.this[string]' cannot be used in this context because the set accessor is inaccessible
// Line: 13

class C
{
	public int this [string i] { private set { } get { return 1; } }
}

public class Test
{
	void Foo ()
	{	C c = new C ();
		c [""] = 9;
	}
}
