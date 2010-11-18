
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
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Web.UI;

namespace System.Web
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpStaticObjectsCollection : ICollection, IEnumerable
	{
		sealed class StaticItem {
			object this_lock = new object();
			
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
					lock (this_lock) {
						if (instance == null)
							instance = Activator.CreateInstance (type);
					}

					return instance;
				}
			}
		}

		Dictionary <string, object> objects;

		Dictionary <string, object> Objects {
			get {
				if (objects == null)
					objects = new Dictionary <string, object> (StringComparer.Ordinal);

				return objects;
			}
		}
		
		// Needs to hold object items that can be latebound and can be serialized
		public HttpStaticObjectsCollection ()
		{
		}

		// this ctor has no security requirements and is used when creating HttpApplicationState
		internal HttpStaticObjectsCollection (HttpApplicationState appstate)
		{
		}

		public object GetObject (string name)
		{
			return this [name];
		}

		public IEnumerator GetEnumerator ()
		{
			return Objects.GetEnumerator ();
		}

		public void CopyTo (Array array, int index)
		{
			if (objects == null)
				return;

			// Copied from Hashtable.CopyTo for the most part
			if (array == null)
                                throw new ArgumentNullException ("array");

                        if (index < 0)
                                throw new ArgumentOutOfRangeException ("index");

                        if (array.Rank > 1)
                                throw new ArgumentException ("array is multidimensional");

                        if ((array.Length > 0) && (index >= array.Length))
                                throw new ArgumentException ("index is equal to or greater than array.Length");

                        if (index + objects.Count > array.Length)
                                throw new ArgumentException ("Not enough room from index to end of array for this collection");

			// We need to emulate Hashtable here, which uses DictionaryEntry for its items
			foreach (var de in objects)
				array.SetValue (new DictionaryEntry (de.Key, de.Value), index++);
		}   

		internal IDictionary GetObjects ()
		{
			return Objects;
		}

		public object this [string name] {
			get {
				if (objects == null)
					return null;
				
				StaticItem item = null;
				object o;
				if (Objects.TryGetValue (name, out o))
					item = o as StaticItem;
				
				if (item == null)
					return null;
				
				return item.Instance;
			}
		}

		public int Count {
			get {
				if (objects == null)
					return 0;
				
				return Objects.Count;
			}
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		[MonoTODO ("Not implemented")]
		public bool NeverAccessed {
			get { throw new NotImplementedException (); }
		}

		public object SyncRoot {
			get { return this; }
		}

		internal HttpStaticObjectsCollection Clone ()
		{
			HttpStaticObjectsCollection coll = new HttpStaticObjectsCollection ();
			if (objects == null)
				return coll;
			
			var collObjects = coll.Objects;
			foreach (var de in objects) {
				StaticItem item = new StaticItem ((StaticItem) de.Value);
				collObjects [de.Key] = item;
			}
			
			return coll;
		}

		internal void Add (ObjectTagBuilder tag)
		{
			Objects.Add (tag.ObjectID, new StaticItem (tag.Type));
		}
		
		void Set (string name, object obj)
		{
			Objects [name] = obj;
		}

		public void Serialize (BinaryWriter writer)
		{
			if (objects == null) {
				writer.Write (0);
				return;
			}

			writer.Write (objects.Count);
			foreach (var de in objects) {
				writer.Write (de.Key);
				System.Web.Util.AltSerialization.Serialize (writer, de.Value);
			}
		}

		public static HttpStaticObjectsCollection Deserialize (BinaryReader reader)
		{
			HttpStaticObjectsCollection result = new HttpStaticObjectsCollection ();
			for (int i = reader.ReadInt32 (); i > 0; i--)
				result.Set (reader.ReadString (), System.Web.Util.AltSerialization.Deserialize (reader));

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

