//
// System.Runtime.InteropServices.VARDESC.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct VARDESC
	{
		public ELEMDESC elemdescVar;
		public string lpstrSchema;
		public int memid;
		public VarEnum varkind;
		public short wVarFlags;

		[ComVisible (false)]
		public struct DESCUNION
		{
			public IntPtr lpvarValue;
			public int oInst;
		}
	} 
}
