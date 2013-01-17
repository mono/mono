// CS0649: Field `TestClass.b' is never assigned to, and will always have its default value
// Line: 12
// Compiler options: -warnaserror -warn:4

public struct Bar
{
	public int x;
}

public class TestClass
{
	Bar b;

	public bool Foo ()
	{
		if (b.x == 0)
			return false;

		return true;
	}

	public static void Main ()
	{
	}
}
