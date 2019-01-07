//
// ExceptionTest.cs - NUnit Test Cases for the System.Exception class
//
// Authors:
//	Linus Upson (linus@linus.com)
//	Duncan Mak (duncan@ximian.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Runtime.Serialization;

using System.Reflection;

using NUnit.Framework;

using System.Threading.Tasks;

namespace MonoTests.System
{
	[TestFixture]
	public class ExceptionTest
	{
		[Test] // .ctor (SerializationInfo, StreamingContext)
		public void Constructor3 ()
		{
			SerializationInfo si;
			MyException ex;
			Exception inner;

			inner = new ArgumentException ();
			si = new SerializationInfo (typeof (Exception),
				new FormatterConverter ());
			si.AddValue ("ClassName", "CLASS");
			si.AddValue ("Message", "MSG");
			si.AddValue ("InnerException", inner, typeof (Exception));
			si.AddValue ("HelpURL", "URL");
			si.AddValue ("StackTraceString", null);
			si.AddValue ("RemoteStackTraceString", null);
			si.AddValue ("RemoteStackIndex", 0);
			si.AddValue ("HResult", 10);
			si.AddValue ("Source", "SRC");
			si.AddValue ("ExceptionMethod", null);
			Hashtable data = new Hashtable ();
			data.Add ("XX", "ZZ");
			si.AddValue ("Data", data, typeof (IDictionary));

			ex = new MyException (si, new StreamingContext ());
			Assert.AreEqual ("MSG", ex.Message, "#A1");
			Assert.AreSame (inner, ex.InnerException, "#A2");
			Assert.AreEqual ("URL", ex.HelpLink, "#A3");
			Assert.AreEqual (10, ex.HResult, "#A4");
			Assert.AreEqual ("SRC", ex.Source, "#A5");
			Assert.IsNotNull (ex.Data, "#A6");
			Assert.AreEqual (1, ex.Data.Keys.Count, "#A7");
			Assert.AreEqual ("ZZ", ex.Data ["XX"], "#A8");

			inner = null;
			si = new SerializationInfo (typeof (Exception),
				new FormatterConverter ());
			si.AddValue ("ClassName", "CLASS");
			si.AddValue ("Message", null);
			si.AddValue ("InnerException", inner, typeof (Exception));
			si.AddValue ("HelpURL", "URL");
			si.AddValue ("StackTraceString", null);
			si.AddValue ("RemoteStackTraceString", null);
			si.AddValue ("RemoteStackIndex", 0);
			si.AddValue ("HResult", 10);
			si.AddValue ("Source", "SRC");
			si.AddValue ("ExceptionMethod", null);

			ex = new MyException (si, new StreamingContext ());
			Assert.IsNotNull (ex.Message, "#B1");
			Assert.IsTrue (ex.Message.IndexOf ("CLASS") != -1, "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.AreEqual ("URL", ex.HelpLink, "#B4");
			Assert.AreEqual (10, ex.HResult, "#B5");
			Assert.AreEqual ("SRC", ex.Source, "#B6");
			Assert.IsNotNull (ex.Data, "#B7");
			Assert.AreEqual (0, ex.Data.Keys.Count, "#B8");
		}


		// This test makes sure that exceptions thrown on block boundaries are
		// handled in the correct block. The meaning of the 'caught' variable is
		// a little confusing since there are two catchers: the method being
		// tested the the method calling the test. There is probably a better
		// name, but I can't think of it right now.
		[Test]
		public void TestThrowOnBlockBoundaries ()
		{
			bool caught;
			
			try {
				caught = false;
				ThrowBeforeTry();
			} catch {
				caught = true;
			}
			Assert.IsTrue (caught, "Exceptions thrown before try blocks should not be caught");
			
			try {
				caught = false;
				ThrowAtBeginOfTry();
			} catch {
				caught = true;
			}
			Assert.IsFalse (caught, "Exceptions thrown at begin of try blocks should be caught");

			try {
				caught = false;
				ThrowAtEndOfTry();
			} catch {
				caught = true;
			}
			Assert.IsFalse (caught, "Exceptions thrown at end of try blocks should be caught");

			try {
				caught = false;
				ThrowAtBeginOfCatch();
			} catch {
				caught = true;
			}
			Assert.IsTrue (caught, "Exceptions thrown at begin of catch blocks should not be caught");

			try {
				caught = false;
				ThrowAtEndOfCatch();
			} catch {
				caught = true;
			}
			Assert.IsTrue (caught, "Exceptions thrown at end of catch blocks should not be caught");

			try {
				caught = false;
				ThrowAtBeginOfFinally();
			} catch {
				caught = true;
			}
			Assert.IsTrue (caught, "Exceptions thrown at begin of finally blocks should not be caught");

			try {
				caught = false;
				ThrowAtEndOfFinally();
			} catch {
				caught = true;
			}
			Assert.IsTrue (caught, "Exceptions thrown at end of finally blocks should not be caught");

			try {
				caught = false;
				ThrowAfterFinally();
			} catch {
				caught = true;
			}
			Assert.IsTrue (caught, "Exceptions thrown after finally blocks should not be caught");
		}
		
		private static void DoNothing()
		{
		}

		private static void ThrowException()
		{
			throw new Exception();
		}

		private static void ThrowBeforeTry()
		{
			ThrowException();
			try {
				DoNothing();
			} catch (Exception) {
				DoNothing();
			}
		}

		private static void ThrowAtBeginOfTry()
		{
			DoNothing();
			try {
				ThrowException();
				DoNothing();
			} catch (Exception) {
				DoNothing();
			}
		}

		private static void ThrowAtEndOfTry()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			}
		}

