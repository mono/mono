//
// System.Collections.Generic.Collection
//
// Author:
//    Carlos Alberto Cortez (carlos@unixmexico.org)
//
// (C) 2004 Carlos Alberto Cortez
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Collections.Generic 
{

	[CLSCompliant(false)]
	[ComVisible(false)]
	public class Collection <T> : ICollection <T>, IEnumerable <T>, IList <T>,
			ICollection, IEnumerable, IList
	{

		const int defaultLength = 16;

		T [] contents;
		int count;
		int modcount;

		public Collection ()
		{
			contents = new T [defaultLength];
		}

		public Collection (List <T> list) : this (list, list.Count * 2)
		{
		}

		internal Collection (IList <T> list, int length)
		{
			if (list == null)
				throw new ArgumentNullException ("list");

			contents = new T [length];
			list.CopyTo (contents, 0);
			count = list.Count;
		}
		
		public void Add (T item)
		{
			if (count >= contents.Length) 
				Array.Resize<T> (ref contents, contents.Length * 2);
			
			InsertItem (count, item);

		}

		public void Clear ()
		{
			ClearItems ();
		}

		protected virtual void ClearItems ()
		{
			modcount++;
			if (count <= 0)
				return;

			Array.Clear (contents, 0, count);
			count = 0;
		}

		public bool Contains (T item)
		{
			return IndexOf (item) >= 0;
		}

		public void CopyTo (T [] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (count < 0)
				return;

			Array.Copy (contents, 0, array, index, count);
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return new Enumerator (this);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (array.Rank > 1)
				throw new ArgumentException ("Only single dimension arrays are supported.","array");

			CopyTo ((T []) array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		int IList.Add (object value)
		{
			int pos = count;
			CheckType (value);
			Add ((T) value);

			return pos;
		}

		bool IList.Contains (object value)
		{
			CheckType (value);
			return Contains ((T) value);
		}

		int IList.IndexOf (object value)
		{
			CheckType (value);
			return IndexOf ((T) value);
		}

		void IList.Insert (int index, object value)
		{
			CheckType (value);
			Insert (index, (T) value);
		}

		void IList.Remove (object value)
		{
			CheckType (value);
			Remove ((T) value);
		}

		public int IndexOf (T item)
		{
			if (item == null)
				for (int i = 0; i < count; i++) {
					if (contents [i] == null)
						return i;
				}
			else 
				for (int i = 0; i < count; i++) {
					if (item.Equals (contents [i]))
						return i;
				}

			return -1;
		}

		public void Insert (int index, T item)
		{
			if (index < 0 || index > count)
				throw new ArgumentOutOfRangeException ("index");

			if (count >= contents.Length)
				Array.Resize<T> (ref contents, contents.Length * 2);

			InsertItem (index, item);

		}

		protected virtual void InsertItem (int index, T item)
		{
			modcount++;
			//
			// Shift them by 1
			//
			int rest = count - index;
			if (rest > 0)
				Array.Copy (contents, index, contents, index + 1, rest);

			contents [index] = item;
			count++;
		}

		public bool Remove (T item)
		{
			int index = IndexOf (item);
			if (index > -1) {
				RemoveItem (index);
				return true;
			}
			
			return false;
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index >= count)
				throw new ArgumentOutOfRangeException ("index");

			RemoveItem (index);
			
		}

		protected virtual void RemoveItem (int index)
		{
			modcount++;
			
			int rest = count - index - 1;
			if (rest > 0)
				Array.Copy (contents, index + 1, contents, index, rest);
			//
			// Clear the last element
			//
			contents [--count] = default (T);
		}

		protected virtual void SetItem (int index, T item)
		{
			modcount++;
			contents [index] = item;
		}

		public int Count {

			get {
				return count;
			}
			
		}

		public T this [int index] {

			get {
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException ("index");	
				return contents [index];
			}

			set {
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException ("index");
				SetItem (index, value);
			}
			
		}

		object IList.this [int index] {

			get {
				return this [index];
			}

			set {
				CheckType(value);
				this [index] = (T) value;
			}

		}

		//
		// We could try to make List.contents internal,
		// avoiding the box and unbox when copying
		//
		protected internal List <T> Items {

			get {
				return new List <T> (this);
			}
			
		}

		public bool IsFixedSize {

			get {
				return false;
			}

		}

		public bool IsReadOnly {
			
			get {
				return false;
			}
			
		}

		public bool IsSynchronized {
			
			get {
				return false;
			}
			
		}

		public object SyncRoot {
			
			get {
				return this;
			}
			
		}

		private void CheckType (object value)
		{
			if (!(value is T)) {
				string valtype = value.GetType ().ToString ();
				throw new ArgumentException (
						String.Format ("A value of type {0} can't be used in this generic collection.", valtype),
						"value");
			}
		}

		//
		// Will we use this iterator until iterators support is done?
		//
		private struct Enumerator : IEnumerator <T>, IEnumerator 
		{

			private Collection <T> collection;
			private int current;
			private int modcount;

			public Enumerator (Collection <T> collection)
			{
				this.collection = collection;
				modcount = collection.modcount;
				current = -1;
			}

			public void Dispose()
			{
				modcount = -1;
			}

			object IEnumerator.Current {

				get {
					return Current;
				}
			
			}

			void IEnumerator.Reset ()
			{
				if (modcount != collection.modcount)
					throw new InvalidOperationException 
						("Collection was modified while enumerating.");

				current = -1;
			}

			public bool MoveNext ()
			{
				if (modcount != collection.modcount)
					throw new InvalidOperationException 
						("Collection was modified while enumerating.");
				
				current++;
				return current < collection.count;
			}

			public T Current {

				get {
					if (current < 0 || current >= collection.count)
						throw new InvalidOperationException 
							("Collection was modified while enumerating.");

					return collection.contents [current];
				}

			}

		}
		
	}


}

#endif

