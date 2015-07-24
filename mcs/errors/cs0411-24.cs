// CS0411: The type arguments for method `C.Foo<T>(out T)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 8
// Compiler options: -langversion:experimental

public class C
{
	public static void Main ()
	{
		Foo (out var y);
	}

	static void Foo<T> (out T t)
	{
		t = default (T);
	}
}