//
// System.Web.Hosting.DefaultVirtualPathProvider
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
	sealed class DefaultVirtualPathProvider : VirtualPathProvider {

		internal DefaultVirtualPathProvider ()
		{
		}

		protected override void Initialize ()
		{
		}

		public override string CombineVirtualPaths (string basePath, string relativePath)
		{
			return VirtualPathUtility.Combine (basePath, relativePath);
		}

		public override bool DirectoryExists (string virtualDir)
		{
			if (virtualDir == null || virtualDir == "")
				throw new ArgumentNullException ("virtualDir");

			if (UrlUtils.IsRelativeUrl (virtualDir)) {
				string msg = String.Format ("The relative virtual path '{0}', is not allowed here.", virtualDir);
				throw new ArgumentException (msg);
			}

			string phys_path = HostingEnvironment.MapPath (virtualDir);
			return Directory.Exists (phys_path);
		}

		public override bool FileExists (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "")
				throw new ArgumentNullException ("virtualPath");

			if (UrlUtils.IsRelativeUrl (virtualPath)) {
				string msg = String.Format ("The relative virtual path '{0}', is not allowed here.", virtualPath);
				throw new ArgumentException (msg);
			}

			string phys_path = HostingEnvironment.MapPath (virtualPath);
			return File.Exists (phys_path);
		}

		public override CacheDependency GetCacheDependency (string virtualPath,
								IEnumerable virtualPathDependencies,
								DateTime utcStart)
		{
			return null;
		}

		public override string GetCacheKey (string virtualPath)
		{
			return null;
		}

		public override VirtualDirectory GetDirectory (string virtualDir)
		{
			if (virtualDir == null || virtualDir == "")
				throw new ArgumentNullException ("virtualDir");

			if (UrlUtils.IsRelativeUrl (virtualDir)) {
				string msg = String.Format ("The relative virtual path '{0}', is not allowed here.", virtualDir);
				throw new ArgumentException (msg);
			}

			return new DefaultVirtualDirectory (virtualDir);
		}

		public override VirtualFile GetFile (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "")
				throw new ArgumentNullException ("virtualPath");

			if (UrlUtils.IsRelativeUrl (virtualPath)) {
				string msg = String.Format ("The relative virtual path '{0}', is not allowed here.", virtualPath);
				throw new ArgumentException (msg);
			}

			return new DefaultVirtualFile (virtualPath);
		}

		public override string GetFileHash (string virtualPath, IEnumerable virtualPathDependencies)
		{
			return null;
		}
	}
}
#endif

