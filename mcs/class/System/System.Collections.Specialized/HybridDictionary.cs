//
// System.Collections.Specialized.HybridDictionary.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System;
using System.Collections;

namespace System.Collections.Specialized {
	
	[Serializable]
	public class HybridDictionary : IDictionary, ICollection, IEnumerable {
		
		private const int switchAfter = 10;

		private ListDictionary list;
		private Hashtable hashtable;
		private bool caseInsensitive = false;

		// Constructors
		
		public HybridDictionary() : this (0, false) { }
		
		public HybridDictionary (bool caseInsensitive) : this (0, caseInsensitive) { }
		
		public HybridDictionary (int initialSize) : this (initialSize, false) { }
		
		public HybridDictionary(int initialSize, bool caseInsensitive) 
		{
			this.caseInsensitive = caseInsensitive;
		
			if (initialSize <= switchAfter)
				if (caseInsensitive)
					list = new ListDictionary (CaseInsensitiveComparer.Default);
				else
					list = new ListDictionary ();
			else
				if (caseInsensitive) 
					hashtable = new Hashtable (initialSize, 
							CaseInsensitiveHashCodeProvider.Default, 
							CaseInsensitiveComparer.Default);
				else
					hashtable = new Hashtable (initialSize);
		}		

		
		// Properties
		
		public int Count {
			get {
				if (list != null)
					return list.Count;
				return hashtable.Count;
			}
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
		
		public object this [object key] {
			get { 
				if (key == null)
					throw new ArgumentNullException("key");
				if (list != null)
					return list [key];
				return hashtable [key];
			}
			set { 
				if (list != null)
					if (list.Count >= switchAfter) 
						Switch ();
					else {
						list [key] = value;
						return;
					}
				hashtable [key] = value;
			}
		}
		
		public ICollection Keys {
			get {
				if (list != null)
					return list.Keys;
				return hashtable.Keys;
			}
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		public ICollection Values {
			get { 
				if (list != null)
					return list.Values;
				return hashtable.Values;
			}
		}
		
		
		// Methods
		
		public void Add (object key, object value) 
		{
			if (list != null)
				if (list.Count >= switchAfter) 
					Switch ();
				else {
					list.Add (key, value);
					return;
				}
			hashtable.Add (key, value);
		}
		
		public void Clear ()
		{
			if (caseInsensitive)
				list = new ListDictionary (CaseInsensitiveComparer.Default);
			else
				list = new ListDictionary ();
			hashtable = null;
		}
		
		public bool Contains (object key)
		{
			if (key == null) {
				if (this.Count == 0)
					return false;
				else
					throw new ArgumentNullException ("key");
			}
			if (list != null)
				return list.Contains (key);
			return hashtable.Contains (key);
		}
		
		public void CopyTo (Array array, int index)
		{
			if (list != null) 
				list.CopyTo (array, index);
			else
				hashtable.CopyTo (array, index);
		}
		
		public IDictionaryEnumerator GetEnumerator ()
		{
			if (list != null)
				return list.GetEnumerator ();
			return hashtable.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public void Remove (object key)
		{
			if (list != null)
				list.Remove (key);
			else	
				hashtable.Remove (key);
		}
		
		private void Switch ()
		{
			if (caseInsensitive) 
				hashtable = new Hashtable (switchAfter + 1,
							CaseInsensitiveHashCodeProvider.Default, 
							CaseInsensitiveComparer.Default);
			else
				hashtable = new Hashtable (switchAfter + 1);
			IDictionaryEnumerator e = list.GetEnumerator ();
			while (e.MoveNext ())
				hashtable.Add (e.Key, e.Value);
			list = null;
		}
	}
}
