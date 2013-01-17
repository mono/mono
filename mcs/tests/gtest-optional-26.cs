// Compiler options: -r:gtest-optional-26-lib.dll

using System;

public class C
{
	public static int Main ()
	{
		var res = CallerTest.Foo ();
		if (res != 0) {
			Console.WriteLine (res);
			return res;
		}
		
		return 0;
	}
}