// 
// PeerResolverSettings.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.ServiceModel.PeerResolvers;

namespace System.ServiceModel.PeerResolvers
{
	public class PeerResolverSettings
	{
		PeerCustomResolverSettings custom = new PeerCustomResolverSettings ();
		// FIXME: Is it really by default Auto?
		PeerResolverMode mode = PeerResolverMode.Auto;
		PeerReferralPolicy referral_policy;
		
		public PeerResolverSettings ()
		{
		}
		
		[MonoTODO]
		public PeerCustomResolverSettings Custom {
			get { return custom; }
		}
		
		[MonoTODO]
		public PeerResolverMode Mode {
			get { return mode; }
			set { mode = value; }
		}
		
		[MonoTODO]
		public PeerReferralPolicy ReferralPolicy {
			get { return referral_policy; }
			set { referral_policy = value; }
		}
	}
}
