//
// System.ComponentModel.EventDescriptorCollection.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc.
//

using System.Collections;

namespace System.ComponentModel
{
	public class EventDescriptorCollection : IList, ICollection, IEnumerable
	{
		private ArrayList eventList;
		
		public static readonly EventDescriptorCollection Empty;
		
		public EventDescriptorCollection (EventDescriptor[] events) {
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

		[MonoTODO]
		public virtual EventDescriptor Find (string name, bool ignoreCase) {
			throw new NotImplementedException ();
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


		[MonoTODO]
		public virtual EventDescriptorCollection Sort () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection Sort (IComparer comparer) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection Sort (string[] order) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection Sort (string[] order,
							       IComparer comparer) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual EventDescriptorCollection InternalSort (IComparer comparer) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual EventDescriptorCollection InternalSort (string[] order) {
			throw new NotImplementedException ();
		}
		
		public int Count {
			get {
				return eventList.Count;
			}
		}

		 public virtual EventDescriptor this[string name] {
			 [MonoTODO]
			 get {
				 throw new NotImplementedException ();
			 }
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
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		// ICollection methods

		[MonoTODO]
		void ICollection.CopyTo (Array array, int index) {
			throw new NotImplementedException ();
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return null; }
		}
	}
}
