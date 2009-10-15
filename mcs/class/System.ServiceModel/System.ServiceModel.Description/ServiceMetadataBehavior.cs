//
// ServiceMetadataBehavior.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain  <jankit@novell.com>
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
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class ServiceMetadataBehavior : IServiceBehavior
	{
		public const string MexContractName = "IMetadataExchange";

		MetadataExporter exporter;

		public ServiceMetadataBehavior ()
		{
		}

		public bool HttpGetEnabled { get; set; }

		public bool HttpsGetEnabled { get; set; }

		public MetadataExporter MetadataExporter {
			get { return exporter ?? (exporter = new WsdlExporter ()); }
			set { exporter = value; }
		}

		public Uri ExternalMetadataLocation { get; set; }

		public Uri HttpGetUrl { get; set; }

		public Uri HttpsGetUrl { get; set; }

		public Binding HttpGetBinding { get; set; }

		public Binding HttpsGetBinding { get; set; }

		void IServiceBehavior.AddBindingParameters (
			ServiceDescription description,
			ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection parameters)
		{
		}

		void IServiceBehavior.ApplyDispatchBehavior (
			ServiceDescription description,
			ServiceHostBase serviceHostBase) {

			ServiceMetadataExtension sme = ServiceMetadataExtension.EnsureServiceMetadataExtension (serviceHostBase);

			//Find ChannelDispatcher for Mex, and add a MexInstanceContextProvider
			//to it
			foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers) {
				ChannelDispatcher cd = cdb as ChannelDispatcher;
				if (cd == null)
					continue;

				foreach (EndpointDispatcher ed in cd.Endpoints) {
					if (ed.ContractName == MexContractName)
						ed.DispatchRuntime.InstanceContextProvider = new MexInstanceContextProvider (serviceHostBase);
				}
			}

			if (HttpGetEnabled) {
				Uri uri = serviceHostBase.CreateUri ("http", HttpGetUrl);
				if (uri != null)
					sme.EnsureChannelDispatcher (true, "http", uri, HttpGetBinding);
			}

			if (HttpsGetEnabled) {
				Uri uri = serviceHostBase.CreateUri ("https", HttpsGetUrl);
				if (uri != null)
					sme.EnsureChannelDispatcher (true, "https", uri, HttpsGetBinding);
			}
		}

		[MonoTODO]
		void IServiceBehavior.Validate (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{			
		}
	}
}
