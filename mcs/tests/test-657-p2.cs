#define DEBUG

using System;

namespace TestDebug
{
	class C
	{
		public static void Method ()
		{
#if !DEBUG
			throw new ApplicationException ("3");
#endif
		}
	}
}
