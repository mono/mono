// CS0306: The type `S' may not be used as a type argument
// Line: 13
// Compiler options: -langversion:latest

public ref struct S
{
}

class Test<T>
{
	public static void Foo ()
	{
		Test<S> local;
	}
}