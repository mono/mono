//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace System.ServiceModel.Discovery
{
	public class FindCriteria
	{
		const string SerializationNS = "http://schemas.microsoft.com/ws/2008/06/discovery";
		const int default_max_results = int.MaxValue;

		public static readonly Uri ScopeMatchByExact = new Uri ("http://schemas.microsoft.com/ws/2008/06/discovery/strcmp0");
		public static readonly Uri ScopeMatchByLdap = new Uri ("http://schemas.microsoft.com/ws/2008/06/discovery/ldap");
		public static readonly Uri ScopeMatchByNone = new Uri ("http://schemas.microsoft.com/ws/2008/06/discovery/none");
		public static readonly Uri ScopeMatchByPrefix = new Uri ("http://schemas.microsoft.com/ws/2008/06/discovery/rfc3986");
		public static readonly Uri ScopeMatchByUuid = new Uri ("http://schemas.microsoft.com/ws/2008/06/discovery/uuid");

		public static FindCriteria CreateMetadataExchangeEndpointCriteria ()
		{
			return CreateMetadataExchangeEndpointCriteria (typeof (IMetadataExchange));
		}

		public static FindCriteria CreateMetadataExchangeEndpointCriteria (IEnumerable<XmlQualifiedName> contractTypeNames)
		{
			var fc = new FindCriteria ();
			foreach (var type in contractTypeNames)
				fc.ContractTypeNames.Add (type);
			return fc;
		}

		public static FindCriteria CreateMetadataExchangeEndpointCriteria (Type contractType)
		{
			return new FindCriteria (contractType);
		}

		public FindCriteria ()
		{
			ContractTypeNames = new Collection<XmlQualifiedName> ();
			Extensions = new Collection<XElement> ();
			Scopes = new Collection<Uri> ();
			ScopeMatchBy = ScopeMatchByPrefix;
			MaxResults = default_max_results;
			Duration = TimeSpan.FromSeconds (20);
		}

		public FindCriteria (Type contractType)
			: this ()
		{
			var cd = ContractDescription.GetContract (contractType);
			ContractTypeNames.Add (new XmlQualifiedName (cd.Name, cd.Namespace));
		}

		public Collection<XmlQualifiedName> ContractTypeNames { get; private set; }
		public TimeSpan Duration { get; set; }
		public Collection<XElement> Extensions { get; private set; }
		public int MaxResults { get; set; }
		public Uri ScopeMatchBy { get; set; }
		public Collection<Uri> Scopes { get; private set; }

		[MonoTODO ("find out conformant behavior, and implement remaining bits")]
		public bool IsMatch (EndpointDiscoveryMetadata endpointDiscoveryMetadata)
		{
			var edm = endpointDiscoveryMetadata;
			if (edm == null)
				throw new ArgumentNullException ("endpointDiscoveryMetadata");
			if (ContractTypeNames.Count > 0) {
				bool match = false;
				foreach (var qn in ContractTypeNames)
					if (edm.ContractTypeNames.Contains (qn))
						match = true;
				if (!match)
					return false;
			}
			if (Scopes.Count > 0) {
				bool match = false;
				foreach (var scope in Scopes) {
					if (ScopeMatchBy == null || ScopeMatchBy.Equals (ScopeMatchByPrefix)) {
						if (edm.Scopes.Contains (scope))
							match = true;
					} else if (ScopeMatchBy.Equals (ScopeMatchByExact)) {
						if (edm.Scopes.Any (s => s.AbsoluteUri == scope.AbsoluteUri))
							match = true;
					}
					else if (ScopeMatchBy.Equals (ScopeMatchByUuid))
						throw new NotImplementedException ();
					else if (ScopeMatchBy.Equals (ScopeMatchByNone))
						throw new NotImplementedException ();
					else if (ScopeMatchBy.Equals (ScopeMatchByLdap))
						throw new NotImplementedException ();
					else
						throw new InvalidOperationException (String.Format ("Unexpected ScopeMatchBy value: {0}", ScopeMatchBy));
				}
				if (!match)
					return false;
			}
			if (Extensions.Count > 0)
				throw new NotImplementedException (String.Format ("{0} extensions are found", Extensions.Count));

			return true;
		}

		internal static FindCriteria ReadXml (XmlReader reader, DiscoveryVersion version)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			var ret = new FindCriteria ();

			reader.MoveToContent ();
			if (!reader.IsStartElement ("Probe", version.Namespace) || reader.IsEmptyElement)
				throw new XmlException (String.Format ("Non-empty ProbeType element is expected. Got '{0}' {1} node in namespace '{2}' instead.", reader.LocalName, reader.NodeType, reader.NamespaceURI));
			reader.ReadStartElement ("Probe", version.Namespace);

			// standard members
			reader.MoveToContent ();
			if (reader.IsStartElement ("Types", version.Namespace))
				ret.ContractTypeNames = new Collection<XmlQualifiedName> ((XmlQualifiedName []) reader.ReadElementContentAs (typeof (XmlQualifiedName []), null, "Types", version.Namespace));

			reader.MoveToContent ();
			if (reader.IsStartElement ("Types", version.Namespace)) {
				if (reader.MoveToAttribute ("MatchBy")) {
					ret.ScopeMatchBy = new Uri (reader.Value, UriKind.RelativeOrAbsolute);
					reader.MoveToElement ();
				}
			}
			if (reader.IsStartElement ("Scopes", version.Namespace))
				ret.Scopes = new Collection<Uri> ((Uri []) reader.ReadElementContentAs (typeof (Uri []), null, "Scopes", version.Namespace));

			// non-standard members
			for (reader.MoveToContent (); !reader.EOF && reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NamespaceURI == SerializationNS) {
					switch (reader.LocalName) {
					case "MaxResults":
						ret.MaxResults = reader.ReadElementContentAsInt ();
						break;
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
			writer.WriteStartElement ("d", "Types", version.Namespace);
			int p = 0;
			foreach (var qname in ContractTypeNames)
				if (writer.LookupPrefix (qname.Namespace) == null)
					writer.WriteAttributeString ("xmlns", "p" + p++, "http://www.w3.org/2000/xmlns/", qname.Namespace);
			writer.WriteValue (ContractTypeNames);
			writer.WriteEndElement ();

			writer.WriteStartElement ("Scopes", version.Namespace);
			if (ScopeMatchBy != null) {
				writer.WriteStartAttribute ("MatchBy");
				writer.WriteValue (ScopeMatchBy);
				writer.WriteEndAttribute ();
			}
			writer.WriteValue (Scopes);
			writer.WriteEndElement ();

			// non-standard members
			if (MaxResults != default_max_results) {
				writer.WriteStartElement ("MaxResults", SerializationNS);
				writer.WriteValue (MaxResults);
				writer.WriteEndElement ();
			}
			writer.WriteStartElement ("Duration", SerializationNS);
			writer.WriteValue (Duration);
			writer.WriteEndElement ();
			
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
			probePart.Items.Add (new XmlSchemaAny () { MinOccurs = 0, MaxOccursString = "unbounded", Namespace = "##other", ProcessContents = XmlSchemaContentProcessing.Lax });
			var ct = new XmlSchemaComplexType () { Name = "ProbeType", Particle = probePart, AnyAttribute = anyAttr };
			schema.Items.Add (ct);

			schema.Items.Add (new XmlSchemaSimpleType () { Name = "QNameListType", Content = new XmlSchemaSimpleTypeList () { ItemTypeName = new XmlQualifiedName ("QName", XmlSchema.Namespace) } });

			var scr = new XmlSchemaSimpleContentRestriction () { BaseTypeName = new XmlQualifiedName ("UriListType", version.Namespace), AnyAttribute = anyAttr };
			scr.Attributes.Add (new XmlSchemaAttribute () { Name = "matchBy", SchemaTypeName = new XmlQualifiedName ("anyURI", XmlSchema.Namespace) });
			schema.Items.Add (new XmlSchemaComplexType () { Name = "ScopesType", ContentModel = new XmlSchemaSimpleContent () { Content = scr } });

			schema.Items.Add (new XmlSchemaSimpleType () { Name = "UriListType", Content = new XmlSchemaSimpleTypeList () { ItemTypeName = new XmlQualifiedName ("anyURI", XmlSchema.Namespace) } });

			schema.Items.Add (new XmlSchemaElement () { Name = "Types", SchemaTypeName = new XmlQualifiedName ("QNameListType", version.Namespace) });
			schema.Items.Add (new XmlSchemaElement () { Name = "Scopes", SchemaTypeName = new XmlQualifiedName ("ScopesType", version.Namespace) });

			return schema;
		}
	}
}
