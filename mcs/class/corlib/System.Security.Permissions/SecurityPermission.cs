//
// System.Security.Permissions.SecurityPermission.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System;
using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Permissions {
	
	[Serializable]
	public sealed class SecurityPermission :
		CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		[MonoTODO]
		public SecurityPermission (PermissionState state) 
		{
			this.flags = SecurityPermissionFlag.NoFlags;
		}

		public SecurityPermission (SecurityPermissionFlag flags) 
		{
			this.flags = flags;
		}

		public SecurityPermissionFlag Flags {
			get { return flags; }
			set { flags = value; }
		}

		[MonoTODO]
		public bool IsUnrestricted () 
		{
			return false;
		}

		public override IPermission Copy () 
		{
			return new SecurityPermission (flags);
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		public override void FromXml (SecurityElement e) 
		{
			if (e == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (e.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (e.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			flags = (SecurityPermissionFlag) Enum.Parse (
				typeof (SecurityPermissionFlag), e.Attribute ("Flags"));
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			e.AddAttribute ("Flags", flags.ToString ());

			return e;
		}

		// private 
		
		private SecurityPermissionFlag flags;

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 6;
		}
	}
}
