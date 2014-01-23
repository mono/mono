//
// TraceTest.cs - NUnit Test Cases for System.Diagnostics.Trace
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Jonathan Pryor
// (C) 2003 Martin Willemoes Hansen
// 

#if !MOBILE

// We want tracing enabled, so...
#define TRACE

using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace MonoTests.System.Diagnostics {

	[TestFixture]
	public class TraceTest {
    
		private StringWriter buffer;
		private TraceListener listener;

		[SetUp]
		public void GetReady ()
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

		[TearDown]
		public void Clear ()
		{
			// Trace.Listeners.Add (new DefaultTraceListener ());
			Trace.Listeners.Remove (listener);
		}

		// Make sure that when we get the output we expect....
		[Test]
		public void Tracing ()
		{
			Trace.IndentLevel = 0;
			Trace.IndentSize = 4;

			string value =  
				"Entering Main" + Environment.NewLine +
				"Exiting Main" + Environment.NewLine;

			Trace.WriteLine ("Entering Main");
			Trace.WriteLine ("Exiting Main");

			Assert.AreEqual (value, buffer.ToString (), "#Tr01");
		}

		// Make sure we get the output we expect in the presence of indenting...
		[Test]
		public void Indent ()
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
			Assert.AreEqual (1, Trace.IndentLevel);
			Trace.WriteLine ("Error 1: File not found");
			Trace.WriteLine ("Error 2: Directory not found");
			Trace.Unindent ();
			Assert.AreEqual (0, Trace.IndentLevel);
			Trace.WriteLine ("End of list of errors");

			Assert.AreEqual (value, buffer.ToString(), "#In01");
		}

		// Make sure that TraceListener properties (IndentLevel, IndentSize) are
		// modified when the corresponding Trace properties are changed.
		[Test]
		public void AddedTraceListenerProperties ()
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
				Assert.AreEqual (ExpectedSize, t.IndentSize, ids);
				Assert.AreEqual (ExpectedLevel, t.IndentLevel, idl);
			}

			Trace.Listeners.Remove(t1);
			Trace.Listeners.Remove(t2);
		}

		// Make sure that the TraceListener properties (IndentLevel, IndentSize)
		// are properly modified when the TraceListener is added to the
		// collection.
		[Test]
		public void Listeners_Add_Values()
		{
			const int ExpectedLevel = 0;
			const int ExpectedSize = 4;
			Trace.IndentLevel = ExpectedLevel;
			Trace.IndentSize = ExpectedSize;
			TraceListener tl = new TextWriterTraceListener(Console.Out);

			tl.IndentLevel = 2*ExpectedLevel;
			tl.IndentSize = 2*ExpectedSize;

			Trace.Listeners.Add(tl);

			// Assertion.Assert that the listener we added has been set to the correct indent
			// level.
			Assert.AreEqual (ExpectedLevel, tl.IndentLevel, "#LATL-L");
			Assert.AreEqual (ExpectedSize, tl.IndentSize, "#LATL-S");

			// Assertion.Assert that all listeners in the collection have the same level.
			foreach (TraceListener t in Trace.Listeners)
			{
				string idl = "#LATL-L:" + t.Name;
				string ids = "#LATL-S:" + t.Name;
				Assert.AreEqual (ExpectedLevel, t.IndentLevel, idl);
				Assert.AreEqual (ExpectedSize, t.IndentSize, ids);
			}
		}

		// IndentSize, IndentLevel are thread-static

		class MyTraceListener : TraceListener
		{
			public int Writes;
			public int WriteLines;

			public MyTraceListener ()
				: base ("mt-test")
			{
			}

			public override void Write (string msg)
			{
				++Writes;
			}

			public override void WriteLine (string msg)
			{
				++WriteLines;
			}
		}

		class MultiThreadModify
		{
			public MyTraceListener listener = new MyTraceListener ();

			public const int MaxIterations = 10000;

			public String Exception = null;

			public MultiThreadModify ()
			{
				Trace.Listeners.Add (listener);
			}

			public void Write ()
			{
				try {
					for (int i = 0; i < MaxIterations; ++i)
						Trace.WriteLine ("message " + i + "... ");
				}
				catch (Exception e) {
					Exception = string.Format (
							"#MTMW: Exception emitted from Trace.WriteLine: {0}", e);
				}
			}

			public void Remove ()
			{
				try {
					Trace.Listeners.Remove (listener);
				}
				catch (Exception e) {
					Exception = string.Format (
							"#MTMR: Exception emitted from Trace.Listeners.Remove: {0}", e);
				}
			}
		}

		[Test]
		[Category ("NotWorking")]
		// Is this even valid !?!?!?!
		public void TestMultiThreadModify ()
		{
			MultiThreadModify m = new MultiThreadModify ();

			Thread t1 = new Thread (new ThreadStart (m.Write));
			Thread t2 = new Thread (new ThreadStart (m.Remove));

			t1.Start ();
			t2.Start ();

			t1.Join ();
			t2.Join ();

			Assert.IsTrue (m.Exception == null, m.Exception);
			Assert.AreEqual (MultiThreadModify.MaxIterations, m.listener.WriteLines,
					"#tmtm: listener was removed before iterations were completed");
		}
	}
}

#endif
