//
// TransportBindingElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005, 2007 Novell, Inc.  http://www.novell.com
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
			TransportBindingElement other)
			: base (other)
		{
			manual_addressing = other.manual_addressing;
			max_buffer_pool_size = other.max_buffer_pool_size;
			max_recv_message_size = other.max_recv_message_size;
		}

		public bool ManualAddressing {
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
#if !NET_2_1
			if (typeof (T) == typeof (ChannelProtectionRequirements))
				// blank one, basically it should not be used
				// for any secure channels (
				return (T) (object) new ChannelProtectionRequirements ();
#endif
			if (typeof (T) == typeof (MessageVersion))
				return (T) (object) MessageVersion.Soap12WSAddressing10;
			return context.GetInnerProperty<T> ();
		}
	}
}
