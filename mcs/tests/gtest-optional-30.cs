// Compiler options: -r:gtest-optional-30-lib.dll

public static class Program
{
	public static int Main()
	{
		if (Lib.Foo<object>() != null)
			return 1;

		return 0;
	}
}