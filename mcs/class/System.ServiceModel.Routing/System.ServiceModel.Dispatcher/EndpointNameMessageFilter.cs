using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Dispatcher
{
	public class EndpointNameMessageFilter : MessageFilter
	{
		public EndpointNameMessageFilter (string endpointName)
		{
			Name = endpointName;
		}

		internal string Name { get; private set; }

		[MonoTODO]
		public override bool Match (Message message)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Match (MessageBuffer buffer)
		{
			throw new NotImplementedException ();
		}
	}
}
