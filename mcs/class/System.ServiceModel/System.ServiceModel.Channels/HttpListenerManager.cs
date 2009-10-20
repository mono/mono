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
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Net;

namespace System.ServiceModel.Channels
{
	internal class HttpListenerManager<TChannel> where TChannel : class, IChannel
	{
		static Dictionary<Uri, HttpListener> opened_listeners;
		static Dictionary<Uri, List<HttpSimpleChannelListener<TChannel>>> registered_channels;
		HttpSimpleChannelListener<TChannel> channel_listener;
		HttpListener http_listener;
		MetadataPublishingInfo mex_info;
		HttpGetWsdl wsdl_instance;
		AutoResetEvent wait_http_ctx = new AutoResetEvent (false);
		List<HttpListenerContext> pending = new List<HttpListenerContext> ();

		static HttpListenerManager ()
		{
			opened_listeners = new Dictionary<Uri, HttpListener> ();
			registered_channels = new Dictionary<Uri, List<HttpSimpleChannelListener<TChannel>>> ();
		}

		public HttpListenerManager (HttpSimpleChannelListener<TChannel> channelListener)
		{
			this.channel_listener = channelListener;
			// FIXME: this cast should not be required, but current JIT somehow causes an internal error.
			mex_info = ((IChannelListener) channelListener).GetProperty<MetadataPublishingInfo> ();
			wsdl_instance = mex_info != null ? mex_info.Instance : null;
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

				// make sure to fill wsdl_instance among other 
				// listeners. It is somewhat hacky way, but 
				// otherwise there is no assured way to do it.
				if (wsdl_instance != null) {
					foreach (var l in registered_channels [channel_listener.Uri])
						l.ListenerManager.wsdl_instance = wsdl_instance;
				}
			}
		}

		public void Stop (bool abort)
		{
			lock (opened_listeners) {
				if (http_listener == null)
					return;
				List<HttpSimpleChannelListener<TChannel>> channelsList = registered_channels [channel_listener.Uri];
				channelsList.Remove (channel_listener);
				if (channelsList.Count == 0) {
					if (http_listener.IsListening) {
						if (abort)
							http_listener.Abort ();
						else
							http_listener.Close ();
					}
					((IDisposable) http_listener).Dispose ();

					opened_listeners.Remove (channel_listener.Uri);
					try {
						foreach (var ctx in pending)
							ctx.Response.Abort ();
					} catch (Exception ex) {
						// FIXME: log it
						Console.WriteLine ("error during HTTP channel listener shutdown: " + ex);
					}
					http_listener = null;
				}
			}
		}

		// Do not directly handle retrieved HttpListenerContexts when
		// the listener received ones.
		// Instead, iterate every listeners to find the most-likely-
		// matching one and immediately handle the listener context.
		// If the listener is not requesting a context right now, then
		// store it in *each* listener's queue.

		public void GetHttpContextAsync (Action<HttpListenerContext> callback)
		{
			lock (pending) {
				foreach (var pctx in pending) {
					if (FilterHttpContext (pctx)) {
						callback (pctx);
						return;
					}
				}
			}
			HttpListenerContext ctx;
			http_listener.BeginGetContext (delegate (IAsyncResult result) {
				ctx = http_listener.EndGetContext (result);
				DispatchHttpListenerContext (ctx);
			}, null);
			wait_http_ctx.WaitOne ();
			lock (pending) {
				ctx = pending.Count > 0 ? pending [0] : null;
				if (ctx != null)
					pending.Remove (ctx);
			}
			callback (ctx);
		}

		void DispatchHttpListenerContext (HttpListenerContext ctx)
		{
			if (wsdl_instance == null) {
				pending.Add (ctx);
				wait_http_ctx.Set ();
				return;
			}
			foreach (var l in registered_channels [channel_listener.Uri]) {
				var lm = l.ListenerManager;
				if (lm.FilterHttpContext (ctx)) {
					lm.pending.Add (ctx);
					lm.wait_http_ctx.Set ();
					return;
				}
			}
			pending.Add (ctx);
			wait_http_ctx.Set ();
		}

		bool FilterHttpContext (HttpListenerContext ctx)
		{
			if (ctx.Request.HttpMethod.ToUpper () != "GET")
				return mex_info == null;

			if (wsdl_instance == null)
				return true;
			if (channel_listener.State != CommunicationState.Opened)
				return true;

			var cmpflag = UriComponents.HttpRequestUrl ^ UriComponents.Query;
			var fmtflag = UriFormat.SafeUnescaped;

			if (Uri.Compare (ctx.Request.Url, wsdl_instance.WsdlUrl, cmpflag, fmtflag, StringComparison.Ordinal) == 0) {
				if (mex_info == null)
					return false; // Do not handle this at normal dispatcher.
				if (ctx.Request.QueryString [null] == "wsdl")
					return mex_info.IsMex; // wsdl dispatcher should handle this.
				if (!wsdl_instance.HelpUrl.Equals (wsdl_instance.WsdlUrl))
					return true; // in case help URL is not equivalent to WSDL URL, it anyways returns WSDL regardless of ?wsdl existence.
			}
			if (Uri.Compare (ctx.Request.Url, wsdl_instance.HelpUrl, cmpflag, fmtflag, StringComparison.Ordinal) == 0) {
				// Do not handle this at normal dispatcher.
				// Do return true otherwise, even if it is with "?wsdl".
				// (It must be handled above if applicable.)
				return mex_info != null;
			}

			return mex_info == null;
		}
	}
}
