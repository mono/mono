//
// TraceListenerTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOBILE

using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class TraceListenerTest
	{
#if NET_2_0
		[Test]
		public void GetSupportedAttributes ()
		{
			MyTraceListener t = new MyTraceListener ();
			string [] attributes = t.SupportedAttributes;
			Assert.IsNull (attributes);
		}

		[Test]
		public void TraceEventAndTraceData ()
		{
			StringWriter sw = new StringWriter ();
			TextWriterTraceListener t = new TextWriterTraceListener (sw);
			t.TraceEvent (null, null, TraceEventType.Error, 0, null);
			t.TraceEvent (null, "bulldog", TraceEventType.Error, 0);
			TraceEventCache cc = new TraceEventCache ();
			t.TraceData (cc, null, TraceEventType.Error, 0);
			t.TraceData (cc, null, TraceEventType.Error, 0);
			t.TraceTransfer (null, "bulldog", 0, "hoge", Guid.Empty);
			t.Close ();
			string expected = @" Error: 0 : 
bulldog Error: 0 : 
 Error: 0 : 
 Error: 0 : 
bulldog Transfer: 0 : hoge, relatedActivityId=00000000-0000-0000-0000-000000000000
";
			Assert.AreEqual (expected, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void TraceOptionValues ()
		{
			Assert.AreEqual (TraceOptions.None,
				new TextWriterTraceListener (TextWriter.Null).TraceOutputOptions, "#1");
			Assert.AreEqual (TraceOptions.None,
				new XmlWriterTraceListener (TextWriter.Null).TraceOutputOptions, "#2");

			StringWriter sw = new StringWriter ();
			TextWriterTraceListener t = new TextWriterTraceListener (sw);
			Trace.CorrelationManager.StartLogicalOperation ("foo");
			Trace.CorrelationManager.StartLogicalOperation ("bar");
			t.TraceOutputOptions = TraceOptions.LogicalOperationStack | TraceOptions.DateTime | TraceOptions.Timestamp;// | TraceOptions.ProcessId | TraceOptions.ThreadId | TraceOptions.Callstack;
			t.TraceEvent (null, null, TraceEventType.Error, 0, null);
			t.TraceEvent (null, "bulldog", TraceEventType.Error, 0);
			TraceEventCache cc = new TraceEventCache ();
			DateTime date = cc.DateTime;
			long time = cc.Timestamp;
			t.TraceData (cc, null, TraceEventType.Error, 0);
			t.TraceData (cc, null, TraceEventType.Error, 0);
			t.TraceTransfer (null, "bulldog", 0, "hoge", Guid.Empty);
			Trace.CorrelationManager.StopLogicalOperation ();
			Trace.CorrelationManager.StopLogicalOperation ();
			t.Close ();
			string expected = String.Format (@" Error: 0 : 
bulldog Error: 0 : 
 Error: 0 : 
    LogicalOperationStack=bar, foo
    DateTime={0}
    Timestamp={1}" +
/*
    Callstack=   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at System.Diagnostics.TraceEventCache.get_Callstack()
   at System.Diagnostics.TraceListener.WriteFooter(TraceEventCache eventCache)
   at System.Diagnostics.TraceListener.TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, Int32 id, Object[] data)
   at MonoTests.System.Diagnostics.TraceListenerTest.TraceOptionValues()
   ...
*/
				@"
 Error: 0 : 
    LogicalOperationStack=bar, foo
    DateTime={0}
    Timestamp={1}
bulldog Transfer: 0 : hoge, relatedActivityId=00000000-0000-0000-0000-000000000000
", date.ToString ("o"), time); // date and time are in current culture
			Assert.AreEqual (expected, sw.ToString ().Replace ("\r\n", "\n"));
		}
#endif

		class MyTraceListener : TraceListener
		{
#if NET_2_0
			public string [] SupportedAttributes {
				get { return base.GetSupportedAttributes (); }
			}
#endif

			public override void Write (string message)
			{
			}

			public override void WriteLine (string message)
			{
			}
		}
	}
}

#endif