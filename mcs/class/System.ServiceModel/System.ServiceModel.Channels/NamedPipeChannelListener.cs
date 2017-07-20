//
// NamedPipeChannelListener.cs
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
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class NamedPipeChannelListener<TChannel> : InternalChannelListenerBase<TChannel> 
		where TChannel : class, IChannel
	{
		NamedPipeTransportBindingElement source;
		XmlDictionaryReaderQuotas quotas = null;
		BindingContext context;
		
		public NamedPipeChannelListener (NamedPipeTransportBindingElement source, BindingContext context)
			: base (context)
		{
			foreach (BindingElement be in context.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoder = CreateEncoder<TChannel> (mbe);
					quotas = mbe.GetProperty<XmlDictionaryReaderQuotas> (context);
					break;
				}
			}
			
			if (MessageEncoder == null)
				MessageEncoder = new BinaryMessageEncoder ();
		}

		NamedPipeServerStream active_server;
		AutoResetEvent server_release_handle = new AutoResetEvent (false);

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
Console.WriteLine ("NamedPipeChannelListener.OnAcceptChannel");

			if (active_server != null) {
				try {
					server_release_handle.WaitOne (timeout);
				} catch (TimeoutException) {
					return null;
				}
			}
Console.WriteLine ("NamedPipeChannelListener.OnAcceptChannel.2");

			var server = new NamedPipeServerStream (Uri.LocalPath.Substring (1).Replace ('/', '\\'), PipeDirection.InOut);
			active_server = server;

Console.WriteLine ("NamedPipeChannelListener.OnAcceptChannel.3");
			Action a = delegate {
				server.WaitForConnection ();
			};
			IAsyncResult r = a.BeginInvoke (null, null);
			try {
				r.AsyncWaitHandle.WaitOne (timeout);
			} catch (TimeoutException) {
				server.Close ();
				return null;
			}

			// FIXME: support IDuplexSessionChannel
			TChannel ch;
			if (typeof (TChannel) == typeof (IDuplexSessionChannel))
				throw new NotImplementedException ();
			else if (typeof (TChannel) == typeof (IReplyChannel))
				ch = (TChannel) (object) new NamedPipeReplyChannel (this, MessageEncoder, server);
			else
				throw new InvalidOperationException (String.Format ("Channel type {0} is not supported.", typeof (TChannel).Name));

			((CommunicationObject) (object) ch).Closed += delegate {
				active_server = null;
				server_release_handle.Set ();
			};
			return ch;
		}

		[MonoTODO]
		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		// CommunicationObject
		
		protected override void OnAbort ()
		{
		}

		protected override void OnClose (TimeSpan timeout)
		{
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (!Uri.IsLoopback)
				throw new NotSupportedException ("Only local namde pipes are supported in this binding");
		}
	}
}
