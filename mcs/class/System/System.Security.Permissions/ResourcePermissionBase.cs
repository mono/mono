//
// System.Security.Permissions.ResourcePermissionBase.cs
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
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
using System.Security.Permissions;

namespace System.Security.Permissions {

	[Serializable]
	public abstract class ResourcePermissionBase 
		: CodeAccessPermission, IUnrestrictedPermission {

		[MonoTODO]
		protected ResourcePermissionBase ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ResourcePermissionBase (PermissionState state)
		{
			throw new NotImplementedException ();
		}

		public const string Any = "*";
		public const string Local = ".";

		[MonoTODO]
		protected Type PermissionAccessType {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		[MonoTODO]
		protected string[] TagNames {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		[MonoTODO]
		protected void AddPermissionAccess (
			ResourcePermissionBaseEntry entry)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Copy ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void FromXml (SecurityElement securityElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ResourcePermissionBaseEntry[] GetPermissionEntries ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsUnrestricted ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void RemovePermissionAccess (
			ResourcePermissionBaseEntry entry)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}
	}
}

