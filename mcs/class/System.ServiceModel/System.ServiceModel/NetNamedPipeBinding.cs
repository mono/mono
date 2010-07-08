//
// NetNamedPipeBinding.cs
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
using System.Net;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	public class NetNamedPipeBinding : Binding, IBindingRuntimePreferences
	{
		// We don't support PNRP
		public static bool IsPnrpAvailable {
			get { return false; }
		}

		XmlDictionaryReaderQuotas reader_quotas = new XmlDictionaryReaderQuotas ();
		NetNamedPipeSecurity security;
		NamedPipeTransportBindingElement transport = new NamedPipeTransportBindingElement ();

		public NetNamedPipeBinding ()
			: this (NetNamedPipeSecurityMode.None)
		{
		}

		public NetNamedPipeBinding (NetNamedPipeSecurityMode securityMode)
		{
			security = new NetNamedPipeSecurity () { Mode = securityMode };
		}

		[MonoTODO]
		public NetNamedPipeBinding (string configurationName)
		{
			throw new NotImplementedException ();
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return EnvelopeVersion.Soap12; }
		}

		[MonoTODO]
		public HostNameComparisonMode HostNameComparisonMode {
			get { return transport.HostNameComparisonMode; }
			set { transport.HostNameComparisonMode = value; }
		}

		[MonoTODO]
		public long MaxBufferPoolSize {
			get { return transport.MaxBufferPoolSize; }
			set { transport.MaxBufferPoolSize = value; }
		}

		[MonoTODO]
		public int MaxBufferSize {
			get { return transport.MaxBufferSize; }
			set { transport.MaxBufferSize = value; }
		}

		[MonoTODO]
		public int MaxConnections { get; set; }

		[MonoTODO]
		public long MaxReceivedMessageSize {
			get { return transport.MaxReceivedMessageSize; }
			set { transport.MaxReceivedMessageSize = value; }
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
			get { return "net.pipe"; }
		}
		
		public NetNamedPipeSecurity Security {
			get { return security; }
		}

		[MonoTODO]
		public bool TransactionFlow { get; set; }

		[MonoTODO]
		public TransactionProtocol TransactionProtocol { get; set; }

		public TransferMode TransferMode {
			get { return transport.TransferMode; }
			set { transport.TransferMode = value; }
		}

		public override BindingElementCollection CreateBindingElements ()
		{
			var mbe = new BinaryMessageEncodingBindingElement ();
			if (ReaderQuotas != null)
				ReaderQuotas.CopyTo (mbe.ReaderQuotas);

			return new BindingElementCollection (new BindingElement [] { mbe, transport.Clone () });
		}

		// explicit interface implementations

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return false; }
		}
	}
}
