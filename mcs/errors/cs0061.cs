// cs0061.cs: Inconsistent accessibility. Base interface less accessible than interface.
// Line: 9

using System;

protected interface IFoo {
}

public interface IBar : IFoo {
}

class ErrorCS0061 {
	public static void Main () {
	}
}

