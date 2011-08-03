using System;

interface IContravariant<in T>
{
}

interface ICovariant<out T>
{
}

class D
{
	public static bool Contra<T> (IContravariant<T> e1, IContravariant<T> e2)
	{
		Console.WriteLine (typeof (T));
		return typeof (T) == typeof (string);
	}
	
	public static bool Covariant<T> (ICovariant<T> e1, ICovariant<T> e2)
	{
		Console.WriteLine (typeof (T));
		return typeof (T) == typeof (object);
	}
	
	public static int Main ()
	{
		ICovariant<object> a = null;
		ICovariant<string> b = null;
		if (!Covariant (a, b))
			return 1;
		
		IContravariant<string> a_1 = null;
		IContravariant<object> b_1 = null;
		if (!Contra (a_1, b_1))
			return 2;
		
		return 0;
	}
}
