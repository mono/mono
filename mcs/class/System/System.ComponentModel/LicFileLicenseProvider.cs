//
// System.ComponentModel.LicFileLicenseProvider
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public class LicFileLicenseProvider : LicenseProvider
	{
		[MonoTODO]
		public LicFileLicenseProvider()
		{
		}

		public override License GetLicense (LicenseContext context,
						    Type type,
						    object instance,
						    bool allowExceptions)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual string GetKey (Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual bool IsKeyValid (string key, Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~LicFileLicenseProvider()
		{
		}
	}
}
