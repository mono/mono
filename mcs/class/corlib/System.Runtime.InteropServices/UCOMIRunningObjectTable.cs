//
// System.Runtime.InteropServices.UCOMIRunningObjectTable.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("00000010-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIRunningObjectTable
	{
		void EnumRunning (out UCOMIEnumMoniker ppenumMoniker);
		void GetObject (UCOMIMoniker pmkObjectName, out object ppunkObject);
		void GetTimeOfLastChange (UCOMIMoniker pmkObjectName, out FILETIME pfiletime);
		void IsRunning (UCOMIMoniker pmkObjectName);
		void NoteChangeTime (int dwRegister, ref FILETIME pfiletime);
		void Register (int grfFlags, object punkObject, UCOMIMoniker pmkObjectName, out int pdwRegister);
		void Revoke (int dwRegister);
	}
}
