// CS0647: Error during emitting `System.Security.Permissions.PrincipalPermissionAttribute' attribute. The reason is `SecurityAction is out of range'
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[PrincipalPermission ((SecurityAction)100, Name="Poupou")]
	public virtual void Show (string message)
	{
	}
}
