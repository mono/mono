// May use a constructed type as constraint.

class Test<T>
{ }

class Foo<T>
	where T : Test<T>
{ }

class X
{
	public static void Main ()
	{
	}
}
