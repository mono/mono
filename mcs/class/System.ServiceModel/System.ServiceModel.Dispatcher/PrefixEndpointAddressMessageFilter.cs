//
// PrefixEndpointAddressMessageFilter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Globalization;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	public class PrefixEndpointAddressMessageFilter : MessageFilter
	{
		EndpointAddress address;
		bool cmp_host;

		public PrefixEndpointAddressMessageFilter (EndpointAddress address)
			: this (address, false)
		{
		}

		public PrefixEndpointAddressMessageFilter (EndpointAddress address,
			bool includeHostNameInComparison)
		{
			this.address = address;
			cmp_host = includeHostNameInComparison;
		}

		public EndpointAddress Address {
			get { return address; }
		}

		public bool IncludeHostNameInComparison {
			get { return cmp_host; }
		}

		[MonoTODO]
		protected internal override IMessageFilterTable<FilterData>
			CreateFilterTable<FilterData> ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Match (Message message)
		{
			Uri to = message.Headers.To;
			if (to == null)
				return false;
			if (to.ToString () == Constants.WsaAnonymousUri || to.Equals (EndpointAddress.AnonymousUri) || to.Equals (EndpointAddress.NoneUri))
				return true;

			bool path = CultureInfo.InvariantCulture.CompareInfo.IsPrefix (to.AbsolutePath, address.Uri.AbsolutePath, CompareOptions.Ordinal);
			bool host = IncludeHostNameInComparison
					? (String.CompareOrdinal (to.Host, address.Uri.Host) == 0)
					: true;

			return path && host;
		}

		public override bool Match (MessageBuffer messageBuffer)
		{
			if (messageBuffer == null)
				throw new ArgumentNullException ("messageBuffer");
			return Match (messageBuffer.CreateMessage ());
		}
	}
}
