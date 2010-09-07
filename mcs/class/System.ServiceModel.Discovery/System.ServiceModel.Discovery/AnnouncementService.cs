//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Discovery.Version11;
using System.ServiceModel.Discovery.VersionApril2005;
using System.ServiceModel.Discovery.VersionCD1;
using System.Threading;

namespace System.ServiceModel.Discovery
{
	// To be usable as ServiceType, it must implement contract interfaces.
	// However, there is no public interfaces, so they must be internal.
	[ServiceBehavior (InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class AnnouncementService : IAnnouncementContract11, IAnnouncementContractApril2005, IAnnouncementContractCD1
	{
		public AnnouncementService ()
			: this (0)
		{
		}

		public AnnouncementService (int duplicateMessageHistoryLength)
		{
		}

		public event EventHandler<AnnouncementEventArgs> OfflineAnnouncementReceived;
		public event EventHandler<AnnouncementEventArgs> OnlineAnnouncementReceived;

		Action<object,AnnouncementEventArgs> online, offline;

		protected virtual IAsyncResult OnBeginOfflineAnnouncement (DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				if (offline == null)
					offline = new Action<object,AnnouncementEventArgs> (OfflineAnnouncementReceived ?? delegate {});
				return offline.BeginInvoke (this, new AnnouncementEventArgs (endpointDiscoveryMetadata, messageSequence), callback, state);
			default:
				if (OfflineAnnouncementReceived != null)
					OfflineAnnouncementReceived (this, new AnnouncementEventArgs (endpointDiscoveryMetadata, messageSequence));
				var result = new TempAsyncResult (null, state);
				if (callback != null)
					callback (result);
				return result;
			}
		}

		protected virtual IAsyncResult OnBeginOnlineAnnouncement (DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				if (online == null)
					online = new Action<object,AnnouncementEventArgs> (OnlineAnnouncementReceived ?? delegate {});
				return online.BeginInvoke (this, new AnnouncementEventArgs (endpointDiscoveryMetadata, messageSequence), callback, state);
			default:
				if (OnlineAnnouncementReceived != null)
					OnlineAnnouncementReceived (this, new AnnouncementEventArgs (endpointDiscoveryMetadata, messageSequence));
				var result = new TempAsyncResult (null, state);
				if (callback != null)
					callback (result);
				return result;
			}
		}

		protected virtual void OnEndOfflineAnnouncement (IAsyncResult result)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				offline.EndInvoke (result);
				break;
			default:
				// do nothing
				break;
			}
		}

		protected virtual void OnEndOnlineAnnouncement (IAsyncResult result)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				online.EndInvoke (result);
				break;
			default:
				// do nothing
				break;
			}
		}

		// BeginOnlineAnnouncement

		IAsyncResult IAnnouncementContract11.BeginOnlineAnnouncement (MessageContracts11.OnlineAnnouncement msg, AsyncCallback callback, object state)
		{
			var msq = msg.MessageSequence != null ? msg.MessageSequence.ToDiscoveryMessageSequence () : null;
			var edm = msg.EndpointDiscoveryMetadata != null ? msg.EndpointDiscoveryMetadata.ToEndpointDiscoveryMetadata () : null;
			return OnBeginOnlineAnnouncement (msq, edm, callback, state);
		}

		IAsyncResult IAnnouncementContractApril2005.BeginOnlineAnnouncement (MessageContractsApril2005.OnlineAnnouncement msg, AsyncCallback callback, object state)
		{
			var msq = msg.MessageSequence != null ? msg.MessageSequence.ToDiscoveryMessageSequence () : null;
			var edm = msg.EndpointDiscoveryMetadata != null ? msg.EndpointDiscoveryMetadata.ToEndpointDiscoveryMetadata () : null;
			return OnBeginOnlineAnnouncement (msq, edm, callback, state);
		}

		IAsyncResult IAnnouncementContractCD1.BeginOnlineAnnouncement (MessageContractsCD1.OnlineAnnouncement msg, AsyncCallback callback, object state)
		{
			var msq = msg.MessageSequence != null ? msg.MessageSequence.ToDiscoveryMessageSequence () : null;
			var edm = msg.EndpointDiscoveryMetadata != null ? msg.EndpointDiscoveryMetadata.ToEndpointDiscoveryMetadata () : null;
			return OnBeginOnlineAnnouncement (msq, edm, callback, state);
		}

		// EndOnlineAnnouncement

		void IAnnouncementContract11.EndOnlineAnnouncement (IAsyncResult result)
		{
			OnEndOnlineAnnouncement (result);
		}

		void IAnnouncementContractApril2005.EndOnlineAnnouncement (IAsyncResult result)
		{
			OnEndOnlineAnnouncement (result);
		}

		void IAnnouncementContractCD1.EndOnlineAnnouncement (IAsyncResult result)
		{
			OnEndOnlineAnnouncement (result);
		}

		// BeginOfflineAnnouncement

		IAsyncResult IAnnouncementContract11.BeginOfflineAnnouncement (MessageContracts11.OfflineAnnouncement msg, AsyncCallback callback, object state)
		{
			var msq = msg.MessageSequence != null ? msg.MessageSequence.ToDiscoveryMessageSequence () : null;
			var edm = msg.EndpointDiscoveryMetadata != null ? msg.EndpointDiscoveryMetadata.ToEndpointDiscoveryMetadata () : null;
			return OnBeginOfflineAnnouncement (msq, edm, callback, state);
		}

		IAsyncResult IAnnouncementContractApril2005.BeginOfflineAnnouncement (MessageContractsApril2005.OfflineAnnouncement msg, AsyncCallback callback, object state)
		{
			var msq = msg.MessageSequence != null ? msg.MessageSequence.ToDiscoveryMessageSequence () : null;
			var edm = msg.EndpointDiscoveryMetadata != null ? msg.EndpointDiscoveryMetadata.ToEndpointDiscoveryMetadata () : null;
			return OnBeginOfflineAnnouncement (msq, edm, callback, state);
		}

		IAsyncResult IAnnouncementContractCD1.BeginOfflineAnnouncement (MessageContractsCD1.OfflineAnnouncement msg, AsyncCallback callback, object state)
		{
			var msq = msg.MessageSequence != null ? msg.MessageSequence.ToDiscoveryMessageSequence () : null;
			var edm = msg.EndpointDiscoveryMetadata != null ? msg.EndpointDiscoveryMetadata.ToEndpointDiscoveryMetadata () : null;
			return OnBeginOfflineAnnouncement (msq, edm, callback, state);
		}

		// EndOfflineAnnouncement

		void IAnnouncementContract11.EndOfflineAnnouncement (IAsyncResult result)
		{
			OnEndOfflineAnnouncement (result);
		}

		void IAnnouncementContractApril2005.EndOfflineAnnouncement (IAsyncResult result)
		{
			OnEndOfflineAnnouncement (result);
		}

		void IAnnouncementContractCD1.EndOfflineAnnouncement (IAsyncResult result)
		{
			OnEndOfflineAnnouncement (result);
		}
	}

	// FIXME: It is introduced for a workaround for (similar to) bug #633945. Remove this class and all of its usage once that bug gets fixed.
	class TempAsyncResult : IAsyncResult
	{
		public TempAsyncResult (object returnValue, object state)
		{
			ReturnValue = returnValue;
			AsyncState = state;
			CompletedSynchronously = true;
			IsCompleted = true;
			AsyncWaitHandle = new ManualResetEvent (true);
		}
		
		public object ReturnValue { get; set; }
		public object AsyncState { get; set; }
		public bool CompletedSynchronously { get; set; }
		public bool IsCompleted { get; set; }
		public WaitHandle AsyncWaitHandle { get; set; }
	}
}
