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
		private IDictionary bag;
		
		public StateBag(bool ignoreCase)
		{
			Initialize(ignoreCase);
		}
		
		public StateBag()
		{
			Initialize(false);
		}

		private void Initialize(bool ignoreCase)
		{
			this.ignoreCase = ignoreCase;
			marked = false;
			bag = new HybridDictionary(false);
		}
		
		public int Count
		{
			get
			{
				return bag.Count;
			}
		}
		
		public object this[object key]
		{
			get
			{
				string sKey = (string)key;
				if(sKey==null || sKey.Length==0)
					throw new ArgumentException(HttpRuntime.FormatResourceString("Key_Cannot_Be_Null"));
				object val = bag[sKey];
				if(val is StateItem)
					return val;
				return null;
			}
			set
			{
				Add((string)key, value);
			}
		}
		
		public ICollection Keys
		{
			get
			{
				return bag.Keys;
			}
		}
		
		public ICollection Values
		{
			get
			{
				return bag.Values;
			}
		}
		
		public StateItem Add(string key, object value)
		{
			if(key == null || key.Length == 0)
			{
				throw new ArgumentException(HttpRuntime.FormatResourceString("Key_Cannot_Be_Null"));
			}
			StateItem val = null;
			if(bag[key] is StateItem)
				val = (StateItem)(bag[key]);
			if(val==null)
			{
				if(value!=null || marked)
				{
					val = new StateItem(value);
					bag.Add(key, val);
				}
				
			} else
			{
				if(value!=null && !marked)
					bag.Remove(key);
					val.Value = value;
			}
			if(val!=null && marked)
			{
				val.IsDirty = true;
			}
			return val;
		}
		
		public void Clear()
		{
			bag.Clear();
		}
		
		public IDictionaryEnumerator GetEnumerator()
		{
			return bag.GetEnumerator();
		}
		
		public bool IsItemDirty(string key)
		{
			object o = bag[key];
			if(o is StateItem)
				return ((StateItem)o).IsDirty;
			return false;
		}
		
		public void Remove(string key)
		{
			bag.Remove(key);
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		public void SetItemDirty(string key, bool dirty)
		{
			if(bag[key] is StateItem)
				((StateItem)bag[key]).IsDirty = dirty;
		}
		
		internal bool IsTrackingViewState
		{
			get
			{
				return marked;
			}
		}
		
		internal void LoadViewState(object state)
		{
			if(state!=null)
			{
				Pair pair = (Pair)state;
				ArrayList keyList = (ArrayList)(pair.First);
				ArrayList valList = (ArrayList)(pair.Second);
				for(int i=0; i < keyList.Count; i++)
					Add((string)keyList[i], valList[i]);
			}
		}
		
		internal object SaveViewState()
		{
			if(bag.Count > 0)
			{
				ArrayList keyList = null, valList = null;
				foreach(IDictionaryEnumerator current in bag)
				{
					StateItem item = (StateItem)current.Value;
					if(item.IsDirty)
					{
						if(keyList==null)
						{
							keyList = new ArrayList();
							valList = new ArrayList();
						}
						keyList.Add(current.Key);
						valList.Add(current.Value);
					}
				}
				if(keyList!=null)
					return new Pair(keyList, valList);
			}
			return null;
		}
		
		internal void TrackViewState()
		{
			marked = true;
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}
		
		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}
		
		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}
		
		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return IsTrackingViewState;
			}
		}
		
		void ICollection.CopyTo(Array array, int index)
		{
			Values.CopyTo(array, index);
		}
		
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}
		
		object ICollection.SyncRoot
		{
			get
			{
				return this;
			}
		}
		
		void IDictionary.Add(object key, object value)
		{
			Add((string)key, value);
		}
		
		void IDictionary.Remove(object key)
		{
			Remove((string)key);
		}
		
		bool IDictionary.Contains(object key)
		{
			return bag.Contains((string)key);
		}
		
		bool IDictionary.IsFixedSize
		{
			get
			{
				return false;
			}
		}
		
		bool IDictionary.IsReadOnly
		{
			get
			{
				return false;
			}
		}
	}
}
