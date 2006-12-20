//
// System.Web.Compilation.SessionStateItemCollection
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
#if NET_2_0
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Web.Util;

namespace System.Web.SessionState 
{
	public sealed class SessionStateItemCollection : NameObjectCollectionBase, ISessionStateItemCollection, ICollection, IEnumerable
	{
		static Dictionary <Type, bool> immutables;
		bool is_dirty;
		
		static SessionStateItemCollection ()
		{
			immutables = new Dictionary <Type, bool> (14);
			Type type = typeof(string);
			immutables.Add (type, true);
			type = typeof(int);
			immutables.Add (type, true);
			type = typeof(bool);
			immutables.Add (type, true);
			type = typeof(decimal);
			immutables.Add (type, true);
			type = typeof(byte);
			immutables.Add (type, true);
			type = typeof(char);
			immutables.Add (type, true);
			type = typeof(float);
			immutables.Add (type, true);
			type = typeof(double);
			immutables.Add (type, true);
			type = typeof(sbyte);
			immutables.Add (type, true);
			type = typeof(short);
			immutables.Add (type, true);
			type = typeof(long);
			immutables.Add (type, true);
			type = typeof(ushort);
			immutables.Add (type, true);
			type = typeof(uint);
			immutables.Add (type, true);
			type = typeof(ulong);
			immutables.Add (type, true);
		}

		static bool IsMutable (object o)
		{
			return (immutables.ContainsKey (o.GetType ()) == false);
		}
		
		public SessionStateItemCollection ()
		{
		}

		internal SessionStateItemCollection (int capacity)
			: base (capacity)
		{
		}
		
		public bool Dirty {
			get { return is_dirty; }
			set { is_dirty = value; }
		}

		public object this [int index] {
			get {
				object o = BaseGet (index);
				if (o != null && IsMutable (o))
					is_dirty = true;
				return o;
			}
			
			set {
				BaseSet (index, value);
				is_dirty = true;
			}
		}
		
                public object this [string name] {
			get {
				object o = BaseGet (name);
				if (o != null && IsMutable (o))
					is_dirty = true;
				return o;
			}
			
			set {
				BaseSet (name, value);
				is_dirty = true;
			}
		}

		// Todo: why override this?
		public override KeysCollection Keys {
			get { return base.Keys; }
		}

		public void Clear ()
		{
			BaseClear ();
			is_dirty = true;
		}

		public static SessionStateItemCollection Deserialize (BinaryReader reader)
		{
			int i = reader.ReadInt32 ();
			SessionStateItemCollection ret = new SessionStateItemCollection (i);
			while (i > 0) {
				i--;
				string key = reader.ReadString ();
				int index = reader.ReadInt32 ();
				if (index == AltSerialization.NullIndex)
					ret [key] = null;
				else
					ret [key] = AltSerialization.DeserializeFromIndex (index, reader);
			}
			return ret;
		}

		public void Serialize (BinaryWriter writer)
		{
			lock (this) {
				writer.Write (Count);
				foreach (string key in base.Keys) {
					object val = BaseGet (key);
					if (val == null) {
						writer.Write (AltSerialization.NullIndex);
						continue;
					}
					AltSerialization.SerializeByType (writer, val);
				}
			}
		}
		
		// Todo: why override this?
		public override IEnumerator GetEnumerator ()
		{
			return base.GetEnumerator ();
		}

		public void Remove (string name)
		{
			BaseRemove (name);
			is_dirty = true;
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
			is_dirty = true;
		}
	}
}
#endif
