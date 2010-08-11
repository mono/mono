
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
		int known_count = -1;
		List<JsonValue> list;
		IEnumerable<JsonValue> source;

		public JsonArray (params JsonValue [] items)
			: this ((IEnumerable<JsonValue>) items)
		{
		}

		public JsonArray (IEnumerable<JsonValue> items)
		{
			if (items == null)
				throw new ArgumentNullException ("items");
			this.source = items;
		}

		public override int Count {
			get {
				if (known_count < 0) {
					if (list != null)
						known_count = list.Count;
					else {
						known_count = 0;
						foreach (JsonValue v in source)
							known_count++;
					}
				}
				return known_count;
			}
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public override sealed JsonValue this [int index] {
			get {
				if (list != null)
					return list [index];
				int i = -1;
				foreach (JsonValue v in source)
					if (++i == index)
						return v;
				throw new ArgumentOutOfRangeException ("index");
			}
			set {
				PopulateList ();
				list [index] = value;
			}
		}

		public override JsonType JsonType {
			get { return JsonType.Array; }
		}

		public void Add (JsonValue item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			PopulateList ();
			list.Add (item);
		}

		public void AddRange (IEnumerable<JsonValue> items)
		{
			if (items == null)
				throw new ArgumentNullException ("items");
			if (list != null)
				list.AddRange (items);
			else
				source = new MergedEnumerable<JsonValue> (source, items);
		}

		public void AddRange (JsonValue [] items)
		{
			AddRange ((IEnumerable<JsonValue>) items);
		}

		static readonly JsonValue [] empty = new JsonValue [0];

		public void Clear ()
		{
			if (list != null)
				list.Clear ();
			else
				source = empty;
		}

		public bool Contains (JsonValue item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			foreach (JsonValue v in this)
				if (object.Equals (item, v))
					return true;
			return false;
		}

		public void CopyTo (JsonValue [] array, int arrayIndex)
		{
			if (list != null)
				list.CopyTo (array, arrayIndex);
			else
				foreach (JsonValue v in source)
					array [arrayIndex++] = v;
		}

		public int IndexOf (JsonValue item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			int idx = 0;
			foreach (JsonValue v in this) {
				if (object.Equals (item, v))
					return idx;
				idx++;
			}
			return -1;
		}

		public void Insert (int index, JsonValue item)
		{
			PopulateList ();
			list.Insert (index, item);
		}

		public bool Remove (JsonValue item)
		{
			PopulateList ();
			return list.Remove (item);
		}

		public void RemoveAt (int index)
		{
			PopulateList ();
			list.RemoveAt (index);
		}

		public override void Save (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			stream.WriteByte ((byte) '[');
			for (int i = 0; i < Count; i++) {
				this [i].Save (stream);
				if (i < Count - 1) {
					stream.WriteByte ((byte) ',');
					stream.WriteByte ((byte) ' ');
				}
			}
			stream.WriteByte ((byte) ']');
		}

		void PopulateList ()
		{
			if (list == null) {
				list = new List<JsonValue> (source);
				source = list;
			}
		}

		IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator ()
		{
			return source.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return source.GetEnumerator ();
		}
	}
}
