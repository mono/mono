// ZipFileInfo.cs created with MonoDevelop
// User: alan at 12:14 13/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	[StructLayoutAttribute (LayoutKind.Sequential)]
	struct ZipFileInfo32
	{
		ZipTime date;
		uint dosDate;
		uint internalFileAttributes;
		uint externalFileAttributes;

		public ZipFileInfo32 (DateTime fileTime)
		{
			date = new ZipTime (fileTime);
			dosDate = 0;
			internalFileAttributes = 0;
			externalFileAttributes = 0;
		}
	}

	[StructLayoutAttribute (LayoutKind.Sequential)]
	struct ZipFileInfo64
	{
		ZipTime date;
		ulong dosDate;
		ulong internalFileAttributes;
		ulong externalFileAttributes;

		public ZipFileInfo64 (DateTime fileTime)
		{
			date = new ZipTime (fileTime);
			dosDate = 0;
			internalFileAttributes = 0;
			externalFileAttributes = 0;
		}
	}
}
