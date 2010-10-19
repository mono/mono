// Compiler options: -t:library

using System;

public class A<T> where T : new ()
{
	public T Value = new T ();
	
	public class N1 : A<N2>
	{
	}
	
	public class N2
	{
		public int Foo ()
		{
			return 0;
		}
	}
}