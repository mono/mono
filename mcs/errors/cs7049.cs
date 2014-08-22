// CS7049: Security attribute `System.Security.Permissions.PrincipalPermissionAttribute' has an invalid SecurityAction value `100'
// Line: 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[PrincipalPermission ((SecurityAction)100, Name="Poupou")]
	public virtual void Show (string message)
	{
	}
}
