//
// System.ComponentModel.LicenseProvider.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public abstract class LicenseProvider
	{

		protected LicenseProvider()
		{
		}

		public abstract License GetLicense (LicenseContext context,
						    Type type,
						    object instance,
						    bool allowExceptions);
	}
}
