// CS1975: The constructor call cannot be dynamically dispatched within constructor initializer
// Line: 14

public class A
{
	public A (dynamic arg)
	{
	}
}

public class B : A
{
	public B (dynamic arg)
		: base (arg)
	{
	}
}
