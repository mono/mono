// CS0451: The `new()' constraint cannot be used with the `struct' constraint
// Line: 6

class C
{
	public static void Foo<T>() where T : struct, new ()
	{
	}
}
