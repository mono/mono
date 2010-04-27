using System;
using System.Collections.Generic;

interface IFoo
{
}

class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
{
	public bool Equals (T[] x, T[] y)
	{
		return false;
	}

	public int GetHashCode (T[] args)
	{
		return 0;
	}
}

public class Program
{
	public static int Main ()
	{
		var d = new Dictionary<IFoo[], IFoo> (new ArrayEqualityComparer<IFoo> ());
		return 0;
	}
}
