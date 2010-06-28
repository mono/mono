// CS0068: `IFoo.OnFoo': event in interface cannot have an initializer
// Line: 14

using System;

class ErrorCS0068 {
	public delegate void FooHandler ();
	public void method () {}
	public static void Main () {
	}
}

interface IFoo {
	event ErrorCS0068.FooHandler OnFoo = new ErrorCS0068.FooHandler (ErrorCS0068.method);
}
