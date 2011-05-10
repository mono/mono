//
// MetadataSet.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	[XmlRoot ("Metadata", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
	public class MetadataSet : IXmlSerializable
	{
		Collection<MetadataSection> sections
				= new Collection<MetadataSection> ();
		Collection<XmlAttribute> attributes
				= new Collection<XmlAttribute> ();

		public MetadataSet ()
		{
		}

		public MetadataSet (IEnumerable<MetadataSection> sections)
		{
			foreach (MetadataSection s in sections)
				this.sections.Add (s);
		}

		[XmlElement ("MetadataSection", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
		public Collection<MetadataSection> MetadataSections {
			get { return sections; }
		}

		[XmlAnyAttribute]
		public Collection<XmlAttribute> Attributes {
			get { return attributes; }
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "Metadata" || 
					reader.NamespaceURI != "http://schemas.xmlsoap.org/ws/2004/09/mex") 
				throw new InvalidOperationException (String.Format ("Unexpected : <{0} ..", reader.LocalName));

			/* Move to MetadataSections */
			reader.Read ();

			MetadataSectionSerializer xs = new MetadataSectionSerializer ();
			for (reader.MoveToContent (); reader.NodeType == XmlNodeType.Element && reader.LocalName == "MetadataSection" && reader.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/mex"; reader.MoveToContent ()) {
				MetadataSection ms = (MetadataSection) xs.Deserialize (reader);
				MetadataSections.Add (ms);
			}
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			throw new NotImplementedException ();
		}

		public static MetadataSet ReadFrom (XmlReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			XmlSerializer xs = new XmlSerializer (typeof (MetadataSet));
			MetadataSet ms = (MetadataSet) xs.Deserialize (reader);

			return ms;
		}

		public void WriteTo (XmlWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			writer.WriteStartElement ("Metadata", "http://schemas.xmlsoap.org/ws/2004/09/mex");

			writer.WriteAttributeString ("xmlns", "xsd", "http://www.w3.org/2000/xmlns/", XmlSchema.Namespace);
			writer.WriteAttributeString ("xmlns", "wsx", "http://www.w3.org/2000/xmlns/", MetadataSection.MetadataExchangeDialect);
			writer.WriteAttributeString ("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", XmlSchema.InstanceNamespace);

			XmlSerializer serializer = MetadataSection.Serializer;
			foreach (MetadataSection section in MetadataSections)
				serializer.Serialize (writer, section);

			writer.WriteEndElement ();
		}
	}
}
