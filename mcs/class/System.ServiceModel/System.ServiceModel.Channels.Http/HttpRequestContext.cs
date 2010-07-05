//
// HttpRequestContext.cs
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

using System.IO;
using System.Net;
using System.Threading;

namespace System.ServiceModel.Channels.Http
{
	internal class HttpStandaloneRequestContext : RequestContext
	{
		public HttpStandaloneRequestContext (HttpStandaloneReplyChannel channel, HttpContextInfo context, Message request)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			if (context == null)
				throw new ArgumentNullException ("context");
			if (request == null)
				throw new ArgumentNullException ("request");
			this.channel = channel;
			this.context = context;
			this.request = request;
		}

		Message request;
		HttpStandaloneReplyChannel channel;
		HttpContextInfo context;

		public override Message RequestMessage {
			get { return request; }
		}

		public HttpStandaloneReplyChannel Channel {
			get { return channel; }
		}
		
		public HttpContextInfo Context {
			get { return context; }
		}

		public override IAsyncResult BeginReply (
			Message msg, AsyncCallback callback, object state)
		{
			return BeginReply (msg,
				Channel.DefaultSendTimeout,
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
			Reply (msg, Channel.DefaultSendTimeout);
		}

		public override void Reply (Message msg, TimeSpan timeout)
		{
			InternalReply (msg, timeout);
		}

		public override void Abort ()
		{
			InternalAbort ();
		}

		public override void Close ()
		{
			Close (Channel.DefaultSendTimeout);
		}

		public override void Close (TimeSpan timeout)
		{
			InternalClose (timeout);
		}
		
		// implementation internals
		
		protected virtual void InternalAbort ()
		{
			Context.Abort ();
		}
		
		protected virtual void InternalClose (TimeSpan timeout)
		{
			Context.Close ();
		}

		protected virtual void InternalReply (Message msg, TimeSpan timeout)
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
			Context.Response.ContentType = Channel.Encoder.ContentType;

			string pname = HttpResponseMessageProperty.Name;
			bool suppressEntityBody = false;
			if (msg.Properties.ContainsKey (pname)) {
				HttpResponseMessageProperty hp = (HttpResponseMessageProperty) msg.Properties [pname];
				string contentType = hp.Headers ["Content-Type"];
				if (contentType != null)
					Context.Response.ContentType = contentType;
				Context.Response.Headers.Add (hp.Headers);
				if (hp.StatusCode != default (HttpStatusCode))
					Context.Response.StatusCode = (int) hp.StatusCode;
				Context.Response.StatusDescription = hp.StatusDescription;
				if (hp.SuppressEntityBody)
					suppressEntityBody = true;
			}
			if (msg.IsFault)
				Context.Response.StatusCode = 500;
			if (!suppressEntityBody) {
				Context.Response.SetLength (ms.Length);
				Context.Response.OutputStream.Write (ms.GetBuffer (), 0, (int) ms.Length);
				Context.Response.OutputStream.Flush ();
			}
			else
				Context.Response.SuppressContent = true;
		}
	}
}
