using System;
using System.IO;
using NUnitLite.Runner;

public class Driver
{
	public static int Main (string[] args)
	{
		var runner = new TextUI ();
		runner.Execute (args);
		
		return (runner.Failure ? 1 : 0);
	}
}
