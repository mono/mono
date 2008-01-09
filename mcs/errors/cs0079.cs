// CS0079: The event `ErrorCS0079.OnFoo' can only appear on the left hand side of `+=' or `-=' operator
// Line: 19
 
using System;

class ErrorCS0079 {
	public delegate void Handler ();
	event Handler privateEvent;
	public event Handler OnFoo {
		add {
			privateEvent += value;
		}
		remove {
			privateEvent -= value;
		}
	}
	void Callback() {
		if (privateEvent != null) 
			OnFoo();
	}
	
	public static void Main () {
	}
}

		
