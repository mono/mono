//
// System.ComponentModel.LicenseManager
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public sealed class LicenseManager
	{
		[MonoTODO]
		public static LicenseContext CurrentContext {
			[MonoTODO]
			get { throw new NotImplementedException(); } 
			[MonoTODO]
			set { throw new NotImplementedException(); }
		}

		public static LicenseUsageMode UsageMode {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public static object CreateWithContext (Type type,
							LicenseContext creationContext)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static object CreateWithContext (Type type, 
							LicenseContext creationContext, 
							object[] args)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static bool IsLicensed (Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static bool IsValid (Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static bool IsValid (Type type, object instance, 
					    out License license)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static void LockContext (object contextUser)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static void UnlockContext (object contextUser)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static void Validate (Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static License Validate (Type type, object instance)
		{
			throw new NotImplementedException();
		}
	}
}
