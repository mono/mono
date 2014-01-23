using System;

public delegate B Test<A,B> (A a);

public class Foo<T>
{
	T t;

	public Foo (T t)
	{
		this.t = t;
	}

	public U Method<U> (Test<T,U> test)
	{
		return test (t);
	}
}

class X
{
	public static void Main ()
	{
		Test<double,int> test = new Test<double,int> (Math.Sign);

		Foo<double> foo = new Foo<double> (Math.PI);
		Console.WriteLine (foo.Method<int> (test));

		string s = foo.Method<string> (delegate (double d) { return "s" + d; });
		Console.WriteLine (s);
	}
}
