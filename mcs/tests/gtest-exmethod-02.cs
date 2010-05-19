// Compiler options: -r:gtest-exmethod-02-lib.dll -noconfig

using System;

public class M
{
	public static void Main ()
	{
		"foo".Test_1 ();
	}
}

namespace N
{
	public class M
	{
		public static void Test2 ()
		{
			"foo".Test_1 ();
		}
	}
}