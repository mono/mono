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

using System.IO;
using System.Web;

namespace System.ServiceModel.Channels {

	class AspNetRequestContext : HttpRequestContextBase
	{
		AspNetReplyChannel owner;
		HttpContext ctx;

		public AspNetRequestContext (
			AspNetReplyChannel channel,
			Message msg, HttpContext ctx)
			: base (channel, msg)
		{
			this.owner = owner;
			this.ctx = ctx;
		}

		public override void Abort ()
		{
			//ctx.Response.Abort ();
			ctx = null;
			owner.CloseContext ();
		}

		protected override void ProcessReply (Message msg, TimeSpan timeout)
		{
Console.WriteLine ("ProcessReply for " + msg.Headers.Action + " with " + Channel.Encoder.GetType ());
			ctx.Response.ContentType = Channel.Encoder.ContentType;
			MemoryStream ms = new MemoryStream ();
			Channel.Encoder.WriteMessage (msg, ms);
			//ctx.Response.ContentLength64 = ms.Length;
			ctx.Response.OutputStream.Write (ms.GetBuffer (), 0, (int) ms.Length);
			ctx.Response.OutputStream.Flush ();
		}

		public override void Close (TimeSpan timeout)
		{
			// FIXME: use timeout
			try {
				ctx.Response.Close ();
			} finally {
				ctx = null;
				owner.CloseContext ();
			}
		}
	}
}
