//
// Microsoft.Win32/IRegistryApi.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
// 

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Win32 {

	internal interface IRegistryApi {
		
		int OpenRegKeyRead { get; }
		int OpenRegKeyWrite { get; }

		
		// type values for registry value data
		int RegStringType { get; }
		int RegEnvironmentString { get; }
		int RegBinaryType { get; }
		int RegDwordType { get; }
		int RegStringArrayType { get; }	

		int RegCreateKey (IntPtr keyBase,
				string keyName, out IntPtr keyHandle);		

		int RegCloseKey (IntPtr keyHandle);

		int RegFlushKey (IntPtr keyHandle);

		int RegOpenKeyEx (IntPtr keyBase,
				string keyName, IntPtr reserved, int access,
				out IntPtr keyHandle);

		int RegDeleteKey (IntPtr keyHandle, 
				string valueName);

		int RegDeleteValue (IntPtr keyHandle, 
				string valueName);

		int RegEnumKey (IntPtr keyBase, int index,
				[Out] byte[] nameBuffer, int bufferLength);
	
		int RegEnumValue (IntPtr keyBase, 
				int index, StringBuilder nameBuffer, 
				ref int nameLength, IntPtr reserved, 
				ref int type, IntPtr data, IntPtr dataLength);

		int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				StringBuilder data, int rawDataLength);

		int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				string data, int rawDataLength);

		int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				byte[] rawData, int rawDataLength);

		int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				ref int data, int rawDataLength);

		int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref int type,
				IntPtr zero, ref int dataSize);
		
		int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref int type,
				[Out] byte[] data, ref int dataSize);

		int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref int type,
				ref int data, ref int dataSize);
	}
}

