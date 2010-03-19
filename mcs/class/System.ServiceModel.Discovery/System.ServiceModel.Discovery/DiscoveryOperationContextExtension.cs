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
		internal DiscoveryOperationContextExtension ()
		{
		}

		void IExtension<OperationContext>.Attach (OperationContext owner)
		{
		}

		void IExtension<OperationContext>.Detach (OperationContext owner)
		{
		}
	}
}
