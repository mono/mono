// CS0164: This label has not been referenced
// Line: 13
// Compiler options: -warnaserror -warn:2

class X {
	static void Main () {
	}

	static void foo (bool b, out int c) {
		if (b) {
			goto LabelA;
		}
		LabelA: LabelB:
			c = 0;	// Out parameter necessary to force reevaluation of LabelA
			return;
	}
}
