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
using System.Collections.Specialized;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Net;
using System.Web;

namespace System.ServiceModel.Channels
{
	abstract class HttpContextInfo
	{
		public abstract NameValueCollection QueryString { get; }
		public abstract Uri RequestUrl { get; }
		public abstract string HttpMethod { get; }
		public abstract void Abort ();
	}

	class HttpListenerContextInfo : HttpContextInfo
	{
		public HttpListenerContextInfo (HttpListenerContext ctx)
		{
			this.ctx = ctx;
		}
		
		HttpListenerContext ctx;

		public HttpListenerContext Source {
			get { return ctx; }
		}

		public override NameValueCollection QueryString {
			get { return ctx.Request.QueryString; }
		}
		public override Uri RequestUrl {
			get { return ctx.Request.Url; }
		}
		public override string HttpMethod {
			get { return ctx.Request.HttpMethod; }
		}
		public override void Abort ()
		{
			ctx.Response.Abort ();
		}
	}

	class AspNetHttpContextInfo : HttpContextInfo
	{
		public AspNetHttpContextInfo (HttpContext ctx)
		{
			this.ctx = ctx;
		}
		
		HttpContext ctx;

		public HttpContext Source {
			get { return ctx; }
		}

		public override NameValueCollection QueryString {
			get { return ctx.Request.QueryString; }
		}
		public override Uri RequestUrl {
			get { return ctx.Request.Url; }
		}
		public override string HttpMethod {
			get { return ctx.Request.HttpMethod; }
		}

		public override void Abort ()
		{
			ctx.Response.Close ();
		}
	}

	internal class HttpSimpleListenerManager : HttpListenerManager
	{
		static Dictionary<Uri, HttpListener> opened_listeners;
		HttpListener http_listener;

		static HttpSimpleListenerManager ()
		{
			opened_listeners = new Dictionary<Uri, HttpListener> ();
		}

		public HttpSimpleListenerManager (IChannelListener channelListener, HttpTransportBindingElement source)
			: base (channelListener, source)
		{
		}

		protected override void OnRegister (IChannelListener channelListener, TimeSpan timeout)
		{
			lock (opened_listeners) {
				if (!opened_listeners.ContainsKey (channelListener.Uri)) {
					HttpListener listener = new HttpListener ();
					listener.AuthenticationSchemes = Source.AuthenticationScheme;
					listener.Realm = Source.Realm;
					listener.UnsafeConnectionNtlmAuthentication = Source.UnsafeConnectionNtlmAuthentication;

					string uriString = channelListener.Uri.ToString ();
					if (!uriString.EndsWith ("/", StringComparison.Ordinal))
						uriString += "/";
					listener.Prefixes.Add (uriString);
					listener.Start ();

					opened_listeners [channelListener.Uri] = listener;
				}

				http_listener = opened_listeners [channelListener.Uri];
			}
		}

		protected override void OnUnregister (IChannelListener listener, bool abort)
		{
			lock (opened_listeners) {
				if (http_listener == null)
					return;
				if (http_listener.IsListening) {
					if (abort)
						http_listener.Abort ();
					else
						http_listener.Close ();
				}
				((IDisposable) http_listener).Dispose ();

				opened_listeners.Remove (listener.Uri);
			}

			http_listener = null;
		}

		protected override void KickContextReceiver (IChannelListener listener, Action<HttpContextInfo> contextReceivedCallback)
		{
			http_listener.BeginGetContext (delegate (IAsyncResult result) {
				var hctx = http_listener.EndGetContext (result);
				contextReceivedCallback (new HttpListenerContextInfo (hctx));
			}, null);
		}
	}

	internal class AspNetListenerManager : HttpListenerManager
	{
		SvcHttpHandler http_handler;

		public AspNetListenerManager (IChannelListener channelListener, HttpTransportBindingElement source)
			: base (channelListener, source)
		{
			http_handler = SvcHttpHandlerFactory.GetHandlerForListener (channelListener);
		}

		public SvcHttpHandler Source {
			get { return http_handler; }
		}

		protected override void OnRegister (IChannelListener channelListener, TimeSpan timeout)
		{
			http_handler.RegisterListener (channelListener);
		}

		protected override void OnUnregister (IChannelListener listener, bool abort)
		{
			http_handler.UnregisterListener (listener);
		}

		Func<IChannelListener,HttpContext> wait_delegate;

		protected override void KickContextReceiver (IChannelListener listener, Action<HttpContextInfo> contextReceivedCallback)
		{
			if (wait_delegate == null)
				wait_delegate = new Func<IChannelListener,HttpContext> (http_handler.WaitForRequest);
			wait_delegate.BeginInvoke (listener, delegate (IAsyncResult result) {
				var ctx = wait_delegate.EndInvoke (result);
				contextReceivedCallback (ctx != null ? new AspNetHttpContextInfo (ctx) : null);
				}, null);
		}
	}

	internal abstract class HttpListenerManager
	{
		static Dictionary<Uri, List<IChannelListener>> registered_channels;
		IChannelListener channel_listener;
		MetadataPublishingInfo mex_info;
		HttpGetWsdl wsdl_instance;
		AutoResetEvent wait_http_ctx = new AutoResetEvent (false);
		List<HttpContextInfo> pending = new List<HttpContextInfo> ();

		public MetadataPublishingInfo MexInfo { get { return mex_info; } }
		public HttpTransportBindingElement Source { get; private set; }

