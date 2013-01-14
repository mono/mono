//
// DataContractJsonSerializer.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
	public sealed class DataContractJsonSerializer : XmlObjectSerializer
	{
		const string default_root_name = "root";

		#region lengthy constructor list

		public DataContractJsonSerializer (Type type)
			: this (type, Type.EmptyTypes)
		{
		}

		public DataContractJsonSerializer (Type type, IEnumerable<Type> knownTypes)
			: this (type, default_root_name, knownTypes)
		{
		}

		public DataContractJsonSerializer (Type type, string rootName)
			: this (type, rootName, Type.EmptyTypes)
		{
		}

		public DataContractJsonSerializer (Type type, XmlDictionaryString rootName)
			: this (type, rootName != null ? rootName.Value : default_root_name, Type.EmptyTypes)
		{
		}

		public DataContractJsonSerializer (Type type, string rootName, IEnumerable<Type> knownTypes)
			: this (type, rootName, knownTypes, int.MaxValue, false, false)
		{
		}

		public DataContractJsonSerializer (Type type, XmlDictionaryString rootName, IEnumerable<Type> knownTypes)
			: this (type, rootName != null ? rootName.Value : default_root_name, knownTypes)
		{
		}

		DataContractJsonSerializer(Type type, string rootName, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, bool alwaysEmitTypeInformation)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (rootName == null)
				throw new ArgumentNullException ("rootName");
			if (maxItemsInObjectGraph < 0)
				throw new ArgumentOutOfRangeException ("maxItemsInObjectGraph");

			this.type = type;
			known_types = new ReadOnlyCollection<Type> (knownTypes != null ? knownTypes.ToArray () : Type.EmptyTypes);
			root = rootName;
			max_items = maxItemsInObjectGraph;
			ignore_extension = ignoreExtensionDataObject;
			always_emit_type = alwaysEmitTypeInformation;
		}

		public DataContractJsonSerializer (Type type, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
            : this (type, default_root_name, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, alwaysEmitTypeInformation)
		{
	}

		public DataContractJsonSerializer (Type type, string rootName, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
			: this (type, rootName, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, alwaysEmitTypeInformation)
		{
			surrogate = dataContractSurrogate;
		}

		public DataContractJsonSerializer (Type type, XmlDictionaryString rootName, IEnumerable<Type> knownTypes, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
			: this (type, rootName != null ? rootName.Value : default_root_name, knownTypes, maxItemsInObjectGraph, ignoreExtensionDataObject, dataContractSurrogate, alwaysEmitTypeInformation)
		{
		}

        #endregion

        Type type;
		string root;
		ReadOnlyCollection<Type> known_types;
		int max_items;
		bool ignore_extension;
		bool always_emit_type;
		IDataContractSurrogate surrogate;

		[MonoTODO]
		public IDataContractSurrogate DataContractSurrogate {
			get { return surrogate; }
		}

		[MonoTODO]
		public bool IgnoreExtensionDataObject {
			get { return ignore_extension; }
		}

		[MonoTODO]
		public ReadOnlyCollection<Type> KnownTypes {
			get { return known_types; }
		}

		public int MaxItemsInObjectGraph {
			get { return max_items; }
		}

		public override bool IsStartObject (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			reader.MoveToContent ();
			return reader.IsStartElement (root, String.Empty);
		}

		public override bool IsStartObject (XmlDictionaryReader reader)
		{
			return IsStartObject ((XmlReader) reader);
		}

		public override object ReadObject (Stream stream)
		{
#if NET_2_1
			var r = (JsonReader) JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
			r.LameSilverlightLiteralParser = true;
			return ReadObject(r);
#else
			return ReadObject (JsonReaderWriterFactory.CreateJsonReader (stream, new XmlDictionaryReaderQuotas ()));
#endif
		}

		public override object ReadObject (XmlDictionaryReader reader)
		{
			return ReadObject (reader, true);
		}

		public override object ReadObject (XmlReader reader)
		{
			return ReadObject (reader, true);
		}

		public override object ReadObject (XmlDictionaryReader reader, bool verifyObjectName)
		{
			return ReadObject ((XmlReader) reader, verifyObjectName);
		}

		public override object ReadObject (XmlReader reader, bool verifyObjectName)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			try {
				if (verifyObjectName && !IsStartObject (reader))
					throw new SerializationException (String.Format ("Expected element was '{0}', but the actual input element was '{1}' in namespace '{2}'", root, reader.LocalName, reader.NamespaceURI));

				return new JsonSerializationReader (this, reader, type, verifyObjectName).ReadRoot ();
			} catch (SerializationException) {
				throw;
			} catch (Exception ex) {
				throw new SerializationException ("Deserialization has failed", ex);
			}
		}

		public override void WriteObject (Stream stream, object graph)
		{
			using (var xw = JsonReaderWriterFactory.CreateJsonWriter (stream))
				WriteObject (xw, graph);
		}

		public override void WriteObject (XmlWriter writer, object graph)
		{
			try {
				WriteStartObject (writer, graph);
				WriteObjectContent (writer, graph);
				WriteEndObject (writer);
			} catch (NotImplementedException) {
				throw;
			} catch (InvalidDataContractException) {
				throw;
			} catch (Exception ex) {
				throw new SerializationException (String.Format ("There was an error during serialization for object of type {0}", graph != null ? graph.GetType () : null), ex);
			}
		}

		public override void WriteObject (XmlDictionaryWriter writer, object graph)
		{
			WriteObject ((XmlWriter) writer, graph);
		}

		public override void WriteStartObject (XmlDictionaryWriter writer, object graph)
		{
			WriteStartObject ((XmlWriter) writer, graph);
		}

		public override void WriteStartObject (XmlWriter writer, object graph)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			writer.WriteStartElement (root);
		}

		public override void WriteObjectContent (XmlDictionaryWriter writer, object graph)
		{
			WriteObjectContent ((XmlWriter) writer, graph);
		}

		public override void WriteObjectContent (XmlWriter writer, object graph)
		{
			new JsonSerializationWriter (this, writer, type, always_emit_type).WriteObjectContent (graph, true, false);
		}

		public override void WriteEndObject (XmlDictionaryWriter writer)
		{
			WriteEndObject ((XmlWriter) writer);
		}

		public override void WriteEndObject (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			writer.WriteEndElement ();
		}
	}
}
