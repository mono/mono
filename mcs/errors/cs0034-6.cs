// CS0034: Operator `==' is ambiguous on operands of type `Foo' and `Foo'
// Line: 23

public struct Foo
{
	public static implicit operator int? (Foo f)
	{
		return 1;
	}

	public static implicit operator bool? (Foo f)
	{
		return false;
	}
}

class C
{
	public static void Main ()
	{
		Foo f;
		Foo f2;
		var v = f == f2;
	}
}