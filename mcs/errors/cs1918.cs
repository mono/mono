// CS1918: Members of value type `S' cannot be assigned using a property `C.Value' object initializer
// Line: 18


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
