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


namespace System.Collections.Specialized {
	
	[Serializable]
	public class StringCollection : IList {
		ArrayList strings = new ArrayList ();
	
		public string this [int index] {
			get { return (string)strings [index]; }
			set { strings [index] = value;	}
		}
		
		public int Count {
			get { return strings.Count; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}
		
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		
		public int Add (string value) {
			return strings.Add (value);
		}
		
		public void AddRange (string [] value) {
			if (value == null)
				throw new ArgumentNullException ("value");

			strings.AddRange (value);
		}
		
		public void Clear () {
			strings.Clear ();
		}
		
		public bool Contains (string value) {
			return strings.Contains (value);
		}
		
		public void CopyTo (string [] array, int index) {
			strings.CopyTo (array, index);
		}
		
		public StringEnumerator GetEnumerator () {
			return new StringEnumerator (this);
		}
		
		public int IndexOf (string value) {
			return strings.IndexOf (value);
		}
		
		public void Insert(int index, string value) {
			strings.Insert (index, value);
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public void Remove (string value) {
			strings.Remove (value);
		}
		
		public void RemoveAt (int index) {
			strings.RemoveAt (index);
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
			strings.CopyTo (array, index);
		}
		
		IEnumerator IEnumerable.GetEnumerator () {
			return strings.GetEnumerator ();
		}
	}
}
