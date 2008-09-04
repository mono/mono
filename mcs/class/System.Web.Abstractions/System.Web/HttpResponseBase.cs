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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Web.Caching;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpResponseBase
	{
		[MonoTODO]
		public virtual bool Buffer { get; set; }
		[MonoTODO]
		public virtual bool BufferOutput { get; set; }
		[MonoTODO]
		public virtual HttpCachePolicyBase Cache { get; private set; }
		[MonoTODO]
		public virtual string CacheControl { get; set; }
		[MonoTODO]
		public virtual string Charset { get; set; }
		[MonoTODO]
		public virtual Encoding ContentEncoding { get; set; }
		[MonoTODO]
		public virtual string ContentType { get; set; }
		[MonoTODO]
		public virtual HttpCookieCollection Cookies { get; private set; }
		[MonoTODO]
		public virtual int Expires { get; set; }
		[MonoTODO]
		public virtual DateTime ExpiresAbsolute { get; set; }
		[MonoTODO]
		public virtual Stream Filter { get; set; }
		[MonoTODO]
		public virtual Encoding HeaderEncoding { get; set; }
		[MonoTODO]
		public virtual NameValueCollection Headers { get; private set; }
		[MonoTODO]
		public virtual bool IsClientConnected { get; private set; }
		[MonoTODO]
		public virtual bool IsRequestBeingRedirected { get; private set; }
		[MonoTODO]
		public virtual TextWriter Output { get; private set; }
		[MonoTODO]
		public virtual Stream OutputStream { get; private set; }
		[MonoTODO]
		public virtual string RedirectLocation { get; set; }
		[MonoTODO]
		public virtual string Status { get; set; }
		[MonoTODO]
		public virtual int StatusCode { get; set; }
		[MonoTODO]
		public virtual string StatusDescription { get; set; }
		[MonoTODO]
		public virtual int SubStatusCode { get; set; }
		[MonoTODO]
		public virtual bool SuppressContent { get; set; }
		[MonoTODO]
		public virtual bool TrySkipIisCustomErrors { get; set; }

		[MonoTODO]
		public virtual void AddCacheDependency (params CacheDependency [] dependencies)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddCacheItemDependencies (ArrayList cacheKeys)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddCacheItemDependencies (string [] cacheKeys)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddCacheItemDependency (string cacheKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddFileDependencies (ArrayList filenames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddFileDependencies (string [] filenames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddFileDependency (string filename)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddHeader (string name, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AppendCookie (HttpCookie cookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AppendHeader (string name, string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AppendToLog (string param)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string ApplyAppPathModifier (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BinaryWrite (byte [] buffer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ClearContent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ClearHeaders ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DisableKernelCache ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void End ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Flush ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Pics (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Redirect (string url)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Redirect (string url, bool endResponse)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveOutputCacheItem (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SetCookie (HttpCookie cookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void TransmitFile (string filename)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void TransmitFile (string filename, long offset, long length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Write (char ch)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Write (object obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Write (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Write (char [] buffer, int index, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteFile (string filename)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteFile (string filename, bool readIntoMemory)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteFile (IntPtr fileHandle, long offset, long size)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteFile (string filename, long offset, long size)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteSubstitution (HttpResponseSubstitutionCallback callback)
		{
			throw new NotImplementedException ();
		}
	}
}
