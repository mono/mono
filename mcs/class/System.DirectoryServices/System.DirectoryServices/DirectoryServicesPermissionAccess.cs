//
// System.DirectoryServices.DirectoryServicesPermissionAccess.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2004 Andreas Nahr
//
//

namespace System.DirectoryServices {

	[Serializable]
	[Flags]
	public enum DirectoryServicesPermissionAccess
	{
		None = 0,
		Browse = 2,
		Write = 6
	}
}

