//
// System.Diagnostics.TraceListenerCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
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
using System.Globalization;

namespace System.Diagnostics {

	public class TraceListenerCollection : IList, ICollection, IEnumerable {

		private ArrayList listeners = ArrayList.Synchronized (new ArrayList (1));

		internal TraceListenerCollection ()
			: this (true)
		{
		}

		internal TraceListenerCollection (bool addDefault)
		{
			if (addDefault)
				Add (new DefaultTraceListener ());
		}

		public int Count{
			get {return listeners.Count;}
		}

		public TraceListener this [string name] {
			get {
				lock (listeners.SyncRoot) {
					foreach (TraceListener listener in listeners) {
						if (listener.Name == name)
							return listener;
					}
				}
				return null;
			}
		}

		public TraceListener this [int index] {
			get {return (TraceListener) listeners[index];}
			set {
				InitializeListener (value);
				listeners[index] = value;
			}
		}

		object IList.this [int index] {
			get {return listeners[index];}
			set {
				TraceListener l = (TraceListener) value;
				InitializeListener (l);
				this[index] = l;
			}
		}

		bool ICollection.IsSynchronized {
			get {return listeners.IsSynchronized;}
		}

		object ICollection.SyncRoot {
			get {return listeners.SyncRoot;}
		}

		bool IList.IsFixedSize {
			get {return listeners.IsFixedSize;}
		}

		bool IList.IsReadOnly {
			get {return listeners.IsReadOnly;}
		}

		public int Add (TraceListener listener)
		{
			InitializeListener (listener);
			return listeners.Add (listener);
		}

#if !MOBILE
		internal void Add (TraceListener listener, TraceImplSettings settings)
		{
//			listener.IndentLevel = settings.IndentLevel;
			listener.IndentSize  = settings.IndentSize;
			listeners.Add (listener);
		}
#endif

		private void InitializeListener (TraceListener listener)
		{
			listener.IndentLevel = TraceImpl.IndentLevel;
			listener.IndentSize = TraceImpl.IndentSize;
		}

		private void InitializeRange (IList listeners)
		{
			int e = listeners.Count;
			for (int i = 0; i != e; ++i)
				InitializeListener (
					(TraceListener) listeners[i]);
		}

		public void AddRange (TraceListener[] value)
		{
			InitializeRange (value);
			listeners.AddRange (value);
		}

		public void AddRange (TraceListenerCollection value)
		{
			InitializeRange (value);
			listeners.AddRange (value.listeners);
		}

		public void Clear ()
		{
			listeners.Clear ();
		}

		public bool Contains (TraceListener listener)
		{
			return listeners.Contains (listener);
		}

		public void CopyTo (TraceListener[] listeners, int index)
		{
			listeners.CopyTo (listeners, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return listeners.GetEnumerator ();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			listeners.CopyTo (array, index);
		}

		int IList.Add (object value)
		{
			if (value is TraceListener)
				return Add ((TraceListener) value);
			throw new NotSupportedException (Locale.GetText (
				"You can only add TraceListener objects to the collection"));
		}

		bool IList.Contains (object value)
		{
			if (value is TraceListener)
				return listeners.Contains (value);
			return false;
		}

		int IList.IndexOf (object value)
		{
			if (value is TraceListener)
				return listeners.IndexOf (value);
			return -1;
		}

		void IList.Insert (int index, object value)
		{
			if (value is TraceListener) {
				Insert (index, (TraceListener) value);
				return;
			}
			throw new NotSupportedException (Locale.GetText (
				"You can only insert TraceListener objects into the collection"));
		}

		void IList.Remove (object value)
		{
			if (value is TraceListener)
				listeners.Remove (value);
		}

		public int IndexOf (TraceListener listener)
		{
			return listeners.IndexOf (listener);
		}

		public void Insert (int index, TraceListener listener)
		{
			InitializeListener (listener);
			listeners.Insert (index, listener);
		}

		public void Remove (string name)
		{
			TraceListener found = null;

			lock (listeners.SyncRoot) {
				foreach (TraceListener listener in listeners) {
					if (listener.Name == name) {
						found = listener;
						break;
					}
				}

				if (found != null)
					listeners.Remove (found);
				else
					throw new ArgumentException (Locale.GetText (
						"TraceListener " + name + " was not in the collection"));
			}
		}

		public void Remove (TraceListener listener)
		{
			listeners.Remove (listener);
		}

		public void RemoveAt (int index)
		{
			listeners.RemoveAt (index);
		}
	}
}

