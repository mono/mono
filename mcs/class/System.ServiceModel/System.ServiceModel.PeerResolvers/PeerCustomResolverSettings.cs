// 
// PeerCustomResolverSettings.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.ServiceModel.Channels;

namespace System.ServiceModel.PeerResolvers
{
	public class PeerCustomResolverSettings
	{
		public PeerCustomResolverSettings ()
		{
		}
		
		public EndpointAddress Address { get; set; }
		
		public Binding Binding { get; set; }
		
		public bool IsBindingSpecified {
			get { return Binding != null; }
		}
		
		public PeerResolver Resolver { get; set; }

		internal PeerCustomResolverBindingElement CreateBinding ()
		{
			return new PeerCustomResolverBindingElement (this);
		}

		internal PeerCustomResolverSettings Clone ()
		{
			return new PeerCustomResolverSettings () {
				Binding = this.Binding,
				Address = this.Address,
				Resolver = this.Resolver
				};
		}
	}
}
