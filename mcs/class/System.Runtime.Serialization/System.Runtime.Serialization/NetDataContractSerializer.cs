//
// NetDataContractSerializer.cs
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
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public sealed class NetDataContractSerializer
		: XmlObjectSerializer, IFormatter
	{
		const string xmlns = "http://www.w3.org/2000/xmlns/";
		const string default_ns = "http://schemas.datacontract.org/2004/07/";

		// This is only for compatible mode.
		StreamingContext context;
//		KnownTypeCollection known_types;
//		IDataContractSurrogate surrogate;

		SerializationBinder binder;
		ISurrogateSelector selector;

		int max_items = 0x10000; // FIXME: could be from config.
		bool ignore_extensions;
		FormatterAssemblyStyle ass_style;

		XmlDictionaryString root_name, root_ns;

		public NetDataContractSerializer ()
		{
		}

		public NetDataContractSerializer (StreamingContext context)
		{
			this.context = context;
		}

		public NetDataContractSerializer (string rootName,
			string rootNamespace)
		{
			FillDictionaryString (rootName, rootNamespace);
		}

		public NetDataContractSerializer (XmlDictionaryString rootName,
			XmlDictionaryString rootNamespace)
		{
			if (rootName == null)
				throw new ArgumentNullException ("rootName");
			if (rootNamespace == null)
				throw new ArgumentNullException ("rootNamespace");
			root_name = rootName;
			root_ns = rootNamespace;
		}

		public NetDataContractSerializer (StreamingContext context, 
			int maxItemsInObjectGraph,
			bool ignoreExtensibleDataObject,
			FormatterAssemblyStyle assemblyFormat,
			ISurrogateSelector surrogateSelector)
		{
			this.context = context;
			max_items = maxItemsInObjectGraph;
			ignore_extensions = ignoreExtensibleDataObject;
			ass_style = assemblyFormat;
			selector = surrogateSelector;
		}

		public NetDataContractSerializer (
			string rootName, string rootNamespace,
			StreamingContext context, 
			int maxItemsInObjectGraph,
			bool ignoreExtensibleDataObject,
			FormatterAssemblyStyle assemblyFormat,
			ISurrogateSelector surrogateSelector)
			: this (context, maxItemsInObjectGraph,
				ignoreExtensibleDataObject, assemblyFormat,
				surrogateSelector)
		{
			FillDictionaryString (rootName, rootNamespace);
		}

		public NetDataContractSerializer (
			XmlDictionaryString rootName,
			XmlDictionaryString rootNamespace,
			StreamingContext context,
			int maxItemsInObjectGraph,
			bool ignoreExtensibleDataObject,
			FormatterAssemblyStyle assemblyFormat,
			ISurrogateSelector surrogateSelector)
			: this (context, maxItemsInObjectGraph,
				ignoreExtensibleDataObject, assemblyFormat,
				surrogateSelector)
		{
			if (rootName == null)
				throw new ArgumentNullException ("rootName");
			if (rootNamespace == null)
				throw new ArgumentNullException ("rootNamespace");
			root_name = rootName;
			root_ns = rootNamespace;
		}

		void FillDictionaryString (string rootName, string rootNamespace)
		{
			if (rootName == null)
				throw new ArgumentNullException ("rootName");
			if (rootNamespace == null)
				throw new ArgumentNullException ("rootNamespace");
			XmlDictionary d = new XmlDictionary ();
			root_name = d.Add (rootName);
			root_ns = d.Add (rootNamespace);
		}

		public FormatterAssemblyStyle AssemblyFormat {
			get { return ass_style; }
			set { ass_style = value; }
		}

		public SerializationBinder Binder {
			get { return binder; }
			set { binder = value; }
		}

		public bool IgnoreExtensionDataObject {
			get { return ignore_extensions; }
		}

		public ISurrogateSelector SurrogateSelector {
			get { return selector; }
			set { selector = value; }
		}

		public StreamingContext Context {
			get { return context; }
			set { context = value; }
		}

		public int MaxItemsInObjectGraph {
			get { return max_items; }
		}

		public object Deserialize (Stream stream)
		{
			return ReadObject (stream);
		}

		[MonoTODO]
		public override bool IsStartObject (XmlDictionaryReader reader)
		{
			throw new NotImplementedException ();
		}

		public override object ReadObject (XmlDictionaryReader reader, bool readContentOnly)
		{
			/*
			int startTypeCount = known_types.Count;

			object ret = XmlFormatterDeserializer.Deserialize (
				// FIXME: remove this second param.
				reader, null, known_types, surrogate, readContentOnly);

			if (!readContentOnly && reader.NodeType == XmlNodeType.EndElement)
				reader.Read ();

			// remove temporarily-added known types for
			// rootType and object graph type.
			while (known_types.Count > startTypeCount)
				known_types.RemoveAt (startTypeCount);

			return ret;
			*/
			throw new NotImplementedException ();
		}

		public void Serialize (Stream stream, Object graph)
		{
			using (XmlWriter w = XmlWriter.Create (stream)) {
				WriteObject (w, graph);
			}
		}

		[MonoTODO ("support arrays; support Serializable; support SharedType; use DataContractSurrogate")]
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
		public override void WriteObjectContent (
			XmlDictionaryWriter writer, object graph)
		{
			/*
			int startTypeCount = known_types.Count;

			string ns = default_ns;

			//writer.WriteAttributeString ("xmlns", "i", xmlns, XmlSchema.InstanceNamespace);
			//writer.WriteAttributeString ("xmlns", "x", xmlns, XmlSchema.Namespace);
			if (ns != null)
				writer.WriteAttributeString ("xmlns", xmlns, ns);

			XmlFormatterSerializer.Serialize (writer, graph,
				known_types,
				ignore_extensions, max_items);

			// remove temporarily-added known types for
			// rootType and object graph type.
			while (known_types.Count > startTypeCount)
				known_types.RemoveAt (startTypeCount);
			*/
			throw new NotImplementedException ();
		}

		public override void WriteStartObject (
			XmlDictionaryWriter writer, object graph)
		{
			/*
			Type rootType = graph.GetType ();
			known_types.Add (rootType);
			SerializationMap map =
				known_types.FindUserMap (rootType);
			QName name = map != null ? map.XmlName :
				KnownTypeCollection.GetPredefinedTypeName (rootType);
			writer.WriteStartElement (
				name.Name, name.Namespace);
			*/
			throw new NotImplementedException ();
		}

		public override void WriteEndObject (XmlDictionaryWriter writer)
		{
			writer.WriteEndElement ();
		}
	}
}
#endif
