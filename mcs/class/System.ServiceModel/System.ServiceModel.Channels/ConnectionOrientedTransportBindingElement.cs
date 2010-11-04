//
// ConnectionOrientedTransportBindingElement.cs
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
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public abstract class ConnectionOrientedTransportBindingElement
		: TransportBindingElement, IPolicyExportExtension
	{
		int connection_buf_size = 0x2000, max_buf_size = 0x10000,
			max_pending_conn = 10, max_pending_accepts = 1;
		HostNameComparisonMode host_cmp_mode = HostNameComparisonMode.StrongWildcard;
		TimeSpan max_output_delay = TimeSpan.FromMilliseconds (200);
		TimeSpan ch_init_timeout = TimeSpan.FromSeconds (5);
		TransferMode transfer_mode = TransferMode.Buffered;

		internal ConnectionOrientedTransportBindingElement ()
		{
		}

		internal ConnectionOrientedTransportBindingElement (
			ConnectionOrientedTransportBindingElement other)
			: base (other)
		{
			connection_buf_size = other.connection_buf_size;
			max_buf_size = other.max_buf_size;
			max_pending_conn = other.max_pending_conn;
			max_pending_accepts = other.max_pending_accepts;
			host_cmp_mode = other.host_cmp_mode;
			max_output_delay = other.max_output_delay;
			transfer_mode = other.transfer_mode;
		}

		public TimeSpan ChannelInitializationTimeout {
			get { return ch_init_timeout; }
			set { ch_init_timeout = value; }
		}

		public int ConnectionBufferSize {
			get { return connection_buf_size; }
			set { connection_buf_size = value; }
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return host_cmp_mode; }
			set { host_cmp_mode = value; }
		}

		public int MaxBufferSize {
			get { return max_buf_size; }
			set { max_buf_size = value; }
		}

		public int MaxPendingConnections {
			get { return max_pending_conn; }
			set { max_pending_conn = value; }
		}

		public TimeSpan MaxOutputDelay {
			get { return max_output_delay; }
			set { max_output_delay = value; }
		}

		public int MaxPendingAccepts {
			get { return max_pending_accepts; }
			set { max_pending_accepts = value; }
		}

		public TransferMode TransferMode {
			get { return transfer_mode; }
			set { transfer_mode = value; }
		}
		
		public override bool CanBuildChannelFactory<TChannel> (
			BindingContext context)
		{
			switch (TransferMode) {
			case TransferMode.Buffered:
			case TransferMode.StreamedResponse:
				return typeof (TChannel) == typeof (IDuplexSessionChannel);
			case TransferMode.Streamed:
			case TransferMode.StreamedRequest:
				return typeof (TChannel) == typeof (IRequestChannel);
			}
			return false;
		}

		public override bool CanBuildChannelListener<TChannel> (
			BindingContext context)
		{
			switch (TransferMode) {
			case TransferMode.Buffered:
			case TransferMode.StreamedRequest:
				return typeof (TChannel) == typeof (IDuplexSessionChannel);
			case TransferMode.Streamed:
			case TransferMode.StreamedResponse:
				return typeof (TChannel) == typeof (IReplyChannel);
			}
			return false;
		}

		public override T GetProperty<T> (BindingContext context)
		{
			// since this class cannot be derived (internal .ctor
			// only), we cannot examine what this should do.
			// So, handle all properties in the derived types.
			return base.GetProperty<T> (context);
		}

		void IPolicyExportExtension.ExportPolicy (MetadataExporter exporter, PolicyConversionContext context)
		{
			if (exporter == null)
				throw new ArgumentNullException ("exporter");
			if (context == null)
				throw new ArgumentNullException ("context");

			PolicyAssertionCollection assertions = context.GetBindingAssertions ();
			XmlDocument doc = new XmlDocument ();

			assertions.Add (doc.CreateElement ("wsaw", "UsingAddressing", "http://www.w3.org/2006/05/addressing/wsdl"));
			assertions.Add (doc.CreateElement ("msb", "BinaryEncoding", "http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1"));

			if (transfer_mode == TransferMode.Streamed || transfer_mode == TransferMode.StreamedRequest ||
				transfer_mode == TransferMode.StreamedResponse)
				assertions.Add (doc.CreateElement ("msf", "Streamed", "http://schemas.microsoft.com/ws/2006/05/framing/policy"));
		}
	}
}
