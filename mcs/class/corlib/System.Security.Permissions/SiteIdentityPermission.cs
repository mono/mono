//
// System.Security.Permissions.SiteIdentityPermission.cs
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
	public sealed class SiteIdentityPermission : CodeAccessPermission, IBuiltInPermission {

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

			this.Site = esd.Attribute ("Site");
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

		public override SecurityElement ToXml ()
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			e.AddAttribute ("Site", _site);

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
			return 10;
		}
	}
}