		static HttpListenerManager ()
		{
			registered_channels = new Dictionary<Uri, List<IChannelListener>> ();
		}

		protected HttpListenerManager (IChannelListener channelListener, HttpTransportBindingElement source)
		{
			this.channel_listener = channelListener;
			// FIXME: this cast should not be required, but current JIT somehow causes an internal error.
			mex_info = ((IChannelListener) channelListener).GetProperty<MetadataPublishingInfo> ();
			wsdl_instance = mex_info != null ? mex_info.Instance : null;
			Source = source;
		}

		public void Open (TimeSpan timeout)
		{
			if (!registered_channels.ContainsKey (channel_listener.Uri))
				registered_channels [channel_listener.Uri] = new List<IChannelListener> ();
			OnRegister (channel_listener, timeout);
			registered_channels [channel_listener.Uri].Add (channel_listener);

			// make sure to fill wsdl_instance among other 
			// listeners. It is somewhat hacky way, but 
			// otherwise there is no assured way to do it.
			var wsdl = wsdl_instance;
			if (wsdl == null)
				foreach (var l in registered_channels [channel_listener.Uri])
					if ((wsdl = l.GetProperty<HttpListenerManager> ().wsdl_instance) != null)
						break;
			if (wsdl != null) {
				foreach (var l in registered_channels [channel_listener.Uri])
					l.GetProperty<HttpListenerManager> ().wsdl_instance = wsdl;
			}
		}

		public void Stop (bool abort)
		{
			List<IChannelListener> channelsList = registered_channels [channel_listener.Uri];
			channelsList.Remove (channel_listener);

			try {
				foreach (var ctx in pending)
					ctx.Abort ();
			} catch (Exception ex) {
				// FIXME: log it
				Console.WriteLine ("error during HTTP channel listener shutdown: " + ex);
			}

			if (channelsList.Count == 0)
				OnUnregister (channel_listener, abort);
		}

		protected abstract void OnRegister (IChannelListener listener, TimeSpan timeout);
		protected abstract void OnUnregister (IChannelListener listener, bool abort);

		public void CancelGetHttpContextAsync ()
		{
			wait_http_ctx.Set ();
		}

		// Do not directly handle retrieved HttpListenerContexts when
		// the listener received ones.
		// Instead, iterate every listeners to find the most-likely-
		// matching one and immediately handle the listener context.
		// If the listener is not requesting a context right now, then
		// store it in *each* listener's queue.

		public void GetHttpContextAsync (TimeSpan timeout, Action<HttpContextInfo> callback)
		{
			lock (pending) {
				foreach (var pctx in pending) {
					if (FilterHttpContext (pctx)) {
						callback (pctx);
						return;
					}
				}
			}
			KickContextReceiver (channel_listener, DispatchHttpListenerContext);
			wait_http_ctx.WaitOne (timeout);
			lock (pending) {
				HttpContextInfo ctx = pending.Count > 0 ? pending [0] : null;
				if (ctx != null)
					pending.Remove (ctx);
				callback (ctx);
			}
		}

		protected abstract void KickContextReceiver (IChannelListener listener, Action<HttpContextInfo> contextReceiverCallback);

		void DispatchHttpListenerContext (HttpContextInfo ctx)
		{
			if (wsdl_instance == null) {
				AddListenerContext (this, ctx);
				return;
			}
			foreach (var l in registered_channels [channel_listener.Uri]) {
				var lm = l.GetProperty<HttpListenerManager> ();
				if (lm.FilterHttpContext (ctx)) {
					AddListenerContext (lm, ctx);
					return;
				}
			}
			AddListenerContext (this, ctx);
		}

		static void AddListenerContext (HttpListenerManager lm, HttpContextInfo ctx)
		{
			lock (registered_channels) {
				lm.pending.Add (ctx);
				// FIXME: this should not be required, but it somehow saves some failures wrt concurrent calls.
				Thread.Sleep (100);
				lm.wait_http_ctx.Set ();
			}
		}

		const UriComponents cmpflag = UriComponents.HttpRequestUrl ^ UriComponents.Query;
		const UriFormat fmtflag = UriFormat.SafeUnescaped;

		internal bool FilterHttpContext (HttpContextInfo ctx)
		{
			if (ctx.HttpMethod.ToUpper () != "GET")
				return mex_info == null;

			if (wsdl_instance == null)
				return true;
			if (channel_listener.State != CommunicationState.Opened)
				return true;

			if (wsdl_instance.WsdlUrl != null && Uri.Compare (ctx.RequestUrl, wsdl_instance.WsdlUrl, cmpflag, fmtflag, StringComparison.Ordinal) == 0) {
				if (mex_info == null)
					return false; // Do not handle this at normal dispatcher.
				if (ctx.QueryString [null] == "wsdl")
					return mex_info.SupportsMex; // wsdl dispatcher should handle this.
				if (!wsdl_instance.HelpUrl.Equals (wsdl_instance.WsdlUrl))
					return true; // in case help URL is not equivalent to WSDL URL, it anyways returns WSDL regardless of ?wsdl existence.
			}
			if (wsdl_instance.HelpUrl != null && Uri.Compare (ctx.RequestUrl, wsdl_instance.HelpUrl, cmpflag, fmtflag, StringComparison.Ordinal) == 0) {
				// Do not handle this at normal dispatcher.
				// Do return true otherwise, even if it is with "?wsdl".
				// (It must be handled above if applicable.)
				return mex_info != null;
			}

			return mex_info == null;
		}
	}
}
