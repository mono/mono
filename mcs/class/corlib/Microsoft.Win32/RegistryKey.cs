//
// RegistryKey.cs: a single node in the Windows registry
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Erik LeBel (eriklebel@yahoo.ca)
//

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft.Win32
{
	/// <summary>
	///	Wrapper class for Windows Registry Entry.
	/// </summary>
	public sealed class RegistryKey : MarshalByRefObject, IDisposable 
	{
		const char NullChar = '\0';
		
		// Arbitrary max size for key/values names that can be fetched.
		// .NET framework SDK docs say that the max name length that can 
		// be used is 255 characters, we'll allow for a bit more.
		const int BufferMaxLength = 1024;

		// FIXME must be a way to determin this dynamically?
		const int Int32ByteSize = 4;

		// FIXME this is hard coded on Mono, can it be determined dynamically? 
		readonly int NativeBytesPerCharacter = Marshal.SystemDefaultCharSize;

		// FIXME this should be determined dynamically.
		// It will be used to decode some return strings
		// for which embeded '\0' must be preserved.
		readonly Encoding Decoder = Encoding.Unicode;
		
		
		IntPtr hkey;	// the reg key handle
		string qname;	// the fully qualified registry key name
		bool isRoot;	// is the an instance of a root key?
		
		IRegistryApi reg_api;

		/// <summary>
		///	Construct an instance of a root registry key entry.
		/// </summary>
		internal RegistryKey (RegistryHive hiveId, string keyName)
		{
			hkey = new IntPtr ((int)hiveId);
			qname = keyName;
			isRoot = true;

			InitRegistryApi ();
		}
		
		/// <summary>
		///	Construct an instance of a registry key entry.
		/// </summary>
		internal RegistryKey (IntPtr hkey, string keyName)
		{
			this.hkey = hkey;
			qname = keyName;
			isRoot = false;

			InitRegistryApi ();
		}

		internal void InitRegistryApi ()
		{
			if (Path.DirectorySeparatorChar == '\\')
				reg_api = new Win32RegistryApi ();
		}

		private IRegistryApi RegistryApi {
			get {
				if (reg_api == null)
					throw new NotImplementedException ("The registry is" +
							" only available on Windows.");
				return reg_api;
			}
		}

		/// <summary>
		///	Fetch the inetrnal registry key.
		/// </summary>
		private IntPtr Handle {
			get { return hkey; }
		}

		
		#region PublicAPI

		/// <summary>
		///	Dispose of registry key object. Close the 
		///	key if it's still open.
		/// </summary>
		public void Dispose ()
		{
			Close ();
			GC.SuppressFinalize (this);
		}

		
		/// <summary>
		///	Final cleanup of registry key object. Close the 
		///	key if it's still open.
		/// </summary>
		~RegistryKey ()
		{
			Close ();
		}

		
		/// <summary>
		///	Get the fully qualified registry key name.
		/// </summary>
		public string Name {
			get { return qname; }
		}
	
		
		/// <summary>
		///	Flush the current registry state to disk.
		/// </summary>
		public void Flush()
		{
			RegTrace (" +Flush");
			RegistryApi.RegFlushKey (Handle);
			RegTrace (" -Flush");
		}
		
		
		/// <summary>
		///	Close the current registry key. This may not 
		///	flush the state of the registry right away.
		/// </summary>
		public void Close()
		{
			if (isRoot)
				return;
			
			RegTrace (" +Close");
			RegistryApi.RegCloseKey (Handle);
			hkey = IntPtr.Zero;
			RegTrace (" -Close");
		}
		
		
		/// <summary>
		///	get the number of sub-keys for this key
		/// </summary>
		public int SubKeyCount {
			get {
				RegTrace (" +SubKeyCount");
				AssertKeyStillValid ();
				
				int index, result;
				byte[] stringBuffer = new byte [BufferMaxLength];

				for (index = 0; true; index ++)
				{
					result = RegistryApi.RegEnumKey (Handle, index, 
							stringBuffer, BufferMaxLength);
					
					if (result == Win32ResultCode.Success)
						continue;
					
					if (result == Win32ResultCode.NoMoreEntries)
						break;
					
					// something is wrong!!
					RegTrace ("Win32Api::ReEnumKey	result='{0}'  name='{1}'", 
							result, Name);
					GenerateException (result);
				}
				
				RegTrace (" -SubKeyCount");
				return index;
			}
		}

		
		/// <summary>
		///	get the number of values for this key
		/// </summary>
		public int ValueCount {
			get {
				RegTrace (" +ValueCount");	
				AssertKeyStillValid ();
			
				int index, result, type, bufferCapacity;
				StringBuilder buffer = new StringBuilder (BufferMaxLength);
				
				for (index = 0; true; index ++)
				{
					type = 0;
					bufferCapacity = buffer.Capacity;
					result = RegistryApi.RegEnumValue (Handle, index, 
							buffer, ref bufferCapacity,
							IntPtr.Zero, ref type, 
							IntPtr.Zero, IntPtr.Zero);

					if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData)
						continue;
					
					if (result == Win32ResultCode.NoMoreEntries)
						break;

					// something is wrong
					RegTrace ("Win32Api::RegEnumValue  result='{0}'	 name='{1}'", 
							result, Name);
					GenerateException (result);
				}

				RegTrace (" -ValueCount");
				return index;
			}
		}

		
		/// <summary>
		///	Set a registry value.
		/// </summary>
		public void SetValue (string name, object value)
		{
			RegTrace (" +SetValue");
			AssertKeyStillValid ();
			
			if (value == null)
				throw new ArgumentNullException ();
			
			Type type = value.GetType ();
			int result;

			if (type == typeof (int))
			{
				int rawValue = (int)value;
				result = RegistryApi.RegSetValueEx (Handle, name, 
						IntPtr.Zero, RegistryApi.RegDwordType, 
						ref rawValue, Int32ByteSize); 
			}
			else if (type == typeof (byte[]))
			{
				byte[] rawValue = (byte[]) value;
				result = RegistryApi.RegSetValueEx (Handle, name,
						IntPtr.Zero, RegistryApi.RegBinaryType,
						rawValue, rawValue.Length);
			}
			else if (type == typeof (string[]))
			{
				string[] vals = (string[]) value;
				StringBuilder fullStringValue = new StringBuilder ();
				foreach (string v in vals)
				{
					fullStringValue.Append (v);
					fullStringValue.Append (NullChar);
				}
				fullStringValue.Append (NullChar);

				byte[] rawValue = Decoder.GetBytes (fullStringValue.ToString ());
			
				result = RegistryApi.RegSetValueEx (Handle, name, 
						IntPtr.Zero, RegistryApi.RegStringArrayType, 
						rawValue, rawValue.Length); 
			}
			else if (type.IsArray)
			{
				throw new ArgumentException ("Only string and byte arrays can written as registry values");
			}
			else
			{
				string rawValue = String.Format ("{0}{1}", value, NullChar);
				result = RegistryApi.RegSetValueEx (Handle, name,
						IntPtr.Zero, RegistryApi.RegStringType,
						rawValue, rawValue.Length * NativeBytesPerCharacter);
			}

			// handle the result codes
			if (result != Win32ResultCode.Success)
			{
				RegTrace ("Win32Api::RegSetValueEx: result: {0}", result);
				GenerateException (result);
			}
			
			RegTrace (" -SetValue");
		}

		
		/// <summary>
		///	Open the sub key specified, for read access.
		/// </summary>
		public RegistryKey OpenSubKey (string keyName)
		{
			return OpenSubKey (keyName, false);
		}

		
		/// <summary>
		///	Open the sub key specified.
		/// </summary>
		public RegistryKey OpenSubKey (string keyName, bool writtable)
		{
			RegTrace (" +OpenSubKey");
			AssertKeyStillValid ();
			AssertKeyNameNotNull (keyName);
			
			int access = RegistryApi.OpenRegKeyRead;
			if (writtable) access |= RegistryApi.OpenRegKeyWrite;
			
			IntPtr subKeyHandle;
			int result = RegistryApi.RegOpenKeyEx (Handle, keyName, IntPtr.Zero, 
					access, out subKeyHandle);

			if (result == Win32ResultCode.FileNotFound)
			{
				RegTrace (" -OpenSubKey");
				return null;
			}
			
			if (result != Win32ResultCode.Success)
			{
				RegTrace ("Win32Api::RegOpenKeyEx  result='{0}'	 key name='{1}'", 
						result, CombineName (keyName));
				GenerateException (result);
			}
			
			RegistryKey subKey = new RegistryKey (subKeyHandle, CombineName (keyName));
			RegTrace (" -OpenSubKey");
			return subKey;
		}
		
		
		/// <summary>
		///	Get a registry value.
		/// </summary>
		public object GetValue (string name)
		{
			RegTrace (" +GetValue");
			object obj = GetValueImpl (name, false, null);
			RegTrace (" -GetValue");
			return obj;
		}

		
		/// <summary>
		///	Get a registry value.
		/// </summary>
		public object GetValue (string name, object defaultValue)
		{
			RegTrace (" +GetValue");
			object obj = GetValueImpl (name, true, defaultValue);
			RegTrace (" -GetValue");
			return obj;
		}

		
		/// <summary>
		///	Create a sub key.
		/// </summary>
		public RegistryKey CreateSubKey (string keyName)
		{
			RegTrace (" +CreateSubKey");
			AssertKeyStillValid ();
			AssertKeyNameNotNull (keyName);
			
			IntPtr subKeyHandle;
			int result = RegistryApi.RegCreateKey (Handle , keyName, out subKeyHandle);

			if (result != Win32ResultCode.Success)
			{
				RegTrace ("Win32Api::RegCreateKey: result='{0}' key name='{1}'", 
						result, CombineName (keyName));
				GenerateException (result);
			}
			
			RegistryKey subKey = new RegistryKey (subKeyHandle, CombineName (keyName));
			RegTrace (" -CreateSubKey");
			return subKey;
		}
		
		
		/// <summary>
		///	Delete the specified subkey.
		/// </summary>
		public void DeleteSubKey(string subkey)
		{
			DeleteSubKey (subkey, true);
		}
		
		
		/// <summary>
		///	Delete the specified subkey.
		/// </summary>
		public void DeleteSubKey(string keyName, bool shouldThrowWhenKeyMissing)
		{
			RegTrace (" +DeleteSubKey");
			AssertKeyStillValid ();
			AssertKeyNameNotNull (keyName);
			
			RegistryKey child = OpenSubKey (keyName);
			
			if (child == null)
			{
				if (shouldThrowWhenKeyMissing)
					throw new ArgumentException ("key " + keyName);
				RegTrace (" -DeleteSubKey");
				return;
			}

			if (child.SubKeyCount > 0)
				throw new InvalidOperationException ("key " + keyName + " has sub keys");
			
			child.Close ();

			int result = RegistryApi.RegDeleteKey (Handle, keyName);
			if (result == Win32ResultCode.FileNotFound)
			{
				if (shouldThrowWhenKeyMissing)
					throw new ArgumentException ("key " + keyName);
				RegTrace (" -DeleteSubKey");
				return;
			}
			
			if (result != Win32ResultCode.Success)
			{
				RegTrace ("Win32Api::RegDeleteKey: result='{0}' key name='{1}'", 
						result, CombineName (keyName));
				GenerateException (result);
			}

			RegTrace (" -DeleteSubKey");
		}
		
		
		/// <summary>
		///	Delete a sub tree (node, and values alike).
		/// </summary>
		public void DeleteSubKeyTree(string keyName)
		{
			// Note: this is done by deleting sub-nodes recursively.
			// The preformance is not very good. There may be a 
			// better way to implement this.
			RegTrace (" +DeleteSubKeyTree");
			AssertKeyStillValid ();
			AssertKeyNameNotNull (keyName);
			
			RegistryKey child = OpenSubKey (keyName, true);
			if (child == null)
				throw new ArgumentException ("key " + keyName);

			child.DeleteChildKeysAndValues ();
			child.Close ();
			DeleteSubKey (keyName, false);
			RegTrace (" -DeleteSubKeyTree");
		}
		

		/// <summary>
		///	Delete a value from the registry.
		/// </summary>
		public void DeleteValue(string value)
		{
			DeleteValue (value, true);
		}
		
		
		/// <summary>
		///	Delete a value from the registry.
		/// </summary>
		public void DeleteValue(string value, bool shouldThrowWhenKeyMissing)
		{
			RegTrace (" +DeleteValue");
			AssertKeyStillValid ();
			AssertKeyNameNotNull (value);
			
			int result = RegistryApi.RegDeleteValue (Handle, value);
			
			if (result == Win32ResultCode.FileNotFound)
			{
				if (shouldThrowWhenKeyMissing)
					throw new ArgumentException ("value " + value);
				RegTrace (" -DeleteValue");
				return;
			}
			
			if (result != Win32ResultCode.Success)
			{
				RegTrace ("Win32Api::RegDeleteValue: result='{0}' value name='{1}'", 
						result, CombineName (value));
				GenerateException (result);
			}
			
			RegTrace (" -DeleteValue");
		}
		
		
		/// <summary>
		///	Get the names of the sub keys.
		/// </summary>
		public string[] GetSubKeyNames()
		{
			RegTrace (" +GetSubKeyNames");
			AssertKeyStillValid ();
			
			byte[] buffer = new byte [BufferMaxLength];
			int bufferCapacity = BufferMaxLength;
			ArrayList keys = new ArrayList ();
				
			for (int index = 0; true; index ++)
			{
				int result = RegistryApi.RegEnumKey (Handle, index, buffer, bufferCapacity);

				if (result == Win32ResultCode.Success)
				{
					keys.Add (DecodeString (buffer));
					continue;
				}

				if (result == Win32ResultCode.NoMoreEntries)
					break;

				// should not be here!
				RegTrace ("Win32Api::RegEnumKey: result='{0}' value name='{1}'", 
						result, CombineName (Name));
				GenerateException (result);
			}

			RegTrace (" -GetSubKeyNames");
			return (string []) keys.ToArray (typeof(String));
		}
		
		
		/// <summary>
		///	Get the names of values contained in this key.
		/// </summary>
		public string[] GetValueNames()
		{
			RegTrace (" +GetValueNames");
			AssertKeyStillValid ();
			
			ArrayList values = new ArrayList ();
			
			for (int index = 0; true; index ++)
			{
				StringBuilder buffer = new StringBuilder (BufferMaxLength);
				int bufferCapacity = buffer.Capacity;
				int type = 0;
				
				int result = RegistryApi.RegEnumValue (Handle, index, buffer, ref bufferCapacity,
							IntPtr.Zero, ref type, IntPtr.Zero, IntPtr.Zero);

				if (result == Win32ResultCode.Success || result == Win32ResultCode.MoreData)
				{
					values.Add (buffer.ToString ());
					continue;
				}
				
				if (result == Win32ResultCode.NoMoreEntries)
					break;
					
				// should not be here!
				RegTrace ("RegistryApi.RegEnumValue: result code='{0}' name='{1}'", 
						result, CombineName (Name));
				GenerateException (result);
			}

			RegTrace (" -GetValueNames");
			return (string []) values.ToArray (typeof(String));
		}
		
		
		[MonoTODO]
		public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey,string machineName)
		{
			throw new NotImplementedException ();
		}
		
		
		/// <summary>
		///	Build a string representation of the registry key.
		///	Conatins the fully qualified key name, and the Hex
		///	representation of the registry key handle.
		/// </summary>
		public override string ToString()
		{
			return String.Format ("{0} [0x{1:X}]", Name, Handle.ToInt32 ());
		}

		#endregion // PublicAPI

		
		/// <summary>
		/// validate that the registry key handle is still usable.
		/// </summary>
		private void AssertKeyStillValid ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException ("Microsoft.Win32.RegistryKey");
		}

		
		/// <summary>
		/// validate that the registry key handle is still usable, and
		/// that the 'subKeyName' is not null.
		/// </summary>
		private void AssertKeyNameNotNull (string subKeyName)
		{
			if (subKeyName == null)
				throw new ArgumentNullException ();
		}
		

		/// <summary>
		///	Utility method to delelte a key's sub keys and values.
		///	This method removes a level of indirection when deleting
		///	key node trees.
		/// </summary>
		private void DeleteChildKeysAndValues ()
		{
			RegTrace (" +DeleteChildKeysAndValues");
			if (isRoot)
			{
				RegTrace (" -DeleteChildKeysAndValues");
				return;
			}
			
			string[] subKeys = GetSubKeyNames ();
			foreach (string subKey in subKeys)
			{
				RegistryKey sub = OpenSubKey (subKey, true);
				sub.DeleteChildKeysAndValues ();
				sub.Close ();
				DeleteSubKey (subKey, false);
			}

			string[] values = GetValueNames ();
			foreach (string value in values)
			{
				DeleteValue (value, false);
			}
			
			RegTrace (" -DeleteChildKeysAndValues");
		}


		/// <summary>
		///	Acctually read a registry value. Requires knoledge of the
		///	value's type and size.
		/// </summary>
		private object GetValueImpl (string name, bool returnDefaultValue, object defaultValue)
		{
			RegTrace (" +GetValueImpl");
			AssertKeyStillValid ();
			
			int type = 0;
			int size = 0;
			object obj = null;
			
			int result = RegistryApi.RegQueryValueEx (Handle, name, IntPtr.Zero,
					ref type, IntPtr.Zero, ref size);
		
			if (returnDefaultValue && result == Win32ResultCode.FileNotFound)
			{
				RegTrace (" -GetValueImpl");
				return defaultValue;
			}
			
			if (result != Win32ResultCode.MoreData && result != Win32ResultCode.Success )
			{
				RegTrace ("Win32Api::RegQueryValueEx: result='{0}'  name='{1}'	type='{2}'  size='{3}'",	
						result, name, type, size);
				GenerateException (result);
			}
			
			if (type == RegistryApi.RegStringType || type == RegistryApi.RegEnvironmentString)
			{
				byte[] data;
				result = GetBinaryValue (name, type, out data, size);
				obj = DecodeString (data);
			}
			else if (type == RegistryApi.RegDwordType)
			{
				int data = 0;
				result = RegistryApi.RegQueryValueEx (Handle, name, IntPtr.Zero,
						ref type, ref data, ref size);
				obj = data;
			}
			else if (type == RegistryApi.RegBinaryType)
			{
				byte[] data;
				result = GetBinaryValue (name, type, out data, size);
				obj = data;
			}
			else if (type == RegistryApi.RegStringArrayType)
			{
				obj = null;
				byte[] data;
				result = GetBinaryValue (name, type, out data, size);
				
				if (result == Win32ResultCode.Success)
					obj = DecodeString (data).Split (NullChar);
			}
			else
			{
				// should never get here
				throw new SystemException ();
			}

			// check result codes again:
			if (result != Win32ResultCode.Success)
			{
				RegTrace ("Win32Api::RegQueryValueEx: result='{0}' name='{1}'", 
						result, name);
				GenerateException (result);
			}
			
			RegTrace (" -ReadValueImpl");
			return obj;
		}

		
		/// <summary>
		///	Get a binary value.
		/// </summary>
		private int GetBinaryValue (string name, int type, out byte[] data, int size)
		{
			byte[] internalData = new byte [size];
			int result = RegistryApi.RegQueryValueEx (Handle, name, 
					IntPtr.Zero, ref type, internalData, ref size);
			data = internalData;
			return result;
		}

		
		/// <summary>
		///	decode a byte array as a string, and strip trailing nulls
		/// </summary>
		private string DecodeString (byte[] data)
		{
			string stringRep = Decoder.GetString (data);
			return stringRep.TrimEnd (NullChar);
		}
		
		
		/// <summary>
		///	utility: Combine the sub key name to the current name to produce a 
		///	fully qualified sub key name.
		/// </summary>
		private string CombineName (string localName)
		{
			return String.Format ("{0}\\{1}", Name, localName);
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
		
#if (false)
		/// <summary>
		///	dump trace messages if this code was compiled with tracing enabled.
		/// </summary>
		[Conditional("TRACE")]
		private static void RegTrace (string message, params object[] args)
		{
			message = "REG " + message;
			if (args.Length > 0)
				message = String.Format (message, args);

			Trace.WriteLine (message);
			//Console.WriteLine (message);
		}
#endif		
		private static void RegTrace (string message, params object[] args)
		{
		}
	}
}

