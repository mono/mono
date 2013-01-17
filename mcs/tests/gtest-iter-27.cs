using System;
using System.Collections.Generic;

public class CompilerBug
{
	public static void Main ()
	{
		foreach (string message in Foo ())
			Console.WriteLine (message);
	}

	static IEnumerable<string> Foo ()
	{
		Action fnAction;
		{
			fnAction = () => { };
		}
		yield return "Executing action";
		{
			fnAction ();
		}
		yield return "Action executed";
	}
}
