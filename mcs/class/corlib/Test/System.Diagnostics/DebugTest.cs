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
	public class DebugTest
	{
		private class DebugTest1 : TestCase
		{
			protected override void SetUp()
			{
				Debug.Listeners.Add(new TextWriterTraceListener(Console.Error));	
			}
			
			protected override void TearDown()
			{
				
			}
			
			public void TestAssert()
			{
				Debug.Assert(false, "Testing Assertions");
			}
			
			public void TestFail()
			{
				Debug.Fail("Testing Fail method");
			}
			
			public void TestWrite()
			{
				Debug.Write("Testing Write", "Testing the output of the Write method");
			}
			
			public void TestWriteIf()
			{
				Debug.WriteIf(true, "Testing WriteIf");
				Debug.WriteIf(false, "Testing WriteIf", "Passed false");
			}
			
			public void TestWriteLine()
			{
				Debug.WriteLine("Testing WriteLine method");
			}
			
			public void TestWriteLineIf()
			{
				Debug.WriteLineIf(true, "Testing WriteLineIf");
				Debug.WriteLineIf(false, "Testing WriteLineIf", "Passed false");
			}
		}
	}
}
