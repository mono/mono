using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	[MonoTODO]
	public sealed class AnnouncementClient : ICommunicationObject, IDisposable
	{
		public AnnouncementClient ()
		{
		}

		public AnnouncementClient (AnnouncementEndpoint announcementEndpoint)
		{
		}

		public AnnouncementClient (string endpointConfigurationName)
		{
		}

		public ChannelFactory ChannelFactory { get; private set; }
		public ClientCredentials ClientCredentials { get; private set; }
		public ServiceEndpoint Endpoint { get; private set; }
		public IClientChannel InnerChannel { get; private set; }
		public DiscoveryMessageSequenceGenerator MessageSequenceGenerator { get; set; }

		CommunicationState ICommunicationObject.State {
			get { return InnerChannel.State; }
		}

		public event EventHandler<AsyncCompletedEventArgs> AnnounceOfflineCompleted;
		public event EventHandler<AsyncCompletedEventArgs> AnnounceOnlineCompleted;

		event EventHandler ICommunicationObject.Closed {
			add { InnerChannel.Closed += value; }
			remove { InnerChannel.Closed -= value; }
		}
		event EventHandler ICommunicationObject.Closing {
			add { InnerChannel.Closing += value; }
			remove { InnerChannel.Closing -= value; }
		}
		event EventHandler ICommunicationObject.Faulted {
			add { InnerChannel.Faulted += value; }
			remove { InnerChannel.Faulted -= value; }
		}
		event EventHandler ICommunicationObject.Opened {
			add { InnerChannel.Opened += value; }
			remove { InnerChannel.Opened -= value; }
		}
		event EventHandler ICommunicationObject.Opening {
			add { InnerChannel.Opening += value; }
			remove { InnerChannel.Opening -= value; }
		}

		public void AnnounceOffline (EndpointDiscoveryMetadata discoveryMetadata)
		{
			throw new NotImplementedException ();
		}

		public void AnnounceOfflineAsync (EndpointDiscoveryMetadata discoveryMetadata)
		{
			throw new NotImplementedException ();
		}

		public void AnnounceOfflineAsync (EndpointDiscoveryMetadata discoveryMetadata, object userState)
		{
			throw new NotImplementedException ();
		}

		public void AnnounceOnline (EndpointDiscoveryMetadata discoveryMetadata)
		{
			throw new NotImplementedException ();
		}

		public void AnnounceOnlineAsync (EndpointDiscoveryMetadata discoveryMetadata)
		{
			throw new NotImplementedException ();
		}

		public void AnnounceOnlineAsync (EndpointDiscoveryMetadata discoveryMetadata, object userState)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginAnnounceOffline (EndpointDiscoveryMetadata discoveryMetadata, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginAnnounceOnline (EndpointDiscoveryMetadata discoveryMetadata, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public void EndAnnounceOffline (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public void EndAnnounceOnline (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
			throw new NotImplementedException ();
		}

		public void Open ()
		{
			throw new NotImplementedException ();
		}

		// explicit interface impl.

		void ICommunicationObject.Open ()
		{
			InnerChannel.Open ();
		}

		void ICommunicationObject.Open (TimeSpan timeout)
		{
			InnerChannel.Open (timeout);
		}

		void ICommunicationObject.Close ()
		{
			InnerChannel.Close ();
		}

		void ICommunicationObject.Close (TimeSpan timeout)
		{
			InnerChannel.Close (timeout);
		}

		IAsyncResult ICommunicationObject.BeginOpen (AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (callback, state);
		}

		IAsyncResult ICommunicationObject.BeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (timeout, callback, state);
		}

		IAsyncResult ICommunicationObject.BeginClose (AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (callback, state);
		}

		IAsyncResult ICommunicationObject.BeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (timeout, callback, state);
		}

		void ICommunicationObject.EndOpen (IAsyncResult result)
		{
			InnerChannel.EndOpen (result);
		}

		void ICommunicationObject.EndClose (IAsyncResult result)
		{
			InnerChannel.EndClose (result);
		}

		void ICommunicationObject.Abort ()
		{
			InnerChannel.Abort ();
		}

		void IDisposable.Dispose ()
		{
			InnerChannel.Dispose ();
		}
	}
}
