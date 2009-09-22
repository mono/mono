using System;

namespace Primes
{
	class MainClass
	{
		public static int Main ()
		{
			const ulong max = 5;
			bool[] numbers = new bool[max];

			for (ulong i = 0; i < max; i++)
				numbers[i] = true;

			for (ulong j = 1; j < max; j++)
				for (ulong k = (j + 1) * 2; k < max; k += j + 1)
					numbers[k] = false;

			for (ulong i = 0; i < max; i++)
				if (numbers[i])
					Console.WriteLine (i + 1);

			return 0;
		}
	}
}
