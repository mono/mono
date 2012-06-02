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
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Diagnostics 
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class EventLogTraceListener : TraceListener 
	{
		private EventLog event_log;
		private string name;

		public EventLogTraceListener ()
		{
		}

		public EventLogTraceListener (EventLog eventLog)
		{
			if (eventLog == null)
				throw new ArgumentNullException ("eventLog");
			this.event_log = eventLog;
		}

		public EventLogTraceListener (string source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			event_log = new EventLog ();
			event_log.Source = source;
		}

		public EventLog EventLog {
			get { return event_log; }
			set { event_log = value; }
		}

		public override string Name {
			get { return name != null ? name : event_log.Source; }
			set { name = value; }
		}

		public override void Close ()
		{
			event_log.Close ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				event_log.Dispose ();
		}

		public override void Write (string message)
		{
			TraceData (new TraceEventCache (), event_log.Source,
				   TraceEventType.Information, 0, message);
		}

		public override void WriteLine (string message)
		{
			Write (message);
		}

		[ComVisible (false)]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, object data)
		{
			EventLogEntryType type;
			switch (eventType) {
			case TraceEventType.Critical:
			case TraceEventType.Error:
				type = EventLogEntryType.Error;
				break;
			case TraceEventType.Warning:
				type = EventLogEntryType.Warning;
				break;
			default:
				type = EventLogEntryType.Information;
				break;
			}
			event_log.WriteEntry (data != null ? data.ToString () : String.Empty, type, id, 0);
		}

		[ComVisible (false)]
		public override void TraceData (TraceEventCache eventCache,
						string source, TraceEventType eventType,
						int id, params object [] data)
		{
			string s = String.Empty;
			if (data != null) {
				string [] arr = new string [data.Length];
				for (int i = 0; i < data.Length; i++)
					arr [i] = data [i] != null ? data [i].ToString () : String.Empty;
				s = String.Join (", ", arr);
			}
			TraceData (eventCache, source, eventType, id, s);
		}

		[ComVisible (false)]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string message)
		{
			TraceData (eventCache, source, eventType, id, message);
		}

		[ComVisible (false)]
		public override void TraceEvent (TraceEventCache eventCache,
						 string source, TraceEventType eventType,
						 int id, string format, params object [] args)
		{
			TraceEvent (eventCache, source, eventType, id, format != null ? String.Format (format, args) : null);
		}
	}
}

