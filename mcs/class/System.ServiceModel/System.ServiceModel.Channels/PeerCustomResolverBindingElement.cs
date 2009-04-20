//
// PeerCustomResolverBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public class PeerCustomResolverBindingElement : PeerResolverBindingElement
	{
		public PeerCustomResolverBindingElement ()
		{
		}

		private PeerCustomResolverBindingElement (
			PeerCustomResolverBindingElement other)
			: base (other)
		{
			ReferralPolicy = other.ReferralPolicy;
		}

		public PeerCustomResolverBindingElement (BindingContext context, PeerCustomResolverSettings settings)
			: this (settings)
		{
			this.context = context;
		}

		public PeerCustomResolverBindingElement (PeerCustomResolverSettings settings)
		{
			this.settings = settings;
		}

		BindingContext context;
		PeerCustomResolverSettings settings;

		public override PeerReferralPolicy ReferralPolicy { get; set; }

		[MonoTODO]
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (
			BindingContext context)
		{
			return context.BuildInnerChannelFactory<TChannel> ();
		}

		[MonoTODO]
		public override IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingContext context)
		{
			return context.BuildInnerChannelListener<TChannel> ();
		}

		public override BindingElement Clone ()
		{
			return new PeerCustomResolverBindingElement (this);
		}

		[MonoTODO]
		public override PeerResolver CreatePeerResolver ()
		{
			if (settings != null)
				return settings.Resolver;

			// FIXME: create from configuration
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
