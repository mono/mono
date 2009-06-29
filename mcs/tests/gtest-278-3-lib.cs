// Compiler options: -r:gtest-278-2-lib.dll -t:library

using System;

public class B
{
	public static int Test ()
	{
		if (C.Print () != "C")
			return 1;

		if (D.Print () != "D")
			return 2;
			
		if (G<int>.Test (5) != 5)
			return 3;
			
		if (C.CC.Print () != "C+CC")
			return 4;
			
		Console.WriteLine (typeof (C));
		Console.WriteLine (typeof (D));
		Console.WriteLine (typeof (G<string>));
		Console.WriteLine (typeof (C.CC).Assembly.FullName);

		return 0;
	}
}
