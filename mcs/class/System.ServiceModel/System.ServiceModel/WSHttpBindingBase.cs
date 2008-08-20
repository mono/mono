//
// WSHttpBindingBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	[MonoTODO]
	public abstract class WSHttpBindingBase : Binding, 
		IBindingRuntimePreferences
	{
		bool bypass_proxy_on_local, reliable_session_enabled;
		HostNameComparisonMode host_name_comparison_mode
			= HostNameComparisonMode.StrongWildcard;
		// FIXME: could be configurable
		long max_buffer_pool_size = 0x80000;
		// FIXME: could be configurable
		long max_recv_msg_size = 0x10000;
		WSMessageEncoding message_encoding
			= WSMessageEncoding.Text;
		Uri proxy_address;
		XmlDictionaryReaderQuotas reader_quotas
			= new XmlDictionaryReaderQuotas ();
		OptionalReliableSession reliable_session;
		// FIXME: could be configurable.
		EnvelopeVersion env_version = EnvelopeVersion.Soap12;
		Encoding text_encoding = new UTF8Encoding ();
		bool transaction_flow;
		bool use_default_web_proxy = true;

		ReliableSessionBindingElement rel_element =
			new ReliableSessionBindingElement ();

		protected WSHttpBindingBase ()
			: this (false)
		{
		}

		protected WSHttpBindingBase (bool reliableSessionEnabled)
		{
			reliable_session = new OptionalReliableSession (rel_element);
			reliable_session.Enabled = reliableSessionEnabled;
		}

		internal WSHttpBindingBase (WSHttpBindingBaseElement config)
			: this (config.ReliableSession.Enabled)
		{
			BypassProxyOnLocal = config.BypassProxyOnLocal;
			HostNameComparisonMode = config.HostNameComparisonMode;
			MaxBufferPoolSize = config.MaxBufferPoolSize;
			MaxReceivedMessageSize = config.MaxReceivedMessageSize;
			MessageEncoding = config.MessageEncoding;
			ProxyAddress = config.ProxyAddress;
			// ReaderQuotas = config.ReaderQuotas;

			TextEncoding = config.TextEncoding;
			TransactionFlow = config.TransactionFlow;
			UseDefaultWebProxy = config.UseDefaultWebProxy;
			throw new NotImplementedException ();
		}

		public bool BypassProxyOnLocal {
			get { return bypass_proxy_on_local; }
			set { bypass_proxy_on_local = value; }
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return host_name_comparison_mode; }
			set { host_name_comparison_mode = value; }
		}

		public long MaxBufferPoolSize {
			get { return max_buffer_pool_size; }
			set { max_buffer_pool_size = value; }
		}

		public long MaxReceivedMessageSize {
			get { return max_recv_msg_size; }
			set { max_recv_msg_size = value; }
		}

		public WSMessageEncoding MessageEncoding {
			get { return message_encoding; }
			set { message_encoding = value; }
		}

		public Uri ProxyAddress {
			get { return proxy_address; }
			set { proxy_address = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set { reader_quotas = value; }
		}

		public override string Scheme {
			get { return GetTransport ().Scheme; }
		}

		public OptionalReliableSession ReliableSession {
			get { return reliable_session; }
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return env_version; }
		}

		public Encoding TextEncoding {
			get { return text_encoding; }
			set { text_encoding = value; }
		}

		public bool TransactionFlow {
			get { return transaction_flow; }
			set { transaction_flow = value; }
		}

		public bool UseDefaultWebProxy {
			get { return use_default_web_proxy; }
			set { use_default_web_proxy = value; }
		}

		[MonoTODO]
		public override BindingElementCollection
			CreateBindingElements ()
		{
			BindingElement tx = new TransactionFlowBindingElement (TransactionProtocol.WSAtomicTransactionOctober2004);
			SecurityBindingElement sec = CreateMessageSecurity ();
			BindingElement msg = null;
			MessageVersion msgver = MessageVersion.CreateVersion (EnvelopeVersion, AddressingVersion.WSAddressing10);
			switch (MessageEncoding) {
			case WSMessageEncoding.Mtom:
				msg = new MtomMessageEncodingBindingElement (msgver, TextEncoding);
				break;
			case WSMessageEncoding.Text:
				msg = new TextMessageEncodingBindingElement (msgver, TextEncoding);
				break;
			default:
				throw new NotImplementedException ("mhm, another WSMessageEncoding?");
			}
			BindingElement tr = GetTransport ();
			List<BindingElement> list = new List<BindingElement> ();
			list.Add (tx);
			if (sec != null)
				list.Add (sec);
			list.Add (msg);
			if (tr != null)
				list.Add (tr);
			return new BindingElementCollection (list.ToArray ());
		}

		protected abstract SecurityBindingElement CreateMessageSecurity ();

		protected abstract TransportBindingElement GetTransport ();

		// explicit interface implementations

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { return false; }
		}
	}
}
