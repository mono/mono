//
// System.ComponentModel.LicFileLicenseProvider.cs
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
	public class LicFileLicenseProvider : LicenseProvider
	{

		public LicFileLicenseProvider()
		{
		}

		[MonoTODO]
		public override License GetLicense (LicenseContext context,
						    Type type,
						    object instance,
						    bool allowExceptions)
		{
			throw new NotImplementedException();
		}

		protected virtual string GetKey (Type type)
		{
			return (type.FullName + " is a licensed component.");
		}

		protected virtual bool IsKeyValid (string key, Type type)
		{
			if (key == null)
				return false;
			return key.Equals (GetKey (type));
		}
	}
}
