//
// System.Web.AspNetHostingPermissionLevel.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

#if NET_1_1

namespace System.Web
{
	public enum AspNetHostingPermissionLevel
	{
		None = 100,
		Minimal = 200,
		Low = 300,
		Medium = 400,
		High = 500,
		Unrestricted = 600
	}
}

#endif