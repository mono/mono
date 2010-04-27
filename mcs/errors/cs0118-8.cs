// CS0118: `A.Foo(string)' is a `method group' but a `type' was expected
// Line: 15

public class A
{
	public static void Foo (string Setting)
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