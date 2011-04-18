// CS0647: Error during emitting `DebugPermissionAttribute' attribute. The reason is `it is attached to invalid parent'
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[DebugPermission (SecurityAction.RequestMinimum)]
        public int i;
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
