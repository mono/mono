using System;
using System.IO;
using System.Threading;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests {

public class MyTestRunner {

	static TextWriter fWriter = Console.Out;

	protected static TextWriter Writer {
		get { return fWriter; }
	}

	public static void Print(TestResult result) {
		PrintErrors(result);
		PrintFailures(result);
		PrintHeader(result);
	}

	/// <summary>Prints the errors to the standard output.</summary>
	public static void PrintErrors(TestResult result) {
		if (result.ErrorCount != 0) {
			if (result.ErrorCount == 1)
				Writer.WriteLine("There was "+result.ErrorCount+" error:");
			else
				Writer.WriteLine("There were "+result.ErrorCount+" errors:");
			
			int i= 1;
			foreach (TestFailure failure in result.Errors) {
				Writer.WriteLine(i++ + ") "+failure+"("+failure.ThrownException.GetType().ToString()+")");
				Writer.Write(failure.ThrownException);
			}
		}
	}

	/// <summary>Prints failures to the standard output.</summary>
	public static void PrintFailures(TestResult result) {
		if (result.FailureCount != 0) {
			if (result.FailureCount == 1)
				Writer.WriteLine("There was " + result.FailureCount + " failure:");
			else
				Writer.WriteLine("There were " + result.FailureCount + " failures:");
			int i = 1;
			foreach (TestFailure failure in result.Failures) {
				Writer.Write(i++ + ") " + failure.FailedTest);
				Exception t= failure.ThrownException;
				if (t.Message != "")
					Writer.WriteLine(" \"" + t.Message + "\"");
				else {
					Writer.WriteLine();
					Writer.Write(failure.ThrownException);
				}
			}
		}
	}

	/// <summary>Prints the header of the report.</summary>
	public static void PrintHeader(TestResult result) {
		if (result.WasSuccessful) {
			Writer.WriteLine();
			Writer.Write("OK");
			Writer.WriteLine (" (" + result.RunCount + " tests)");
			
		} else {
			Writer.WriteLine();
			Writer.WriteLine("FAILURES!!!");
			Writer.WriteLine("Tests Run: "+result.RunCount+ 
					 ", Failures: "+result.FailureCount+
					 ", Errors: "+result.ErrorCount);
		}
	}

}

}
