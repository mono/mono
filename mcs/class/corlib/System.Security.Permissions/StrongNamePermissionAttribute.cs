//
// System.Security.Permissions.StrongNameIdentityPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class StrongNameIdentityPermissionAttribute : CodeAccessSecurityAttribute	{

		// Fields
		private string name;
		private string key;
		private string version;
		
		// Constructor
		public StrongNameIdentityPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string PublicKey
		{
			get { return key; }
			set { key = value; }
		}

		public string Version
		{
			get { return version; }
			set { version = value; }
		}
			 
		// Methods
		public override IPermission CreatePermission ()
		{
			if (this.Unrestricted)
				throw new ArgumentException ("Unsupported PermissionState.Unrestricted");

			StrongNameIdentityPermission perm = null;
			if ((name == null) && (key == null) && (version == null))
				perm = new StrongNameIdentityPermission (PermissionState.None);
			else {
				if (key == null)
					throw new ArgumentException ("PublicKey is required");

				byte[] keyblob = Convert.FromBase64String (key);
				StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (keyblob);
				
				Version v = null;
				if (version != null)
					v = new Version (version);
				else
					v = new Version ();

				if (name == null)
					name = String.Empty;

				perm = new StrongNameIdentityPermission (blob, name, v);
			}
			return perm;
		}
	}
}
