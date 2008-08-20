//
// MetadataResolver.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.IO;
using System.Text;

using QName = System.Xml.XmlQualifiedName;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public static class MetadataResolver
	{
		public static ServiceEndpointCollection Resolve (
				Type contract,
				EndpointAddress address)
		{
			if (contract == null)
				throw new ArgumentNullException ("contract");

			return ResolveContracts (
					new ContractDescription [] {ContractDescription.GetContract (contract)},
					address, 
					MetadataExchangeClientMode.MetadataExchange);
		}

		public static ServiceEndpointCollection Resolve (
				Type contract,
				Uri address,
				MetadataExchangeClientMode mode)
		{
			return ResolveContracts (
					new ContractDescription [] {ContractDescription.GetContract (contract)},
					address, 
					mode);
		}

		public static ServiceEndpointCollection Resolve (
				IEnumerable<ContractDescription> contracts,
				EndpointAddress address)
		{
			return ResolveContracts (contracts, address, MetadataExchangeClientMode.MetadataExchange);
		}

		public static ServiceEndpointCollection Resolve (
				IEnumerable<ContractDescription> contracts,
				Uri address,
				MetadataExchangeClientMode mode)
		{
			return ResolveContracts (contracts, address, mode);
		}

		public static ServiceEndpointCollection Resolve (
				IEnumerable<ContractDescription> contracts,
				EndpointAddress address,
				MetadataExchangeClient client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			/* FIXME: client is used for what? */
			return ResolveContracts (contracts, address, MetadataExchangeClientMode.MetadataExchange);
		}

		/* FIXME: What is the mode/client used for here? 
		 * According to the tests, address is used */
		public static ServiceEndpointCollection Resolve (
				IEnumerable<ContractDescription> contracts,
				Uri address,
				MetadataExchangeClientMode mode,
				MetadataExchangeClient client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			return ResolveContracts (contracts, address, mode);
		}

		private static ServiceEndpointCollection ResolveContracts (
				IEnumerable<ContractDescription> contracts,
				Uri address,
				MetadataExchangeClientMode mode)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			return ResolveContracts (contracts, new EndpointAddress (address), mode);
		}

		private static ServiceEndpointCollection ResolveContracts (
				IEnumerable<ContractDescription> contracts,
				EndpointAddress address,
				MetadataExchangeClientMode mode)
		{
			if (contracts == null)
				throw new ArgumentNullException ("contracts");

			List<ContractDescription> list = new List<ContractDescription> (contracts);
			if (list.Count == 0)
				throw new ArgumentException ("There must be atleast one ContractDescription", "contracts");

			if (address == null)
				throw new ArgumentNullException ("address");

			MetadataExchangeClient client = new MetadataExchangeClient (address);
			MetadataSet metadata = client.GetMetadata ();
			WsdlImporter importer = new WsdlImporter (metadata);
			ServiceEndpointCollection endpoints = importer.ImportAllEndpoints ();
			
			ServiceEndpointCollection ret = new ServiceEndpointCollection ();

			foreach (ContractDescription contract in list) {
				Collection<ServiceEndpoint> colln = 
					endpoints.FindAll (new QName (contract.Name, contract.Namespace));

				for (int i = 0; i < colln.Count; i ++)
					ret.Add (colln [i]);
			}

			return ret;
		}
	}
}
