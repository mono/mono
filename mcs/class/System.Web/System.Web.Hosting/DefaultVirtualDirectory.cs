//
// System.Web.Hosting.DefaultVirtualDirectory
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

//
// Copyright (c) 2006-2011 Novell, Inc (http://www.novell.com)
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
using System.IO;

namespace System.Web.Hosting {

	sealed class DefaultVirtualDirectory : VirtualDirectory
	{
		string phys_dir;
		string virtual_dir;

		internal DefaultVirtualDirectory (string virtualPath)
			: base (virtualPath)
		{
		}

		void Init ()
		{
			if (phys_dir == null) {
				string vpath = VirtualPath;
				string path = HostingEnvironment.MapPath (vpath);
				if (File.Exists (path)) {
					virtual_dir = VirtualPathUtility.GetDirectory (vpath);
					phys_dir = HostingEnvironment.MapPath (virtual_dir);
				} else {
					virtual_dir = VirtualPathUtility.AppendTrailingSlash (vpath);
					phys_dir = path;
				}
			}
		}

		List <VirtualFileBase> AddDirectories (List <VirtualFileBase> list, string dir)
		{
			if (String.IsNullOrEmpty (dir) || !Directory.Exists (dir))
				return list;
			
			foreach (string name in Directory.GetDirectories (phys_dir))
				list.Add (new DefaultVirtualDirectory (VirtualPathUtility.Combine (virtual_dir, Path.GetFileName (name))));

			return list;
		}

		List <VirtualFileBase> AddFiles (List <VirtualFileBase> list, string dir)
		{
			if (String.IsNullOrEmpty (dir) || !Directory.Exists (dir))
				return list;
			
			foreach (string name in Directory.GetFiles (phys_dir))
				list.Add (new DefaultVirtualFile (VirtualPathUtility.Combine (virtual_dir, Path.GetFileName (name))));

			return list;
		}

		public override IEnumerable Children {
			get {
				Init ();
				var list = new List <VirtualFileBase> ();
				AddDirectories (list, phys_dir);
				return AddFiles (list, phys_dir);
			}
		}

		public override IEnumerable Directories {
			get {
				Init ();
				return AddDirectories (new List <VirtualFileBase> (), phys_dir);
			}
		}
		
		public override IEnumerable Files {
			get {
				Init ();
				return AddFiles (new List <VirtualFileBase> (), phys_dir);
			}
		}
	}
}


