// CS1503: Argument `#1' cannot convert `long' expression to type `ulong'
// Line: 17

class A
{
	public static long Prop {
		get {
			return 1;
		}
	}
}

class Test
{
	static void Main ()
	{
		Foo (A.Prop);
	}
	
	static void Foo (ulong l)
	{
	}
}
