#define DEBUG
using System;
using System.Text;
using System.Diagnostics;

class Z
{
	static public void Test2 (string message, params object[] args)
	{
	}

	static public void Test (string message, params object[] args)
	{
		Test2 (message, args);
	}

	public static int Main ()
	{
		Test ("TEST");
		Test ("Foo", 8);
		Test ("Foo", 8, 9, "Hello");
		return 0;
	}
}
