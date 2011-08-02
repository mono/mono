// CS0073: An add or remove accessor must have a body
// Line: 9

using System;

class ErrorCS0073 {
	delegate void Handler ();
	event Handler OnFoo {
		add;
		remove {
			OnFoo -= value;
		}
	}
	public static void Main () {
	}
}

