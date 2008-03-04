// CS1501: No overload for method `LogFileLine' takes `1' arguments
// Line: 12

class C
{
	static void LogFileLine (string file, string msg, params object [] args)
	{
	}
	
	public static void Main ()
	{
		LogFileLine ("aaa");
	}
}
