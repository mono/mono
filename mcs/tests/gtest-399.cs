using System;
using System.Collections.Generic;

namespace TestIssue
{
	class Base
	{
	}

	class Derived : Base
	{
	}

	class Program
	{
		public static int Main ()
		{
			try {
				IEnumerable<Derived> e1 = (IEnumerable<Derived>) (new Base [] { });
				return 1;
			}
			catch (InvalidCastException)
			{
				return 0;
			}
		}
	}
}
