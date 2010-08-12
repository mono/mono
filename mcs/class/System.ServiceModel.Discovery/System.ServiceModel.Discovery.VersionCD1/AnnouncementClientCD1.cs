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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Discovery;

namespace System.ServiceModel.Discovery.VersionCD1
{
	internal class AnnouncementClientCD1 : ClientBase<IAnnouncementContractCD1>, AnnouncementClient.IAnnouncementCommon
	{
		public AnnouncementClientCD1 (AnnouncementEndpoint endpoint)
			: base (endpoint)
		{
		}
		
		public IAsyncResult BeginAnnounceOnline (EndpointDiscoveryMetadata metadata, DiscoveryMessageSequence sequence, AsyncCallback callback, object state)
		{
			var md = new EndpointDiscoveryMetadataCD1 (metadata);
			var seq = new DiscoveryMessageSequenceCD1 (sequence);
			var msg = new MessageContractsCD1.OnlineAnnouncement () { EndpointDiscoveryMetadata = md, MessageSequence = seq };
			return Channel.BeginOnlineAnnouncement (msg, callback, state);
		}

		public void EndAnnounceOnline (IAsyncResult result)
		{
			Channel.EndOnlineAnnouncement (result);
		}

		public IAsyncResult BeginAnnounceOffline (EndpointDiscoveryMetadata metadata, DiscoveryMessageSequence sequence, AsyncCallback callback, object state)
		{
			var md = new EndpointDiscoveryMetadataCD1 (metadata);
			var seq = new DiscoveryMessageSequenceCD1 (sequence);
			var msg = new MessageContractsCD1.OfflineAnnouncement () { EndpointDiscoveryMetadata = md, MessageSequence = seq };
			return Channel.BeginOfflineAnnouncement (msg, callback, state);
		}

		public void EndAnnounceOffline (IAsyncResult result)
		{
			Channel.EndOfflineAnnouncement (result);
		}
	}
}
