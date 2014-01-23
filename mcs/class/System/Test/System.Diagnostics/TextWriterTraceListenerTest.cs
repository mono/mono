//
// MonoTests.System.Diagnostics.TextWriterTraceListenerTest.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001

#if !MOBILE

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class TextWriterTraceListenerTest
	{
		private TextWriterTraceListener listener;

		[SetUp]
		public void SetUp()
		{
			listener = new TextWriterTraceListener();
			listener.Writer = Console.Out;
		}
		
		[TearDown]
		public void TearDown()
		{
			listener = null;
		}

		[Test]
		public void TestWrite()
		{
			Assert.IsTrue(!(listener == null), "Null Listener");
			Assert.IsTrue(!(listener.Writer == null), "Null Writer");
			listener.Write("Test Message\n");
			
		}
		
		[Test]
		public void TestWriteLine()
		{
			Assert.IsTrue(!(listener == null), "Null Listener");
			Assert.IsTrue(!(listener.Writer == null), "Null Writer");
			listener.WriteLine("Test WriteLine Message");
		}
		
		[Test]
		public void TestFlush()
		{
			listener.Flush();
		}
	}
}

#endif