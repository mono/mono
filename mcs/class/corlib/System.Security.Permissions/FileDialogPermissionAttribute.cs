//
// System.Security.Permissions.FileDialogPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	   [AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
					AttributeTargets.Struct | AttributeTargets.Constructor |
					AttributeTargets.Method)]
        [Serializable]
	   public sealed class FileDialogPermissionAttribute : CodeAccessSecurityAttribute
	   {
			 // Constructor
			 public FileDialogPermissionAttribute (SecurityAction action) : base (action) {}

			 // Properties
			 [MonoTODO]
			 public bool Open
			 {
				    get { return false; }
				    set {}
			 } 

			 [MonoTODO]
			 public bool Save
			 {
				    get { return false; }
				    set {}
			 }

			 // Methods
			 [MonoTODO]
			 public override IPermission CreatePermission ()
			 {
				    return null;
			 }
	   }
}
