//
// StringCollection.cs
//
// Authors:
//	John Barnette (jbarn@httcb.net)
//	Sean MacIsaac (macisaac@ximian.com)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2001 John Barnette
// (C) Ximian, Inc.  http://www.ximian.com

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


namespace System.Collections.Specialized {
	
	[Serializable]
	public class StringCollection : IList {
		ArrayList data = new ArrayList ();
	
		public string this [int index] {
			get { return (string)data [index]; }
			set { data [index] = value;	}
		}
		
		public int Count {
			get { return data.Count; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}
		
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		
		public int Add (string value) {
			return data.Add (value);
		}
		
		public void AddRange (string [] value) {
			if (value == null)
				throw new ArgumentNullException ("value");

			data.AddRange (value);
		}
		
		public void Clear () {
			data.Clear ();
		}
		
		public bool Contains (string value) {
			return data.Contains (value);
		}
		
		public void CopyTo (string [] array, int index) {
			data.CopyTo (array, index);
		}
		
		public StringEnumerator GetEnumerator () {
			return new StringEnumerator (this);
		}
		
		public int IndexOf (string value) {
			return data.IndexOf (value);
		}
		
		public void Insert(int index, string value) {
			data.Insert (index, value);
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public void Remove (string value) {
			data.Remove (value);
		}
		
		public void RemoveAt (int index) {
			data.RemoveAt (index);
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (string)value; }
		}
		
		int IList.Add (object value) {
			return Add ((string)value);
		}
		
		bool IList.Contains (object value) {
			return Contains ((string) value);
		}

		int IList.IndexOf (object value) {
			return IndexOf ((string)value);
		}
		
		void IList.Insert (int index, object value) {
			Insert (index, (string)value);
		}
		
		void IList.Remove (object value) {
			Remove ((string)value);
		}
		
		void ICollection.CopyTo (Array array, int index) {
			data.CopyTo (array, index);
		}
		
		IEnumerator IEnumerable.GetEnumerator () {
			return data.GetEnumerator ();
		}
	}
}
