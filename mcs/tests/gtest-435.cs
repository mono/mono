using System;

namespace testcase
{
	public class Program
	{
		public static int Main ()
		{
			DateTime? a = default (DateTime?);
			DateTime? b = default (DateTime?);
			bool res = a == b;
			if (!res)
				return 4;

			res = a != b;
			if (res)
				return 3;

			decimal? D1 = null;
			decimal? D2 = 7;

			if (D1 == D2) {
				Console.WriteLine ("null == 7  incorrect");
				return 1;
			} else if (D1 != D2) {
				Console.WriteLine ("null != 7  correct");
				return 0;
			}
			return 2;
		}
	}
}

