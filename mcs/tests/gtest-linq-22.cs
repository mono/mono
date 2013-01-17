using System;
using System.Linq;

namespace Test
{
	class MainClass
	{
		public static void Main ()
		{
			DateTime junk = DateTime.Today;
			var results = from item in "abcd"
						  let parsed = DateTime.TryParse ("today", out junk)
						  select parsed ? junk : DateTime.Now;
		}
	}
}
