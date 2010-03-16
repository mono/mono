using System;

public class Test
{
	public static int Main ()
	{
		float a = 1f / 3;
		float b = (float) Math.Acos ((float) (a * 3));
		Console.WriteLine (b);
		if (b != 0 && b != float.NaN) {
			throw new ApplicationException (b.ToString ());
			return 1;
		}

		return 0;
	}
}
