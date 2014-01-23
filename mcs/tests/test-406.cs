// Compiler options: -unsafe

//
// This tests excercises the compound assignment when the left side
// is an dereference operator.
//
using System;
namespace TestCase {
	public unsafe class Test {
		public static int Main(string[] args) {
			uint[] uArr = {0, 200};
			uint[] uArr2 = {0, 200};

			fixed (uint* u = uArr, u2 = uArr2) {
				if (DoOp (u) != 100)
					return 1;

				if (uArr [0] != 100)
					return 2;

				if (uArr [1] != 200)
					return 3;

				if (DoOp2 (u2) != 100)
					return 4;

				if (uArr2 [0] != 100)
					return 5;

				if (uArr2 [1] != 200)
					return 6;
			}

			return 0;
		}

		private static uint DoOp (uint *u) {
			return *(u) += 100;
		}

		private static uint DoOp2 (uint *u) {
			*(u) += 100;
			return *u;
		}

	}
}

