// CS7070: Security attribute `DebugPermissionAttribute' is not valid on this declaration type. Security attributes are only valid on assembly, type and method declarations
// Line: 11

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

        [return: DebugPermission (SecurityAction.LinkDemand)]
        public int Show (string message)
        {
                return 2;
        }    
}

[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[Serializable]
public class DebugPermissionAttribute : CodeAccessSecurityAttribute {

	public DebugPermissionAttribute (SecurityAction action)
		: base (action)
	{
	}
        
	public override IPermission CreatePermission ()
	{
		return null;
	}
}
