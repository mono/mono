// CS0647: Error during emitting `DebugPermissionAttribute' attribute. The reason is `it is attached to invalid parent'
// Line : 11

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
