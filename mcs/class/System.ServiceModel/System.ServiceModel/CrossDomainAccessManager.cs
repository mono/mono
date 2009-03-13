//
// CrossDomainAccessManager.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;

namespace System.ServiceModel
{
	class CrossDomainAccessManager
	{
#if TEST_CROSS_DOMAIN_ACCESS_MANAGER
		public static void Main (string [] args)
		{
			if (args.Length < 4)
				Console.WriteLine ("CrossDomainAccessManager.exe [appuri] [clientaccesspolicy.xml-or-N/A] [crossdomain.xml-or-N/A] [target]");
			else {
				var m = new CrossDomainAccessManager (new Uri (args [0]));
				if (args [1] != "N/A")
					m.Client = ClientAccessPolicy.Read (XmlReader.Create (args [1]));
				if (args [2] != "N/A")
					m.Domain = CrossDomainPolicy.Read (XmlReader.Create (args [2]));
				Console.WriteLine (m.IsAllowed (new Uri (args [3]), args.Skip (4).ToArray ()));
			}
		}
#endif

		static CrossDomainAccessManager current;

		public static CrossDomainAccessManager Current {
			get {
				if (current == null)
					current = new CrossDomainAccessManager (GetApplicationDocumentUri ());
				return current;
			}
		}

		static Uri GetApplicationDocumentUri ()
		{
			var assembly = Assembly.Load ("System.Windows, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e");
			if (assembly == null)
				throw new InvalidOperationException ("Can not load System.Windows.dll");

			var type = assembly.GetType ("System.Windows.Interop.PluginHost");
			if (type == null)
				throw new InvalidOperationException ("Can not get HtmlPage");

			var prop = type.GetProperty ("RootUri");
			return (Uri) prop.GetValue (null, null);
		}

		public static CrossDomainAccessManager CreateForUri (Uri applicationUri)
		{
			var m = new CrossDomainAccessManager (applicationUri);

			var wreq = (HttpWebRequest) WebRequest.Create (new Uri (applicationUri, "/clientaccesspolicy.xml"));
			var wres = (HttpWebResponse) wreq.EndGetResponse (wreq.BeginGetResponse (null, null));
			if ((int) wres.StatusCode >= 400)
				try {
					using (var xr = XmlReader.Create (wres.GetResponseStream ()))
						m.Client = ClientAccessPolicy.Read (xr);
				} catch (Exception ex) {
					Console.WriteLine (String.Format ("CrossDomainAccessManager caught an exception while reading clientaccesspolicy.xml: {0}", ex.Message));
					// and ignore.
				}

			if (m.Client != null)
				return m;

			wreq = (HttpWebRequest) WebRequest.Create (new Uri (applicationUri, "/crossdomain.xml"));
			wres = (HttpWebResponse) wreq.EndGetResponse (wreq.BeginGetResponse (null, null));
			if ((int) wres.StatusCode >= 400)
				try {
					using (var xr = XmlReader.Create (wres.GetResponseStream ()))
						m.Domain = CrossDomainPolicy.Read (xr);
				} catch (Exception ex) {
					Console.WriteLine (String.Format ("CrossDomainAccessManager caught an exception while reading crossdomain.xml: {0}", ex.Message));
					// and ignore.
				}

			return m;
		}

		public CrossDomainAccessManager (Uri applicationUri)
		{
			if (applicationUri == null)
				throw new ArgumentNullException ("applicationUri");
			ApplicationUri = applicationUri;
		}

		public Uri ApplicationUri { get; private set; }

		public ClientAccessPolicy Client { get; set; }
		public CrossDomainPolicy Domain { get; set; }

		public Dictionary<Uri,bool> checked_uris = new Dictionary<Uri,bool> ();

		public bool IsAllowed (HttpWebRequest request)
		{
			return IsAllowed (request.RequestUri, request.Headers.AllKeys);
		}

		public bool IsAllowed (Uri uri, params string [] headerKeys)
		{
			if (uri.Host == ApplicationUri.Host)
				return true;

			bool ret;
			if (checked_uris.TryGetValue (uri, out ret))
				return ret;
			if (Client != null) {
				if (Client.AllowedServices.Any (af => af.IsAllowed (uri, headerKeys)))
					if (Client.GrantedResources.Any (gt => gt.IsGranted (uri)))
						return true;
			}
			else if (Domain != null) {
				if (Domain.IsAllowed (uri, headerKeys))
					return true;
			}
			// FIXME: it should really reject access
Console.WriteLine ("##### Warning!!! Cross Domain Access Manager detected '{0}' with headers {1}. Moonlight will be blocking this access in later versions.", uri, String.Join (",", headerKeys));
			return true;//false;
		}
	}
}
