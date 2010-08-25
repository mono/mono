//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public sealed class DiscoveryClientBindingElement : BindingElement
	{
		public static readonly EndpointAddress DiscoveryEndpointAddress = new EndpointAddress ("http://schemas.microsoft.com/discovery/dynamic");

		public DiscoveryClientBindingElement ()
		{
			DiscoveryEndpointProvider = DiscoveryEndpointProvider.CreateDefault ();
			FindCriteria = new FindCriteria (); // empty
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
			if (!(context.Binding.CreateBindingElements ().First () is DiscoveryClientBindingElement))
				throw new InvalidOperationException ("DiscoveryClientBindingElement is expected at the top of the input binding elements in the BindingContext");

			return new DiscoveryChannelFactory<TChannel> (this, context);
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelFactory<TChannel> ();
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override BindingElement Clone ()
		{
			return new DiscoveryClientBindingElement (DiscoveryEndpointProvider, FindCriteria);
		}

		public override T GetProperty<T> (BindingContext context)
		{
			return context.GetInnerProperty<T> ();
		}
	}
}
