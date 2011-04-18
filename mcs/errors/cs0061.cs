// CS0061: Inconsistent accessibility: base interface `IFoo' is less accessible than interface `IBar'
// Line: 9

using System;

interface IFoo {
}

public interface IBar : IFoo {
}

class ErrorCS0061 {
	public static void Main () {
	}
}

