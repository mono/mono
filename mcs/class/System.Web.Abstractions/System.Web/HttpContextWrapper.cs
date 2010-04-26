//
// HttpContextWrapper.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008-2010 Novell Inc. http://novell.com
//

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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Profile;
using System.Web.SessionState;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpContextWrapper : HttpContextBase
	{
		HttpContext w;

		public HttpContextWrapper (HttpContext httpContext)
		{
			if (httpContext == null)
				throw new ArgumentNullException ("httpContext");
			w = httpContext;
		}

		public override Exception [] AllErrors {
			get { return w.AllErrors; }
		}

		public override HttpApplicationStateBase Application {
			get { return new HttpApplicationStateWrapper (w.Application); }
		}

		public override HttpApplication ApplicationInstance {
			get { return w.ApplicationInstance; }
			set { w.ApplicationInstance = value; }
		}

		public override Cache Cache {
			get { return w.Cache; }
		}

		public override IHttpHandler CurrentHandler {
			get { return w.CurrentHandler; }
		}

		public override RequestNotification CurrentNotification {
			get { return w.CurrentNotification; }
		}

		public override Exception Error {
			get { return w.Error; }
		}

		public override IHttpHandler Handler {
			get { return w.Handler; }
			set { w.Handler = value; }
		}

		public override bool IsCustomErrorEnabled {
			get { return w.IsCustomErrorEnabled; }
		}

		public override bool IsDebuggingEnabled {
			get { return w.IsDebuggingEnabled; }
		}

		public override bool IsPostNotification {
			get { return w.IsPostNotification; }
		}

		public override IDictionary Items {
			get { return w.Items; }
		}

		public override IHttpHandler PreviousHandler {
			get { return w.PreviousHandler; }
		}

		public override ProfileBase Profile {
			get { return w.Profile; }
		}

		public override HttpRequestBase Request {
			get { return new HttpRequestWrapper (w.Request); }
		}

		public override HttpResponseBase Response {
			get { return new HttpResponseWrapper (w.Response); }
		}

		public override HttpServerUtilityBase Server {
			get { return new HttpServerUtilityWrapper (w.Server); }
		}

		public override HttpSessionStateBase Session {
			get { return new HttpSessionStateWrapper (w.Session); }
		}

		public override bool SkipAuthorization {
			get { return w.SkipAuthorization; }
			set { w.SkipAuthorization = value; }
		}

		public override DateTime Timestamp {
			get { return w.Timestamp; }
		}

		public override TraceContext Trace {
			get { return w.Trace; }
		}

		public override IPrincipal User {
			get { return w.User; }
			set { w.User = value; }
		}

		public override void AddError (Exception errorInfo)
		{
			w.AddError (errorInfo);
		}

		public override void ClearError ()
		{
			w.ClearError ();
		}

		public override object GetGlobalResourceObject (string classKey, string resourceKey)
		{
			return HttpContext.GetGlobalResourceObject (classKey, resourceKey);
		}

		public override object GetGlobalResourceObject (string classKey, string resourceKey, CultureInfo culture)
		{
			return HttpContext.GetGlobalResourceObject (classKey, resourceKey, culture);
		}

		public override object GetLocalResourceObject (string overridePath, string resourceKey)
		{
			return HttpContext.GetLocalResourceObject (overridePath, resourceKey);
		}

		public override object GetLocalResourceObject (string overridePath, string resourceKey, CultureInfo culture)
		{
			return HttpContext.GetLocalResourceObject (overridePath, resourceKey, culture);
		}

		public override object GetSection (string sectionName)
		{
			return w.GetSection (sectionName);
		}

		[MonoTODO]
		public override object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}
#if NET_4_0
		public override void RemapHandler (IHttpHandler handler)
		{
			w.RemapHandler (handler);
		}
#endif
		public override void RewritePath (string path)
		{
			w.RewritePath (path);
		}

		public override void RewritePath (string path, bool rebaseClientPath)
		{
			w.RewritePath (path, rebaseClientPath);
		}

		public override void RewritePath (string filePath, string pathInfo, string queryString)
		{
			w.RewritePath (filePath, pathInfo, queryString);
		}

		public override void RewritePath (string filePath, string pathInfo, string queryString, bool setClientFilePath)
		{
			w.RewritePath (filePath, pathInfo, queryString, setClientFilePath);
		}
#if NET_4_0
		public override void SetSessionStateBehavior (SessionStateBehavior sessionStateBehavior)
		{
			w.SetSessionStateBehavior (sessionStateBehavior);
		}
#endif
	}
}
