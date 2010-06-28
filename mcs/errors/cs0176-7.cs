// CS0176: Static member `A.Foo()' cannot be accessed with an instance reference, qualify it with a type name instead
// Line: 21

public class A
{
	public static void Foo ()
	{
	}
}

public class Test
{
	static A Prop
	{
		get {
			return null;
		}
	}

	public static void Main ()
	{
		Test.Prop.Foo ();
	}
}
