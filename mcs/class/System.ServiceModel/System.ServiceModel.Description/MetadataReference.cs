//
// MetadataReference.cs
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	[XmlRoot ("MetadataReference", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
	public class MetadataReference : IXmlSerializable
	{
		EndpointAddress address;
		AddressingVersion address_version;

		public MetadataReference ()
			: this (null, null)
		{
		}

		public MetadataReference (EndpointAddress address, AddressingVersion addressVersion)
		{
			this.address = address;
			this.address_version = addressVersion;
		}

		public EndpointAddress Address {
			get { return address; }
			set { address = value; }
		}

		public AddressingVersion AddressVersion {
			get { return address_version; }
			set { address_version = value; }
		}
		
		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			if (reader.NodeType != XmlNodeType.Element || 
				reader.LocalName != "MetadataReference" || 
				reader.NamespaceURI != "http://schemas.xmlsoap.org/ws/2004/09/mex") 
				throw new InvalidOperationException (String.Format ("Unexpected : <{0} ..", reader.LocalName));

			throw new NotImplementedException ("Implement me!");
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

	}
}
