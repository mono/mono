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
	public sealed class DiscoveryClientBindingElement : BindingElement
	{
		public static readonly EndpointAddress DiscoveryEndpointAddress = new EndpointAddress ("http://schemas.microsoft.com/discovery/dynamic");

		public DiscoveryClientBindingElement ()
		{
		}

		public DiscoveryClientBindingElement (DiscoveryEndpointProvider discoveryEndpointProvider, FindCriteria findCriteria)
		{
			if (discoveryEndpointProvider == null)
				throw new ArgumentNullException ("discoveryEndpointProvider");
			if (findCriteria == null)
				throw new ArgumentNullException ("findCriteria");

			DiscoveryEndpointProvider = discoveryEndpointProvider;
			FindCriteria = findCriteria;
		}

		public DiscoveryEndpointProvider DiscoveryEndpointProvider { get; set; }
		public FindCriteria FindCriteria { get; set; }

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override BindingElement Clone ()
		{
			throw new NotImplementedException ();
		}

		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
