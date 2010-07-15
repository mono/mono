//
// IssuedTokenClientCredential.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	[MonoTODO]
	public sealed class IssuedTokenClientCredential
	{
		internal IssuedTokenClientCredential ()
		{
		}

		bool cache = true;
		Dictionary<Uri,KeyedByTypeCollection<IEndpointBehavior>> behaviors =
			new Dictionary<Uri,KeyedByTypeCollection<IEndpointBehavior>> ();
		SecurityKeyEntropyMode entropy = SecurityKeyEntropyMode.CombinedEntropy;
		KeyedByTypeCollection<IEndpointBehavior> local_behaviors =
			new KeyedByTypeCollection<IEndpointBehavior> ();
		EndpointAddress local_issuer_address;
		Binding local_issuer_binding;
		TimeSpan max_cache_time = TimeSpan.MaxValue;
		// FIXME: could be related to LocalClientSecuritysettings.CookieRenewalThresholdPercentage ?
		int renewal_threshold = 60;

		internal IssuedTokenClientCredential Clone ()
		{
			var ret = (IssuedTokenClientCredential) MemberwiseClone ();
			ret.local_behaviors = new KeyedByTypeCollection<IEndpointBehavior> (local_behaviors);
			ret.behaviors = new Dictionary<Uri,KeyedByTypeCollection<IEndpointBehavior>> (behaviors);
			return ret;
		}

		public bool CacheIssuedTokens {
			get { return cache; }
			set { cache = value; }
		}

		public int IssuedTokenRenewalThresholdPercentage {
			get { return renewal_threshold; }
			set { renewal_threshold = value; }
		}

		public Dictionary<Uri,KeyedByTypeCollection<IEndpointBehavior>> IssuerChannelBehaviors {
			get { return behaviors; }
		}

		public SecurityKeyEntropyMode DefaultKeyEntropyMode {
			get { return entropy; }
			set { entropy = value; }
		}

		public KeyedByTypeCollection<IEndpointBehavior> LocalIssuerChannelBehaviors { 
			get { return local_behaviors; }
		}

		public EndpointAddress LocalIssuerAddress {
			get { return local_issuer_address; }
			set { local_issuer_address = value; }
		}

		public Binding LocalIssuerBinding {
			get { return local_issuer_binding; }
			set { local_issuer_binding = value; }
		}

		public TimeSpan MaxIssuedTokenCachingTime {
			get { return max_cache_time; }
			set { max_cache_time = value; }
		}
	}
}
