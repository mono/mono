//
// System.Runtime.InteropServices.UCOMIBindCtx.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("0000000e-0000-0000-c000-000000000046")]
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
		void SetBindOptions ([In] ref BIND_OPTS pbindopts);
	}
}
