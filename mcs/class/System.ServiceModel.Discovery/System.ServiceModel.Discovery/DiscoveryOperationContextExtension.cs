using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	[MonoTODO]
	public class DiscoveryOperationContextExtension : IExtension<OperationContext>
	{
		internal DiscoveryOperationContextExtension (DiscoveryEndpoint endpoint)
		{
			this.endpoint = endpoint;
		}
		
		DiscoveryEndpoint endpoint;

		public ServiceDiscoveryMode DiscoveryMode {
			get { return endpoint.DiscoveryMode; }
		}

		public DiscoveryVersion DiscoveryVersion {
			get { return endpoint.DiscoveryVersion; }
		}

		public TimeSpan MaxResponseDelay {
			get { return endpoint.MaxResponseDelay; }
			internal set { endpoint.MaxResponseDelay = value; }
		}

		void IExtension<OperationContext>.Attach (OperationContext owner)
		{
		}

		void IExtension<OperationContext>.Detach (OperationContext owner)
		{
		}
	}
}
