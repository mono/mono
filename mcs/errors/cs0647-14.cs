// cs0647.cs: Error emitting 'System.Security.Permissions.FileIOPermissionAttribute' because 'System.ArgumentException was thrown during attribute processing: Absolute path information is required.'
// Line: 9

using System;
using System.Security.Permissions;

public class Program {

	[FileIOPermission (SecurityAction.Demand, PathDiscovery="..%%\\")]
	static public int Main (string[] args)
	{
		return 0;
	}
}
