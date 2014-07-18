// CS7070: Security attribute `DebugPermissionAttribute' is not valid on this declaration type. Security attributes are only valid on assembly, type and method declarations
// Line: 11

using System;
using System.Security;
using System.Security.Permissions;

public class Program {
        public delegate int DisplayHandler (string msg);
     
	[DebugPermission (SecurityAction.RequestMinimum)]
        public event DisplayHandler OnShow;
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
