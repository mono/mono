using System;
using System.Linq;

namespace Test
{
	class S<T>
	{
		public S ()
		{
		}

		public string Where (Func<C, string> cexpr)
		{
			return String.Empty;
		}
	}

	class C
	{
	}

	static class Extension
	{
		public static string Is (this C c)
		{
			return null;
		}
	}
	
	class Program
	{
		static void Main ()
		{
			var value = new S<int> ();
			var e = from item in value
							  where item.Is ()
							  select item;

			var foo = value.Where (p => p.Is ());
		}
	}	
}

