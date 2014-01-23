//
// DelimitedListTraceListenerTest.cs
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
	public class DelimitedListTraceListenerTest
	{
#if NET_2_0
		string sample1 = "sample\n";

		string sample2 = ";Error;4;;;;;;;;\n";

		string sample3 = "\"bulldog\";Error;5;;;;;;;;\n\"bulldog\";Error;3;\"test event arg:arg1\";;;;;;;\n";

		string sample4 = ";Error;2;;;;;;;;\n;Error;3;;;;;;;;\n";

		string sample5 = ";Error;7;;\"XYZ\";;;;;;\n;Error;7;;\"ABC\",\"DEF\",\"123\";;;;;;\n";

		string sample6 = "\"my name is \"\"tricky\"\"\";Transfer;0;\"hoge;hoge, relatedActivityId=00000000-0000-0000-0000-000000000000\";;;;;;;\n";

		string sample7 = "Fail: error summary error details\n";

		[Test]
		public void WriteLine1 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			x.WriteLine ("sample");
			x.Close ();
			Assert.AreEqual (sample1, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void TraceEvent1 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			x.TraceEvent (null, null, TraceEventType.Error, 4, null);
			x.Close ();
			Assert.AreEqual (sample2, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void TraceEvent2 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			x.TraceEvent (null, "bulldog", TraceEventType.Error, 5);
			x.TraceEvent (null, "bulldog", TraceEventType.Error, 3, "test event arg:{0}", "arg1");
			x.Close ();
			Assert.AreEqual (sample3, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void TraceDataWithCache1 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			Trace.CorrelationManager.StartLogicalOperation ("op1"); // ... irrelevant?
			TraceEventCache cc = new TraceEventCache ();
			x.TraceData (cc, null, TraceEventType.Error, 2);
			x.TraceData (cc, null, TraceEventType.Error, 3);
			Trace.CorrelationManager.StopLogicalOperation ();
			x.Close ();
			Assert.AreEqual (sample4, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void TraceDataWithCache2 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			TraceEventCache cc = new TraceEventCache ();
			x.TraceData (cc, null, TraceEventType.Error, 7, "XYZ");
			x.TraceData (cc, null, TraceEventType.Error, 7, "ABC", "DEF", 123);
			x.Close ();
			Assert.AreEqual (sample5, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		public void TraceTransfer1 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			x.TraceTransfer (null, "my name is \"tricky\"", 0, "hoge;hoge", Guid.Empty);
			x.Close ();
			Assert.AreEqual (sample6, sw.ToString ().Replace ("\r\n", "\n"));
		}

		[Test]
		[Category ("NotWorking")]
		public void Fail1 ()
		{
			StringWriter sw = new StringWriter ();
			DelimitedListTraceListener x = new DelimitedListTraceListener (sw);
			TraceEventCache cc = new TraceEventCache ();
			x.Fail ("error summary", "error details");
			x.Close ();
			Assert.AreEqual (sample7, sw.ToString ().Replace ("\r\n", "\n"));
		}
#endif
	}
}

#endif