//
// System.Management.ComparisonSettings
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Management
{
	[Flags]
	public enum ComparisonSettings
	{
		IncludeAll = 0,
		IgnoreQualifiers = 1,
		IgnoreObjectSource = 2,
		IgnoreDefaultValues = 4,
		IgnoreClass = 8,
		IgnoreCase = 16,
		IgnoreFlavor = 32
	}
}

