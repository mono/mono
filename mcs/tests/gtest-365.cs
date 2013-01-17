using System;

namespace B
{
	class Program
	{
		public static int Main()
		{
			int? i = 0;
			bool b = i == (int?)null;
			if (b)
				return 1;
			
			if (i == (int?)null)
				return 2;
				
			Console.WriteLine (b);
			return 0;
		}
	}
}

