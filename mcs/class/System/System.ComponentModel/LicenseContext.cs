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
	// Serves as base class for RuntimeLicenseContext and DesignTimeLicenseContext
	// (no clue why this isn't abstract)
	public class LicenseContext : IServiceProvider
	{

		public LicenseContext()
		{
		}

		public virtual string GetSavedLicenseKey (Type type,
							  Assembly resourceAssembly)
		{
			return null;
		}

		public virtual object GetService (Type type)
		{
			return null;
		}

		public virtual void SetSavedLicenseKey (Type type, string key)
		{
			// Intentionally empty
		}

		public virtual LicenseUsageMode UsageMode {
			get {
				return LicenseUsageMode.Runtime;
			}
		}
	}
}
