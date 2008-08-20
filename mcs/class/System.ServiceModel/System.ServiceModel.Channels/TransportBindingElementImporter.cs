//
// TransportBindingElementImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public class TransportBindingElementImporter
		: IWsdlImportExtension, IPolicyImportExtension
	{
		public TransportBindingElementImporter ()
		{
		}

		void IWsdlImportExtension.BeforeImport (ServiceDescriptionCollection wsdlDocuments,
			XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
		{
		}

		void IWsdlImportExtension.ImportContract (WsdlImporter importer,
			WsdlContractConversionContext context)
		{
		}

		void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer,
			WsdlEndpointConversionContext context)
		{
			for (int i = 0; i < context.WsdlBinding.Extensions.Count; i ++) {
				if (context.WsdlBinding.Extensions [i] is SoapBinding) {
					SoapBinding transport = context.WsdlBinding.Extensions [i] as SoapBinding;
					if (transport.Transport != SoapBinding.HttpTransport)
						//FIXME: not http
						return;

					if (! (context.Endpoint.Binding is CustomBinding))
						//FIXME: 
						throw new Exception ();

					((CustomBinding) context.Endpoint.Binding).Elements.Add (new HttpTransportBindingElement ());
					//((CustomBinding) context.Endpoint.Binding).Scheme = "http";

					for (int j = 0; j < context.WsdlPort.Extensions.Count; j ++) {
						SoapAddressBinding address = context.WsdlPort.Extensions [j] as SoapAddressBinding;
						if (address == null)
							continue;

						context.Endpoint.Address = new EndpointAddress (address.Location);
						context.Endpoint.ListenUri = new Uri (address.Location);
					}

					break;
				}
			}
		}

		void IPolicyImportExtension.ImportPolicy (MetadataImporter importer,
			PolicyConversionContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
