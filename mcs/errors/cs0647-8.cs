// cs0647.cs : Security custom attribute 'DebugPermission' attached to invalid parent
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[PrincipalPermission (SecurityAction.Assert, Name="Poupou")]
	public virtual void Show (string message)
	{
		Console.WriteLine (message);
	}
}
