//
// System.Diagnostics.EventLogEntryCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//


using System;
using System.Collections;
using System.Diagnostics;

namespace System.Diagnostics {

	public class EventLogEntryCollection : ICollection, IEnumerable {

		private ArrayList eventLogs = new ArrayList ();

		internal EventLogEntryCollection()
		{
		}

		public int Count {
			get {return eventLogs.Count;}
		}

		public virtual EventLogEntry this [int index] {
			get {return (EventLogEntry) eventLogs[index];}
		}

		bool ICollection.IsSynchronized {
			get {return eventLogs.IsSynchronized;}
		}

		object ICollection.SyncRoot {
			get {return eventLogs.SyncRoot;}
		}

		public void CopyTo (EventLogEntry[] eventLogs, int index)
		{
			eventLogs.CopyTo (eventLogs, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return eventLogs.GetEnumerator ();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			eventLogs.CopyTo (array, index);
		}
	}
}