		private static void ThrowAtBeginOfCatch()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				throw;
			}
		}

		private static void ThrowAtEndOfCatch()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
				throw;
			}
		}

		private static void ThrowAtBeginOfFinally()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			} finally {
				ThrowException();
				DoNothing();
			}
		}

		private static void ThrowAtEndOfFinally()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			} finally {
				DoNothing();
				ThrowException();
			}
		}

		private static void ThrowAfterFinally()
		{
			DoNothing();
			try {
				DoNothing();
				ThrowException();
			} catch (Exception) {
				DoNothing();
			} finally {
				DoNothing();
			}
			ThrowException();
		}

		[Test]
		public void GetObjectData ()
		{
			string msg = "MESSAGE";
			Exception inner = new ArgumentException ("whatever");
			SerializationInfo si;
			Exception se;

			se = new Exception (msg, inner);
			si = new SerializationInfo (typeof (Exception),
				new FormatterConverter ());
			se.GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (11, si.MemberCount, "#A1");
			Assert.AreEqual (typeof (Exception).FullName, si.GetString ("ClassName"), "#A2");
			Assert.IsNull (si.GetValue ("Data", typeof (IDictionary)), "#A3");
			Assert.AreSame (msg, si.GetString ("Message"), "#A4");
			Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#A5");
			Assert.AreSame (se.HelpLink, si.GetString ("HelpURL"), "#A6");
			Assert.IsNull (si.GetString ("StackTraceString"), "#A7");
			Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#A8");
			Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#A9");
			Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#A10");
			Assert.IsNull (si.GetString ("Source"), "#A11");
			Assert.IsNull (si.GetString ("ExceptionMethod"), "#A12");

			// attempt initialization of lazy init members
			Assert.IsNotNull (se.Data);
			Assert.IsNull (se.Source);
			Assert.IsNull (se.StackTrace);

			si = new SerializationInfo (typeof (Exception),
				new FormatterConverter ());
			se.GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (11, si.MemberCount, "#B1");
			Assert.AreEqual (typeof (Exception).FullName, si.GetString ("ClassName"), "#B2");
			Assert.AreSame (se.Data, si.GetValue ("Data", typeof (IDictionary)), "#B3");
			Assert.AreSame (msg, si.GetString ("Message"), "#B4");
			Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#B5");
			Assert.AreSame (se.HelpLink, si.GetString ("HelpURL"), "#B6");
			Assert.IsNull (si.GetString ("StackTraceString"), "#B7");
			Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#B8");
			Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#B9");
			Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#B10");
			Assert.IsNull (si.GetString ("Source"), "#B11");
			Assert.IsNull (si.GetString ("ExceptionMethod"), "#B12");

			try {
				throw new Exception (msg, inner);
			} catch (Exception ex) {
				si = new SerializationInfo (typeof (Exception),
					new FormatterConverter ());
				ex.GetObjectData (si, new StreamingContext ());
				Assert.AreEqual (11, si.MemberCount, "#C1");
				Assert.AreEqual (typeof (Exception).FullName, si.GetString ("ClassName"), "#C2");
				Assert.IsNull (si.GetValue ("Data", typeof (IDictionary)), "#C3");
				Assert.AreSame (msg, si.GetString ("Message"), "#C4");
				Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#C5");
				Assert.AreSame (se.HelpLink, si.GetString ("HelpURL"), "#C6");
				Assert.IsNotNull (si.GetString ("StackTraceString"), "#C7");
				Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#C8");
				Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#C9");
				Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#C10");
				Assert.IsNotNull (si.GetString ("Source"), "#C11");
				//Assert.IsNotNull (si.GetString ("ExceptionMethod"), "#C12");
			}

			try {
				throw new Exception (msg, inner);
			} catch (Exception ex) {
				// force initialization of lazy init members
				Assert.IsNotNull (ex.Data);
				Assert.IsNotNull (ex.StackTrace);

				si = new SerializationInfo (typeof (Exception),
					new FormatterConverter ());
				ex.GetObjectData (si, new StreamingContext ());
				Assert.AreEqual (11, si.MemberCount, "#D1");
				Assert.AreEqual (typeof (Exception).FullName, si.GetString ("ClassName"), "#D2");
				Assert.AreSame (ex.Data, si.GetValue ("Data", typeof (IDictionary)), "#D3");
				Assert.AreSame (msg, si.GetString ("Message"), "#D4");
				Assert.AreSame (inner, si.GetValue ("InnerException", typeof (Exception)), "#D5");
				Assert.AreSame (ex.HelpLink, si.GetString ("HelpURL"), "#D6");
				Assert.IsNotNull (si.GetString ("StackTraceString"), "#D7");
				Assert.IsNull (si.GetString ("RemoteStackTraceString"), "#D8");
				Assert.AreEqual (0, si.GetInt32 ("RemoteStackIndex"), "#D9");
				Assert.AreEqual (-2146233088, si.GetInt32 ("HResult"), "#D10");
				Assert.AreEqual (typeof (ExceptionTest).Assembly.GetName ().Name, si.GetString ("Source"), "#D11");
				//Assert.IsNotNull (si.GetString ("ExceptionMethod"), "#D12");
			}
		}

		[Test]
		public void GetObjectData_Info_Null ()
		{
			Exception e = new Exception ();
			try {
				e.GetObjectData (null, new StreamingContext ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("info", ex.ParamName, "#5");
			}
		}

		[Test]
		public void HResult ()
		{
			MyException ex = new MyException ();
			Assert.AreEqual (-2146233088, ex.HResult, "#1");
			ex.HResult = int.MaxValue;
			Assert.AreEqual (int.MaxValue, ex.HResult, "#2");
			ex.HResult = int.MinValue;
			Assert.AreEqual (int.MinValue, ex.HResult, "#3");
		}

		[Test]
		public void Source ()
		{
			Exception ex1 = new Exception ("MSG");
			Assert.IsNull (ex1.Source, "#1");

			try {
				throw new Exception ("MSG");
			} catch (Exception ex2) {
				Assert.AreEqual (typeof (ExceptionTest).Assembly.GetName ().Name, ex2.Source, "#2");
			}
		}

		[Test]
		public void Source_InnerException ()
		{
			Exception a = new Exception ("a", new ArgumentException ("b"));
			a.Source = "foo";

			Assert.IsNull (a.InnerException.Source);
		}

#if !MOBILE
		// Ensure that we can convert a stacktrace to a
		// telemetry message
		//
		[Test]
		[Category("NotOnWindows")]
		public void StacktraceToState ()
		{
			try {
				throw new Exception ("#0");
			} catch (Exception exc) {
				var monoType = Type.GetType ("Mono.Runtime", false);
				var convert = monoType.GetMethod("ExceptionToState", BindingFlags.NonPublic | BindingFlags.Static);
				object [] convert_params = new object[] {exc};
				var output = (Tuple<String, ulong, ulong>) convert.Invoke(null, convert_params);

				var dump = output.Item1;
				var portable_hash = output.Item2;
				var unportable_hash = output.Item3;

				Assert.IsTrue (portable_hash != 0, "#1");
				Assert.IsTrue (unportable_hash != 0, "#2");
				Assert.IsTrue (dump.Length > 0, "#3");

				// Console.WriteLine (dump);
				// dump should look something like:
				// {
				//  "protocol_version" : "0.0.1",
				//  "configuration" : {
				//    "version" : "5.19.0 (managed_telemetry_pipeline/d342c73e320 Wed Aug 15 14:40:40 EDT 2018)",
				//    "tlc" : "normal",
				//    "sigsgev" : "altstack",
				//    "notifications" : "kqueue",
				//    "architecture" : "amd64",
				//    "disabled_features" : "none",
				//    "smallconfig" : "disabled",
				//    "bigarrays" : "disabled",
				//    "softdebug" : "enabled",
				//    "interpreter" : "enabled",
				//    "llvm_support" : "disabled",
				//    "suspend" : "hybrid"
				//  },
				//  "memory" : {
				//    "Resident Size" : "40693760",
				//    "Virtual Size" : "4521312256",
				//    "minor_gc_time" : "216992",
				//    "major_gc_time" : "0",
				//    "minor_gc_count" : "6",
				//    "major_gc_count" : "0",
				//    "major_gc_time_concurrent" : "0"
				//  },
				//  "threads" : [
				//    {
				//      "is_managed" : false,
				//      "managed_thread_ptr" : "0x0",
				//      "thread_info_addr" : "0x0",
				//      "native_thread_id" : "0x0",
				//      "managed_exception_type" : "System.Exception",
				//      "managed_frames" : [
				//        {
				//          "is_managed" : "true",
				//          "guid" : "43A03618-E657-44B0-B9FA-F63314A3C1B2",
				//          "token" : "0x60008da",
				//          "native_offset" : "0xb2",
				//          "il_offset" : "0x00000"
				//        }
				//      ]
				//    }
				//  ]
				//  }
			}
		}

		void DumpSingle ()
		{
			var monoType = Type.GetType ("Mono.Runtime", false);
			var convert = monoType.GetMethod("DumpStateSingle", BindingFlags.NonPublic | BindingFlags.Static);
			var output = (Tuple<String, ulong, ulong>) convert.Invoke(null, Array.Empty<object> ());

			var dump = output.Item1;
			var portable_hash = output.Item2;
			var unportable_hash = output.Item3;

			Assert.IsTrue (portable_hash != 0, "#1");
			Assert.IsTrue (unportable_hash != 0, "#2");
			Assert.IsTrue (dump.Length > 0, "#3");
		}

		void DumpTotal ()
		{
			var monoType = Type.GetType ("Mono.Runtime", false);
			var convert = monoType.GetMethod("DumpStateTotal", BindingFlags.NonPublic | BindingFlags.Static);
			var output = (Tuple<String, ulong, ulong>) convert.Invoke(null, Array.Empty<object> ());

			var dump = output.Item1;
			var portable_hash = output.Item2;
			var unportable_hash = output.Item3;

			Assert.IsTrue (portable_hash != 0, "#1");
			Assert.IsTrue (unportable_hash != 0, "#2");
			Assert.IsTrue (dump.Length > 0, "#3");
		}

		void DumpLogSet ()
		{
			var monoType = Type.GetType ("Mono.Runtime", false);
			var convert = monoType.GetMethod("EnableCrashReportLog", BindingFlags.NonPublic | BindingFlags.Static);
			convert.Invoke(null, new object[] { "./" });
		}

		void DumpLogUnset ()
		{
			var monoType = Type.GetType ("Mono.Runtime", false);
			var convert = monoType.GetMethod("EnableCrashReportLog", BindingFlags.NonPublic | BindingFlags.Static);
			convert.Invoke(null, new object[] { null });
		}

		void DumpLogCheck ()
		{
			var monoType = Type.GetType ("Mono.Runtime", false);
			var convert = monoType.GetMethod("CheckCrashReportLog", BindingFlags.NonPublic | BindingFlags.Static);
			var result = (int) convert.Invoke(null, new object[] { "./", true });
			var monoSummaryDone = 8;
			Assert.AreEqual (monoSummaryDone, result, "#DLC1");
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallTotalLogged ()
		{
			DumpLogSet ();
			DumpTotal ();
			DumpLogUnset ();
			DumpLogCheck ();
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallSingleOnce ()
		{
			DumpSingle ();
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallSingleConcurrent ()
		{
			// checks that self-dumping works in parallel, locklessly
			int amt = 20;
			var tasks = new Task [amt];
			for (int i=0; i < amt; i++)
				tasks [i] = Task.Run(() => DumpSingle ());
			Task.WaitAll (tasks);
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallSingleAsync ()
		{
			// checks that dumping works in an async context
			var t = Task.Run(() => DumpSingle ());
			t.Wait ();
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallTotalOnce ()
		{
			DumpTotal ();
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallTotalRepeated ()
		{
			// checks that the state doesn't get broken with repeated use
			DumpTotal ();
			DumpTotal ();
			DumpTotal ();
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallTotalAsync ()
		{
			// checks that dumping works in an async context
			var t = Task.Run(() => DumpTotal ());
			t.Wait ();
		}

		[Test]
		[Category("NotOnWindows")]
		public void DumpICallTotalConcurrent ()
		{
			int amt = 3;
			var tasks = new Task [amt];
			for (int i=0; i < amt; i++)
				tasks [i] = Task.Run(() => DumpTotal ());
			Task.WaitAll (tasks);
		}

#endif

		[Test]
		public void StackTrace ()
		{
			Exception ex1 = new Exception ("MSG");
			Assert.IsNull (ex1.StackTrace, "#1");

			try {
				throw new Exception ("MSG");
			} catch (Exception ex2) {
				Assert.IsNotNull (ex2.StackTrace, "#2");
			}
		}

		class MyException : Exception {
			public MyException ()
			{
			}

			public MyException (SerializationInfo info, StreamingContext context)
				: base (info, context)
			{
			}

			public new int HResult {
				get { return base.HResult; }
				set { base.HResult = value; }
			}
		}
	}
}
