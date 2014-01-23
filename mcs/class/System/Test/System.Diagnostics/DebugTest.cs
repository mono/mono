//
// MonoTests.System.Diagnostics.DebugTest.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2002
using System;
using System.Diagnostics;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
		[TestFixture]
		public class DebugTest2
		{
			DefaultTraceListener listener = new DefaultTraceListener ();

			[SetUp]
			protected void SetUp()
			{
				Debug.Listeners.Add(listener);	
			}
			
			[TearDown]
			protected void TearDown()
			{
				Debug.Listeners.Remove (listener);	
			}

			[Test]
			public void TestAssert()
			{
				Debug.Assert(false, "Testing Assertions");
			}

			[Test]			
			public void TestFail ()
			{
				Debug.Fail("Testing Fail method");
			}

			[Test]			
			public void TestWrite()
			{
				Debug.Write("Testing Write", "Testing the output of the Write method");
			}

			[Test]			
			public void TestWriteIf()
			{
				Debug.WriteIf(true, "Testing WriteIf");
				Debug.WriteIf(false, "Testing WriteIf", "Passed false");
			}

			[Test]			
			public void TestWriteLine()
			{
				Debug.WriteLine("Testing WriteLine method");
			}

			[Test]			
			public void TestWriteLineIf()
			{
				Debug.WriteLineIf(true, "Testing WriteLineIf");
				Debug.WriteLineIf(false, "Testing WriteLineIf", "Passed false");
			}
		}
}
