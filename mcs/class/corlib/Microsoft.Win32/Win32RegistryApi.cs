//
// Microsoft.Win32/Win32RegistryApi.cs: wrapper for win32 registry API
//
// Authos:
//	Erik LeBel (eriklebel@yahoo.ca)
//      Jackson Harper (jackson@ximian.com)
//
// Copyright (C) Erik LeBel 2004
// (C) 2004 Novell, Inc (http://www.novell.com)
// 

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Win32
{
	/// <summary>
	///	Function stubs, constants and helper functions for
	///	the Win32 registry manipulation utilities.
	/// </summary>
	internal class Win32RegistryApi : IRegistryApi
	{
		// bit masks for registry key open access permissions
		public int OpenRegKeyRead {
			get { return 0x00020019; }
		}

		public int OpenRegKeyWrite {
			get { return 0x00020006; }
		}
		
		// type values for registry value data
		public int RegStringType {
			get { return 1; }
		}

		public int RegEnvironmentString {
			get { return 2; }
		}

		public int RegBinaryType {
			get { return 3; }
		}

		public int RegDwordType {
			get { return 4; }
		}

		public int RegStringArrayType {
			get { return 7; }
		}

		/// <summary>
		///	Create a registry key.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegCreateKey_Internal (IntPtr keyBase, 
				string keyName, out IntPtr keyHandle);

		public int RegCreateKey (IntPtr keybase, string keyname, out IntPtr handle)
		{
			return RegCreateKey_Internal (keybase, keyname, out handle);
		}
	       
		/// <summary>
		///	Close a registry key.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegCloseKey_Internal (IntPtr keyHandle);

		public int RegCloseKey (IntPtr handle)
		{
			return RegCloseKey_Internal (handle);
		}

		/// <summary>
		///	Flush a registry key's current state to disk.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegFlushKey_Internal (IntPtr keyHandle);

		public int RegFlushKey (IntPtr handle)
		{
			return RegFlushKey_Internal (handle);
		}

		/// <summary>
		///	Open a registry key.
		///	'unknown' must be zero.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegOpenKeyEx_Internal (IntPtr keyBase,
				string keyName, IntPtr reserved, int access,
				out IntPtr keyHandle);

		public int RegOpenKeyEx (IntPtr keybase, string keyname, IntPtr reserved,
				int access, out IntPtr handle)
		{
			return RegOpenKeyEx_Internal (keybase, keyname, reserved, access, out handle);
		}

		/// <summary>
		///	Delete a registry key.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegDeleteKey_Internal (IntPtr keyHandle, 
				string valueName);

		public int RegDeleteKey (IntPtr handle, string valuename)
		{
			return RegDeleteKey_Internal (handle, valuename);
		}

		/// <summary>
		///	Delete a registry value.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegDeleteValue_Internal (IntPtr keyHandle, 
				string valueName);

		public int RegDeleteValue (IntPtr handle, string valuename)
		{
			return RegDeleteValue_Internal (handle, valuename);
		}

		/// <summary>
		///	Fetch registry key subkeys itteratively.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegEnumKey_Internal (IntPtr keyBase, int index,
				[Out] byte[] nameBuffer, int bufferLength);

		public int RegEnumKey (IntPtr keybase, int index,
				[Out] byte [] namebuffer, int buffer_length)
		{
			return RegEnumKey_Internal (keybase, index, namebuffer, buffer_length);
		}

		/// <summary>
		///	Fetch registry key value names itteratively.
		///
		///	Arguments 'reserved', 'data', 'dataLength' 
		///	should be set to IntPtr.Zero.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegEnumValue_Internal (IntPtr keyBase, 
				int index, StringBuilder nameBuffer, 
				ref int nameLength, IntPtr reserved, 
				ref int type, IntPtr data, IntPtr dataLength);

		public int RegEnumValue (IntPtr keybase, int index, StringBuilder namebuffer,
				ref int namelength, IntPtr reserved, ref int type, IntPtr data,
				IntPtr datalength)
		{
			return RegEnumValue_Internal (keybase, index, namebuffer, ref namelength,
					reserved, ref type, data, datalength);
		}

		/// <summary>
		///	Set a registry value with string builder data.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegSetValueEx_Internal (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				StringBuilder data, int rawDataLength);

		public int RegSetValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				int type, StringBuilder data, int datalength)
		{
			return RegSetValueEx_Internal (keybase, valuename, reserved,
					type, data, datalength);
		}

		/// <summary>
		///	Set a registry value with string data.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegSetValueEx_Internal (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				string data, int rawDataLength);

		public int RegSetValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				int type, string data, int datalength)
		{
			return RegSetValueEx_Internal (keybase, valuename, reserved,
					type, data, datalength);
		}
		
		/// <summary>
		///	Set a registry value with binary data (a byte array).
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegSetValueEx_Internal (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				byte[] rawData, int rawDataLength);

		public int RegSetValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				int type, byte [] data, int datalength)
		{
			return RegSetValueEx_Internal (keybase, valuename, reserved,
					type, data, datalength);
		}
		
		/// <summary>
		///	Set a registry value to a DWORD value.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegSetValueEx_Internal (IntPtr keyBase, 
				string valueName, IntPtr reserved, int type,
				ref int data, int rawDataLength);

		public int RegSetValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				int type, ref int data, int datalength)
		{
			return RegSetValueEx_Internal (keybase, valuename, reserved, type,
					ref data, datalength);
		}

		/// <summary>
		///	Get a registry value's info. No data.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegQueryValueEx_Internal (IntPtr keyBase,
				string valueName, IntPtr reserved, ref int type,
				IntPtr zero, ref int dataSize);

		public int RegQueryValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				ref int type, IntPtr zero, ref int datasize)
		{
			return RegQueryValueEx_Internal (keybase, valuename, reserved,
					ref type, zero, ref datasize);
		}

		/// <summary>
		///	Get a registry value. Binary data.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegQueryValueEx_Internal (IntPtr keyBase,
				string valueName, IntPtr reserved, ref int type,
				[Out] byte[] data, ref int dataSize);

		public int RegQueryValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				ref int type, [Out] byte [] data, ref int datasize)
		{
			return RegQueryValueEx_Internal (keybase, valuename, reserved,
					ref type, data, ref datasize);
		}

		/// <summary>
		///	Get a registry value. DWORD data.
		/// </summary>
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		private static extern int RegQueryValueEx_Internal (IntPtr keyBase,
				string valueName, IntPtr reserved, ref int type,
				ref int data, ref int dataSize);

		public int RegQueryValueEx (IntPtr keybase, string valuename, IntPtr reserved,
				ref int type, ref int data, ref int datasize)
		{
			return RegQueryValueEx_Internal (keybase, valuename, reserved,
					ref type, ref data, ref datasize);
		}
	}
}

