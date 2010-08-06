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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Xml.Linq;

namespace System.ServiceModel.Discovery
{
	public class FindCriteria
	{
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
			MaxResults = int.MaxValue;
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

		[MonoTODO]
		public bool IsMatch (EndpointDiscoveryMetadata endpointDiscoveryMetadata)
		{
			throw new NotImplementedException ();
		}
	}
}
