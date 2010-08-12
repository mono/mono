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
	public sealed class AnnouncementClient : ICommunicationObject, IDisposable
	{
		internal interface IAnnouncementCommon
		{
			IAsyncResult BeginAnnounceOnline (EndpointDiscoveryMetadata metadata, DiscoveryMessageSequence sequence, AsyncCallback callback, object state);
			void EndAnnounceOnline (IAsyncResult result);
			IAsyncResult BeginAnnounceOffline (EndpointDiscoveryMetadata metadata, DiscoveryMessageSequence sequence, AsyncCallback callback, object state);
			void EndAnnounceOffline (IAsyncResult result);
		}

		public AnnouncementClient ()
			: this (String.Empty)
		{
		}

		public AnnouncementClient (AnnouncementEndpoint announcementEndpoint)
		{
			if (announcementEndpoint == null)
				throw new ArgumentNullException ("announcementEndpoint");
			MessageSequenceGenerator = new DiscoveryMessageSequenceGenerator ();
			client = Activator.CreateInstance (announcementEndpoint.DiscoveryVersion.AnnouncementClientType, new object [] {announcementEndpoint});
		}

		public AnnouncementClient (string endpointConfigurationName)
		{
			throw new NotImplementedException ();
		}

		public DiscoveryMessageSequenceGenerator MessageSequenceGenerator { get; set; }

		// FIXME: make it dynamic (dmcs crashes for now)
		object client;

		public ChannelFactory ChannelFactory {
			get { return (ChannelFactory) client.GetType ().GetProperty ("ChannelFactory").GetValue (client, null); }
		}

		public ClientCredentials ClientCredentials {
			get { return ChannelFactory.Credentials; }
		}

		public ServiceEndpoint Endpoint {
			get { return ChannelFactory.Endpoint; }
		}

		public IClientChannel InnerChannel {
			get { return (IClientChannel) client.GetType ().GetProperty ("InnerChannel").GetValue (client, null); }
		}

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

		// open/close

		public void Open ()
		{
			InnerChannel.Open ();
		}

		public void Close ()
		{
			InnerChannel.Close ();
		}

		// sync

		public void AnnounceOffline (EndpointDiscoveryMetadata discoveryMetadata)
		{
			EndAnnounceOffline (BeginAnnounceOffline (discoveryMetadata, null, null));
		}

		public void AnnounceOnline (EndpointDiscoveryMetadata discoveryMetadata)
		{
			EndAnnounceOnline (BeginAnnounceOnline (discoveryMetadata, null, null));
		}

		// async

		public void AnnounceOfflineAsync (EndpointDiscoveryMetadata discoveryMetadata)
		{
			AnnounceOfflineAsync (discoveryMetadata, null);
		}

		public void AnnounceOfflineAsync (EndpointDiscoveryMetadata discoveryMetadata, object userState)
		{
			AsyncCallback cb = delegate (IAsyncResult result) {
				var st = (AnnouncementEventArgs) result.AsyncState;
				try {
					EndAnnounceOffline (result);
				} catch (Exception ex) {
					OnAnnounceOfflineCompleted (new AsyncCompletedEventArgs (ex, false, result.AsyncState));
				} finally {
					OnAnnounceOfflineCompleted (new AsyncCompletedEventArgs (null, false, result.AsyncState));
				}
			};
			BeginAnnounceOffline (discoveryMetadata, cb, userState);
		}

		void OnAnnounceOfflineCompleted (AsyncCompletedEventArgs args)
		{
			if (AnnounceOfflineCompleted != null)
				AnnounceOfflineCompleted (this, args);
		}

		public void AnnounceOnlineAsync (EndpointDiscoveryMetadata discoveryMetadata)
		{
			AnnounceOnlineAsync (discoveryMetadata, null);
		}

		public void AnnounceOnlineAsync (EndpointDiscoveryMetadata discoveryMetadata, object userState)
		{
			AsyncCallback cb = delegate (IAsyncResult result) {
				var st = (AnnouncementEventArgs) result.AsyncState;
				try {
					EndAnnounceOnline (result);
				} catch (Exception ex) {
					OnAnnounceOnlineCompleted (new AsyncCompletedEventArgs (ex, false, result.AsyncState));
				} finally {
					OnAnnounceOnlineCompleted (new AsyncCompletedEventArgs (null, false, result.AsyncState));
				}
			};
			BeginAnnounceOnline (discoveryMetadata, cb, userState);
		}

		void OnAnnounceOnlineCompleted (AsyncCompletedEventArgs args)
		{
			if (AnnounceOnlineCompleted != null)
				AnnounceOnlineCompleted (this, args);
		}

		// begin/end

		public IAsyncResult BeginAnnounceOffline (EndpointDiscoveryMetadata discoveryMetadata, AsyncCallback callback, object state)
		{
			return ((IAnnouncementCommon) client).BeginAnnounceOffline (discoveryMetadata, MessageSequenceGenerator.Next (), callback, state);
		}

		public IAsyncResult BeginAnnounceOnline (EndpointDiscoveryMetadata discoveryMetadata, AsyncCallback callback, object state)
		{
			return ((IAnnouncementCommon) client).BeginAnnounceOnline (discoveryMetadata, MessageSequenceGenerator.Next (), callback, state);
		}

		public void EndAnnounceOffline (IAsyncResult result)
		{
			((IAnnouncementCommon) client).EndAnnounceOffline (result);
		}

		public void EndAnnounceOnline (IAsyncResult result)
		{
			((IAnnouncementCommon) client).EndAnnounceOnline (result);
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
			ChannelFactory.Abort ();
		}

		void IDisposable.Dispose ()
		{
			Close ();
		}
	}
}
