//
// System.Diagnostics.EntryWrittenEventArgs.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Diagnostics {

	public class EntryWrittenEventArgs : EventArgs {

		private EventLogEntry entry;

		public EntryWrittenEventArgs () : this (null)
		{
		}

		public EntryWrittenEventArgs (EventLogEntry entry)
		{
			this.entry = entry;
		}

		public EventLogEntry Entry {
			get {return entry;}
		}
	}
}

