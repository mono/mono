//
// System.Runtime.Remoting.Channels.Tcp.TcpClientChannel.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

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

using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Threading;

namespace System.Runtime.Remoting.Channels.Tcp
{
	public class TcpClientChannel : IChannelSender, IChannel
	{
		int priority = 1;					
		string name = "tcp";
		IClientChannelSinkProvider _sinkProvider;
		
		public TcpClientChannel ()
		{
			_sinkProvider = new BinaryClientFormatterSinkProvider ();
			_sinkProvider.Next = new TcpClientTransportSinkProvider ();
		}

		public TcpClientChannel (IDictionary properties, IClientChannelSinkProvider sinkProvider)
		{
			if (properties != null) {
				object val = properties ["name"];
				if (val != null) name = val as string;
			
				val = properties ["priority"];
				if (val != null) priority = Convert.ToInt32 (val);
			}
			
			if (sinkProvider != null)
			{
				_sinkProvider = sinkProvider;

				// add the tcp provider at the end of the chain
				IClientChannelSinkProvider prov = sinkProvider;
				while (prov.Next != null) prov = prov.Next;
				prov.Next = new TcpClientTransportSinkProvider ();

				// Note: a default formatter is added only when
				// no sink providers are specified in the config file.
			}
			else
			{
				_sinkProvider = new BinaryClientFormatterSinkProvider ();
				_sinkProvider.Next = new TcpClientTransportSinkProvider ();
			}

		}

		public TcpClientChannel (string name, IClientChannelSinkProvider sinkProvider)
		{
			this.name = name;
			
			if (sinkProvider != null) {
				_sinkProvider = sinkProvider;
				
				// add the tcp provider at the end of the chain
				IClientChannelSinkProvider prov = sinkProvider;
				while (prov.Next != null)
					prov = prov.Next;
				prov.Next = new TcpClientTransportSinkProvider ();
			} else {
				_sinkProvider = new BinaryClientFormatterSinkProvider ();
				_sinkProvider.Next = new TcpClientTransportSinkProvider ();
			}
		}
		
		public string ChannelName
		{
			get {
				return name;
			}
		}

		public int ChannelPriority
		{
			get {
				return priority;
			}
		}

		public virtual IMessageSink CreateMessageSink (string url,
						       object remoteChannelData,
						       out string objectURI)
	    {
			if (url != null && Parse (url, out objectURI) != null)
				return (IMessageSink) _sinkProvider.CreateSink (this, url, remoteChannelData);
			
			if (remoteChannelData != null) {
				IChannelDataStore ds = remoteChannelData as IChannelDataStore;
				if (ds != null && ds.ChannelUris.Length > 0)
					url = ds.ChannelUris [0];
				else {
					objectURI = null;
					return null;
				}
			}

			if (Parse (url, out objectURI) == null)
				return null;

			return (IMessageSink) _sinkProvider.CreateSink (this, url, remoteChannelData);
		}

		public string Parse (string url, out string objectURI)
		{
			return TcpChannel.ParseChannelUrl (url, out objectURI);
		}
	}
}
