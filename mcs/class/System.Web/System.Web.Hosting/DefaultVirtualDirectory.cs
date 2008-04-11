//
// System.Web.Hosting.DefaultVirtualDirectory
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

//
// Copyright (c) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Hosting {

	class DefaultVirtualDirectory : VirtualDirectory
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
				virtual_dir = VirtualPathUtility.GetDirectory (vpath);
				phys_dir = HostingEnvironment.MapPath (virtual_dir);
			}
		}

		ArrayList AddDirectories (ArrayList list, string dir)
		{
			foreach (string name in Directory.GetDirectories (phys_dir)) {
				string vdir = VirtualPathUtility.Combine (virtual_dir, Path.GetFileName (name));
				list.Add (new DefaultVirtualDirectory (vdir));
			}
			return list;
		}

		ArrayList AddFiles (ArrayList list, string dir)
		{
			foreach (string name in Directory.GetFiles (phys_dir)) {
				string vdir = VirtualPathUtility.Combine (virtual_dir, Path.GetFileName (name));
				list.Add (new DefaultVirtualFile (vdir));
			}
			return list;
		}

		public override IEnumerable Children {
			get {
				Init ();
				ArrayList list = new ArrayList ();
				AddDirectories (list, phys_dir);
				return AddFiles (list, phys_dir);
			}
		}

		public override IEnumerable Directories {
			get {
				Init ();
				ArrayList list = new ArrayList ();
				return AddDirectories (list, phys_dir);
			}
		}
		
		public override IEnumerable Files {
			get {
				Init ();
				ArrayList list = new ArrayList ();
				return AddFiles (list, phys_dir);
			}
		}
	}
}
#endif

