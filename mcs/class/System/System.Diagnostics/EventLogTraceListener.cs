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


using System;
using System.Collections;
using System.Diagnostics;

namespace System.Diagnostics 
{

	public class EventLogTraceListener : TraceListener 
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
	}
}

