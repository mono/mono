//
// ClientAccessPolicy.cs
//
// Authors:
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

	partial class ClientAccessPolicy : BaseDomainPolicy {

		public class AccessPolicy {

			public const short MinPort = 4502;
			public const short MaxPort = 4534;

			public List<AllowFrom> AllowedServices { get; private set; }
			public List<GrantTo> GrantedResources { get; private set; }
			public long PortMask { get; set; }

			public AccessPolicy ()
			{
				AllowedServices = new List<AllowFrom> ();
				GrantedResources = new List<GrantTo> ();
			}

			public bool PortAllowed (int port)
			{
				if ((port < MinPort) || (port > MaxPort))
					return false;

				return (((PortMask >> (port - MinPort)) & 1) == 1);
			}
		}

		public ClientAccessPolicy ()
		{
			AccessPolicyList = new List<AccessPolicy> ();
		}

		public List<AccessPolicy> AccessPolicyList { get; private set; }

		public bool IsAllowed (IPEndPoint endpoint)
		{
			foreach (AccessPolicy policy in AccessPolicyList) {
				// does something allow our URI in this policy ?
				foreach (AllowFrom af in policy.AllowedServices) {
					// fake "GET" as method as this does not apply to sockets
					if (af.IsAllowed (ApplicationUri, "GET")) {
						// if so, is our request port allowed ?
						if (policy.PortAllowed (endpoint.Port))
							return true;
					}
				}
			}
			// no policy allows this socket connection
			return false;
		}

		// note: tests show that it only applies to Silverlight policy (seems to work with Flash)
		// and only if we're not granting full access (i.e. '/' with all subpaths)
		// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=466043
		private bool CheckOriginalPath (Uri uri)
		{
			// Path Restriction for cross-domain requests
			// http://msdn.microsoft.com/en-us/library/cc838250(VS.95).aspx
			string original = uri.OriginalString;
			// applies to the *path* only (not the query part)
			int query = original.IndexOf ('?');
			if (query != -1)
				original = original.Substring (0, query);

			if (original.Contains ('%') || original.Contains ("./") || original.Contains ("..")) {
				// special case when no path restriction applies - i.e. the above characters are accepted by SL
				if (AccessPolicyList.Count != 1)
					return false;
				AccessPolicy policy = AccessPolicyList [0];
				if (policy.GrantedResources.Count != 1)
					return false;
				GrantTo gt = policy.GrantedResources [0];
				if (gt.Resources.Count != 1)
					return false;
				Resource r = gt.Resources [0];
				return (r.IncludeSubpaths && (r.Path == "/"));
			}
			return true;
		}

		public override bool IsAllowed (WebRequest request)
		{
			return IsAllowed (request.RequestUri, request.Method, request.Headers.AllKeys);
		}

		public bool IsAllowed (Uri uri, string method, params string [] headerKeys)
		{
			foreach (AccessPolicy policy in AccessPolicyList) {
				// does something allow our URI in this policy ?
				foreach (AllowFrom af in policy.AllowedServices) {
					// is the application (XAP) URI allowed by the policy ?
					// check headers
					if (!af.HttpRequestHeaders.IsAllowed (headerKeys)) {
						Exception = new NotSupportedException ();
						return false;
					}

					if (af.IsAllowed (ApplicationUri, method)) {
						foreach (GrantTo gt in policy.GrantedResources) {
							// is the requested access to the Uri granted under this policy ?
							if (gt.IsGranted (uri)) {
								// at this stage the URI has removed the "offending" characters so 
								// we need to look at the original
								return CheckOriginalPath (uri);
							}
						}
					}
				}
			}
			// no policy allows this web connection
			return false;
		}

		public class AllowFrom {

			public AllowFrom ()
			{
				Domains = new List<string> ();
				HttpRequestHeaders = new Headers ();
				Scheme = String.Empty;
			}

			public bool AllowAnyDomain { get; set; }

			public List<string> Domains { get; private set; }

			public Headers HttpRequestHeaders { get; private set; }

			public bool AllowAnyMethod { get; set; }

			public string Scheme { get; internal set; }

			public bool IsAllowed (Uri uri, string method)
			{
				// check scheme
				if ((Scheme.Length > 0) && (Scheme == uri.Scheme)) {
					switch (Scheme) {
					case "http":
						return (uri.Port == 80);
					case "https":
						return (uri.Port == 443);
					case "file":
						return true;
					default:
						return false;
					}
				}
				// check methods
				if (!AllowAnyMethod) {
					// if not all methods are allowed (*) then only GET and POST request are possible
					// further restriction exists in the Client http stack
					if ((String.Compare (method, "GET", StringComparison.OrdinalIgnoreCase) != 0) &&
						(String.Compare (method, "POST", StringComparison.OrdinalIgnoreCase) != 0)) {
						return false;
					}
				}

				// check domains
				if (AllowAnyDomain)
					return true;

				if (Domains.All (domain => !CheckDomainUri (domain)))
					return false;
				return true;
			}

			static bool CheckDomainUri (string policy)
			{
				Uri uri;
				if (Uri.TryCreate (policy, UriKind.Absolute, out uri)) {
					// if no local path is part of the policy domain then we compare to the root
					if (uri.LocalPath == "/")
						return (uri.ToString () == ApplicationRoot);
					// otherwise the path must match
					if (uri.LocalPath != ApplicationUri.LocalPath)
						return false;
					return (CrossDomainPolicyManager.GetRoot (uri) == ApplicationRoot);
				}

				// SL policies supports a * wildcard at the start of their host name (but not elsewhere)

				// check for matching protocol
				if (!policy.StartsWith (ApplicationUri.Scheme))
					return false;
				// check for the wirld card immediately after the scheme
				if (policy.IndexOf ("://*.", ApplicationUri.Scheme.Length) != ApplicationUri.Scheme.Length)
					return false;
				// remove *. from uri
				policy = policy.Remove (ApplicationUri.Scheme.Length + 3, 2);
				// create Uri - without the *. it should be a valid one
				if (!Uri.TryCreate (policy, UriKind.Absolute, out uri))
					return false;
				// path must be "empty" and query and fragment (really) empty
				if ((uri.LocalPath != "/") || !String.IsNullOrEmpty (uri.Query) || !String.IsNullOrEmpty (uri.Fragment))
					return false;
				// port must match
				if (ApplicationUri.Port != uri.Port)
					return false;
				// the application uri host must end with the policy host name
				return ApplicationUri.DnsSafeHost.EndsWith (uri.DnsSafeHost);
			}
		}

		public class GrantTo
		{
			public GrantTo ()
			{
				Resources = new List<Resource> ();
			}

			public List<Resource> Resources { get; private set; }

			public bool IsGranted (Uri uri)
			{
				foreach (var gr in Resources) {
					if (gr.IncludeSubpaths) {
						string granted = gr.Path;
						string local = uri.LocalPath;
						if (local.StartsWith (granted, StringComparison.Ordinal)) {
							// "/test" equals "/test" and "test/xyx" but not "/test2"
							// "/test/" equals "test/xyx" but not "/test" or "/test2"
							if (local.Length == granted.Length)
								return true;
							else if (granted [granted.Length - 1] == '/')
								return true;
							else if (local [granted.Length] == '/')
								return true;
						}
					} else {
						if (uri.LocalPath == gr.Path)
							return true;
					}
				}
				return false;
			}
		}

		public class Resource {
			private string path;

			public string Path { 
				get { return path; }
				set {
					// an empty Path Ressource makes the *whole* policy file invalid
					if (String.IsNullOrEmpty (value))
						throw new NotSupportedException ();
					path = value;
				}
			}

			public bool IncludeSubpaths { get; set; }
		}
	}
}

#endif


