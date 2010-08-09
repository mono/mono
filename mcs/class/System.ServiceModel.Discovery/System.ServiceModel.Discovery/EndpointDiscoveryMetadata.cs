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
	[MonoTODO]
	public class EndpointDiscoveryMetadata
	{
		public static EndpointDiscoveryMetadata FromServiceEndpoint (ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}

		public static EndpointDiscoveryMetadata FromServiceEndpoint (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			throw new NotImplementedException ();
		}

		public EndpointAddress Address { get; set; }
		public Collection<XmlQualifiedName> ContractTypeNames { get; private set; }
		public Collection<XElement> Extensions { get; private set; }
		public Collection<Uri> ListenUris { get; private set; }
		public Collection<Uri> Scopes { get; private set; }
		public int Version { get; set; }

		internal XmlSchema BuildSchema ()
		{
			throw new NotImplementedException ();
		}

		internal EndpointDiscoveryMetadata ReadXml (XmlReader reader, DiscoveryVersion version)
		{
			throw new NotImplementedException ();
		}

		internal void WriteXml (XmlWriter writer, DiscoveryVersion version)
		{
			throw new NotImplementedException ();
		}
	}
}
