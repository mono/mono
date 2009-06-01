//
// XmlFormatterSerializer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	internal class XmlFormatterSerializer
	{
		XmlDictionaryWriter writer;
		object graph;
		KnownTypeCollection types;
		
		bool save_id;
		bool ignore_unknown;
		IDataContractSurrogate surrogate;
		int max_items;

		ArrayList objects = new ArrayList ();
		Hashtable references = new Hashtable (); // preserve possibly referenced objects to ids. (new in 3.5 SP1)

		public static void Serialize (XmlDictionaryWriter writer, object graph,
			KnownTypeCollection types,
			bool ignoreUnknown, int maxItems, string root_ns)
		{
			new XmlFormatterSerializer (writer, types, ignoreUnknown, maxItems, root_ns)
				.Serialize (graph != null ? graph.GetType () : null, graph);
		}

		public XmlFormatterSerializer (XmlDictionaryWriter writer,
			KnownTypeCollection types,
			bool ignoreUnknown, int maxItems, string root_ns)
		{
			this.writer = writer;
			this.types = types;
			ignore_unknown = ignoreUnknown;
			max_items = maxItems;
		}

		public ArrayList SerializingObjects {
			get { return objects; }
		}

		public IDictionary References {
			get { return references; }
		}

		public XmlDictionaryWriter Writer {
			get { return writer; }
		}

		public void Serialize (Type type, object graph)
		{
			if (graph == null)
				writer.WriteAttributeString ("nil", XmlSchema.InstanceNamespace, "true");
			else {
				Type actualType = graph.GetType ();

				SerializationMap map = types.FindUserMap (actualType);
				// For some collection types, the actual type does not matter. So get nominal serialization type instead.
				// (The code below also covers the lines above, but I don't remove above lines to avoid extra search cost.)
				if (map == null) {
					actualType = types.GetSerializedType (actualType);
					map = types.FindUserMap (actualType);
				}
				// If it is still unknown, then register it.
				if (map == null) {
					types.Add (actualType);
					map = types.FindUserMap (actualType);
				}

				if (actualType != type && (map == null || map.OutputXsiType)) {
					QName qname = types.GetXmlName (actualType);
					string name = qname.Name;
					string ns = qname.Namespace;
					if (qname == QName.Empty) {
						name = XmlConvert.EncodeLocalName (actualType.Name);
						ns = KnownTypeCollection.DefaultClrNamespaceBase + actualType.Namespace;
					} else if (qname.Namespace == KnownTypeCollection.MSSimpleNamespace)
						ns = XmlSchema.Namespace;
					if (writer.LookupPrefix (ns) == null) // it goes first (extraneous, but it makes att order compatible)
						writer.WriteXmlnsAttribute (null, ns);
					writer.WriteStartAttribute ("type", XmlSchema.InstanceNamespace);
					writer.WriteQualifiedName (name, ns);
					writer.WriteEndAttribute ();
				}
				QName predef = KnownTypeCollection.GetPredefinedTypeName (actualType);
				if (predef != QName.Empty)
					SerializePrimitive (type, graph, predef);
				else
					map.Serialize (graph, this);
			}
		}

		public void SerializePrimitive (Type type, object graph, QName qname)
		{
//			writer.WriteStartAttribute ("type", XmlSchema.InstanceNamespace);
//			writer.WriteQualifiedName (qname.Name, qname.Namespace);
//			writer.WriteEndAttribute ();
			writer.WriteString (KnownTypeCollection.PredefinedTypeObjectToString (graph));
		}

		public void WriteStartElement (string rootName, string rootNamespace, string currentNamespace)
		{
			writer.WriteStartElement (rootName, rootNamespace);
			if (!string.IsNullOrEmpty (currentNamespace) && currentNamespace != rootNamespace)
				writer.WriteXmlnsAttribute (null, currentNamespace);
		}

		public void WriteEndElement ()
		{
			writer.WriteEndElement ();
		}
	}
}
#endif
