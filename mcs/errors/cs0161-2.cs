// CS0161: `Test.Main()': not all code paths return a value
// Line: 4
class Test {
	static int Main () {
		bool b = false;
		while (true) {
			if (b)
				break;
			else
				break;
		}
	}
}

