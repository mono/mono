// CS0308: The non-generic method `X.Foo()' cannot be used with the type arguments
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
