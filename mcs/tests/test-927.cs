// Compiler options: -unsafe

using System;

class MainClass
{
	static int called;

	public static double[] GetTempBuffer ()
	{
		++called;
		return new double[4];
	}

	public static int Main ()
	{
		unsafe {
			fixed (double* dummy = GetTempBuffer()) {
			}
		}

		if (called != 1)
			return 1;

		return 0;
	}
}
