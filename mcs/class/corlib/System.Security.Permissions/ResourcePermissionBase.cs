//
// System.Security.Permissions.ResourcePermissionBase.cs
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[Serializable]
	public class ResourcePermissionBase 
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

		[MonoTODO]
		public const string Any = "any?";

		[MonoTODO]
		public const string Local = "local?";

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

