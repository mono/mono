//
// UriList.cs: Uri List
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;

namespace Microsoft.Web.Services {

	public class UriList : ICollection {

		private ArrayList list;

		public UriList () 
		{
			list = new ArrayList ();
		}

		public int Add (Uri uri) 
		{
			return list.Add (uri);
		}

		public bool Contains (Uri uri) 
		{
			return list.Contains (uri);
		}

		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (Uri uri) 
		{
			return list.IndexOf (uri);
		}

		public void Insert (int index, Uri uri) 
		{
			list.Insert (index, uri);
		}

		public void Remove (Uri uri) 
		{
			list.Remove (uri);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public int Count { 
			get { return list.Count; }
		}

		public bool IsFixedSize { 
			get { return list.IsFixedSize; }
		}

		public bool IsReadOnly { 
			get { return list.IsReadOnly; }
		}

		public bool IsSynchronized { 
			get { return list.IsSynchronized; }
		}

		public Uri this [int index] { 
			get { return (Uri) list [index]; }
			set { list [index] = value; }
		}

		public object SyncRoot { 
			get { return list.SyncRoot; }
		}
	} 
}
