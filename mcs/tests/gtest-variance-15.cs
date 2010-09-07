using System;

public class C
{
	delegate void D<in T> (T t);
	
	static void M<T> (ref T t, D<T> a)
	{
		a (t);
	}

	static void M2<T> (T t, D<T> a)
	{
		a (t);
	}
	
	static void MethodArg (object o)
	{
	}
	
	public static int Main ()
	{
		D<object> action = l => Console.WriteLine (l);
		string s = "value";
		
		M (ref s, action);
		M2 (s, action);
		return 0;
	}
}
