// CS1501: No overload for method `A.Foo' takes `1' arguments
// Line: 15

public class A
{
	public class Foo
	{
	}
}

class Example
{
	public void Main(string[] args)
	{
		A a = new A.Foo ("test");  
	}
}