// cs0072.cs: Event `Child.OnFoo' can override only event
// Line: 16

using System;

class ErrorCS0072 {
	public delegate void FooHandler ();
	protected void OnFoo () {}
}

class Child : ErrorCS0072 {
	// We are trying to override a method with an event.
	protected override event FooHandler OnFoo {
		add {
			OnFoo += value;
		}
		remove {
			OnFoo -= value;
		}
	}

	public static void Main () {
	}
}

