using System;
using System.IO;
using System.Collections;

namespace System.Web {
	public sealed class HttpStaticObjectsCollection : ICollection, IEnumerable {
		private Hashtable _Objects;

		// Needs to hold object items that can be latebound and can be serialized
		public HttpStaticObjectsCollection ()
		{
			_Objects = new Hashtable();
		}

		public object GetObject (string name)
		{
			return this [name];
		}

		public IEnumerator GetEnumerator ()
		{
			return _Objects.GetEnumerator ();
		}

		public void CopyTo (Array array, int index)
		{
			_Objects.CopyTo (array, index);
		}   

		internal IDictionary GetObjects ()
		{
			return _Objects;
		}

		public object this [string name] {
			get { return _Objects [name]; }
		}

		public int Count {
			get { return _Objects.Count; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}


		private void Set (string name, object obj)
		{
			_Objects [name] = obj;
		}

		internal void Serialize (BinaryWriter w)
		{
			lock (_Objects) {
				w.Write (Count);
				foreach (string key in _Objects.Keys) {
					w.Write (key);
					object value = _Objects [key];
					if (value == null) {
						w.Write (System.Web.Util.AltSerialization.NullIndex);
						continue;
					}

					System.Web.Util.AltSerialization.SerializeByType (w, value);
				}
			}
		}

		internal static HttpStaticObjectsCollection Deserialize (BinaryReader r)
		{
			HttpStaticObjectsCollection result = new HttpStaticObjectsCollection ();
			for (int i = r.ReadInt32 (); i > 0; i--) {
				string key = r.ReadString ();
				int index = r.ReadInt32 ();
				if (index == System.Web.Util.AltSerialization.NullIndex)
					result.Set (key, null);
				else
					result.Set (key, System.Web.Util.AltSerialization.DeserializeFromIndex (index, r));
			}

			return result;
		}

		internal byte [] ToByteArray ()
		{
			MemoryStream stream = null;
			try {
				stream = new MemoryStream ();
				Serialize (new BinaryWriter (stream));
				return stream.GetBuffer ();
			} catch {
				throw;
			} finally {
				if (stream != null)
					stream.Close ();
			}
		}

		internal static HttpStaticObjectsCollection FromByteArray (byte [] data)
		{
			HttpStaticObjectsCollection objs = null;
			MemoryStream stream = null;
			try {
				stream = new MemoryStream (data);
				objs = Deserialize (new BinaryReader (stream));
			} catch {
				throw;
			} finally {
				if (stream != null)
					stream.Close ();
			}
			return objs;
		}
	}
}

