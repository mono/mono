//
// HttpRequestWrapper.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Web.Caching;

#if NET_4_0
using System.Security.Authentication.ExtendedProtection;
using System.Web.Routing;
#endif

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpRequestWrapper : HttpRequestBase
	{
		HttpRequest w;

		public HttpRequestWrapper (HttpRequest httpRequest)
		{
			if (httpRequest == null)
				throw new ArgumentNullException ("httpRequest");
			w = httpRequest;
		}

		public override string [] AcceptTypes {
			get { return w.AcceptTypes; }
		}

		public override string AnonymousID {
			get { return w.AnonymousID; }
		}

		public override string ApplicationPath {
			get { return w.ApplicationPath; }
		}

		public override string AppRelativeCurrentExecutionFilePath {
			get { return w.AppRelativeCurrentExecutionFilePath; }
		}

		public override HttpBrowserCapabilitiesBase Browser {
			get { return new HttpBrowserCapabilitiesWrapper (w.Browser); }
		}

		public override HttpClientCertificate ClientCertificate {
			get { return w.ClientCertificate; }
		}

		public override Encoding ContentEncoding {
			get { return w.ContentEncoding; }
			set { w.ContentEncoding = value; }
		}

		public override int ContentLength {
			get { return w.ContentLength; }
		}

		public override string ContentType {
			get { return w.ContentType; }
			set { w.ContentType = value; }
		}

		public override HttpCookieCollection Cookies {
			get { return w.Cookies; }
		}

		public override string CurrentExecutionFilePath {
			get { return w.CurrentExecutionFilePath; }
		}

		public override string FilePath {
			get { return w.FilePath; }
		}

		public override HttpFileCollectionBase Files {
			get { return new HttpFileCollectionWrapper (w.Files); }
		}

		public override Stream Filter {
			get { return w.Filter; }
			set { w.Filter = value; }
		}

		public override NameValueCollection Form {
			get { return w.Form; }
		}

		public override NameValueCollection Headers {
			get { return w.Headers; }
		}

		public override string HttpMethod {
			get { return w.HttpMethod; }
		}
#if NET_4_0
		public override ChannelBinding HttpChannelBinding {
			get { return w.HttpChannelBinding; }
		}
#endif
		public override Stream InputStream {
			get { return w.InputStream; }
		}

		public override bool IsAuthenticated {
			get { return w.IsAuthenticated; }
		}

		public override bool IsLocal {
			get { return w.IsLocal; }
		}

		public override bool IsSecureConnection {
			get { return w.IsSecureConnection; }
		}

		public override string this [string key] {
			get { return w [key]; }
		}

		public override WindowsIdentity LogonUserIdentity {
			get { return w.LogonUserIdentity; }
		}

		public override NameValueCollection Params {
			get { return w.Params; }
		}

		public override string Path {
			get { return w.Path; }
		}

		public override string PathInfo {
			get { return w.PathInfo; }
		}

		public override string PhysicalApplicationPath {
			get { return w.PhysicalApplicationPath; }
		}

		public override string PhysicalPath {
			get { return w.PhysicalPath; }
		}

		public override NameValueCollection QueryString {
			get { return w.QueryString; }
		}

		public override string RawUrl {
			get { return w.RawUrl; }
		}

		public override string RequestType {
			get { return w.RequestType; }
			set { w.RequestType = value; }
		}
#if NET_4_0
		public override RequestContext RequestContext {
			get { return w.RequestContext; }
			internal set { w.RequestContext = value; }	
		}
#endif
		public override NameValueCollection ServerVariables {
			get { return w.ServerVariables; }
		}

		public override int TotalBytes {
			get { return w.TotalBytes; }
		}

		public override Uri Url {
			get { return w.Url; }
		}

		public override Uri UrlReferrer {
			get { return w.UrlReferrer; }
		}

		public override string UserAgent {
			get { return w.UserAgent; }
		}

		public override string UserHostAddress {
			get { return w.UserHostAddress; }
		}

		public override string UserHostName {
			get { return w.UserHostName; }
		}

		public override string [] UserLanguages {
			get { return w.UserLanguages; }
		}

		public override byte [] BinaryRead (int count)
		{
			return w.BinaryRead (count);
		}

		public override int [] MapImageCoordinates (string imageFieldName)
		{
			return w.MapImageCoordinates (imageFieldName);
		}

		public override string MapPath (string overridePath)
		{
			return w.MapPath (overridePath);
		}

		public override string MapPath (string overridePath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			return w.MapPath (overridePath, baseVirtualDir, allowCrossAppMapping);
		}

		public override void SaveAs (string filename, bool includeHeaders)
		{
			w.SaveAs (filename, includeHeaders);
		}

		public override void ValidateInput ()
		{
			w.ValidateInput ();
		}
	}
}
