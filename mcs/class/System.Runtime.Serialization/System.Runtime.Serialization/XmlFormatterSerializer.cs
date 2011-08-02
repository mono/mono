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
using System.Collections.Generic;
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
		DataContractResolver resolver, default_resolver; // new in 4.0
		int max_items;

		List<object> objects = new List<object> ();
		Dictionary<object,string> references = new Dictionary<object,string> (); // preserve possibly referenced objects to ids. (new in 3.5 SP1)

		public static void Serialize (XmlDictionaryWriter writer, object graph, Type declaredType, KnownTypeCollection types,
			bool ignoreUnknown, int maxItems, string root_ns, bool preserveObjectReferences, DataContractResolver resolver, DataContractResolver defaultResolver)
		{
			new XmlFormatterSerializer (writer, types, ignoreUnknown, maxItems, root_ns, preserveObjectReferences, resolver, defaultResolver)
				.Serialize (/*graph != null ? graph.GetType () : */declaredType, graph); // FIXME: I believe it should always use declaredType, but such a change brings some test breakages.
		}

		public XmlFormatterSerializer (XmlDictionaryWriter writer, KnownTypeCollection types, bool ignoreUnknown,
					       int maxItems, string root_ns, bool preserveObjectReferences,
					       DataContractResolver resolver, DataContractResolver defaultResolver)
		{
			this.writer = writer;
			this.types = types;
			ignore_unknown = ignoreUnknown;
			max_items = maxItems;
			PreserveObjectReferences = preserveObjectReferences;
			this.resolver = resolver;
			this.default_resolver = defaultResolver;
		}

		public bool PreserveObjectReferences { get; private set; }

		public List<object> SerializingObjects {
			get { return objects; }
		}

		public XmlDictionaryWriter Writer {
			get { return writer; }
		}

		public void Serialize (Type type, object graph)
		{
			if (graph == null)
				writer.WriteAttributeString ("nil", XmlSchema.InstanceNamespace, "true");
#if !MOONLIGHT
			else if (type == typeof (XmlElement))
				((XmlElement) graph).WriteTo (Writer);
			else if (type == typeof (XmlNode [])) {
				foreach (var xn in (XmlNode []) graph)
					xn.WriteTo (Writer);
			}
#endif
			else {
				QName resolvedQName = null;
				if (resolver != null) {
					XmlDictionaryString rname, rns;
					if (resolver.TryResolveType (graph != null ? graph.GetType () : typeof (object), type, default_resolver, out rname, out rns))
						resolvedQName = new QName (rname.Value, rns.Value);
				}

				Type actualType = graph.GetType ();

				SerializationMap map;
				map = types.FindUserMap (actualType);
				// For some collection types, the actual type does not matter. So get nominal serialization type instead.
				// (The code below also covers the lines above, but I don't remove above lines to avoid extra search cost.)
				if (map == null) {
					// FIXME: not sure if type.IsInterface is the correct condition to determine whether items are serialized with i:type or not. (e.g. bug #675144 server response).
					actualType = types.GetSerializedType (type.IsInterface ? type : actualType);
					map = types.FindUserMap (actualType);
				}
				// If it is still unknown, then register it.
				if (map == null) {
					types.Add (actualType);
					map = types.FindUserMap (actualType);
				}

				if (actualType != type && (map == null || map.OutputXsiType)) {
					QName qname = resolvedQName ?? types.GetXmlName (actualType);
					string name = qname.Name;
					string ns = qname.Namespace;
					if (qname == QName.Empty) {
						name = XmlConvert.EncodeLocalName (actualType.Name);
						ns = KnownTypeCollection.DefaultClrNamespaceBase + actualType.Namespace;
					} else if (qname.Namespace == KnownTypeCollection.MSSimpleNamespace)
						ns = XmlSchema.Namespace;
					if (writer.LookupPrefix (ns) == null) // it goes first (extraneous, but it makes att order compatible)
						writer.WriteXmlnsAttribute (null, ns);
					writer.WriteStartAttribute ("i", "type", XmlSchema.InstanceNamespace);
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
			string label;
			if (TrySerializeAsReference (false, graph, out label))
				return;
			if (label != null)
				Writer.WriteAttributeString ("z", "Id", KnownTypeCollection.MSSimpleNamespace, label);

//			writer.WriteStartAttribute ("type", XmlSchema.InstanceNamespace);
//			writer.WriteQualifiedName (qname.Name, qname.Namespace);
//			writer.WriteEndAttribute ();

			// It is the only exceptional type that does not serialize to string but serializes into complex element.
			if (type == typeof (DateTimeOffset)) {
				var v = (DateTimeOffset) graph;
				writer.WriteStartElement ("DateTime", KnownTypeCollection.DefaultClrNamespaceSystem);
				SerializePrimitive (typeof (DateTime), DateTime.SpecifyKind (v.DateTime.Subtract (v.Offset), DateTimeKind.Utc), KnownTypeCollection.GetPredefinedTypeName (typeof (DateTime)));
				writer.WriteEndElement ();
				writer.WriteStartElement ("OffsetMinutes", KnownTypeCollection.DefaultClrNamespaceSystem);
				SerializePrimitive (typeof (int), v.Offset.TotalMinutes, KnownTypeCollection.GetPredefinedTypeName (typeof (int)));
				writer.WriteEndElement ();
			}
			else
				writer.WriteString (KnownTypeCollection.PredefinedTypeObjectToString (graph));
		}

		public void WriteStartElement (string memberName, string memberNamespace, string contentNamespace)
		{
			writer.WriteStartElement (memberName, memberNamespace);
			if (!string.IsNullOrEmpty (contentNamespace) && contentNamespace != memberNamespace)
				writer.WriteXmlnsAttribute (null, contentNamespace);
		}

		public void WriteEndElement ()
		{
			writer.WriteEndElement ();
		}

		// returned bool: whether z:Ref is written or not.
		// out label: object label either in use or newly allocated.
		public bool TrySerializeAsReference (bool isMapReference, object graph, out string label)
		{
			label = null;
			if (!isMapReference && (!PreserveObjectReferences || graph == null || graph.GetType ().IsValueType))
				return false;

			if (references.TryGetValue (graph, out label)) {
				Writer.WriteAttributeString ("z", "Ref", KnownTypeCollection.MSSimpleNamespace, label);
				label = null; // do not write label
				return true;
			}

			label = "i" + (references.Count + 1);
			references.Add (graph, label);

			return false;
		}
	}
}
#endif
