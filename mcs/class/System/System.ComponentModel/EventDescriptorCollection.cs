//
// System.ComponentModel.EventDescriptorCollection.cs
//
// Authors: 
//   Rodrigo Moya (rodrigo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.
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
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public class EventDescriptorCollection : IList, ICollection, IEnumerable
	{
		private ArrayList eventList = new ArrayList ();
		private bool isReadOnly;
		public static readonly EventDescriptorCollection Empty = new EventDescriptorCollection (null, true);
		
		private EventDescriptorCollection ()
		{
		}
		
		internal EventDescriptorCollection (ArrayList list)
		{
			eventList = list;
		}
		
		public EventDescriptorCollection (EventDescriptor[] events) 
			: this (events, false)
		{
		}

		public EventDescriptorCollection (EventDescriptor[] events, bool readOnly) 
		{
			this.isReadOnly = readOnly;
			if (events == null)
				return;

			for (int i = 0; i < events.Length; i++)
				this.Add (events[i]);
		}

		public int Add (EventDescriptor value)
		{
			if (isReadOnly)
				throw new NotSupportedException ("The collection is read-only");
			return eventList.Add (value);
		}

		void IList.Clear ()
		{
			Clear ();
		}

		public void Clear ()
		{
			if (isReadOnly)
				throw new NotSupportedException ("The collection is read-only");
			eventList.Clear ();
		}

		public bool Contains (EventDescriptor value) {
			return eventList.Contains (value);
		}

		public virtual EventDescriptor Find (string name, bool ignoreCase) 
		{
			foreach (EventDescriptor e in eventList) {
				if (ignoreCase) {
					if (0 == String.Compare (name, e.Name, StringComparison.OrdinalIgnoreCase))
						return e;
				} else {
					if (0 == String.Compare (name, e.Name, StringComparison.Ordinal))
						return e;
				}
			}
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator ();
		}

		public IEnumerator GetEnumerator () {
			return eventList.GetEnumerator ();
		}

		public int IndexOf (EventDescriptor value) {
			return eventList.IndexOf (value);
		}

		public void Insert (int index, EventDescriptor value)
		{
			if (isReadOnly)
				throw new NotSupportedException ("The collection is read-only");
			eventList.Insert (index, value);
		}

		public void Remove (EventDescriptor value)
		{
			if (isReadOnly)
				throw new NotSupportedException ("The collection is read-only");
			eventList.Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			RemoveAt (index);
		}

		public void RemoveAt (int index)
		{
			if (isReadOnly)
				throw new NotSupportedException ("The collection is read-only");
			eventList.RemoveAt (index);
		}

		public virtual EventDescriptorCollection Sort () {
			EventDescriptorCollection col = CloneCollection ();
			col.InternalSort ((IComparer) null);
			return col;
		}

		public virtual EventDescriptorCollection Sort (IComparer comparer) {
			EventDescriptorCollection col = CloneCollection ();
			col.InternalSort (comparer);
			return col;
		}

		public virtual EventDescriptorCollection Sort (string[] order) {
			EventDescriptorCollection col = CloneCollection ();
			col.InternalSort (order);
			return col;
		}

		public virtual EventDescriptorCollection Sort (string[] order, IComparer comparer) {
			EventDescriptorCollection col = CloneCollection ();
			if (order != null) {
				ArrayList sorted = col.ExtractItems (order);
				col.InternalSort (comparer);
				sorted.AddRange (col.eventList);
				col.eventList = sorted;
			} else {
				col.InternalSort (comparer);
			}
			return col;
		}

		protected void InternalSort (IComparer comparer) {
			if (comparer == null)
				comparer = MemberDescriptor.DefaultComparer;
			eventList.Sort (comparer);
		}

		protected void InternalSort (string[] order) {
			if (order != null) {
				ArrayList sorted = ExtractItems (order);
				InternalSort ((IComparer) null);
				sorted.AddRange (eventList);
				eventList = sorted;
			} else {
				InternalSort ((IComparer) null);
			}
		}
		
		ArrayList ExtractItems (string[] names)
		{
			ArrayList sorted = new ArrayList (eventList.Count);
			object[] ext = new object [names.Length];
			
			for (int n=0; n<eventList.Count; n++)
			{
				EventDescriptor ed = (EventDescriptor) eventList[n];
				int i = Array.IndexOf (names, ed.Name);
				if (i != -1) {
					ext[i] = ed;
					eventList.RemoveAt (n);
					n--;
				}
			}
			foreach (object ob in ext)
				if (ob != null) sorted.Add (ob);
				
			return sorted;
		}
		
		private EventDescriptorCollection CloneCollection ()
		{
			EventDescriptorCollection col = new EventDescriptorCollection ();
			col.eventList = (ArrayList) eventList.Clone ();
			return col;
		}
		
		internal EventDescriptorCollection Filter (Attribute[] attributes)
		{
			EventDescriptorCollection col = new EventDescriptorCollection ();
			foreach (EventDescriptor ed in eventList)
				if (ed.Attributes.Contains (attributes))
					col.eventList.Add (ed);
			return col;
		}
		
		int ICollection.Count {
			get { return Count; }
		}

		public int Count {
			get {
				return eventList.Count;
			}
		}

		 public virtual EventDescriptor this[string name] {
			 get { return Find (name, false); }
		 }

		public virtual EventDescriptor this[int index] {
			get {
				return (EventDescriptor) eventList[index];
			}
		}

		// IList methods

		int IList.Add (object value) {
			return Add ((EventDescriptor) value);
		}

		bool IList.Contains (object value) {
			return Contains ((EventDescriptor) value);
		}

		int IList.IndexOf (object value) {
			return IndexOf ((EventDescriptor) value);
		}

		void IList.Insert (int index, object value) {
			Insert (index, (EventDescriptor) value);
		}

		void IList.Remove (object value) {
			Remove ((EventDescriptor) value);
		}

		bool IList.IsFixedSize {
			get {
				return isReadOnly;
			}
		}

		bool IList.IsReadOnly {
			get { return isReadOnly; }
		}

		object IList.this[int index] {
			get {
				return eventList[index];
			}
			set {
				if (isReadOnly)
					throw new NotSupportedException ("The collection is read-only");
				eventList[index] = value;
			}
		}

		// ICollection methods

		void ICollection.CopyTo (Array array, int index) {
			eventList.CopyTo (array, index);
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return null; }
		}
	}
}
