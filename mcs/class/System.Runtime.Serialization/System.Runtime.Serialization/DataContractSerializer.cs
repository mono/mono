//
// DataContractSerializer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public sealed class DataContractSerializer : XmlObjectSerializer
	{
		const string xmlns = "http://www.w3.org/2000/xmlns/";

		Type type;
		bool ignore_ext, preserve_refs;

		// This is only for compatible mode.
		StreamingContext context;
		ReadOnlyCollection<Type> returned_known_types;
		KnownTypeCollection known_types;
		IDataContractSurrogate surrogate;
		DataContractResolver resolver, default_resolver;

		int max_items = 0x10000; // FIXME: could be from config.

		bool names_filled;
		XmlDictionaryString root_name, root_ns;

		public DataContractSerializer (Type type)
			: this (type, Type.EmptyTypes)
		{
			// nothing to do here.
		}

		public DataContractSerializer (Type type,
			IEnumerable<Type> knownTypes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			this.type = type;
			PopulateTypes (knownTypes);
			known_types.Add (type);
			QName qname = known_types.GetQName (type);

			FillDictionaryString (qname.Name, qname.Namespace);
			
		}

		public DataContractSerializer (Type type, string rootName,
			string rootNamespace)
			: this (type, rootName, rootNamespace, Type.EmptyTypes)
		{
			// nothing to do here.
		}

		public DataContractSerializer (Type type,
			XmlDictionaryString rootName,
			XmlDictionaryString rootNamespace)
			: this (type, rootName, rootNamespace, Type.EmptyTypes)
		{
			// nothing to do here.
		}

		public DataContractSerializer (Type type, string rootName,
			string rootNamespace, IEnumerable<Type> knownTypes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (rootName == null)
				throw new ArgumentNullException ("rootName");
			if (rootNamespace == null)
				throw new ArgumentNullException ("rootNamespace");
			this.type = type;
			PopulateTypes (knownTypes);
			FillDictionaryString (rootName, rootNamespace);
		}

		public DataContractSerializer (Type type,
			XmlDictionaryString rootName,
			XmlDictionaryString rootNamespace,
			IEnumerable<Type> knownTypes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (rootName == null)
				throw new ArgumentNullException ("rootName");
			if (rootNamespace == null)
				throw new ArgumentNullException ("rootNamespace");
			this.type = type;
			PopulateTypes (knownTypes);
			root_name = rootName;
			root_ns = rootNamespace;
		}

		public DataContractSerializer (Type type,
			IEnumerable<Type> knownTypes,
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate)
			: this (type, knownTypes)
		{
			Initialize (maxObjectsInGraph,
				ignoreExtensionDataObject,
				preserveObjectReferences,
				dataContractSurrogate);
		}

		public DataContractSerializer (Type type,
			string rootName,
			string rootNamespace,
			IEnumerable<Type> knownTypes,
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate)
			: this (type, rootName, rootNamespace, knownTypes)
		{
			Initialize (maxObjectsInGraph,
				ignoreExtensionDataObject,
				preserveObjectReferences,
				dataContractSurrogate);
		}

		public DataContractSerializer (Type type,
			XmlDictionaryString rootName,
			XmlDictionaryString rootNamespace,
			IEnumerable<Type> knownTypes,
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate)
			: this (type, rootName, rootNamespace, knownTypes)
		{
			Initialize (maxObjectsInGraph,
				ignoreExtensionDataObject,
				preserveObjectReferences,
				dataContractSurrogate);
		}

#if NET_4_0
		public DataContractSerializer (Type type,
			IEnumerable<Type> knownTypes,
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate,
			DataContractResolver dataContractResolver)
			: this (type, knownTypes, maxObjectsInGraph, ignoreExtensionDataObject, preserveObjectReferences, dataContractSurrogate)
		{
			DataContractResolver = dataContractResolver;
		}

		public DataContractSerializer (Type type,
			string rootName,
			string rootNamespace,
			IEnumerable<Type> knownTypes,
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate,
			DataContractResolver dataContractResolver)
			: this (type, rootName, rootNamespace, knownTypes, maxObjectsInGraph, ignoreExtensionDataObject, preserveObjectReferences, dataContractSurrogate)
		{
			DataContractResolver = dataContractResolver;
		}

		public DataContractSerializer (Type type,
			XmlDictionaryString rootName,
			XmlDictionaryString rootNamespace,
			IEnumerable<Type> knownTypes,
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate,
			DataContractResolver dataContractResolver)
			: this (type, rootName, rootNamespace, knownTypes, maxObjectsInGraph, ignoreExtensionDataObject, preserveObjectReferences, dataContractSurrogate)
		{
			DataContractResolver = dataContractResolver;
		}
#endif

		void PopulateTypes (IEnumerable<Type> knownTypes)
		{
			if (known_types == null)
				known_types= new KnownTypeCollection ();

			if (knownTypes != null) {
				foreach (Type t in knownTypes)
					known_types.Add (t);
			}

			Type elementType = type;
			if (type.HasElementType)
				elementType = type.GetElementType ();

			/* Get all KnownTypeAttribute-s, including inherited ones */
			object [] attrs = elementType.GetCustomAttributes (typeof (KnownTypeAttribute), true);
			for (int i = 0; i < attrs.Length; i ++) {
				KnownTypeAttribute kt = (KnownTypeAttribute) attrs [i];
				known_types.Add (kt.Type);
			}
		}

		void FillDictionaryString (string name, string ns)
		{
			XmlDictionary d = new XmlDictionary ();
			root_name = d.Add (name);
			root_ns = d.Add (ns);
			names_filled = true;
		}

		void Initialize (
			int maxObjectsInGraph,
			bool ignoreExtensionDataObject,
			bool preserveObjectReferences,
			IDataContractSurrogate dataContractSurrogate)
		{
			if (maxObjectsInGraph < 0)
				throw new ArgumentOutOfRangeException ("maxObjectsInGraph must not be negative.");
			max_items = maxObjectsInGraph;
			ignore_ext = ignoreExtensionDataObject;
			preserve_refs = preserveObjectReferences;
			surrogate = dataContractSurrogate;
		}

#if NET_4_0
		public
#else
		internal
#endif
		DataContractResolver DataContractResolver {
			get { return resolver; }
			private set {
				resolver = value;
				default_resolver = default_resolver ?? new DefaultDataContractResolver (this);
			}
		}

		public bool IgnoreExtensionDataObject {
			get { return ignore_ext; }
		}

		public ReadOnlyCollection<Type> KnownTypes {
			get {
				if (returned_known_types == null)
					returned_known_types = new ReadOnlyCollection<Type> (known_types);
				return returned_known_types;
			}
		}

		internal KnownTypeCollection InternalKnownTypes {
			get { return known_types; }
		}

		public IDataContractSurrogate DataContractSurrogate {
			get { return surrogate; }
		}

		public int MaxItemsInObjectGraph {
			get { return max_items; }
		}

		public bool PreserveObjectReferences {
			get { return preserve_refs; }
		}

		public override bool IsStartObject (XmlDictionaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.IsStartElement (root_name, root_ns);
		}

		// SP1
		public override bool IsStartObject (XmlReader reader)
		{
			return IsStartObject (XmlDictionaryReader.CreateDictionaryReader (reader));
		}

		// SP1
		public override object ReadObject (XmlReader reader)
		{
			return ReadObject (XmlDictionaryReader.CreateDictionaryReader (reader));
		}

		public override object ReadObject (XmlReader reader, bool verifyObjectName)
		{
			return ReadObject (XmlDictionaryReader.CreateDictionaryReader (reader), verifyObjectName);
		}

		public override object ReadObject (XmlDictionaryReader reader, bool verifyObjectName)
		{
			int startTypeCount = known_types.Count;
			known_types.Add (type);

			bool isEmpty = reader.IsEmptyElement;

			object ret = XmlFormatterDeserializer.Deserialize (reader, type,
				known_types, surrogate, DataContractResolver, default_resolver, root_name.Value, root_ns.Value, verifyObjectName);

			// remove temporarily-added known types for
			// rootType and object graph type.
			while (known_types.Count > startTypeCount)
				known_types.RemoveAt (startTypeCount);

			return ret;
		}

#if NET_4_0
		public object ReadObject (XmlDictionaryReader reader, bool verifyObjectName, DataContractResolver resolver)
		{
			var bak = DataContractResolver;
			try {
				DataContractResolver = resolver;
				return ReadObject (reader, verifyObjectName);
			} finally {
				DataContractResolver = bak;
			}
		}
#endif

		private void ReadRootStartElement (XmlReader reader, Type type)
 		{
			SerializationMap map =
				known_types.FindUserMap (type);
			QName name = map != null ? map.XmlName :
				KnownTypeCollection.GetPredefinedTypeName (type);
			reader.MoveToContent ();
			reader.ReadStartElement (name.Name, name.Namespace);
			// FIXME: could there be any attributes to handle here?
			reader.Read ();
		}

		// SP1
		public override void WriteObject (XmlWriter writer, object graph)
		{
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (writer);
			WriteObject (w, graph);
		}

#if NET_4_0
		public void WriteObject (XmlDictionaryWriter writer, object graph, DataContractResolver resolver)
		{
			var bak = DataContractResolver;
			try {
				DataContractResolver = resolver;
				WriteObject (writer, graph);
			} finally {
				DataContractResolver = bak;
			}
		}
#endif

		[MonoTODO ("use DataContractSurrogate")]
		/*
			when writeContentOnly is true, then the input XmlWriter
			must be at element state. This is to write possible
			xsi:nil.

			rootType determines the top-level element QName (thus
			it is ignored when writeContentOnly is true).

			preserveObjectReferences indicates that whether the
			output should contain ms:Id or not.
			(http://schemas.microsoft.com/2003/10/Serialization/)
		*/
		public override void WriteObjectContent (XmlDictionaryWriter writer, object graph)
		{
			if (graph == null)
				return;

			int startTypeCount = known_types.Count;

			XmlFormatterSerializer.Serialize (writer, graph,
				type, known_types,
				ignore_ext, max_items, root_ns.Value, preserve_refs, DataContractResolver, default_resolver);

			// remove temporarily-added known types for
			// rootType and object graph type.
			while (known_types.Count > startTypeCount)
				known_types.RemoveAt (startTypeCount);
		}

		public override void WriteObjectContent (XmlWriter writer, object graph)
		{
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (writer);
			WriteObjectContent (w, graph);
		}

		// SP1
		public override void WriteStartObject (
			XmlWriter writer, object graph)
		{
			WriteStartObject (XmlDictionaryWriter.CreateDictionaryWriter (writer), graph);
		}

		public override void WriteStartObject (
			XmlDictionaryWriter writer, object graph)
		{
			Type rootType = type;
			
			if (root_name.Value == "")
				throw new InvalidDataContractException ("Type '" + type.ToString () +
					"' cannot have a DataContract attribute Name set to null or empty string.");


			if (graph == null) {
				if (names_filled)
					writer.WriteStartElement (root_name.Value, root_ns.Value);
				else
					writer.WriteStartElement (root_name, root_ns);
				writer.WriteAttributeString ("i", "nil", XmlSchema.InstanceNamespace, "true");
				return;
			}

			QName rootQName = null;
			XmlDictionaryString name, ns;
			if (DataContractResolver != null && DataContractResolver.TryResolveType (graph.GetType (), type, default_resolver, out name, out ns))
				rootQName = new QName (name.Value, ns.Value);

			// It is error unless 1) TypeResolver resolved the type name, 2) the object is the exact type, 3) the object is known or 4) the type is primitive.

			if (rootQName == null &&
			    graph.GetType () != type &&
			    !known_types.Contains (graph.GetType ()) &&
			    KnownTypeCollection.GetPrimitiveTypeName (graph.GetType ()) == QName.Empty)
				throw new SerializationException (String.Format ("Type '{0}' is unexpected. The type should either be registered as a known type, or DataContractResolver should be used.", graph.GetType ()));

			QName instName = rootQName;
			rootQName = rootQName ?? known_types.GetQName (rootType);
			QName graph_qname = known_types.GetQName (graph.GetType ());

			known_types.Add (graph.GetType ());

			if (names_filled)
				writer.WriteStartElement (root_name.Value, root_ns.Value);
			else
				writer.WriteStartElement (root_name, root_ns);

			if (rootQName != graph_qname || rootQName.Namespace != KnownTypeCollection.MSSimpleNamespace && !rootType.IsEnum)
				//FIXME: Hack, when should the "i:type" be written?
				//Not used in case of enums
				writer.WriteXmlnsAttribute ("i", XmlSchema.InstanceNamespace);

			if (root_ns.Value != rootQName.Namespace)
				if (rootQName.Namespace != KnownTypeCollection.MSSimpleNamespace)
					writer.WriteXmlnsAttribute (null, rootQName.Namespace);

			if (rootQName == graph_qname)
				return;

			/* Different names */
			known_types.Add (rootType);
			
			instName = instName ?? KnownTypeCollection.GetPredefinedTypeName (graph.GetType ());
			if (instName == QName.Empty)
				/* Not a primitive type */
				instName = graph_qname;
			else
				/* FIXME: Hack, .. see test WriteObject7 () */
				instName = new QName (instName.Name, XmlSchema.Namespace);

/* // disabled as it now generates extraneous i:type output.
			// output xsi:type as rootType is not equivalent to the graph's type.
			writer.WriteStartAttribute ("i", "type", XmlSchema.InstanceNamespace);
			writer.WriteQualifiedName (instName.Name, instName.Namespace);
			writer.WriteEndAttribute ();
*/
		}

		public override void WriteEndObject (XmlDictionaryWriter writer)
		{
			writer.WriteEndElement ();
		}

		// SP1
		public override void WriteEndObject (XmlWriter writer)
		{
			WriteEndObject (XmlDictionaryWriter.CreateDictionaryWriter (writer));
		}
	}
}
#endif
