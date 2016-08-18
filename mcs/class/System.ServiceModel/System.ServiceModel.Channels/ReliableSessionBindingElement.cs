//
// ReliableSessionBindingElement.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

namespace System.ServiceModel.Channels
{
	public sealed class ReliableSessionBindingElement : BindingElement
#if !MOBILE
		, IPolicyExportExtension
#endif
	{
		public ReliableSessionBindingElement ()
		{
			// FIXME: apply configuration
		}

		public ReliableSessionBindingElement (bool ordered)
			: this ()
		{
			Ordered = ordered;
		}

		[MonoTODO]
		public TimeSpan AcknowledgementInterval { get; set; }
		[MonoTODO]
		public bool FlowControlEnabled { get; set; }
		[MonoTODO]
		public TimeSpan InactivityTimeout { get; set; }
		[MonoTODO]
		public int MaxPendingChannels { get; set; }
		[MonoTODO]
		public int MaxRetryCount { get; set; }
		[MonoTODO]
		public int MaxTransferWindowSize { get; set; }
		[MonoTODO]
		public bool Ordered { get; set; }
		[MonoTODO]
		public ReliableMessagingVersion ReliableMessagingVersion { get; set; }

		[MonoTODO]
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
		
#if !MOBILE
		[MonoTODO]
		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}

		public override BindingElement Clone ()
		{
			return (ReliableSessionBindingElement) MemberwiseClone ();
		}

		public override T GetProperty<T> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
		
#if !MOBILE && !XAMMAC_4_5
		void IPolicyExportExtension.ExportPolicy (MetadataExporter exporter, PolicyConversionContext context)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
