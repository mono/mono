// CS0019: Operator `<<' cannot be applied to operands of type `byte' and `uint'
// Line: 9

using System;
public class PerformanceTest2 {
	public static void Main () {
		uint j, k;
		j = 0;
		k = ((byte) 1 << (7 - j % 8));
	}
}
