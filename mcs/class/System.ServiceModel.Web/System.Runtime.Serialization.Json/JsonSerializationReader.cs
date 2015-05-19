//
// JsonSerializationReader.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace System.Runtime.Serialization.Json
{
	class JsonSerializationReader
	{
		DataContractJsonSerializer serializer;
		XmlReader reader;
		int serialized_object_count;
		bool verify_object_name;
		Dictionary<Type, TypeMap> typemaps = new Dictionary<Type, TypeMap> ();
		Type root_type;

		public JsonSerializationReader (DataContractJsonSerializer serializer, XmlReader reader, Type rootType, bool verifyObjectName)
		{
			this.serializer = serializer;
			this.reader = reader;
			this.root_type = rootType;
			this.verify_object_name = verifyObjectName;
		}

		public XmlReader Reader {
			get { return reader; }
		}

		public object ReadRoot ()
		{
			TypeMap rootMap = GetTypeMap (root_type);

			object v = ReadObject (root_type);
			return v;
		}

		public object ReadObject (Type type)
		{
			return ReadObject (type, null);
		}
		
		public object ReadObject (Type type, object instance)
		{
			if (serialized_object_count ++ == serializer.MaxItemsInObjectGraph)
				throw SerializationError (String.Format ("The object graph exceeded the maximum object count '{0}' specified in the serializer", serializer.MaxItemsInObjectGraph));

			bool nullable = false;
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>)) {
				nullable = true;
				type = Nullable.GetUnderlyingType (type);
			}

			bool isNull = reader.GetAttribute ("type") == "null";

			switch (Type.GetTypeCode (type)) {
			case TypeCode.DBNull:
				string dbn = reader.ReadElementContentAsString ();
				if (dbn != String.Empty)
					throw new SerializationException (String.Format ("The only expected DBNull value string is '{{}}'. Tha actual input was '{0}'.", dbn));
				return DBNull.Value;
			case TypeCode.String:
				if (isNull) {
					reader.ReadElementContentAsString ();
					return null;
				}
				else
					return reader.ReadElementContentAsString ();
			case TypeCode.Char:
				var c = reader.ReadElementContentAsString ();
				if (c.Length > 1)
					throw new XmlException ("Invalid JSON char");
				return Char.Parse(c);
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
					return ReadValueType (type, nullable);
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.Int64:
				if (type.IsEnum)
					return Enum.ToObject (type, Convert.ChangeType (reader.ReadElementContentAsLong (), Enum.GetUnderlyingType (type), null));
				else
					return ReadValueType (type, nullable);
			case TypeCode.UInt64:
				if (type.IsEnum)
					return Enum.ToObject (type, Convert.ChangeType (reader.ReadElementContentAsDecimal (), Enum.GetUnderlyingType (type), null));
				else
					return ReadValueType (type, nullable);
			case TypeCode.Boolean:
				return ReadValueType (type, nullable);
			case TypeCode.DateTime:
				// it does not use ReadElementContentAsDateTime(). Different string format.
				var s = reader.ReadElementContentAsString ();
				if (s.Length < 2 || !s.StartsWith ("/Date(", StringComparison.Ordinal) || !s.EndsWith (")/", StringComparison.Ordinal)) {
					if (nullable)
						return null;
					throw new XmlException ("Invalid JSON DateTime format. The value format should be '/Date(UnixTime)/'");
				}

				// The date can contain [SIGN]LONG, [SIGN]LONG+HOURSMINUTES or [SIGN]LONG-HOURSMINUTES
				// the format for HOURSMINUTES is DDDD
				int tidx = s.IndexOf ('-', 8);
				if (tidx == -1)
					tidx = s.IndexOf ('+', 8);
				int minutes = 0;
				if (tidx == -1){
					s = s.Substring (6, s.Length - 8);
				} else {
					int offset;
					int.TryParse (s.Substring (tidx+1, s.Length-3-tidx), out offset);

					minutes = (offset % 100) + (offset / 100) * 60;
					if (s [tidx] == '-')
						minutes = -minutes;

					s = s.Substring (6, tidx-6);
				}
				var date = new DateTime (1970, 1, 1).AddMilliseconds (long.Parse (s));
				if (minutes != 0)
					date = date.AddMinutes (minutes);
				return date;
			default:
				if (type == typeof (Guid)) {
					return new Guid (reader.ReadElementContentAsString ());
				} else if (type == typeof (Uri)) {
					if (isNull) {
						reader.ReadElementContentAsString ();
						return null;
					}
					else
						return new Uri (reader.ReadElementContentAsString (), UriKind.RelativeOrAbsolute);
				} else if (type == typeof (XmlQualifiedName)) {
					s = reader.ReadElementContentAsString ();
					int idx = s.IndexOf (':');
					return idx < 0 ? new XmlQualifiedName (s) : new XmlQualifiedName (s.Substring (0, idx), s.Substring (idx + 1));
				} else if (type != typeof (object)) {
					// strongly-typed object
					if (reader.IsEmptyElement) {
						// empty -> null array or object
						reader.Read ();
						return null;
					}

					Type ct = GetCollectionElementType (type);
					if (ct != null) {
						return DeserializeGenericCollection (type, ct, instance);
					} else {
						string typeHint = reader.GetAttribute ("__type");
						if (typeHint != null) {
							// this might be a derived & known type. We allow it when it's both.
							Type exactType = GetRuntimeType (typeHint, type);
							if (exactType == null)
								throw SerializationError (String.Format ("Cannot load type '{0}'", typeHint));
							 TypeMap map = GetTypeMap (exactType);
							 return map.Deserialize (this, instance);
						} else { // no type hint
							TypeMap map = GetTypeMap (type);
							 return map.Deserialize (this, instance);
						}
					}
				}
				else
					return ReadInstanceDrivenObject ();
			}
		}
		
		object ReadValueType (Type type, bool nullable)
		{
			string s = reader.ReadElementContentAsString ();
			return nullable && s.Trim ().Length == 0 ? null : Convert.ChangeType (s, type, CultureInfo.InvariantCulture);
		}
		

		Type GetRuntimeType (string name, Type baseType)
		{
			string properName = ToRuntimeTypeName (name);

			if (baseType != null && baseType.FullName.Equals (properName))
				return baseType;

			if (serializer.KnownTypes != null)
				foreach (Type t in serializer.KnownTypes)
					if (t.FullName.Equals (properName)) 
						return t;

			if (baseType != null)
				foreach (var attr in baseType.GetCustomAttributes (typeof (KnownTypeAttribute), false))
					if ((attr as KnownTypeAttribute).Type.FullName.Equals (properName))
						return (attr as KnownTypeAttribute).Type;

			return null;
		}

		object ReadInstanceDrivenObject ()
		{
			string type = reader.GetAttribute ("type");
			switch (type) {
			case "null":
				reader.Skip ();
				return null;
			case "object":
				string runtimeType = reader.GetAttribute ("__type");
				if (runtimeType != null) {
					Type t = GetRuntimeType (runtimeType, null);
					if (t == null)
						throw SerializationError (String.Format ("Cannot load type '{0}'", runtimeType));
					return ReadObject (t);
				}
				break;
			}
			string v = reader.ReadElementContentAsString ();
			switch (type) {
			case "boolean":
				switch (v) {
				case "true":
					return true;
				case "false":
					return false;
				default:
					throw SerializationError (String.Format ("Invalid JSON boolean value: {0}", v));
				}
			case "string":
				return v;
			case "number":
				int i;
				if (int.TryParse (v, NumberStyles.None, CultureInfo.InvariantCulture, out i))
					return i;
				long l;
				if (long.TryParse (v, NumberStyles.None, CultureInfo.InvariantCulture, out l))
					return l;
				ulong ul;
				if (ulong.TryParse (v, NumberStyles.None, CultureInfo.InvariantCulture, out ul))
					return ul;
				double dbl;
				if (double.TryParse (v, NumberStyles.None, CultureInfo.InvariantCulture, out dbl))
					return dbl;
				decimal dec;
				if (decimal.TryParse (v, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dec))
					return dec;
				throw SerializationError (String.Format ("Invalid JSON input: {0}", v));
			default:
				throw SerializationError (String.Format ("Unexpected type: {0}", type));
			}
		}

		string FormatTypeName (Type type)
		{
			return type.Namespace == null ? type.Name : String.Format ("{0}:#{1}", type.Name, type.Namespace);
		}

		string ToRuntimeTypeName (string s)
		{
			int idx = s.IndexOf (":#", StringComparison.Ordinal);
			return idx < 0 ? s : String.Concat (s.Substring (idx + 2), ".", s.Substring (0, idx));
		}

		Type GetCollectionElementType (Type type)
		{
			if (type.IsArray)
				return type.GetElementType ();

			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IEnumerable<>))
				return type.GetGenericArguments () [0];
			var inter = type.GetInterface ("System.Collections.Generic.IEnumerable`1", false);
			if (inter != null)
				return inter.GetGenericArguments () [0];
			
			if (typeof (IEnumerable).IsAssignableFrom (type))
				// return typeof(object) for mere collection.
				return typeof (object);
			else
				return null;
		}

		object DeserializeGenericCollection (Type collectionType, Type elementType, object collectionInstance)
		{
			reader.ReadStartElement ();
			object ret;
			if (collectionType.IsInterface)
				collectionType = typeof (List<>).MakeGenericType (elementType);
			if (TypeMap.IsDictionary (collectionType)) {
				if (collectionInstance == null)
					collectionInstance = Activator.CreateInstance (collectionType);
				
				var keyType = elementType.IsGenericType ? elementType.GetGenericArguments () [0] : typeof (object);
				var valueType = elementType.IsGenericType ? elementType.GetGenericArguments () [1] : typeof (object);
				MethodInfo add = collectionType.GetMethod ("Add", new Type [] { keyType, valueType });

				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!reader.IsStartElement ("item"))
						throw SerializationError (String.Format ("Expected element 'item', but found '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));

					// reading a KeyValuePair in the form of <Key .../><Value .../>
					reader.Read ();
					reader.MoveToContent ();
					object key = ReadObject (keyType);
					reader.MoveToContent ();
					object val = ReadObject (valueType);
					reader.Read ();
					add.Invoke (collectionInstance, new [] { key, val });
				}
				ret = collectionInstance;
			} else if (typeof (IList).IsAssignableFrom (collectionType)) {
#if NET_2_1
				Type listType = collectionType.IsArray ? typeof (List<>).MakeGenericType (elementType) : null;
#else
				Type listType = collectionType.IsArray ? typeof (ArrayList) : null;
#endif
				
				IList c;
				if (collectionInstance == null)
					c = (IList) Activator.CreateInstance (listType ?? collectionType);
				else 
					c = (IList) collectionInstance;
				
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!reader.IsStartElement ("item"))
						throw SerializationError (String.Format ("Expected element 'item', but found '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));
					object elem = ReadObject (elementType);
					c.Add (elem);
				}
#if NET_2_1
				if (collectionType.IsArray) {
					Array array = Array.CreateInstance (elementType, c.Count);
					c.CopyTo (array, 0);
					ret = array;
				}
				else
					ret = c;
#else
				ret = collectionType.IsArray ? ((ArrayList) c).ToArray (elementType) : c;
#endif
			} else {
				if (collectionInstance == null)
					collectionInstance = Activator.CreateInstance (collectionType);
				
				MethodInfo add;
				if (collectionInstance.GetType ().IsGenericType &&
					collectionInstance.GetType ().GetGenericTypeDefinition () == typeof (LinkedList<>))
					add = collectionType.GetMethod ("AddLast", new Type [] { elementType });
				else
					add = collectionType.GetMethod ("Add", new Type [] { elementType });
				
				if (add == null) {
					var icoll = typeof (ICollection<>).MakeGenericType (elementType);
					if (icoll.IsAssignableFrom (collectionInstance.GetType ()))
						add = icoll.GetMethod ("Add");
				}
				if (add == null) 
					throw new MissingMethodException (elementType.FullName, "Add");
				
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!reader.IsStartElement ("item"))
						throw SerializationError (String.Format ("Expected element 'item', but found '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));
					object element = ReadObject (elementType);
					add.Invoke (collectionInstance, new object [] { element });
				}
				ret = collectionInstance;
			}

			reader.ReadEndElement ();
			return ret;
		}

		TypeMap GetTypeMap (Type type)
		{
			TypeMap map;
			if (!typemaps.TryGetValue (type, out map)) {
				map = TypeMap.CreateTypeMap (type);
				typemaps [type] = map;
			}
			return map;
		}

		Exception SerializationError (string basemsg)
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			if (li == null || !li.HasLineInfo ())
				return new SerializationException (basemsg);
			else
				return new SerializationException (String.Format ("{0}. Error at {1} ({2},{3})", basemsg, reader.BaseURI, li.LineNumber, li.LinePosition));
		}
	}
}
