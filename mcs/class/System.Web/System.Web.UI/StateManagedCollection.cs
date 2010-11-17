//
// System.Web.UI.StateManagedCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005-2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

namespace System.Web.UI {

	public abstract class StateManagedCollection : IList, IStateManager
	{		
		ArrayList items = new ArrayList ();
		bool saveEverything = false;

		protected virtual object CreateKnownType (int index)
		{
			return null;
		}

		public void SetDirty ()
		{
			saveEverything = true;
			for (int i = 0; i < items.Count; i++)
				SetDirtyObject (items[i]);
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

			Triplet state = savedState as Triplet;
			if (state == null)
				throw new InvalidOperationException ("Internal error.");

			List <int> indices = state.First as List <int>;
			List <object> states = state.Second as List <object>;
			List <object> types = state.Third as List <object>;
			IList list = this as IList;
			IStateManager item;
			object t;
			
			saveEverything = indices == null;
			if (saveEverything) {
				Clear ();

				for (int i = 0; i < states.Count; i++) {
					t = types [i];
					if (t is Type)
						item = (IStateManager) Activator.CreateInstance ((Type) t);
					else if (t is int)
						item = (IStateManager) CreateKnownType ((int) t);
					else
						continue;

					item.TrackViewState ();
					item.LoadViewState (states [i]);
					list.Add (item);
				}
				return;
			}

			int idx;
			for (int i = 0; i < indices.Count; i++) {
				idx = indices [i];

				if (idx < Count) {
					item = list [idx] as IStateManager;
					item.TrackViewState ();
					item.LoadViewState (states [i]);
					continue;
				}

				t = types [i];

				if (t is Type)
					item = (IStateManager) Activator.CreateInstance ((Type) t);
				else if (t is int)
					item = (IStateManager) CreateKnownType ((int) t);
				else
					continue;

				item.TrackViewState ();
				item.LoadViewState (states [i]);
				list.Add (item);
			}
		}
		
		void AddListItem <T> (ref List <T> list, T item)
		{
			if (list == null)
				list = new List <T> ();

			list.Add (item);
		}
			
		object IStateManager.SaveViewState ()
		{
			Type[] knownTypes = GetKnownTypes ();
			bool haveData = false, haveKnownTypes = knownTypes != null && knownTypes.Length > 0;
			int count = items.Count;
			IStateManager item;
			object itemState;
			Type type;
			int idx;
			List <int> indices = null;
			List <object> states = null;
			List <object> types = null;

			for (int i = 0; i < count; i++) {
				item = items [i] as IStateManager;
				if (item == null)
					continue;
				item.TrackViewState ();
				itemState = item.SaveViewState ();
				if (saveEverything || itemState != null) {
					haveData = true;
					type = item.GetType ();
					idx = haveKnownTypes ? Array.IndexOf (knownTypes, type) : -1;

					if (!saveEverything)
						AddListItem <int> (ref indices, i);
					AddListItem <object> (ref states, itemState);
					if (idx == -1)
						AddListItem <object> (ref types, type);
					else
						AddListItem <object> (ref types, idx);
				}
			}

			if (!haveData)
				return null;

			return new Triplet (indices, states, types);
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
			IList list = (IList)this;
			int i = list.IndexOf (value);
			if (i >= 0)
				list.RemoveAt (i);
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
					SetDirty ();
				}
				
				items [index] = value;
			}
		}
		#endregion
	}
}

