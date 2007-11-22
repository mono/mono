// CS0308: The non-generic method `Foo' cannot be used with type arguments
// Line: 12

class X
{
	public void Foo ()
	{
	}
	
	void Test ()
	{
		Foo<int> ();
	}
}
