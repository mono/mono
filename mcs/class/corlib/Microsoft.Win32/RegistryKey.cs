//
// RegistryKey.cs: a single node in the Windows registry
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//

using System;

namespace Microsoft.Win32 {
	
	public sealed class RegistryKey : MarshalByRefObject, IDisposable {

		void IDisposable.Dispose ()
		{
		}

		[MonoTODO]
		public string Name 
		{
			get {
				return String.Empty;
			}
		}

		[MonoTODO]
		public int SubKeyCount 
		{
			get {
				return 0;
			}
		}

		[MonoTODO]
		public int ValueCount 
		{
			get {
				return 0;
			}
		}

		[MonoTODO]
		public void SetValue (string name, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RegistryKey OpenSubKey (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RegistryKey OpenSubKey (string name, bool writtable)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public object GetValue (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetValue (string name, object defaultvalue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RegistryKey CreateSubKey (string subkey)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void DeleteSubKey(string subkey)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void DeleteSubKeyTree(string subkey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DeleteValue(string value)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Flush()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Close()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string[] GetSubKeyNames()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string[] GetValueNames()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey,string machineName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		
	}
}
