// CS1547: Keyword `void' cannot be used in this context
// Line: 11

namespace OtherTest
{
	public static class Program
	{
		static void MainD (object p)
		{
			if (p is String)
				(void)((string) p).ToString ();
		}
	}
}
