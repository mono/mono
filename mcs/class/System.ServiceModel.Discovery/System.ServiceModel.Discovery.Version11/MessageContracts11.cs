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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Discovery;

namespace System.ServiceModel.Discovery.Version11
{
	internal class MessageContracts11
	{
		public const string NS = DiscoveryVersion.Namespace11;
		public const string HelloAction = NS + "/Hello";
		public const string ByeAction = NS + "/Bye";
		public const string ProbeAction = NS + "/Probe";
		public const string ProbeMatchAction = NS + "/ProbeMatches";
		public const string ResolveAction = NS + "/Resolve";
		public const string ResolveMatchAction = NS + "/ResolveMatches";

		[MessageContract (WrapperName = "Hello", WrapperNamespace = NS)]
		public class OnlineAnnouncement
		{
			[MessageHeader (Name = "AppSequence", Namespace = NS)]
			public DiscoveryMessageSequence11 MessageSequence { get; set; }
			[MessageBodyMember]
			public EndpointDiscoveryMetadata11 EndpointDiscoveryMetadata { get; set; }
		}

		[MessageContract (WrapperName = "Bye", WrapperNamespace = NS)]
		public class OfflineAnnouncement
		{
			[MessageHeader (Name = "AppSequence", Namespace = NS)]
			public DiscoveryMessageSequence11 MessageSequence { get; set; }
			[MessageBodyMember]
			public EndpointDiscoveryMetadata11 EndpointDiscoveryMetadata { get; set; }
		}

		[MessageContract (IsWrapped = false)]
		public class FindRequest
		{
			[MessageBodyMember (Name = "Probe", Namespace = NS)]
			public FindCriteria11 Body { get; set; }
		}

		[MessageContract (IsWrapped = false)]
		public class FindResponse
		{
			[MessageHeader (Name = "AppSequence", Namespace = NS)]
			public DiscoveryMessageSequence11 MessageSequence { get; set; }
			[MessageBodyMember (Name = "ProbeMatches", Namespace = NS)]
			public FindResponse11 Body { get; set; }
		}

		public class FindResponse11 : List<EndpointDiscoveryMetadata11>
		{
		}

		[MessageContract (IsWrapped = false)]
		public class ResolveRequest
		{
			[MessageBodyMember (Name = "Resolve", Namespace = NS)]
			public ResolveCriteria11 Body { get; set; }
		}

		[MessageContract (IsWrapped = false)]
		public class ResolveResponse
		{
			[MessageHeader (Name = "AppSequence", Namespace = NS)]
			public DiscoveryMessageSequence11 MessageSequence { get; set; }
			[MessageBodyMember (Name = "ResolveMatches", Namespace = NS)]
			public EndpointDiscoveryMetadata11 Body { get; set; }
		}
	}
}
