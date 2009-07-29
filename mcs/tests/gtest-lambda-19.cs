using System;

public class Crasher
{
	public static void Crash ()
	{
		double [] array = new double [1];
		Do (() => {
			int col = 1;
			array [col] += array [col];
		});
	}

	static void Do (Action action)
	{
	}

	public static void Main ()
	{
	}
}
