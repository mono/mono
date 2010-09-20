using System;

class A<T>
{
}

class B
{
	static void M1<T> (T t) where T : struct
	{
	}
	
	static void M2<T, U> (T t, U u) where U : IEquatable<T>
	{
	}
	
	static void M3<T, U> (T t, A<U> u) where U : IEquatable<T>
	{
	}

	static void M4<T, U> (T t, IEquatable<U> u) where T : IEquatable<U>
	{
	}

	public static void Main ()
	{
		dynamic d = 2;
		M1 (d);
		
		M2 (d, 6);
		M2 (4, d);
		
		M3 (d, new A<int> ());
		
		M4 (d, 6);
		// TODO: type inference
		//M4 (4, d);
	}
}

