//
// System.ComponentModel.LicenseContext
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Reflection;

namespace System.ComponentModel
{
	public class LicenseContext : IServiceProvider
	{
		[MonoTODO]
		public LicenseContext()
		{
		}

		public virtual LicenseUsageMode UsageMode {
			[MonoTODO]
			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public virtual string GetSavedLicenseKey (Type type,
							  Assembly resourceAssembly)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual object GetService (Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void SetSavedLicenseKey (Type type, string key)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~LicenseContext()
		{
		}
	}
}
