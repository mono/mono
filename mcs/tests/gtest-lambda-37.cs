using System;

public class MainClass 
{
	static long? X<T> (T a1, Func<T, T?> a2) where T : struct
	{
		return 0;
	}

	static int? X<T> (T a1, Func<T, int?> a2)
	{
		return 0;
	}

	static double? X<T> (T a1, Func<T, double?> a2)
	{
		return null;
	}

	public static void Main ()
	{
		int? sum = X<int> (1, i => {
			if (i > 0)
				return i;

			return null;
		});


		int? sum2 = X (1, i => {
			if (i > 0)
				return i;

			return null;
		});

	}
}
