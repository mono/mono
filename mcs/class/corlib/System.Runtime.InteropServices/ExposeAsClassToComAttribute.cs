//
// System.Runtime.InteropServices.ExposeAsClassToComAttribute
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;

namespace System.Runtime.InteropServices
{
	[AttributeUsage (AttributeTargets.Struct, Inherited = false)]
	public class ExposeAsClassToComAttribute : Attribute
	{
		public ExposeAsClassToComAttribute ()
		{
		}
	}
}
#endif