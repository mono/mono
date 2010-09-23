//
// CrossDomainPolicyManager.cs
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
using System.Net.Sockets;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;

namespace System.Net.Policy {

	internal static class CrossDomainPolicyManager {

		public static string GetRoot (Uri uri)
		{
			if ((uri.Scheme == "http" && uri.Port == 80) || (uri.Scheme == "https" && uri.Port == 443) || (uri.Port == -1))
				return String.Format ("{0}://{1}/", uri.Scheme, uri.DnsSafeHost);
			else
				return String.Format ("{0}://{1}:{2}/", uri.Scheme, uri.DnsSafeHost, uri.Port);
		}
#if !TEST
		public const string ClientAccessPolicyFile = "/clientaccesspolicy.xml";
		public const string CrossDomainFile = "/crossdomain.xml";

		const int Timeout = 10000;

		// Web Access Policy

		static Dictionary<string,ICrossDomainPolicy> policies = new Dictionary<string,ICrossDomainPolicy> ();

		static internal ICrossDomainPolicy PolicyDownloadPolicy = new PolicyDownloadPolicy ();
		static ICrossDomainPolicy site_of_origin_policy = new SiteOfOriginPolicy ();
		static ICrossDomainPolicy no_access_policy = new NoAccessPolicy ();

		static Uri GetRootUri (Uri uri)
		{
			return new Uri (GetRoot (uri));
		}

		public static Uri GetSilverlightPolicyUri (Uri uri)
		{
			return new Uri (GetRootUri (uri), CrossDomainPolicyManager.ClientAccessPolicyFile);
		}

		public static Uri GetFlashPolicyUri (Uri uri)
		{
			return new Uri (GetRootUri (uri), CrossDomainPolicyManager.CrossDomainFile);
		}

		public static ICrossDomainPolicy GetCachedWebPolicy (Uri uri)
		{
			// if we request an Uri from the same site then we return an "always positive" policy
			if (SiteOfOriginPolicy.HasSameOrigin (uri, BaseDomainPolicy.ApplicationUri))
				return site_of_origin_policy;

			// otherwise we search for an already downloaded policy for the web site
			string root = GetRoot (uri);
			ICrossDomainPolicy policy = null;
			policies.TryGetValue (root, out policy);
			// and we return it (if we have it) or null (if we dont)
			return policy;
		}

		private static void AddPolicy (Uri responseUri, ICrossDomainPolicy policy)
		{
			string root = GetRoot (responseUri);
			policies [root] = policy;
		}

		// see moon/test/2.0/WebPolicies/Pages.xaml.cs for all test cases
		private static bool CheckContentType (string contentType)
		{
			const string application_xml = "application/xml";

			// most common case: all text/* are accepted
			if (contentType.StartsWith ("text/"))
				return true;

			// special case (e.g. used in nbcolympics)
			if (contentType.StartsWith (application_xml)) {
				if (application_xml.Length == contentType.Length)
					return true; // exact match

				// e.g. "application/xml; charset=x" - we do not care what comes after ';'
				if (contentType.Length > application_xml.Length)
					return contentType [application_xml.Length] == ';';
			}
			return false;
		}

		public static ICrossDomainPolicy BuildSilverlightPolicy (HttpWebResponse response)
		{
			// return null if no Silverlight policy was found, since we offer a second chance with a flash policy
			if ((response.StatusCode != HttpStatusCode.OK) || !CheckContentType (response.ContentType))
				return null;

			ICrossDomainPolicy policy = null;
			try {
				policy = ClientAccessPolicy.FromStream (response.GetResponseStream ());
				if (policy != null)
					AddPolicy (response.ResponseUri, policy);
			} catch (Exception ex) {
				Console.WriteLine (String.Format ("CrossDomainAccessManager caught an exception while reading {0}: {1}", 
					response.ResponseUri, ex));
				// and ignore.
			}
			return policy;
		}

		public static ICrossDomainPolicy BuildFlashPolicy (HttpWebResponse response)
		{
			ICrossDomainPolicy policy = null;
			if ((response.StatusCode == HttpStatusCode.OK) && CheckContentType (response.ContentType)) {
				try {
					policy = FlashCrossDomainPolicy.FromStream (response.GetResponseStream ());
				} catch (Exception ex) {
					Console.WriteLine (String.Format ("CrossDomainAccessManager caught an exception while reading {0}: {1}", 
						response.ResponseUri, ex));
					// and ignore.
				}
				if (policy != null) {
					// see DRT# 864 and 865
					string site_control = response.InternalHeaders ["X-Permitted-Cross-Domain-Policies"];
					if (!String.IsNullOrEmpty (site_control))
						(policy as FlashCrossDomainPolicy).SiteControl = site_control;
				}
			}

			// the flash policy was the last chance, keep a NoAccess into the cache
			if (policy == null)
				policy = no_access_policy;

			AddPolicy (response.ResponseUri, policy);
			return policy;
		}

