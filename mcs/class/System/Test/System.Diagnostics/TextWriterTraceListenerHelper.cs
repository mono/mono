// TextWriterTraceListenerHelper.cs -
// Test Helper for System.Diagnostics/SourceSwitchTest.cs

//
//  Author:
//	Ramtin Raji Kermani
//
//  Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Diagnostics;

namespace MonoTests.System.Diagnostics
{
	public class TestTextWriterTraceListener: TextWriterTraceListener
	{
		public int TotalMessageCount { set; get;}
		public int CritialMessageCount { get; set;}
		public int ErrorMessageCount { get; set;}
		public int WarningMessageCount { get; set;}
		public int InfoMessageCount { get; set;}
		public int VerboseMessageCount { set; get;}

		public TestTextWriterTraceListener(TextWriter textWriter): base(textWriter)
		{
			Console.WriteLine ("TextWriterTraceListener is instantiated.");
		}


		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			base.TraceEvent (eventCache, source, eventType, id, message); 
			TotalMessageCount++;

			switch (eventType) {
			case TraceEventType.Critical:
				CritialMessageCount++; break;
			case TraceEventType.Error:
				ErrorMessageCount++; break;
			case TraceEventType.Warning:
				WarningMessageCount++; break;
			case TraceEventType.Information:
				InfoMessageCount++; break;
			case TraceEventType.Verbose:
				VerboseMessageCount++; break;
			default:
				break;
			}
		}

		public void clearMessageCounters()
		{
			TotalMessageCount	= 0;
			CritialMessageCount = 0;
			WarningMessageCount = 0;
			ErrorMessageCount 	= 0;
			InfoMessageCount 	= 0;
			VerboseMessageCount	= 0;
		}

	}
}

#endif