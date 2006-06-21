using System;
using System.Collections.Generic;

class Tests {

	public static int Main () {
		int[] x = new int[] {100, 200};

		GenericClass<int>.Z (x, 0);

		return 0;
	}

	class GenericClass <T> {
		public static T Z (IList<T> x, int index)
		{
			return x [index];
		}
	}
}
