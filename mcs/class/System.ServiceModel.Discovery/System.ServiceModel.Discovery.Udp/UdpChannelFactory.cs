//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Discovery
{
	internal class UdpChannelFactory : ChannelFactoryBase<IDuplexChannel>
	{
		public UdpChannelFactory (UdpTransportBindingElement source, BindingContext ctx)
		{
			Source = source;
			Context = ctx;
		}
		
		protected override void OnOpen (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		protected override void OnClose (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		Action<TimeSpan> open_delegate, close_delegate;
		
		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}
		
		protected override void OnEndOpen (IAsyncResult result)
		{
			open_delegate.EndInvoke (result);
		}
		
		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}
		
		protected override void OnEndClose (IAsyncResult result)
		{
			close_delegate.EndInvoke (result);
		}
		
		protected override IDuplexChannel OnCreateChannel (EndpointAddress address, Uri via)
		{
			if (address.Uri.Scheme != "soap.udp")
				throw new ArgumentException (String.Format ("Unexpected endpoint address URI scheme: expected 'soap.udp' but got '{0}'", address.Uri.Scheme));
			return new UdpDuplexChannel (this, Context, address, via);
		}
		
		public UdpTransportBindingElement Source { get; private set; }
		public BindingContext Context { get; private set; }
	}
}
