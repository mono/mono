// cs0196.cs: pointers must be indexed by a single value
// line: 10
// Compiler options: -unsafe
using System;

unsafe class ZZ {
	static void Main () {
		int *p = null;

		if (p [10,4] == 4)
			return;
	}
}
