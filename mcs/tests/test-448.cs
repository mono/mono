using System;

public class MonoDivideProblem
{
	static uint dividend = 0x80000000;
	static uint divisor = 1;
	public static void Main(string[] args)
	{
		Console.WriteLine("Dividend/Divisor = {0}", dividend/divisor);
	}

}
