// CS0647: Error during emitting `System.Security.Permissions.FileIOPermissionAttribute' attribute. The reason is `Absolute path information is required
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
