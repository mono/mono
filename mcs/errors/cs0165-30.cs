// CS0165: Use of unassigned local variable `a'
// Line: 9

using System;

class Test {
	
	static void Main () {
		Action a = () => a();
	}
}