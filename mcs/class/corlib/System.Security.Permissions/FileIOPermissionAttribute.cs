//
// System.Security.Permissions.FileIOPermissionAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Permissions {

	[ComVisible (true)]
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class FileIOPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string append;
		private string path;
		private string read;
		private string write;
		private FileIOPermissionAccess allFiles;
		private FileIOPermissionAccess allLocalFiles;
		private string changeAccessControl;
		private string viewAccessControl;
		//private string viewAndModify;

		// Constructor
		public FileIOPermissionAttribute (SecurityAction action) : base (action)
		{
		}
		
		// Properties
		[Obsolete ("use newer properties")]
		public string All {
			get { throw new NotSupportedException ("All"); }
			set {
				append = value; 
				path = value;
				read = value;
				write = value;
			}
		}

		public string Append {
			get { return append; }
			set { append = value; }
		}

		public string PathDiscovery {
			get { return path; }
			set { path = value; }
		}

		public string Read {
			get { return read; }
			set { read = value; }
		}				    
		
		public string Write {
			get { return write; }
			set { write = value; }
		}

		public FileIOPermissionAccess AllFiles {
			get { return allFiles; }
			set { allFiles = value; }
		}

		public FileIOPermissionAccess AllLocalFiles {
			get { return allLocalFiles; }
			set { allLocalFiles = value; }
		}

		public string ChangeAccessControl {
			get { return changeAccessControl; }
			set { changeAccessControl = value; }
		}

		public string ViewAccessControl {
			get { return viewAccessControl; }
			set { viewAccessControl = value; }
		}

		public string ViewAndModify {
			get { throw new NotSupportedException (); }	// as documented
			set {
				append = value;
				path = value;
				read = value;
				write = value;
			}
		}
		// Methods
		public override IPermission CreatePermission ()
		{
#if MOBILE
			return null;
#else
			FileIOPermission perm = null;
			if (this.Unrestricted)
				perm = new FileIOPermission (PermissionState.Unrestricted);
			else {
				perm = new FileIOPermission (PermissionState.None);
				if (append != null)
					perm.AddPathList (FileIOPermissionAccess.Append, append);
				if (path != null)
					perm.AddPathList (FileIOPermissionAccess.PathDiscovery, path);
				if (read != null)
					perm.AddPathList (FileIOPermissionAccess.Read, read);
				if (write != null)
					perm.AddPathList (FileIOPermissionAccess.Write, write);
			}
			return perm;
#endif
		}
	}
}	   
