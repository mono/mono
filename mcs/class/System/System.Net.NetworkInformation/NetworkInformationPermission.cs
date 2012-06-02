//
// System.Net.NetworkInformation.NetworkInformationPermission
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;

namespace System.Net.NetworkInformation {
	[Serializable]
	public sealed class NetworkInformationPermission : CodeAccessPermission, IUnrestrictedPermission {
		private const int version = 1;

		[MonoTODO]
		public NetworkInformationPermission (PermissionState state)
		{
		}

		[MonoTODO]
		public NetworkInformationPermission (NetworkInformationAccess access)
		{
		}

		[MonoTODO]
		public void AddPermission (NetworkInformationAccess access)
		{
		}

		[MonoTODO]
		public override IPermission Copy ()
		{
			return null;
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
		{
		}

		[MonoTODO]
		public override IPermission Intersect (System.Security.IPermission target)
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			return false;
		}

		[MonoTODO]
		public bool IsUnrestricted ()
		{
			return false;
		}

		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			SecurityElement se = PermissionHelper.Element (typeof (NetworkInformationPermission), version);

			// FIXME: add fields

			return se;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			return null;
		}
		
		[MonoTODO]
		public NetworkInformationAccess Access {
			get { return 0; }
		}
	}
}

