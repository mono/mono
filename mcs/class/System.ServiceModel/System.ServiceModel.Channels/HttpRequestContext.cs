//
// HttpRequestContext.cs
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

using System.IO;
using System.Net;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class HttpRequestContext : HttpRequestContextBase
	{
		HttpListenerContext ctx;

		public HttpRequestContext (
			HttpReplyChannel channel,
			Message msg, HttpListenerContext ctx)
			: base (channel, msg)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");
			this.ctx = ctx;
		}

		public override void Abort ()
		{
			ctx.Response.Abort ();
		}

		protected override void ProcessReply (Message msg, TimeSpan timeout)
		{
			if (msg == null)
				throw new ArgumentNullException ("msg");

			// FIXME: probably in WebHttpBinding land, there should 
			// be some additional code (probably IErrorHandler) that
			// treats DestinationUnreachable (and possibly any other)
			// errors as HTTP 400 or something appropriate. 
			// I originally rewrote the HTTP status here, but it 
			// was wrong.

			// FIXME: should this be done here?
			if (Channel.MessageVersion.Addressing.Equals (AddressingVersion.None))
				msg.Headers.Action = null; // prohibited

			MemoryStream ms = new MemoryStream ();
			Channel.Encoder.WriteMessage (msg, ms);
			ctx.Response.ContentType = Channel.Encoder.ContentType;

			string pname = HttpResponseMessageProperty.Name;
			bool suppressEntityBody = false;
			if (msg.Properties.ContainsKey (pname)) {
				HttpResponseMessageProperty hp = (HttpResponseMessageProperty) msg.Properties [pname];
				string contentType = hp.Headers ["Content-Type"];
				if (contentType != null)
					ctx.Response.ContentType = contentType;
				ctx.Response.Headers.Add (hp.Headers);
				if (hp.StatusCode != default (HttpStatusCode))
					ctx.Response.StatusCode = (int) hp.StatusCode;
				ctx.Response.StatusDescription = hp.StatusDescription;
				if (hp.SuppressEntityBody)
					suppressEntityBody = true;
			}
			if (msg.IsFault)
				ctx.Response.StatusCode = 500;
			if (!suppressEntityBody) {
				ctx.Response.ContentLength64 = ms.Length;
				ctx.Response.OutputStream.Write (ms.GetBuffer (), 0, (int) ms.Length);
				ctx.Response.OutputStream.Flush ();
			}
		}

		public override void Close (TimeSpan timeout)
		{
			ctx.Response.Close ();
		}
	}

	internal abstract class HttpRequestContextBase : RequestContext
	{
		Message request;
		HttpReplyChannel channel;

		public HttpRequestContextBase (
			HttpReplyChannel channel,
			Message request)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (request == null)
				throw new ArgumentNullException ("request");
			this.channel = channel;
			this.request = request;
		}

		public override Message RequestMessage {
			get { return request; }
		}

		public HttpReplyChannel Channel {
			get { return channel; }
		}

		protected abstract void ProcessReply (Message msg, TimeSpan timeout);

		public override IAsyncResult BeginReply (
			Message msg, AsyncCallback callback, object state)
		{
			return BeginReply (msg,
				channel.DefaultSendTimeout,
				callback, state);
		}

		Action<Message,TimeSpan> reply_delegate;

		public override IAsyncResult BeginReply (
			Message msg, TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (reply_delegate == null)
				reply_delegate = new Action<Message,TimeSpan> (Reply);
			return reply_delegate.BeginInvoke (msg, timeout, callback, state);
		}

		public override void EndReply (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			if (reply_delegate == null)
				throw new InvalidOperationException ("reply operation has not started");
			reply_delegate.EndInvoke (result);
		}

		public override void Reply (Message msg)
		{
			Reply (msg, channel.DefaultSendTimeout);
		}

		public override void Reply (Message msg, TimeSpan timeout)
		{
			ProcessReply (msg, timeout);
		}

		public override void Close ()
		{
			Close (Channel.DefaultSendTimeout);
		}
	}
}
