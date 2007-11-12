// CS1918: Members of a value type property `C.Value' cannot be assigned with an object initializer
// Line: 18
// Compiler options: -langversion:linq

struct S
{
	public int X;
}

class C
{
	public S Value {
		set { }
	}

	static void Main ()
	{
		C c = new C { Value = { X = 2 } };
	}
}
