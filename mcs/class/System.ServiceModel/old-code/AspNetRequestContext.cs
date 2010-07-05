//
// AspNetRequestContext.cs
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

using System.Globalization;
using System.IO;
using System.Net;
using System.Web;

namespace System.ServiceModel.Channels {

	class AspNetRequestContext : HttpRequestContextBase
	{
		AspNetReplyChannel channel;
		HttpContext ctx;

		public AspNetRequestContext (
			AspNetReplyChannel channel,
			Message msg, HttpContext ctx)
			: base (channel, msg)
		{
			this.channel = channel;
			this.ctx = ctx;
		}

		public override void Abort ()
		{
			//ctx.Response.Abort ();
			ctx = null;
			channel.CloseContext ();
		}

		protected override void ProcessReply (Message msg, TimeSpan timeout)
		{
			ctx.Response.ContentType = Channel.Encoder.ContentType;
			MemoryStream ms = new MemoryStream ();
			Channel.Encoder.WriteMessage (msg, ms);

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
				ctx.Response.AddHeader ("Content-Length", ms.Length.ToString (CultureInfo.InvariantCulture));
				ctx.Response.OutputStream.Write (ms.GetBuffer (), 0, (int) ms.Length);
				ctx.Response.OutputStream.Flush ();
			}
			else
				ctx.Response.SuppressContent = true;
		}

		public override void Close (TimeSpan timeout)
		{
			ctx = null;
			channel.CloseContext ();
		}
	}
}
