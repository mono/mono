//
// System.ComponentModel.LicenseContext.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

		public virtual LicenseUsageMode UsageMode {
			get {
				return LicenseUsageMode.Runtime;
			}
		}
	}
}
