public class TestClass<T> where T : class
{
	public bool Check (T x, T y) { return x == y; }
}

public class C
{
}

public class TestClass2<T> where T : C
{
	public bool Check (T x, T y) { return x == y; }
}

public class X
{
	static void Main ()
	{ }
}


