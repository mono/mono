using System;
using SizeType = System.UInt64;

public class broken_cast
{
	public static int Main ()
	{
		SizeType m = 1;
		SizeType i = 1;
		SizeType [] n = new SizeType [2] { 7, 8 };
		m = m * n [i];
		return 0;
	}
}
