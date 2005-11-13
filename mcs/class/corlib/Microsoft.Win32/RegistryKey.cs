//
// RegistryKey.cs: a single node in the Windows registry
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Erik LeBel (eriklebel@yahoo.ca)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
		//
		// This represents the backend data, used when creating the
		// RegistryKey object
		//
		internal object Data;
		
		string qname;	// the fully qualified registry key name
		bool isRoot;	// is the an instance of a root key?
		
		static readonly IRegistryApi RegistryApi;

		static RegistryKey ()
		{
			if (Path.DirectorySeparatorChar == '\\')
				RegistryApi = new Win32RegistryApi ();
			else
				RegistryApi = new UnixRegistryApi ();
		}

		/// <summary>
		///	Construct an instance of a root registry key entry.
		/// </summary>
		internal RegistryKey (RegistryHive hiveId, string keyName)
		{
			Data = hiveId;
			qname = keyName;
			isRoot = true;
		}
		
		/// <summary>
		///	Construct an instance of a registry key entry.
		/// </summary>
		internal RegistryKey (object data, string keyName)
		{
			Data = data;
			qname = keyName;
			isRoot = false;
		}

		#region PublicAPI

		/// <summary>
		///	Dispose of registry key object. Close the 
		///	key if it's still open.
		/// </summary>
		void IDisposable.Dispose ()
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
			RegistryApi.Flush (this);
		}
		
		
		/// <summary>
		///	Close the current registry key. This may not 
		///	flush the state of the registry right away.
		/// </summary>
		public void Close()
		{
			if (isRoot)
				return;
			
			RegistryApi.Close (this);
			Data = null;
		}
		
		
		/// <summary>
		///	get the number of sub-keys for this key
		/// </summary>
		public int SubKeyCount {
			get {
				AssertKeyStillValid ();

				return RegistryApi.SubKeyCount (this);
			}
		}

		
		/// <summary>
		///	get the number of values for this key
		/// </summary>
		public int ValueCount {
			get {
				AssertKeyStillValid ();

				return RegistryApi.ValueCount (this);
			}
		}

		
		/// <summary>
		///	Set a registry value.
		/// </summary>
		public void SetValue (string name, object value)
		{
			AssertKeyStillValid ();
			
			if (value == null)
				throw new ArgumentNullException ();

			if (isRoot)
				throw new UnauthorizedAccessException ();
			
			RegistryApi.SetValue (this, name, value);
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
			AssertKeyStillValid ();
			AssertKeyNameNotNull (keyName);

			return RegistryApi.OpenSubKey (this, keyName, writtable);
		}
		
		
		/// <summary>
		///	Get a registry value.
		/// </summary>
		public object GetValue (string name)
		{
			return RegistryApi.GetValue (this, name, false, null);
		}

		
		/// <summary>
		///	Get a registry value.
		/// </summary>
		public object GetValue (string name, object defaultValue)
		{
			AssertKeyStillValid ();
			
			return RegistryApi.GetValue (this, name, true, defaultValue);
		}

		
		/// <summary>
		///	Create a sub key.
		/// </summary>
		[MonoTODO("RegistryPermission")]
		public RegistryKey CreateSubKey (string subkey)
		{
			AssertKeyStillValid ();
			AssertKeyNameNotNull (subkey);
			if (subkey.Length > 255)
				throw new ArgumentException ("keyName length is larger than 255 characters", subkey);
			return RegistryApi.CreateSubKey (this, subkey);
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
		public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
		{
			AssertKeyStillValid ();
			AssertKeyNameNotNull (subkey);

			RegistryKey child = OpenSubKey (subkey);
			
			if (child == null) {
				if (throwOnMissingSubKey)
					throw new ArgumentException ("key missing: " + subkey, "subkey");
				return;
			}

			if (child.SubKeyCount > 0){
				throw new InvalidOperationException ("key " + subkey + " has sub keys");
			}
			
			child.Close ();

			RegistryApi.DeleteKey (this, subkey, throwOnMissingSubKey);
		}
		
		
		/// <summary>
		///	Delete a sub tree (node, and values alike).
		/// </summary>
		public void DeleteSubKeyTree(string keyName)
		{
			// Note: this is done by deleting sub-nodes recursively.
			// The preformance is not very good. There may be a 
			// better way to implement this.
			
			AssertKeyStillValid ();
			AssertKeyNameNotNull (keyName);
			
			RegistryKey child = OpenSubKey (keyName, true);
			if (child == null)
				throw new ArgumentException ("key " + keyName + " at " + Name);

			child.DeleteChildKeysAndValues ();
			child.Close ();
			DeleteSubKey (keyName, false);
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
			AssertKeyStillValid ();
			AssertKeyNameNotNull (value);

			RegistryApi.DeleteValue (this, value, shouldThrowWhenKeyMissing);
		}
		
		
		/// <summary>
		///	Get the names of the sub keys.
		/// </summary>
		public string[] GetSubKeyNames()
		{
			AssertKeyStillValid ();

			return RegistryApi.GetSubKeyNames (this);
		}
		
		
		/// <summary>
		///	Get the names of values contained in this key.
		/// </summary>
		public string[] GetValueNames()
		{
			AssertKeyStillValid ();
			return RegistryApi.GetValueNames (this);
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
			return RegistryApi.ToString (this);
		}

		#endregion // PublicAPI

		
		/// <summary>
		/// validate that the registry key handle is still usable.
		/// </summary>
		private void AssertKeyStillValid ()
		{
			if (Data == null)
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
			if (isRoot)
				return;
			
			string[] subKeys = GetSubKeyNames ();
			foreach (string subKey in subKeys)
			{
				RegistryKey sub = OpenSubKey (subKey, true);
				sub.DeleteChildKeysAndValues ();
				sub.Close ();
				DeleteSubKey (subKey, false);
			}

			string[] values = GetValueNames ();
			foreach (string value in values) {
				DeleteValue (value, false);
			}
		}

		/// <summary>
		///	decode a byte array as a string, and strip trailing nulls
		/// </summary>
		static internal string DecodeString (byte[] data)
		{
			string stringRep = Encoding.Unicode.GetString (data);
			int idx = stringRep.IndexOf ('\0');
			if (idx >= 0)
				stringRep = stringRep.Substring (0, idx);
			return stringRep;
		}
	}
}

