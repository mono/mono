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
	
	public static bool CovContCont<T> (ICovariant<T> e1, IContravariant<T> e2, IContravariant<T> e3)
	{
		Console.WriteLine (typeof (T));
		return typeof (T) == typeof (string);
	}

	public static bool ContCovContCov<T> (IContravariant<T> e1, ICovariant<T> e2, IContravariant<T> e3, ICovariant<T> e4)
	{
		Console.WriteLine (typeof (T));
		return typeof (T) == typeof (string);
	}
	
	public static bool CovCovCont<T> (ICovariant<T> e1, ICovariant<T> e2, IContravariant<T> e3)
	{
		Console.WriteLine (typeof (T));
		return typeof (T) == typeof (string);
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
		
		ICovariant<string> a_2 = null;
		IContravariant<object> b_2 = null;
		IContravariant<string> c_2 = null;
		if (!CovContCont (a_2, b_2, c_2))
			return 3;
		
		IContravariant<object> a_3 = null;
		ICovariant<string> b_3 = null;
		IContravariant<string> c_3 = null;
		ICovariant<string> d_3 = null;
		if (!ContCovContCov (a_3, b_3, c_3, d_3))
			return 4;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
