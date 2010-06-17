using System.Linq;
using System.Runtime.CompilerServices;

public class G<T>
{
}

namespace dynamic
{
	public class C : G<dynamic>
	{
		public static int Main ()
		{
			var da = typeof (C).GetCustomAttributes (typeof (System.Runtime.CompilerServices.DynamicAttribute), false) [0] as DynamicAttribute;
			
			if (!da.TransformFlags.SequenceEqual (new bool[] { false, true }))
				return 1;
			
			return 0;
		}
	}
}

