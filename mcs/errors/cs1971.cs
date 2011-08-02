// CS1971: The base call to method `Foo' cannot be dynamically dispatched. Consider casting the dynamic arguments or eliminating the base access
// Line: 16

class A
{
	public void Foo (int i)
	{
	}
}

class B : A
{
	public void Test ()
	{
		dynamic d = null;
		var r = base.Foo (d);
	}
}
