//
// System.Security.Permissions.FileIOPermissionAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot <spouliot@motus.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class FileIOPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string append;
		private string path;
		private string read;
		private string write;
		
		// Constructor
		public FileIOPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string All
		{
#if ! NET_1_0
			get { throw new NotSupportedException ("All"); }
#endif
			set {
				append = value; 
				path = value;
				read = value;
				write = value;
			}
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
		public override IPermission CreatePermission ()
		{
			FileIOPermission p = new FileIOPermission (PermissionState.None);
			if (append != null)
				p.AddPathList (FileIOPermissionAccess.Append, append);
			if (path != null)
				p.AddPathList (FileIOPermissionAccess.PathDiscovery, path);
			if (read != null)
				p.AddPathList (FileIOPermissionAccess.Read, read);
			if (write != null)
				p.AddPathList (FileIOPermissionAccess.Write, write);
			return p;
		}
	}
}	   
