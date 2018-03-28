//
// TransportBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (C) 2005, 2007 Novell, Inc.  http://www.novell.com
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
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
using System.ServiceModel.Security;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public abstract class TransportBindingElement : BindingElement
	{
		bool manual_addressing;
		long max_buffer_pool_size = 0x80000;
		long max_recv_message_size = 0x10000;

		protected TransportBindingElement ()
		{
		}

		protected TransportBindingElement (
			TransportBindingElement elementToBeCloned)
			: base (elementToBeCloned)
		{
			manual_addressing = elementToBeCloned.manual_addressing;
			max_buffer_pool_size = elementToBeCloned.max_buffer_pool_size;
			max_recv_message_size = elementToBeCloned.max_recv_message_size;
		}

		public virtual bool ManualAddressing {
			get { return manual_addressing; }
			set { manual_addressing = value; }
		}

		public virtual long MaxBufferPoolSize {
			get { return max_buffer_pool_size; }
			set { max_buffer_pool_size = value; }
		}

		public virtual long MaxReceivedMessageSize {
			get { return max_recv_message_size; }
			set { max_recv_message_size = value; }
		}

		public abstract string Scheme { get; }

		public override T GetProperty<T> (BindingContext context)
		{
			if (typeof (T) == typeof (XmlDictionaryReaderQuotas)) {
				XmlDictionaryReaderQuotas q =
					new XmlDictionaryReaderQuotas ();
				q.MaxStringContentLength = (int) MaxReceivedMessageSize;
				return (T) (object) q;
			}
#if !MOBILE
			if (typeof (T) == typeof (ChannelProtectionRequirements))
				// blank one, basically it should not be used
				// for any secure channels (
				return (T) (object) new ChannelProtectionRequirements ();
#endif
			if (typeof (T) == typeof (MessageVersion))
				return (T) (object) MessageVersion.Soap12WSAddressing10;
			return context.GetInnerProperty<T> ();
		}

#if !MOBILE && !XAMMAC_4_5
		internal static XmlElement CreateTransportBinding (XmlElement transportToken)
		{
			var doc = new XmlDocument ();
			var transportBinding = doc.CreateElement (
				"sp", "TransportBinding", PolicyImportHelper.SecurityPolicyNS);
			
			var token = doc.CreateElement (
				"sp", "TransportToken", PolicyImportHelper.SecurityPolicyNS);
			PolicyImportHelper.AddWrappedPolicyElement (token, transportToken);
			
			var algorithmSuite = doc.CreateElement (
				"sp", "AlgorithmSuite", PolicyImportHelper.SecurityPolicyNS);
			var basic256 = doc.CreateElement (
				"sp", "Basic256", PolicyImportHelper.SecurityPolicyNS);
			PolicyImportHelper.AddWrappedPolicyElement (algorithmSuite, basic256);
			
			var layout = doc.CreateElement (
				"sp", "Layout", PolicyImportHelper.SecurityPolicyNS);
			var strict = doc.CreateElement (
				"sp", "Strict", PolicyImportHelper.SecurityPolicyNS);
			PolicyImportHelper.AddWrappedPolicyElement (layout, strict);
			
			PolicyImportHelper.AddWrappedPolicyElements (
				transportBinding, token, algorithmSuite, layout);
			
			return transportBinding;
		}

		internal static MessageEncodingBindingElement ExportAddressingPolicy (
			PolicyConversionContext context)
		{
			MessageEncodingBindingElement messageEncodingElement = null;
			foreach (var element in context.BindingElements) {
				var check = element as MessageEncodingBindingElement;
				if (check == null)
					continue;
				messageEncodingElement = check;
				break;
			}

			var doc = new XmlDocument ();
			var assertions = context.GetBindingAssertions ();
			
			if (messageEncodingElement == null) {
				assertions.Add (doc.CreateElement (
					"wsaw", "UsingAddressing",
					"http://www.w3.org/2006/05/addressing/wsdl"));
				return null;
			}

			var addressing = messageEncodingElement.MessageVersion.Addressing;
			if (addressing == AddressingVersion.WSAddressingAugust2004)
				assertions.Add (doc.CreateElement (
					"wsaw", "UsingAddressing",
					"http://schemas.xmlsoap.org/ws/2004/08/addressing/policy"));
			else if (addressing != AddressingVersion.None)
				assertions.Add (doc.CreateElement (
					"wsaw", "UsingAddressing",
					"http://www.w3.org/2006/05/addressing/wsdl"));

			return messageEncodingElement;
		}
#endif
	}
}
