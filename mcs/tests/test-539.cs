// Compiler options: -optimize
using System;

class Test
{
	public static int Main ()
	{
		//switching to a constant fixes the problem
		double thisIsCausingTheProblem = 5.0;

		double[,] m1 = new double[4, 4] {
			{ 1.0, 0.0, 0.0, thisIsCausingTheProblem },
			{ 0.0, 1.0, 0.0, thisIsCausingTheProblem },
			{ 0.0, 0.0, 1.0, thisIsCausingTheProblem },
			{ 0.0, 0.0, 0.0, 1.0 }
		};

		var r = m1[0, 3];
		if (r != 5)
			return 1;

		return 0;
	}
}
