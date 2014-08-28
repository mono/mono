using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace System.IdentityModel.Protocols.WSTrust
{
	public class EndpointReference
	{
		private Collection<XmlElement> details = new Collection<XmlElement> ();
		private Uri uri = null;

		public Collection<XmlElement> Details { get { return details; } }
		public Uri Uri { get { return uri; } }

		public EndpointReference (string uri) {
			this.uri = new Uri (uri);
		}

		[MonoTODO]
		public static EndpointReference ReadFrom (XmlDictionaryReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static EndpointReference ReadFrom(Xml.XmlReader reader) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteTo (XmlWriter writer) {
			throw new NotImplementedException ();
		}
	}
}