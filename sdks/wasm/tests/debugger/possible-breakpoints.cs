using System;

public class PossibleBreakpoints {
	// These two methods must be kept at the end of the file.
	public static void GetPossibleBreakpoints () {
		Console.WriteLine ("FIRST LINE");
		Console.WriteLine ("SECOND LINE");
	}

	public static void GetPossibleBreakpoints2 () {
		Console.WriteLine ($"THIRD LINE");
	}
}
