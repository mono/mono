// CS1674: `object': type used in a using statement must be implicitly convertible to `System.IDisposable'
// Line: 10

using System;

class C
{
	void Method (IDisposable i)
	{
		using (object o = i)
		{
		}
    }
}