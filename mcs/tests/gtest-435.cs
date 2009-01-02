using System;

namespace testcase
{
	public class Program
	{
		private static int Main ()
		{
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

