//
// System.Security.Permissions.PermissionSetAttribute.cs
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
	   public sealed class PermissionSetAttribute : CodeAccessSecurityAttribute
	   {
			 // Constructor
			 public PermissionSetAttribute (SecurityAction action) : base (action) {}

			 // Properties
			 [MonoTODO]
			 public string File
			 {
				    get { return null; }
				    set {}
			 }

			 [MonoTODO]
			 public string Name
			 {
				    get { return null; }
				    set {}
			 }

			 [MonoTODO]
			 public bool UnicodeEncoded
			 {
				    get { return false; }
				    set {}
			 }

			 [MonoTODO]
			 public string XML
			 {
				    get { return null; }
				    set {}
			 }

			 // Methods
			 [MonoTODO]
			 public override IPermission CreatePermission ()
			 {
				    return null;
			 }

			 [MonoTODO]
			 public PermissionSet CreatePermissionSet ()
			 {
				    return null;
			 }
	   }
}		    
