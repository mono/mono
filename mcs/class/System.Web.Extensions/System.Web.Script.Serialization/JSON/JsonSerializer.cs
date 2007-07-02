#region License
// Copyright 2006 James Newton-King
// http://www.newtonsoft.com
//
// Copyright 2007 Konstantin Triger <kostat@mainsoft.com>
//
// This work is licensed under the Creative Commons Attribution 2.5 License
// http://creativecommons.org/licenses/by/2.5/
//
// You are free:
//    * to copy, distribute, display, and perform the work
//    * to make derivative works
//    * to make commercial use of the work
//
// Under the following conditions:
//    * For any reuse or distribution, you must make clear to others the license terms of this work.
//    * Any of these conditions can be waived if you get permission from the copyright holder.
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json.Utilities;
using System.Web.Script.Serialization;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies reference loop handling options for the <see cref="JsonWriter"/>.
	/// </summary>
	enum ReferenceLoopHandling
	{
		/// <summary>
		/// Throw a <see cref="JsonSerializationException"/> when a loop is encountered.
		/// </summary>
		Error = 0,
		/// <summary>
		/// Ignore loop references and do not serialize.
		/// </summary>
		Ignore = 1,
		/// <summary>
		/// Serialize loop references.
		/// </summary>
		Serialize = 2
	}

	/// <summary>
	/// Serializes and deserializes objects into and from the Json format.
	/// The <see cref="JsonSerializer"/> enables you to control how objects are encoded into Json.
	/// </summary>
	sealed class JsonSerializer
	{
		sealed class DeserializerLazyDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly JsonReader _reader;
			readonly JsonSerializer _serializer;
			public DeserializerLazyDictionary (JsonReader reader, JsonSerializer serializer) {
				_reader = reader;
				_serializer = serializer;
			}

			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				return _serializer.PopulateObject (_reader);
			}
		}

		sealed class SerializerLazyDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly object _source;

			public SerializerLazyDictionary (object source) {
				_source = source;
			}

			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				foreach (MemberInfo member in ReflectionUtils.GetFieldsAndProperties (_source.GetType (), BindingFlags.Public | BindingFlags.Instance)) {
					if (ReflectionUtils.CanReadMemberValue (member) && !member.IsDefined (typeof (ScriptIgnoreAttribute), true))
						if (!ReflectionUtils.IsIndexedProperty (member))
							yield return new KeyValuePair<string, object> (member.Name, ReflectionUtils.GetMemberValue (member, _source));
				}
			}
		}

		sealed class GenericDictionaryLazyDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly object _source;
			readonly PropertyInfo _piKeys;
			readonly PropertyInfo _piValues;


			public GenericDictionaryLazyDictionary (object source, Type dictType) {
				_source = source;
				_piKeys = dictType.GetProperty ("Keys");
				_piValues = dictType.GetProperty ("Values");
			}

			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				
				IEnumerable eKeys = (IEnumerable) _piKeys.GetValue (_source, null);
				IEnumerator eValues = ((IEnumerable) _piValues.GetValue (_source, null)).GetEnumerator();
				foreach (object key in eKeys) {
					string keyString = key == null ? null : key.ToString ();
					if (!eValues.MoveNext ())
						throw new IndexOutOfRangeException (keyString);


					yield return new KeyValuePair<string, object> (keyString, eValues.Current);
				}

				if (eValues.MoveNext ())
					throw new IndexOutOfRangeException (eValues.Current != null ? eValues.Current.ToString () : String.Empty);
			}
		}

		private ReferenceLoopHandling _referenceLoopHandling;
		private int _level;
		readonly JavaScriptSerializer _context;

		/// <summary>
		/// Get or set how reference loops (e.g. a class referencing itself) is handled.
		/// </summary>
		public ReferenceLoopHandling ReferenceLoopHandling
		{
			get { return _referenceLoopHandling; }
			set
			{
				if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_referenceLoopHandling = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonSerializer"/> class.
		/// </summary>
		public JsonSerializer(JavaScriptSerializer context)
		{
			_context = context;
			_referenceLoopHandling = ReferenceLoopHandling.Error;
		}

		#region Deserialize
		public object Deserialize (TextReader reader) {
			return Deserialize (new JsonReader (reader));
		}

		/// <summary>
		/// Deserializes the Json structure contained by the specified <see cref="JsonReader"/>
		/// into an instance of the specified type.
		/// </summary>
		/// <param name="reader">The type of object to create.</param>
		/// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
		/// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
		object Deserialize (JsonReader reader)
		{
			if (!reader.Read())
				return null;

			return GetObject(reader);
		}

		private object GetObject (JsonReader reader/*, Type objectType*/) {
			_level++;

			object value;

			switch (reader.TokenType) {
			// populate a typed object or generic dictionary/array
			// depending upon whether an objectType was supplied
			case JsonToken.StartObject:
				//value = PopulateObject(reader/*, objectType*/);
				value = new DeserializerLazyDictionary (reader, this);
				break;
			case JsonToken.StartArray:
				value = PopulateList (reader/*, objectType*/);
				break;
			case JsonToken.Integer:
			case JsonToken.Float:
			case JsonToken.String:
			case JsonToken.Boolean:
			case JsonToken.Date:
				//value = EnsureType(reader.Value, objectType);
				value = reader.Value;
				break;
			case JsonToken.Constructor:
				value = reader.Value.ToString ();
				break;
			case JsonToken.Null:
			case JsonToken.Undefined:
				value = null;
				break;
			default:
				throw new JsonSerializationException ("Unexpected token whil deserializing object: " + reader.TokenType);
			}

			_level--;

			return value;
		}

		private IEnumerable<object> PopulateList(JsonReader reader/*, Type objectType*/)
		{

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.EndArray:
						yield break;
					case JsonToken.Comment:
						break;
					default:
						yield return GetObject(reader/*, elementType*/);

						break;
				}
			}

			throw new JsonSerializationException("Unexpected end when deserializing array.");
		}

		private IEnumerator<KeyValuePair<string, object>> PopulateObject (JsonReader reader/*, Type objectType*/)
		{
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.PropertyName:
						string memberName = reader.Value.ToString();

						if (!reader.Read ())
							throw new JsonSerializationException (string.Format ("Unexpected end when setting {0}'s value.", memberName));
						yield return new KeyValuePair<string, object> (memberName, GetObject (reader));
						break;
					case JsonToken.EndObject:
						yield break;
					default:
						throw new JsonSerializationException("Unexpected token when deserializing object: " + reader.TokenType);
				}
			}

			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}
		#endregion

		#region Serialize
		/// <summary>
		/// Serializes the specified <see cref="Object"/> and writes the Json structure
		/// to a <c>Stream</c> using the specified <see cref="TextWriter"/>. 
		/// </summary>
		/// <param name="textWriter">The <see cref="TextWriter"/> used to write the Json structure.</param>
		/// <param name="value">The <see cref="Object"/> to serialize.</param>
		public void Serialize(TextWriter textWriter, object value)
		{
			Serialize(new JsonWriter(textWriter), value);
		}

		/// <summary>
		/// Serializes the specified <see cref="Object"/> and writes the Json structure
		/// to a <c>Stream</c> using the specified <see cref="JsonWriter"/>. 
		/// </summary>
		/// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the Json structure.</param>
		/// <param name="value">The <see cref="Object"/> to serialize.</param>
		void Serialize(JsonWriter jsonWriter, object value)
		{
			SerializeValue(jsonWriter, value);
		}


		private void SerializeValue(JsonWriter writer, object value)
		{
			//JsonConverter converter;

			if (value == null) {
				writer.WriteNull ();
			}
			else {
				JavaScriptConverter jsconverter = _context.GetConverter (value.GetType ());
				if (jsconverter != null) {
					value = jsconverter.Serialize (value, _context);
					if (value == null) {
						writer.WriteNull ();
						return;
					}
				}

				Type valueType = value.GetType ();
				switch (Type.GetTypeCode (valueType)) {
				case TypeCode.String:
					writer.WriteValue ((string) value);
					break;
				case TypeCode.Char:
					writer.WriteValue ((char) value);
					break;
				case TypeCode.Boolean:
					writer.WriteValue ((bool) value);
					break;
				case TypeCode.SByte:
					writer.WriteValue ((sbyte) value);
					break;
				case TypeCode.Int16:
					writer.WriteValue ((short) value);
					break;
				case TypeCode.UInt16:
					writer.WriteValue ((ushort) value);
					break;
				case TypeCode.Int32:
					writer.WriteValue ((int) value);
					break;
				case TypeCode.Byte:
					writer.WriteValue ((byte) value);
					break;
				case TypeCode.UInt32:
					writer.WriteValue ((uint) value);
					break;
				case TypeCode.Int64:
					writer.WriteValue ((long) value);
					break;
				case TypeCode.UInt64:
					writer.WriteValue ((ulong) value);
					break;
				case TypeCode.Single:
					writer.WriteValue ((float) value);
					break;
				case TypeCode.Double:
					writer.WriteValue ((double) value);
					break;
				case TypeCode.DateTime:
					writer.WriteValue ((DateTime) value);
					break;
				case TypeCode.Decimal:
					writer.WriteValue ((decimal) value);
					break;
				default:
					

					ThrowOnReferenceLoop (writer, value);
					writer.SerializeStack.Push (value);
					try {
						Type genDictType;
						if (value is IDictionary)
							SerializeDictionary (writer, (IDictionary) value);
						else if (value is IDictionary<string, object>)
							SerializeDictionary (writer, (IDictionary<string, object>) value);
						else if ((genDictType = ReflectionUtils.GetGenericDictionary (valueType)) != null)
							SerializeDictionary (writer, new GenericDictionaryLazyDictionary (value, genDictType));
						else if (value is IEnumerable) {
							SerializeEnumerable (writer, (IEnumerable) value);
						}
						else {
							TypeConverter converter = TypeDescriptor.GetConverter (valueType);

							// use the objectType's TypeConverter if it has one and can convert to a string
							if (converter != null) {
								if (!(converter is ComponentConverter) && converter.GetType () != typeof (TypeConverter)) {
									if (converter.CanConvertTo (typeof (string))) {
										writer.WriteValue (converter.ConvertToInvariantString (value));
										return;
									}
								}
							}

							SerializeDictionary (writer, new SerializerLazyDictionary (value));
						}
					}
					finally {

						object x = writer.SerializeStack.Pop ();
						if (x != value)
							throw new InvalidOperationException ("Serialization stack is corrupted");
					}

					break;
				}
			}
		}

		private void ThrowOnReferenceLoop (JsonWriter writer, object value)
		{
			switch (_referenceLoopHandling) {
			case ReferenceLoopHandling.Error:
				if (writer.SerializeStack.Contains (value))
					throw new JsonSerializationException ("Self referencing loop");
				break;
			case ReferenceLoopHandling.Ignore:
				// return from method
				return;
			case ReferenceLoopHandling.Serialize:
				// continue
				break;
			default:
				throw new InvalidOperationException (string.Format ("Unexpected ReferenceLoopHandling value: '{0}'", _referenceLoopHandling));
			}
		}

		private void SerializeEnumerable (JsonWriter writer, IEnumerable values) {
			writer.WriteStartArray ();

			foreach (object value in values)
				SerializeValue (writer, value);

			writer.WriteEndArray ();
		}

		private void SerializeDictionary(JsonWriter writer, IDictionary values)
		{
			writer.WriteStartObject();

			foreach (DictionaryEntry entry in values)
				SerializePair (writer, entry.Key.ToString (), entry.Value);

			writer.WriteEndObject();
		}

		private void SerializeDictionary (JsonWriter writer, IDictionary<string, object> values) {
			writer.WriteStartObject ();

			foreach (KeyValuePair<string, object> entry in values)
				SerializePair (writer, entry.Key, entry.Value);

			writer.WriteEndObject ();
		}

		private void SerializePair (JsonWriter writer, string key, object value) {
			writer.WritePropertyName (key);
			SerializeValue (writer, value);
		}

		#endregion
	}
}
