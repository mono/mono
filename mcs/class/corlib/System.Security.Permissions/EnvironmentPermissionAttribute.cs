//
// System.Security.Permissions.EnvironmentPermissionAttribute.cs
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
	public sealed class EnvironmentPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string read;
		private string write;
		
		// Constructor
		public EnvironmentPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string All {
#if ! NET_1_0
			get { throw new NotSupportedException ("All"); }
#endif
			set { 
				read = value; 
				write = value;
			}
		}

		public string Read {
			get { return read; }
			set { read = value; }
		}

		public string Write {
			get { return write; }
			set { write = value; }
		}

		// Methods
		public override IPermission CreatePermission ()
		{
			EnvironmentPermission perm = null;
			if (this.Unrestricted)
				perm = new EnvironmentPermission (PermissionState.Unrestricted);
			else {
				perm = new EnvironmentPermission (PermissionState.None);
				if (read != null)
					perm.AddPathList (EnvironmentPermissionAccess.Read, read);
				if (write != null)
					perm.AddPathList (EnvironmentPermissionAccess.Write, write);
			}
			return perm;
		}
	}
}
