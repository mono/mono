//
// JsonSerializationWriter.cs
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
	class JsonSerializationWriter
	{
		DataContractJsonSerializer serializer;
		XmlWriter writer;
		int serialized_object_count;
		bool always_emit_type;
		Dictionary<Type, TypeMap> typemaps = new Dictionary<Type, TypeMap> ();
		Type root_type;

		public JsonSerializationWriter (DataContractJsonSerializer serializer, XmlWriter writer, Type rootType, bool alwaysEmitTypeInformation)
		{
			this.serializer = serializer;
			this.writer = writer;
			this.root_type = rootType;
			this.always_emit_type = alwaysEmitTypeInformation;
		}

		public XmlWriter Writer {
			get { return writer; }
		}

		public void WriteObjectContent (object graph, bool top, bool outputTypeName)
		{
			if (graph == null) {
				if (top)
					GetTypeMap (root_type); // to make sure to reject invalid contracts
				writer.WriteString (null);
				return;
			}

			if (serialized_object_count ++ == serializer.MaxItemsInObjectGraph)
				throw new SerializationException (String.Format ("The object graph exceeded the maximum object count '{0}' specified in the serializer", serializer.MaxItemsInObjectGraph));

			switch (Type.GetTypeCode (graph.GetType ())) {
			case TypeCode.Char:
			case TypeCode.String:
				writer.WriteString (graph.ToString ());
				break;
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				writer.WriteAttributeString ("type", "number");
				writer.WriteString (((IFormattable) graph).ToString ("R", CultureInfo.InvariantCulture));
				break;
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				writer.WriteAttributeString ("type", "number");
				if (graph.GetType ().IsEnum)
					graph = ((IConvertible) graph).ToType (Enum.GetUnderlyingType (graph.GetType ()), CultureInfo.InvariantCulture);
				writer.WriteString (((IFormattable) graph).ToString ("G", CultureInfo.InvariantCulture));
				break;
			case TypeCode.Boolean:
				writer.WriteAttributeString ("type", "boolean");
				if ((bool) graph)
					writer.WriteString ("true");
				else
					writer.WriteString ("false");
				break;
			case TypeCode.DateTime:
				writer.WriteString (String.Format (CultureInfo.InvariantCulture, "/Date({0})/", (long) ((DateTime) graph).Subtract (new DateTime (1970, 1, 1)).TotalMilliseconds));
				break;
			default:
				if (graph is Guid) {
					goto case TypeCode.String;
				} else if (graph is Uri) {
					goto case TypeCode.String;
				} else if (graph is XmlQualifiedName) {
					XmlQualifiedName qn = (XmlQualifiedName) graph;
					writer.WriteString (qn.Name);
					writer.WriteString (":");
					writer.WriteString (qn.Namespace);
				} else if (graph is IDictionary) {
					writer.WriteAttributeString ("type", "array");
					IDictionary dic = (IDictionary) graph;
					foreach (object o in dic.Keys) {
						writer.WriteStartElement ("item");
						writer.WriteAttributeString ("type", "object");
						// outputting a KeyValuePair as <Key .. /><Value ... />
						writer.WriteStartElement ("Key");
						WriteObjectContent (o, false, !(graph is Array && graph.GetType ().GetElementType () != typeof (object)));
						writer.WriteEndElement ();
						writer.WriteStartElement ("Value");
						WriteObjectContent (dic[o], false, !(graph is Array && graph.GetType ().GetElementType () != typeof (object)));
						writer.WriteEndElement ();
						writer.WriteEndElement ();
					}
				} else if (graph is ICollection) { // array
					writer.WriteAttributeString ("type", "array");
					foreach (object o in (ICollection) graph) {
						writer.WriteStartElement ("item");
						// when it is typed, then no need to output "__type"
						WriteObjectContent (o, false, !(graph is Array && graph.GetType ().GetElementType () != typeof (object)));
						writer.WriteEndElement ();
					}
				} else { // object
					TypeMap tm = GetTypeMap (graph.GetType ());
					if (tm != null) {
						// FIXME: I'm not sure how it is determined whether __type is written or not...
						if (outputTypeName || always_emit_type)
							writer.WriteAttributeString ("__type", FormatTypeName (graph.GetType ()));
						writer.WriteAttributeString ("type", "object");
						tm.Serialize (this, graph);
					}
					else
						// it does not emit type="object" (as the graph is regarded as a string)
//						writer.WriteString (graph.ToString ());
throw new InvalidDataContractException (String.Format ("Type {0} cannot be serialized by this JSON serializer", graph.GetType ()));
				}
				break;
			}
		}

		string FormatTypeName (Type type)
		{
			return String.Format ("{0}:#{1}", type.Name, type.Namespace);
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
	}
}
