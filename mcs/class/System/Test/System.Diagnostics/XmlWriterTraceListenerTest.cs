//
// XmlWriterTraceListenerTest.cs
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

using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class XmlWriterTraceListenerTest
	{
#if NET_2_0
		string sample1 = @"<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>0</EventID><Type>3</Type><SubType Name='Information'>0</SubType><Level>8</Level><TimeCreated SystemTime='2007-04-19T21:18:30.6250000+09:00' /><Source Name='Trace' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData>sample</ApplicationData></E2ETraceEvent>";

		string sample2 = @"<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>4</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T21:18:30.6250000+09:00' /><Source Name='' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData></ApplicationData></E2ETraceEvent>";

		string sample3 = @"<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>5</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T21:18:30.6250000+09:00' /><Source Name='bulldog' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData></ApplicationData></E2ETraceEvent>";

		string sample4 = @"<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>2</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T12:18:30.6250000Z' /><Source Name='' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData><TraceData></TraceData></ApplicationData></E2ETraceEvent><E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>3</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T12:18:30.6250000Z' /><Source Name='' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData><TraceData></TraceData></ApplicationData></E2ETraceEvent>";

		string sample5 = @"<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>7</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T12:18:30.6250000Z' /><Source Name='' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData><TraceData><DataItem>XYZ</DataItem></TraceData></ApplicationData></E2ETraceEvent><E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>7</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T12:18:30.6250000Z' /><Source Name='' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData><TraceData><DataItem>ABC</DataItem><DataItem>DEF</DataItem></TraceData></ApplicationData></E2ETraceEvent>";

		string sample6 = "<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>0</EventID><Type>3</Type><SubType Name='Transfer'>0</SubType><Level>255</Level><TimeCreated SystemTime='2007-04-19T21:18:30.6250000+09:00' /><Source Name='bulldog' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' RelatedActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData>hoge</ApplicationData></E2ETraceEvent>";

		string sample7 = "<E2ETraceEvent xmlns='http://schemas.microsoft.com/2004/06/E2ETraceEvent'><System xmlns='http://schemas.microsoft.com/2004/06/windows/eventlog/system'><EventID>0</EventID><Type>3</Type><SubType Name='Error'>0</SubType><Level>2</Level><TimeCreated SystemTime='2007-04-19T21:18:30.6250000+09:00' /><Source Name='Trace' /><Correlation ActivityID='{00000000-0000-0000-0000-000000000000}' /><Execution ProcessName='zzz' ProcessID='4776' ThreadID='1' /><Channel/><Computer>PC</Computer></System><ApplicationData>error summary error details</ApplicationData></E2ETraceEvent>";

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void WriteLine1 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			x.WriteLine ("sample");
			x.Close ();
			Assert.AreEqual (sample1.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void TraceEvent1 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			x.TraceEvent (null, null, TraceEventType.Error, 4, null);
			x.Close ();
			Assert.AreEqual (sample2.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void TraceEvent2 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			x.TraceEvent (null, "bulldog", TraceEventType.Error, 5);
			x.Close ();
			Assert.AreEqual (sample3.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void TraceDataWithCache1 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			TraceEventCache cc = new TraceEventCache ();
			x.TraceData (cc, null, TraceEventType.Error, 2);
			x.TraceData (cc, null, TraceEventType.Error, 3);
			x.Close ();
			Assert.AreEqual (sample4.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void TraceDataWithCache2 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			TraceEventCache cc = new TraceEventCache ();
			x.TraceData (cc, null, TraceEventType.Error, 7, "XYZ");
			x.TraceData (cc, null, TraceEventType.Error, 7, "ABC", "DEF");
			x.Close ();
			Assert.AreEqual (sample5.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void TraceTransfer1 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			x.TraceTransfer (null, "bulldog", 0, "hoge", Guid.Empty);
			x.Close ();
			Assert.AreEqual (sample6.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		[Ignore ("the test should be rewritten to not compare instance-specific items.")]
		public void Fail1 ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterTraceListener x = new XmlWriterTraceListener (sw);
			TraceEventCache cc = new TraceEventCache ();
			x.Fail ("error summary", "error details");
			x.Close ();
			Assert.AreEqual (sample7.Replace ('\'', '"'), sw.ToString ());
		}

		[Test]
		public void XPathNavigatorAsData ()
		{
			// While XmlReader, XmlDocument and XDocument are not supported as direct xml content (i.e. to not get escaped), XPathNavigator is.
			var sw = new StringWriter ();
			var xl = new XmlWriterTraceListener (sw);
			var doc = new XmlDocument ();
			string xml = "<root><child xmlns=\"urn:foo\">text</child></root>";
			doc.LoadXml (xml);
			xl.TraceData (null, "my source", TraceEventType.Information, 1, doc.CreateNavigator ());
			// Note that it does not result in "<root xmlns=''>...".
			// See XmlWriterTraceListener.TraceCore() for details.
			Assert.IsTrue (sw.ToString ().IndexOf (xml) > 0, "#1");
		}
#endif
	}
}

