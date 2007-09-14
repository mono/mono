using System;

namespace Bug
{
	public static partial class GL
	{
		static partial class Core
		{
			internal static bool A () { return true; }
		}
		
		/*internal static partial class Bar
		{
			internal static bool A () { return true; }
		}*/
	}

	partial class GL
	{
		public static void Main ()
		{
			Core.A ();
			//Bar.A ();
		}

		internal partial class Core
		{
		}
		
		/*partial class Bar
		{
		}*/
	}
}