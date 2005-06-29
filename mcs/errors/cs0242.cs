// cs0242.cs: The array index operation is not valid on void pointers
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

