//
// System.Runtime.InteropServices.ELEMDESC.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct ELEMDESC
	{
		public DESCUNION desc;
		public TYPEDESC tdesc;

		[ComVisible (false)]
		public struct DESCUNION
		{
			public IDLDESC idldesc;
			public PARAMDESC paramdesc;
		}
	}
}
