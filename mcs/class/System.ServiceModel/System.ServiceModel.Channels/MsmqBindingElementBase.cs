//
// MsmqBindingElementBase.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel.Description;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public abstract class MsmqBindingElementBase : TransportBindingElement,
		ITransactedBindingElement, IPolicyExportExtension, IWsdlExportExtension
	{
		Uri custom_dead_letter_queue;
		DeadLetterQueue dead_letter_queue = DeadLetterQueue.System;
		bool durable = true, exactly_once = true, tx_enabled, use_msmq_trace, use_source_journal;
		int max_retry_cycles = 2, receive_retry_count = 5;
		ReceiveErrorHandling receive_error_handling;
		TimeSpan retry_cycle_delay = TimeSpan.FromMinutes (30), ttl = TimeSpan.FromDays (1);
		MsmqTransportSecurity transport_security =
			new MsmqTransportSecurity ();

		internal MsmqBindingElementBase ()
		{
		}

		public Uri CustomDeadLetterQueue {
			get { return custom_dead_letter_queue; }
			set { custom_dead_letter_queue = value; }
		}

		public DeadLetterQueue DeadLetterQueue {
			get { return dead_letter_queue; }
			set { dead_letter_queue = value; }
		}

		public bool Durable {
			get { return durable; }
			set { durable = value; }
		}

		public bool ExactlyOnce {
			get { return exactly_once; }
			set { exactly_once = value; }
		}

		public int MaxRetryCycles {
			get { return max_retry_cycles; }
			set { max_retry_cycles = value; }
		}

		public MsmqTransportSecurity MsmqTransportSecurity {
			get { return transport_security; }
		}

		public ReceiveErrorHandling ReceiveErrorHandling {
			get { return receive_error_handling; }
			set { receive_error_handling = value; }
		}

		public int ReceiveRetryCount {
			get { return receive_retry_count; }
			set { receive_retry_count = value; }
		}

		public TimeSpan RetryCycleDelay {
			get { return retry_cycle_delay; }
			set { retry_cycle_delay = value; }
		}

		public TimeSpan TimeToLive {
			get { return ttl; }
			set { ttl = value; }
		}

		public bool TransactedReceiveEnabled {
			get { return tx_enabled; }
		}

		public bool UseMsmqTracing {
			get { return use_msmq_trace; }
			set { use_msmq_trace = value; }
		}

		public bool UseSourceJournal {
			get { return use_source_journal; }
			set { use_source_journal = value; }
		}

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) is IBindingDeliveryCapabilities)
				throw new NotImplementedException ();
			if (typeof (T) is ISecurityCapabilities)
				throw new NotImplementedException ();
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
			assertions.Add (doc.CreateElement ("msmq", "Authenticated", "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
			assertions.Add (doc.CreateElement ("msb", "BinaryEncoding",
					"http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1"));

			if (transport_security.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain)
				assertions.Add (doc.CreateElement ("msmq", "WindowsDomain",
					"http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));

			if (!durable)
				assertions.Add (doc.CreateElement ("msmq", "MsmqVolatile", 
					"http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));

			if (!exactly_once)
				assertions.Add (doc.CreateElement ("msmq", "MsmqBestEffort", 
					"http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq"));
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportContract (WsdlExporter exporter, WsdlContractConversionContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IWsdlExportExtension.ExportEndpoint (WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
		{
			throw new NotImplementedException ();
		}
	}
}
