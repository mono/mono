//
// System.Security.Permissions.RegistryPermissionAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot <spouliot@motus.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class RegistryPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string create;
		private string read;
		private string write;
		       
		// Constructor
		public RegistryPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string All
		{
#if ! NET_1_0
			get { throw new NotSupportedException ("All"); }
#endif
			set { 
				create = value; 
				read = value;
				write = value;
			}
		}
		
		public string Create
		{
			get { return create; }
			set { create = value; }
		}

		public string Read
		{ 
			get { return read; }
			set { read = value; }
		}

		public string Write
		{
			get { return write; }
			set { write = value; }
		}

		// Methods
		public override IPermission CreatePermission ()
		{
			RegistryPermission perm = null;
			if (this.Unrestricted)
				perm = new RegistryPermission (PermissionState.Unrestricted);
			else {
				perm = new RegistryPermission (PermissionState.None);
				if (create != null)
					perm.AddPathList (RegistryPermissionAccess.Create, create);
				if (read != null)
					perm.AddPathList (RegistryPermissionAccess.Read, read);
				if (write != null)
					perm.AddPathList (RegistryPermissionAccess.Write, write);
			}
			return perm;
		}
	}
}
