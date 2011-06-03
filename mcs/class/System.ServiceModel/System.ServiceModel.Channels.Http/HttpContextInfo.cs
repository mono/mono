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
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web;

namespace System.ServiceModel.Channels.Http
{
	// Context
	
	abstract class HttpContextInfo
	{
		public abstract HttpRequestInfo Request { get; }
		public abstract HttpResponseInfo Response { get; }

		public abstract string User { get; }
		public abstract string Password { get; }
		public abstract void ReturnUnauthorized ();

		public void Abort ()
		{
			Response.Abort ();
			OnContextClosed ();
		}

		public void Close ()
		{
			Response.Close ();
			OnContextClosed ();
		}
		
		protected virtual void OnContextClosed ()
		{
		}
	}

	class HttpStandaloneContextInfo : HttpContextInfo
	{
		public HttpStandaloneContextInfo (HttpListenerContext ctx)
		{
			this.ctx = ctx;
			request = new HttpStandaloneRequestInfo (ctx.Request);
			response = new HttpStandaloneResponseInfo (ctx.Response);
		}
		
		HttpListenerContext ctx;
		HttpStandaloneRequestInfo request;
		HttpStandaloneResponseInfo response;

		public HttpListenerContext Source {
			get { return ctx; }
		}

		public override HttpRequestInfo Request {
			get { return request; }
		}

		public override HttpResponseInfo Response {
			get { return response; }
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
		public AspNetHttpContextInfo (SvcHttpHandler handler, HttpContext ctx)
		{
			this.ctx = ctx;
			this.handler = handler;
			this.request = new AspNetHttpRequestInfo (ctx.Request);
			this.response = new AspNetHttpResponseInfo (ctx.Response);
		}
		
		HttpContext ctx;
		SvcHttpHandler handler;
		AspNetHttpRequestInfo request;
		AspNetHttpResponseInfo response;

		public HttpContext Source {
			get { return ctx; }
		}
		
		public override HttpRequestInfo Request {
			get { return request; }
		}

		public override HttpResponseInfo Response {
			get { return response; }
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

		protected override void OnContextClosed ()
		{
			handler.EndHttpRequest (ctx);
		}
	}

	// Request

	abstract class HttpRequestInfo
	{
		public abstract long ContentLength64 { get; }
		public abstract NameValueCollection QueryString { get; }
		public abstract NameValueCollection Headers { get; }
		public abstract Uri Url { get; }
		public abstract string ContentType { get; }
		public abstract string HttpMethod { get; }
		public abstract Stream InputStream { get; }
		public abstract string ClientIPAddress { get; }
		public abstract int ClientPort { get; }
	}

	class HttpStandaloneRequestInfo : HttpRequestInfo
	{
		public HttpStandaloneRequestInfo (HttpListenerRequest request)
		{
			this.req = request;
		}
		
		HttpListenerRequest req;

		public override long ContentLength64 {
			get { return req.ContentLength64; }
		}
		public override NameValueCollection QueryString {
			get { return req.QueryString; }
		}
		public override NameValueCollection Headers {
			get { return req.Headers; }
		}
		public override Uri Url {
			get { return req.Url; }
		}
		public override string ContentType {
			get { return req.ContentType; }
		}
		public override string HttpMethod {
			get { return req.HttpMethod; }
		}
		public override Stream InputStream {
			get { return req.InputStream; }
		}
		public override string ClientIPAddress {
			get { return req.RemoteEndPoint.Address.ToString (); }
		}
		public override int ClientPort {
			get { return req.RemoteEndPoint.Port; }
		}
	}

	class AspNetHttpRequestInfo : HttpRequestInfo
	{
		public AspNetHttpRequestInfo (HttpRequest request)
		{
			this.req = request;
		}
		
		HttpRequest req;

		public override long ContentLength64 {
			get { return req.ContentLength; }
		}
		public override NameValueCollection QueryString {
			get { return req.QueryString; }
		}
		public override NameValueCollection Headers {
			get { return req.Headers; }
		}
		public override Uri Url {
			get { return req.Url; }
		}
		public override string ContentType {
			get { return req.ContentType; }
		}
		public override string HttpMethod {
			get { return req.HttpMethod; }
		}
		public override Stream InputStream {
			get { return req.InputStream; }
		}
		public override string ClientIPAddress {
			get { return req.UserHostAddress; }
		}
		public override int ClientPort {
			get { return -1; } // cannot retrieve
		}
	}
	
	// Response
	
	abstract class HttpResponseInfo
	{
		public abstract string ContentType { get; set; }
		public abstract NameValueCollection Headers { get; }
		public abstract Stream OutputStream { get; }
		public abstract int StatusCode { get; set; }
		public abstract string StatusDescription { get; set; }
		public abstract void Abort ();
		public abstract void Close ();
		public abstract void SetLength (long value);
		
		public virtual bool SuppressContent { get; set; }
	}

	class HttpStandaloneResponseInfo : HttpResponseInfo
	{
		public HttpStandaloneResponseInfo (HttpListenerResponse response)
		{
			this.res = response;
		}
		
		HttpListenerResponse res;

		public override string ContentType {
			get { return res.ContentType; }
			set { res.ContentType = value; }
		}
		public override NameValueCollection Headers {
			get { return res.Headers; }
		}
		public override int StatusCode {
			get { return res.StatusCode; }
			set { res.StatusCode = value; }
		}
		public override string StatusDescription {
			get { return res.StatusDescription; }
			set { res.StatusDescription = value; }
		}
		public override Stream OutputStream {
			get { return res.OutputStream; }
		}
		
		public override void Abort ()
		{
			res.Abort ();
		}
		
		public override void Close ()
		{
			res.Close ();
		}
		
		public override void SetLength (long value)
		{
			res.ContentLength64 = value;
		}
	}

	class AspNetHttpResponseInfo : HttpResponseInfo
	{
		public AspNetHttpResponseInfo (HttpResponse response)
		{
			this.res = response;
		}
		
		HttpResponse res;
		
		public override bool SuppressContent {
			get { return res.SuppressContent; }
			set { res.SuppressContent = value; }
		}
		public override string ContentType {
			get { return res.ContentType; }
			set { res.ContentType = value; }
		}
		public override NameValueCollection Headers {
			get { return res.Headers; }
		}
		public override int StatusCode {
			get { return res.StatusCode; }
			set { res.StatusCode = value; }
		}
		
		public override string StatusDescription {
			get { return res.StatusDescription; }
			set { res.StatusDescription = value; }
		}
		public override Stream OutputStream {
			get { return res.OutputStream; }
		}
		
		public override void Abort ()
		{
			res.End ();
		}
		
		public override void Close ()
		{
			// We must not close the response here, as everything is taking place in the
			// HttpApplication's pipeline context and the output is sent to the client
			// _after_ we leave this method. Closing the response here will stop any
			// output from reaching the client.
		}
		
		public override void SetLength (long value)
		{
			res.AddHeader ("Content-Length", value.ToString (CultureInfo.InvariantCulture));
		}
	}
}
