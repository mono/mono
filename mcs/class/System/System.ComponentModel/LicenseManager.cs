//
// System.ComponentModel.LicenseManager.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public sealed class LicenseManager
	{
		private static LicenseContext mycontext = null;

		private LicenseManager ()
		{
		}

		public static LicenseContext CurrentContext {
			get {
				lock (mycontext) {
					return mycontext;
				}
			} 
			set { 
				lock (mycontext) {
					mycontext = value;
				}
			}
		}

		public static LicenseUsageMode UsageMode {
			get { 
				lock (mycontext) 
				{
					return mycontext.UsageMode;

				}
			}
		}

		public static object CreateWithContext (Type type,
							LicenseContext creationContext)
		{
			return CreateWithContext (type, creationContext, new object[0]);
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
