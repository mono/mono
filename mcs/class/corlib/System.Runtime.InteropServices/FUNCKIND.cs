//
// System.Runtime.InteropServices.FUNCKIND.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public enum FUNCKIND
	{
		FUNC_VIRTUAL = 0,
		FUNC_PUREVIRTUAL = 1,
		FUNC_NONVIRTUAL = 2,
		FUNC_STATIC = 3,
		FUNC_DISPATCH = 4
	}
}
