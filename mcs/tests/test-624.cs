using System;

//
// The problem here is that `(Type)' is being recognized as a Property
// but inside a Cast expression this is invalid.
//
class X {

	int Type {
		get {
			return 1;
		}
	}

	public static void Main ()
	{
		Type t = (Type) null;
	}

}
