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
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public sealed class DiscoveryClient : ICommunicationObject, IDisposable
	{
		internal interface IDiscoveryCommon
		{
			IAsyncResult BeginFind (FindCriteria criteria, AsyncCallback callback, object state);
			FindResponse EndFind (IAsyncResult result);
			IAsyncResult BeginResolve (ResolveCriteria criteria, AsyncCallback callback, object state);
			ResolveResponse EndResolve (IAsyncResult result);
		}
		
		public DiscoveryClient ()
			: this (String.Empty)
		{
		}

		public DiscoveryClient (DiscoveryEndpoint discoveryEndpoint)
		{
			if (discoveryEndpoint == null)
				throw new ArgumentNullException ("discoveryEndpoint");

			// create DiscoveryTargetClientXX for each version:
			// Managed -> DiscoveryTargetClientType (request-reply)
			// Adhoc   -> DiscoveryProxyClientType (duplex)
			if (discoveryEndpoint.DiscoveryMode == ServiceDiscoveryMode.Managed)
				client = Activator.CreateInstance (discoveryEndpoint.DiscoveryVersion.DiscoveryProxyClientType, new object [] {discoveryEndpoint});
			else
				client = Activator.CreateInstance (discoveryEndpoint.DiscoveryVersion.DiscoveryTargetClientType, new object [] {discoveryEndpoint});
		}

		public DiscoveryClient (string endpointConfigurationName)
		{
			throw new NotImplementedException ();
		}

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

		CommunicationState State {
			get { return ((ICommunicationObject) this).State; }
		}

		CommunicationState ICommunicationObject.State {
			get { return ((ICommunicationObject) client).State; }
		}

		public event EventHandler<FindCompletedEventArgs> FindCompleted;
		public event EventHandler<FindProgressChangedEventArgs> FindProgressChanged;
		public event EventHandler<AnnouncementEventArgs> ProxyAvailable;
		public event EventHandler<ResolveCompletedEventArgs> ResolveCompleted;

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

		public void Open ()
		{
			((ICommunicationObject) this).Open ();
		}

		public void Close ()
		{
			((ICommunicationObject) this).Close ();
		}

		bool cancelled;

		public void CancelAsync (object userState)
		{
			throw new NotImplementedException ();
		}

		// find

		public FindResponse Find (FindCriteria criteria)
		{
			return EndFind (BeginFind (criteria, null, null));
		}

		public void FindAsync (FindCriteria criteria)
		{
			FindAsync (criteria, null);
		}

		public void FindAsync (FindCriteria criteria, object userState)
		{
			AsyncCallback cb = delegate (IAsyncResult result) {
				FindResponse ret = null;
				Exception error = null;
				try {
					ret = EndFind (result);
				} catch (Exception ex) {
					error = ex;
				}
				OnFindCompleted (new FindCompletedEventArgs (ret, error, cancelled, result.AsyncState));
			};
			cancelled = false;
			BeginFind (criteria, cb, userState);
		}

		void OnFindCompleted (FindCompletedEventArgs args)
		{
			if (FindCompleted != null)
				FindCompleted (this, args);
		}

		IAsyncResult BeginFind (FindCriteria criteria, AsyncCallback callback, object state)
		{
			return ((IDiscoveryCommon) client).BeginFind (criteria, callback, state);
		}
		
		FindResponse EndFind (IAsyncResult result)
		{
			return ((IDiscoveryCommon) client).EndFind (result);
		}

		// resolve

		public ResolveResponse Resolve (ResolveCriteria criteria)
		{
			return EndResolve (BeginResolve (criteria, null, null));
		}

		public void ResolveAsync (ResolveCriteria criteria)
		{
			ResolveAsync (criteria, null);
		}

		public void ResolveAsync (ResolveCriteria criteria, object userState)
		{
			AsyncCallback cb = delegate (IAsyncResult result) {
				ResolveResponse ret = null;
				Exception error = null;
				try {
					ret = EndResolve (result);
				} catch (Exception ex) {
					error = ex;
				}
				OnResolveCompleted (new ResolveCompletedEventArgs (ret, error, cancelled, result.AsyncState));
			};
			cancelled = false;
			BeginResolve (criteria, cb, userState);
		}

		void OnResolveCompleted (ResolveCompletedEventArgs args)
		{
			if (ResolveCompleted != null)
				ResolveCompleted (this, args);
		}

		IAsyncResult BeginResolve (ResolveCriteria criteria, AsyncCallback callback, object state)
		{
			return ((IDiscoveryCommon) client).BeginResolve (criteria, callback, state);
		}
		
		ResolveResponse EndResolve (IAsyncResult result)
		{
			return ((IDiscoveryCommon) client).EndResolve (result);
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
