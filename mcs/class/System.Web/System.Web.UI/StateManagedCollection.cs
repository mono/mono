//
// System.Web.UI.StateManagedCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI {
	public abstract class StateManagedCollection : IList, IStateManager {
		
		protected abstract object CreateKnownType (int index);
		protected abstract void SetDirtyObject (object o);
		protected virtual Type [] GetKnownTypes ()
		{
			return null;
		}
		
		#region OnXXX
		protected virtual void OnClear ()
		{
		}
		
		protected virtual void OnClearComplete ()
		{
		}
		
		protected virtual void OnInsert (int index, object value)
		{
		}
		
		protected virtual void OnInsertComplete (int index, object value)
		{
		}
		
		protected virtual void OnRemove (int index, object value)
		{
		}
		
		protected virtual void OnRemoveComplete (int index, object value)
		{
		}
		
		protected virtual void OnValidate (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
		}
		#endregion
		
		#region IStateManager
		void IStateManager.LoadViewState (object savedState)
		{
			int pos = -1;
			foreach (Pair p in (ArrayList)savedState) {
				pos ++;
				
				if (p == null)
					continue;
				IStateManager itm;
				
				if (p.Second is Type)
					itm = (IStateManager) Activator.CreateInstance ((Type) p.Second);
				else
					itm = (IStateManager) CreateKnownType ((int) p.Second);
				
				itm.LoadViewState (p.First);
				
				if (pos >= Count)
					items.Add (itm);
				else
					items [pos] = itm;
				
			}
		}
		
		object IStateManager.SaveViewState ()
		{
			ArrayList saved = new ArrayList ();
			Type [] knownTypes = GetKnownTypes ();
			
			foreach (IStateManager itm in items) {
				object state = itm.SaveViewState ();
				if (state == null && !saveEverything) {
					saved.Add (null);
					continue;
				}
				
				Pair p = new Pair ();
				p.First = state;
				
				Type t = itm.GetType ();
				int idx = -1;
				if (knownTypes != null)
					idx = Array.IndexOf (knownTypes, t);
				
				if (idx != -1)
					p.Second = idx;
				else
					p.Second = t;
				
				saved.Add (p);
			}
			
			return saved;
		}
		
		void IStateManager.TrackViewState ()
		{
			isTrackingViewState = true;
			
			foreach (IStateManager i in items)
				i.TrackViewState ();
		}
		
		bool isTrackingViewState;
		bool IStateManager.IsTrackingViewState {
			get { return isTrackingViewState; }
		}
		#endregion
		
		#region ICollection, IList, IEnumerable
		
		public void Clear ()
		{
			this.OnClear ();
			items.Clear ();
			this.OnClearComplete ();
			
			SetSaveEverything ();
		}
		
		public int IndexOf (object o)
		{
			if (o == null)
				return -1;
			return items.IndexOf (o);
		}
		
		public bool Contains (object o)
		{
			return o != null && items.Contains (o);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			items.CopyTo (array, index);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		int IList.Add (object value)
		{
			OnValidate(value);
			if (isTrackingViewState) {
				((IStateManager) value).TrackViewState ();
				SetDirtyObject (value);
			}
			
			OnInsert (-1, value);
			items.Add (value);
			OnInsertComplete (-1, value);
			
			return Count - 1;
		}
		
		void IList.Insert (int index, object value)
		{
			OnValidate(value);
			if (isTrackingViewState) {
				((IStateManager) value).TrackViewState ();
				SetDirtyObject (value);
			}
			
			OnInsert (index, value);
			items.Insert (index, value);
			OnInsertComplete(index, value);
			
			SetSaveEverything ();
		}
		
		void IList.Remove (object value)
		{
			if (value == null)
				return;
			OnValidate (value);
			((IList)this).RemoveAt (IndexOf (value));
		}
		void IList.RemoveAt (int index)
		{
			object o = items [index];
			
			OnRemove (index, o);
			items.RemoveAt (index);
			OnRemoveComplete(index, o);
			
			SetSaveEverything ();
		}
			
		void IList.Clear ()
		{
			this.Clear ();
		}
		
		bool IList.Contains (object value)
		{
			if (value == null)
				return false;
			
			OnValidate (value);
			return Contains (value);
		}
		
		int IList.IndexOf (object value)
		{
			if (value == null)
				return -1;
			
			OnValidate (value);
			return IndexOf (value);
		}

		public int Count {
			get { return items.Count; }
		}
		
		int ICollection.Count {
			get { return items.Count; }
		}
		
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		bool IList.IsReadOnly {
			get { return false; }
		}
		
		object IList.this [int index] {
			get { return items [index]; }
			set {
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				
				OnValidate (value);
				if (isTrackingViewState) {
					((IStateManager) value).TrackViewState ();
					SetDirtyObject (value);
				}
				
				items [index] = value;
			}
		}
		#endregion

		ArrayList items = new ArrayList ();
				
		bool saveEverything = false;
		void SetSaveEverything ()
		{
			if (isTrackingViewState)
				saveEverything = true;
		}
	}
}
#endif

