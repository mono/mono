using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderByBugExample
{
	class Foo
	{
		public string Name { get; set; }
		public int Value1 { get; set; }
		public int Value2 { get; set; }
	}

	static class Program
	{
		public static int Main ()
		{
			List<Foo> test = new List<Foo> ()
			{ 
				new Foo { Name="b", Value1=37, Value2=2 },
				new Foo { Name="b", Value1=37, Value2=1 }
			};

			// Sort using a linq expression. Mono 2.6.1 ignores item.Value2, which is incorrect behaviour.
			var result = from item in test
						 orderby item.Name, item.Value1, item.Value2
						 select item;

			var r = result.ToList ();

			foreach (Foo item in r)
				Console.WriteLine ("{0}, {1}, {2}", item.Name, item.Value1, item.Value2);

			if (r[0].Value2 != 1 && r[1].Value2 != 2)
				return 1;

			Console.WriteLine ("ok");
			return 0;
		}
	}
}
