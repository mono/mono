// cs0647.cs : Error emitting 'System.Security.Permissions.SecurityPermissionAttribute' attribute -- 'SecurityAction RequestMinimum is not valid on this declaration
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[SecurityPermission (SecurityAction.RequestMinimum, ControlPrincipal=true, Flags=SecurityPermissionFlag.ControlPrincipal)]
	static public void Main (string[] args)
	{
	}
}
