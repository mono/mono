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
	public class NetPeerTcpBinding : Binding, IBindingRuntimePreferences
	{
		// We don't support PNRP
		public static bool IsPnrpAvailable {
			get { return false; }
		}

		XmlDictionaryReaderQuotas reader_quotas = new XmlDictionaryReaderQuotas ();
		PeerResolverSettings resolver = new PeerResolverSettings ();
		PeerSecuritySettings security = new PeerSecuritySettings ();
		PeerTransportBindingElement transport = new PeerTransportBindingElement ();

		public NetPeerTcpBinding ()
		{
		}

		[MonoTODO]
		public NetPeerTcpBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IPAddress ListenIPAddress {
			get { return transport.ListenIPAddress; }
			set { transport.ListenIPAddress = value; }
		}

		[MonoTODO]
		public long MaxBufferPoolSize {
			get { return transport.MaxBufferPoolSize; }
			set { transport.MaxBufferPoolSize = value; }
		}

		[MonoTODO]
		public long MaxReceivedMessageSize {
			get { return transport.MaxReceivedMessageSize; }
			set { transport.MaxReceivedMessageSize = value; }
		}

		public int Port {
			get { return transport.Port; }
			set { transport.Port = value; }
		}

		public PeerResolverSettings Resolver {
			get { return resolver; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				reader_quotas = value;
			}
		}

		public override string Scheme {
			get { return "net.p2p"; }
		}
		
		public PeerSecuritySettings Security {
			get { return security; }
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return EnvelopeVersion.Soap12; }
		}

		public override BindingElementCollection
			CreateBindingElements ()
		{
			var mbe = new BinaryMessageEncodingBindingElement ();
			if (ReaderQuotas != null)
				ReaderQuotas.CopyTo (mbe.ReaderQuotas);

			var prbe = Resolver.CreateBinding ();

			return new BindingElementCollection (new BindingElement [] { mbe, prbe, transport.Clone () });
		}

		// explicit interface implementations

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return false; }
		}
	}
}
