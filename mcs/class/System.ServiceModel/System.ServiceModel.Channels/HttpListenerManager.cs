//
// HttpListenerManager.cs
//
// Author:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Mainsoft, Inc.  http://www.mainsoft.com
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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Security.Principal;
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

		public abstract string User { get; }
		public abstract string Password { get; }
		public abstract void ReturnUnauthorized ();
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

		public override string User {
			get { return ctx.User != null ? ((HttpListenerBasicIdentity) ctx.User.Identity).Name : null; }
		}

		public override string Password {
			get { return ctx.User != null ? ((HttpListenerBasicIdentity) ctx.User.Identity).Password : null; }
		}

		public override void ReturnUnauthorized ()
		{
			ctx.Response.StatusCode = 401;
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

		public override string User {
			get { return ctx.User != null ? ((GenericIdentity) ctx.User.Identity).Name : null; }
		}

		// FIXME: how to acquire this?
		public override string Password {
			get { return null; }
		}

		public override void ReturnUnauthorized ()
		{
			ctx.Response.StatusCode = 401;
		}
	}

	internal class HttpSimpleListenerManager : HttpListenerManager
	{
		static Dictionary<object,Dictionary<Uri,HttpListener>> http_listeners_table = new Dictionary<object,Dictionary<Uri,HttpListener>> ();

		Dictionary<Uri, HttpListener> opened_listeners;
		HttpListener http_listener;

		public HttpSimpleListenerManager (IChannelListener channelListener, HttpTransportBindingElement source, ServiceCredentialsSecurityTokenManager securityTokenManager, ChannelDispatcher dispatcher)
			: base (channelListener, source, securityTokenManager, dispatcher)
		{
			object key = dispatcher != null ? dispatcher.Host : new object (); // so that HttpChannelListener without ServiceHost is always assigned a new table.
			if (!http_listeners_table.TryGetValue (key, out opened_listeners)) {
				opened_listeners = new Dictionary<Uri, HttpListener> ();
				http_listeners_table [key] = opened_listeners;
			}
		}

		protected override void OnRegister (IChannelListener channelListener, TimeSpan timeout)
		{
			lock (opened_listeners) {
				if (!opened_listeners.ContainsKey (channelListener.Uri)) {
					HttpListener listener = new HttpListener ();
					listener.AuthenticationSchemeSelectorDelegate = delegate (HttpListenerRequest req) {
						return Source.AuthenticationScheme;
					};
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

		public AspNetListenerManager (IChannelListener channelListener, HttpTransportBindingElement source, ServiceCredentialsSecurityTokenManager securityTokenManager, ChannelDispatcher dispatcher)
			: base (channelListener, source, securityTokenManager, dispatcher)
		{
			http_handler = SvcHttpHandler.Current;
		}

		internal SvcHttpHandler HttpHandler { get { return http_handler; } }

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
// FIXME: Remove this workaround. This Func<>.BeginInvoke() invocation
// somehow fails and does not kick this method asynchronously.
#if false
			wait_delegate.BeginInvoke (listener, delegate (IAsyncResult result) {
				var ctx = wait_delegate.EndInvoke (result);
				contextReceivedCallback (ctx != null ? new AspNetHttpContextInfo (ctx) : null);
				}, null);
#else
			var ctx = wait_delegate.Invoke (listener);
				contextReceivedCallback (ctx != null ? new AspNetHttpContextInfo (ctx) : null);
#endif
		}
	}

	internal abstract class HttpListenerManager
	{
		static Dictionary<Uri, List<IChannelListener>> registered_channels;
		IChannelListener channel_listener;
		MetadataPublishingInfo mex_info;
		HttpGetWsdl wsdl_instance;
		ManualResetEvent wait_http_ctx = new ManualResetEvent (false);
		List<HttpContextInfo> pending = new List<HttpContextInfo> ();

		public MetadataPublishingInfo MexInfo { get { return mex_info; } }
		public HttpTransportBindingElement Source { get; private set; }
		public ChannelDispatcher Dispatcher { get; private set; }

		SecurityTokenAuthenticator security_token_authenticator;
		SecurityTokenResolver security_token_resolver;

		static HttpListenerManager ()
		{
			registered_channels = new Dictionary<Uri, List<IChannelListener>> ();
		}

		protected HttpListenerManager (IChannelListener channelListener, HttpTransportBindingElement source, ServiceCredentialsSecurityTokenManager securityTokenManager, ChannelDispatcher dispatcher)
		{
			this.Dispatcher = dispatcher;
			this.channel_listener = channelListener;
			mex_info = Dispatcher != null ? Dispatcher.Listener.GetProperty<MetadataPublishingInfo> () : null;
			wsdl_instance = mex_info != null ? mex_info.Instance : null;
			Source = source;

			if (securityTokenManager != null) {
				var str = new SecurityTokenRequirement () { TokenType = SecurityTokenTypes.UserName };
				security_token_authenticator = securityTokenManager.CreateSecurityTokenAuthenticator (str, out security_token_resolver);
			}
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
						pending.Remove (pctx);
						callback (pctx);
						return;
					}
				}
			}
			KickContextReceiver (channel_listener, DispatchHttpListenerContext);
			wait_http_ctx.WaitOne (timeout);
			wait_http_ctx.Reset ();
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
				AddListenerContext (ctx);
				return;
			}
			foreach (var l in registered_channels [channel_listener.Uri]) {
				var lm = l.GetProperty<HttpListenerManager> ();
				if (lm.FilterHttpContext (ctx)) {
					lm.AddListenerContext (ctx);
					return;
				}
			}
			AddListenerContext (ctx);
		}

		void AddListenerContext (HttpContextInfo ctx)
		{
			if (Source.AuthenticationScheme != AuthenticationSchemes.Anonymous) {
				if (security_token_authenticator != null)
					// FIXME: use return value?
					try {
						security_token_authenticator.ValidateToken (new UserNameSecurityToken (ctx.User, ctx.Password));
					} catch (Exception) {
						ctx.ReturnUnauthorized ();
					}
				else {
					ctx.ReturnUnauthorized ();
				}
			}

			lock (pending) {
				pending.Add (ctx);
				// FIXME: this should not be required, but it somehow saves some failures wrt concurrent calls.
				Thread.Sleep (100);
				wait_http_ctx.Set ();
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
				if (String.Compare (ctx.QueryString [null], "wsdl", StringComparison.OrdinalIgnoreCase) == 0)
					return mex_info.SupportsMex; // wsdl dispatcher should handle this.
				if (wsdl_instance.HelpUrl == null || !wsdl_instance.HelpUrl.Equals (wsdl_instance.WsdlUrl))
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
