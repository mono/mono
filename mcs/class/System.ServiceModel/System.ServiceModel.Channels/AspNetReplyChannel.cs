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
		HttpContext http_context;
		AspNetChannelListener<IReplyChannel> listener;
		Uri uri;

		public AspNetReplyChannel (AspNetChannelListener<IReplyChannel> listener)
			: base (listener)
		{
			this.listener = listener;
			uri = listener.Uri;
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			try {
				return TryReceiveRequestCore (timeout, out context);
			} catch (Exception ex) {
				// FIXME: log it
				Console.WriteLine ("AspNetReplyChannel caught an error: " + ex);
				throw;
			} finally {
				listener.HttpHandler.EndRequest (http_context);
				http_context = null;
			}
		}

		bool TryReceiveRequestCore (TimeSpan timeout, out RequestContext context)
		{
			context = null;
			if (!WaitForRequest (timeout))
				return false;

			Message msg;
			// FIXME: remove this hack
			if (http_context.Request.HttpMethod == "GET") {
				if (http_context.Request.QueryString ["wsdl"] != null) {
					msg = Message.CreateMessage (Encoder.MessageVersion,
						"http://schemas.xmlsoap.org/ws/2004/09/transfer/Get");
					msg.Headers.To = http_context.Request.Url;
				} else {
					msg = Message.CreateMessage (Encoder.MessageVersion, null);
					msg.Headers.To = http_context.Request.Url;
				}
			} else {
				//FIXME: Do above stuff for HttpContext ?
				int maxSizeOfHeaders = 0x10000;

				msg = Encoder.ReadMessage (
					http_context.Request.InputStream, maxSizeOfHeaders);

				if (Encoder.MessageVersion.Envelope == EnvelopeVersion.Soap11) {
					string action = GetHeaderItem (http_context.Request.Headers ["SOAPAction"]);
					if (action != null)
						msg.Headers.Action = action;
				}
			}
			context = new AspNetRequestContext (this, msg, http_context);

			return true;
		}

		public override bool WaitForRequest (TimeSpan timeout)
		{
			if (http_context == null)
				http_context = listener.HttpHandler.WaitForRequest (timeout);
			return http_context != null;
		}
	}
}
