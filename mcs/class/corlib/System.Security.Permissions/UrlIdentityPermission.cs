//
// System.Security.Permissions.UrlIdentityPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;
using System.Globalization;

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

		public override void FromXml (SecurityElement esd)
		{
			if (esd == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (esd.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (esd.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			url = esd.Attribute ("Url");
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

		public override SecurityElement ToXml () 
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			e.AddAttribute ("Url", url);

			return e;
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
