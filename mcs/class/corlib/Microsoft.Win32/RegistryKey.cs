//
// RegistryKey.cs: a single node in the Windows registry
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Erik LeBel (eriklebel@yahoo.ca)
//   Gert Driesen (drieseng@users.sourceforge.net)
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
using System.Security.AccessControl;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32
{

#if MOBILE && !WIN_PLATFORM
	public sealed class RegistryKey : IDisposable
	{
		internal RegistryKey (RegistryHive hiveId)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Dispose ()
		{
		}

		public RegistryKey CreateSubKey (string subkey)
		{
			throw new PlatformNotSupportedException ();
		}

		public RegistryKey CreateSubKey (String subkey, bool writable)
		{
			throw new PlatformNotSupportedException ();
		}

		public RegistryKey CreateSubKey (String subkey, bool writable, RegistryOptions options)
		{
			throw new PlatformNotSupportedException ();
		}

		public void DeleteSubKey (string subkey)
		{
		}

		public void DeleteSubKey (string subkey, bool throwOnMissingSubKey)
		{
		}

		public void DeleteSubKeyTree (string subkey)
		{
		}

		public void DeleteSubKeyTree (string subkey, bool throwOnMissingSubKey)
		{
		}

		public void DeleteValue (string name)
		{
		}

		public void DeleteValue (string name, bool throwOnMissingValue)
		{
		}

		public void Flush()
		{
		}

		public static RegistryKey FromHandle (SafeRegistryHandle handle)
		{
			throw new PlatformNotSupportedException ();
		}

		public static RegistryKey FromHandle (SafeRegistryHandle handle, RegistryView view)
		{
			throw new PlatformNotSupportedException ();
		}

		public string[] GetSubKeyNames ()
		{
			throw new PlatformNotSupportedException ();
		}

		public object GetValue (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public object GetValue (string name, object defaultValue)
		{
			throw new PlatformNotSupportedException ();
		}

		public object GetValue (string name, object defaultValue, RegistryValueOptions options)
		{
			throw new PlatformNotSupportedException ();
		}

		public RegistryValueKind GetValueKind (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public string[] GetValueNames ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static RegistryKey OpenBaseKey (RegistryHive hKey, RegistryView view)
		{
			throw new PlatformNotSupportedException ();
		}

		public RegistryKey OpenSubKey (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public RegistryKey OpenSubKey (string name, bool writable)
		{
			throw new PlatformNotSupportedException ();
		}

		public RegistryKey OpenSubKey (string name, RegistryRights rights)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetValue (string name, object value)
		{
		}

		public void SetValue (string name, object value, RegistryValueKind valueKind)
		{
		}

		public SafeRegistryHandle Handle {
			get { throw new PlatformNotSupportedException (); }
		}

		public string Name {
			get { throw new PlatformNotSupportedException (); }
		}

		public int SubKeyCount {
			get { throw new PlatformNotSupportedException (); }
		}

		public int ValueCount {
			get { throw new PlatformNotSupportedException (); }
		}

		public RegistryView View {
			get { throw new PlatformNotSupportedException (); }
		}
	}
#else
	/// <summary>
	///	Wrapper class for Windows Registry Entry.
	/// </summary>
	[ComVisible (true)]
	public sealed class RegistryKey : MarshalByRefObject, IDisposable 
	{
		//
		// This represents the backend data, used when creating the
		// RegistryKey object
		//
		object handle;
		SafeRegistryHandle safe_handle;

		object hive; // the RegistryHive if the key represents a base key
		readonly string qname;	// the fully qualified registry key name
		readonly bool isRemoteRoot;	// is an instance of a remote root key?
		readonly bool isWritable;	// is the key openen in writable mode

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
		internal RegistryKey (RegistryHive hiveId) : this (hiveId, 
			new IntPtr ((int) hiveId), false)
		{
		}

		/// <summary>
		///	Construct an instance of a root registry key entry.
		/// </summary>
		internal RegistryKey (RegistryHive hiveId, IntPtr keyHandle, bool remoteRoot)
		{
			hive = hiveId;
			handle = keyHandle;
			qname = GetHiveName (hiveId);
			isRemoteRoot = remoteRoot;
			isWritable = true; // always consider root writable
		}

		/// <summary>
		///	Construct an instance of a registry key entry.
		/// </summary>
		internal RegistryKey (object data, string keyName, bool writable)
		{
			handle = data;
			qname = keyName;
			isWritable = writable;
		}

		static internal bool IsEquals (RegistryKey a, RegistryKey b)
		{
			return a.hive == b.hive && a.handle == b.handle && a.qname == b.qname  && a.isRemoteRoot == b.isRemoteRoot && a.isWritable == b.isWritable;
		}

		#region PublicAPI

		/// <summary>
		///	Dispose of registry key object. Close the 
		///	key if it's still open.
		/// </summary>
		public void Dispose ()
		{
			GC.SuppressFinalize (this);
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
		///	Close the current registry key and flushes the state of the registry
		/// right away.
		/// </summary>
		public void Close()
		{
			Flush ();

			// a handle to a remote hive must be closed, while one to a local
			// hive should not be closed
			if (!isRemoteRoot && IsRoot)
				return;
			
			RegistryApi.Close (this);
			handle = null;
			safe_handle = null;
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

		[ComVisible (false)]
		[MonoTODO ("Not implemented in Unix")]
		public SafeRegistryHandle Handle {
			get {
				AssertKeyStillValid ();

				if (safe_handle == null) {
					IntPtr h = RegistryApi.GetHandle (this);
					safe_handle = new SafeRegistryHandle (h, true);
				}

				return safe_handle;
			}
		}

		[ComVisible (false)]
		[MonoLimitation ("View is ignored in Mono.")]
		public RegistryView View {
			get {
				return RegistryView.Default;
			}
		}

		
		/// <summary>
		///	Set a registry value.
		/// </summary>
		public void SetValue (string name, object value)
		{
			AssertKeyStillValid ();

			if (value == null)
				throw new ArgumentNullException ("value");

			if (name != null)
				AssertKeyNameLength (name);

			if (!IsWritable)
				throw new UnauthorizedAccessException ("Cannot write to the registry key.");

			RegistryApi.SetValue (this, name, value);
		}

		[ComVisible (false)]
		public void SetValue (string name, object value, RegistryValueKind valueKind)
		{
			AssertKeyStillValid ();
			
			if (value == null)
				throw new ArgumentNullException ("value");

			if (name != null)
				AssertKeyNameLength (name);

			if (!IsWritable)
				throw new UnauthorizedAccessException ("Cannot write to the registry key.");

			RegistryApi.SetValue (this, name, value, valueKind);
		}

		/// <summary>
		///	Open the sub key specified, for read access.
		/// </summary>
		public RegistryKey OpenSubKey (string name)
		{
			return OpenSubKey (name, false);
		}

		
		/// <summary>
		///	Open the sub key specified.
		/// </summary>
		public RegistryKey OpenSubKey (string name, bool writable)
		{
			AssertKeyStillValid ();

			if (name == null)
				throw new ArgumentNullException ("name");

			AssertKeyNameLength (name);

			return RegistryApi.OpenSubKey (this, name, writable);
		}
		
		
		/// <summary>
		///	Get a registry value.
		/// </summary>
		public object GetValue (string name)
		{
			return GetValue (name, null);
		}

		
		/// <summary>
		///	Get a registry value.
		/// </summary>
		public object GetValue (string name, object defaultValue)
		{
			AssertKeyStillValid ();
			
			return RegistryApi.GetValue (this, name, defaultValue,
				RegistryValueOptions.None);
		}

		[ComVisible (false)]
		public object GetValue (string name, object defaultValue, RegistryValueOptions options)
		{
			AssertKeyStillValid ();

			return RegistryApi.GetValue (this, name, defaultValue, options);
		}

		[ComVisible (false)]
		public RegistryValueKind GetValueKind (string name)
		{
			return RegistryApi.GetValueKind (this, name);
		}

		/// <summary>
		///	Create a sub key.
		/// </summary>
		public RegistryKey CreateSubKey (string subkey)
		{
			AssertKeyStillValid ();
			AssertKeyNameNotNull (subkey);
			AssertKeyNameLength (subkey);

			if (!IsWritable)
				throw new UnauthorizedAccessException ("Cannot write to the registry key.");
			return RegistryApi.CreateSubKey (this, subkey);
		}

		[ComVisible (false)]
		[MonoLimitation ("permissionCheck is ignored in Mono")]
		public RegistryKey CreateSubKey (string subkey, RegistryKeyPermissionCheck permissionCheck)
		{
			return CreateSubKey (subkey);
		}

		[ComVisible (false)]
		[MonoLimitation ("permissionCheck and registrySecurity are ignored in Mono")]
		public RegistryKey CreateSubKey (string subkey, RegistryKeyPermissionCheck permissionCheck, RegistrySecurity registrySecurity)
		{
			return CreateSubKey (subkey);
		}

		[ComVisible (false)]
		[MonoLimitation ("permissionCheck is ignored in Mono")]
		public RegistryKey CreateSubKey (string subkey, RegistryKeyPermissionCheck permissionCheck, RegistryOptions options)
		{
			AssertKeyStillValid ();
			AssertKeyNameNotNull (subkey);
			AssertKeyNameLength (subkey);

			if (!IsWritable)
				throw new UnauthorizedAccessException ("Cannot write to the registry key.");

			return RegistryApi.CreateSubKey (this, subkey, options);
		}

		[ComVisible (false)]
		[MonoLimitation ("permissionCheck and registrySecurity are ignored in Mono")]
		public RegistryKey CreateSubKey (string subkey, RegistryKeyPermissionCheck permissionCheck, RegistryOptions registryOptions,
			RegistrySecurity registrySecurity)
		{
			return CreateSubKey (subkey, permissionCheck, registryOptions);
		}

		[ComVisible(false)]
		public RegistryKey CreateSubKey (string subkey, bool writable)
		{
			return CreateSubKey (subkey, writable ? RegistryKeyPermissionCheck.ReadWriteSubTree : RegistryKeyPermissionCheck.ReadSubTree);
		}

		[ComVisible(false)]
		public RegistryKey CreateSubKey (string subkey, bool writable, RegistryOptions options)
		{
			return CreateSubKey (subkey, writable ? RegistryKeyPermissionCheck.ReadWriteSubTree : RegistryKeyPermissionCheck.ReadSubTree, options);
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
			AssertKeyNameLength (subkey);

			if (!IsWritable)
				throw new UnauthorizedAccessException ("Cannot write to the registry key.");

			RegistryKey child = OpenSubKey (subkey);
			
			if (child == null) {
				if (throwOnMissingSubKey)
					throw new ArgumentException ("Cannot delete a subkey tree"
						+ " because the subkey does not exist.");
				return;
			}

			if (child.SubKeyCount > 0){
				throw new InvalidOperationException ("Registry key has subkeys"
					+ " and recursive removes are not supported by this method.");
			}
			
			child.Close ();

			RegistryApi.DeleteKey (this, subkey, throwOnMissingSubKey);
		}
		
		
		/// <summary>
		///	Delete a sub tree (node, and values alike).
		/// </summary>
		public void DeleteSubKeyTree(string subkey)
		{
			DeleteSubKeyTree (subkey, true);
		}

		public
		void DeleteSubKeyTree (string subkey, bool throwOnMissingSubKey)
		{
			// Note: this is done by deleting sub-nodes recursively.
			// The preformance is not very good. There may be a 
			// better way to implement this.
			
			AssertKeyStillValid ();
			AssertKeyNameNotNull (subkey);
			AssertKeyNameLength (subkey);
			
			RegistryKey child = OpenSubKey (subkey, true);
			if (child == null) {
				if (!throwOnMissingSubKey)
					return;

				throw new ArgumentException ("Cannot delete a subkey tree"
					+ " because the subkey does not exist.");
			}

			child.DeleteChildKeysAndValues ();
			child.Close ();
			DeleteSubKey (subkey, false);
		}
		

		/// <summary>
		///	Delete a value from the registry.
		/// </summary>
		public void DeleteValue(string name)
		{
			DeleteValue (name, true);
		}
		
		
		/// <summary>
		///	Delete a value from the registry.
		/// </summary>
		public void DeleteValue(string name, bool throwOnMissingValue)
		{
			AssertKeyStillValid ();

			if (name == null)
				throw new ArgumentNullException ("name");

			if (!IsWritable)
				throw new UnauthorizedAccessException ("Cannot write to the registry key.");

			RegistryApi.DeleteValue (this, name, throwOnMissingValue);
		}

		public RegistrySecurity GetAccessControl ()
		{
			return GetAccessControl (AccessControlSections.Owner |
						 AccessControlSections.Group |
						 AccessControlSections.Access);
		}
		
		public RegistrySecurity GetAccessControl (AccessControlSections includeSections)
		{
			return new RegistrySecurity (Name, includeSections);
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

		[ComVisible (false)]
		[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[MonoTODO ("Not implemented on unix")]
		public static RegistryKey FromHandle (SafeRegistryHandle handle)
		{
			if (handle == null)
				throw new ArgumentNullException ("handle");

			return RegistryApi.FromHandle (handle);
		}

		[ComVisible (false)]
		[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[MonoTODO ("Not implemented on unix")]
		public static RegistryKey FromHandle (SafeRegistryHandle handle, RegistryView view)
		{
			return FromHandle (handle);
		}
		
		
		[MonoTODO ("Not implemented on unix")]
		public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey,string machineName)
		{
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			return RegistryApi.OpenRemoteBaseKey (hKey, machineName);
		}

		[ComVisible (false)]
		[MonoTODO ("Not implemented on unix")]
		public static RegistryKey OpenRemoteBaseKey (RegistryHive hKey, string machineName, RegistryView view)
		{
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			return RegistryApi.OpenRemoteBaseKey (hKey, machineName);
		}

		[ComVisible (false)]
		[MonoLimitation ("View is ignored in Mono")]
		public static RegistryKey OpenBaseKey (RegistryHive hKey, RegistryView view)
		{
			switch (hKey) {
				case RegistryHive.ClassesRoot:
					return Registry.ClassesRoot;
				case RegistryHive.CurrentConfig:
					return Registry.CurrentConfig;
				case RegistryHive.CurrentUser:
					return Registry.CurrentUser;
				case RegistryHive.DynData:
					return Registry.DynData;
				case RegistryHive.LocalMachine:
					return Registry.LocalMachine;
				case RegistryHive.PerformanceData:
					return Registry.PerformanceData;
				case RegistryHive.Users:
					return Registry.Users;
			}

			throw new ArgumentException ("hKey");
		}

		[ComVisible (false)]
		public RegistryKey OpenSubKey (string name, RegistryKeyPermissionCheck permissionCheck)
		{
			return OpenSubKey (name, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree);
		}

		[ComVisible (false)]
		[MonoLimitation ("rights are ignored in Mono")]
		public RegistryKey OpenSubKey (string name, RegistryRights rights)
		{
			return OpenSubKey (name);
		}

		[ComVisible (false)]
		[MonoLimitation ("rights are ignored in Mono")]
		public RegistryKey OpenSubKey (string name, RegistryKeyPermissionCheck permissionCheck, RegistryRights rights)
		{
			return OpenSubKey (name, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree);
		}
		
		public void SetAccessControl (RegistrySecurity registrySecurity)
		{
			if (null == registrySecurity)
				throw new ArgumentNullException ("registrySecurity");
				
			registrySecurity.PersistModifications (Name);
		}
		
		
		/// <summary>
		///	Build a string representation of the registry key.
		///	Conatins the fully qualified key name, and the Hex
		///	representation of the registry key handle.
		/// </summary>
		public override string ToString()
		{
			AssertKeyStillValid ();

			return RegistryApi.ToString (this);
		}

		#endregion // PublicAPI

		internal bool IsRoot {
			get { return hive != null; }
		}

		private bool IsWritable {
			get { return isWritable; }
		}

		internal RegistryHive Hive {
			get {
				if (!IsRoot)
					throw new NotSupportedException ();
				return (RegistryHive) hive;
			}
		}

		// returns the key handle for the win32 implementation and the
		// KeyHandler for the unix implementation
		internal object InternalHandle {
			get { return handle; }
		}

		/// <summary>
		/// validate that the registry key handle is still usable.
		/// </summary>
		private void AssertKeyStillValid ()
		{
			if (handle == null)
				throw new ObjectDisposedException ("Microsoft.Win32.RegistryKey");
		}

		
		/// <summary>
		/// validate that the registry key handle is still usable, and
		/// that the 'subKeyName' is not null.
		/// </summary>
		private void AssertKeyNameNotNull (string subKeyName)
		{
			if (subKeyName == null)
				throw new ArgumentNullException ("name");
		}

		private void AssertKeyNameLength (string name)
		{
			if (name.Length > 255)
				throw new ArgumentException ("Name of registry key cannot be greater than 255 characters");
		}

		/// <summary>
		///	Utility method to delelte a key's sub keys and values.
		///	This method removes a level of indirection when deleting
		///	key node trees.
		/// </summary>
		private void DeleteChildKeysAndValues ()
		{
			if (IsRoot)
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
			if (idx != -1)
				stringRep = stringRep.TrimEnd ('\0');
			return stringRep;
		}

		static internal IOException CreateMarkedForDeletionException ()
		{
			throw new IOException ("Illegal operation attempted on a"
				+ " registry key that has been marked for deletion.");
		}

		static string GetHiveName (RegistryHive hive)
		{
			switch (hive) {
			case RegistryHive.ClassesRoot:
				return "HKEY_CLASSES_ROOT";
			case RegistryHive.CurrentConfig:
				return "HKEY_CURRENT_CONFIG";
			case RegistryHive.CurrentUser:
				return "HKEY_CURRENT_USER";
			case RegistryHive.DynData:
				return "HKEY_DYN_DATA";
			case RegistryHive.LocalMachine:
				return "HKEY_LOCAL_MACHINE";
			case RegistryHive.PerformanceData:
				return "HKEY_PERFORMANCE_DATA";
			case RegistryHive.Users:
				return "HKEY_USERS";
			}

			throw new NotImplementedException (string.Format (
				"Registry hive '{0}' is not implemented.", hive.ToString ()));
		}

	}
#endif
}

