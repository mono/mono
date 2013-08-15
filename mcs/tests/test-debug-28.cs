public class StatementsAsBlocks
{
	static void ForEach (string[] args)
	{
		foreach (var v in args)
			;
		foreach (var v in args)
			;
	}
	
	public static int Main ()
	{
		return 0;
	}
}