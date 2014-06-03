// CS7051: SecurityAction value `System.Security.Permissions.SecurityAction' is invalid for security attributes applied to a type or a method
// Line: 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[SecurityPermission (SecurityAction.RequestMinimum, ControlPrincipal=true, Flags=SecurityPermissionFlag.ControlPrincipal)]
	static public void Main (string[] args)
	{
	}
}
