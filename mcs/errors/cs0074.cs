// CS0074: `ErrorCS0074.OnFoo': abstract event cannot have an initializer
// Line: 8

using System;

abstract class ErrorCS0074 {
	public delegate void Handler ();
	public abstract event Handler OnFoo = null;
	public static void Main () {
	}
}

