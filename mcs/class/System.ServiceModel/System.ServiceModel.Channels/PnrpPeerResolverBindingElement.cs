//
// PnrpPeerResolverBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
using System.Net.Security;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	[MonoTODO ("We aren't actually going to implement this windows-only protocol")]
	public class PnrpPeerResolverBindingElement : PeerResolverBindingElement
	{
		public PnrpPeerResolverBindingElement ()
		{
		}

		private PnrpPeerResolverBindingElement (
			PnrpPeerResolverBindingElement other)
			: base (other)
		{
			ReferralPolicy = other.ReferralPolicy;
		}

		public override PeerReferralPolicy ReferralPolicy { get; set; }

		[MonoTODO]
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override BindingElement Clone ()
		{
			return new PnrpPeerResolverBindingElement (this);
		}

		[MonoTODO]
		public override PeerResolver CreatePeerResolver ()
		{
			return new PnrpPeerResolver (this);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
	}

	class PnrpPeerResolver : PeerResolver
	{
		public PnrpPeerResolver (PnrpPeerResolverBindingElement binding)
		{
		}

		public override bool CanShareReferrals {
			get{ throw new NotImplementedException (); }
		}

		public override object Register (string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override ReadOnlyCollection<PeerNodeAddress> Resolve (string meshId, int maxAddresses, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override void Unregister (object registrationId, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public override void Update (object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
