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

using System.Collections;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public class EventDescriptorCollection : IList, ICollection, IEnumerable
	{
		private ArrayList eventList = new ArrayList ();
		
		public static readonly EventDescriptorCollection Empty;
		
		private EventDescriptorCollection ()
		{
		}
		
		public EventDescriptorCollection (EventDescriptor[] events) 
		{
			for (int i = 0; i < events.Length; i++)
				this.Add (events[i]);
		}

		public int Add (EventDescriptor value) {
			return eventList.Add (value);
		}

		public void Clear () {
			eventList.Clear ();
		}

		public bool Contains (EventDescriptor value) {
			return eventList.Contains (value);
		}

		public virtual EventDescriptor Find (string name, bool ignoreCase) 
		{
			foreach (EventDescriptor e in eventList) {
				if (0 == String.Compare (name, e.Name, ignoreCase))
					return e;
			}
			return null;
		}

		public IEnumerator GetEnumerator () {
			return eventList.GetEnumerator ();
		}

		public int IndexOf (EventDescriptor value) {
			return eventList.IndexOf (value);
		}

		public void Insert (int index, EventDescriptor value) {
			eventList.Insert (index, value);
		}

		public void Remove (EventDescriptor value) {
			eventList.Remove (value);
		}

		public void RemoveAt (int index) {
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
			ArrayList sorted = col.ExtractItems (order);
			col.InternalSort (comparer);
			sorted.AddRange (col.eventList);
			col.eventList = sorted;
			return col;
		}

		protected void InternalSort (IComparer comparer) {
			eventList.Sort (comparer);
		}

		protected void InternalSort (string[] order) {
			ArrayList sorted = ExtractItems (order);
			InternalSort ((IComparer) null);
			sorted.AddRange (eventList);
			eventList = sorted;
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
				
			sorted.AddRange (eventList);
			return sorted;
		}
		
		private EventDescriptorCollection CloneCollection ()
		{
			EventDescriptorCollection col = new EventDescriptorCollection ();
			col.eventList = (ArrayList) eventList.Clone ();
			return col;
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
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		object IList.this[int index] {
			get {
				return eventList[index];
			}
			set {
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
