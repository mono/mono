// cs0647.cs :  Error emitting 'PrincipalPermission' attribute because 'Invalid SecurityAction for non-Code Access Security permission'
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
