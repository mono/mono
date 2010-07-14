// Compiler options: -t:library

using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("gtest-friend-14")]

namespace N
{
	internal class C
	{
		internal struct S
		{
			public string sa;
			public string sb;
		}
		
		internal static void Init (IList<C.S> arg)
		{
		}
	}
}
