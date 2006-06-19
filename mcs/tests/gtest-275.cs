using System;
using System.Collections;
using System.Collections.Generic;

public class Test
{
	public class C
	{
		public C()
		{
			Type t = typeof(Dictionary<,>);
		}
	}

	public class D<T, U>
	{
		public D()
		{
			Type t = typeof(Dictionary<,>);
		}
	}

	public class E<T>
	{
		public E()
		{
			Type t = typeof(Dictionary<,>);
		}
	}

	public static void Main()
	{		
		new C();
		new D<string, string>();
		new E<string>();
	}
}

