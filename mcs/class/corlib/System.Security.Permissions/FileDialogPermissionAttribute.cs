//
// System.Security.Permissions.FileDialogPermissionAttribute.cs
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
	public sealed class FileDialogPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private bool canOpen;
		private bool canSave;
		
		// Constructor
		public FileDialogPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		// Properties
		public bool Open {
			get { return canOpen; }
			set { canOpen = value; }
		} 

		public bool Save {
			get { return canSave; }
			set { canSave = value; }
		}

		// Methods
		public override IPermission CreatePermission ()
		{
			FileDialogPermission perm = null;
			if (this.Unrestricted)
				perm = new FileDialogPermission (PermissionState.Unrestricted);
			else {
				FileDialogPermissionAccess access = FileDialogPermissionAccess.None;
				if (canOpen)
					access |= FileDialogPermissionAccess.Open;
				if (canSave)
					access |= FileDialogPermissionAccess.Save;
				perm = new FileDialogPermission (access);
			}
			return perm;
		}
	}
}
