//
// System.Security.Permissions.RegistryPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class RegistryPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Fields
		private string all;
		private string create;
		private string read;
		private string write;
		       
		// Constructor
		public RegistryPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string All
		{
			set { all = value; }
			get { return all; }
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
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}
	}
}
