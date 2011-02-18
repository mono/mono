//
// EndpointAddressAugust2004.cs
//
// Author:
//	Ankit Jain <jankit@novell.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel
{
	[XmlSchemaProvider ("GetSchema")]
	[XmlRoot ("EndpointReference", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
	public class EndpointAddressAugust2004 : IXmlSerializable
	{
		EndpointAddress address;

		internal EndpointAddressAugust2004 ()
		{
		}

		internal EndpointAddressAugust2004 (EndpointAddress address)
		{
			this.address = address;
		}
		
		public static EndpointAddressAugust2004 FromEndpointAddress (EndpointAddress address)
		{
			return new EndpointAddressAugust2004 (address);
		}

		public static XmlQualifiedName GetSchema (XmlSchemaSet xmlSchemaSet)
		{
			if (xmlSchemaSet == null)
				throw new ArgumentNullException ("xmlSchemaSet");
			xmlSchemaSet.Add (XmlSchema.Read (typeof (EndpointAddress10).Assembly.GetManifestResourceStream ("WS-Addressing.schema"), null));
			return new XmlQualifiedName ("EndpointReferenceType", Constants.WsaNamespace);
		}

		public EndpointAddress ToEndpointAddress ()
		{
			return address;
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			address = EndpointAddress.ReadFrom (AddressingVersion.WSAddressingAugust2004, reader);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			address.WriteContentsTo (AddressingVersion.WSAddressingAugust2004, writer);
		}
	}
}
