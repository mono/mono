using System;

class X
{
	static void Main ()
	{
		new X().WriteLine("some text");
	}

	public void WriteLine(string format, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black, params object[] args)
	{
		throw new ApplicationException ();
	}

	public void WriteLine(string line, ConsoleColor foreColor = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black)
	{
	}
}
