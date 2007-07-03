//
// System.Runtime.InteropServices.ComTypes.VARKIND.cs
//
// Author:
//   Kazuki Oikawa (kazuki@panicode.com)
//

#if NET_2_0
namespace System.Runtime.InteropServices.ComTypes
{
	[Serializable]
	public enum VARKIND
	{
		VAR_PERINSTANCE = 0,
		VAR_STATIC = 1,
		VAR_CONST = 2,
		VAR_DISPATCH = 3
	}
}
#endif
