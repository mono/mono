// cs0072.cs: An event can override anything but another event.
// Line: 16

using System;

class ErrorCS0072 {
	delegate void FooHandler ();
	protected void Callback () {
	}
	protected virtual event FooHandler OnFoo;
}

class Child : ErrorCS0072 {
	// We are trying to override a method with an event.
	// To get this right comment the next line and uncomment the others below.
	protected override event FooHandler Callback {
	//protected override event FooHandler OnFoo {
		add {
			Callback += value;
			//OnFoo += value;
		}
		remove {
			Callback -= value;
			//OnFoo -= value;
		}
	}

	public static void Main () {
	}
}

