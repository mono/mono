using System;

[X (null)]
class X : Attribute {
	int ID;
	public X () {}
	public X (object o)
	{ 
		if (o == null)
			ID = 55;
	}
	
	public static int Main () {
		object[] attrs = typeof(X).GetCustomAttributes(typeof (X),false);
		if (attrs.Length != 1)
			return 2;
	    
		X x = attrs [0] as X;
		if (x.ID != 55)
			return 2;
	    
		Console.WriteLine("OK");
		return 0;
	}
}
