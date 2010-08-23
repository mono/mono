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
	public class EndpointDiscoveryMetadata
	{
		public static EndpointDiscoveryMetadata FromServiceEndpoint (ServiceEndpoint endpoint)
		{
			var ret = new EndpointDiscoveryMetadata ();
			ret.ContractTypeNames.Add (new XmlQualifiedName (endpoint.Contract.Name, endpoint.Contract.Namespace));
			ret.Address = endpoint.Address;
			if (endpoint.Address != null)
				ret.ListenUris.Add (endpoint.Address.Uri);

			var edb = endpoint.Behaviors.Find<EndpointDiscoveryBehavior> ();
			if (edb != null) {
				foreach (var ctn in edb.ContractTypeNames)
					ret.ContractTypeNames.Add (ctn);
				foreach (var ext in edb.Extensions)
					ret.Extensions.Add (ext);
			}

			return ret;
		}

		public static EndpointDiscoveryMetadata FromServiceEndpoint (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			throw new NotImplementedException ();
		}
		
		public EndpointDiscoveryMetadata ()
		{
			Address = new EndpointAddress (EndpointAddress.AnonymousUri);
			ContractTypeNames = new Collection<XmlQualifiedName> ();
			ListenUris = new Collection<Uri> ();
			Scopes = new Collection<Uri> ();
			Extensions = new Collection<XElement> ();
		}

		public EndpointAddress Address { get; set; }
		public Collection<XmlQualifiedName> ContractTypeNames { get; private set; }
		public Collection<XElement> Extensions { get; private set; }
		public Collection<Uri> ListenUris { get; private set; }
		public Collection<Uri> Scopes { get; private set; }
		public int Version { get; set; }

		internal static EndpointDiscoveryMetadata ReadXml (XmlReader reader, DiscoveryVersion version)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			var ret = new EndpointDiscoveryMetadata ();

			reader.MoveToContent ();
			if (!reader.IsStartElement ("ProbeMatchType", version.Namespace) || reader.IsEmptyElement)
				throw new XmlException (String.Format ("Non-empty ProbeMatchType element is expected. Got {2} {0} in {1} namespace instead.", reader.LocalName, reader.NamespaceURI, reader.IsEmptyElement ? "empty" : "non-empty"));
			reader.ReadStartElement ("ProbeType", version.Namespace);

			// standard members
			reader.MoveToContent ();
			ret.Address = EndpointAddress.ReadFrom (AddressingVersion.WSAddressing10, reader);

			reader.MoveToContent ();
			bool isEmpty = reader.IsEmptyElement;
			ret.ContractTypeNames = new Collection<XmlQualifiedName> ((XmlQualifiedName []) reader.ReadElementContentAs (typeof (XmlQualifiedName []), null, "Types", version.Namespace));

			reader.MoveToContent ();
			if (reader.IsStartElement ("Scopes", version.Namespace))
				ret.Scopes = new Collection<Uri> ((Uri []) reader.ReadElementContentAs (typeof (Uri []), null, "Scopes", version.Namespace));

			if (reader.IsStartElement ("XAddrs", version.Namespace))
				ret.ListenUris = new Collection<Uri> ((Uri []) reader.ReadElementContentAs (typeof (Uri []), null, "XAddrs", version.Namespace));

			if (reader.IsStartElement ("MetadataVersion", version.Namespace))
				ret.Version = reader.ReadElementContentAsInt ();

			// non-standard members
			for (reader.MoveToContent (); !reader.EOF && reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ())
				ret.Extensions.Add (XElement.Load (reader));

			reader.ReadEndElement ();

			return ret;
		}

		internal void WriteXml (XmlWriter writer, DiscoveryVersion version)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			// standard members
			if (Address != null)
				Address.WriteTo (AddressingVersion.WSAddressing10, writer);

			writer.WriteStartElement ("d", "Types", version.Namespace);
			int p = 0;
			foreach (var qname in ContractTypeNames)
				if (writer.LookupPrefix (qname.Namespace) == null)
					writer.WriteAttributeString ("xmlns", "p" + p++, "http://www.w3.org/2000/xmlns/", qname.Namespace);
			writer.WriteValue (ContractTypeNames);
			writer.WriteEndElement ();

			if (Scopes.Count > 0) {
				writer.WriteStartElement ("Scopes", version.Namespace);
				writer.WriteValue (Scopes);
				writer.WriteEndElement ();
			}

			if (ListenUris.Count > 0) {
				writer.WriteStartElement ("XAddrs", version.Namespace);
				writer.WriteValue (ListenUris);
				writer.WriteEndElement ();
			}
			
			writer.WriteStartElement ("MetadataVersion", version.Namespace);
			writer.WriteValue (Version);
			writer.WriteEndElement ();

			// non-standard members

			foreach (var ext in Extensions)
				ext.WriteTo (writer);
		}

		internal static XmlSchema BuildSchema (DiscoveryVersion version)
		{
			var schema = new XmlSchema () { TargetNamespace = version.Namespace };

			var anyAttr = new XmlSchemaAnyAttribute () { Namespace = "##other", ProcessContents = XmlSchemaContentProcessing.Lax };

			var probePart = new XmlSchemaSequence ();
			probePart.Items.Add (new XmlSchemaElement () { RefName = new XmlQualifiedName ("Types", version.Namespace), MinOccurs = 0 });
			probePart.Items.Add (new XmlSchemaElement () { RefName = new XmlQualifiedName ("Scopes", version.Namespace), MinOccurs = 0 });
			probePart.Items.Add (new XmlSchemaElement () { RefName = new XmlQualifiedName ("XAddrs", version.Namespace), MinOccurs = 0 });
			probePart.Items.Add (new XmlSchemaElement () { RefName = new XmlQualifiedName ("MetadataVersion", version.Namespace), MinOccurs = 0 });
			probePart.Items.Add (new XmlSchemaAny () { MinOccurs = 0, MaxOccursString = "unbounded", Namespace = "##other", ProcessContents = XmlSchemaContentProcessing.Lax });
			var ct = new XmlSchemaComplexType () { Name = "ProbeMatchType", Particle = probePart, AnyAttribute = anyAttr };
			schema.Items.Add (ct);

			schema.Items.Add (new XmlSchemaSimpleType () { Name = "QNameListType", Content = new XmlSchemaSimpleTypeList () { ItemTypeName = new XmlQualifiedName ("QName", XmlSchema.Namespace) } });

			var scr = new XmlSchemaSimpleContentRestriction () { BaseTypeName = new XmlQualifiedName ("UriListType", version.Namespace), AnyAttribute = anyAttr };
			scr.Attributes.Add (new XmlSchemaAttribute () { Name = "matchBy", SchemaTypeName = new XmlQualifiedName ("anyURI", XmlSchema.Namespace) });
			schema.Items.Add (new XmlSchemaComplexType () { Name = "ScopesType", ContentModel = new XmlSchemaSimpleContent () { Content = scr } });

			schema.Items.Add (new XmlSchemaSimpleType () { Name = "UriListType", Content = new XmlSchemaSimpleTypeList () { ItemTypeName = new XmlQualifiedName ("anyURI", XmlSchema.Namespace) } });

			schema.Items.Add (new XmlSchemaElement () { Name = "Types", SchemaTypeName = new XmlQualifiedName ("QNameListType", version.Namespace) });
			schema.Items.Add (new XmlSchemaElement () { Name = "Scopes", SchemaTypeName = new XmlQualifiedName ("ScopesType", version.Namespace) });
			schema.Items.Add (new XmlSchemaElement () { Name = "XAddrs", SchemaTypeName = new XmlQualifiedName ("UriListType", version.Namespace) });
			schema.Items.Add (new XmlSchemaElement () { Name = "MetadataVersion", SchemaTypeName = new XmlQualifiedName ("unisgnedInt", XmlSchema.Namespace) });

			return schema;
		}
	}
}
