class Class1
{
	public int Method1 () { return 1000; }
}

class Class2<T>
{
	public T t;
}

class Class3 : Class2<dynamic>
{
	public void Method2 ()
	{
		t.Method1 ();
	}
}

class Program
{
	public static void Main ()
	{
		var c3 = new Class3 ();
		c3.t = new Class1 ();
		c3.Method2 ();
	}
}
