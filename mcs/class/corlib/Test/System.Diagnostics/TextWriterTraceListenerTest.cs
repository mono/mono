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
	
	public class TextWriterTraceListenerTest
	{
		private TextWriterTraceListenerTest()
		{
			
		}
		
		public static ITest Suite
		{
			get
			{
				TestSuite suite = new TestSuite();
				suite.AddTest(TextWriterTraceListenerTest1.Suite);
				return suite;
			}
		}
		
		private class TextWriterTraceListenerTest1 : TestCase
		{
			public TextWriterTraceListenerTest1(string name) : base(name)
			{
			}
			
			private TextWriterTraceListener listener;
			
			internal static ITest Suite
			{
				get
				{
					return new TestSuite(typeof(TextWriterTraceListenerTest1));
				}
			}
			
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
}
