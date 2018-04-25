//
// Microsoft.Win32/Win32RegistryApi.cs: wrapper for win32 registry API
//
// Authos:
//	Erik LeBel (eriklebel@yahoo.ca)
//      Jackson Harper (jackson@ximian.com)
//      Miguel de Icaza (miguel@gnome.org)
//
// Copyright (C) Erik LeBel 2004
// (C) 2004, 2005 Novell, Inc (http://www.novell.com)
// 

//
// Copyright (C) 2004, 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if WIN_PLATFORM

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32
{
	/// <summary>
	///	Function stubs, constants and helper functions for
	///	the Win32 registry manipulation utilities.
	/// </summary>
	internal class Win32RegistryApi : IRegistryApi
	{
		// bit masks for registry key open access permissions
		const int OpenRegKeyRead = 0x00020019; 
		const int OpenRegKeyWrite = 0x00020006; 

		// FIXME must be a way to determin this dynamically?
		const int Int32ByteSize = 4;
		const int Int64ByteSize = 8;

		// FIXME this is hard coded on Mono, can it be determined dynamically? 
		readonly int NativeBytesPerCharacter = Marshal.SystemDefaultCharSize;

		const int RegOptionsNonVolatile = 0x00000000;
		const int RegOptionsVolatile = 0x00000001;

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegCreateKeyEx")]
		static extern int RegCreateKeyEx (IntPtr keyBase, string keyName, int reserved, 
			IntPtr lpClass, int options, int access, IntPtr securityAttrs,
			out IntPtr keyHandle, out int disposition);
	       
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegCloseKey")]
		static extern int RegCloseKey (IntPtr keyHandle);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode)]
		static extern int RegConnectRegistry (string machineName, IntPtr hKey,
				out IntPtr keyHandle);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegFlushKey")]
		private static extern int RegFlushKey (IntPtr keyHandle);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegOpenKeyEx")]
		private static extern int RegOpenKeyEx (IntPtr keyBase,
				string keyName, IntPtr reserved, int access,
				out IntPtr keyHandle);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegDeleteKey")]
		private static extern int RegDeleteKey (IntPtr keyHandle, string valueName);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegDeleteValue")]
		private static extern int RegDeleteValue (IntPtr keyHandle, string valueName);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegEnumKeyExW")]
		internal unsafe static extern int RegEnumKeyEx (IntPtr keyHandle, int dwIndex,
					char* lpName, ref int lpcbName, int[] lpReserved,
					[Out]StringBuilder lpClass, int[] lpcbClass,
					long[] lpftLastWriteTime);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegEnumValue")]
		internal unsafe static extern int RegEnumValue (IntPtr hKey, int dwIndex,
					char* lpValueName, ref int lpcbValueName,
					IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData,
					int[] lpcbData);

