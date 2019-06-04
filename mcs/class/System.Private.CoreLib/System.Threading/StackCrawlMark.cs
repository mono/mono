namespace System.Threading
{
	// declaring a local var of this enum type and passing it by ref into a function that needs to do a
	// stack crawl will both prevent inlining of the calle and pass an ESP point to stack crawl to
	// Declaring these in EH clauses is illegal; they must declared in the main method body
	internal enum StackCrawlMark
	{
		LookForMe = 0,
		LookForMyCaller = 1,
		LookForMyCallersCaller = 2,
		LookForThread = 3
	}
}