// CS0540: `N.Nested.C.I.P': containing type does not implement interface `N.Nested.I'
// Line: 17

using System;

namespace N
{
	class Nested
	{
		public interface I
		{
			bool P { get; }
		}

		public class C
		{
			bool I.P
			{
				get { return true; }
			}
		}
	}
}
