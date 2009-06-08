//
// HttpListenerManager.cs
//
// Author:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
//
// Copyright (C) 2005-2006 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using System.Net;

namespace System.ServiceModel.Channels
{
	internal class HttpListenerManager<TChannel> where TChannel : class, IChannel
	{
		static Dictionary<Uri, HttpListener> opened_listeners;
		static Dictionary<Uri, List<HttpSimpleChannelListener<TChannel>>> registered_channels;
		HttpSimpleChannelListener<TChannel> channel_listener;
		HttpListener http_listener;

		static HttpListenerManager ()
		{
			opened_listeners = new Dictionary<Uri, HttpListener> ();
			registered_channels = new Dictionary<Uri, List<HttpSimpleChannelListener<TChannel>>> ();
		}

		public HttpListenerManager (HttpSimpleChannelListener<TChannel> channel_listener)
		{
			this.channel_listener = channel_listener;
		}

		public void Open (TimeSpan timeout)
		{
			lock (opened_listeners) {
				if (!opened_listeners.ContainsKey (channel_listener.Uri)) {
					HttpListener listener = new HttpListener ();

					string uriString = channel_listener.Uri.ToString ();
					if (!uriString.EndsWith ("/", StringComparison.Ordinal))
						uriString += "/";
					listener.Prefixes.Add (uriString);
					listener.Start ();

					opened_listeners [channel_listener.Uri] = listener;
					List<HttpSimpleChannelListener<TChannel>> registeredList = new List<HttpSimpleChannelListener<TChannel>> ();
					registered_channels [channel_listener.Uri] = registeredList;
				}

				http_listener = opened_listeners [channel_listener.Uri];
				registered_channels [channel_listener.Uri].Add (channel_listener);
			}
		}

		public void Stop ()
		{
			lock (opened_listeners) {
				if (http_listener == null)
					return;
				List<HttpSimpleChannelListener<TChannel>> channelsList = registered_channels [channel_listener.Uri];
				channelsList.Remove (channel_listener);
				if (channelsList.Count == 0) {
					if (http_listener.IsListening)
						http_listener.Stop ();
					((IDisposable) http_listener).Dispose ();

					opened_listeners.Remove (channel_listener.Uri);
			}
		}

		public HttpListener HttpListener
		{
			get { return http_listener; }
		}
	}	
}
