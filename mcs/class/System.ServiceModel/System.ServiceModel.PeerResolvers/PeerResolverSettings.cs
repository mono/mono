// 
// PeerResolverSettings.cs
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
	public class PeerResolverSettings
	{
		PeerCustomResolverSettings custom = new PeerCustomResolverSettings ();
		// FIXME: Is it really by default Auto?
		PeerResolverMode mode = PeerResolverMode.Auto;
		
		public PeerResolverSettings ()
		{
		}

		public PeerCustomResolverSettings Custom {
			get { return custom; }
		}
		
		public PeerResolverMode Mode { get; set; }
		
		public PeerReferralPolicy ReferralPolicy { get; set; }

		internal BindingElement CreateBinding ()
		{
			switch (Mode) {
			case PeerResolverMode.Pnrp:
				return new PnrpPeerResolverBindingElement () { ReferralPolicy = this.ReferralPolicy };
			default:
				var be = Custom.CreateBinding ();
				be.ReferralPolicy = this.ReferralPolicy;
				return be;
			}
		}
	}
}
