//
// HttpCachePolicyWrapper.cs
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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpCachePolicyWrapper : HttpCachePolicyBase
	{
		HttpCachePolicy w;

		public HttpCachePolicyWrapper (HttpCachePolicy httpCachePolicy)
		{
			if (httpCachePolicy == null)
				throw new ArgumentNullException ("httpCachePolicy");
			w = httpCachePolicy;
		}

		public override HttpCacheVaryByContentEncodings VaryByContentEncodings {
			get { return w.VaryByContentEncodings; }
		}

		public override HttpCacheVaryByHeaders VaryByHeaders {
			get { return w.VaryByHeaders; }
		}

		public override HttpCacheVaryByParams VaryByParams {
			get { return w.VaryByParams; }
		}

		public override void AddValidationCallback (HttpCacheValidateHandler handler, object data)
		{
			w.AddValidationCallback (handler, data);
		}

		public override void AppendCacheExtension (string extension)
		{
			w.AppendCacheExtension (extension);
		}

		public override void SetAllowResponseInBrowserHistory (bool allow)
		{
			w.SetAllowResponseInBrowserHistory (allow);
		}

		public override void SetCacheability (HttpCacheability cacheability)
		{
			w.SetCacheability (cacheability);
		}

		public override void SetCacheability (HttpCacheability cacheability, string field)
		{
			w.SetCacheability (cacheability, field);
		}

		public override void SetVaryByCustom (string custom)
		{
			w.SetVaryByCustom (custom);
		}

		public override void SetETag (string etag)
		{
			w.SetETag (etag);
		}

		public override void SetETagFromFileDependencies ()
		{
			w.SetETagFromFileDependencies ();
		}

		public override void SetExpires (DateTime date)
		{
			w.SetExpires (date);
		}

		public override void SetLastModified (DateTime date)
		{
			w.SetLastModified (date);
		}

		public override void SetLastModifiedFromFileDependencies ()
		{
			w.SetLastModifiedFromFileDependencies ();
		}

		public override void SetMaxAge (TimeSpan delta)
		{
			w.SetMaxAge (delta);
		}

		public override void SetNoServerCaching ()
		{
			w.SetNoServerCaching ();
		}

		public override void SetNoStore ()
		{
			w.SetNoStore ();
		}

		public override void SetNoTransforms ()
		{
			w.SetNoTransforms ();
		}

		public override void SetOmitVaryStar (bool omit)
		{
			w.SetOmitVaryStar (omit);
		}

		public override void SetProxyMaxAge (TimeSpan delta)
		{
			w.SetProxyMaxAge (delta);
		}

		public override void SetRevalidation (HttpCacheRevalidation revalidation)
		{
			w.SetRevalidation (revalidation);
		}

		public override void SetSlidingExpiration (bool slide)
		{
			w.SetSlidingExpiration (slide);
		}

		public override void SetValidUntilExpires (bool validUntilExpires)
		{
			w.SetValidUntilExpires (validUntilExpires);
		}
	}
}
