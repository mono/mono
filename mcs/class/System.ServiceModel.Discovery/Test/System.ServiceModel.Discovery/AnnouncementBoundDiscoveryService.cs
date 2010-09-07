//
// Author: Atsushi Enomoto <atsushi@ximian.com>
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	[ServiceBehavior (InstanceContextMode = InstanceContextMode.Single)]
	public class AnnouncementBoundDiscoveryService : DiscoveryService, IDisposable
	{
		ServiceHost ahost;

		public AnnouncementBoundDiscoveryService (AnnouncementEndpoint aendpoint)
		{
			var ans = new AnnouncementService ();
			ans.OnlineAnnouncementReceived += RegisterEndpoint;
			ans.OfflineAnnouncementReceived += UnregisterEndpoint;
			ahost = new ServiceHost (ans);
			ahost.AddServiceEndpoint (aendpoint);
			ahost.Open ();
			foreach (var cd in ahost.ChannelDispatchers)
				TextWriter.Null.WriteLine ("AnnouncementService.ChannelDispatcher " + cd.Listener.Uri);
		}

		public void Dispose ()
		{
			if (ahost.State == CommunicationState.Opened)
				ahost.Close ();
		}

		Action<FindRequestContext> find_delegate;
		Func<ResolveCriteria,EndpointDiscoveryMetadata> resolve_delegate;

		protected override IAsyncResult OnBeginFind (FindRequestContext findRequestContext, AsyncCallback callback, object state)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				if (find_delegate == null)
					find_delegate = new Action<FindRequestContext> (Find);
				return find_delegate.BeginInvoke (findRequestContext, callback, state);
			default:
				Find (findRequestContext);
				var result = new TempAsyncResult (null, state);
				if (callback != null)
					callback (result);
				return result;
			}
		}
		
		Queue<DiscoveryMessageSequence> sequences = new Queue<DiscoveryMessageSequence> ();
		List<EndpointDiscoveryMetadata> endpoints = new List<EndpointDiscoveryMetadata> ();

		bool PushQueueItem (DiscoveryMessageSequence seq)
		{
			if (sequences.Contains (seq))
				return false;
			sequences.Enqueue (seq);
			if (sequences.Count > 20)
				sequences.Dequeue ();
			return true;
		}

		void RegisterEndpoint (object obj, AnnouncementEventArgs e)
		{
			if (!PushQueueItem (e.MessageSequence))
				return;
			endpoints.Add (e.EndpointDiscoveryMetadata);
		}

		void UnregisterEndpoint (object obj, AnnouncementEventArgs e)
		{
			if (!PushQueueItem (e.MessageSequence))
				return;
			endpoints.Remove (e.EndpointDiscoveryMetadata);
		}

		protected override void OnEndFind (IAsyncResult result)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				find_delegate.EndInvoke (result);
				break;
			default:
				break;
			}
		}

		protected override IAsyncResult OnBeginResolve (ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				if (resolve_delegate == null)
					resolve_delegate = new Func<ResolveCriteria,EndpointDiscoveryMetadata> (Resolve);
				return resolve_delegate.BeginInvoke (resolveCriteria, callback, state);
			default:
				var ret = Resolve (resolveCriteria);
				var result = new TempAsyncResult (ret, state);
				if (callback != null)
					callback (result);
				return result;
			}
		}

		protected override EndpointDiscoveryMetadata OnEndResolve (IAsyncResult result)
		{
			// FIXME: this is a workaround for (similar to) bug #633945.
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				return resolve_delegate.EndInvoke (result);
			default:
				return (EndpointDiscoveryMetadata) ((TempAsyncResult) result).ReturnValue;
			}
		}

		void Find (FindRequestContext context)
		{
			TextWriter.Null.WriteLine ("Find operation: " + context);
			foreach (var edm in endpoints)
				if (context.Criteria.IsMatch (edm))
					context.AddMatchingEndpoint (edm);
		}

		EndpointDiscoveryMetadata Resolve (ResolveCriteria criteria)
		{
			TextWriter.Null.WriteLine ("Resolve operation: " + criteria);
			throw new NotImplementedException ();
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
