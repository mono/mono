//
// System.Runtime.InteropServices.UCOMIPersistFile.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
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
