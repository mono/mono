//
// HttpResponseWrapper.cs
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

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpResponseWrapper : HttpResponseBase
	{
		HttpResponse w;

		public HttpResponseWrapper (HttpResponse httpResponse)
		{
			if (httpResponse == null)
				throw new ArgumentNullException ("httpResponse");
			w = httpResponse;
		}

		public override bool Buffer {
			get { return w.Buffer; }
			set { w.Buffer = value; }
		}

		public override bool BufferOutput {
			get { return w.BufferOutput; }
			set { w.BufferOutput = value; }
		}

		public override HttpCachePolicyBase Cache {
			get { return new HttpCachePolicyWrapper (w.Cache); }
		}

		public override string CacheControl {
			get { return w.CacheControl; }
			set { w.CacheControl = value; }
		}

		public override string Charset {
			get { return w.Charset; }
			set { w.Charset = value; }
		}

		public override Encoding ContentEncoding {
			get { return w.ContentEncoding; }
			set { w.ContentEncoding = value; }
		}

		public override string ContentType {
			get { return w.ContentType; }
			set { w.ContentType = value; }
		}

		public override HttpCookieCollection Cookies {
			get { return w.Cookies; }
		}

		public override int Expires {
			get { return w.Expires; }
			set { w.Expires = value; }
		}

		public override DateTime ExpiresAbsolute {
			get { return w.ExpiresAbsolute; }
			set { w.ExpiresAbsolute = value; }
		}

		public override Stream Filter {
			get { return w.Filter; }
			set { w.Filter = value; }
		}

		public override Encoding HeaderEncoding {
			get { return w.HeaderEncoding; }
			set { w.HeaderEncoding = value; }
		}

		public override NameValueCollection Headers {
			get { return w.Headers; }
		}

		public override bool IsClientConnected {
			get { return w.IsClientConnected; }
		}

		public override bool IsRequestBeingRedirected {
			get { return w.IsRequestBeingRedirected; }
		}

		public override TextWriter Output {
			get { return w.Output; }
#if NET_4_0
			set { w.Output = value; }
#endif
		}

		public override Stream OutputStream {
			get { return w.OutputStream; }
		}

		public override string RedirectLocation {
			get { return w.RedirectLocation; }
			set { w.RedirectLocation = value; }
		}

		public override string Status {
			get { return w.Status; }
			set { w.Status = value; }
		}

		public override int StatusCode {
			get { return w.StatusCode; }
			set { w.StatusCode = value; }
		}

		public override string StatusDescription {
			get { return w.StatusDescription; }
			set { w.StatusDescription = value; }
		}

		public override int SubStatusCode {
			get { return w.SubStatusCode; }
			set { w.SubStatusCode = value; }
		}

		public override bool SuppressContent {
			get { return w.SuppressContent; }
			set { w.SuppressContent = value; }
		}

		public override bool TrySkipIisCustomErrors {
			get { return w.TrySkipIisCustomErrors; }
			set { w.TrySkipIisCustomErrors = value; }
		}

		public override void AddCacheDependency (params CacheDependency [] dependencies)
		{
			w.AddCacheDependency (dependencies);
		}

		public override void AddCacheItemDependencies (ArrayList cacheKeys)
		{
			w.AddCacheItemDependencies (cacheKeys);
		}

		public override void AddCacheItemDependencies (string [] cacheKeys)
		{
			w.AddCacheItemDependencies (cacheKeys);
		}

		public override void AddCacheItemDependency (string cacheKey)
		{
			w.AddCacheItemDependency (cacheKey);
		}

		public override void AddFileDependencies (ArrayList filenames)
		{
			w.AddFileDependencies (filenames);
		}

		public override void AddFileDependencies (string [] filenames)
		{
			w.AddFileDependencies (filenames);
		}

		public override void AddFileDependency (string filename)
		{
			w.AddFileDependency (filename);
		}

		public override void AddHeader (string name, string value)
		{
			w.AddHeader (name, value);
		}

		public override void AppendCookie (HttpCookie cookie)
		{
			w.AppendCookie (cookie);
		}

		public override void AppendHeader (string name, string value)
		{
			w.AppendHeader (name, value);
		}

		public override void AppendToLog (string param)
		{
			w.AppendToLog (param);
		}

		public override string ApplyAppPathModifier (string overridePath)
		{
			return w.ApplyAppPathModifier (overridePath);
		}

		public override void BinaryWrite (byte [] buffer)
		{
			w.BinaryWrite (buffer);
		}

		public override void Clear ()
		{
			w.Clear ();
		}

		public override void ClearContent ()
		{
			w.ClearContent ();
		}

		public override void ClearHeaders ()
		{
			w.ClearHeaders ();
		}

		public override void Close ()
		{
			w.Close ();
		}

		public override void DisableKernelCache ()
		{
			 w.DisableKernelCache ();
		}

		public override void End ()
		{
			w.End ();
		}

		public override void Flush ()
		{
			w.Flush ();
		}

		public override void Pics (string value)
		{
			w.Pics (value);
		}

		public override void Redirect (string url)
		{
			w.Redirect (url);
		}

		public override void Redirect (string url, bool endResponse)
		{
			w.Redirect (url, endResponse);
		}
#if NET_4_0
		public override void RedirectPermanent (string url)
		{
			w.RedirectPermanent (url);
		}

		public override void RedirectPermanent (string url, bool endResponse)
		{
			w.RedirectPermanent (url, endResponse);
		}

		public override void RemoveOutputCacheItem (string path, string providerName)
		{
			HttpResponse.RemoveOutputCacheItem (path, providerName);
		}
#endif
		public override void RemoveOutputCacheItem (string path)
		{
			 HttpResponse.RemoveOutputCacheItem (path);
		}

		public override void SetCookie (HttpCookie cookie)
		{
			w.SetCookie (cookie);
		}

		public override void TransmitFile (string filename)
		{
			w.TransmitFile (filename);
		}

		public override void TransmitFile (string filename, long offset, long length)
		{
			w.TransmitFile (filename, offset, length);
		}

		public override void Write (char ch)
		{
			w.Write (ch);
		}

		public override void Write (object obj)
		{
			w.Write (obj);
		}

		public override void Write (string s)
		{
			w.Write (s);
		}

		public override void Write (char [] buffer, int index, int count)
		{
			w.Write (buffer, index, count);
		}

		public override void WriteFile (string filename)
		{
			w.WriteFile (filename);
		}

		public override void WriteFile (string filename, bool readIntoMemory)
		{
			w.WriteFile (filename, readIntoMemory);
		}

		public override void WriteFile (IntPtr fileHandle, long offset, long size)
		{
			w.WriteFile (fileHandle, offset, size);
		}

		public override void WriteFile (string filename, long offset, long size)
		{
			w.WriteFile (filename, offset, size);
		}

		public override void WriteSubstitution (HttpResponseSubstitutionCallback callback)
		{
			w.WriteSubstitution (callback);
		}
	}
}
