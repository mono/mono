//
// System.Diagnostics.EventLogTraceListener.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Jonathan Pryor
// (C) 2003 Andreas Nahr
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

using System.Collections;
using System.Security.Permissions;

namespace System.Diagnostics 
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class EventLogTraceListener : TraceListener 
	{
		private EventLog eventLog;
		private string source;

		public EventLogTraceListener ()
		{
		}

		public EventLogTraceListener (EventLog eventLog)
		{
			this.eventLog = eventLog;
		}

		public EventLogTraceListener (string source)
		{
			this.source = source;
		}

		public EventLog EventLog {
			get {return eventLog;}
			set {eventLog = value;}
		}

		public override string Name {
			get {return source;}
			set {source = value;}
		}

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Write (string message)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void WriteLine (string message)
		{
			throw new NotImplementedException();
		}

#if NET_2_0
		[MonoTODO]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, object data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, params object [] data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string message)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string format, params object [] args)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}

