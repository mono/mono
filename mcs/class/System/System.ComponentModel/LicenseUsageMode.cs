//
// System.ComponentModel.LicenseUsageMode
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Reflection;

namespace System.ComponentModel
{
	[Serializable]
        public enum LicenseUsageMode
	{
		Designtime,
		Runtime,
	}
}
