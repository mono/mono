// cs0074.cs: Abstracts events can't have initializers.
// Line: 8

using System;

abstract class ErrorCS0074 {
	delegate void Handler ();
	public abstract event Handler OnFoo = null;
	public static void Main () {
	}
}

