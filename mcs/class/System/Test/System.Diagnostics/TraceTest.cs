//
// TraceTest.cs - NUnit Test Cases for System.Diagnostics.Trace
//
// Jonathan Pryor (jonpryor@vt.edu)
//
// (C) Jonathan Pryor
// 

// We want tracing enabled, so...
#define TRACE

using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;

namespace MonoTests.System.Diagnostics {

	public class TraceTest : TestCase {
    
		private StringWriter buffer;
		private TraceListener listener;

		public TraceTest () 
			: base ("System.Diagnostics.Trace testsuite")
		{
		}

		public TraceTest (string name)
			: base(name)
		{
		}

		protected override void SetUp ()
		{
			// We don't want to deal with the default listener, which can send the
			// output to various places (Debug stream, Console.Out, ...)
			// Trace.Listeners.Remove ("Default");

			buffer = new StringWriter ();

			listener = new TextWriterTraceListener (buffer, "TestOutput");

			Trace.Listeners.Add (listener);

			Trace.AutoFlush = true;

		}

		protected override void TearDown ()
		{
			Trace.Listeners.Add (new DefaultTraceListener ());
			Trace.Listeners.Remove (listener);
		}

    public static ITest Suite {
 			get { 
				return new TestSuite (typeof (TraceTest)); 
			}
		}

		public void TestTracing ()
		{
			string value =  
				"Entering Main" + Environment.NewLine +
				"Exiting Main" + Environment.NewLine;

			Trace.WriteLine ("Entering Main");
			Trace.WriteLine ("Exiting Main");

			AssertEquals ("#Tr01", value, buffer.ToString ());
		}

		public void TestIndent ()
		{
			Console.Error.WriteLine ("TraceTest.TestIndent");
			string value =  
				"List of errors:" + Environment.NewLine +
				"    Error 1: File not found" + Environment.NewLine +
				"    Error 2: Directory not found" + Environment.NewLine +
				"End of list of errors" + Environment.NewLine;

			Trace.WriteLine ("List of errors:");
			Trace.Indent ();
			Trace.WriteLine ("Error 1: File not found");
			Trace.WriteLine ("Error 2: Directory not found");
			Trace.Unindent ();
			Trace.WriteLine ("End of list of errors");

			AssertEquals ("#In01", value, buffer.ToString());
		}
	}
}