//		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
//		private static extern int RegSetValueEx (IntPtr keyBase, 
//				string valueName, IntPtr reserved, RegistryValueKind type,
//				StringBuilder data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryValueKind type,
				string data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryValueKind type,
				byte[] rawData, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryValueKind type,
				ref int data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryValueKind type,
				ref long data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryValueKind type,
				IntPtr zero, ref int dataSize);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryValueKind type,
				[Out] byte[] data, ref int dataSize);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryValueKind type,
				ref int data, ref int dataSize);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryValueKind type,
				ref long data, ref int dataSize);

		[DllImport ("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint="RegQueryInfoKeyW")]
		internal static extern int RegQueryInfoKey (IntPtr hKey, [Out]StringBuilder lpClass,
			int[] lpcbClass, IntPtr lpReserved_MustBeZero, ref int lpcSubKeys,
			int[] lpcbMaxSubKeyLen, int[] lpcbMaxClassLen,
			ref int lpcValues, int[] lpcbMaxValueNameLen,
			int[] lpcbMaxValueLen, int[] lpcbSecurityDescriptor,
			int[] lpftLastWriteTime);

		// Returns our handle from the RegistryKey
		public IntPtr GetHandle (RegistryKey key)
		{
			return (IntPtr) key.InternalHandle;
		}

		static bool IsHandleValid (RegistryKey key)
		{
			return key.InternalHandle != null;
		}

		public RegistryValueKind GetValueKind (RegistryKey rkey, string name)
		{
			RegistryValueKind type = 0;
			int size = 0;
			IntPtr handle = GetHandle (rkey);
			int result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, IntPtr.Zero, ref size);

			if (result == Win32ResultCode.FileNotFound || result == Win32ResultCode.MarkedForDeletion) 
				return RegistryValueKind.Unknown;

			return type;
		}
		
		/// <summary>
		/// Acctually read a registry value. Requires knowledge of the
		/// value's type and size.
		/// </summary>
		public object GetValue (RegistryKey rkey, string name, object defaultValue, RegistryValueOptions options)
		{
			RegistryValueKind type = 0;
			int size = 0;
			object obj = null;
			IntPtr handle = GetHandle (rkey);
			int result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, IntPtr.Zero, ref size);

			if (result == Win32ResultCode.FileNotFound || result == Win32ResultCode.MarkedForDeletion) {
				return defaultValue;
			}
			
			if (result != Win32ResultCode.MoreData && result != Win32ResultCode.Success ) {
				GenerateException (result);
			}
			
			if (type == RegistryValueKind.String) {
				byte[] data;
				result = GetBinaryValue (rkey, name, type, out data, size);
				obj = RegistryKey.DecodeString (data);
			} else if (type == RegistryValueKind.ExpandString) {
				byte [] data;
				result = GetBinaryValue (rkey, name, type, out data, size);
				obj = RegistryKey.DecodeString (data);
				if ((options & RegistryValueOptions.DoNotExpandEnvironmentNames) == 0)
					obj = Environment.ExpandEnvironmentVariables ((string) obj);
			} else if (type == RegistryValueKind.DWord) {
				int data = 0;
				result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, ref data, ref size);
				obj = data;
			} else if (type == RegistryValueKind.QWord) {
				long data = 0;
				result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, ref data, ref size);
				obj = data;
			} else if (type == RegistryValueKind.Binary) {
				byte[] data;
				result = GetBinaryValue (rkey, name, type, out data, size);
				obj = data;
			} else if (type == RegistryValueKind.MultiString) {
				obj = null;
				byte[] data;
				result = GetBinaryValue (rkey, name, type, out data, size);
				
				if (result == Win32ResultCode.Success)
					obj = RegistryKey.DecodeString (data).Split ('\0');
			} else {
				// should never get here
				throw new SystemException ();
			}

			// check result codes again:
			if (result != Win32ResultCode.Success)
			{
				GenerateException (result);
			}
			

			return obj;
		}

		//
		// This version has to do extra checking, make sure that the requested
		// valueKind matches the type of the value being stored
		//
		public void SetValue (RegistryKey rkey, string name, object value, RegistryValueKind valueKind)
		{
			Type type = value.GetType ();
			IntPtr handle = GetHandle (rkey);

			switch (valueKind) {
			case RegistryValueKind.QWord:
				try {
					long rawValue = Convert.ToInt64 (value);
					CheckResult (RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.QWord, ref rawValue, Int64ByteSize));
					return;
				} catch (OverflowException) {
				}
				break;
			case RegistryValueKind.DWord:
				try {
					int rawValue = Convert.ToInt32 (value);
					CheckResult (RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.DWord, ref rawValue, Int32ByteSize));
					return;
				} catch (OverflowException) {
				}
				break;
			case RegistryValueKind.Binary:
				if (type == typeof (byte[])) {
					byte[] rawValue = (byte[]) value;
					CheckResult (RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.Binary, rawValue, rawValue.Length));
					return;
				}
				break;
			case RegistryValueKind.MultiString:
				if (type == typeof (string[])) {
					string[] vals = (string[]) value;
					StringBuilder fullStringValue = new StringBuilder ();
					foreach (string v in vals)
					{
						fullStringValue.Append (v);
						fullStringValue.Append ('\0');
					}
					fullStringValue.Append ('\0');

					byte[] rawValue = Encoding.Unicode.GetBytes (fullStringValue.ToString ());
			
					CheckResult (RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.MultiString, rawValue, rawValue.Length));
					return;
				}
				break;
			case RegistryValueKind.String:
			case RegistryValueKind.ExpandString:
				if (type == typeof (string)) {
					string rawValue = String.Format ("{0}{1}", value, '\0');
					CheckResult (RegSetValueEx (handle, name, IntPtr.Zero, valueKind, rawValue,
								rawValue.Length * NativeBytesPerCharacter));
					return;
				}
				break;
			default:
				if (type.IsArray) {
					throw new ArgumentException ("Only string and byte arrays can written as registry values");
				}
				break;
			}

			throw new ArgumentException ("Type does not match the valueKind");
		}
	
		public void SetValue (RegistryKey rkey, string name, object value)
		{
			Type type = value.GetType ();
			int result;
			IntPtr handle = GetHandle (rkey);

			if (type == typeof (int)) {
				int rawValue = (int)value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.DWord, ref rawValue, Int32ByteSize); 
			} else if (type == typeof (byte[])) {
				byte[] rawValue = (byte[]) value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.Binary, rawValue, rawValue.Length);
			} else if (type == typeof (string[])) {
				string[] vals = (string[]) value;
				StringBuilder fullStringValue = new StringBuilder ();
				foreach (string v in vals)
				{
					fullStringValue.Append (v);
					fullStringValue.Append ('\0');
				}
				fullStringValue.Append ('\0');

				byte[] rawValue = Encoding.Unicode.GetBytes (fullStringValue.ToString ());
			
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.MultiString, rawValue, rawValue.Length); 
			} else if (type.IsArray) {
				throw new ArgumentException ("Only string and byte arrays can written as registry values");
			} else {
				string rawValue = String.Format ("{0}{1}", value, '\0');
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.String, rawValue,
							rawValue.Length * NativeBytesPerCharacter);
			}

			// handle the result codes
			if (result != Win32ResultCode.Success)
			{
				GenerateException (result);
			}
		}

		/// <summary>
		///	Get a binary value.
		/// </summary>
		private int GetBinaryValue (RegistryKey rkey, string name, RegistryValueKind type, out byte[] data, int size)
		{
			byte[] internalData = new byte [size];
			IntPtr handle = GetHandle (rkey);
			int result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, internalData, ref size);
			data = internalData;
			return result;
		}

		// MSDN defines the following limits for registry key names & values:
		// Key Name: 255 characters
		// Value name:  16,383 Unicode characters
		// Value: either 1 MB or current available memory, depending on registry format.
		private const int MaxKeyLength = 255;
		private const int MaxValueLength = 16383;
		
		public int SubKeyCount (RegistryKey rkey)
		{
			int subkeys = 0;
			int junk = 0;
			int ret = RegQueryInfoKey (GetHandle (rkey),
									   null,
									   null,
									   IntPtr.Zero,
									   ref subkeys,  // subkeys
									   null,
									   null,
									   ref junk,     // values
									   null,
									   null,
									   null,
									   null);

			if (ret != Win32ResultCode.Success)
				GenerateException (ret);
			return subkeys;
		}

		public int ValueCount (RegistryKey rkey)
		{
			int values = 0;
			int junk = 0;
			int ret = RegQueryInfoKey (GetHandle (rkey),
									   null,
									   null,
									   IntPtr.Zero,
									   ref junk,     // subkeys
									   null,
									   null,
									   ref values,   // values
									   null,
									   null,
									   null,
									   null);
			if (ret != Win32ResultCode.Success)
				GenerateException (ret);
			return values;
		}

		public RegistryKey OpenRemoteBaseKey (RegistryHive hKey, string machineName)
		{
			IntPtr handle = new IntPtr ((int) hKey);

			IntPtr keyHandle;
			int result = RegConnectRegistry (machineName, handle, out keyHandle);
			if (result != Win32ResultCode.Success)
				GenerateException (result);

			return new RegistryKey (hKey, keyHandle, true);
		}

		public RegistryKey OpenSubKey (RegistryKey rkey, string keyName, bool writable)
		{
			int access = OpenRegKeyRead;
			if (writable) access |= OpenRegKeyWrite;
			IntPtr handle = GetHandle (rkey);
			
			IntPtr subKeyHandle;
			int result = RegOpenKeyEx (handle, keyName, IntPtr.Zero, access, out subKeyHandle);

			if (result == Win32ResultCode.FileNotFound || result == Win32ResultCode.MarkedForDeletion)
				return null;
			
			if (result != Win32ResultCode.Success)
				GenerateException (result);
			
			return new RegistryKey (subKeyHandle, CombineName (rkey, keyName), writable);
		}

		public void Flush (RegistryKey rkey)
		{
			if (!IsHandleValid (rkey))
				return;
			IntPtr handle = GetHandle (rkey);
			RegFlushKey (handle);
		}

		public void Close (RegistryKey rkey)
		{
			if (!IsHandleValid (rkey))
				return;
			SafeRegistryHandle safe_handle = rkey.Handle;
			if (safe_handle != null) {
				// closes the unmanaged pointer for us.
				safe_handle.Close ();
				return;
			}
			IntPtr handle = GetHandle (rkey);
			RegCloseKey (handle);
		}

		public RegistryKey FromHandle (SafeRegistryHandle handle)
		{
			// At this point we can't tell whether the key is writable
			// or not (nor the name), so we let the error check code handle it later, as
			// .Net seems to do.
			return new RegistryKey (handle.DangerousGetHandle (), String.Empty, true);
		}

		public RegistryKey CreateSubKey (RegistryKey rkey, string keyName)
		{
			IntPtr handle = GetHandle (rkey);
			IntPtr subKeyHandle;
			int disposition;
			int result = RegCreateKeyEx (handle , keyName, 0, IntPtr.Zero,
				RegOptionsNonVolatile,
				OpenRegKeyRead | OpenRegKeyWrite, IntPtr.Zero, out subKeyHandle, out disposition);

			if (result != Win32ResultCode.Success) {
				GenerateException (result);
			}
			
			return new RegistryKey (subKeyHandle, CombineName (rkey, keyName),
				true);
		}

		public RegistryKey CreateSubKey (RegistryKey rkey, string keyName, RegistryOptions options)
		{
			IntPtr handle = GetHandle (rkey);
			IntPtr subKeyHandle;
			int disposition;
			int result = RegCreateKeyEx (handle , keyName, 0, IntPtr.Zero,
				options == RegistryOptions.Volatile ? RegOptionsVolatile : RegOptionsNonVolatile,
				OpenRegKeyRead | OpenRegKeyWrite, IntPtr.Zero, out subKeyHandle, out disposition);

			if (result != Win32ResultCode.Success)
				GenerateException (result);
			
			return new RegistryKey (subKeyHandle, CombineName (rkey, keyName),
				true);
		}

		public void DeleteKey (RegistryKey rkey, string keyName, bool shouldThrowWhenKeyMissing)
		{
			IntPtr handle = GetHandle (rkey);
			int result = RegDeleteKey (handle, keyName);

			if (result == Win32ResultCode.FileNotFound) {
				if (shouldThrowWhenKeyMissing)
					throw new ArgumentException ("key " + keyName);
				return;
			}
			
			if (result != Win32ResultCode.Success)
				GenerateException (result);
		}

		public void DeleteValue (RegistryKey rkey, string value, bool shouldThrowWhenKeyMissing)
		{
			IntPtr handle = GetHandle (rkey);
			int result = RegDeleteValue (handle, value);

			if (result == Win32ResultCode.MarkedForDeletion)
				return;

			if (result == Win32ResultCode.FileNotFound){
				if (shouldThrowWhenKeyMissing)
					throw new ArgumentException ("value " + value);
				return;
			}
			
			if (result != Win32ResultCode.Success)
				GenerateException (result);
		}

		public unsafe string [] GetSubKeyNames (RegistryKey rkey)
		{
			int subkeys = SubKeyCount (rkey);
			var names = new string [subkeys];  // Returns 0-length array if empty.

			if (subkeys > 0) {
				var hkey = GetHandle (rkey);
				char[] name = new char [MaxKeyLength + 1];
				int namelen;

				fixed (char* namePtr = &name [0]) {
					for (int i = 0; i < subkeys; i++) {
						namelen = name.Length; // Don't remove this. The API's doesn't work if this is not properly initialised.
						int ret = RegEnumKeyEx (hkey,
							i,
							namePtr,
							ref namelen,
							null,
							null,
							null,
							null);

						if (ret != 0)
							GenerateException (ret);
						names [i] = new String (namePtr);
					}
				}
			}

			return names;
		}

		public unsafe string [] GetValueNames (RegistryKey rkey)
		{
			int values = ValueCount (rkey);
			String[] names = new String [values];

			if (values > 0) {
				IntPtr hkey = GetHandle (rkey);
				char[] name = new char [MaxValueLength + 1];
				int namelen;

				fixed (char* namePtr = &name [0]) {
					for (int i = 0; i < values; i++) {
						namelen = name.Length;

						int ret = RegEnumValue (hkey,
							i,
							namePtr,
							ref namelen,
							IntPtr.Zero,
							null,
							null,
							null);

						if (ret != Win32ResultCode.Success && ret != Win32Native.ERROR_MORE_DATA)
							GenerateException (ret);

						names [i] = new String (namePtr);
					}
				}
			}

			return names;
		}

		private void CheckResult (int result)
		{
			if (result != Win32ResultCode.Success) {
				GenerateException (result);
			}
		}

		/// <summary>
		/// convert a win32 error code into an appropriate exception.
		/// </summary>
		private void GenerateException (int errorCode)
		{
			switch (errorCode) {
				case Win32ResultCode.FileNotFound:
				case Win32ResultCode.InvalidParameter:
					throw new ArgumentException ();
				case Win32ResultCode.AccessDenied:
					throw new SecurityException ();
				case Win32ResultCode.NetworkPathNotFound:
					throw new IOException ("The network path was not found.");
				case Win32ResultCode.InvalidHandle:
					throw new IOException ("Invalid handle.");
				case Win32ResultCode.MarkedForDeletion:
					throw RegistryKey.CreateMarkedForDeletionException ();
				case Win32ResultCode.ChildMustBeVolatile:
					throw new IOException ("Cannot create a stable subkey under a volatile parent key.");
				default:
					// unidentified system exception
					throw new SystemException ();
			}
		}

		public string ToString (RegistryKey rkey)
		{
			return rkey.Name;
		}

		/// <summary>
		///	utility: Combine the sub key name to the current name to produce a 
		///	fully qualified sub key name.
		/// </summary>
		internal static string CombineName (RegistryKey rkey, string localName)
		{
			return String.Concat (rkey.Name, "\\", localName);
		}
	}
}

#endif // MOBILE

