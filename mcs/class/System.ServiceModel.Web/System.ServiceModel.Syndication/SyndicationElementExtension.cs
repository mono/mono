//
// SyndicationElementExtension.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.ServiceModel.Syndication
{
	public class SyndicationElementExtension
	{
		ReadWriteHandler handler;

		public SyndicationElementExtension (object dataContractExtension)
			: this (dataContractExtension, (XmlObjectSerializer) null)
		{
		}

		public SyndicationElementExtension (object dataContractExtension, XmlObjectSerializer dataContractSerializer)
			: this (null, null, dataContractExtension, dataContractSerializer)
		{
		}

		public SyndicationElementExtension (string outerName, string outerNamespace, object dataContractExtension)
			: this (outerName, outerNamespace, dataContractExtension, null)
		{
		}

		public SyndicationElementExtension (string outerName, string outerNamespace, object dataContractExtension, XmlObjectSerializer dataContractSerializer)
		{
			if (dataContractExtension == null)
				throw new ArgumentNullException ("dataContractExtension");
			handler = new DataContractReadWriteHandler (outerName, outerNamespace, dataContractExtension, dataContractSerializer);
		}

		public SyndicationElementExtension (object xmlSerializerExtension, XmlSerializer serializer)
		{
			if (xmlSerializerExtension == null)
				throw new ArgumentNullException ("xmlSerializerExtension");
			handler = new XmlSerializationReadWriteHandler (xmlSerializerExtension, serializer);
		}

		public SyndicationElementExtension (XmlReader xmlReader)
		{
			if (xmlReader == null)
				throw new ArgumentNullException ("xmlReader");
			xmlReader.MoveToContent ();
			if (xmlReader.NodeType != XmlNodeType.Element)
				throw new XmlException ("Element node is expected on the argument xmlReader");

			handler = new XmlReaderReadWriteHandler (xmlReader);
		}

		public string OuterName {
			get { return handler != null ? handler.Name : null; }
		}

		public string OuterNamespace {
			get { return handler != null ? handler.Namespace : null; }
		}

		public TExtension GetObject<TExtension> ()
		{
			return GetObject<TExtension> (new DataContractSerializer (typeof (TExtension)));
		}

		public TExtension GetObject<TExtension> (XmlObjectSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException ("serializer");
			return (TExtension) serializer.ReadObject (GetReader (), false);
		}

		public TExtension GetObject<TExtension> (XmlSerializer serializer)
		{
			if (serializer == null)
				throw new ArgumentNullException ("serializer");
			return (TExtension) serializer.Deserialize (GetReader ());
		}

		public XmlReader GetReader ()
		{
			var r = handler.GetReader ();
			r.MoveToContent ();
			return r;
		}

		public void WriteTo (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			handler.WriteTo (writer);
		}

		abstract class ReadWriteHandler
		{
			public string Name { get; protected set; }

			public string Namespace { get; protected set; }

			public virtual XmlReader GetReader ()
			{
				StringWriter sw = new StringWriter ();
				using (XmlWriter w = XmlWriter.Create (sw))
					WriteTo (w);
				return XmlReader.Create (new StringReader (sw.ToString ()));
			}

			public abstract void WriteTo (XmlWriter writer);
		}

		class DataContractReadWriteHandler : ReadWriteHandler
		{
			object extension;
			XmlObjectSerializer serializer;
			
			public DataContractReadWriteHandler (string name, string ns, object extension, XmlObjectSerializer serializer)
			{
				this.Name = name;
				this.Namespace = ns;
				this.extension = extension;
				this.serializer = serializer;

				if (this.serializer == null)
					this.serializer = new DataContractSerializer (extension.GetType ());
			}

			public override void WriteTo (XmlWriter writer)
			{
				if (Name != null) {
					writer.WriteStartElement (Name, Namespace);
					serializer.WriteObjectContent (writer, extension);
					writer.WriteFullEndElement ();
				}
				else
					serializer.WriteObject (writer, extension);
			}
		}

		class XmlSerializationReadWriteHandler : ReadWriteHandler
		{
			object extension;
			XmlSerializer serializer;

			public XmlSerializationReadWriteHandler (object extension, XmlSerializer serializer)
			{
				this.extension = extension;
				this.serializer = serializer;

				if (serializer == null)
					serializer = new XmlSerializer (extension.GetType ());
			}

			public override void WriteTo (XmlWriter writer)
			{
				serializer.Serialize (writer, extension);
			}
		}

		class XmlReaderReadWriteHandler : ReadWriteHandler
		{
			string xml;

			public XmlReaderReadWriteHandler (XmlReader reader)
			{
				reader.MoveToContent ();
				Name = reader.LocalName;
				Namespace = reader.NamespaceURI;
				xml = reader.ReadOuterXml ();
			}

			public override XmlReader GetReader ()
			{
				var r = XmlReader.Create (new StringReader (xml));
				r.MoveToContent ();
				return r;
			}

			public override void WriteTo (XmlWriter writer)
			{
				writer.WriteNode (GetReader (), false);
			}
		}
	}
}
