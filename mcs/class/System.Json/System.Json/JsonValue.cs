using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;


namespace System.Json
{
	public abstract class JsonValue : IEnumerable
	{
		public static JsonValue Load (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			return Load (new StreamReader (stream, true));
		}

		public static JsonValue Load (TextReader textReader)
		{
			if (textReader == null)
				throw new ArgumentNullException ("textReader");

			var ret = new JavaScriptReader (textReader, true).Read ();

			return ToJsonValue (ret);
		}

		static IEnumerable<KeyValuePair<string,JsonValue>> ToJsonPairEnumerable (IEnumerable<KeyValuePair<string,object>> kvpc)
		{
			foreach (var kvp in kvpc)
				yield return new KeyValuePair<string,JsonValue> (kvp.Key, ToJsonValue (kvp.Value));
		}

		static IEnumerable<JsonValue> ToJsonValueEnumerable (IEnumerable<object> arr)
		{
			foreach (var obj in arr)
				yield return ToJsonValue (obj);
		}

		static JsonValue ToJsonValue (object ret)
		{
			if (ret == null)
				return null;
			var kvpc = ret as IEnumerable<KeyValuePair<string,object>>;
			if (kvpc != null)
				return new JsonObject (ToJsonPairEnumerable (kvpc));
			var arr = ret as IEnumerable<object>;
			if (arr != null)
				return new JsonArray (ToJsonValueEnumerable (arr));

			if (ret is bool)
				return new JsonPrimitive ((bool) ret);
			if (ret is byte)
				return new JsonPrimitive ((byte) ret);
			if (ret is char)
				return new JsonPrimitive ((char) ret);
			if (ret is decimal)
				return new JsonPrimitive ((decimal) ret);
			if (ret is double)
				return new JsonPrimitive ((double) ret);
			if (ret is float)
				return new JsonPrimitive ((float) ret);
			if (ret is int)
				return new JsonPrimitive ((int) ret);
			if (ret is long)
				return new JsonPrimitive ((long) ret);
			if (ret is sbyte)
				return new JsonPrimitive ((sbyte) ret);
			if (ret is short)
				return new JsonPrimitive ((short) ret);
			if (ret is string)
				return new JsonPrimitive ((string) ret);
			if (ret is uint)
				return new JsonPrimitive ((uint) ret);
			if (ret is ulong)
				return new JsonPrimitive ((ulong) ret);
			if (ret is ushort)
				return new JsonPrimitive ((ushort) ret);
			if (ret is DateTime)
				return new JsonPrimitive ((DateTime) ret);
			if (ret is DateTimeOffset)
				return new JsonPrimitive ((DateTimeOffset) ret);
			if (ret is Guid)
				return new JsonPrimitive ((Guid) ret);
			if (ret is TimeSpan)
				return new JsonPrimitive ((TimeSpan) ret);
			if (ret is Uri)
				return new JsonPrimitive ((Uri) ret);
			throw new NotSupportedException (String.Format ("Unexpected parser return type: {0}", ret.GetType ()));
		}

		public static JsonValue Parse (string jsonString)
		{
			if (jsonString == null)
				throw new ArgumentNullException ("jsonString");
			return Load (new StringReader (jsonString));
		}

		public virtual int Count {
			get { throw new InvalidOperationException (); }
		}

		public abstract JsonType JsonType { get; }

		public virtual JsonValue this [int index] {
			get { throw new InvalidOperationException (); }
			set { throw new InvalidOperationException (); }
		}

		public virtual JsonValue this [string key] {
			get { throw new InvalidOperationException (); }
			set { throw new InvalidOperationException (); }
		}

		public virtual bool ContainsKey (string key)
		{
			throw new InvalidOperationException ();
		}

		public virtual void Save (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			Save (new StreamWriter (stream));
		}

		public virtual void Save (TextWriter textWriter)
		{
			if (textWriter == null)
				throw new ArgumentNullException ("textWriter");
			SaveInternal (textWriter);
		}
		
