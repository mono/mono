//
// System.ComponentModel.Design.DesigntimeLicenseContext.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.ComponentModel;
using System.Reflection;
using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesigntimeLicenseContext : LicenseContext
	{
		private Hashtable keys = new Hashtable ();

		public DesigntimeLicenseContext()
		{
		}

		public override string GetSavedLicenseKey (Type type,
							   Assembly resourceAssembly)
		{
			return (string)keys[type];
		}

		public override void SetSavedLicenseKey (Type type, string key)
		{
			keys[type] = key;
		}

		public override LicenseUsageMode UsageMode {
			get {
				// It's a 'Designtime'LicenseContext
				return LicenseUsageMode.Designtime;
			}
		}
	}
}
