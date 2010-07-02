//
// HttpContextInfo.cs
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
using System.IO;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web;

namespace System.ServiceModel.Channels.Http
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

	class HttpStandaloneContextInfo : HttpContextInfo
	{
		public HttpStandaloneContextInfo (HttpListenerContext ctx)
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
}
