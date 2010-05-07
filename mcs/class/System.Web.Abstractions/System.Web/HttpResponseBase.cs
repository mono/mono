//
// HttpResponseBase.cs
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
using System.Web.Routing;
#endif

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpResponseBase
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}


		public virtual bool Buffer { get { NotImplemented (); return false; } set { NotImplemented (); } }

		public virtual bool BufferOutput { get { NotImplemented (); return false; } set { NotImplemented (); } }

		public virtual HttpCachePolicyBase Cache { get { NotImplemented (); return null; } }

		public virtual string CacheControl { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual string Charset { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual Encoding ContentEncoding { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual string ContentType { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual HttpCookieCollection Cookies { get { NotImplemented (); return null; } }

		public virtual int Expires { get { NotImplemented (); return 0; } set { NotImplemented (); } }

		public virtual DateTime ExpiresAbsolute { get { NotImplemented (); return DateTime.MinValue; } set { NotImplemented (); } }

		public virtual Stream Filter { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual Encoding HeaderEncoding { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual NameValueCollection Headers { get { NotImplemented (); return null; } }

		public virtual bool IsClientConnected { get { NotImplemented (); return false; } }

		public virtual bool IsRequestBeingRedirected { get { NotImplemented (); return false; } }

		public virtual TextWriter Output { get { NotImplemented (); return null; }  set { NotImplemented (); } }

		public virtual Stream OutputStream { get { NotImplemented (); return null; } }

		public virtual string RedirectLocation { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual string Status { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual int StatusCode { get { NotImplemented (); return 0; } set { NotImplemented (); } }

		public virtual string StatusDescription { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual int SubStatusCode { get { NotImplemented (); return 0; } set { NotImplemented (); } }

		public virtual bool SuppressContent { get { NotImplemented (); return false; } set { NotImplemented (); } }

		public virtual bool TrySkipIisCustomErrors { get { NotImplemented (); return false; } set { NotImplemented (); } }


		public virtual void AddCacheDependency (params CacheDependency [] dependencies)
		{
			NotImplemented ();
		}


		public virtual void AddCacheItemDependencies (ArrayList cacheKeys)
		{
			NotImplemented ();
		}


		public virtual void AddCacheItemDependencies (string [] cacheKeys)
		{
			NotImplemented ();
		}


		public virtual void AddCacheItemDependency (string cacheKey)
		{
			NotImplemented ();
		}


		public virtual void AddFileDependencies (ArrayList filenames)
		{
			NotImplemented ();
		}


		public virtual void AddFileDependencies (string [] filenames)
		{
			NotImplemented ();
		}


		public virtual void AddFileDependency (string filename)
		{
			NotImplemented ();
		}


		public virtual void AddHeader (string name, string value)
		{
			NotImplemented ();
		}


		public virtual void AppendCookie (HttpCookie cookie)
		{
			NotImplemented ();
		}


		public virtual void AppendHeader (string name, string value)
		{
			NotImplemented ();
		}


		public virtual void AppendToLog (string param)
		{
			NotImplemented ();
		}


		public virtual string ApplyAppPathModifier (string virtualPath)
		{
			NotImplemented ();
			return null;
		}


		public virtual void BinaryWrite (byte [] buffer)
		{
			NotImplemented ();
		}


		public virtual void Clear ()
		{
			NotImplemented ();
		}


		public virtual void ClearContent ()
		{
			NotImplemented ();
		}


		public virtual void ClearHeaders ()
		{
			NotImplemented ();
		}


		public virtual void Close ()
		{
			NotImplemented ();
		}


		public virtual void DisableKernelCache ()
		{
			NotImplemented ();
		}


		public virtual void End ()
		{
			NotImplemented ();
		}


		public virtual void Flush ()
		{
			NotImplemented ();
		}


		public virtual void Pics (string value)
		{
			NotImplemented ();
		}


		public virtual void Redirect (string url)
		{
			NotImplemented ();
		}


		public virtual void Redirect (string url, bool endResponse)
		{
			NotImplemented ();
		}
#if NET_4_0
		public virtual void RedirectPermanent (string url)
		{
			NotImplemented ();
		}

		public virtual void RedirectPermanent (string url, bool endResponse)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoute (object routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoute (RouteValueDictionary routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoute (string routeName)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoute (string routeName, object routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoute (string routeName, RouteValueDictionary routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoutePermanent (object routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoutePermanent (RouteValueDictionary routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoutePermanent (string routeName)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoutePermanent (string routeName, object routeValues)
		{
			NotImplemented ();
		}

		public virtual void RedirectToRoutePermanent (string routeName, RouteValueDictionary routeValues)
		{
			NotImplemented ();
		}

		public virtual void RemoveOutputCacheItem (string path, string providerName)
		{
			NotImplemented ();
		}
#endif
		public virtual void RemoveOutputCacheItem (string path)
		{
			NotImplemented ();
		}


		public virtual void SetCookie (HttpCookie cookie)
		{
			NotImplemented ();
		}


		public virtual void TransmitFile (string filename)
		{
			NotImplemented ();
		}


		public virtual void TransmitFile (string filename, long offset, long length)
		{
			NotImplemented ();
		}


		public virtual void Write (char ch)
		{
			NotImplemented ();
		}


		public virtual void Write (object obj)
		{
			NotImplemented ();
		}


		public virtual void Write (string s)
		{
			NotImplemented ();
		}


		public virtual void Write (char [] buffer, int index, int count)
		{
			NotImplemented ();
		}


		public virtual void WriteFile (string filename)
		{
			NotImplemented ();
		}


		public virtual void WriteFile (string filename, bool readIntoMemory)
		{
			NotImplemented ();
		}


		public virtual void WriteFile (IntPtr fileHandle, long offset, long size)
		{
			NotImplemented ();
		}


		public virtual void WriteFile (string filename, long offset, long size)
		{
			NotImplemented ();
		}


		public virtual void WriteSubstitution (HttpResponseSubstitutionCallback callback)
		{
			NotImplemented ();
		}
	}
}
