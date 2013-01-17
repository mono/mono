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

		readonly EventLogImpl _impl;

		internal EventLogEntryCollection(EventLogImpl impl)
		{
			_impl = impl;
		}

		public int Count {
			get { return _impl.EntryCount; }
		}

		public virtual EventLogEntry this [int index] {
			get { return _impl[index]; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		public void CopyTo (EventLogEntry[] eventLogEntries, int index)
		{
			EventLogEntry[] entries = _impl.GetEntries ();
			Array.Copy (entries, 0, eventLogEntries, index, entries.Length);
		}

		public IEnumerator GetEnumerator ()
		{
			return new EventLogEntryEnumerator (_impl);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			EventLogEntry[] entries = _impl.GetEntries ();
			Array.Copy (entries, 0, array, index, entries.Length);
		}

		private class EventLogEntryEnumerator : IEnumerator
		{
			internal EventLogEntryEnumerator (EventLogImpl impl)
			{
				_impl = impl;
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public EventLogEntry Current {
				get {
					if (_currentEntry != null)
						return _currentEntry;

					throw new InvalidOperationException ("No current EventLog"
						+ " entry available, cursor is located before the first"
						+ " or after the last element of the enumeration.");
				}
			}

			public bool MoveNext ()
			{
				_currentIndex++;
				if (_currentIndex >= _impl.EntryCount) {
					_currentEntry = null;
					return false;
				}
				_currentEntry = _impl [_currentIndex];
				return true;
			}

			public void Reset ()
			{
				_currentIndex = - 1;
			}

			readonly EventLogImpl _impl;
			int _currentIndex = -1;
			EventLogEntry _currentEntry;
		}
}
}

