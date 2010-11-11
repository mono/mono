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

#if !NET_2_1

using System;
using System.Collections;
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

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegEnumKey")]
		private static extern int RegEnumKey (IntPtr keyBase, int index, StringBuilder nameBuffer, int bufferLength);

		[DllImport ("advapi32.dll", CharSet=CharSet.Unicode, EntryPoint="RegEnumValue")]
		private static extern int RegEnumValue (IntPtr keyBase, 
				int index, StringBuilder nameBuffer, 
				ref int nameLength, IntPtr reserved, 
				ref RegistryValueKind type, IntPtr data, IntPtr dataLength);

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
			int result;
			IntPtr handle = GetHandle (rkey);

			if (valueKind == RegistryValueKind.QWord && type == typeof (long)) {
				long rawValue = (long)value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.QWord, ref rawValue, Int64ByteSize); 
			} else if (valueKind == RegistryValueKind.DWord && type == typeof (int)) {
				int rawValue = (int)value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.DWord, ref rawValue, Int32ByteSize); 
			} else if (valueKind == RegistryValueKind.Binary && type == typeof (byte[])) {
				byte[] rawValue = (byte[]) value;
				result = RegSetValueEx (handle, name, IntPtr.Zero, RegistryValueKind.Binary, rawValue, rawValue.Length);
			} else if (valueKind == RegistryValueKind.MultiString && type == typeof (string[])) {
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
			} else if ((valueKind == RegistryValueKind.String || valueKind == RegistryValueKind.ExpandString) &&
				   type == typeof (string)){
				string rawValue = String.Format ("{0}{1}", value, '\0');
				result = RegSetValueEx (handle, name, IntPtr.Zero, valueKind, rawValue,
							rawValue.Length * NativeBytesPerCharacter);
				
			} else if (type.IsArray) {
				throw new ArgumentException ("Only string and byte arrays can written as registry values");
			} else {
				throw new ArgumentException ("Type does not match the valueKind");
			}

			// handle the result codes
			if (result != Win32ResultCode.Success)
			{
				GenerateException (result);
			}
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

			if (result == Win32ResultCode.MarkedForDeletion)
				throw RegistryKey.CreateMarkedForDeletionException ();

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

		
		// Arbitrary max size for key/values names that can be fetched.
		// .NET framework SDK docs say that the max name length that can 
		// be used is 255 characters, we'll allow for a bit more.
		const int BufferMaxLength = 1024;
		
		public int SubKeyCount (RegistryKey rkey)
		{
			int index;
			StringBuilder stringBuffer = new StringBuilder (BufferMaxLength);
			IntPtr handle = GetHandle (rkey);
			
			for (index = 0; true; index ++) {
				int result = RegEnumKey (handle, index, stringBuffer,
					stringBuffer.Capacity);

				if (result == Win32ResultCode.MarkedForDeletion)
					throw RegistryKey.CreateMarkedForDeletionException ();

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
			RegistryValueKind type;
			StringBuilder buffer = new StringBuilder (BufferMaxLength);
			
			IntPtr handle = GetHandle (rkey);
			for (index = 0; true; index ++) {
				type = 0;
				bufferCapacity = buffer.Capacity;
				result = RegEnumValue (handle, index, 
						       buffer, ref bufferCapacity,
						       IntPtr.Zero, ref type, 
						       IntPtr.Zero, IntPtr.Zero);

				if (result == Win32ResultCode.MarkedForDeletion)
					throw RegistryKey.CreateMarkedForDeletionException ();

				if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData)
					continue;
				
				if (result == Win32ResultCode.NoMoreEntries)
					break;
				
				// something is wrong
				GenerateException (result);
			}
			return index;
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
#if NET_4_0
			SafeRegistryHandle safe_handle = rkey.Handle;
			if (safe_handle != null) {
				// closes the unmanaged pointer for us.
				safe_handle.Close ();
				return;
			}
#endif
			IntPtr handle = GetHandle (rkey);
			RegCloseKey (handle);
		}

#if NET_4_0
		public RegistryKey FromHandle (SafeRegistryHandle handle)
		{
			// At this point we can't tell whether the key is writable
			// or not (nor the name), so we let the error check code handle it later, as
			// .Net seems to do.
			return new RegistryKey (handle.DangerousGetHandle (), String.Empty, true);
		}
#endif

		public RegistryKey CreateSubKey (RegistryKey rkey, string keyName)
		{
			IntPtr handle = GetHandle (rkey);
			IntPtr subKeyHandle;
			int disposition;
			int result = RegCreateKeyEx (handle , keyName, 0, IntPtr.Zero,
				RegOptionsNonVolatile,
				OpenRegKeyRead | OpenRegKeyWrite, IntPtr.Zero, out subKeyHandle, out disposition);

			if (result == Win32ResultCode.MarkedForDeletion)
				throw RegistryKey.CreateMarkedForDeletionException ();

			if (result != Win32ResultCode.Success) {
				GenerateException (result);
			}
			
			return new RegistryKey (subKeyHandle, CombineName (rkey, keyName),
				true);
		}

#if NET_4_0
		public RegistryKey CreateSubKey (RegistryKey rkey, string keyName, RegistryOptions options)
		{
			IntPtr handle = GetHandle (rkey);
			IntPtr subKeyHandle;
			int disposition;
			int result = RegCreateKeyEx (handle , keyName, 0, IntPtr.Zero,
				options == RegistryOptions.Volatile ? RegOptionsVolatile : RegOptionsNonVolatile,
				OpenRegKeyRead | OpenRegKeyWrite, IntPtr.Zero, out subKeyHandle, out disposition);

			if (result == Win32ResultCode.MarkedForDeletion)
				throw RegistryKey.CreateMarkedForDeletionException ();

			if (result != Win32ResultCode.Success)
				GenerateException (result);
			
			return new RegistryKey (subKeyHandle, CombineName (rkey, keyName),
				true);
		}
#endif

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

		public string [] GetSubKeyNames (RegistryKey rkey)
		{
			IntPtr handle = GetHandle (rkey);
			StringBuilder buffer = new StringBuilder (BufferMaxLength);
			ArrayList keys = new ArrayList ();
				
			for (int index = 0; true; index ++) {
				int result = RegEnumKey (handle, index, buffer, buffer.Capacity);

				if (result == Win32ResultCode.Success) {
					keys.Add (buffer.ToString ());
					buffer.Length = 0;
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
				RegistryValueKind type = 0;
				
				int result = RegEnumValue (handle, index, buffer, ref bufferCapacity,
							IntPtr.Zero, ref type, IntPtr.Zero, IntPtr.Zero);

				if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData) {
					values.Add (buffer.ToString ());
					continue;
				}
				
				if (result == Win32ResultCode.NoMoreEntries)
					break;

				if (result == Win32ResultCode.MarkedForDeletion)
					throw RegistryKey.CreateMarkedForDeletionException ();

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
				case Win32ResultCode.NetworkPathNotFound:
					throw new IOException ("The network path was not found.");
				case Win32ResultCode.InvalidHandle:
					throw new IOException ("Invalid handle.");
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

#endif // NET_2_1

