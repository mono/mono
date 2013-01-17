//
// EventTypeFilter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2007 Novell, Inc.
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
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	public class EventTypeFilter : TraceFilter
	{
		SourceLevels event_type;

		public EventTypeFilter (SourceLevels eventType)
		{
			event_type = eventType;
		}

		public SourceLevels EventType {
			get { return event_type; }
			set { event_type = value; }
		}

		public override bool ShouldTrace (TraceEventCache cache,
						  string source, TraceEventType eventType,
						  int id, string formatOrMessage,
						  object [] args, object data1,
						  object [] data)
		{
			switch (eventType) {
			case TraceEventType.Critical:
				return (event_type & SourceLevels.Critical) != 0;
			case TraceEventType.Error:
				return (event_type & SourceLevels.Error) != 0;
			case TraceEventType.Information:
				return (event_type & SourceLevels.Information) != 0;
			case TraceEventType.Verbose:
				return (event_type & SourceLevels.Verbose) != 0;
			case TraceEventType.Warning:
				return (event_type & SourceLevels.Warning) != 0;
			case TraceEventType.Start:
			case TraceEventType.Stop:
			case TraceEventType.Suspend:
			case TraceEventType.Resume:
			case TraceEventType.Transfer:
				return (event_type & SourceLevels.ActivityTracing) != 0;
			}
			return event_type != SourceLevels.Off; // does it happen?
		}
	}
}
