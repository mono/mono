//
// System.Security.Permissions.IsolatedStorageFilePermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Permissions {

	[Serializable]
	public sealed class IsolatedStorageFilePermission : IsolatedStoragePermission, IBuiltInPermission {

		// Constructors

		public IsolatedStorageFilePermission (PermissionState state) : base (state) {}

		// Properties

		// Methods

		[MonoTODO]
		public override IPermission Copy () 
		{
			IsolatedStorageFilePermission p = new IsolatedStorageFilePermission (PermissionState.None);
			// TODO add stuff into p
			return p;
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			return null;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 3;
		}
	}
}