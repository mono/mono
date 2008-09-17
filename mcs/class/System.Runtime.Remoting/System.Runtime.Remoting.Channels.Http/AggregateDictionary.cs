//
// System.Runtime.Remoting.Channels.AggregateDictionary.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Novell, Inc.
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

using System.Collections;

namespace System.Runtime.Remoting.Channels.Http
{
	internal class AggregateDictionary : IDictionary
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

		public object this[object key]
		{
			get
			{
				foreach (IDictionary dic in dictionaries)
					if (dic.Contains (key)) return dic[key];
				return null;
			}

			set
			{
				foreach (IDictionary dic in dictionaries)
					if (dic.Contains (key))
						dic[key] = value;
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

	internal class AggregateEnumerator : IDictionaryEnumerator
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

			if (!currente.MoveNext ()) {
				pos++;
				if (pos >= dictionaries.Length) return false;
				currente = dictionaries[pos].GetEnumerator ();
				return MoveNext ();
			}

			return true;
		}

		public void Reset ()
		{
			pos = 0;
			if (dictionaries.Length > 0)
				currente = dictionaries[0].GetEnumerator ();
		}
	}
}
