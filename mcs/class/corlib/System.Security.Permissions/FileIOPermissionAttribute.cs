//
// System.Security.Permissions.FileIOPermissionAttribute.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc.		http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class FileIOPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Fields
		private string all;
		private string append;
		private string path;
		private string read;
		private string write;
		
		// Constructor
		public FileIOPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string All
		{
			 set { all = value; }
		}

		public string Append
		{
			get { return append; }
			set { append = value; }
		}

		public string PathDiscovery
		{
			get { return path; }
			set { path = value; }
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
