
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
using System.IO;
using System.Web.UI;

namespace System.Web {
	public sealed class HttpStaticObjectsCollection : ICollection, IEnumerable {
		private Hashtable _Objects;

		class StaticItem {
			Type type;
			object instance;

			public StaticItem (Type type)
			{
				this.type = type;
			}

			public StaticItem (StaticItem item)
			{
				this.type = item.type;
			}
			
			public object Instance {
				get {
					lock (this) {
						if (instance == null)
							instance = Activator.CreateInstance (type);
					}

					return instance;
				}
			}
		}

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
			get {
				StaticItem item = _Objects [name] as StaticItem;
				if (item == null)
					return null;
				
				return item.Instance;
			}
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

		internal HttpStaticObjectsCollection Clone ()
		{
			HttpStaticObjectsCollection coll = new HttpStaticObjectsCollection ();
			coll._Objects = new Hashtable ();
			foreach (string key in _Objects.Keys) {
				StaticItem item = new StaticItem ((StaticItem) _Objects [key]);
				coll._Objects [key] = item;
			}
			
			return coll;
		}

		internal void Add (ObjectTagBuilder tag)
		{
			_Objects.Add (tag.ObjectID, new StaticItem (tag.Type));
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

