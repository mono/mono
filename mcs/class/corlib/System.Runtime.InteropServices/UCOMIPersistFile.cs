//
// System.Runtime.InteropServices.UCOMIPersistFile.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("0000010b-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIPersistFile
	{
		void GetClassID (ref Guid pClassID);
		void GetCurFile (ref string ppszFileName);
		int IsDirty ();
		void Load (string pszFileName, int dwMode);
		void Save (string pszFileName, bool fRemember);
		void SaveCompleted (string pszFileName);
	}
}
