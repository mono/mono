
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Json
{
	public class JsonArray : JsonValue, IList<JsonValue>
	{
		List<JsonValue> list;

		public JsonArray (params JsonValue [] items)
		{
			list = new List<JsonValue> ();
			AddRange (items);
		}

		public JsonArray (IEnumerable<JsonValue> items)
		{
			if (items == null)
				throw new ArgumentNullException ("items");

			list = new List<JsonValue> (items);
		}

		public override int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public override sealed JsonValue this [int index] {
			get { return list [index]; }
			set { list [index] = value; }
		}

		public override JsonType JsonType {
			get { return JsonType.Array; }
		}

		public void Add (JsonValue item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");

			list.Add (item);
		}

		public void AddRange (IEnumerable<JsonValue> items)
		{
			if (items == null)
				throw new ArgumentNullException ("items");

			list.AddRange (items);
		}

		public void AddRange (params JsonValue [] items)
		{
			if (items == null)
				return;

			list.AddRange (items);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (JsonValue item)
		{
			return list.Contains (item);
		}

		public void CopyTo (JsonValue [] array, int arrayIndex)
		{
			list.CopyTo (array, arrayIndex);
		}

		public int IndexOf (JsonValue item)
		{
			return list.IndexOf (item);
		}

		public void Insert (int index, JsonValue item)
		{
			list.Insert (index, item);
		}

		public bool Remove (JsonValue item)
		{
			return list.Remove (item);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public override void Save (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			stream.WriteByte ((byte) '[');
			for (int i = 0; i < list.Count; i++) {
				list [i].Save (stream);
				if (i < Count - 1) {
					stream.WriteByte ((byte) ',');
					stream.WriteByte ((byte) ' ');
				}
			}
			stream.WriteByte ((byte) ']');
		}

		IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
	}
}
