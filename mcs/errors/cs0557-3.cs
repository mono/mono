// CS0557: Duplicate user-defined conversion in type `Foo'
// Line: 5

public enum Bar
{
	ABar
}


public class Foo
{
	public static explicit operator Foo(Bar the_bar)
	{
		return new Foo();
	}
	public static implicit operator Foo(Bar the_bar)
	{
		return new Foo();
	}
}
