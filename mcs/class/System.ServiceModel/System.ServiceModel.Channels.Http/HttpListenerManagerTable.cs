//
// HttpListenerManagerTable.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels.Http
{
	// instantiated per service host
	internal class HttpListenerManagerTable
	{
		// static members

		static readonly List<HttpListenerManagerTable> instances = new List<HttpListenerManagerTable> ();

		public static HttpListenerManagerTable GetOrCreate (object serviceHostKey)
		{
			var m = instances.FirstOrDefault (p => p.ServiceHostKey == serviceHostKey);
			if (m == null) {
				m = new HttpListenerManagerTable (serviceHostKey);
				instances.Add (m);
			}
			return m;
		}
		
		// instance members

		HttpListenerManagerTable (object serviceHostKey)
		{
			ServiceHostKey = serviceHostKey ?? new object ();
			listeners = new Dictionary<Uri, HttpListenerManager> ();
		}
		
		Dictionary<Uri,HttpListenerManager> listeners;

		public object ServiceHostKey { get; private set; }

		public HttpListenerManager GetOrCreateManager (Uri uri)
		{
			var m = listeners.FirstOrDefault (p => p.Key.Equals (uri)).Value;
			if (m == null) {
				// Two special cases
				string absolutePath = uri.AbsolutePath;
				if (absolutePath.EndsWith ("/js", StringComparison.Ordinal) ||
				    absolutePath.EndsWith ("/jsdebug", StringComparison.Ordinal))
					return CreateListenerManager (uri);
				
				// Try without the query, if any
				UriBuilder ub = null;
				if (!String.IsNullOrEmpty (uri.Query)) {
					ub = new UriBuilder (uri);
					ub.Query = null;

					m = listeners.FirstOrDefault (p => p.Key.Equals (ub.Uri)).Value;
					if (m != null)
						return m;
				}

				// Chop off the part following the last / in the absolut path part
				// of the Uri - this is the operation being called in, the remaining
				// left-hand side of the absolute path should be the service
				// endpoint address
				if (ub == null) {
					ub = new UriBuilder (uri);
					ub.Query = null;
				}
				
				int lastSlash = absolutePath.LastIndexOf ('/');
				if (lastSlash != -1) {
					ub.Path = absolutePath.Substring (0, lastSlash);
					m = listeners.FirstOrDefault (p => p.Key.Equals (ub.Uri)).Value;
					if (m != null)
						return m;
				}
			}
			
			if (m == null)
				return CreateListenerManager (uri);
			
			return m;
		}

		HttpListenerManager CreateListenerManager (Uri uri)
		{
			HttpListenerManager m;
			
			if (ServiceHostingEnvironment.InAspNet)
				m = new AspNetHttpListenerManager (uri);
			else
				m = new HttpStandaloneListenerManager (uri);
			listeners [uri] = m;

			return m;
		}
	}
}
