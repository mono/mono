using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mono
{
	class C
	{
		public static int Main ()
		{
			int[] i2 = new int [] { 10, 14 };

			Expression<Func<IEnumerable<int>>> e = () => from i in i2 select i;
			int sum = e.Compile () ().Sum ();
			if (sum != 24)
				return 1;
				
			Expression<Func<IEnumerable<long>>> e2 = () => from i in GetValues () select i;
			var s2 = e2.Compile () ().Sum ();
			if (s2 != 14)
				return 2;
				
			return 0;
		}
		
		static long[] GetValues ()
		{
			return new long [] { 9, 2, 3 };
		}
	}
}

