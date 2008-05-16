//
// System.Web.UI.StateManagedCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

namespace System.Web.UI {

	public abstract class StateManagedCollection : IList, IStateManager {

		ArrayList items = new ArrayList ();
		bool saveEverything = false;

		protected virtual object CreateKnownType (int index)
		{
			return null;
		}

		public void SetDirty ()
		{
			saveEverything = true;
		}

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
				foreach (IStateManager i in items)
					i.LoadViewState (null);
				return;
			}
			
			Triplet[] state = savedState as Triplet[];
			saveEverything = (bool)(state [0].First);
			if (saveEverything)
				items.Clear ();

			object itemState;
			object type;
			Triplet triplet;
			IStateManager item;
			
			for (int i = 1; i < state.Length; i++) {
				triplet = state [i];
				if (triplet == null)
					continue;

				itemState = triplet.Second;
				type = triplet.Third;
				if (type is Type)
					item = (IStateManager) Activator.CreateInstance ((Type) type);
				else if (type is int)
					item = (IStateManager) CreateKnownType ((int) type);
				else
					continue;

				if (saveEverything)
					((IList)this).Add (item);
				else
					item.TrackViewState ();
				
				item.LoadViewState (itemState);
			}
		}
		
		object IStateManager.SaveViewState ()
		{
			bool hasData = false;
			Type[] knownTypes = GetKnownTypes ();
			bool haveKnownTypes = knownTypes != null;
			List <Triplet> state = new List <Triplet> ();
			int count = items.Count;
			IStateManager item;
			object itemState;
			Type type;
			int idx;

			for (int i = 0; i < count; i++) {
				item = items [i] as IStateManager;
				if (item == null)
					continue;
				item.TrackViewState ();
				itemState = item.SaveViewState ();
				if (saveEverything || itemState != null) {
					hasData = true;
					type = item.GetType ();
					idx = haveKnownTypes ? Array.IndexOf (knownTypes, type) : -1;
					if (idx == -1)
						state.Add (new Triplet (i, itemState, type));
					else
						state.Add (new Triplet (i, itemState, idx));
				}
			}

			if (hasData) {
				state.Insert (0, new Triplet (saveEverything, null, null));
				return state.ToArray ();
			} else
				return null;
		}
		
		void IStateManager.TrackViewState ()
		{
			isTrackingViewState = true;
			if (items != null && items.Count > 0) {
				IStateManager item;
				foreach (object o in items) {
					item = o as IStateManager;
					if (item == null)
						continue;
					item.TrackViewState ();
				}
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
				SetDirty ();
		}
		
		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		public void CopyTo (Array array, int index)
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
				SetDirty ();
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
				SetDirty ();
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
			int i = items.IndexOf (value);
			if (i >= 0)
				((IList)this).RemoveAt (i);
		}

		void IList.RemoveAt (int index)
		{
			object o = items [index];
			
			OnRemove (index, o);
			items.RemoveAt (index);
			OnRemoveComplete(index, o);
			
			if (isTrackingViewState)
				SetDirty ();
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
			return items.Contains (value);
		}
		
		int IList.IndexOf (object value)
		{
			if (value == null)
				return -1;
			
			OnValidate (value);
			return items.IndexOf (value);
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
					SetDirty ();
				}
				
				items [index] = value;
			}
		}
		#endregion
	}
}
#endif

