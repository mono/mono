//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace System.ServiceModel.Discovery
{
	public class ResolveCriteria
	{
		const string SerializationNS = "http://schemas.microsoft.com/ws/2008/06/discovery";
		static readonly EndpointAddress anonymous_address = new EndpointAddress (EndpointAddress.AnonymousUri);

		public ResolveCriteria ()
		{
			Extensions = new Collection<XElement> ();
			Duration = TimeSpan.FromSeconds (20);
			Address = anonymous_address;
		}

		public ResolveCriteria (EndpointAddress address)
			: this ()
		{
			Address = address;
		}

		public EndpointAddress Address { get; set; }
		public TimeSpan Duration { get; set; }
		public Collection<XElement> Extensions { get; private set; }

		internal static ResolveCriteria ReadXml (XmlReader reader, DiscoveryVersion version)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			var ret = new ResolveCriteria ();

			reader.MoveToContent ();
			if (!reader.IsStartElement ("ResolveType", version.Namespace) || reader.IsEmptyElement)
				throw new XmlException ("Non-empty ResolveType element is expected");
			reader.ReadStartElement ("ResolveType", version.Namespace);

			// standard members
			reader.MoveToContent ();
			ret.Address = EndpointAddress.ReadFrom (AddressingVersion.WSAddressing10, reader);

			// non-standard members
			for (reader.MoveToContent (); !reader.EOF && reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NamespaceURI == SerializationNS) {
					switch (reader.LocalName) {
					case "Duration":
						ret.Duration = (TimeSpan) reader.ReadElementContentAs (typeof (TimeSpan), null);
						break;
					}
				}
				else
					ret.Extensions.Add (XElement.Load (reader));
			}

			reader.ReadEndElement ();

			return ret;
		}

		internal void WriteXml (XmlWriter writer, DiscoveryVersion version)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			// standard members
			Address.WriteTo (AddressingVersion.WSAddressing10, writer);

			// non-standard members
			writer.WriteStartElement ("Duration", SerializationNS);
			writer.WriteValue (Duration);
			writer.WriteEndElement ();
			
			foreach (var ext in Extensions)
				ext.WriteTo (writer);
		}

		internal static XmlSchema BuildSchema (DiscoveryVersion version)
		{
			var schema = new XmlSchema () { TargetNamespace = version.Namespace };
			string addrNS = "http://www.w3.org/2005/08/addressing";

			var anyAttr = new XmlSchemaAnyAttribute () { Namespace = "##other", ProcessContents = XmlSchemaContentProcessing.Lax };

			var resolvePart = new XmlSchemaSequence ();
			resolvePart.Items.Add (new XmlSchemaElement () { RefName = new XmlQualifiedName ("EndpointReference", addrNS), MinOccurs = 0 });
			resolvePart.Items.Add (new XmlSchemaAny () { MinOccurs = 0, MaxOccursString = "unbounded", Namespace = "##other", ProcessContents = XmlSchemaContentProcessing.Lax });
			var ct = new XmlSchemaComplexType () { Name = "ResolveType", Particle = resolvePart, AnyAttribute = anyAttr };

			schema.Includes.Add (new XmlSchemaImport () { Namespace = addrNS });
			schema.Items.Add (ct);

			return schema;
		}
	}
}
