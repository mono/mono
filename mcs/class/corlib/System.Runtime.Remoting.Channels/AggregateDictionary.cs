//
// System.Runtime.Remoting.Channels.AggregateDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Novell, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting
{
	internal class AggregateDictionary: IDictionary
	{
		IDictionary[] dictionaries;
		ArrayList _values;
		ArrayList _keys;
		
		public AggregateDictionary (IDictionary[] dics)
		{
			dictionaries = dics;
		}
		
		public bool IsFixedSize 
		{ 
			get { return true; }
		}		
		
		public bool IsReadOnly 
		{ 
			get { return true; }
		} 
		
		public object this [object key]
		{ 
			get 
			{
				foreach (IDictionary dic in dictionaries)
					if (dic.Contains (key)) return dic [key];
				return null;
			}
			
			set
			{
				throw new NotSupportedException ();
			}
		}
		
		public ICollection Keys 
		{ 
			get
			{
				if (_keys != null) return _keys;
				
				_keys = new ArrayList ();
				foreach (IDictionary dic in dictionaries)
					_keys.AddRange (dic.Keys);
				return _keys;
			}
		} 
		
		public ICollection Values 
		{ 
			get
			{
				if (_values != null) return _values;
				
				_values = new ArrayList ();
				foreach (IDictionary dic in dictionaries)
					_values.AddRange (dic.Values);
				return _values;
			}
		}
		
		public void Add (object key, object value)
		{
			throw new NotSupportedException ();
		}
		
		public void Clear ()
		{
			throw new NotSupportedException ();
		}
		
		public bool Contains (object ob)
		{
			foreach (IDictionary dic in dictionaries)
				if (dic.Contains (ob)) return true;
			return false;
		}
		
		public IDictionaryEnumerator GetEnumerator ()
		{
			return new AggregateEnumerator (dictionaries);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new AggregateEnumerator (dictionaries);
		}
		
		public void Remove (object ob)
		{
			throw new NotSupportedException ();
		}
		
		public void CopyTo (Array array, int index)
		{
			foreach (object ob in this)
				array.SetValue (ob, index++);
		}
		
		public int Count 
		{ 
			get
			{
				int c = 0;
				foreach (IDictionary dic in dictionaries)
					c += dic.Count;
				return c;
			}
		}
		
		public bool IsSynchronized 
		{ 
			get { return false; }
		}
		
		public object SyncRoot 
		{ 
			get { return this; }
		} 
	}
	
	internal class AggregateEnumerator: IDictionaryEnumerator
	{
		IDictionary[] dictionaries;
		int pos = 0;
		IDictionaryEnumerator currente;
		
		public AggregateEnumerator (IDictionary[] dics)
		{
			dictionaries = dics;
			Reset ();
		}
		
		public DictionaryEntry Entry 
		{ 
			get { return currente.Entry; }
		}
		
		public object Key 
		{
			get { return currente.Key; }
		}
		
		public object Value 
		{ 
			get { return currente.Value; }
		}
		
		public object Current 
		{ 
			get { return currente.Current; }
		}
		
		public bool MoveNext ()
		{
			if (pos >= dictionaries.Length) return false;

			if (!currente.MoveNext()) 
			{
				pos++;
				if (pos >= dictionaries.Length) return false;
				currente = dictionaries [pos].GetEnumerator ();
				return MoveNext ();
			}
			
			return true;
		}
		
		public void Reset ()
		{
			pos = 0;
			if (dictionaries.Length > 0)
				currente = dictionaries [0].GetEnumerator ();
		}
	}
}
