using System;
using System.Collections.Generic;

class Program
{
	static void Main ()
	{
	}

	public static void Transform<V> (Area<V> area, Func<V, V> transform)
		where V : IA<V>
	{
		Test (GetIB<V> (), t => Transform2 (null, transform));
	}

	static IB<W> GetIB<W> ()
		where W : IA<W>
	{
		return null;
	}

	static void Test<T> (T values, Func<T, T> func)
	{
	}

	public static IB<U> Transform2<U> (
		IB<U> b,
		Func<U, U> transform) where U : IA<U>
	{
		return null;
	}
}


public class Area<TVector>
	where TVector : IA<TVector>
{
	public IB<TVector> GetSegments ()
	{
		return null;
	}
}

public interface IB<TB>
	where TB : IA<TB>
{
}

public interface IA<T>
{
}
