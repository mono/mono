//
// System.ComponentModel.Design.DesigntimeLicenseContext
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Reflection;

namespace System.ComponentModel.Design
{
	public class DesigntimeLicenseContext : LicenseContext
	{
		[MonoTODO]
		public DesigntimeLicenseContext()
		{
		}

		[MonoTODO]
		public override string GetSavedLicenseKey (Type type,
							   Assembly resourceAssembly)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void SetSavedLicenseKey (Type type, string key)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~DesigntimeLicenseContext()
		{
		}
	}
}
