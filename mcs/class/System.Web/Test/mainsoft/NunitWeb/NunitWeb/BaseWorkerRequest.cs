using System;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Collections;

namespace MonoTests.SystemWeb.Framework
{
	public class BaseWorkerRequest: SimpleWorkerRequest, IDictionary
	{
		public BaseWorkerRequest (string page, string query, TextWriter writer):
			base (page, query, writer)
		{
		}

		Hashtable data = new Hashtable ();
		#region IDictionary Members

		void IDictionary.Add (object key, object value)
		{
			data.Add (key, value);
		}

		void IDictionary.Clear ()
		{
			data.Clear ();
		}

		bool IDictionary.Contains (object key)
		{
			return data.Contains (key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return data.GetEnumerator ();
		}

		bool IDictionary.IsFixedSize
		{
			get { return data.IsFixedSize; }
		}

		bool IDictionary.IsReadOnly
		{
			get { return data.IsReadOnly; }
		}

		ICollection IDictionary.Keys
		{
			get { return data.Keys; }
		}

		void IDictionary.Remove (object key)
		{
			data.Remove (key);
		}

		ICollection IDictionary.Values
		{
			get { return data.Values; }
		}

		object IDictionary.this[object key]
		{
			get
			{
				return data[key];
			}
			set
			{
				data[key] = value;
			}
		}

		void ICollection.CopyTo (Array array, int index)
		{
			data.CopyTo (array, index);
		}

		int ICollection.Count
		{
			get { return data.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return data.IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return data.SyncRoot; }
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return data.GetEnumerator ();
		}
		#endregion
	}
}
