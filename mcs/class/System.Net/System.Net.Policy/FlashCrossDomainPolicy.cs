//
// FlashCrossDomainPolicy.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright (C) 2009-2010 Novell, Inc.  http://www.novell.com
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

#if NET_2_1

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Net.Policy {

	partial class FlashCrossDomainPolicy : BaseDomainPolicy {

		private string site_control;

		public FlashCrossDomainPolicy ()
		{
			AllowedAccesses = new List<AllowAccessFrom> ();
			AllowedHttpRequestHeaders = new List<AllowHttpRequestHeadersFrom> ();
		}

		public List<AllowAccessFrom> AllowedAccesses { get; private set; }
		public List<AllowHttpRequestHeadersFrom> AllowedHttpRequestHeaders { get; private set; }

		public string SiteControl {
			get { return String.IsNullOrEmpty (site_control) ? "all" : site_control; }
			set { site_control = value; }
		}

		public override bool IsAllowed (WebRequest request)
		{
			return IsAllowed (request.RequestUri, request.Headers.AllKeys);
		}

		public bool IsAllowed (Uri uri, string [] headerKeys)
		{
			switch (SiteControl) {
			case "all":
			case "master-only":
			case "by-ftp-filename":
				break;
			default:
				// others, e.g. 'none', are not supported/accepted
				return false;
			}

			if (AllowedAccesses.Count > 0 &&
			    !AllowedAccesses.Any (a => a.IsAllowed (uri, headerKeys)))
				return false;
			if (AllowedHttpRequestHeaders.Count > 0 && 
			    AllowedHttpRequestHeaders.Any (h => h.IsRejected (uri, headerKeys)))
				return false;

			return true;
		}

		public class AllowAccessFrom {

			public AllowAccessFrom ()
			{
				Secure = true;	// true by default
			}

			public string Domain { get; set; }
			public bool AllowAnyPort { get; set; }
			public int [] ToPorts { get; set; }
			public bool Secure { get; set; }

			public bool IsAllowed (Uri uri, string [] headerKeys)
			{
				// "A Flash policy file must allow access to all domains to be used by the Silverlight runtime."
				// http://msdn.microsoft.com/en-us/library/cc645032(VS.95).aspx
				if (Domain != "*")
					return false;
				if (!AllowAnyPort && ToPorts != null && Array.IndexOf (ToPorts, uri.Port) < 0)
					return false;

				// if Secure is false then it allows applications from HTTP to download data from HTTPS servers
				if (!Secure)
					return true;
				// if Secure is true then data on HTTPS servers can only be accessed by application on HTTPS servers
				if (uri.Scheme == Uri.UriSchemeHttps)
					return (ApplicationUri.Scheme == Uri.UriSchemeHttps);
				// otherwise FILE/HTTP applications can access HTTP uris
				return true;
			}
		}

		public class AllowHttpRequestHeadersFrom {

			public AllowHttpRequestHeadersFrom ()
			{
				Headers = new Headers ();
			}

			public string Domain { get; set; }
			public bool AllowAllHeaders { get; set; }
			public Headers Headers { get; private set; }
			public bool Secure { get; set; }

			public bool IsRejected (Uri uri, string [] headerKeys)
			{
				// "A Flash policy file must allow access to all domains to be used by the Silverlight runtime."
				// http://msdn.microsoft.com/en-us/library/cc645032(VS.95).aspx
				if (Domain != "*")
					return false;

				if (Headers.IsAllowed (headerKeys))
					return false;

				// if Secure is false then it allows applications from HTTP to download data from HTTPS servers
				if (!Secure)
					return true;
				// if Secure is true then only application on HTTPS servers can access data on HTTPS servers
				if (ApplicationUri.Scheme == Uri.UriSchemeHttps)
					return (uri.Scheme == Uri.UriSchemeHttps);
				// otherwise FILE/HTTP applications can access HTTP uris
				return true;
			}
		}
	}
}

#endif

