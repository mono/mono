//
// System.Web.UI/KeyedList.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

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
using System.Collections.Specialized;

namespace System.Web.UI
{
	class KeyedList : IOrderedDictionary
#if !NET_4_0
	, IStateManager // why do we implement it at all?
#endif
	{

		Hashtable objectTable = new Hashtable ();
		ArrayList objectList = new ArrayList ();

		public void Add (object key, object value)
		{
			objectTable.Add (key, value);
			objectList.Add (new DictionaryEntry (key, value));
		}

		public void Clear ()
		{
			objectTable.Clear ();
			objectList.Clear ();
		}

		public bool Contains (object key)
		{
			return objectTable.Contains (key);
		}

		public void CopyTo (Array array, int idx)
		{
			objectTable.CopyTo (array, idx);
		}

		public void Insert (int idx, object key, object value)
		{
			if (idx > Count)
				throw new ArgumentOutOfRangeException ("index");

			objectTable.Add (key, value);
			objectList.Insert (idx, new DictionaryEntry (key, value));
		}

		public void Remove (object key)
		{
			objectTable.Remove (key);
			int index = IndexOf (key);
			if (index >= 0)
				objectList.RemoveAt (index);
		}

		public void RemoveAt (int idx)
		{
			if (idx >= Count)
				throw new ArgumentOutOfRangeException ("index");

			objectTable.Remove ( ((DictionaryEntry)objectList[idx]).Key );
			objectList.RemoveAt (idx);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new KeyedListEnumerator (objectList);
		}

		IDictionaryEnumerator IOrderedDictionary.GetEnumerator ()
		{
			return new KeyedListEnumerator (objectList);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new KeyedListEnumerator (objectList);
		}
#if !NET_4_0
		void IStateManager.LoadViewState (object state)
		{
			if (state != null)
			{
				object[] states = (object[]) state;
				if (states[0] != null) {
					objectList = (ArrayList) states[0];
					for (int i = 0; i < objectList.Count; i++)
					{
						DictionaryEntry pair = (DictionaryEntry) objectList[i];
						objectTable.Add (pair.Key, pair.Value);
					}
				}
			}
		}

		object IStateManager.SaveViewState ()
		{
			object[] ret = new object[] { objectList };
			if (ret[0] == null)
				return null;

			return ret;
		}

		void IStateManager.TrackViewState ()
		{
			trackViewState = true;
		}
#endif
		public int Count {
			get { return objectList.Count; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object this[int idx] {
			get { return ((DictionaryEntry) objectList[idx]).Value; }
			set {
				if (idx < 0 || idx >= Count)
					throw new ArgumentOutOfRangeException ("index");

				object key = ((DictionaryEntry) objectList[idx]).Key;
				objectList[idx] = new DictionaryEntry (key, value);
				objectTable[key] = value;
			}
		}

		public object this[object key] {
			get { return objectTable[key]; }
			set {
				if (objectTable.Contains (key))
				{
					objectTable[key] = value;
					objectTable[IndexOf (key)] = new DictionaryEntry (key, value);
					return;
				}
				Add (key, value);
			}
		}

		public ICollection Keys {
			get { 
				ArrayList retList = new ArrayList ();
				for (int i = 0; i < objectList.Count; i++)
				{
					retList.Add ( ((DictionaryEntry)objectList[i]).Key );
				}
				return retList;
			}
		}

		public ICollection Values {
			get {
				ArrayList retList = new ArrayList ();
				for (int i = 0; i < objectList.Count; i++)
				{
					retList.Add ( ((DictionaryEntry)objectList[i]).Value );
				}
				return retList;
			}
		}

		public object SyncRoot {
			get { return this; }
		}

#if !NET_4_0
		bool trackViewState;

		bool IStateManager.IsTrackingViewState {
			get { return trackViewState; }
		}
#endif
		int IndexOf (object key)
		{
			for (int i = 0; i < objectList.Count; i++)
			{
				if (((DictionaryEntry) objectList[i]).Key.Equals (key))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
