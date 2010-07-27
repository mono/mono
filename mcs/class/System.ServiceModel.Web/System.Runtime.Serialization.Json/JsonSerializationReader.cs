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
			if (serialized_object_count ++ == serializer.MaxItemsInObjectGraph)
				throw SerializationError (String.Format ("The object graph exceeded the maximum object count '{0}' specified in the serializer", serializer.MaxItemsInObjectGraph));

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
				return reader.ReadElementContentAsFloat ();
			case TypeCode.Double:
				return reader.ReadElementContentAsDouble ();
			case TypeCode.Decimal:
				return reader.ReadElementContentAsDecimal ();
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
				int i = reader.ReadElementContentAsInt ();
				if (type.IsEnum)
					return Enum.ToObject (type, (object)i);
				else
					return Convert.ChangeType (i, type, null);
			case TypeCode.Int64:
			case TypeCode.UInt64:
				long l = reader.ReadElementContentAsLong ();
				if (type.IsEnum)
					return Enum.ToObject (type, (object)l);
				else
					return Convert.ChangeType (l, type, null);
			case TypeCode.Boolean:
				return reader.ReadElementContentAsBoolean ();
			case TypeCode.DateTime:
				// it does not use ReadElementContentAsDateTime(). Different string format.
				var s = reader.ReadElementContentAsString ();
				if (s.Length < 2 || !s.StartsWith ("/Date(", StringComparison.Ordinal) || !s.EndsWith (")/", StringComparison.Ordinal))
					throw new XmlException ("Invalid JSON DateTime format. The value format should be '/Date(UnixTime)/'");
				return new DateTime (1970, 1, 1).AddMilliseconds (long.Parse (s.Substring (6, s.Length - 8)));
			default:
				if (type == typeof (Guid)) {
					return new Guid (reader.ReadElementContentAsString ());
				} else if (type == typeof (Uri)) {
					if (isNull) {
						reader.ReadElementContentAsString ();
						return null;
					}
					else
						return new Uri (reader.ReadElementContentAsString ());
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

					Type ct = GetCollectionType (type);
					if (ct != null) {
						return DeserializeGenericCollection (type, ct);
					} else {
						TypeMap map = GetTypeMap (type);
						return map.Deserialize (this);
					}
				}
				else
					return ReadInstanceDrivenObject ();
			}
		}

		Type GetRuntimeType (string name)
		{
			name = ToRuntimeTypeName (name);
			if (serializer.KnownTypes != null)
				foreach (Type t in serializer.KnownTypes)
					if (t.FullName == name)
						return t;
			var ret = root_type.Assembly.GetType (name, false) ?? Type.GetType (name, false);
			if (ret != null)
				return ret;
#if !NET_2_1 // how to do that in ML?
			// We probably have to iterate all the existing
			// assemblies that are loaded in current domain.
			foreach (var ass in AppDomain.CurrentDomain.GetAssemblies ()) {
				ret = ass.GetType (name, false);
				if (ret != null)
					return ret;
			}
#endif
			return null;
		}

		object ReadInstanceDrivenObject ()
		{
			string type = reader.GetAttribute ("type");
			if (type == "object") {
				string runtimeType = reader.GetAttribute ("__type");
				if (runtimeType != null) {
					Type t = GetRuntimeType (runtimeType);
					if (t == null)
						throw SerializationError (String.Format ("Cannot load type '{0}'", runtimeType));
					return ReadObject (t);
				}
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
			case "null":
				if (v != "null")
					throw SerializationError (String.Format ("Invalid JSON null value: {0}", v));
				return null;
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
				if (decimal.TryParse (v, NumberStyles.None, CultureInfo.InvariantCulture, out dec))
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

		Type GetCollectionType (Type type)
		{
			if (type.IsArray)
				return type.GetElementType ();
			if (type.IsGenericType) {
				// returns T for ICollection<T>
				Type [] ifaces = type.GetInterfaces ();
				foreach (Type i in ifaces)
					if (i.IsGenericType && i.GetGenericTypeDefinition ().Equals (typeof (ICollection<>)))
						return i.GetGenericArguments () [0];
			}
			if (typeof (IList).IsAssignableFrom (type))
				// return typeof(object) for mere collection.
				return typeof (object);
			else
				return null;
		}

		object DeserializeGenericCollection (Type collectionType, Type elementType)
		{
			reader.ReadStartElement ();
			object ret;
			if (collectionType.IsInterface)
				collectionType = typeof (List<>).MakeGenericType (elementType);
			if (typeof (IList).IsAssignableFrom (collectionType)) {
#if NET_2_1
				Type listType = collectionType.IsArray ? typeof (List<>).MakeGenericType (elementType) : null;
#else
				Type listType = collectionType.IsArray ? typeof (ArrayList) : null;
#endif
				IList c = (IList) Activator.CreateInstance (listType ?? collectionType);
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!reader.IsStartElement ("item"))
						throw SerializationError (String.Format ("Expected element 'item', but found '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));
					Type et = elementType == typeof (object) || elementType.IsAbstract ? null : elementType;
					object elem = ReadObject (et ?? typeof (object));
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
			} else if (typeof (IDictionary).IsAssignableFrom(collectionType)) {
				IDictionary id = (IDictionary)Activator.CreateInstance (collectionType);

				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!reader.IsStartElement ("item"))
						throw SerializationError (String.Format ("Expected element 'item', but found '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));

					// reading a KeyValuePair in the form of <Key .../><Value .../>
					reader.Read ();
					reader.MoveToContent ();
					object key = ReadObject (elementType.GetGenericArguments ()[0]);
					reader.MoveToContent ();
					object val = ReadObject (elementType.GetGenericArguments ()[1]);
					reader.Read ();
					id[key] = val;
				}
				ret = id;
			} else {
				object c = Activator.CreateInstance (collectionType);
				MethodInfo add = collectionType.GetMethod ("Add", new Type [] {elementType});
				if (add == null) {
					var icoll = typeof (ICollection<>).MakeGenericType (elementType);
					if (icoll.IsAssignableFrom (c.GetType ()))
						add = icoll.GetMethod ("Add");
				}
				
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (!reader.IsStartElement ("item"))
						throw SerializationError (String.Format ("Expected element 'item', but found '{0}' in namespace '{1}'", reader.LocalName, reader.NamespaceURI));
					object elem = ReadObject (elementType);
					add.Invoke (c, new object [] {elem});
				}
				ret = c;
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
