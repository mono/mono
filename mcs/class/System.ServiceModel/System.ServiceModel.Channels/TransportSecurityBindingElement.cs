//
// TransportSecurityBindingElement.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Channels
{
	public sealed class TransportSecurityBindingElement
#if MOBILE
		: SecurityBindingElement
#else
		: SecurityBindingElement, IPolicyExportExtension
#endif
	{
		public TransportSecurityBindingElement ()
		{
		}

		private TransportSecurityBindingElement (
			TransportSecurityBindingElement other)
			: base (other)
		{
		}

		public override BindingElement Clone ()
		{
			return new TransportSecurityBindingElement (this);
		}

		[MonoTODO]
		protected override IChannelFactory<TChannel>
			BuildChannelFactoryCore<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

#if !MOBILE && !XAMMAC_4_5
		[MonoTODO]
		protected override IChannelListener<TChannel>
			BuildChannelListenerCore<TChannel> (
			BindingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IPolicyExportExtension.ExportPolicy (
			MetadataExporter exporter,
			PolicyConversionContext policyContext)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
