//
// MonoTests.System.Diagnostics.TextWriterTraceListenerTest.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2001

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class TextWriterTraceListenerTest1 : TestCase
	{
		private TextWriterTraceListener listener;
		
		protected override void SetUp()
		{
			listener = new TextWriterTraceListener();
			listener.Writer = Console.Out;
		}
		
		protected override void TearDown()
		{
			listener = null;
		}
		
		public void TestWrite()
		{
			Assert("Null Listener", !(listener == null));
			Assert("Null Writer", !(listener.Writer == null));
			listener.Write("Test Message\n");
			
		}
		
		public void TestWriteLine()
		{
			Assert("Null Listener", !(listener == null));
			Assert("Null Writer", !(listener.Writer == null));
			listener.WriteLine("Test WriteLine Message");
		}
		
		public void TestFlush()
		{
			listener.Flush();
		}
	}
}
