// Compiler options: -unsafe

using System;

struct SS
{
}

public class C
{
	public static int[] field = new int [] { 66 };

	public static int Main()
	{
		unsafe {
			SS* ss = stackalloc SS [10];
			SS* s1 = &ss [5];
	    
			int* values = stackalloc int[20];
			int* p = &values[1];
			int* q = &values[15];

			Console.WriteLine("p - q = {0}", p - q);
			Console.WriteLine("q - p = {0}", q - p);
		}
		return 0;
	}
}

