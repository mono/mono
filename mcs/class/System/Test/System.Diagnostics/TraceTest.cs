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
			Trace.Listeners.Clear ();
			Trace.Listeners.Add (listener);
			Trace.AutoFlush = true;
		}

		protected override void TearDown ()
		{
			// Trace.Listeners.Add (new DefaultTraceListener ());
			Trace.Listeners.Remove (listener);
		}

		public static ITest Suite {
 			get { 
				return new TestSuite (typeof (TraceTest)); 
			}
		}

		// Make sure that when we get the output we expect....
		public void TestTracing ()
		{
			Trace.IndentLevel = 0;
			Trace.IndentSize = 4;

			string value =  
				"Entering Main" + Environment.NewLine +
				"Exiting Main" + Environment.NewLine;

			Trace.WriteLine ("Entering Main");
			Trace.WriteLine ("Exiting Main");

			AssertEquals ("#Tr01", value, buffer.ToString ());
		}

		// Make sure we get the output we expect in the presence of indenting...
		public void TestIndent ()
		{
			Trace.IndentLevel = 0;
			Trace.IndentSize = 4;

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

		// Make sure that TraceListener properties (IndentLevel, IndentSize) are
		// modified when the corresponding Trace properties are changed.
		public void TestAddedTraceListenerProperties ()
		{
			TraceListener t1 = new TextWriterTraceListener (Console.Out);
			TraceListener t2 = new TextWriterTraceListener (Console.Error);
			Trace.Listeners.Add(t1);
			Trace.Listeners.Add(t2);

			const int ExpectedSize = 5;
			const int ExpectedLevel = 2;

			Trace.IndentSize = ExpectedSize;
			Trace.IndentLevel = ExpectedLevel;

			foreach (TraceListener t in Trace.Listeners) {
				string ids = "#TATLP-S-" + t.Name;
				string idl = "#TATLP-L-" + t.Name;
				AssertEquals (ids, ExpectedSize, t.IndentSize);
				AssertEquals (idl, ExpectedLevel, t.IndentLevel);
			}

			Trace.Listeners.Remove(t1);
			Trace.Listeners.Remove(t2);
		}

		// Make sure that the TraceListener properties (IndentLevel, IndentSize)
		// are properly modified when the TraceListener is added to the
		// collection.
		public void TestListeners_Add_Values()
		{
			const int ExpectedLevel = 0;
			const int ExpectedSize = 4;
			Trace.IndentLevel = ExpectedLevel;
			Trace.IndentSize = ExpectedSize;
			TraceListener tl = new TextWriterTraceListener(Console.Out);

			tl.IndentLevel = 2*ExpectedLevel;
			tl.IndentSize = 2*ExpectedSize;

			Trace.Listeners.Add(tl);

			// Assert that the listener we added has been set to the correct indent
			// level.
			AssertEquals ("#LATL-L", ExpectedLevel, tl.IndentLevel);
			AssertEquals ("#LATL-S", ExpectedSize, tl.IndentSize);

			// Assert that all listeners in the collection have the same level.
			foreach (TraceListener t in Trace.Listeners)
			{
				string idl = "#LATL-L:" + t.Name;
				string ids = "#LATL-S:" + t.Name;
				AssertEquals(idl, ExpectedLevel, t.IndentLevel);
				AssertEquals(ids, ExpectedSize, t.IndentSize);
			}
		}

		// IndentSize, IndentLevel are thread-static
	}
}

