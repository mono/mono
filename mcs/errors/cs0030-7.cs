// CS0030: Cannot convert type `S' to `E'
// Line: 10

enum E
{
	V
}

struct S
{
	public static explicit operator int (S val)
	{
		return 1;
	}
}

class C
{
	E Foo ()
	{
		S s = new S ();
		return (E) s;
	}

	public static void Main ()
	{
	}
}
