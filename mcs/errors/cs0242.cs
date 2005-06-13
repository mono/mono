// cs0242.cs: The operation in question is undefined on void pointers
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
