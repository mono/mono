//
// HttpCachePolicyBase.cs
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
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpCachePolicyBase
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual HttpCacheVaryByContentEncodings VaryByContentEncodings { get { NotImplemented (); return null; } }

		public virtual HttpCacheVaryByHeaders VaryByHeaders { get { NotImplemented (); return null; } }

		public virtual HttpCacheVaryByParams VaryByParams { get { NotImplemented (); return null; } }

		public virtual void AddValidationCallback (HttpCacheValidateHandler handler, object data)
		{
			NotImplemented ();
		}

		public virtual void AppendCacheExtension (string extension)
		{
			NotImplemented ();
		}

		public virtual void SetAllowResponseInBrowserHistory (bool allow)
		{
			NotImplemented ();
		}

		public virtual void SetCacheability (HttpCacheability cacheability)
		{
			NotImplemented ();
		}

		public virtual void SetCacheability (HttpCacheability cacheability, string field)
		{
			NotImplemented ();
		}

		public virtual void SetETag (string etag)
		{
			NotImplemented ();
		}

		public virtual void SetETagFromFileDependencies ()
		{
			NotImplemented ();
		}

		public virtual void SetExpires (DateTime date)
		{
			NotImplemented ();
		}

		public virtual void SetLastModified (DateTime date)
		{
			NotImplemented ();
		}

		public virtual void SetLastModifiedFromFileDependencies ()
		{
			NotImplemented ();
		}

		public virtual void SetMaxAge (TimeSpan delta)
		{
			NotImplemented ();
		}

		public virtual void SetNoServerCaching ()
		{
			NotImplemented ();
		}

		public virtual void SetNoStore ()
		{
			NotImplemented ();
		}

		public virtual void SetNoTransforms ()
		{
			NotImplemented ();
		}

		public virtual void SetOmitVaryStar (bool omit)
		{
			NotImplemented ();
		}

		public virtual void SetProxyMaxAge (TimeSpan delta)
		{
			NotImplemented ();
		}

		public virtual void SetRevalidation (HttpCacheRevalidation revalidation)
		{
			NotImplemented ();
		}

		public virtual void SetSlidingExpiration (bool slide)
		{
			NotImplemented ();
		}

		public virtual void SetValidUntilExpires (bool validUntilExpires)
		{
			NotImplemented ();
		}

		public virtual void SetVaryByCustom (string custom)
		{
			NotImplemented ();
		}
	}
}
