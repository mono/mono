//
// WebChannelFactory.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

// This class does:
// - manual addressing support (with ChannelFactory, client will fail with
//   InvalidOperationException that claims missing manual addressing) in every
//   messages.

namespace System.ServiceModel.Web
{
	public class WebChannelFactory<TChannel> : ChannelFactory<TChannel>
	{
#if !MOBILE
		public WebChannelFactory ()
			: base ()
		{
		}

		public WebChannelFactory(Binding binding)
			: base(binding)
		{
		}

		public WebChannelFactory(ServiceEndpoint endpoint)
			: base(endpoint)
		{
		}
#endif

		public WebChannelFactory(Type channelType)
			: base (channelType)
		{
		}

		public WebChannelFactory (string endpointConfigurationName)
			: base (endpointConfigurationName)
		{
		}

		public WebChannelFactory (Uri remoteAddress)
			: this (String.Empty, remoteAddress)
		{
		}

		public WebChannelFactory (string endpointConfigurationName, Uri remoteAddress)
			: base (endpointConfigurationName)
		{
			Endpoint.Address = new EndpointAddress (remoteAddress);
		}

		public WebChannelFactory (Binding binding, Uri remoteAddress)
			: base (binding, new EndpointAddress (remoteAddress))
		{
		}

		protected override void OnOpening ()
		{
#if !MOBILE
			if (Endpoint.Behaviors.Find<WebHttpBehavior> () == null)
				Endpoint.Behaviors.Add (new WebHttpBehavior ());
#endif

			if (Endpoint.Binding == null)
				Endpoint.Binding = new WebHttpBinding ();

			base.OnOpening ();
		}
	}
}
