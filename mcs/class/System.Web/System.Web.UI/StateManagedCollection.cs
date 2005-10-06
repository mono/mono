//
// System.Web.UI.StateManagedCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI {
	public abstract class StateManagedCollection : IList, IStateManager
	{
		ArrayList items = new ArrayList ();
		bool saveEverything = false;
		IStateManager[] originalItems;
		
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
			if (savedState == null) {
				foreach (IStateManager item in items)
					item.LoadViewState (null);
				return;
			}
			
			object[] its = (object[]) savedState;
			
			saveEverything = (bool)its [0];
			
			if (saveEverything)
				items.Clear ();

			for (int n=1; n<its.Length; n++) {
				int oi;
				object state;
				object type;
				
				Triplet triplet = its [n] as Triplet;
				if (triplet != null) {
					oi = (int) triplet.First;
					state = triplet.Second;
					type = triplet.Third; 
				} else {
					Pair pair = (Pair) its [n];
					oi = (int) pair.First;
					state = pair.Second;
					type = null;
				}
				
				IStateManager item;
				if (oi != -1)
					item = originalItems [oi];
				else {
					if (type is Type)
						item = (IStateManager) Activator.CreateInstance ((Type) type);
					else
						item = (IStateManager) CreateKnownType ((int) type);
				}
				
				if (saveEverything) ((IList)this).Add (item);
				
				item.LoadViewState (state);
			}
		}
		
		object IStateManager.SaveViewState ()
		{
			object[] state = null;
			bool hasData = false;
			Type[] knownTypes = GetKnownTypes ();
			
			if (saveEverything) {
				state = new object [items.Count + 1];
				state [0] = true;
				for (int n=0; n<items.Count; n++)
				{
					IStateManager item = (IStateManager) items [n];
					int oi = Array.IndexOf (originalItems, item);
					object ns = item.SaveViewState ();
					if (ns != null) hasData = true;
					
					if (oi == -1) {
						Type t = item.GetType ();
						int idx = knownTypes == null ? -1 : Array.IndexOf (knownTypes, t);
						if (idx != -1)
							state [n + 1] = new Triplet (oi, ns, idx);
						else
							state [n + 1] = new Triplet (oi, ns, t);
					}
					else
						state [n + 1] = new Pair (oi, ns);
				}
			} else {
				ArrayList list = new ArrayList ();
				for (int n=0; n<items.Count; n++) {
					IStateManager item = (IStateManager) items [n];
					object ns = item.SaveViewState ();
					if (ns != null) {
						hasData = true;
						list.Add (new Pair (n, ns));
					}
				}
				if (hasData) {
					list.Insert (0, false);
					state = list.ToArray ();
				}
			}
			
			if (hasData)
				return state;
			else
				return null;
		}
		
		void IStateManager.TrackViewState ()
		{
			isTrackingViewState = true;
			originalItems = new IStateManager [items.Count];
			for (int n=0; n<items.Count; n++) {
				originalItems [n] = (IStateManager) items [n];
				originalItems [n].TrackViewState ();
			}
				
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
			
			if (isTrackingViewState)
				saveEverything = true;
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
				saveEverything = true;
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
				saveEverything = true;
			}
			
			OnInsert (index, value);
			items.Insert (index, value);
			OnInsertComplete(index, value);
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
			
			if (isTrackingViewState)
				saveEverything = true;
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
					saveEverything = true;
				}
				
				items [index] = value;
			}
		}
		#endregion
	}
}
#endif