		// Socket Policy
		//
		// - we connect once to a site for the entire application life time
		// - this returns us a policy file (silverlight format only) or else no access is granted
		// - this policy file
		// 	- can contain multiple policies
		// 	- can apply to multiple domains
		//	- can grant access to several resources

		static Dictionary<string,ClientAccessPolicy> socket_policies = new Dictionary<string,ClientAccessPolicy> ();
		static byte [] socket_policy_file_request = Encoding.UTF8.GetBytes ("<policy-file-request/>");
		const int PolicyPort = 943;

		// make sure this work in a IPv6-only environment
		static AddressFamily GetBestFamily ()
		{
			if (Socket.OSSupportsIPv4)
				return AddressFamily.InterNetwork;
			else if (Socket.OSSupportsIPv6)
				return AddressFamily.InterNetworkV6;
			else
				return AddressFamily.Unspecified;
		}

		static Stream GetPolicyStream (IPEndPoint endpoint)
		{
			MemoryStream ms = new MemoryStream ();
			ManualResetEvent mre = new ManualResetEvent (false);
			// Silverlight only support TCP
			Socket socket = new Socket (GetBestFamily (), SocketType.Stream, ProtocolType.Tcp);

			// Application code can't connect to port 943, so we need a special/internal API/ctor to allow this
			SocketAsyncEventArgs saea = new SocketAsyncEventArgs (true);
			saea.RemoteEndPoint = new IPEndPoint (endpoint.Address, PolicyPort);
			saea.Completed += delegate (object sender, SocketAsyncEventArgs e) {
				if (e.SocketError != SocketError.Success) {
					mre.Set ();
					return;
				}

				switch (e.LastOperation) {
				case SocketAsyncOperation.Connect:
					e.SetBuffer (socket_policy_file_request, 0, socket_policy_file_request.Length);
					socket.SendAsync (e);
					break;
				case SocketAsyncOperation.Send:
					byte [] buffer = new byte [256];
					e.SetBuffer (buffer, 0, buffer.Length);
					socket.ReceiveAsync (e);
					break;
				case SocketAsyncOperation.Receive:
					int transfer = e.BytesTransferred;
					if (transfer > 0) {
						ms.Write (e.Buffer, 0, transfer);
						// Console.Write (Encoding.UTF8.GetString (e.Buffer, 0, transfer));
					}

					if ((transfer == 0) || (transfer < e.Buffer.Length)) {
						ms.Position = 0;
						mre.Set ();
					} else {
						socket.ReceiveAsync (e);
					}
					break;
				}
			};

			socket.ConnectAsync (saea);

			// behave like there's no policy (no socket access) if we timeout
			if (!mre.WaitOne (Timeout))
				return null;

			return ms;
		}

		static Stream GetPolicyStream (Uri uri)
		{
			// FIXME
			throw new NotSupportedException ("Fetching socket policy from " + uri.ToString () + " is not yet available in moonlight");
		}

		public static ClientAccessPolicy CreateForEndPoint (IPEndPoint endpoint, SocketClientAccessPolicyProtocol protocol)
		{
			Stream s = null;

			switch (protocol) {
			case SocketClientAccessPolicyProtocol.Tcp:
				s = GetPolicyStream (endpoint);
				break;
			case SocketClientAccessPolicyProtocol.Http:
				// <quote>It will NOT attempt to download the policy via the custom TCP protocol if the 
				// policy check fails.</quote>
				// http://blogs.msdn.com/ncl/archive/2010/04/15/silverlight-4-socket-policy-changes.aspx
				string url = String.Format ("http://{0}:80{1}", endpoint.Address.ToString (), 
					CrossDomainPolicyManager.ClientAccessPolicyFile);
				s = GetPolicyStream (new Uri (url));
				break;
			}

			if (s == null)
				return null;

			ClientAccessPolicy policy = null;
			try {
				policy = (ClientAccessPolicy) ClientAccessPolicy.FromStream (s);
			} catch (Exception ex) {
				Console.WriteLine (String.Format ("CrossDomainAccessManager caught an exception while reading {0}: {1}", 
					endpoint, ex.Message));
				// and ignore.
			}

			return policy;
		}

		static public bool CheckEndPoint (EndPoint endpoint, SocketClientAccessPolicyProtocol protocol)
		{
			// if needed transform the DnsEndPoint into a usable IPEndPoint
			IPEndPoint ip = (endpoint as IPEndPoint);
			if (ip == null)
				throw new ArgumentException ("endpoint");

			// find the policy (cached or to be downloaded) associated with the endpoint
			string address = ip.Address.ToString ();
			ClientAccessPolicy policy = null;
			if (!socket_policies.TryGetValue (address, out policy)) {
				policy = CreateForEndPoint (ip, protocol);
				socket_policies.Add (address, policy);
			}

			// no access granted if no policy is available
			if (policy == null)
				return false;

			// does the policy allows access ?
			return policy.IsAllowed (ip);
		}
#endif
	}
}

#endif

