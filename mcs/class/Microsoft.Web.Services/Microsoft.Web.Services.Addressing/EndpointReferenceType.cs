//
// Microsoft.Web.Services.Addressing
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public abstract class EndpointReferenceType : OpenElement
	{

		private Address _address;
		private PortType _portType;
		private ReferenceProperties _properties;
		private ServiceName _serviceName;

		public EndpointReferenceType (Uri address) : base ()
		{
			if(address == null) {
				throw new ArgumentNullException ("address");
			}
			_address = new Address (address);
		}

		public EndpointReferenceType (Address address) : base ()
		{
			if(address == null) {
				throw new ArgumentNullException ("address");
			}
			_address = address;
		}

		public EndpointReferenceType () : base ()
		{
		}

		protected override void GetXmlAny (XmlDocument document, XmlElement element)
		{
			if(document == null) {
				throw new ArgumentNullException ("document");
			}
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			element.AppendChild (_address.GetXml (document));

			if(_portType != null) {
				element.AppendChild (_portType.GetXml (document));
			}

			if(_properties != null) {
				element.AppendChild (_properties.GetXml (document));
			}

			if(_serviceName != null) {
				element.AppendChild (_serviceName.GetXml (document));
			}

			base.GetXmlAny (document, element);
		}

		protected override void LoadXmlAny (XmlElement element)
		{
			if(element == null) {
				throw new ArgumentNullException ("element");
			}

			foreach (XmlAttribute attrib in element.Attributes) {
				AnyAttributes.Add (attrib);
			}

			foreach (XmlElement node in element.ChildNodes) {
				if(node.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {
					continue;
				}
				switch (node.LocalName) {
					case "Address":
						_address = new Address (node);
						continue;
					case "ReferenceProperties":
						_properties = new ReferenceProperties (node);
						continue;
					case "PortType":
						_portType = new PortType (node);
						continue;
					case "ServiceName":
						_serviceName = new ServiceName (node);
						continue;
				}

				AnyElements.Add (node);
			}
		}

		public Address Address {
			get { return _address; }
			set { 
				if(value == null) {
					throw new ArgumentNullException ("Address");
				}
				_address = value; 
			}
		}

		public PortType PortType {
			get { return _portType; }
			set { _portType = value; }
		}

		public ReferenceProperties ReferenceProperties {
			get { return _properties; }
			set { _properties = value; }
		}

		public ServiceName ServiceName {
			get { return _serviceName; }
			set { _serviceName = value; }
		}

	}

}
