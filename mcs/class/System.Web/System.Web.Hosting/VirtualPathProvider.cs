//
// System.Web.Hosting.VirtualPathProvider
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Collections;
using System.IO;
using System.Web.Caching;
using System.Web.Util;

namespace System.Web.Hosting {

	public abstract class VirtualPathProvider : MarshalByRefObject
	{
		VirtualPathProvider prev;

		protected VirtualPathProvider ()
		{
		}

		protected internal VirtualPathProvider Previous {
			get { return prev; }
		}

		protected virtual void Initialize ()
		{
		}

		internal void InitializeAndSetPrevious (VirtualPathProvider prev)
		{
			this.prev = prev;
			Initialize ();
		}
		
		public virtual string CombineVirtualPaths (string basePath, string relativePath)
		{
			return VirtualPathUtility.Combine (basePath, relativePath);
		}

		public virtual bool DirectoryExists (string virtualDir)
		{
			if (prev != null)
				return prev.DirectoryExists (virtualDir);
			
			return false;
		}

		public virtual bool FileExists (string virtualPath)
		{
			if (prev != null)
				return prev.FileExists (virtualPath);
			
			return false;
		}

		public virtual CacheDependency GetCacheDependency (string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (prev != null)
				return prev.GetCacheDependency (virtualPath, virtualPathDependencies, utcStart);
			
			return null;
		}

		public virtual string GetCacheKey (string virtualPath)
		{
			if (prev != null)
				return prev.GetCacheKey (virtualPath);
			
			return null;
		}

		public virtual VirtualDirectory GetDirectory (string virtualDir)
		{
			if (prev != null)
				return prev.GetDirectory (virtualDir);
			
			return null;
		}

		public virtual VirtualFile GetFile (string virtualPath)
		{
			if (prev != null)
				return prev.GetFile (virtualPath);
			
			return null;
		}

		public virtual string GetFileHash (string virtualPath, IEnumerable virtualPathDependencies)
		{
			if (prev != null)
				return prev.GetFileHash (virtualPath, virtualPathDependencies);
			
			return null;
		}

		public override object InitializeLifetimeService ()
		{
			return null; // forever young
		}

		public static Stream OpenFile (string virtualPath)
		{
			// This thing throws a nullref when we're not inside an ASP.NET appdomain, which is what MS does.
			VirtualPathProvider provider = HostingEnvironment.VirtualPathProvider;
			VirtualFile file = provider.GetFile (virtualPath);
			if (file != null)
				return file.Open ();

			return null;
		}
	}
}
#endif

