using System;

public class Tests {

	public delegate void CallTargetWithContextN (object o, params object[] args);

	public static void CallWithContextN (object o, object[] args) {
	}

	public static void Main () {
		object o = new CallTargetWithContextN (CallWithContextN);
	}
}
