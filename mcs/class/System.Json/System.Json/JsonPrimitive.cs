using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Json
{
	public class JsonPrimitive : JsonValue
	{
		object value;

		public JsonPrimitive (bool value)
		{
			this.value = value;
		}

		public JsonPrimitive (byte value)
		{
			this.value = value;
		}

		public JsonPrimitive (char value)
		{
			this.value = value;
		}

		public JsonPrimitive (decimal value)
		{
			this.value = value;
		}

		public JsonPrimitive (double value)
		{
			this.value = value;
		}

		public JsonPrimitive (float value)
		{
			this.value = value;
		}

		public JsonPrimitive (int value)
		{
			this.value = value;
		}

		public JsonPrimitive (long value)
		{
			this.value = value;
		}

		public JsonPrimitive (sbyte value)
		{
			this.value = value;
		}

		public JsonPrimitive (short value)
		{
			this.value = value;
		}

		public JsonPrimitive (string value)
		{
			this.value = value;
		}

		public JsonPrimitive (DateTime value)
		{
			this.value = value;
		}

		public JsonPrimitive (uint value)
		{
			this.value = value;
		}

		public JsonPrimitive (ulong value)
		{
			this.value = value;
		}

		public JsonPrimitive (ushort value)
		{
			this.value = value;
		}

		public JsonPrimitive (DateTimeOffset value)
		{
			this.value = value;
		}

		public JsonPrimitive (Guid value)
		{
			this.value = value;
		}

		public JsonPrimitive (TimeSpan value)
		{
			this.value = value;
		}

		public JsonPrimitive (Uri value)
		{
			this.value = value;
		}

		internal object Value {
			get { return value; }
		}

		public override JsonType JsonType {
			get {
				// FIXME: what should we do for null? Handle it as null so far.
				if (value == null)
					return JsonType.String;

				switch (Type.GetTypeCode (value.GetType ())) {
				case TypeCode.Boolean:
					return JsonType.Boolean;
				case TypeCode.Char:
				case TypeCode.String:
				case TypeCode.DateTime:
				case TypeCode.Object: // DateTimeOffset || Guid || TimeSpan || Uri
					return JsonType.String;
				default:
					return JsonType.Number;
				}
			}
		}

		static readonly byte [] true_bytes = Encoding.UTF8.GetBytes ("true");
		static readonly byte [] false_bytes = Encoding.UTF8.GetBytes ("false");

		public override void Save (Stream stream)
		{
			switch (JsonType) {
			case JsonType.Boolean:
				if ((bool) value)
					stream.Write (true_bytes, 0, 4);
				else
					stream.Write (false_bytes, 0, 5);
				break;
			case JsonType.String:
				stream.WriteByte ((byte) '\"');
				byte [] bytes = Encoding.UTF8.GetBytes (EscapeString (value.ToString ()));
				stream.Write (bytes, 0, bytes.Length);
				stream.WriteByte ((byte) '\"');
				break;
			default:
				bytes = Encoding.UTF8.GetBytes (GetFormattedString ());
				stream.Write (bytes, 0, bytes.Length);
				break;
			}
		}

		internal string GetFormattedString ()
		{
			switch (JsonType) {
			case JsonType.String:
				if (value is string || value == null)
					return (string) value;
				throw new NotImplementedException ("GetFormattedString from value type " + value.GetType ());
			case JsonType.Number:
				return ((IFormattable) value).ToString ("G", NumberFormatInfo.InvariantInfo);
			default:
				throw new InvalidOperationException ();
			}
		}
	}
}
