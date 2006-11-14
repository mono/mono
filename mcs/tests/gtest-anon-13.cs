using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class X
{
	public delegate T ModuleBinder<T> (object o);

	public ModuleBinder<TDelegate> CreateMethodUnscoped<TDelegate> ()
	{
		return delegate (object o) {
			return (TDelegate)(object)null;
		};
	}

	static void Main ()
	{ }
}
