//
// NetPeerTcpBinding.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marcos Cobena (marcoscobena@gmail.com)
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
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
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	public class NetPeerTcpBinding : Binding,
		IBindingDeliveryCapabilities, IBindingMulticastCapabilities,
		ISecurityCapabilities, IBindingRuntimePreferences
	{
		long max_buffer_pool_size = 0x80000;
		long max_recv_message_size = 0x10000;
		bool msg_auth;
		int port;
		XmlDictionaryReaderQuotas reader_quotas;
//		PeerResolver resolver = new PeerResolverImpl ();
		PeerResolverSettings resolver = new PeerResolverSettings ();
		PeerSecuritySettings security = new PeerSecuritySettings ();

		public NetPeerTcpBinding ()
		{
		}

		[MonoTODO]
		public NetPeerTcpBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}

		public IPAddress ListenIPAddress { get; set; }

		public long MaxBufferPoolSize {
			get { return max_buffer_pool_size; }
			set { max_buffer_pool_size = value; }
		}

		public bool MessageAuthentication {
			get { return msg_auth; }
			set { msg_auth = value; }
		}

		public long MaxReceivedMessageSize {
			get { return max_recv_message_size; }
			set { max_recv_message_size = value; }
		}

		[MonoTODO]
		public int Port {
			get { return port; }
			set { port = value; }
		}

		[MonoTODO]
		public PeerResolverSettings Resolver {
			get { return resolver; }
			set { resolver = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set { reader_quotas = value; }
		}

		public override string Scheme {
			get { return "net.p2p"; }
		}
		
		public PeerSecuritySettings Security {
			get { return security; }
		}

		public EnvelopeVersion SoapVersion {
			get { return EnvelopeVersion.Soap12; }
		}

		public override BindingElementCollection
			CreateBindingElements ()
		{
			BinaryMessageEncodingBindingElement mbe = 
				new BinaryMessageEncodingBindingElement ();
			ReaderQuotas.CopyTo (mbe.ReaderQuotas);

			PeerTransportBindingElement tbe =
				new PeerTransportBindingElement ();
			tbe.ListenIPAddress = ListenIPAddress;
			tbe.MaxBufferPoolSize = MaxBufferPoolSize;
			tbe.MaxReceivedMessageSize = MaxReceivedMessageSize;
			tbe.MessageAuthentication = MessageAuthentication;

			return new BindingElementCollection (new BindingElement [] { mbe, tbe });
		}

		// explicit interface implementations

		[MonoTODO]
		bool IBindingDeliveryCapabilities.AssuresOrderedDelivery {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IBindingDeliveryCapabilities.QueuedDelivery {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { throw new NotImplementedException (); }
		}

		bool IBindingMulticastCapabilities.IsMulticast {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ProtectionLevel ISecurityCapabilities.SupportedRequestProtectionLevel {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		ProtectionLevel ISecurityCapabilities.SupportedResponseProtectionLevel {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ISecurityCapabilities.SupportsClientAuthentication {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ISecurityCapabilities.SupportsClientWindowsIdentity {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool ISecurityCapabilities.SupportsServerAuthentication {
			get { throw new NotImplementedException (); }
		}
	}
}
