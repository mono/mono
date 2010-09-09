// CS1674: `int': type used in a using statement must be implicitly convertible to `System.IDisposable'
// Line: 10

using System;

class C
{
	void Method (IDisposable i)
	{
		using (int o = 1, b = 2)
		{
		}
	}
}
