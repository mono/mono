using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;


namespace System.Json
{
	public class JsonObject : JsonValue//, IDictionary<string, JsonValue>, ICollection<KeyValuePair<string, JsonValue>>
	{
		int known_count = -1;
		Dictionary<string, JsonValue> map;
		JsonPairEnumerable source;

		public JsonObject (params KeyValuePair<string, JsonValue> [] items)
			: this ((JsonPairEnumerable) items)
		{
		}

		public JsonObject (JsonPairEnumerable items)
		{
			if (items == null)
				throw new ArgumentNullException ("items");
			this.source = items;
		}

		public override int Count {
			get {
				if (known_count < 0) {
					if (map != null)
						known_count = map.Count;
					else {
						known_count = 0;
						foreach (JsonPair p in source)
							known_count++;
					}
				}
				return known_count;
			}
		}

		public IEnumerator<JsonPair> GetEnumerator ()
		{
			return AsEnumerable ().GetEnumerator ();
		}

		JsonPairEnumerable AsEnumerable ()
		{
			return map ?? source;
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public override sealed JsonValue this [string key] {
			get {
				foreach (JsonPair pair in AsEnumerable ())
					if (pair.Key == key)
						return pair.Value;
				throw new KeyNotFoundException (String.Format ("key '{0}' was not found.", key));
			}
			set {
				PopulateMap ();
				map [key] = value;
			}
		}

		public override JsonType JsonType {
			get { return JsonType.Object; }
		}

		public IEnumerable<string> Keys {
			get {
				foreach (JsonPair pair in AsEnumerable ())
					yield return pair.Key;
			}
		}

		public IEnumerable<JsonValue> Values {
			get {
				foreach (JsonPair pair in AsEnumerable ())
					yield return pair.Value;
			}
		}

		public void Add (string key, JsonValue value)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (value == null)
				throw new ArgumentNullException ("value");
			PopulateMap ();
			map.Add (key, value);
		}

		public void Add (JsonPair pair)
		{
			Add (pair.Key, pair.Value);
		}

		public void AddRange (JsonPairEnumerable items)
		{
			if (items == null)
				throw new ArgumentNullException ("items");
			source = new MergedEnumerable<JsonPair> (source, items);
			map = null;
		}

		public void AddRange (JsonPair [] items)
		{
			AddRange ((JsonPairEnumerable) items);
		}

		static readonly JsonPair [] empty = new JsonPair [0];

		public void Clear ()
		{
			if (map != null)
				map.Clear ();
			else
				source = empty;
		}

		public override bool ContainsKey (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			foreach (string k in Keys)
				if (k == key)
					return true;
			return false;
		}

		public void CopyTo (JsonPair [] array, int arrayIndex)
		{
			foreach (JsonPair p in AsEnumerable ())
				array [arrayIndex++] = p;
		}

		public void Remove (string key)
		{
			PopulateMap ();
			map.Remove (key);
		}

		public override void Save (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			stream.WriteByte ((byte) '{');
			foreach (JsonPair pair in this) {
				stream.WriteByte ((byte) '"');
				byte [] bytes = Encoding.UTF8.GetBytes (EscapeString (pair.Key));
				stream.Write (bytes, 0, bytes.Length);
				stream.WriteByte ((byte) '"');
				stream.WriteByte ((byte) ',');
				stream.WriteByte ((byte) ' ');
				pair.Value.Save (stream);
			}
			stream.WriteByte ((byte) '}');
		}

		void PopulateMap ()
		{
			if (map == null) {
				map = new Dictionary<string, JsonValue> ();
				foreach (JsonPair pair in source)
					map.Add (pair.Key, pair.Value);
			}
		}
	}
}
