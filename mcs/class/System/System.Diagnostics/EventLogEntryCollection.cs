//
// System.Diagnostics.EventLogEntryCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
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

