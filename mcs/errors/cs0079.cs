// cs0079.cs: Events can only appear on the left hand side of += or -=
// Line: 19
 
using System;

class ErrorCS0079 {
	delegate void Handler ();
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
		ErrorCS0079 error = new ErrorCS0079 ();
		error.OnFoo += new Handler (error.Callback);
		error.privateEvent ();
	}
}

		
