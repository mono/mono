//
// HttpReplyChannel.cs
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
using System.Collections.Specialized;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels.Http
{
	internal class HttpReplyChannel : InternalReplyChannelBase
	{
		HttpChannelListener<IReplyChannel> source;
		RequestContext reqctx;
		SecurityTokenAuthenticator security_token_authenticator;
		SecurityTokenResolver security_token_resolver;

		public HttpReplyChannel (HttpChannelListener<IReplyChannel> listener)
			: base (listener)
		{
			this.source = listener;

			if (listener.SecurityTokenManager != null) {
				var str = new SecurityTokenRequirement () { TokenType = SecurityTokenTypes.UserName };
				security_token_authenticator = listener.SecurityTokenManager.CreateSecurityTokenAuthenticator (str, out security_token_resolver);
			}
		}

		internal HttpChannelListener<IReplyChannel> Source {
			get { return source; }
		}

		public MessageEncoder Encoder {
			get { return source.MessageEncoder; }
		}

		internal MessageVersion MessageVersion {
			get { return source.MessageEncoder.MessageVersion; }
		}

		public override RequestContext ReceiveRequest (TimeSpan timeout)
		{
			RequestContext ctx;
			if (!TryReceiveRequest (timeout, out ctx))
				throw new TimeoutException ();
			return ctx;
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		protected override void OnAbort ()
		{
			AbortConnections (TimeSpan.Zero);
			base.OnAbort (); // FIXME: remove it. The base is wrong. But it is somehow required to not block some tests.
		}

		public override bool CancelAsync (TimeSpan timeout)
		{
			AbortConnections (timeout);
			// FIXME: this wait is sort of hack (because it should not be required), but without it some tests are blocked.
			// This hack even had better be moved to base.CancelAsync().
			if (CurrentAsyncResult != null)
				CurrentAsyncResult.AsyncWaitHandle.WaitOne (TimeSpan.FromMilliseconds (300));
			return base.CancelAsync (timeout);
		}

		void AbortConnections (TimeSpan timeout)
		{
			if (reqctx != null)
				reqctx.Close (timeout);
		}

		bool close_started;

		protected override void OnClose (TimeSpan timeout)
		{
			if (close_started)
				return;
			close_started = true;
			DateTime start = DateTime.Now;

			// FIXME: consider timeout
			AbortConnections (timeout - (DateTime.Now - start));

			base.OnClose (timeout - (DateTime.Now - start));
		}

		protected string GetHeaderItem (string raw)
		{
			if (raw == null || raw.Length == 0)
				return raw;
			switch (raw [0]) {
			case '\'':
			case '"':
				if (raw [raw.Length - 1] == raw [0])
					return raw.Substring (1, raw.Length - 2);
				// FIXME: is it simply an error?
				break;
			}
			return raw;
		}

		protected HttpRequestMessageProperty CreateRequestProperty (HttpContextInfo ctxi)
		{
			var query = ctxi.Request.Url.Query;
			var prop = new HttpRequestMessageProperty ();
			prop.Method = ctxi.Request.HttpMethod;
			prop.QueryString = query.StartsWith ("?") ? query.Substring (1) : query;
			// FIXME: prop.SuppressEntityBody
			prop.Headers.Add (ctxi.Request.Headers);
			return prop;
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			context = null;
			HttpContextInfo ctxi;
			if (!source.ListenerManager.TryDequeueRequest (source.ChannelDispatcher, timeout, out ctxi))
				return false;
			if (ctxi == null)
				return true; // returning true, yet context is null. This happens at closing phase.

			if (source.Source.AuthenticationScheme != AuthenticationSchemes.Anonymous) {
				if (security_token_authenticator != null)
					// FIXME: use return value?
					try {
						security_token_authenticator.ValidateToken (new UserNameSecurityToken (ctxi.User, ctxi.Password));
					} catch (Exception) {
						ctxi.ReturnUnauthorized ();
					}
				else {
					ctxi.ReturnUnauthorized ();
				}
			}

			Message msg = null;

			if (ctxi.Request.HttpMethod == "POST") {
				msg = CreatePostMessage (ctxi);
				if (msg == null)
					return false;
			} else if (ctxi.Request.HttpMethod == "GET")
				msg = Message.CreateMessage (MessageVersion.None, null); // HTTP GET-based request

			if (msg.Headers.To == null)
				msg.Headers.To = ctxi.Request.Url;
			msg.Properties.Add ("Via", LocalAddress.Uri);
			msg.Properties.Add (HttpRequestMessageProperty.Name, CreateRequestProperty (ctxi));

			Logger.LogMessage (MessageLogSourceKind.TransportReceive, ref msg, source.Source.MaxReceivedMessageSize);

			context = new HttpRequestContext (this, ctxi, msg);
			reqctx = context;
			return true;
		}

		protected Message CreatePostMessage (HttpContextInfo ctxi)
		{
			if (ctxi.Response.StatusCode != 200) { // it's already invalid.
				ctxi.Close ();
				return null;
			}

			if (!Encoder.IsContentTypeSupported (ctxi.Request.ContentType)) {
				ctxi.Response.StatusCode = (int) HttpStatusCode.UnsupportedMediaType;
				ctxi.Response.StatusDescription = String.Format (
						"Expected content-type '{0}' but got '{1}'", Encoder.ContentType, ctxi.Request.ContentType);
				ctxi.Close ();

				return null;
			}

			// FIXME: supply maxSizeOfHeaders.
			int maxSizeOfHeaders = 0x10000;

#if false // FIXME: enable it, once duplex callback test gets passed.
			Stream stream = ctxi.Request.InputStream;
			if (source.Source.TransferMode == TransferMode.Buffered) {
				if (ctxi.Request.ContentLength64 <= 0)
					throw new ArgumentException ("This HTTP channel is configured to use buffered mode, and thus expects Content-Length sent to the listener");
				long size = 0;
				var ms = new MemoryStream ();
				var buf = new byte [0x1000];
				while (size < ctxi.Request.ContentLength64) {
					if ((size += stream.Read (buf, 0, 0x1000)) > source.Source.MaxBufferSize)
						throw new QuotaExceededException ("Message quota exceeded");
					ms.Write (buf, 0, (int) (size - ms.Length));
				}
				ms.Position = 0;
				stream = ms;
			}

			var msg = Encoder.ReadMessage (
				stream, maxSizeOfHeaders, ctxi.Request.ContentType);
#else
			var msg = Encoder.ReadMessage (
				ctxi.Request.InputStream, maxSizeOfHeaders, ctxi.Request.ContentType);
#endif

			if (MessageVersion.Envelope.Equals (EnvelopeVersion.Soap11) ||
			    MessageVersion.Addressing.Equals (AddressingVersion.None)) {
				string action = GetHeaderItem (ctxi.Request.Headers ["SOAPAction"]);
				if (action != null) {
					if (action.Length > 2 && action [0] == '"' && action [action.Length] == '"')
						action = action.Substring (1, action.Length - 2);
					msg.Headers.Action = action;
				}
			}
			msg.Properties.Add (RemoteEndpointMessageProperty.Name, new RemoteEndpointMessageProperty (ctxi.Request.ClientIPAddress, ctxi.Request.ClientPort));

			return msg;
		}

		public override bool WaitForRequest (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
