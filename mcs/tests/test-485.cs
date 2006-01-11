// Compiler options: /checked+

using System;


public class MonoBUG
{
	public static void Main (string[] args)
	{
		double B = 4.0;
		double K = 2.0;
		double A = - B / (K * K);

		Console.WriteLine ("{0}", A);
	}
}
