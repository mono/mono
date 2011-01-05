// CS0411: The type arguments for method `C.Foo<T>(ref T, ref T)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 14

class C
{
	public static void Foo<T> (ref T t1, ref T t2)
	{
	}
	
	public static void Main ()
	{
		string s = "a";
		object o = null;
		Foo (ref s, ref o);
	}
}
