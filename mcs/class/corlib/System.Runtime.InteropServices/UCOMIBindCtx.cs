//
// System.Runtime.InteropServices.UCOMIBindCtx.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIBindCtx
	{
		void EnumObjectParam (ref UCOMIEnumString ppenum);
		void GetBindOptions (ref BIND_OPTS pbindopts);
		void GetObjectParam (string pszKey, ref object ppunk);
		void GetRunningObjectTable (ref UCOMIRunningObjectTable pprot);
		void RegisterObjectBound (object punk);
		void RegisterObjectParam (string pszKey, object punk);
		void ReleaseBoundObjects ();
		void RevokeObjectBound (object punk);
		void RevokeObjectParam (string pszKey);
		void SetBindOptions (ref BIND_OPTS pbindopts);
	}
}
