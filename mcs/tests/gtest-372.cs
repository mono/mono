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
	public static int Main ()
	{
		new TestClass<object> ().Check (null, null);
		new TestClass2<C> ().Check (null, null);
		return 0;
	}
}


