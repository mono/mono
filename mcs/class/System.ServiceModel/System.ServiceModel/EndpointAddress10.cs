//
// EndpointAddress10.cs
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
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel
{
	[XmlSchemaProvider ("GetSchema")]
	[XmlRoot ("EndpointReference", Namespace = "http://www.w3.org/2005/08/addressing")]
#if NET_2_1
	internal class EndpointAddress10 : IXmlSerializable
#else
	public class EndpointAddress10 : IXmlSerializable
#endif
	{
		static readonly Uri w3c_anonymous = new Uri (Constants.WsaAnonymousUri);
		EndpointAddress address;

		private EndpointAddress10 () // for serialization
		{
		}

		internal EndpointAddress10 (EndpointAddress address)
		{
			this.address = address;
		}
		
		public static EndpointAddress10 FromEndpointAddress (EndpointAddress address)
		{
			return new EndpointAddress10 (address);
		}

#if !NET_2_1
		public static XmlQualifiedName GetSchema (XmlSchemaSet xmlSchemaSet)
		{
			if (xmlSchemaSet == null)
				throw new ArgumentNullException ("xmlSchemaSet");
			xmlSchemaSet.Add (XmlSchema.Read (typeof (EndpointAddress10).Assembly.GetManifestResourceStream ("ws-addr.xsd"), null));
			return new XmlQualifiedName ("EndpointReferenceType", Constants.WsaNamespace);
		}
#endif

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
			address = EndpointAddress.ReadFrom (AddressingVersion.WSAddressing10, reader);
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			address.WriteContentsTo (AddressingVersion.WSAddressing10, writer);
		}
	}
}
