// Compiler options: -unsafe

using System;

public class Program
{
	public static void Main ()
	{
		new TestStruct ("a");
	}
}

public unsafe struct TestStruct
{
	private fixed byte symbol[30];

	public TestStruct (string a)
	{
	}
}
