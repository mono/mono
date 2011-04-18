// CS0196: A pointer must be indexed by only one value
// Line: 10
// Compiler options: -unsafe
using System;

unsafe class ZZ {
	static void Main () {
		int *p = null;

		if (p [10,4] == 4)
			return;
	}
}
