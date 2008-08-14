// CS0242: The operation in question is undefined on void pointers
// Line: 11
// Compiler options: -unsafe

using System;

unsafe class ZZ {
	static void Main () {
		void *p = null;

		if (p [10] == 4)
			return;
	}
}

