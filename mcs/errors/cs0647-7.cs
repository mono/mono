// CS0647: Error during emitting `DebugPermissionAttribute' attribute. The reason is `it is attached to invalid parent'
// Line : 11

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

        [DebugPermission (SecurityAction.LinkDemand)]
        public int Show
        {
            get {
                return 2;
            }
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
