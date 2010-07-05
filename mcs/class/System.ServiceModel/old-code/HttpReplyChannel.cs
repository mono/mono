//
// HttpReplyChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class HttpSimpleReplyChannel : HttpReplyChannel
	{
		HttpSimpleChannelListener<IReplyChannel> source;
		List<HttpListenerContext> waiting = new List<HttpListenerContext> ();
		RequestContext reqctx;

		public HttpSimpleReplyChannel (HttpSimpleChannelListener<IReplyChannel> listener)
			: base (listener)
		{
			this.source = listener;
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

		void SignalAsyncWait ()
		{
			if (wait == null)
				return;
			var wait_ = wait;
			wait = null;
			wait_.Set ();
		}

		void AbortConnections (TimeSpan timeout)
		{
			// FIXME: use timeout
			lock (waiting)
				foreach (var ctx in waiting)
					ctx.Response.Close ();
			if (wait != null)
				source.ListenerManager.CancelGetHttpContextAsync ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			if (reqctx != null)
				reqctx.Close (timeout);

			// FIXME: consider timeout
			AbortConnections (timeout - (DateTime.Now - start));

			base.OnClose (timeout - (DateTime.Now - start));
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			context = null;
			if (waiting.Count == 0 && !WaitForRequest (timeout))
				return false;
			HttpListenerContext ctx = null;
			lock (waiting) {
				if (waiting.Count > 0) {
					ctx = waiting [0];
					waiting.RemoveAt (0);
				}
			}
			if (ctx == null)
				// Though as long as this instance is used
				// synchronously, it should not happen.
				return false;
			if (ctx.Response.StatusCode != 200) {
				ctx.Response.Close ();
				return false;
			}

			// FIXME: supply maxSizeOfHeaders.
			int maxSizeOfHeaders = 0x10000;

			Message msg = null;

			if (ctx.Request.HttpMethod == "POST") {
				if (!Encoder.IsContentTypeSupported (ctx.Request.ContentType)) {
					ctx.Response.StatusCode = (int) HttpStatusCode.UnsupportedMediaType;
					ctx.Response.StatusDescription = String.Format (
							"Expected content-type '{0}' but got '{1}'", Encoder.ContentType, ctx.Request.ContentType);
					ctx.Response.Close ();

					return false;
				}

				msg = Encoder.ReadMessage (
					ctx.Request.InputStream, maxSizeOfHeaders);

				if (MessageVersion.Envelope.Equals (EnvelopeVersion.Soap11) ||
				    MessageVersion.Addressing.Equals (AddressingVersion.None)) {
					string action = GetHeaderItem (ctx.Request.Headers ["SOAPAction"]);
					if (action != null) {
						if (action.Length > 2 && action [0] == '"' && action [action.Length] == '"')
							action = action.Substring (1, action.Length - 2);
						msg.Headers.Action = action;
					}
				}
			} else if (ctx.Request.HttpMethod == "GET") {
				msg = Message.CreateMessage (MessageVersion.None, null);
			}
			if (msg.Headers.To == null)
				msg.Headers.To = ctx.Request.Url;
			msg.Properties.Add ("Via", LocalAddress.Uri);
			msg.Properties.Add (HttpRequestMessageProperty.Name, CreateRequestProperty (ctx.Request.HttpMethod, ctx.Request.Url.Query, ctx.Request.Headers));
/*
MessageBuffer buf = msg.CreateBufferedCopy (0x10000);
msg = buf.CreateMessage ();
Console.WriteLine (buf.CreateMessage ());
*/
			context = new HttpRequestContext (this, msg, ctx);
			reqctx = context;
			return true;
		}

		ManualResetEvent wait;

		public override bool WaitForRequest (TimeSpan timeout)
		{
			if (wait != null)
				throw new InvalidOperationException ("Another wait operation is in progress");
			try {
				var wait_ = new ManualResetEvent (false);
				wait = wait_;	// wait can be set to null if HttpContextAcquired runs to completion before we do WaitOne
				source.ListenerManager.GetHttpContextAsync (timeout, HttpContextAcquired);
				return wait_.WaitOne (timeout, false) && waiting.Count > 0;
			} catch (HttpListenerException e) {
				// FIXME: does this make sense? I doubt.
				if ((uint) e.ErrorCode == 0x80004005) // invalid handle. Happens during shutdown.
					while (true) Thread.Sleep (1000); // thread is about to be terminated.
				throw;
			} catch (ObjectDisposedException) {
				return false;
			} finally {
				wait = null;
			}
		}

		void HttpContextAcquired (HttpContextInfo ctx)
		{
			if (wait == null)
				throw new InvalidOperationException ("WaitForRequest operation has not started");
			var sctx = (HttpListenerContextInfo) ctx;
			if (State == CommunicationState.Opened && ctx != null)
				lock (waiting)
					waiting.Add (sctx.Source);
			SignalAsyncWait ();
		}
	}

	internal abstract class HttpReplyChannel : InternalReplyChannelBase
	{
		HttpChannelListenerBase<IReplyChannel> source;

		public HttpReplyChannel (HttpChannelListenerBase<IReplyChannel> listener)
			: base (listener)
		{
			this.source = listener;
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
			TryReceiveRequest (timeout, out ctx);
			return ctx;
		}

		protected override void OnOpen (TimeSpan timeout)
		{
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

		protected HttpRequestMessageProperty CreateRequestProperty (string method, string query, NameValueCollection headers)
		{
			var prop = new HttpRequestMessageProperty ();
			prop.Method = method;
			prop.QueryString = query.StartsWith ("?") ? query.Substring (1) : query;
			// FIXME: prop.SuppressEntityBody
			prop.Headers.Add (headers);
			return prop;
		}
	}
}
