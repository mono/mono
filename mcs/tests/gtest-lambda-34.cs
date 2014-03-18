using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
	static void Main ()
	{
		SomeMethod ();
	}

	private static bool SomeMethod ()
	{
		int m;
		int n = 2;
		bool b_const = true;

		bool b = F (() => F1 (n, out m) && Ferror (m)) && b_const;
		return b;
	}

	protected static bool F (Func<bool> rule)
	{
		return true;
	}

	private static bool F1 (int j, out int m)
	{
		m = 2;

		return true;
	}

	private static bool Ferror (int i)
	{
		return true;
	}

	private static bool Fouter ()
	{
		return true;
	}
}
