

using System;

public delegate TResult Func<TArg0, TResult> (TArg0 arg0);

class Demo
{
	static Y F<X, Y> (int a, X value, Func<X, Y> f1)
	{
		return f1 (value);
	}
	public static int Main ()
	{
		object o = F (1, "1:15:30", s => TimeSpan.Parse (s));
		Console.WriteLine (o);
		return 0;
	}
}
