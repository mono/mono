//
// System.Diagnostics.EventLogTraceListener.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2017 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Diagnostics {
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class EventLogTraceListener : TraceListener {
		const string EXCEPTION_MESSAGE = "System.Diagnostics.EventLogTraceListener is not supported on the current platform.";

		private EventLogTraceListener ()
		{
		}

#if FIXME
		public EventLogTraceListener (EventLog eventLog)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public EventLogTraceListener (string source)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public EventLog EventLog {
			get { return event_log; }
			set { event_log = value; }
		}
#endif

		public override string Name {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Write (string message)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void WriteLine (string message)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		[ComVisible (false)]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType severity,
						int id, object data)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		[ComVisible (false)]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType severity,
						int id, params object [] data)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		[ComVisible (false)]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType severity,
						 int id, string message)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		[ComVisible (false)]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType severity,
						 int id, string format, params object [] args)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}

