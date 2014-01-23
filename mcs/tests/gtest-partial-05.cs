using System;
using System.Collections.Generic;

public interface IC : IB
{
}

public partial interface IB : IEnumerable<char>
{
}

public partial interface IB : IA
{
}

public interface IA : IDisposable
{
}

class Driver
{
	static void Foo<T> (T t) where T : IA
	{
	}

	static void Main ()
	{
		IC i = null;
		Foo<IC> (i);
	}
}