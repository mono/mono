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
	}
}

