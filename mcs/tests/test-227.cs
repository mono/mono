using System;
using Mat = A.ABC.BMT;

namespace A
{
	namespace ABC
	{
		public enum BMT
		{
			X,
		}
	}

	class T
	{
		public enum Mat
		{
			A = 5,
			B
		}

		public static void Main() {
			Mat c;
			c = Mat.A;
		}
	}
}
