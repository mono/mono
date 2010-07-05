//
// AspNetReplyChannel.cs
//
// Author:
//	Ankit Jain  <jankit@novell.com>
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
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web;

namespace System.ServiceModel.Channels
{
	internal class AspNetReplyChannel : HttpReplyChannel
	{
		AspNetChannelListener<IReplyChannel> listener;
		List<HttpContext> waiting = new List<HttpContext> ();
		HttpContext http_context;
		ManualResetEvent wait;

		public AspNetReplyChannel (AspNetChannelListener<IReplyChannel> listener)
			: base (listener)
		{
			this.listener = listener;
		}

		internal void CloseContext ()
		{
			if (http_context == null)
				return;
			try {
				((AspNetListenerManager) listener.ListenerManager).HttpHandler.EndRequest (listener, http_context);
			} finally {
				http_context = null;
			}
		}

		void ShutdownPendingRequests ()
		{
			lock (waiting)
				foreach (HttpContext ctx in waiting)
					try {
						((AspNetListenerManager) listener.ListenerManager).HttpHandler.EndRequest (listener, ctx);
					} catch {
					}
		}

		protected override void OnAbort ()
		{
			ShutdownPendingRequests ();
			CloseContext ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			base.OnClose (timeout);

			ShutdownPendingRequests ();
			CloseContext ();
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			try {
				return TryReceiveRequestCore (timeout, out context);
			} catch (Exception ex) {
				// FIXME: log it
				Console.WriteLine ("AspNetReplyChannel caught an error: " + ex);
				throw;
			}
		}

		bool TryReceiveRequestCore (TimeSpan timeout, out RequestContext context)
		{
			context = null;
			if (waiting.Count == 0 && !WaitForRequest (timeout))
				return false;
			lock (waiting) {
				if (waiting.Count > 0) {
					http_context = waiting [0];
					waiting.RemoveAt (0);
				}
			}
			if (http_context == null) 
				// Though as long as this instance is used
				// synchronously, it should not happen.
				return false;
			if (http_context.Response.StatusCode != 200) {
				http_context.Response.Close ();
				return false;
			}

			Message msg;
			var req = http_context.Request;
			if (req.HttpMethod == "GET") {
				msg = Message.CreateMessage (Encoder.MessageVersion, null);
			} else {
				//FIXME: Do above stuff for HttpContext ?
				int maxSizeOfHeaders = 0x10000;

				msg = Encoder.ReadMessage (
					req.InputStream, maxSizeOfHeaders);

				if (Encoder.MessageVersion.Envelope == EnvelopeVersion.Soap11) {
					string action = GetHeaderItem (req.Headers ["SOAPAction"]);
					if (action != null)
						msg.Headers.Action = action;
				}
			}

			// FIXME: prop.SuppressEntityBody
			msg.Headers.To = req.Url;
			msg.Properties.Add ("Via", LocalAddress.Uri);
			msg.Properties.Add (HttpRequestMessageProperty.Name, CreateRequestProperty (req.HttpMethod, req.Url.Query, req.Headers));

			context = new AspNetRequestContext (this, msg, http_context);

			return true;
		}

		/*
		public override bool WaitForRequest (TimeSpan timeout)
		{
			if (http_context == null)
				http_context = listener.HttpHandler.WaitForRequest (listener, timeout);
			return http_context != null;
		}
		*/

		public override bool WaitForRequest (TimeSpan timeout)
		{
			if (wait != null)
				throw new InvalidOperationException ("Another wait operation is in progress");
			try {
				var wait_ = new ManualResetEvent (false);
				wait = wait_;
				listener.ListenerManager.GetHttpContextAsync (timeout, HttpContextAcquired);
				return wait_.WaitOne (timeout, false) && waiting.Count > 0;
			} finally {
				wait = null;
			}
		}

		void HttpContextAcquired (HttpContextInfo ctx)
		{
			if (wait == null)
				throw new InvalidOperationException ("WaitForRequest operation has not started");
			var sctx = (AspNetHttpContextInfo) ctx;
			if (State == CommunicationState.Opened && ctx != null)
				waiting.Add (sctx.Source);
			var wait_ = wait;
			wait = null;
			wait_.Set ();
		}
	}
}
