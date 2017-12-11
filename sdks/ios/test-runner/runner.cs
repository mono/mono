using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnitLite.Runner;
using NUnit.Framework.Internal;

public class TestRunner
{
	public static int Main(string[] args) {
		TextUI runner;

		runner = new TextUI ();
		runner.Execute (args);
            
		return (runner.Failure ? 1 : 0);
    }
}
