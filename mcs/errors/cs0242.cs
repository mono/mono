// cs0242: operation is not defined for void *
// Line: 10
// Compiler options: -unsafe
using System;

unsafe class ZZ {
	static void Main () {
		void *p = null;

		if (p [10] == 4)
			return;
	}
}
