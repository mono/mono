//
// System.Security.Permissions.UrlIdentityPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class UrlIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private string url;

		public UrlIdentityPermission (PermissionState state) : base ()
		{
			if (state != PermissionState.None)
				throw new ArgumentException ("only accept None");
		}

		public UrlIdentityPermission (string site) : base ()
		{
			if (site == null)
				throw new ArgumentNullException ("site");
			url = site;
		}

		public string Url { 
			get { return url; }
			set { url = value; }
		}

		public override IPermission Copy () 
		{
			return new UrlIdentityPermission (url);
		}

		[MonoTODO]
		public override void FromXml (SecurityElement esd)
		{
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			// if one permission is null (object or url) then there's no intersection
			// if both are null then intersection is null
			if ((target == null) || (url == null))
				return null;

			// if non null, target must be of the same type
			if (!(target is UrlIdentityPermission))
				throw new ArgumentNullException ("target");

			UrlIdentityPermission targetUrl = (target as UrlIdentityPermission);
			if (targetUrl.Url == null)
				return null;

			// TODO
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

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 12;
		}
	}
}
