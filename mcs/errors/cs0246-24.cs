// CS0246: The type or namespace name `TypeMe' could not be found. Are you missing a using directive or an assembly reference?
// Line: 12

class C
{
	static void Foo<T> (int i)
	{
	}

	public static void Main ()
	{
		Foo<TypeMe> (1);
	}
}
