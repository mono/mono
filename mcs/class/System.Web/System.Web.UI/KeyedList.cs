//
// System.Web.UI/KeyedList.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

#if NET_1_2

using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI
{

	public class KeyedList : IOrderedDictionary, IStateManager
	{

		private Hashtable objectTable = new Hashtable ();
		private ArrayList objectList = new ArrayList ();

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
			objectList.RemoveAt (IndexOf (key));
		}

		public void RemoveAt (int idx)
		{
			if (idx >= Count)
				throw new ArgumentOutOfRangeException ("index");

			objectTable.Remove ( ((DictionaryEntry)objectList[idx]).Key );
			objectList.RemoveAt (idx);
		}

		private IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new KeyedListEnumerator (objectList);
		}

		private IEnumerator IEnumerable.GetEnumerator ()
		{
			return new KeyedListEnumerator (objectList);
		}

		private void IStateManager.LoadViewState (object state)
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

		private object IStateManager.SaveViewState ()
		{
			object[] ret = new object[] { objectList };
			if (ret[0] == null)
				return null;

			return ret;
		}

		private void IStateManager.TrackViewState ()
		{
			trackViewState = true;
		}

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

		private bool trackViewState;
		private bool IStateManager.IsTrackingViewState {
			get { return trackViewState; }
		}

		private int IndexOf (object key)
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

#endif
