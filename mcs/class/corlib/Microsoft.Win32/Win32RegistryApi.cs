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

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security;
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
		const int OpenRegKeyRead = 0x00020019; 
		const int OpenRegKeyWrite = 0x00020006; 

		// FIXME must be a way to determin this dynamically?
		const int Int32ByteSize = 4;

		// FIXME this is hard coded on Mono, can it be determined dynamically? 
		readonly int NativeBytesPerCharacter = Marshal.SystemDefaultCharSize;

		internal enum RegistryType {
			String = 1,
			EnvironmentString = 2,
			Binary = 3,
			Dword = 4,
			StringArray = 7,
		}
		
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegCreateKey")]
		static extern int RegCreateKey (IntPtr keyBase, string keyName, out IntPtr keyHandle);
	       
		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegCloseKey")]
		static extern int RegCloseKey (IntPtr keyHandle);

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

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegEnumKey")]
		private static extern int RegEnumKey (IntPtr keyBase, int index, [Out] byte[] nameBuffer, int bufferLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegEnumValue")]
		private static extern int RegEnumValue (IntPtr keyBase, 
				int index, StringBuilder nameBuffer, 
				ref int nameLength, IntPtr reserved, 
				ref RegistryType type, IntPtr data, IntPtr dataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryType type,
				StringBuilder data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryType type,
				string data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryType type,
				byte[] rawData, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegSetValueEx")]
		private static extern int RegSetValueEx (IntPtr keyBase, 
				string valueName, IntPtr reserved, RegistryType type,
				ref int data, int rawDataLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryType type,
				IntPtr zero, ref int dataSize);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryType type,
				[Out] byte[] data, ref int dataSize);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegQueryValueEx")]
		private static extern int RegQueryValueEx (IntPtr keyBase,
				string valueName, IntPtr reserved, ref RegistryType type,
				ref int data, ref int dataSize);

		// Returns our handle from the RegistryKey
		static IntPtr GetHandle (RegistryKey key)
		{
			return key.IsRoot ? new IntPtr ((int) key.Data)
				: (IntPtr) key.Data;
		}

		/// <summary>
		///	Acctually read a registry value. Requires knoledge of the
		///	value's type and size.
		/// </summary>
		public object GetValue (RegistryKey rkey, string name, bool returnDefaultValue, object defaultValue)
		{
			RegistryType type = 0;
			int size = 0;
			object obj = null;
			IntPtr handle = GetHandle (rkey);
			int result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, IntPtr.Zero, ref size);

			if (result == Win32ResultCode.FileNotFound) {
				if (returnDefaultValue) {
					return defaultValue;
				}
				return null;
			}
			
			if (result != Win32ResultCode.MoreData && result != Win32ResultCode.Success ) {
				GenerateException (result);
			}
			
			if (type == RegistryType.String || type == RegistryType.EnvironmentString) {
				byte[] data;
				result = GetBinaryValue (rkey, name, type, out data, size);
				obj = RegistryKey.DecodeString (data);
			} else if (type == RegistryType.Dword) {
				int data = 0;
				result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, ref data, ref size);
				obj = data;
			} else if (type == RegistryType.Binary) {
				byte[] data;
				result = GetBinaryValue (rkey, name, type, out data, size);
				obj = data;
			} else if (type == RegistryType.StringArray) {
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

		public void SetValue (RegistryKey rkey, string name, object value)
		{
			Type type = value.GetType ();
			int result;
			IntPtr handle = GetHandle (rkey);

			if (type == typeof (int)) {
				int rawValue = (int)value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryType.Dword, ref rawValue, Int32ByteSize); 
			} else if (type == typeof (byte[])) {
				byte[] rawValue = (byte[]) value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryType.Binary, rawValue, rawValue.Length);
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
			
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryType.StringArray, rawValue, rawValue.Length); 
			} else if (type.IsArray) {
				throw new ArgumentException ("Only string and byte arrays can written as registry values");
			} else {
				string rawValue = String.Format ("{0}{1}", value, '\0');
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryType.String, rawValue,
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
		private int GetBinaryValue (RegistryKey rkey, string name, RegistryType type, out byte[] data, int size)
		{
			byte[] internalData = new byte [size];
			IntPtr handle = GetHandle (rkey);
			int result = RegQueryValueEx (handle, name, IntPtr.Zero, ref type, internalData, ref size);
			data = internalData;
			return result;
		}

		
		// Arbitrary max size for key/values names that can be fetched.
		// .NET framework SDK docs say that the max name length that can 
		// be used is 255 characters, we'll allow for a bit more.
		const int BufferMaxLength = 1024;
		
		public int SubKeyCount (RegistryKey rkey)
		{
			int index, result;
			byte[] stringBuffer = new byte [BufferMaxLength];
			IntPtr handle = GetHandle (rkey);
			
			for (index = 0; true; index ++) {
				result = RegEnumKey (handle, index, stringBuffer, BufferMaxLength);
				
				if (result == Win32ResultCode.Success)
					continue;
				
				if (result == Win32ResultCode.NoMoreEntries)
					break;
				
				// something is wrong!!
				GenerateException (result);
			}
			return index;
		}

		public int ValueCount (RegistryKey rkey)
		{
			int index, result, bufferCapacity;
			RegistryType type;
			StringBuilder buffer = new StringBuilder (BufferMaxLength);
			
			IntPtr handle = GetHandle (rkey);
			for (index = 0; true; index ++) {
				type = 0;
				bufferCapacity = buffer.Capacity;
				result = RegEnumValue (handle, index, 
						       buffer, ref bufferCapacity,
						       IntPtr.Zero, ref type, 
						       IntPtr.Zero, IntPtr.Zero);
				
				if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData)
					continue;
				
				if (result == Win32ResultCode.NoMoreEntries)
					break;
				
				// something is wrong
				GenerateException (result);
			}
			return index;
		}
		
		public RegistryKey OpenSubKey (RegistryKey rkey, string keyName, bool writtable)
		{
			int access = OpenRegKeyRead;
			if (writtable) access |= OpenRegKeyWrite;
			IntPtr handle = GetHandle (rkey);
			
			IntPtr subKeyHandle;
			int result = RegOpenKeyEx (handle, keyName, IntPtr.Zero, access, out subKeyHandle);

			if (result == Win32ResultCode.FileNotFound)
				return null;
			
			if (result != Win32ResultCode.Success)
				GenerateException (result);
			
			return new RegistryKey (subKeyHandle, CombineName (rkey, keyName));
		}

		public void Flush (RegistryKey rkey)
		{
			IntPtr handle = GetHandle (rkey);
			RegFlushKey (handle);
		}

		public void Close (RegistryKey rkey)
		{
			IntPtr handle = GetHandle (rkey);
			RegCloseKey (handle);
		}

		public RegistryKey CreateSubKey (RegistryKey rkey, string keyName)
		{
			IntPtr handle = GetHandle (rkey);
			IntPtr subKeyHandle;
			int result = RegCreateKey (handle , keyName, out subKeyHandle);

			if (result != Win32ResultCode.Success) {
				GenerateException (result);
			}
			
			RegistryKey subKey = new RegistryKey (subKeyHandle, CombineName (rkey, keyName));

			return subKey;
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
			
			if (result == Win32ResultCode.FileNotFound){
				if (shouldThrowWhenKeyMissing)
					throw new ArgumentException ("value " + value);
				return;
			}
			
			if (result != Win32ResultCode.Success)
				GenerateException (result);
		}

		public string [] GetSubKeyNames (RegistryKey rkey)
		{
			IntPtr handle = GetHandle (rkey);
			byte[] buffer = new byte [BufferMaxLength];
			int bufferCapacity = BufferMaxLength;
			ArrayList keys = new ArrayList ();
				
			for (int index = 0; true; index ++) {
				int result = RegEnumKey (handle, index, buffer, bufferCapacity);

				if (result == Win32ResultCode.Success) {
					keys.Add (RegistryKey.DecodeString (buffer));
					continue;
				}

				if (result == Win32ResultCode.NoMoreEntries)
					break;

				// should not be here!
				GenerateException (result);
			}
			return (string []) keys.ToArray (typeof(String));
		}


		public string [] GetValueNames (RegistryKey rkey)
		{
			IntPtr handle = GetHandle (rkey);
			ArrayList values = new ArrayList ();
			
			for (int index = 0; true; index ++)
			{
				StringBuilder buffer = new StringBuilder (BufferMaxLength);
				int bufferCapacity = buffer.Capacity;
				RegistryType type = 0;
				
				int result = RegEnumValue (handle, index, buffer, ref bufferCapacity,
							IntPtr.Zero, ref type, IntPtr.Zero, IntPtr.Zero);

				if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData) {
					values.Add (buffer.ToString ());
					continue;
				}
				
				if (result == Win32ResultCode.NoMoreEntries)
					break;
					
				GenerateException (result);
			}

			return (string []) values.ToArray (typeof(String));
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

				default:
					// unidentified system exception
					throw new SystemException ();
			}
		}

		public string ToString (RegistryKey rkey)
		{
			IntPtr handle = GetHandle (rkey);
			
			return String.Format ("{0} [0x{1:X}]", rkey.Name, handle.ToInt32 ());
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

