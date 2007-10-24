using System;
using System.Diagnostics;

public class ShiftReduceParser<TokenValueType, TokenLocationType>
{
	[Conditional ("DUMP")]
	public static void Dump (string format)
	{
		throw new ApplicationException ();
	}
}

public class Parser : ShiftReduceParser<int, int>
{
	[Conditional ("DUMP")]
	static void NoCall<T> (T t)
	{
	}
	
	public static int Main ()
	{
		Dump ("Should not be called");
		NoCall (1);
		
		return 0;
	}
}
