// CS1729: The type `A.Foo' does not contain a constructor that takes `1' arguments
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
