

// Tests `var' keyword contextual parsing

public class Test : var
{
	static void var ()
	{
	}
	
	public static int Main ()
	{
		var i = null;
		var v = new var ();
		
		if (v.GetType () != typeof (var))
			return 1;
		
		return 0;	
	}
}

public class var
{
}