		void SaveInternal (TextWriter w)
		{
			switch (JsonType) {
			case JsonType.Object:
				w.Write ('{');
				bool following = false;
				foreach (JsonPair pair in ((JsonObject) this)) {
					if (following)
						w.Write (", ");
					w.Write ('\"');
					w.Write (EscapeString (pair.Key));
					w.Write ("\": ");
					if (pair.Value == null)
						w.Write ("null");
					else
						pair.Value.SaveInternal (w);
					following = true;
				}
				w.Write ('}');
				break;
			case JsonType.Array:
				w.Write ('[');
				following = false;
				foreach (JsonValue v in ((JsonArray) this)) {
					if (following)
						w.Write (", ");
					v.SaveInternal (w);
					following = true;
				}
				w.Write (']');
				break;
			case JsonType.Boolean:
				w.Write ((bool) this ? "true" : "false");
				break;
			case JsonType.String:
				w.Write ('"');
				w.Write (EscapeString (((JsonPrimitive) this).GetFormattedString ()));
				w.Write ('"');
				break;
			default:
				w.Write (((JsonPrimitive) this).GetFormattedString ());
				break;
			}
		}

		public override string ToString ()
		{
			StringWriter sw = new StringWriter ();
			Save (sw);
			return sw.ToString ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new InvalidOperationException ();
		}

		internal string EscapeString (string src)
		{
			if (src == null)
				return null;

			for (int i = 0; i < src.Length; i++)
				if (src [i] == '"' || src [i] == '\\') {
					var sb = new StringBuilder ();
					if (i > 0)
						sb.Append (src, 0, i);
					return DoEscapeString (sb, src, i);
				}
			return src;
		}

		string DoEscapeString (StringBuilder sb, string src, int cur)
		{
			int start = cur;
			for (int i = cur; i < src.Length; i++)
				if (src [i] == '"' || src [i] == '\\') {
					sb.Append (src, start, i - start);
					sb.Append ('\\');
					sb.Append (src [i++]);
					start = i;
				}
			sb.Append (src, start, src.Length - start);
			return sb.ToString ();
		}

		// CLI -> JsonValue

		public static implicit operator JsonValue (bool value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (byte value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (char value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (decimal value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (double value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (float value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (int value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (long value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (sbyte value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (short value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (string value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (uint value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (ulong value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (ushort value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (DateTime value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (DateTimeOffset value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (Guid value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (TimeSpan value)
		{
			return new JsonPrimitive (value);
		}

		public static implicit operator JsonValue (Uri value)
		{
			return new JsonPrimitive (value);
		}

		// JsonValue -> CLI

		public static implicit operator bool (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToBoolean (((JsonPrimitive) value).Value);
		}

		public static implicit operator byte (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToByte (((JsonPrimitive) value).Value);
		}

		public static implicit operator char (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToChar (((JsonPrimitive) value).Value);
		}

		public static implicit operator decimal (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToDecimal (((JsonPrimitive) value).Value);
		}

		public static implicit operator double (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToDouble (((JsonPrimitive) value).Value);
		}

		public static implicit operator float (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToSingle (((JsonPrimitive) value).Value);
		}

		public static implicit operator int (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToInt32 (((JsonPrimitive) value).Value);
		}

		public static implicit operator long (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToInt64 (((JsonPrimitive) value).Value);
		}

		public static implicit operator sbyte (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToSByte (((JsonPrimitive) value).Value);
		}

		public static implicit operator short (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToInt16 (((JsonPrimitive) value).Value);
		}

		public static implicit operator string (JsonValue value)
		{
			if (value == null)
				return null;
			return (string) ((JsonPrimitive) value).Value;
		}

		public static implicit operator uint (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToUInt16 (((JsonPrimitive) value).Value);
		}

		public static implicit operator ulong (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToUInt64(((JsonPrimitive) value).Value);
		}

		public static implicit operator ushort (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return Convert.ToUInt16 (((JsonPrimitive) value).Value);
		}

		public static implicit operator DateTime (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return (DateTime) ((JsonPrimitive) value).Value;
		}

		public static implicit operator DateTimeOffset (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return (DateTimeOffset) ((JsonPrimitive) value).Value;
		}

		public static implicit operator TimeSpan (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return (TimeSpan) ((JsonPrimitive) value).Value;
		}

		public static implicit operator Guid (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return (Guid) ((JsonPrimitive) value).Value;
		}

		public static implicit operator Uri (JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			return (Uri) ((JsonPrimitive) value).Value;
		}
	}
}
