//
// MonoTests.System.Diagnostics.StackFrameTest.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//
// (C) 2001
//

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace MonoTests.System.Diagnostics
{
	/// <summary>
	/// Tests the case where StackFrame is created for specified file name and
	/// location inside it.
	/// </summary>
	[TestFixture]
	public class StackFrameTest1
	{
		private StackFrame frame1;
		private StackFrame frame2;

		[SetUp]
		public void SetUp ()
		{
			frame1 = new StackFrame ("dir/someFile", 13, 45);
			frame2 = new StackFrame ("SomeFile2.cs", 24);
		}

		[TearDown]
		public void TearDown ()
		{
			frame1 = null;
			frame2 = null;
		}

		/// <summary>
		///   Tests whether getting file name works.
		/// </summary>
		[Test]
		[Category("LLVMNotWorking")]
		public void TestGetFileName ()
		{
			Assert.AreEqual ("dir/someFile",
						 frame1.GetFileName (),
						 "File name (1)");

			Assert.AreEqual ("SomeFile2.cs",
						 frame2.GetFileName (),
						 "File name (2)");
		}

		/// <summary>
		/// Tests whether getting file line number works.
		/// </summary>
		[Test]
		[Category("LLVMNotWorking")]
		public void TestGetFileLineNumber ()
		{
			Assert.AreEqual (13,
							 frame1.GetFileLineNumber (),
							 "Line number (1)");

			Assert.AreEqual (24,
							 frame2.GetFileLineNumber (),
							 "Line number (2)");
		}

		/// <summary>
		/// Tests whether getting file column number works.
		/// </summary>
		[Test]
		public void TestGetFileColumnNumber ()
		{
			Assert.AreEqual (45,
							 frame1.GetFileColumnNumber (),
							 "Column number (1)");

			Assert.AreEqual (0,
							 frame2.GetFileColumnNumber (),
							 "Column number (2)");
		}

		/// <summary>
		/// Tests whether getting method associated with frame works.
		/// </summary>
		[Test]
		[Category("StackWalks")]
		[Category("NotWasm")]
		public void TestGetMethod ()
		{
			Assert.IsTrue ((frame1.GetMethod () != null), "Method not null (1)");

			Assert.AreEqual (this.GetType (),
							 frame1.GetMethod ().DeclaringType,
							 "Class declaring the method (1)");
			Assert.AreEqual ("SetUp",
							 frame1.GetMethod ().Name,
							 "Method name (1)");

			Assert.IsTrue ((frame2.GetMethod () != null), "Method not null (2)");

			Assert.AreEqual (this.GetType (),
							 frame2.GetMethod ().DeclaringType,
							 "Class declaring the method (2)");
			Assert.AreEqual ("SetUp",
							 frame2.GetMethod ().Name,
							 "Method name (2)");
		}
	}

	/// <summary>
	/// Tests the case where StackFrame is created for current method.
	/// </summary>
	/// <remarks>
	/// FIXME: Must be compiled with /debug switch. Otherwise some file
	/// information will be incorrect for the following test cases.
	/// What's the best way to do both types of tests with and without
	/// debug information?
	/// </remarks>
	[TestFixture]
	public class StackFrameTest2
	{
		private StackFrame frame1;
		private StackFrame frame2;
		private StackFrame frame3;

		[SetUp]
		public void SetUp ()
		{
			frame1 = new StackFrame ();
			frame2 = new StackFrame (true);
			frame3 = new StackFrame (0);
		}

		[TearDown]
		public void TearDown ()
		{
			frame1 = null;
			frame2 = null;
			frame3 = null;
		}

		/// <summary>
		/// Tests whether getting file name works.
		/// </summary>
		[Test]
		public void TestGetFileName1 ()
		{
			Assert.IsNull (frame1.GetFileName (),
						   "File name (1)");
		}

		[Test]
		[Category ("LLVMNotWorking")]
		public void TestGetFileName2 ()
		{
#if MOBILE && !DEBUG
			Assert.Ignore ("The .mdb file won't be present inside the app and no file name will be available");
#endif
			Assert.IsNotNull (frame2.GetFileName (), "File name not null");
			Assert.IsTrue (frame2.GetFileName ().Length != 0, "File name not empty");
			Assert.IsTrue (frame2.GetFileName ().EndsWith ("StackFrameTest.cs"),
						   "File name (2) " + frame2.GetFileName () + " ends with StackFrameTest.cs");
		}

		/// <summary>
		/// Tests whether getting file line number works.
		/// </summary>
		[Test]
		[Category ("LLVMNotWorking")]
		public void TestGetFileLineNumber ()
		{
#if MOBILE && !DEBUG
			Assert.Ignore ("The .mdb file won't be present inside the app and no line number will be available");
#endif
			Assert.AreEqual (0,
							 frame1.GetFileLineNumber (),
							 "Line number (1)");

			Assert.AreEqual (138,
							 frame2.GetFileLineNumber (),
							 "Line number (2)");

			Assert.AreEqual (0,
							 frame3.GetFileLineNumber (),
							 "Line number (3)");
		}

		/// <summary>
		/// Tests whether getting file column number works.
		/// </summary>
		[Test]
		[Category ("NotWorking")] // bug #45730 - Column numbers always zero
		public void TestGetFileColumnNumber ()
		{
			Assert.AreEqual (0,
							 frame1.GetFileColumnNumber (),
							 "Column number (1)");

			Assert.AreEqual (4,
							 frame2.GetFileColumnNumber (),
							 "Column number (2)");

			Assert.AreEqual (0,
							 frame3.GetFileColumnNumber (),
							 "Column number (3)");
		}

		/// <summary>
		/// Tests whether getting method associated with frame works.
		/// </summary>
		[Test]
		[Category("StackWalks")]
		[Category("NotWasm")]
		public void TestGetMethod ()
		{
			Assert.IsNotNull (frame1.GetMethod (),
							  "Method not null (1)");

			Assert.AreEqual (this.GetType (),
							 frame1.GetMethod ().DeclaringType,
							 "Class declaring the method (1)");
			Assert.AreEqual ("SetUp",
							 frame1.GetMethod ().Name,
							 "Method name (1)");

			Assert.IsNotNull (frame2.GetMethod (),
							  "Method not null (2)");

			Assert.AreEqual (this.GetType (),
							 frame2.GetMethod ().DeclaringType,
							 "Class declaring the method (2)");
			Assert.AreEqual ("SetUp",
							 frame2.GetMethod ().Name,
							 "Method name (2)");

			Assert.IsNotNull (frame3.GetMethod (),
							  "Method not null (3)");

			Assert.AreEqual (this.GetType (),
							 frame3.GetMethod ().DeclaringType,
							 "Class declaring the method (3)");
			Assert.AreEqual ("SetUp",
							 frame3.GetMethod ().Name,
							 "Method name (3)");
		}
	}

	/// <summary>
	/// Tests the case where StackFrame is created for current method but
	/// skipping some frames.
	/// </summary>
	/// <remarks>
	/// FIXME: Must be compiled with /debug switch. Otherwise some file
	/// information will be incorrect for the following test cases.
	/// What's the best way to do both types of tests with and without
	/// debug information?
	/// </remarks>
	[TestFixture]
	public class StackFrameTest3
	{
		protected StackFrame frame1;
		protected StackFrame frame2;

		[SetUp]
		public void SetUp ()
		{
			// In order to get better test cases with stack traces
			NestedSetUp ();
		}

		private void NestedSetUp ()
		{
			frame1 = new StackFrame (2);
			frame2 = new StackFrame (1, true);
			// Without this access of frame2 on the RHS, none of 
			// the properties or methods seem to return any data ???
			string s = frame2.GetFileName ();
		}

		[TearDown]
		public void TearDown ()
		{
			frame1 = null;
			frame2 = null;
		}

		/// <summary>
		/// Tests whether getting file name works.
		/// </summary>
		[Test]
		[Category ("LLVMNotWorking")]
		public void TestGetFileName ()
		{
#if MOBILE && !DEBUG
			Assert.Ignore ("The .mdb file won't be present inside the app and no file name will be available");
#endif
			Assert.IsNull (frame1.GetFileName (),
						   "File name (1)");

			Assert.IsNotNull (frame2.GetFileName (),
							  "File name (2) should not be null");

			Assert.IsTrue (frame2.GetFileName ().EndsWith ("StackFrameTest.cs"),
						   "File name (2) " + frame2.GetFileName () + " ends with StackFrameTest.cs");
		}

		/// <summary>
		///   Tests whether getting file line number works.
		/// </summary>
		[Test]
		[Category ("LLVMNotWorking")]
		public void TestGetFileLineNumber ()
		{
#if MOBILE && !DEBUG
			Assert.Ignore ("The .mdb file won't be present inside the app and no line number will be available");
#endif
			Assert.AreEqual (0,
							 frame1.GetFileLineNumber (),
							 "Line number (1)");

			Assert.AreEqual (276,
							 frame2.GetFileLineNumber (),
							 "Line number (2)");
		}

		/// <summary>
		/// Tests whether getting file column number works.
		/// </summary>
		[Test]
		[Category ("NotWorking")] // bug #45730 - Column numbers always zero
		public void TestGetFileColumnNumber ()
		{
			Assert.AreEqual (0,
						 frame1.GetFileColumnNumber (),
							 "Column number (1)");

			Assert.AreEqual (4,
							 frame2.GetFileColumnNumber (),
							 "Column number (2)");
		}

		/// <summary>
		/// Test whether GetFrames contains the frames for nested exceptions
		/// </summary>
		[Test]
		[Category("StackWalks")]
		public void GetFramesAndNestedExc ()
		{
			try
			{
				throw new Exception("This is a test");
			}
			catch (Exception e)
			{
				try
				{
					ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
				}
				catch (Exception ee)
				{
					StackTrace st = new StackTrace(ee, true);
					StackFrame[] frames = st.GetFrames();

					//Console.WriteLine("StackFrame.ToString() foreach StackFrame in Stacktrace.GetFrames():");
					//foreach (StackFrame frame in frames)
						//Console.WriteLine(frame);
					//Console.WriteLine("Expecting: Main, Throw, Main");

					Assert.AreEqual (3, frames.Length);
					var wrongFrames = false;
					Assert.AreEqual ("GetFramesAndNestedExc", frames [0].GetMethod ().Name);
					Assert.AreEqual ("Throw", frames [1].GetMethod ().Name);
					Assert.AreEqual ("GetFramesAndNestedExc", frames [2].GetMethod ().Name);
				}
			}
		}

		[Test]
		[Category("NotWasm")]
		[Category("MobileNotWorking")]
		// https://github.com/mono/mono/issues/12688
		public async Task GetFrames_AsyncCalls ()
		{
			await StartAsyncCalls ();
		}

		private async Task StartAsyncCalls ()
		{
			try
			{
				await AsyncMethod1 ();
			}
			catch (Exception exception)
			{
				var stackTrace = new StackTrace (exception, true);
				Assert.AreEqual (25, stackTrace.GetFrames ().Length);
			}
		}

		private async Task<int> AsyncMethod1 ()
		{
			return await AsyncMethod2 ();
		}

		private async Task<int> AsyncMethod2 ()
		{
			return await AsyncMethod3 ();
		}

		private async Task<int> AsyncMethod3 ()
		{
			return await AsyncMethod4 ();
		}

		private async Task<int> AsyncMethod4 ()
		{
			await Task.Delay (10);
			throw new Exception ("Test exception thrown!");
		}

		/// <summary>
		/// Tests whether getting method associated with frame works.
		/// </summary>
		[Test]
		[Category("StackWalks")]
		[Category("NotWasm")]
		public void TestGetMethod ()
		{
			Assert.IsTrue ((frame1.GetMethod () != null), "Method not null (1)");

			Assert.IsTrue ((frame2.GetMethod () != null), "Method not null (2)");

			Assert.AreEqual (this.GetType (),
							 frame2.GetMethod ().DeclaringType,
							 "Class declaring the method (2)");

			Assert.AreEqual ("SetUp",
							 frame2.GetMethod ().Name,
							 "Method name (2)");
		}
	}
}
