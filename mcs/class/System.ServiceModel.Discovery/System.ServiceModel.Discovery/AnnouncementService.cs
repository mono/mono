using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	[MonoTODO]
	[ServiceBehavior (InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class AnnouncementService
	{
		public AnnouncementService ()
		{
		}

		public AnnouncementService (int duplicateMessageHistoryLength)
		{
		}

		public event EventHandler<AnnouncementEventArgs> OfflineAnnouncementReceived;
		public event EventHandler<AnnouncementEventArgs> OnlineAnnouncementReceived;

		protected virtual IAsyncResult OnBeginOfflineAnnouncement (DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected virtual IAsyncResult OnBeginOnlineAnnouncement (DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnEndOfflineAnnouncement (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnEndOnlineAnnouncement (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
