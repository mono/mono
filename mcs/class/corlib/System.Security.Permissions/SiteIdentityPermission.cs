//
// System.Security.Permissions.SiteIdentityPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class SiteIdentityPermission : CodeAccessPermission {

		private string _site;

		// Constructors

		public SiteIdentityPermission (PermissionState state) 
		{
		}

		public SiteIdentityPermission (string site) 
		{
			_site = site;
		}

		// Properties

		public string Site {
			get { return _site; }
			set { _site = value; }
		}

		// Methods

		public override IPermission Copy () 
		{
			return new SiteIdentityPermission (_site);
		}

		[MonoTODO]
		public override void FromXml (SecurityElement esd) 
		{
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
		public override SecurityElement ToXml ()
		{
			return null;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			return null;
		}
	}
}