
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
/**
 * Namespace: System.Web.UI
 * Class:     StateBag
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI
{
	public sealed class StateBag : IStateManager, IDictionary, ICollection, IEnumerable
	{
		private bool ignoreCase;
		private bool marked;
		private HybridDictionary bag;
		
		public StateBag (bool ignoreCase)
		{
			Initialize (ignoreCase);
		}
		
		public StateBag ()
		{
			Initialize (false);
		}

		private void Initialize (bool ignoreCase)
		{
			this.ignoreCase = ignoreCase;
			marked = false;
			bag = new HybridDictionary (ignoreCase);
		}
		
		public int Count {
			get { return bag.Count; }
		}

		
		public object this [string key] {
			get {
				if (key == null || key.Length == 0)
					throw new ArgumentException (HttpRuntime.FormatResourceString ("Key_Cannot_Be_Null"));

				object val = bag [key];

				if (val is StateItem)
					return ((StateItem) val).Value;

				return null; // 
			}

			set { Add (key, value); }
		}

		object IDictionary.this [object key] {
			get { return this [(string) key] as object; }

			set { Add ((string) key, value); }
		}
		
		public ICollection Keys {
			get { return bag.Keys; }
		}
		
		public ICollection Values {
			get { return bag.Values; }
		}
		
		public StateItem Add (string key, object value)
		{
			if (key == null || key.Length == 0)
				throw new ArgumentException (HttpRuntime.FormatResourceString ("Key_Cannot_Be_Null"));

			StateItem val = bag [key] as StateItem; //don't throw exception when null
			if(val == null) {
				if(value != null || marked) {
					val = new StateItem (value);
					bag.Add (key, val);
				}
			}
			else if (value == null && !marked)
				bag.Remove (key);
			else
				val.Value = value;

			if (val != null && marked) {
				val.IsDirty = true;
			}

			return val;
		}
		
		public void Clear ()
		{
			bag.Clear ();
		}
		
		public IDictionaryEnumerator GetEnumerator ()
		{
			return bag.GetEnumerator ();
		}
		
		public bool IsItemDirty (string key)
		{
			object o = bag [key];

			if (o is StateItem)
				return ((StateItem) o).IsDirty;
			
			return false;
		}
		
		public void Remove (string key)
		{
			bag.Remove (key);
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		public void SetItemDirty (string key, bool dirty)
		{
			if (bag [key] is StateItem)
				((StateItem) bag [key]).IsDirty = dirty;
		}
		
		internal bool IsTrackingViewState {
			get { return marked; }
		}
		
		internal void LoadViewState (object state)
		{
			if(state!=null) {
				Pair pair = (Pair) state;
				ArrayList keyList = (ArrayList) (pair.First);
				ArrayList valList = (ArrayList) (pair.Second);

				int valCount = valList.Count;
				for(int i = 0; i < keyList.Count; i++) {
					if (i < valCount)
						Add ((string) keyList [i], valList [i]);
					else
						Add ((string) keyList [i], null);
				}
			}
		}
		
		internal object SaveViewState ()
		{
			if(bag.Count > 0) {
				ArrayList keyList = null, valList = null;

				foreach (string key in bag.Keys) {
					StateItem item = (StateItem) bag [key];

					if (item.IsDirty) {
						if (keyList == null) {
							keyList = new ArrayList ();
							valList = new ArrayList ();
						}
						
						keyList.Add (key);
						valList.Add (item.Value);
					}
				}

				if (keyList!=null)
					return new Pair (keyList, valList);
			}
			return null;
		}
		
		internal void TrackViewState()
		{
			marked = true;
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}
		
		void ICollection.CopyTo (Array array, int index)
		{
			Values.CopyTo (array, index);
		}
		
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot
		{
			get { return this; }
		}
		
		void IDictionary.Add (object key, object value)
		{
			Add ((string) key, value);
		}
		
		void IDictionary.Remove (object key)
		{
			Remove ((string) key);
		}
		
		bool IDictionary.Contains (object key)
		{
			return bag.Contains ((string) key);
		}
		
		bool IDictionary.IsFixedSize {
			get { return false; }
		}
		
		bool IDictionary.IsReadOnly {
			get { return false; }
		}
		
#if NET_2_0
		public void SetDirty ()
		{
			foreach (string key in bag.Keys)
				SetItemDirty (key, true);
		}
#endif

	}
}
