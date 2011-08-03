// CS0051: Inconsistent accessibility: parameter type `MyClass.X' is less accessible than method `MyClass.method(MyClass.X)'
// Line: 12

public class MyClass {

	//
	// To fix change the next line to "public enum X {
	enum X {
		a, b
	}

	public void method (X arg)
	{
	}
}
